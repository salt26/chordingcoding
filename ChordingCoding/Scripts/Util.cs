using System;
using System.Collections.Generic;
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

        #region Lock: 한 번에 하나씩만 수행되어야 하는 작업을 관리하는 클래스
        /// <summary>
        /// 한 번에 하나씩만 수행되어야 하는 작업들을 관리합니다.
        /// </summary>
        class Lock
        {
            public delegate void Task(params object[] arguments);

            private static Dictionary<string, Queue<KeyValuePair<Task, object[]>>> lockedTaskQueue =
                new Dictionary<string, Queue<KeyValuePair<Task, object[]>>>();                          // 작업 대기열
            private static Dictionary<string, bool> isLocked = new Dictionary<string, bool>();          // 작업이 진행 중이면 true, 놀고 있으면 false

            // 작업을 넣기만 하는 함수와 작업을 빼서 처리하기만 하는 함수가 분리되어야 한다.
            // 같은 작업임을 구분하는 변수가 꼭 bool일 필요는 없다. string이어도 된다. 작업이 처리 중인지는 이 안에서만 관리한다.

            /// <summary>
            /// 새 작업을 추가합니다.
            /// 이 작업은 같은 lockName을 공유하는 이전의 작업들이 모두 끝난 후에 처리됩니다.
            /// </summary>
            /// <param name="lockName">동시에 실행되면 안 되는 작업들의 그룹 이름</param>
            /// <param name="task">추가할 작업 delegate</param>
            /// <param name="args">추가할 작업에 필요한 인자들</param>
            public static void AddTask(string lockName, Task task, object[] args)
            {
                if (!isLocked.ContainsKey(lockName))
                {
                    isLocked.Add(lockName, false);
                }
                if (!lockedTaskQueue.ContainsKey(lockName))
                {
                    lockedTaskQueue.Add(lockName, new Queue<KeyValuePair<Task, object[]>>());
                }

                lockedTaskQueue[lockName].Enqueue(new KeyValuePair<Task, object[]>(task, args));

                if (!isLocked[lockName])
                {
                    DoTask(lockName);
                }
            }

            private static void DoTask(string lockName)
            {
                if (!isLocked.ContainsKey(lockName)) return;
                if (!lockedTaskQueue.ContainsKey(lockName)) return;

                // TODO
            }

            public static bool GetTaskLocked(string lockName)
            {
                if (!isLocked.ContainsKey(lockName)) return false;
                return isLocked[lockName];
            }
        }
        #endregion
    }
}
