using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Sanford.Multimedia.Midi;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Configuration;
using ChordingCoding.SFX;
using ChordingCoding.VFX;

namespace ChordingCoding.UI
{
    public partial class Form1 : Form
    {
        static bool _isReady = false;
        static Dictionary<string, int> _opacity = new Dictionary<string, int>();
        static List<ParticleSystem> particleSystems = new List<ParticleSystem>();
        static ParticleSystem basicParticleSystem = null;
        //Bitmap bitmap;
        public static Form1 form1;
        
        /*
         * 새 Theme을 추가할 때
         * 1. SFXTheme.cs의 Initialize()에서 availableSFXThemes.Add()로 새 음악 테마 생성
         * 2. Theme.cs의 Initialize()에서 _availableThemes.Add()로 새 테마(시각 효과)를 생성하고 음악 테마와 연동
         */

        #region 프로퍼티 정의 (isReady, opacity)
        public static bool isReady
        {
            get
            {
                return _isReady;
            }
        }

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

        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Form을 초기화하는 함수입니다.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {
            Form form = (Form)sender;
            form.ShowInTaskbar = false;
            form1 = this;
            
            Theme.Initialize();

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

            int resolution = (int)Properties.Settings.Default["NoteResolution"];
            Music.Initialize(Theme.CurrentTheme.SFX.Name, resolution,
                new ChordingCoding.SFX.Timer.TickDelegate[] { MarshallingUpdateFrame });
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

            SetNoteResolution(resolution);

            //bitmap = new Bitmap(Width, Height);

            notifyIcon1.ShowBalloonTip(8);

            _isReady = true;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Music.Dispose();
            Properties.Settings.Default["Theme"] = Theme.CurrentTheme.Name;
            foreach (Theme theme in Theme.AvailableThemes)
            {
                Properties.Settings.Default["Opacity" + theme.Name] = _opacity[theme.Name];
                Properties.Settings.Default["Volume" + theme.Name] = theme.SFX.Volume;
            }
            Properties.Settings.Default["NoteResolution"] = Music.NoteResolution;
            Properties.Settings.Default.Save();
            notifyIcon1.Dispose();
            _isReady = false;
        }

        /// <summary>
        /// Cross-thread 환경에서 Marshalling을 통해 안전하게 UpdateFrame 메서드를 호출하게 합니다.
        /// </summary>
        private void MarshallingUpdateFrame()
        {
            if (isReady)
            {
                BeginInvoke(new ChordingCoding.SFX.Timer.TickDelegate(UpdateFrame));
            }
        }

        /// <summary>
        /// 매 프레임마다 화면의 시각 효과를 업데이트하기 위해, 동기화된 리듬을 갖는 음악 타이머에 맞춰 호출됩니다.
        /// </summary>
        private void UpdateFrame()
        {
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

                foreach (ParticleSystem dead in deadParticleSystem)
                {
                    particleSystems.Remove(dead);
                }
            }
            catch (InvalidOperationException)
            {

            }

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
            foreach (ParticleSystem ps in particleSystems)
            {
                ps.Draw(e.Graphics);
            }
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
            // 한 번에 활성화되는 파티클 시스템 수를 10개로 제한
            if (particleSystems.Count > 10)
            {
                particleSystems.RemoveAt(0);
            }
            ParticleSystem ps = new ParticleSystem(startPosX, startPosY, velocityX, velocityY, lifetime,
                                                       createNumber, createRange, createFunction,
                                                       particleType, particleColor, particleSize, particleLifetime);
            particleSystems.Add(ps);
        }
        
        /// <summary>
        /// 새 파티클 시스템을 추가합니다.
        /// 파티클 시스템은 한 번에 10개까지만 활성화될 수 있습니다.
        /// </summary>
        public static void AddParticleSystem(ParticleSystem particleSystem)
        {
            // 한 번에 활성화되는 파티클 시스템 수를 10개로 제한
            if (particleSystems.Count > 10)
            {
                particleSystems.RemoveAt(0);
            }
            ParticleSystem ps = new ParticleSystem(particleSystem.startPositionX, particleSystem.startPositionY,
                particleSystem.velocityX, particleSystem.velocityY, particleSystem.lifetime,
                particleSystem.createNumber, particleSystem.createRange, particleSystem.createFunction,
                particleSystem.particleType, particleSystem.particleColor, particleSystem.particleSize, particleSystem.particleLifetime);

            particleSystems.Add(ps);
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
            opacity = trackBarMenuItem1.Value * 5;
            Opacity = opacity / 100D;
            불투명도ToolStripMenuItem.Text = "불투명도 (" + opacity + "%)";
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
            basicParticleSystem = theme.BasicParticleSystem;
            basicParticleSystem.particles = new List<Particle>();
            particleSystems = new List<ParticleSystem>();

            Opacity = opacity / 100D;
            trackBarMenuItem1.Value = opacity / 5;
            trackBarMenuItem2.Value = SFXTheme.CurrentSFXTheme.Volume / 5;
            불투명도ToolStripMenuItem.Text = "불투명도 (" + opacity + "%)";
            음량ToolStripMenuItem.Text = "음량 (" + SFXTheme.CurrentSFXTheme.Volume + "%)";
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
    }
}
