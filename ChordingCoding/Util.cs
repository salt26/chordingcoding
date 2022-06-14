/*
MIT License

Copyright (c) 2019 Dantae An

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace ChordingCoding.Utility
{
    class Util
    {
        private static Dictionary<string, bool> cachedModuleName = new Dictionary<string, bool>();

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
        /// value를 반올림한 값을 min(포함)과 max(포함) 사이의 값으로 변환하여 반환합니다.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int RoundAndClamp(float value, int min, int max)
        {
            int r = (int)Math.Round(value);
            if (r < min) return min;
            else if (r > max) return max;
            else return r;
        }

        /// <summary>
        /// QWERTY 자판에서 Shift를 눌렀는지 여부에 따라 str을 변환합니다.
        /// </summary>
        /// <param name="str">Shift를 누르지 않았을 때 기준으로 입력한 키들의 문자열</param>
        /// <param name="hasShiftPressed">Shift가 눌린 상태이면 true, 아니면 false</param>
        /// <returns></returns>
        public static string StringWithShift(string str, bool hasShiftPressed)
        {
            if (!hasShiftPressed) return str;

            string newStr = "";
            for (int i = 0; i < str.Length; i++)
            {
                switch (str[i])
                {
                    case '`': newStr += '~'; break;
                    case '1': newStr += '!'; break;
                    case '2': newStr += '@'; break;
                    case '3': newStr += '#'; break;
                    case '4': newStr += '$'; break;
                    case '5': newStr += '%'; break;
                    case '6': newStr += '^'; break;
                    case '7': newStr += '&'; break;
                    case '8': newStr += '*'; break;
                    case '9': newStr += '('; break;
                    case '0': newStr += ')'; break;
                    case '-': newStr += '_'; break;
                    case '=': newStr += '+'; break;
                    case 'q': newStr += 'Q'; break;
                    case 'w': newStr += 'W'; break;
                    case 'e': newStr += 'E'; break;
                    case 'r': newStr += 'R'; break;
                    case 't': newStr += 'T'; break;
                    case 'y': newStr += 'Y'; break;
                    case 'u': newStr += 'U'; break;
                    case 'i': newStr += 'I'; break;
                    case 'o': newStr += 'O'; break;
                    case 'p': newStr += 'P'; break;
                    case '[': newStr += '{'; break;
                    case ']': newStr += '}'; break;
                    case '\\': newStr += '|'; break;
                    case 'a': newStr += 'A'; break;
                    case 's': newStr += 'S'; break;
                    case 'd': newStr += 'D'; break;
                    case 'f': newStr += 'F'; break;
                    case 'g': newStr += 'G'; break;
                    case 'h': newStr += 'H'; break;
                    case 'j': newStr += 'J'; break;
                    case 'k': newStr += 'K'; break;
                    case 'l': newStr += 'L'; break;
                    case ';': newStr += ':'; break;
                    case '\'': newStr += '"'; break;
                    case 'z': newStr += 'Z'; break;
                    case 'x': newStr += 'X'; break;
                    case 'c': newStr += 'C'; break;
                    case 'v': newStr += 'V'; break;
                    case 'b': newStr += 'B'; break;
                    case 'n': newStr += 'N'; break;
                    case 'm': newStr += 'M'; break;
                    case ',': newStr += '<'; break;
                    case '.': newStr += '>'; break;
                    case '/': newStr += '?'; break;
                    default: newStr += str[i]; break;
                }
            }
            return newStr;
        }
        
        /// <summary>
        /// 주어진 moduleName이 이 프로젝트에 포함되어 있는지 런타임에 확인합니다.
        /// 같은 moduleName으로 여러 번 호출할 경우 항상 처음과 같은 결과를, 빠르게 반환합니다.
        /// </summary>
        /// <param name="moduleName">모듈 이름 (예: "ChordingCoding.UI.MainForm")</param>
        /// <returns></returns>
        public static bool HasModuleExists(string moduleName)
        {
            if (moduleName is null)
            {
                return false;
            }
            else if (cachedModuleName.ContainsKey(moduleName))
            {
                return cachedModuleName[moduleName];
            }
            else
            {
                if (Type.GetType(moduleName) is null)
                {
                    cachedModuleName.Add(moduleName, false);
                    return false;
                }
                else
                {
                    cachedModuleName.Add(moduleName, true);
                    return true;
                }
            }
        }

        /// <summary>
        /// 주어진 namespaceName과 className이 이 프로젝트에 포함되어 있는지 런타임에 확인합니다.
        /// 같은 인자 쌍으로 여러 번 호출할 경우 항상 처음과 같은 결과를, 빠르게 반환합니다.
        /// </summary>
        /// <param name="namespaceName">네임스페이스 이름</param>
        /// <param name="className">클래스 이름</param>
        /// <returns></returns>
        public static bool HasModuleExists(string namespaceName, string className)
        {
            return HasModuleExists(namespaceName + "." + className);
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

                try
                {
                    var task2 = System.Threading.Tasks.Task.Run(() => DoTask(lockName));
                    System.Threading.Tasks.Task.WaitAll(task2);
                }
                catch (NullReferenceException)
                {
                    DoTask(lockName);
                }
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
                        Console.WriteLine("Error: In DoTask(), dequeued task is null!");
                    }
                    else
                    {
                        try
                        {
                            p.Key(p.Value);     // Task 수행
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.StackTrace);
                        }
                    }
                }

                try
                {
                    System.Threading.Tasks.Task.Run(() => DoTask(lockName));   // 연쇄적으로 다음 Task 수행
                }
                catch (NullReferenceException)
                {
                    DoTask(lockName);
                }
            }
        }

        /// <summary>
        /// .csv 형식의 파일을 읽고 그 안의 데이터를 보관하는 클래스입니다.
        /// </summary>
        public class CSVReader
        {
            StreamReader streamReader;
            List<List<string>> data = new List<List<string>>();
            List<string> header = new List<string>();
            bool hasHeader = false;
            Dictionary<int, List<string>> columns = new Dictionary<int, List<string>>();

            /// <summary>
            /// filename의 .csv 파일을 읽고 그 데이터를 정리하여 보관합니다.
            /// </summary>
            /// <param name="filename">파일 이름(경로)</param>
            /// <param name="hasHeader">첫 줄에 헤더가 오면 true, 헤더 없이 데이터가 바로 오면 false</param>
            /// <param name="delimiter">열 구분 문자</param>
            public CSVReader(string filename, bool hasHeader, char delimiter = ',')
            {
                streamReader = new StreamReader(filename, Encoding.GetEncoding("UTF-8"));
                int i = 0;
                if (hasHeader)
                {
                    i = -1;
                    this.hasHeader = true;
                }
                while (!streamReader.EndOfStream)
                {
                    string s = streamReader.ReadLine();
                    if (i >= 0)
                    {
                        data.Add(new List<string>());
                        string[] temp = s.Split(delimiter);
                        for (int j = 0; j < temp.Length; j++)
                        {
                            data[i].Add(temp[j]);
                        }
                    }
                    else
                    {
                        string[] temp = s.Split(delimiter);
                        for (int j = 0; j < temp.Length; j++)
                        {
                            header.Add(temp[j]);
                        }
                    }
                    i++;
                }
            }

            /// <summary>
            /// 주어진 headerName이 몇 번째 열(0부터 시작)의 헤더 이름인지 반환합니다.
            /// 없으면 -1을 반환합니다.
            /// </summary>
            /// <param name="headerName">헤더 이름</param>
            /// <returns></returns>
            public int GetHeaderIndex(string headerName)
            {
                return header.IndexOf(headerName);
            }

            /// <summary>
            /// index(0부터 시작)번째 행의 데이터를 반환합니다.
            /// 반환된 데이터를 함부로 수정하지 마세요.
            /// </summary>
            /// <param name="index">행 인덱스 (0 이상)</param>
            /// <returns></returns>
            public List<string> GetRow(int index)
            {
                if (index < 0 || index >= data.Count) return null;
                return data[index];
            }

            /// <summary>
            /// headerIndex(0부터 시작)번째 열의 데이터를 반환합니다.
            /// 특정 headerIndex에 대해 처음 호출하면 시간이 조금 걸릴 수 있습니다.
            /// 데이터의 어떤 행에 headerIndex번째 값이 없으면
            /// 그 행의 값은 null로 채워집니다.
            /// </summary>
            /// <param name="headerIndex">열 인덱스 (0 이상)</param>
            /// <returns></returns>
            public List<string> GetColumn(int headerIndex)
            {
                if (headerIndex < 0)
                {
                    return null;
                }
                else if (columns.ContainsKey(headerIndex))
                {
                    return columns[headerIndex];
                }
                else
                {
                    List<string> column = new List<string>();
                    for (int i = 0; i < data.Count; i++)
                    {
                        if (headerIndex < data[i].Count)
                        {
                            column.Add(data[i][headerIndex]);
                        }
                        else
                        {
                            column.Add(null);
                        }
                    }
                    columns.Add(headerIndex, column);
                    return column;
                }
            }

            /// <summary>
            /// headerName을 헤더 이름으로 하는 열의 데이터를 반환합니다.
            /// 특정 headerName에 대해 처음 호출하면 시간이 조금 걸릴 수 있습니다.
            /// </summary>
            /// <param name="headerName">헤더 이름</param>
            /// <returns></returns>
            public List<string> GetColumn(string headerName)
            {
                return GetColumn(GetHeaderIndex(headerName));
            }

            /// <summary>
            /// 헤더가 있는 경우 헤더 목록을 반환합니다.
            /// 없으면 null을 반환합니다.
            /// </summary>
            /// <returns></returns>
            public List<string> GetHeader()
            {
                if (hasHeader)
                    return header;
                else return null;
            }

            /// <summary>
            /// 데이터 전체를 반환합니다.
            /// 반환된 이중 배열의 첫 번째 인덱스는 행 인덱스,
            /// 두 번째 인덱스는 열 인덱스입니다.
            /// 반환된 데이터를 함부로 수정하지 마세요.
            /// </summary>
            /// <returns></returns>
            public List<List<string>> GetData()
            {
                return data;
            }
        }

    }
}
