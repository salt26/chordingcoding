using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChordingCoding
{
    class Util
    {
        /// <summary>
        /// 평균이 0, 표준편차가 1인 표준정규분포를 따르는 랜덤한 값을 생성합니다.
        /// </summary>
        /// <returns></returns>
        public static float GaussianRandom(Random r)
        {
            double u1 = 1.0 - r.NextDouble(); // uniform (0,1] random doubles
            double u2 = 1.0 - r.NextDouble();
            float randStdNormal = (float)(Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2)); // random normal(0,1)
            return randStdNormal;
        }
        
        /// <summary>
        /// 한 번에 하나씩만 수행되어야 하는 작업들을 관리합니다.
        /// </summary>
        public class TaskQueue
        {
            /// <summary>
            /// TaskQueue에 넣을 수 있는, 임의의 인자들을 받고 값을 반환하지 않는 작업 delegate입니다.
            /// </summary>
            /// <param name="arguments"></param>
            public delegate void Task(params object[] arguments);

            /// <summary>
            /// 같은 string(Key)으로 묶이는 작업 그룹들을 순차적으로 실행하는 Queue(Value.Key)와
            /// 이것에 대한 lock object(Value.Value)를 관리하는 Dictionary입니다.
            /// Queue 안에는 수행할 작업인 Util.Lock.Task(Key)와 인자 object[](Value)가 들어갑니다.
            /// </summary>
            private static Dictionary<string, KeyValuePair<Queue<KeyValuePair<Task, object[]>>, object>> lockedTaskQueues =
                new Dictionary<string, KeyValuePair<Queue<KeyValuePair<Task, object[]>>, object>>();                          // 작업 대기열

            // 작업을 넣기만 하는 함수와 작업을 빼서 처리하기만 하는 함수가 분리되어야 한다.
            // (Producer-consumer problem)
            
            /// <summary>
            /// 새 작업을 추가합니다.
            /// 이 작업은 같은 lockName을 공유하는 이전의 작업들이 모두 끝난 후에 처리됩니다.
            /// </summary>
            /// <param name="lockName">동시에 실행되면 안 되는 작업들의 그룹 이름</param>
            /// <param name="task">추가할 작업 delegate</param>
            /// <param name="args">추가할 작업에 필요한 인자들</param>
            public static void Add(string lockName, 
                Task task, params object[] args)
            {
                if (!lockedTaskQueues.ContainsKey(lockName))
                {
                    lockedTaskQueues.Add(lockName, new KeyValuePair<Queue<KeyValuePair<Task, object[]>>, object>
                        (new Queue<KeyValuePair<Task, object[]>>(), new object()));
                }

                lock (lockedTaskQueues[lockName].Value)
                {
                    lockedTaskQueues[lockName].Key.Enqueue(new KeyValuePair<Task, object[]>(task, args));
                }
                var task2 = System.Threading.Tasks.Task.Run(() => DoTask(lockName));
                System.Threading.Tasks.Task.WaitAll(task2);
            }

            private static void DoTask(string lockName)
            {
                if (!lockedTaskQueues.ContainsKey(lockName))
                {
                    return;
                }
                lock (lockedTaskQueues[lockName].Value)
                {
                    if (lockedTaskQueues[lockName].Key.Count <= 0)
                    {
                        return;
                    }
                    KeyValuePair<Task, object[]> p = lockedTaskQueues[lockName].Key.Dequeue();
                    if (p.Key is null)
                    {
                        throw new NullReferenceException();
                    }
                    p.Key(p.Value);     // Task 수행
                }

                System.Threading.Tasks.Task.Run(() => DoTask(lockName));   // 연쇄적으로 다음 Task 수행
            }
        }
        
    }
}
