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
    /// <summary>
    /// 키보드와 마우스 입력을 관리하는 클래스입니다.
    /// </summary>
    class InterceptKeys
    {
        // https://stackoverflow.com/questions/604410/global-keyboard-capture-in-c-sharp-application
        // https://looool.tistory.com/15
        // https://docs.microsoft.com/en-us/archive/blogs/toub/low-level-mouse-hook-in-c
        // https://docs.microsoft.com/en-us/windows/win32/learnwin32/mouse-clicks

        private const int WH_KEYBOARD_LL = 13;
        private const int WH_MOUSE_LL = 14;

        private const int WM_KEYDOWN = 0x0100;
        private enum MouseMessages
        {
            WM_LBUTTONDOWN = 0x0201,
            WM_LBUTTONUP = 0x0202,
            WM_MOUSEMOVE = 0x0200,
            WM_MOUSEWHEEL = 0x020A,
            WM_RBUTTONDOWN = 0x0204,
            WM_RBUTTONUP = 0x0205,
            WM_MBUTTONDOWN = 0x0207,
            WM_MBUTTONUP = 0x0208
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct LowLevelMouseHookStruct
        {
            public POINT point;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        public delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        public static HookProc _keyboardProc = KeyboardHookCallback;
        public static HookProc _mouseProc = MouseHookCallback;
        public static IntPtr _keyboardHookID = IntPtr.Zero;
        public static IntPtr _mouseHookID = IntPtr.Zero;

        public static string wordState = "";

        public static IntPtr SetLowLevelKeyboardHook(HookProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        public static IntPtr SetLowLevelMouseHook(HookProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_MOUSE_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        /// <summary>
        /// 키보드 입력 시 운영체제에 의해 호출될 함수입니다.
        /// </summary>
        /// <param name="nCode"></param>
        /// <param name="wParam">키가 눌린 유형</param>
        /// <param name="lParam">눌린 키의 종류</param>
        /// <returns></returns>        
        private static IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);

                // https://sites.google.com/site/douglaslash/Home/programming/c-notes--snippets/c-keycodes
                // Do something when KeyDown event occurs.

                //Console.WriteLine("KeyCode: " + (Keys)vkCode);

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
                        // TODO 사전 검색
                    }
                    Util.TaskQueue.Add("wordState", AddCharToWord, vkCode);

                    Music.PlayNoteInChord();
                    // 음표 재생 후에 Music.OnPlayNotes()가 호출되면서 시각 효과 발생
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
                else if ((vkCode == 32) || (vkCode == 9) || (vkCode == 13))
                {
                    // Whitespaces (Space, Enter, Tab)
                    void ResetWord(object[] args)
                    {
                        if (wordState.Length > 0)
                            Console.WriteLine("wordState: " + wordState);
                        wordState = "";
                    }
                    Util.TaskQueue.Add("wordState", ResetWord);

                    Music.PlayChordTransitionSync();
                    // 화음 전이 후에 Music.OnChordTransition()이 호출되면서 시각 효과 발생
                }
                else if ((vkCode >= 33 && vkCode <= 40) || (vkCode == 27) || /*(vkCode == 17) || (vkCode == 18) ||
                    (vkCode >= 91 && vkCode <= 95) || */(vkCode >= 162 && vkCode <= 165) || (vkCode >= 131072))
                {
                    // Cursor relocation (Page Up, Page Down, End, Home, Arrows), Modifier (Ctrl), etc (ESC, WinLogo, App, Sleep)
                    void ResetWord(object[] args)
                    {
                        if (wordState.Length > 0)
                            Console.WriteLine("wordState: " + wordState);
                        wordState = "";
                    }
                    Util.TaskQueue.Add("wordState", ResetWord);
                }
            }
            return CallNextHookEx(_keyboardHookID, nCode, wParam, lParam);
        }

        /// <summary>
        /// 마우스 입력 시 운영체제에 의해 호출될 함수입니다.
        /// </summary>
        /// <param name="nCode"></param>
        /// <param name="wParam">마우스 입력의 유형</param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        private static IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                if ((MouseMessages)wParam == MouseMessages.WM_LBUTTONDOWN ||
                    (MouseMessages)wParam == MouseMessages.WM_RBUTTONDOWN ||
                    (MouseMessages)wParam == MouseMessages.WM_MBUTTONDOWN)
                {
                    //LowLevelMouseHookStruct hookStruct = (LowLevelMouseHookStruct)Marshal.PtrToStructure(lParam, typeof(LowLevelMouseHookStruct));
                    //Console.WriteLine("(" + hookStruct.point.x + ", " + hookStruct.point.y + ")");

                    void ResetWord(object[] args)
                    {
                        if (wordState.Length > 0)
                            Console.WriteLine("wordState: " + wordState);
                        wordState = "";
                    }
                    Util.TaskQueue.Add("wordState", ResetWord);
                }
            }
            return CallNextHookEx(_mouseHookID, nCode, wParam, lParam);
        }

        /// <summary>
        /// Hook 프로시저를 설치합니다.
        /// </summary>
        /// <param name="idHook">설치하려는 hook 타입을 지정하는 상수</param>
        /// <param name="lpfn">Hook 프로시저의 주소</param>
        /// <param name="hMod">Hook 프로시저를 가진 인스턴스 핸들</param>
        /// <param name="dwThreadId">Hook 프로시저가 감시할 쓰레드 ID (0이면 모든 쓰레드의 메시지가 전달됨)</param>
        /// <returns>설치한 hook 핸들</returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            HookProc lpfn, IntPtr hMod, uint dwThreadId);

        /// <summary>
        /// Hook 프로시저를 해제합니다.
        /// </summary>
        /// <param name="hhk">해제하려는 hook 핸들</param>
        /// <returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        /// <summary>
        /// 다음 hook 프로시저에게 전달합니다.
        /// </summary>
        /// <param name="hhk">현재 처리하고 있는 hook의 핸들. SetWindowsHookEx에서 반환된 값</param>
        /// <param name="nCode">운영체제에서 전달받은 인자</param>
        /// <param name="wParam">운영체제에서 전달받은 인자</param>
        /// <param name="lParam">운영체제에서 전달받은 인자</param>
        /// <returns></returns>
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