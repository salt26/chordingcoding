using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using Microsoft.Win32;

namespace ChordingCoding.SFX
{
    /// <summary>
    /// Windows의 전원 상태를 감지하는 클래스입니다.
    /// </summary>
    class PowerManagement
    {
        // https://stackoverflow.com/questions/4693689/how-to-programmatically-detect-when-the-os-windows-is-waking-up-or-going-to-sl

        public delegate void PowerEvent();

        /// <summary>
        /// Windows가 절전 모드가 될 때 실행되는 이벤트입니다.
        /// </summary>
        public event PowerEvent OnPowerSuspend;

        /// <summary>
        /// Windows가 절전 모드에서 깨어날 때 실행되는 이벤트입니다.
        /// </summary>
        public event PowerEvent OnPowerResume;

        private ManagementEventWatcher managementEventWatcher;
        private readonly Dictionary<string, string> powerValues = new Dictionary<string, string>
                         {
                             {"4", "Entering Suspend"},
                             {"7", "Resume from Suspend"},
                             {"10", "Power Status Change"},
                             {"11", "OEM Event"},
                             {"18", "Resume Automatic"}
                         };

        /// <summary>
        /// 전원 상태 감지 시스템을 초기화하고 시작합니다.
        /// 처음 한 번만 호출하거나, Stop()을 먼저 호출한 후에 호출해야 합니다.
        /// </summary>
        public void InitPowerEvents()
        {
            var q = new WqlEventQuery();

            q.EventClassName = "Win32_PowerManagementEvent";
            managementEventWatcher = new ManagementEventWatcher(q);
            managementEventWatcher.EventArrived += PowerEventArrive;

            if (OnPowerSuspend != null)
            {
                foreach (Delegate d in OnPowerSuspend.GetInvocationList())
                {
                    OnPowerSuspend -= (PowerEvent)d;
                }
            }
            if (OnPowerResume != null)
            {
                foreach (Delegate d in OnPowerResume.GetInvocationList())
                {
                    OnPowerResume -= (PowerEvent)d;
                }
            }

            managementEventWatcher.Start();

            //SystemEvents.PowerModeChanged += OnPowerChange;
        }

        private void PowerEventArrive(object sender, EventArrivedEventArgs e)
        {
            foreach (PropertyData pd in e.NewEvent.Properties)
            {
                if (pd == null || pd.Value == null) continue;
                var name = powerValues.ContainsKey(pd.Value.ToString())
                               ? powerValues[pd.Value.ToString()]
                               : pd.Value.ToString();
                Console.WriteLine("PowerEvent:" + name);
                if (pd.Value.ToString() == "4" && OnPowerSuspend != null &&
                    OnPowerSuspend.GetInvocationList().Length > 0)
                {
                    OnPowerSuspend();
                }
                else if (pd.Value.ToString() == "7" && OnPowerResume != null && 
                    OnPowerResume.GetInvocationList().Length > 0)
                {
                    OnPowerResume();
                }
            }
        }

        /// <summary>
        /// 전원 상태 감지 시스템을 멈춥니다.
        /// 이것을 호출하지 않으면 메모리 누수가 발생할 수 있습니다.
        /// </summary>
        public void Stop()
        {
            managementEventWatcher.Stop();
            Console.WriteLine("Power Management stopped");

            //SystemEvents.PowerModeChanged -= OnPowerChange;
        }

        /*
        private void OnPowerChange(object s, PowerModeChangedEventArgs e)
        {
            switch (e.Mode)
            {
                case PowerModes.Resume:
                    Console.WriteLine("OnPowerChange:Resume");
                    break;
                case PowerModes.Suspend:
                    Console.WriteLine("OnPowerChange:Suspend");
                    break;
            }
        }
        */
    }
}
