﻿using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing;

namespace ChordingCoding
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

                // TODO Do something when KeyDown event occurs.
                //Console.WriteLine((Keys)vkCode);
                Keys key = (Keys)vkCode;
                if ((vkCode >= 48 && vkCode <= 90) ||
                    (vkCode >= 96 && vkCode <= 111) ||
                    (vkCode >= 186 && vkCode <= 192) ||
                    (vkCode >= 219 && vkCode <= 223) ||
                    (vkCode == 226)
                    )
                {
                    // Characters
                    int pitch;

                    switch (Form1.theme)
                    {
                        case Form1.Theme.Forest:
                            pitch = Form1.chord.NextNote();
                            Form1.PlayANote(pitch - 12, 14, 0, 96);
                            Form1.PlayANote(pitch, 16, 1);
                            //Form1.PlayANote(vkCode % 24 + 50);
                            break;
                    }

                }
                else if (
                    (vkCode == 32) ||
                    (vkCode == 9) ||
                    (vkCode == 13)
                    )
                {
                    // Whitespaces
                    int pitch;

                    switch (Form1.theme)
                    {
                        case Form1.Theme.Forest:
                            Form1.StopPlaying(0);
                            Form1.chord = new Chord(Form1.theme, Form1.chord);
                            pitch = Form1.chord.NextNote();
                            Form1.PlayANote(pitch, 16, 1);
                            Form1.PlayANote(pitch % 12 + 54, 4, 2, 48);
                            foreach (int p in Form1.chord.NextChord())
                            {
                                Form1.PlayANote((p + 6) % 12 + 54, 14, 0, 96);
                            }
                            if (Form1.form1 != null)
                            {
                                /* Starfall */
                                Form1.AddParticleSystem(
                                    /*posX*/ (float)(r.NextDouble() * Form1.form1.Size.Width),
                                    /*posY*/ (float)(r.NextDouble() * Form1.form1.Size.Height * 2 / 3 - Form1.form1.Size.Height / 6),
                                    /*velX*/ 2, /*velY*/ 8, /*life*/ 64,
                                    /*cNum*/ 10, /*cRange*/ 5,
                                    ParticleSystem.CreateFunction.Gaussian,
                                    Particle.Type.dot, Form1.chord.ChordColor(),
                                    /*pSize*/ 1, /*pLife*/ 20);
                            }
                            break;

                        case Form1.Theme.Rain:
                            Form1.StopPlaying(0);
                            Form1.chord = new Chord(Form1.theme, Form1.chord);
                            pitch = Form1.chord.NextNote();
                            Form1.PlayANote(pitch, 16, 1);
                            Form1.PlayANote(pitch % 12 + 54, 4, 2, 48);
                            foreach (int p in Form1.chord.NextChord())
                            {
                                Form1.PlayANote((p + 6) % 12 + 54, 14, 0, 96);
                            }
                            if (Form1.form1 != null)
                            {
                                /* Rain */
                                Form1.AddParticleSystem(
                                    /*posX*/ (float)(r.NextDouble() * Form1.form1.Size.Width),
                                    /*posY*/ 0,
                                    /*velX*/ 0.2f, /*velY*/ 16, /*life*/ 160,
                                    /*cNum*/ 1, /*cRange*/ 0,
                                    ParticleSystem.CreateFunction.Gaussian,
                                    Particle.Type.rain, Color.CornflowerBlue,
                                    /*pSize*/ 0.5f, /*pLife*/ 1);
                            }
                            break;
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