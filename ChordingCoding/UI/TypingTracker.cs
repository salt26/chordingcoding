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
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using ChordingCoding.Utility;
using ChordingCoding.SFX;
using ChordingCoding.Sentiment;
using ChordingCoding.Word.English;
using ChordingCoding.Word.Korean;

namespace ChordingCoding.UI
{
    /// <summary>
    /// 키보드와 마우스 입력을 관리하는 클래스입니다.
    /// </summary>
    class TypingTracker
    {
        // https://stackoverflow.com/questions/604410/global-keyboard-capture-in-c-sharp-application
        // https://looool.tistory.com/15
        // https://docs.microsoft.com/en-us/archive/blogs/toub/low-level-mouse-hook-in-c
        // https://docs.microsoft.com/en-us/windows/win32/learnwin32/mouse-clicks
        // https://stackoverflow.com/questions/4372055/detect-active-window-changed-using-c-sharp-without-polling

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

        private const uint WINEVENT_OUTOFCONTEXT = 0;
        private const uint EVENT_SYSTEM_FOREGROUND = 3;

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
        public delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);
        public static WinEventDelegate wed = null;

        public static HookProc _keyboardProc = KeyboardHookCallback;
        public static HookProc _mouseProc = MouseHookCallback;
        public static IntPtr _keyboardHookID = IntPtr.Zero;
        public static IntPtr _mouseHookID = IntPtr.Zero;
        public static IntPtr _hhook = IntPtr.Zero;

        public static bool IsReady { get; private set; } = false;

        private static string wordState = "";
        private static string backspaceState = null;

        private const string savePath = "WorkingContext.csv";
        private static long prevTicks = DateTime.Now.Ticks;

        /// <summary>
        /// 키보드 및 마우스 입력 이벤트를 감지하도록 합니다.
        /// </summary>
        public static void Start()
        {
            if (!IsReady)
            {
                // 키보드 및 마우스 입력 이벤트를 감지하여 콜백을 호출하도록 합니다.
                _keyboardHookID = SetLowLevelKeyboardHook(TypingTracker._keyboardProc);
                _mouseHookID = SetLowLevelMouseHook(TypingTracker._mouseProc);
                if (MainForm.ENABLE_CONTEXT_LOGGING)
                {
#pragma warning disable CS0162 // 접근할 수 없는 코드가 있습니다.
                    wed = new WinEventDelegate(WinEventProc);
                    _hhook = SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, wed, 0, 0, WINEVENT_OUTOFCONTEXT);
#pragma warning restore CS0162 // 접근할 수 없는 코드가 있습니다.
                }
                IsReady = true;
            }
        }

        /// <summary>
        /// 키보드 및 마우스 입력 이벤트를 더이상 감지하지 않도록 합니다.
        /// Start()가 호출된 적이 있는 경우,
        /// 프로그램이 종료되기 전에 반드시 호출되어야 합니다.
        /// </summary>
        public static void Stop()
        {
            if (IsReady)
            {
                // SetHook에서 잡은 handle을 놓습니다.
                UnhookWindowsHookEx(_keyboardHookID);
                UnhookWindowsHookEx(_mouseHookID);
                if (MainForm.ENABLE_CONTEXT_LOGGING)
                {
#pragma warning disable CS0162 // 접근할 수 없는 코드가 있습니다.
                    wed = null;
                    UnhookWinEvent(_hhook);
#pragma warning restore CS0162 // 접근할 수 없는 코드가 있습니다.
                }
                IsReady = false;
            }
        }

        private static IntPtr SetLowLevelKeyboardHook(HookProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private static IntPtr SetLowLevelMouseHook(HookProc proc)
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
                if (MainForm.ENABLE_CONTEXT_LOGGING)
#pragma warning disable CS0162 // 접근할 수 없는 코드가 있습니다.
                    AppendContextLog(1, (Keys)vkCode);
#pragma warning restore CS0162 // 접근할 수 없는 코드가 있습니다.

                Keys key = (Keys)vkCode;
                if (vkCode >= 21 && vkCode <= 25 && (Control.ModifierKeys & Keys.Alt) == Keys.Alt)
                {
                    // Alt + IME Mode Change
                    // Do nothing!
                }
                else if (vkCode >= 21 && vkCode <= 25)
                {
                    // IME Mode Change (Hangul/Kana mode, Junja mode, Final mode, Hanja/Kanji mode)
                    Util.TaskQueue.Add("wordState", ResetWord);
                    Util.TaskQueue.Add("wordState", BackspaceStateToNull);
                }
                else if ((vkCode == 91 || vkCode == 92) &&
                    (Control.ModifierKeys & Keys.Alt) == Keys.Alt)
                {
                    // Alt + WinLogo 
                    // Do nothing!
                }
                else if ((vkCode == 91 || vkCode == 92) && 
                    ((Control.ModifierKeys & Keys.Control) == Keys.Control ||
                    (Control.ModifierKeys & Keys.Shift) == Keys.Shift))
                {
                    // Modifier (Ctrl, Shift) + WinLogo 
                    Util.TaskQueue.Add("wordState", BackspaceStateToNull);
                }
                else if ((Control.ModifierKeys & Keys.Control) == Keys.Control ||
                    (Control.ModifierKeys & Keys.Alt) == Keys.Alt)
                {
                    // Modifier (Ctrl, Alt)
                    Util.TaskQueue.Add("wordState", ResetWord);
                    Util.TaskQueue.Add("wordState", BackspaceStateToNull);
                }
                else if (vkCode >= 65 && vkCode <= 90)
                {
                    // Characters (Alphabet)
                    bool hasShiftPressed = (Control.ModifierKeys & Keys.Shift) == Keys.Shift;

                    Util.TaskQueue.Add("wordState", AddCharToWord, vkCode, hasShiftPressed);
                    Util.TaskQueue.Add("wordState", BackspaceStateToNull);

                    Music.PlayNoteInChord();
                    // 음표 재생 후에 Music.OnPlayNotes()가 호출되면서 시각 효과 발생
                }
                else if (vkCode == 109 || 
                    ((vkCode == 189 || vkCode == 222) &&
                    (Control.ModifierKeys & Keys.Shift) != Keys.Shift))
                {
                    // Characters (-, ')
                    Util.TaskQueue.Add("wordState", AddCharToWord, vkCode, false);
                    Util.TaskQueue.Add("wordState", BackspaceStateToNull);

                    Music.PlayNoteInChord();
                    // 음표 재생 후에 Music.OnPlayNotes()가 호출되면서 시각 효과 발생
                }
                else if ((vkCode >= 48 && vkCode <= 57) ||
                    (vkCode >= 96 && vkCode <= 111) ||
                    (vkCode >= 186 && vkCode <= 192) ||
                    (vkCode >= 219 && vkCode <= 223) ||
                    (vkCode == 226))
                {
                    // Characters (Non-alphabet)
                    Util.TaskQueue.Add("wordState", ResetWord);
                    Util.TaskQueue.Add("wordState", BackspaceStateToNull);

                    Music.PlayNoteInChord();
                    // 음표 재생 후에 Music.OnPlayNotes()가 호출되면서 시각 효과 발생
                }
                else if (vkCode == 8)
                {
                    // Backspace
                    Util.TaskQueue.Add("wordState", BackspaceWord);
                }
                else if ((vkCode == 32) || (vkCode == 9) || (vkCode == 13))
                {
                    // Whitespaces (Space, Enter, Tab)
                    Util.TaskQueue.Add("wordState", ResetWord);
                    Util.TaskQueue.Add("wordState", BackspaceStateToNull);

                    Music.PlayChordTransitionSync();
                    // 화음 전이 후에 Music.OnChordTransition()이 호출되면서 시각 효과 발생
                }
                else if ((vkCode >= 33 && vkCode <= 40) || (vkCode == 27) ||
                    (vkCode >= 91 && vkCode <= 95))
                {
                    // Cursor relocation (Page Up, Page Down, End, Home, Arrows), etc (ESC, WinLogo, App, Sleep)
                    Util.TaskQueue.Add("wordState", ResetWord);
                    Util.TaskQueue.Add("wordState", BackspaceStateToNull);
                }
                else if (vkCode == 18 || vkCode == 164 || vkCode == 165)
                {
                    // Alt
                    // This key input may not be detected...
                    Util.TaskQueue.Add("wordState", BackspaceStateToNull);
                }
                else if (vkCode == 45 || vkCode == 46 ||
                    vkCode == 20 || vkCode == 144 || vkCode == 145)
                {
                    // Insert, Delete, Caps Lock, Num Lock, Scroll Lock
                    Util.TaskQueue.Add("wordState", BackspaceStateToNull);
                }
            }
            return CallNextHookEx(_keyboardHookID, nCode, wParam, lParam);
        }

        /// <summary>
        /// 현재까지 추적한 단어를 감정 분석기에 넘기고 초기화합니다.
        /// </summary>
        /// <param name="args">없음</param>
        private static void ResetWord(object[] args)
        {
            if (!MainForm.ENABLE_SENTIMENT_ANALYZER) return;
#pragma warning disable CS0162 // 접근할 수 없는 코드가 있습니다.
            if (wordState.Length > 0)
            {
                if (IsIMESetToEnglish())
                {
                    Console.WriteLine("wordState: " + wordState);
                    EnglishSentimentAnalyzer.instance.Analyze(wordState);
                    //EnglishSentimentAnalyzer.instance.GetSentimentAndFlush().Print();
                    EnglishWordSentiment w = (EnglishWordSentiment)EnglishSentimentAnalyzer.instance.GetSentimentAndFlush();
                    w.Print();
                    // TODO 음악 생성기에 넘기기
                    SentimentState.UpdateState(w);
                }
                else
                {
                    Console.WriteLine("wordState: " + Hangul.Assemble(wordState));
                    KoreanSentimentAnalyzer.instance.Analyze(Hangul.Assemble(wordState));
                    //KoreanSentimentAnalyzer.instance.GetSentimentAndFlush().Print();
                    KoreanWordSentiment w = (KoreanWordSentiment)KoreanSentimentAnalyzer.instance.GetSentimentAndFlush();
                    w.Print();
                    // TODO 음악 생성기에 넘기기
                    SentimentState.UpdateState(w);
                }
            }
            wordState = "";
#pragma warning restore CS0162 // 접근할 수 없는 코드가 있습니다.
        }

        /// <summary>
        /// 추적하고 있는 단어에서 Backspace 입력을 처리합니다.
        /// </summary>
        /// <param name="args">없음</param>
        private static void BackspaceWord(object[] args)
        {
            if (!MainForm.ENABLE_SENTIMENT_ANALYZER) return;
#pragma warning disable CS0162 // 접근할 수 없는 코드가 있습니다.
            if (wordState.Length > 0)
            {
                if (IsIMESetToEnglish())
                    wordState = wordState.Substring(0, wordState.Length - 1);
                else
                {
                    if (backspaceState is null)
                    {
                        backspaceState = Hangul.Assemble(wordState);
                        backspaceState = backspaceState.Substring(0, backspaceState.Length - 1) +
                            Hangul.Disassemble(backspaceState.Substring(backspaceState.Length - 1, 1));
                    }
                    if (backspaceState.Length > 0)
                    {
                        backspaceState = backspaceState.Substring(0, backspaceState.Length - 1);
                        wordState = backspaceState;
                    }
                }
            }
#pragma warning restore CS0162 // 접근할 수 없는 코드가 있습니다.
        }

        /// <summary>
        /// 백스페이스 키를 연속으로 누르지 않은 경우 호출되어야 합니다.
        /// </summary>
        /// <param name="args"></param>
        private static void BackspaceStateToNull(object[] args)
        {
            backspaceState = null;
        }

        /// <summary>
        /// 추적하고 있는 단어에 새 글자를 추가합니다.
        /// </summary>
        /// <param name="args">첫 번째 인자: 입력된 키 코드(int), 두 번째 인자: 키 입력 시 Shift가 함께 눌렸는지 여부(bool)</param>
        private static void AddCharToWord(object[] args)
        {
            if (!MainForm.ENABLE_SENTIMENT_ANALYZER) return;
#pragma warning disable CS0162 // 접근할 수 없는 코드가 있습니다.
            int vkCode_ = (int)args[0];
            bool hasShiftPressed_ = (bool)args[1];
            StringBuilder charPressed = new StringBuilder(256);
            ToUnicode((uint)vkCode_, 0, new byte[256], charPressed, charPressed.Capacity, 0);
            // TODO 영어와 한글 구분하여 처리
            if (IsIMESetToEnglish())
            {
                wordState += Util.StringWithShift(charPressed.ToString(), hasShiftPressed_);
            }
            else
            {
                wordState += Hangul.EnglishToKorean(Util.StringWithShift(charPressed.ToString(), hasShiftPressed_));
            }
#pragma warning restore CS0162 // 접근할 수 없는 코드가 있습니다.
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
                if (MainForm.ENABLE_CONTEXT_LOGGING)
                {
#pragma warning disable CS0162 // 접근할 수 없는 코드가 있습니다.
                    switch ((MouseMessages)wParam)
                    {
                        case MouseMessages.WM_LBUTTONDOWN:
                            AppendContextLog(2, "MouseLeftDown");
                            break;
                        case MouseMessages.WM_RBUTTONDOWN:
                            AppendContextLog(2, "MouseRightDown");
                            break;
                        case MouseMessages.WM_MBUTTONDOWN:
                            AppendContextLog(2, "MouseMiddleDown");
                            break;
                        case MouseMessages.WM_MOUSEWHEEL:
                            AppendContextLog(2, "MouseWheel");
                            break;
                    }
#pragma warning restore CS0162 // 접근할 수 없는 코드가 있습니다.
                }

                if ((MouseMessages)wParam == MouseMessages.WM_LBUTTONDOWN ||
                    (MouseMessages)wParam == MouseMessages.WM_RBUTTONDOWN ||
                    (MouseMessages)wParam == MouseMessages.WM_MBUTTONDOWN)
                {
                    //LowLevelMouseHookStruct hookStruct = (LowLevelMouseHookStruct)Marshal.PtrToStructure(lParam, typeof(LowLevelMouseHookStruct));
                    //Console.WriteLine("(" + hookStruct.point.x + ", " + hookStruct.point.y + ")");

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

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

        /// <summary>
        /// Hook 프로시저를 해제합니다.
        /// </summary>
        /// <param name="hhk">해제하려는 hook 핸들</param>
        /// <returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        /// <summary>
        /// SetWinEventHook에서 생성된 이벤트 hook을 제거합니다.
        /// </summary>
        /// <param name="hWinEventHook"></param>
        /// <returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWinEvent(IntPtr hWinEventHook);

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
        /// <summary>
        /// 키보드로 입력한 문자를 문자열로 변환하여 receivingBuffer에 넣어줍니다.
        /// </summary>
        /// <param name="virtualKeyCode">가상 키 코드</param>
        /// <param name="scanCode"></param>
        /// <param name="keyboardState">입력받을 키 수만큼 할당된 배열</param>
        /// <param name="receivingBuffer">입력받을 키 수만큼 문자열을 받을 버퍼</param>
        /// <param name="bufferSize">입력받을 문자 수</param>
        /// <param name="flags"></param>
        /// <returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int ToUnicode(
            uint virtualKeyCode,
            uint scanCode,
            byte[] keyboardState,
            StringBuilder receivingBuffer,
            int bufferSize,
            uint flags
        );

        /// <summary>
        /// 핸들의 기본 IME 윈도우를 가져옵니다.
        /// 동아시아권(한국, 중국, 일본) 운영체제에서만 작동합니다.
        /// </summary>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        [DllImport("imm32.dll")]
        private static extern IntPtr ImmGetDefaultIMEWnd(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr IParam);

        /// <summary>
        /// 현재 포커스를 가진 창을 가져옵니다.
        /// </summary>
        /// <returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetForegroundWindow();

        /// <summary>
        /// 핸들 창의 창 이름을 가져옵니다.
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="text"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        /// <summary>
        /// 핸들 창을 생성한 프로세스 또는 쓰레드의 ID를 가져옵니다.
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="lpdwProcessId"></param>
        /// <returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern Int32 GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        /// <summary>
        /// 현재 포커스를 가진 창의 이름을 반환합니다.
        /// </summary>
        /// <returns></returns>
        private static string GetActiveWindowTitle()
        {
            const int nChars = 256;
            IntPtr handle = IntPtr.Zero;
            StringBuilder Buff = new StringBuilder(nChars);
            handle = GetForegroundWindow();

            if (GetWindowText(handle, Buff, nChars) > 0)
            {
                return Buff.ToString();
            }
            return null;
        }

        private static string GetForegroundProcessName()
        {
            IntPtr handle = IntPtr.Zero;
            handle = GetForegroundWindow();
            if (handle == null)
                return null;
            uint pid;
            GetWindowThreadProcessId(handle, out pid);

            foreach (Process p in Process.GetProcesses())
            {
                if (p.Id == pid)
                    return p.ProcessName;
            }
            return null;
        }

        /// <summary>
        /// 현재 포커스를 가진 창의 이름을 로그에 출력합니다.
        /// </summary>
        /// <param name="hWinEventHook"></param>
        /// <param name="eventType"></param>
        /// <param name="hwnd"></param>
        /// <param name="idObject"></param>
        /// <param name="idChild"></param>
        /// <param name="dwEventThread"></param>
        /// <param name="dwmsEventTime"></param>
        public static void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            if (MainForm.ENABLE_CONTEXT_LOGGING)
            {
#pragma warning disable CS0162 // 접근할 수 없는 코드가 있습니다.
                AppendContextLog(0, GetForegroundProcessName(), GetActiveWindowTitle());
#pragma warning restore CS0162 // 접근할 수 없는 코드가 있습니다.
            }
        }

        private const int WM_IME_CONTROL = 643;

        // https://m.blog.naver.com/gostarst/220627552770
        /// <summary>
        /// 현재 입력기의 상태가 영어 입력 모드인지 확인합니다.
        /// 영어이면 true, 한글이면 false를 반환합니다.
        /// 동아시아권(한국, 중국, 일본) 운영체제에서만 작동하며,
        /// 이외의 운영체제에서는 true를 반환합니다.
        /// </summary>
        /// <returns></returns>
        private static bool IsIMESetToEnglish()
        {
            IntPtr hwnd = GetForegroundWindow();
            if (hwnd == IntPtr.Zero)
            {
                return true;
            }
            try
            {
                IntPtr hime = ImmGetDefaultIMEWnd(hwnd);
                IntPtr status = SendMessage(hime, WM_IME_CONTROL, new IntPtr(0x5), new IntPtr(0));

                //Console.WriteLine("IME is English? " + (status.ToInt32() == 0));
                return status.ToInt32() == 0;
            }
            catch (Exception e)
            {
                if (e is DllNotFoundException || e is EntryPointNotFoundException)
                    return true;
                throw;
            }
        }

        /// <summary>
        /// 새 input context를 만듭니다.
        /// 동아시아권(한국, 중국, 일본) 운영체제에서만 작동합니다.
        /// </summary>
        /// <returns></returns>
        [DllImport("imm32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr ImmCreateContext();

        /// <summary>
        /// 윈도우 핸들에 input context를 연결합니다.
        /// 동아시아권(한국, 중국, 일본) 운영체제에서만 작동합니다.
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="hImc"></param>
        /// <returns></returns>
        [DllImport("imm32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr ImmAssociateContext(IntPtr hWnd, IntPtr hImc);

        /// <summary>
        /// input context를 제거합니다.
        /// ImmCreateContext()를 호출한 적이 있으면 프로그램 종료 전에 반드시 호출되어야 합니다.
        /// 동아시아권(한국, 중국, 일본) 운영체제에서만 작동합니다.
        /// </summary>
        /// <param name="hImc"></param>
        /// <returns></returns>
        [DllImport("imm32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr ImmDestroyContext(IntPtr hImc);

        private static IntPtr newHwnd, newHimc, oldImc;
        private static bool hasNewContext = false;

        /// <summary>
        /// Form에서 텍스트 입력을 활성화합니다.
        /// ChordingCoding의 Form에 포커스가 놓인 동안 한/영 전환이 가능합니다.
        /// 동아시아권(한국, 중국, 일본) 운영체제에서만 작동합니다.
        /// </summary>
        public static void NewIMEContext()
        {
            if (hasNewContext) return;
            newHwnd = MainForm.instance.Handle;
            try
            {
                newHimc = ImmCreateContext();
                oldImc = ImmAssociateContext(newHwnd, newHimc);
            }
            catch (Exception e)
            {
                if (!(e is DllNotFoundException || e is EntryPointNotFoundException))
                    throw;
            }
            finally
            {
                hasNewContext = true;
            }
        }

        /// <summary>
        /// NewContext()를 호출한 적이 있는 경우,
        /// 프로그램이 종료되기 전에 반드시 호출되어야 합니다.
        /// 동아시아권(한국, 중국, 일본) 운영체제에서만 작동합니다.
        /// </summary>
        public static void DestroyIMEContext()
        {
            if (!hasNewContext) return;
            try
            {
                ImmAssociateContext(newHwnd, oldImc);
                ImmDestroyContext(newHimc);
            }
            catch (Exception e)
            {
                if (!(e is DllNotFoundException || e is EntryPointNotFoundException))
                    throw;
            }
            finally
            {
                hasNewContext = false;
            }
        }

        public static void AppendContextLog(int type, params object[] messages)
        {
            long deltaTicks = DateTime.Now.Ticks - prevTicks;
            prevTicks = DateTime.Now.Ticks;

            string s = type + "," + DateTime.Now.Year + "," + DateTime.Now.Month + 
                "," + DateTime.Now.Day + "," + DateTime.Now.Hour + "," + DateTime.Now.Minute +
                "," + DateTime.Now.Second + "," + DateTime.Now.Millisecond + "," + (deltaTicks / 10000000f);
            foreach (object message in messages)
            {
                if (message is null)
                {
                    s += ",";
                }
                else
                {
                    s += "," + message.ToString();
                }
            }
            try
            {
                if (!File.Exists(savePath))
                {
                    File.AppendAllText(savePath, "id,year,month,day,hour,minute,second,ms,delta,e,w1,w2,w3\n", Encoding.UTF8);
                }
                File.AppendAllText(savePath, s + "\n", Encoding.UTF8);
            }
            catch (Exception e)
            {
                if (!(e is IOException))
                    throw;
            }
        }
    }
}