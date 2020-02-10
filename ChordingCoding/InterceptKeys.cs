using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Text;
using System.Drawing;
using System.Collections.Generic;
using ChordingCoding.SFX;

namespace ChordingCoding.UI
{
    class InterceptKeys
    {
        // https://stackoverflow.com/questions/604410/global-keyboard-capture-in-c-sharp-application

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        public static LowLevelKeyboardProc _proc = HookCallback;
        public static IntPtr _hookID = IntPtr.Zero;

        public static string wordState = "";

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

                // https://sites.google.com/site/douglaslash/Home/programming/c-notes--snippets/c-keycodes
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
                    void AddCharToWord(object[] args)
                    {
                        int vkCode_ = (int)args[0];
                        StringBuilder charPressed = new StringBuilder(256);
                        ToUnicode((uint)vkCode_, 0, new byte[256], charPressed, charPressed.Capacity, 0);
                        wordState += charPressed.ToString();
                    }
                    Util.TaskQueue.Add("wordState", AddCharToWord, vkCode);

                    Music.PlayNoteInChord();
                    // 음표 재생 후에 Music.OnPlayNotes()가 호출되면서 시각 효과 발생
                }
                else if (
                    (vkCode == 32) ||
                    (vkCode == 9) ||
                    (vkCode == 13))
                {
                    // Whitespaces
                    void ResetWord(object[] args)
                    {
                        Console.WriteLine("wordState: " + wordState);
                        wordState = "";
                    }
                    Util.TaskQueue.Add("wordState", ResetWord);

                    Music.PlayChordTransitionSync();
                    // 화음 전이 후에 Music.OnChordTransition()이 호출되면서 시각 효과 발생
                }
                else if (vkCode == 8)
                {
                    // Backspace
                    void BackspaceWord(object[] args)
                    {
                        if (wordState.Length > 0)
                            wordState = wordState.Substring(0, wordState.Length - 1);
                    }
                    Util.TaskQueue.Add("wordState", BackspaceWord);
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

        // https://stackoverflow.com/questions/23170259/convert-keycode-to-char-string
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int ToUnicode(
            uint virtualKeyCode,
            uint scanCode,
            byte[] keyboardState,
            StringBuilder receivingBuffer,
            int bufferSize,
            uint flags
        );

    }
}