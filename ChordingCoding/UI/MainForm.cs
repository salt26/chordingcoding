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
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Threading;
using System.Text;
//using Sanford.Multimedia.Midi;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Configuration;
using System.Media;
using ChordingCoding.Utility;
using ChordingCoding.SFX;
using ChordingCoding.UI.VFX;
using ChordingCoding.Word.Korean;
using ChordingCoding.Word.English;

namespace ChordingCoding.UI
{
    public partial class MainForm : Form
    {
        /// <summary>
        /// 시각 효과를 표시할지 결정합니다.
        /// </summary>
        public const bool ENABLE_VFX = false;

        /// <summary>
        /// 감정 분석을 실시할지 결정합니다.
        /// </summary>
        public const bool ENABLE_SENTIMENT_ANALYZER = true;

        /// <summary>
        /// 작업 시 발생하는 이벤트를 파일에 기록할지 결정합니다.
        /// </summary>
        public const bool ENABLE_CONTEXT_LOGGING = false;

        static Dictionary<string, int> _opacity = new Dictionary<string, int>();
        static List<ParticleSystem> particleSystems = new List<ParticleSystem>();
        static ParticleSystem basicParticleSystem = null;
        static SplashScreen splash;
        static Thread splashThread;

        //Bitmap bitmap;
        public static MainForm instance;
        public KoreanSentimentAnalyzer ksa;
        public EnglishSentimentAnalyzer esa;

        /*
         * 새 Theme을 추가할 때
         * 1. SFXTheme.cs의 Initialize()에서 availableSFXThemes.Add()로 새 음악 테마 생성
         * 2. Theme.cs의 Initialize()에서 _availableThemes.Add()로 새 테마(시각 효과)를 생성하고 음악 테마와 연동
         */

        #region 프로퍼티 정의 (IsReady, opacity)
        public static bool IsReady { get; private set; }

        public static int opacity
        {
            get
            {
                return _opacity[Theme.CurrentTheme.Name];
            }
            set
            {
                if (value < 0) _opacity[Theme.CurrentTheme.Name] = 0;
                else if (value > 100) _opacity[Theme.CurrentTheme.Name] = 100;
                else _opacity[Theme.CurrentTheme.Name] = value;
            }
        }
        #endregion

        #region 마우스 클릭을 투명하게 통과시키는 코드
        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        const int GWL_EXSTYLE = -20;
        const int WS_EX_LAYERED = 0x80000;
        const int WS_EX_TRANSPARENT = 0x20;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            var style = GetWindowLong(this.Handle, GWL_EXSTYLE);
            SetWindowLong(this.Handle, GWL_EXSTYLE, style | WS_EX_LAYERED | WS_EX_TRANSPARENT);
        }
        #endregion

        #region 여러 효과음을 동시에 재생하는 코드
        // https://stackoverflow.com/questions/1285294/play-multiple-sounds-using-soundplayer

        [DllImport("winmm.dll")]
        static extern Int32 mciSendString(string command, StringBuilder buffer, int bufferSize, IntPtr hwndCallback);

        /// <summary>
        /// .wav 파일을 재생합니다.
        /// 동시에 여러 파일을 재생할 수 있습니다.
        /// </summary>
        /// <param name="soundFileName"></param>
        private void PlayOneShot(string soundFileName)
        {
            StringBuilder sb = new StringBuilder();
            mciSendString("open \"" + soundFileName + "\" alias " + soundFileName, sb, 0, IntPtr.Zero);
            mciSendString("play " + soundFileName + " from 0", sb, 0, IntPtr.Zero);
        }

        /// <summary>
        /// PlayOneShot()으로 재생하던 소리를 멈춥니다.
        /// </summary>
        /// <param name="soundFileName"></param>
        private void StopPlaying(string soundFileName)
        {
            StringBuilder sb = new StringBuilder();
            mciSendString("stop " + "Typing2.wav", sb, 0, IntPtr.Zero);
            mciSendString("close " + "Typing2.wav", sb, 0, IntPtr.Zero);
        }
        #endregion

        public MainForm()
        {
            splash = new SplashScreen();
            splashThread = new Thread(new ThreadStart(() => Application.Run(splash)));
            splashThread.Start();

            MarshallingUpdateSplashScreen(0);
            InitializeComponent();

            MarshallingUpdateSplashScreen(1);
        }

        /// <summary>
        /// Form을 초기화하는 함수입니다.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {
            MarshallingUpdateSplashScreen(2);

            #region Initialize phase

            Form form = (Form)sender;
            form.ShowInTaskbar = false;
            instance = this;

            MarshallingUpdateSplashScreen(3);
            TypingTracker.NewIMEContext();

            MarshallingUpdateSplashScreen(4);
            Theme.Initialize();

            MarshallingUpdateSplashScreen(5);
            bool themeExist = false;
            foreach (Theme t in Theme.AvailableThemes)
            {
                _opacity[t.Name] = (int)Properties.Settings.Default["Opacity" + t.Name];
                t.SFX.Volume = (int)Properties.Settings.Default["Volume" + t.Name];

                if (t.Name.Equals((string)Properties.Settings.Default["Theme"]))
                {
                    themeExist = true;
                    SetTheme(t);
                }
            }

            MarshallingUpdateSplashScreen(6);
            /*
             * TODO
             * 테마를 추가했다가 삭제하는 경우를 처리해야 함.
             * Theme.AvailableThemes에 themeName의 테마가 없는데
             * Properties.Settings.Default["Opacity/Volume" + themeName]이 남아있는 경우
             * Properties.Settings.Default.Properties.Remove() 사용하여 제거 바람.
             */
            if (!themeExist && Theme.AvailableThemes.Count > 0)
            {
                SetTheme(Theme.AvailableThemes[0]);
            }

            MarshallingUpdateSplashScreen(7);
            int resolution = (int)Properties.Settings.Default["NoteResolution"];

            MarshallingUpdateSplashScreen(8);
            Music.Initialize(Theme.CurrentTheme.SFX.Name, resolution,
                new SFX.Timer.TickDelegate[] { MarshallingUpdateFrame });

            MarshallingUpdateSplashScreen(9);
            foreach (Theme t in Theme.AvailableThemes)
            {
                // 이 코드를 위의 foreach문과 합치면 CurrentSFXTheme이 아직 설정되지 않은 상태가 되어 문제가 발생할 수 있음
                t.SFX.hasAccompanied = (bool)Properties.Settings.Default["Accompaniment" + t.Name];
            }

            if (Theme.CurrentTheme.SFX.hasAccompanied)
            {
                자동반주ToolStripMenuItem.CheckState = CheckState.Checked;
            }
            else
            {
                자동반주ToolStripMenuItem.CheckState = CheckState.Unchecked;
            }

            MarshallingUpdateSplashScreen(10);
            if (ENABLE_VFX)
            {
#pragma warning disable CS0162 // 접근할 수 없는 코드가 있습니다.
                Music.OnPlayNotes += (pitch) => AddParticleToBasicParticleSystem((Chord.Root)(pitch % 12));
                Music.OnChordTransition += (pitch) =>
                {
                    if (Theme.CurrentTheme.ParticleSystemForWhitespace != null)
                    {
                        AddParticleSystem(Theme.CurrentTheme.ParticleSystemForWhitespace);
                    }
                    else
                    {
                        AddParticleToBasicParticleSystem((Chord.Root)(pitch % 12));
                    }
                };
#pragma warning restore CS0162 // 접근할 수 없는 코드가 있습니다.
            }
            else
            {
#pragma warning disable CS0162 // 접근할 수 없는 코드가 있습니다.
                List<TrackBarMenuItem> l = new List<TrackBarMenuItem>();
                foreach (TrackBarMenuItem i in 불투명도ToolStripMenuItem.DropDownItems)
                {
                    l.Add(i);
                }
                foreach (TrackBarMenuItem i in l)
                {
                    i.TrackBar.Dispose();
                    i.Dispose();
                }
                불투명도ToolStripMenuItem.Dispose();
#pragma warning restore CS0162 // 접근할 수 없는 코드가 있습니다.
            }

            MarshallingUpdateSplashScreen(11);
            SetNoteResolution(resolution);

            MarshallingUpdateSplashScreen(12);
            if (ENABLE_SENTIMENT_ANALYZER)
            {
#pragma warning disable CS0162 // 접근할 수 없는 코드가 있습니다.
                ksa = new KoreanSentimentAnalyzer(); // 반드시 Music.Initialize()가 완료된 후에 호출할 것.
#pragma warning restore CS0162 // 접근할 수 없는 코드가 있습니다.
            }

            MarshallingUpdateSplashScreen(13);
            if (ENABLE_SENTIMENT_ANALYZER)
            {
#pragma warning disable CS0162 // 접근할 수 없는 코드가 있습니다.
                esa = new EnglishSentimentAnalyzer(); // 반드시 Music.Initialize()가 완료된 후에 호출할 것.
#pragma warning restore CS0162 // 접근할 수 없는 코드가 있습니다.
            }

            #endregion

            MarshallingUpdateSplashScreen(14);

            #region Start phase

            TypingTracker.Start();
            if (ENABLE_SENTIMENT_ANALYZER)
            {
#pragma warning disable CS0162 // 접근할 수 없는 코드가 있습니다.
                ksa.Start();
                esa.Start();
#pragma warning restore CS0162 // 접근할 수 없는 코드가 있습니다.
            }
            Music.Start();

            notifyIcon1.ShowBalloonTip(8);
            MarshallingUpdateSplashScreen(15);
            splash.DisposeFont();
            splashThread.Abort();

            IsReady = true;

            #endregion
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Music.Dispose();
            Properties.Settings.Default["Theme"] = Theme.CurrentTheme.Name;
            foreach (Theme theme in Theme.AvailableThemes)
            {
                Properties.Settings.Default["Opacity" + theme.Name] = _opacity[theme.Name];
                Properties.Settings.Default["Volume" + theme.Name] = theme.SFX.Volume;
                Properties.Settings.Default["Accompaniment" + theme.Name] = theme.SFX.hasAccompanied;
            }
            Properties.Settings.Default["NoteResolution"] = Music.NoteResolution;
            Properties.Settings.Default.Save();
            notifyIcon1.Dispose();
            TypingTracker.DestroyIMEContext();
            TypingTracker.Stop();
            IsReady = false;
        }

        private void MarshallingUpdateSplashScreen(int step)
        {
            if (splash is null) return;

            Bitmap b;
            switch (step)
            {
                case 0: b = Properties.Resources.logo00; break;
                case 1: b = Properties.Resources.logo01; break;
                case 2: b = Properties.Resources.logo02; break;
                case 3: b = Properties.Resources.logo03; break;
                case 4: b = Properties.Resources.logo04; break;
                case 5: b = Properties.Resources.logo05; break;
                case 6: b = Properties.Resources.logo06; break;
                case 7: b = Properties.Resources.logo07; break;
                case 8: b = Properties.Resources.logo08; break;
                case 9: b = Properties.Resources.logo09; break;
                case 10: b = Properties.Resources.logo10; break;
                case 11: b = Properties.Resources.logo11; break;
                case 12: b = Properties.Resources.logo12; break;
                case 13: b = Properties.Resources.logo13; break;
                case 14: b = Properties.Resources.logo14; break;
                default: b = Properties.Resources.logo00; break;
            }

            if (step <= 14 && step > 0)
            {
                PlayOneShot("Typing.wav");
            }

            if (splash.InvokeRequired)
            {
                if (step > 14)
                {
                    splash.Invoke(new EventHandler(delegate
                    {
                        splash.Close();
                    }));
                }
                else
                {
                    splash.Invoke(new EventHandler(delegate
                    {
                        splash.BackgroundImage = b;
                    }));
                }
            }
            else
            {
                if (step > 14) splash.Close();
                else splash.BackgroundImage = b;
            }
        }

        /// <summary>
        /// Cross-thread 환경에서 Marshalling을 통해 안전하게 UpdateFrame 메서드를 호출하게 합니다.
        /// </summary>
        private void MarshallingUpdateFrame()
        {
            if (ENABLE_VFX && IsReady)
            {
                BeginInvoke(new SFX.Timer.TickDelegate(UpdateFrame));
            }
        }

        /// <summary>
        /// 매 프레임마다 화면의 시각 효과를 업데이트하기 위해, 동기화된 리듬을 갖는 음악 타이머에 맞춰 호출됩니다.
        /// </summary>
        private void UpdateFrame()
        {
            /*
            List<ParticleSystem> deadParticleSystem = new List<ParticleSystem>();

            // 각 파티클 시스템 객체의 Update 함수 호출
            try
            {
                foreach (ParticleSystem ps in particleSystems)
                {
                    // 수명이 다한 파티클 시스템 처리
                    if (ps.CanDestroy())
                    {
                        deadParticleSystem.Add(ps);
                    }
                    else
                    {
                        ps.Update();
                    }
                }
                
                particleSystems.RemoveAll(x => deadParticleSystem.Contains(x));
            }
            catch (InvalidOperationException)
            {

            }
            */

            // TODO
            void particleSystemsUpdateOrRemove(object[] args)
            {
                List<ParticleSystem> particleSystems_ = args[0] as List<ParticleSystem>;
                
                List<ParticleSystem> deadParticleSystems = new List<ParticleSystem>();
                for (int i = particleSystems_.Count - 1; i >= 0; i--)
                {
                    // 수명이 다한 파티클 시스템 처리
                    if (particleSystems_[i].CanDestroy())
                    {
                        deadParticleSystems.Add(particleSystems_[i]);
                    }
                    else
                    {
                        particleSystems_[i].Update();
                    }
                }
                particleSystems_.RemoveAll(x => deadParticleSystems.Contains(x));
            }

            Util.TaskQueue.Add("particleSystems", particleSystemsUpdateOrRemove, particleSystems);
            if (basicParticleSystem != null)
            {
                basicParticleSystem.Update();
            }

            if (opacity > 0)
                Invalidate(true);   // 화면을 다시 그리게 함
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            // 각 파티클 시스템 객체의 Draw 함수 호출
            void particleSystemsDraw(object[] args)
            {
                List<ParticleSystem> particleSystems_ = args[0] as List<ParticleSystem>;
                Graphics graphics_ = args[1] as Graphics;

                for (int i = particleSystems_.Count - 1; i >= 0; i--)
                {
                    particleSystems_[i].Draw(graphics_);
                }
            }
            
            Util.TaskQueue.Add("particleSystems", particleSystemsDraw, particleSystems, e.Graphics);
            if (basicParticleSystem != null)
                basicParticleSystem.Draw(e.Graphics);
        }

        /// <summary>
        /// 새 파티클 시스템을 추가합니다.
        /// 파티클 시스템은 한 번에 10개까지만 활성화될 수 있습니다.
        /// </summary>
        public static void AddParticleSystem(
            ParticleSystem.StartPosition startPosX, ParticleSystem.StartPosition startPosY,
            float velocityX, float velocityY, int lifetime,
            int createNumber, float createRange,
            ParticleSystem.CreateFunction createFunction,
            Particle.Type particleType, ParticleSystem.ParticleColor particleColor,
            float particleSize, int particleLifetime)
        {
            void particleSystemsRemoveAtZero(object[] args)
            {
                List<ParticleSystem> particleSystems_ = args[0] as List<ParticleSystem>;

                if (particleSystems_.Count > 10)
                {
                    particleSystems_.RemoveAt(0);
                }
            }

            void particleSystemsAdd(object[] args)
            {
                List<ParticleSystem> particleSystems_ = args[0] as List<ParticleSystem>;
                ParticleSystem particleSystem_ = args[1] as ParticleSystem;

                particleSystems_.Add(particleSystem_);
            }

            // 한 번에 활성화되는 파티클 시스템 수를 10개로 제한
            Util.TaskQueue.Add("particleSystems", particleSystemsRemoveAtZero, particleSystems);
            ParticleSystem ps = new ParticleSystem(startPosX, startPosY, velocityX, velocityY, lifetime,
                                                       createNumber, createRange, createFunction,
                                                       particleType, particleColor, particleSize, particleLifetime);
            Util.TaskQueue.Add("particleSystems", particleSystemsAdd, particleSystems, ps);
        }
        
        /// <summary>
        /// 새 파티클 시스템을 추가합니다.
        /// 파티클 시스템은 한 번에 10개까지만 활성화될 수 있습니다.
        /// </summary>
        public static void AddParticleSystem(ParticleSystem particleSystem)
        {
            void particleSystemsRemoveAtZero(object[] args)
            {
                List<ParticleSystem> particleSystems_ = args[0] as List<ParticleSystem>;

                if (particleSystems_.Count > 10)
                {
                    particleSystems_.RemoveAt(0);
                }
            }

            void particleSystemsAdd(object[] args)
            {
                List<ParticleSystem> particleSystems_ = args[0] as List<ParticleSystem>;
                ParticleSystem particleSystem_ = args[1] as ParticleSystem;

                particleSystems_.Add(particleSystem_);
            }

            // 한 번에 활성화되는 파티클 시스템 수를 10개로 제한
            Util.TaskQueue.Add("particleSystems", particleSystemsRemoveAtZero, particleSystems);
            ParticleSystem ps = new ParticleSystem(particleSystem.startPositionX, particleSystem.startPositionY,
                particleSystem.velocityX, particleSystem.velocityY, particleSystem.lifetime,
                particleSystem.createNumber, particleSystem.createRange, particleSystem.createFunction,
                particleSystem.particleType, particleSystem.particleColor, particleSystem.particleSize, particleSystem.particleLifetime);

            Util.TaskQueue.Add("particleSystems", particleSystemsAdd, particleSystems, ps);
        }

        /// <summary>
        /// 기본 파티클 시스템에 파티클을 추가합니다.
        /// </summary>
        public static void AddParticleToBasicParticleSystem(Chord.Root pitch)
        {
            if (basicParticleSystem == null)
            {
                return;
            }

            basicParticleSystem.AddParticleInBasic(
                Theme.CurrentTheme.ParticleInfoForCharacter.particleType,
                Theme.CurrentTheme.ParticleInfoForCharacter.particleLifetime,
                Theme.CurrentTheme.ParticleInfoForCharacter.pitchToParticleColor(pitch),
                Theme.CurrentTheme.ParticleInfoForCharacter.particleSize);
        }

        /// <summary>
        /// 새 theme이 만들어질 때, UI에서 사용자가 이 테마로 설정할 수 있도록 UI를 변경합니다.
        /// </summary>
        /// <param name="theme"></param>
        public void AddNewThemeToolStripMenuItem(Theme theme)
        {
            ToolStripMenuItem item = new ToolStripMenuItem()
            {
                Name = theme.Name.Replace(' ', '_') + "ToolStripMenuItem",
                Text = theme.DisplayName,
            };
            item.Click += (object sender, EventArgs e) => {
                if (Theme.CurrentTheme.Name != theme.Name)
                {
                    SetTheme(theme);
                }
            };
            테마ToolStripMenuItem.DropDownItems.Add(item);

            bool hasAddProperties = false;
            try
            {
                int temp = (int)Properties.Settings.Default["Opacity" + theme.Name];
            }
            catch (SettingsPropertyNotFoundException)
            {
                SettingsProperty opacityProperty = new SettingsProperty("Opacity" + theme.Name);
                opacityProperty.DefaultValue = 80;
                opacityProperty.IsReadOnly = false;
                opacityProperty.PropertyType = typeof(int);
                opacityProperty.Provider = Properties.Settings.Default.Providers["LocalFileSettingsProvider"];
                opacityProperty.Attributes.Add(typeof(UserScopedSettingAttribute), new UserScopedSettingAttribute());
                Properties.Settings.Default.Properties.Add(opacityProperty);

                hasAddProperties = true;
            }
            try
            {
                int temp = (int)Properties.Settings.Default["Volume" + theme.Name];
            }
            catch (SettingsPropertyNotFoundException)
            {
                SettingsProperty volumeProperty = new SettingsProperty("Volume" + theme.Name);
                volumeProperty.DefaultValue = 80;
                volumeProperty.IsReadOnly = false;
                volumeProperty.PropertyType = typeof(int);
                volumeProperty.Provider = Properties.Settings.Default.Providers["LocalFileSettingsProvider"];
                volumeProperty.Attributes.Add(typeof(UserScopedSettingAttribute), new UserScopedSettingAttribute());
                Properties.Settings.Default.Properties.Add(volumeProperty);

                hasAddProperties = true;
            }
            try
            {
                bool temp = (bool)Properties.Settings.Default["Accompaniment" + theme.Name];
            }
            catch (SettingsPropertyNotFoundException)
            {
                SettingsProperty accompanimentProperty = new SettingsProperty("Accompaniment" + theme.Name);
                accompanimentProperty.DefaultValue = true;
                accompanimentProperty.IsReadOnly = false;
                accompanimentProperty.PropertyType = typeof(bool);
                accompanimentProperty.Provider = Properties.Settings.Default.Providers["LocalFileSettingsProvider"];
                accompanimentProperty.Attributes.Add(typeof(UserScopedSettingAttribute), new UserScopedSettingAttribute());
                Properties.Settings.Default.Properties.Add(accompanimentProperty);

                hasAddProperties = true;
            }

            if (hasAddProperties)
            {
                Properties.Settings.Default.Reload();
                Properties.Settings.Default.Save();
            }
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                MethodInfo mi = typeof(NotifyIcon).GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic);
                mi.Invoke(notifyIcon1, null);
            }
            else if (e.Button == MouseButtons.Right)
            {
                notifyIcon1.ContextMenuStrip = contextMenuStrip1;
            }
        }

        private void trackBarMenuItem1_ValueChanged(object sender, EventArgs e)
        {
            if (ENABLE_VFX)
            {
#pragma warning disable CS0162 // 접근할 수 없는 코드가 있습니다.
                opacity = trackBarMenuItem1.Value * 5;
                Opacity = opacity / 100D;
                불투명도ToolStripMenuItem.Text = "불투명도 (" + opacity + "%)";
#pragma warning restore CS0162 // 접근할 수 없는 코드가 있습니다.
            }
        }


        private void trackBarMenuItem2_ValueChanged(object sender, EventArgs e)
        {
            SFXTheme.CurrentSFXTheme.Volume = trackBarMenuItem2.Value * 5;
            음량ToolStripMenuItem.Text = "음량 (" + SFXTheme.CurrentSFXTheme.Volume + "%)";
        }

        private void 종료ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// 현재 테마를 설정하고 UI와 시각 효과를 이에 맞게 변화시킵니다.
        /// </summary>
        /// <param name="theme"></param>
        private void SetTheme(Theme theme)
        {
            Theme.CurrentTheme = theme;

            foreach (ToolStripMenuItem item in 테마ToolStripMenuItem.DropDownItems)
            {
                if (item.Name.Substring(0,
                    item.Name.IndexOf("ToolStripMenuItem")).Equals(theme.Name))
                {
                    item.CheckState = CheckState.Checked;
                }
                else
                {
                    item.CheckState = CheckState.Unchecked;
                }
            }
            테마ToolStripMenuItem.Text = "테마 (" + theme.DisplayName + ")";

            Opacity = opacity / 100D;

            if (ENABLE_VFX)
            {
#pragma warning disable CS0162 // 접근할 수 없는 코드가 있습니다.
                basicParticleSystem = theme.BasicParticleSystem;
                basicParticleSystem.particles = new List<Particle>();
                particleSystems = new List<ParticleSystem>();

                trackBarMenuItem1.Value = opacity / 5;
                불투명도ToolStripMenuItem.Text = "불투명도 (" + opacity + "%)";
#pragma warning restore CS0162 // 접근할 수 없는 코드가 있습니다.
            }

            trackBarMenuItem2.Value = SFXTheme.CurrentSFXTheme.Volume / 5;
            음량ToolStripMenuItem.Text = "음량 (" + SFXTheme.CurrentSFXTheme.Volume + "%)";

            if (Theme.CurrentTheme.SFX.hasAccompanied)
            {
                자동반주ToolStripMenuItem.CheckState = CheckState.Checked;
            }
            else
            {
                자동반주ToolStripMenuItem.CheckState = CheckState.Unchecked;
            }
        }

        /// <summary>
        /// 음악의 단위 리듬을 설정하고 UI를 이에 맞게 변화시킵니다.
        /// </summary>
        /// <param name="resolution"></param>
        private void SetNoteResolution(int resolution)
        {
            if (!new int[] { 0, 2, 4, 8, 16 }.Contains(resolution)) return;

            Music.NoteResolution = resolution;
            _4분음표ToolStripMenuItem.CheckState = CheckState.Unchecked;
            _8분음표ToolStripMenuItem.CheckState = CheckState.Unchecked;
            _16분음표ToolStripMenuItem.CheckState = CheckState.Unchecked;
            _32분음표ToolStripMenuItem.CheckState = CheckState.Unchecked;
            _없음ToolStripMenuItem.CheckState = CheckState.Unchecked;

            switch (resolution)
            {
                case 16:
                    _4분음표ToolStripMenuItem.CheckState = CheckState.Checked;
                    단위리듬ToolStripMenuItem.Text = "단위 리듬 (4분음표)";
                    break;
                case 8:
                    _8분음표ToolStripMenuItem.CheckState = CheckState.Checked;
                    단위리듬ToolStripMenuItem.Text = "단위 리듬 (8분음표)";
                    break;
                case 4:
                    _16분음표ToolStripMenuItem.CheckState = CheckState.Checked;
                    단위리듬ToolStripMenuItem.Text = "단위 리듬 (16분음표)";
                    break;
                case 2:
                    _32분음표ToolStripMenuItem.CheckState = CheckState.Checked;
                    단위리듬ToolStripMenuItem.Text = "단위 리듬 (32분음표)";
                    break;
                case 0:
                    _없음ToolStripMenuItem.CheckState = CheckState.Checked;
                    단위리듬ToolStripMenuItem.Text = "단위 리듬 (없음)";
                    break;
            }
        }

        private void _4분음표ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Music.NoteResolution != 16)
            {
                SetNoteResolution(16);
            }
        }

        private void _8분음표ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Music.NoteResolution != 8)
            {
                SetNoteResolution(8);
            }
        }

        private void _16분음표ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Music.NoteResolution != 4)
            {
                SetNoteResolution(4);
            }
        }

        private void _32분음표toolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Music.NoteResolution != 2)
            {
                SetNoteResolution(2);
            }
        }

        private void _제한없음ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Music.NoteResolution != 0)
            {
                SetNoteResolution(0);
            }
        }

        private void 자동반주ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Theme.CurrentTheme.SFX.hasAccompanied = !Theme.CurrentTheme.SFX.hasAccompanied;
            if (Theme.CurrentTheme.SFX.hasAccompanied)
            {
                자동반주ToolStripMenuItem.CheckState = CheckState.Checked;
            }
            else
            {
                자동반주ToolStripMenuItem.CheckState = CheckState.Unchecked;
            }
        }
    }
}
