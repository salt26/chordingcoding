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
        private static int rainPitch = 2;

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
                    (vkCode == 226))
                {
                    // Characters
                    int pitch;

                    switch (Form1.theme)
                    {
                        case Form1.Theme.Autumn:
                            pitch = Form1.chord.NextNote();
                            Form1.PlayANoteSync(pitch, 16, 0);
                            Form1.PlayANoteSync(pitch, 16, 1);
                            /*
                            foreach (int p in Form1.chord.NextChord())
                            {
                                Form1.PlayANote(p - 12, 16, 0, 48);
                            }
                            */
                            Form1.AddParticleToBasicParticleSystem((Chord.Root)(pitch % 12));
                            //Form1.PlayANote(vkCode % 24 + 50);
                            break;
                        case Form1.Theme.Rain:
                            pitch = Form1.chord.NextNote();
                            Form1.PlayANoteSync(pitch - 12, 14, 0, 96);
                            Form1.PlayANoteSync(pitch, 16, 1);
                            Form1.AddParticleToBasicParticleSystem((Chord.Root)(pitch % 12));
                            break;
                        case Form1.Theme.Star:
                            pitch = Form1.chord.NextNote();
                            Form1.PlayANoteSync(pitch - 12, 16, 0, 72);
                            Form1.PlayANoteSync(pitch, 16, 1);
                            Form1.AddParticleToBasicParticleSystem((Chord.Root)(pitch % 12));
                            //Form1.PlayANote(vkCode % 24 + 50);
                            break;
                    }

                }
                else if (
                    (vkCode == 32) ||
                    (vkCode == 9) ||
                    (vkCode == 13))
                {
                    // Whitespaces
                    int pitch;

                    switch (Form1.theme)
                    {
                        case Form1.Theme.Autumn:
                            #region Autumn Input
                            Form1.StopPlaying(0);
                            Form1.chord = new Chord(Form1.theme, Form1.chord);
                            pitch = Form1.chord.NextNote();
                            Form1.PlayANoteSync(pitch, 16, 1);
                            Form1.PlayANoteSync(pitch % 12 + 54, 4, 2, 54);
                            foreach (int p in Form1.chord.NextChord())
                            {
                                Form1.PlayANoteSync(p, 16, 0, 64);
                            }
                            if (Form1.form1 != null)
                            {
                                Form1.AddParticleToBasicParticleSystem((Chord.Root)(pitch % 12));
                            }
                            break;
                            #endregion
                        case Form1.Theme.Rain:
                            #region Rain Input
                            Form1.StopPlaying(0);
                            Form1.chord = new Chord(Form1.theme, Form1.chord);
                            pitch = Form1.chord.NextNote();
                            Form1.PlayANoteSync(pitch, 16, 1);
                            rainPitch += 5;
                            rainPitch %= 12;
                            Form1.PlayANoteSync(rainPitch + 46, 64, 2, 24);
                            foreach (int p in Form1.chord.NextChord())
                            {
                                Form1.PlayANoteSync((p + 6) % 12 + 54, 14, 0, 96);
                            }
                            if (Form1.form1 != null)
                            {
                                /* Rain */
                                Form1.AddParticleSystem(
                                    /*posX*/ 0,
                                    /*posY*/ 0,
                                    /*velX*/ 0, /*velY*/ 0, /*life*/ 160,
                                    /*cNum*/ 1, /*cRange*/ 0,
                                    ParticleSystem.CreateFunction.TopRandom,
                                    Particle.Type.rain, Color.White,
                                    /*pSize*/ 0.1f, /*pLife*/ (Form1.form1.Size.Height + 150) / 30);
                            }
                            break;
                            #endregion
                        case Form1.Theme.Star:
                            #region Star Input
                            Form1.StopPlaying(0);
                            Form1.chord = new Chord(Form1.theme, Form1.chord);
                            pitch = Form1.chord.NextNote();
                            Form1.PlayANoteSync(pitch, 16, 1);
                            //Form1.PlayANote(pitch % 12 + 54, 4, 2, 48);
                            foreach (int p in Form1.chord.NextChord())
                            {
                                Form1.PlayANoteSync((p + 6) % 12 + 54, 16, 0, 72);
                            }
                            if (Form1.form1 != null)
                            {
                                /* Starfall */
                                Form1.AddParticleSystem(
                                    /*posX*/ (float)(r.NextDouble() * Form1.form1.Size.Width),
                                    /*posY*/ (float)(r.NextDouble() * Form1.form1.Size.Height * 5 / 6 - Form1.form1.Size.Height / 12),
                                    /*velX*/ 2, /*velY*/ 8, /*life*/ 38,
                                    /*cNum*/ 7, /*cRange*/ 4,
                                    ParticleSystem.CreateFunction.Gaussian,
                                    Particle.Type.dot, Form1.chord.ChordColor(),
                                    /*pSize*/ 1, /*pLife*/ 10);
                            }
                            break;
                            #endregion
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