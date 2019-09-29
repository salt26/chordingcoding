using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Collections.Generic;
using ChordingCoding.SFX;

namespace ChordingCoding.UI
{
    class InterceptKeys
    {
        private static Random r = new Random();

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        public static LowLevelKeyboardProc _proc = HookCallback;
        public static IntPtr _hookID = IntPtr.Zero;

        public static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        /*
         * 키보드 입력 시 호출될 함수
         */ 
        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);

                // Do something when KeyDown event occurs.
                //Console.WriteLine((Keys)vkCode);
                Keys key = (Keys)vkCode;
                if ((vkCode >= 48 && vkCode <= 90) ||
                    (vkCode >= 96 && vkCode <= 111) ||
                    (vkCode >= 186 && vkCode <= 192) ||
                    (vkCode >= 219 && vkCode <= 223) ||
                    (vkCode == 226))
                {
                    // Characters
                    int pitch = Music.PlayNoteInChord();
                    Form1.AddParticleToBasicParticleSystem((Chord.Root)(pitch % 12));

                }
                else if (
                    (vkCode == 32) ||
                    (vkCode == 9) ||
                    (vkCode == 13))
                {
                    // Whitespaces
                    int pitch = Music.PlayChordTransition();

                    if (Theme.CurrentTheme.ParticleSystemForWhitespace != null)
                    {
                        Form1.AddParticleSystem(Theme.CurrentTheme.ParticleSystemForWhitespace);
                    }
                    else
                    {
                        Form1.AddParticleToBasicParticleSystem((Chord.Root)(pitch % 12));
                    }

                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
        
    }
}