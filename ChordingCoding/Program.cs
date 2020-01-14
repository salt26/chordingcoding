using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Timers;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Security.AccessControl;
using System.Security.Principal;
using ChordingCoding.UI;

namespace ChordingCoding
{
    static class Program
    {
        /// <summary>
        /// 해당 응용 프로그램의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // get application GUID as defined in AssemblyInfo.cs
            string appGuid =
                ((GuidAttribute)Assembly.GetExecutingAssembly().
                    GetCustomAttributes(typeof(GuidAttribute), false).GetValue(0)).Value.ToString();

            // unique ID for global mutex - Global prefix means it is global to the machine
            string mutexId = string.Format("Global\\{{{0}}}", appGuid);

            // Need a place to store a return value in Mutex() constructor call
            bool createdNew;

            // to add example of setting up security for multi-user usage
            // to work also on localized systems (don't use just "Everyone") 
            var allowEveryoneRule =
                new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), 
                                    MutexRights.FullControl, AccessControlType.Allow);
            var securitySettings = new MutexSecurity();
            securitySettings.AddAccessRule(allowEveryoneRule);

            // to prevent race condition on security settings
            using (var mutex = new Mutex(false, mutexId, out createdNew, securitySettings))
            {
                var hasHandle = false;
                try
                {
                    try
                    {
                        // 만약 global mutex가 잡혀 있으면 기다리지 않고 즉시 종료합니다.
                        hasHandle = mutex.WaitOne(0, false);
                        if (hasHandle == false) {
                            MessageBox.Show("이미 실행 중인 프로그램이 있습니다.", "ChordingCoding", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            throw new TimeoutException("Timeout waiting for exclusive access");
                        }
                    }
                    catch (AbandonedMutexException)
                    {
                        // Log the fact that the mutex was abandoned in another process,
                        // it will still get acquired
                        hasHandle = true;
                    }

                    
                    // 아래 코드는 프로그램이 단독으로 실행할 때 수행할 명령들입니다.
                    // Form1을 실행합니다.
                    

                    // 키보드 입력 이벤트를 감지하여 콜백을 호출하도록 합니다.
                    InterceptKeys._hookID = InterceptKeys.SetHook(InterceptKeys._proc);

                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);

                    Form1 form = new Form1();
                    Application.Run(form);

                    // SetHook에서 잡은 handle을 놓습니다.
                    InterceptKeys.UnhookWindowsHookEx(InterceptKeys._hookID);
                }
                finally
                {
                    if (hasHandle)
                        mutex.ReleaseMutex();
                }
            }
        }

    }
}
