using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

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

        #region Lock: 한 번에 하나씩만 수행되어야 하는 작업을 관리하는 클래스
        /// <summary>
        /// 한 번에 하나씩만 수행되어야 하는 작업들을 관리합니다.
        /// </summary>
        public class Lock
        {
            public delegate void Task(params object[] arguments);

            
            private static Dictionary<string, Queue<KeyValuePair<Task, object[]>>> lockedTaskQueue =
                new Dictionary<string, Queue<KeyValuePair<Task, object[]>>>();                          // 작업 대기열
            private static List<string> isLocked = new List<string>();          // 진행 중인 작업 그룹의 이름들을 담는 목록
            
            //private static ConcurrentDictionary<string, SerialQueue> taskQueues = new ConcurrentDictionary<string, SerialQueue>();

            // 작업을 넣기만 하는 함수와 작업을 빼서 처리하기만 하는 함수가 분리되어야 한다.
            // 같은 작업임을 구분하는 변수가 꼭 bool일 필요는 없다. string이어도 된다. 작업이 처리 중인지는 이 안에서만 관리한다.

                /*
            /// <summary>
            /// 새 작업을 추가합니다.
            /// 이 작업은 같은 lockName을 공유하는 이전의 작업들이 모두 끝난 후에 처리됩니다.
            /// </summary>
            /// <param name="lockName">동시에 실행되면 안 되는 작업들의 그룹 이름</param>
            /// <param name="task">추가할 작업 delegate</param>
            /// <param name="args">추가할 작업에 필요한 인자들</param>
            public static async void AddTaskAsync(string lockName, 
                Task task, params object[] args)
            {
                SerialQueue serialQueue = taskQueues.GetOrAdd(lockName, new SerialQueue());
                await serialQueue.Enqueue(task, args);
            }
            */
            
            /// <summary>
            /// 새 작업을 추가합니다.
            /// 이 작업은 같은 lockName을 공유하는 이전의 작업들이 모두 끝난 후에 처리됩니다.
            /// </summary>
            /// <param name="lockName">동시에 실행되면 안 되는 작업들의 그룹 이름</param>
            /// <param name="task">추가할 작업 delegate</param>
            /// <param name="args">추가할 작업에 필요한 인자들</param>
            public static void AddTask(string lockName, 
                Task task, params object[] args)
            {
                if (!lockedTaskQueue.ContainsKey(lockName))
                {
                    lockedTaskQueue.Add(lockName, new Queue<KeyValuePair<Task, object[]>>());
                }

                lockedTaskQueue[lockName].Enqueue(new KeyValuePair<Task, object[]>(task, args));

                // lock이 안 잡혀있는 경우 (처리 중인 Task가 없는 경우)
                if (isLocked.IndexOf(lockName) == -1)
                {
                    // lock 잡고 Task 수행
                    isLocked.Add(lockName);
                    DoTask(lockName);
                }
            }

            private static void DoTask(string lockName)
            {
                if (!lockedTaskQueue.ContainsKey(lockName))
                {
                    if (isLocked.IndexOf(lockName) != -1)
                    {
                        // lock이 잡혀있는 경우 lock 놓음
                        isLocked.Remove(lockName);
                    }
                    return;
                }
                if (lockedTaskQueue[lockName].Count <= 0)
                {
                    // 수행할 Task가 없음

                    if (isLocked.IndexOf(lockName) != -1)
                    {
                        // lock이 잡혀있는 경우 lock 놓음
                        isLocked.Remove(lockName);
                    }
                    //lockedTaskQueue.Remove(lockName);
                    return;
                }
                if (isLocked.IndexOf(lockName) == -1)
                {
                    // lock 잡음
                    isLocked.Add(lockName);
                }
                
                KeyValuePair<Task, object[]> p = lockedTaskQueue[lockName].Dequeue();
                p.Key(p.Value);     // Task 수행

                DoTask(lockName);   // 잡은 lock을 놓지 않고 연쇄적으로 다음 Task 수행
            }

            /// <summary>
            /// 인자로 주어진 lockName을 공유하는 작업이 처리 중이면 true, 아니면 false를 반환합니다.
            /// </summary>
            /// <param name="lockName">동시에 실행되면 안 되는 작업들의 그룹 이름</param>
            /// <returns></returns>
            public static bool GetTaskLocked(string lockName)
            {
                if (isLocked.IndexOf(lockName) == -1) return false;
                return true;
            }
        }
        #endregion

        /*
        // https://github.com/Gentlee/SerialQueue
        public class SerialQueue
        {
            readonly object _locker = new object();
            WeakReference<Task> _lastTask;

            public Task Enqueue(Action action)
            {
                return Enqueue<bool>(() => {
                    action();
                    return true;
                });
            }

            public Task<T> Enqueue<T>(Func<T> function)
            {
                lock (_locker)
                {
                    Task lastTask = null;
                    Task<T> resultTask = null;

                    if (_lastTask != null && _lastTask.TryGetTarget(out lastTask))
                    {
                        resultTask = lastTask.ContinueWith(_ => function(), TaskContinuationOptions.ExecuteSynchronously);
                    }
                    else
                    {
                        resultTask = Task.Run(function);
                    }

                    _lastTask = new WeakReference<Task>(resultTask);
                    return resultTask;
                }
            }

            public Task Enqueue(Func<Task> asyncAction)
            {
                lock (_locker)
                {
                    Task lastTask = null;
                    Task resultTask = null;

                    if (_lastTask != null && _lastTask.TryGetTarget(out lastTask))
                    {
                        resultTask = lastTask.ContinueWith(_ => asyncAction(), TaskContinuationOptions.ExecuteSynchronously).Unwrap();
                    }
                    else
                    {
                        resultTask = Task.Run(asyncAction);
                    }

                    _lastTask = new WeakReference<Task>(resultTask);
                    return resultTask;
                }
            }

            public Task<T> Enqueue<T>(Func<Task<T>> asyncFunction)
            {
                lock (_locker)
                {
                    Task lastTask = null;
                    Task<T> resultTask = null;

                    if (_lastTask != null && _lastTask.TryGetTarget(out lastTask))
                    {
                        resultTask = lastTask.ContinueWith(_ => asyncFunction(), TaskContinuationOptions.ExecuteSynchronously).Unwrap();
                    }
                    else
                    {
                        resultTask = Task.Run(asyncFunction);
                    }

                    _lastTask = new WeakReference<Task>(resultTask);
                    return resultTask;
                }
            }

            public Task Enqueue(Lock.Task task, params object[] args)
            {
                return Enqueue<bool>(() => {
                    task(args);
                    return true;
                });
            }
        }
        */
    }
}
