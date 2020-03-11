/*
MIT License

Copyright (c) 2019 salt26

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
using System.Runtime.InteropServices;

namespace ChordingCoding.SFX
{
    class Timer
    {
        public delegate void TickDelegate();

        private delegate void TimerEventDelegate(int id, int msg, IntPtr user, int dw1, int dw2);
        private const int TIME_PERIODIC = 1;
        private const int EVENT_TYPE = TIME_PERIODIC;
        [DllImport("winmm.dll")]
        private static extern int timeBeginPeriod(int msec);
        [DllImport("winmm.dll")]
        private static extern int timeEndPeriod(int msec);
        [DllImport("winmm.dll")]
        private static extern int timeSetEvent(int delay, int resolution,
            TimerEventDelegate handler, IntPtr user, int eventType);
        [DllImport("winmm.dll")]
        private static extern int timeKillEvent(int id);

        TickDelegate m_Action;
        private int m_TimerId;
        private TimerEventDelegate m_Handler;

        /// <summary>
        /// 새 타이머를 생성하고 시작합니다.
        /// </summary>
        /// <param name="interval">tick이 호출되는 주기 (millisecond)</param>
        /// <param name="timerTickDelegate">호출될 tick 함수 대리자</param>
        public Timer(int interval, TickDelegate timerTickDelegate)
        {
            m_Action = timerTickDelegate;
            timeBeginPeriod(1);
            m_Handler = new TimerEventDelegate((id, msg, user, dw1, dw2) => m_Action());
            m_TimerId = timeSetEvent(interval, 0, m_Handler, IntPtr.Zero, EVENT_TYPE);
        }

        /// <summary>
        /// 타이머를 종료합니다.
        /// </summary>
        public void Stop()
        {
            int error = timeKillEvent(m_TimerId);
            timeEndPeriod(1);
            System.Threading.Thread.Sleep(100); // Ensure callbacks are drained
        }
    }
}
