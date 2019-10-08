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
using ChordingCoding.SFX;
using ChordingCoding.VFX;

namespace ChordingCoding.UI
{
    public partial class Form1 : Form
    {
        //public enum Theme { Autumn, Rain, Star }
        static bool _isReady = false;
        static Dictionary<string, int> _opacity = new Dictionary<string, int>();
        //static Dictionary<string, int> _volume = new Dictionary<string, int>();
        static List<ParticleSystem> particleSystems = new List<ParticleSystem>();
        static ParticleSystem basicParticleSystem = null;
        //Bitmap bitmap;
        public static Form1 form1;

        /*
         * [Deprecated]
         * 새 Theme를 추가할 때
         * 1. Theme에 이름 추가 (주의: 각 enum에 할당된 int 값을 임의로 바꾸지 말 것)
         * 2. _opacity, _volume의 맨 뒤에 값 추가
         * 3. 솔루션 탐색기 - ChordingCoding 속성 - 설정에서 OpacityX, VolumeX 추가
         * 4. Form1_Load에서 switch문에 SetTheme() 호출 추가
         * 5. Form1 디자인에서 contextMenuStrip1 안에 테마 안에 버튼 추가
         * 6. 5.의 버튼 클릭 시 SetTheme() 호출하는 코드 추가
         * 7. SetTheme() 안에 case 추가
         * 8. SetTheme()의 다른 case에서도 5.의 버튼 체크가 해제되도록 코드 수정
         * 9. InterceptKeys.cs와 Chord.cs에서 Form1.Theme이 쓰이는 코드 수정
         */
        /*
         * 새 Theme을 추가할 때
         * 
         */

        #region 프로퍼티 정의 (isReady, opacity, volume, volumeD, theme, (private)theme.name)
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

        /*
        public static int volume
        {
            get
            {
                return _volume[theme.Name];
            }
            set
            {
                if (value < 0) _volume[theme.Name] = 0;
                else if (value > 100) _volume[theme.Name] = 100;
                else _volume[theme.Name] = value;
            }
        }

        public static double volumeD
        {
            get
            {
                return _volume[theme.Name] / 100D;
            }
        }
        
        public static Theme theme
        {
            get
            {
                return _theme;
            }
        }
        */
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
                //Console.WriteLine(t.Name);
                _opacity[t.Name] = (int)Properties.Settings.Default["Opacity" + t.Name];
                //_volume[t.Name] = (int)Properties.Settings.Default["Volume" + t.Name];
                t.SFX.Volume = (int)Properties.Settings.Default["Volume" + t.Name];

                if (t.Name.Equals((string)Properties.Settings.Default["Theme"]))
                {
                    themeExist = true;
                    SetTheme(t);
                }
            }
            if (!themeExist && Theme.AvailableThemes.Count > 0)
            {
                SetTheme(Theme.AvailableThemes[0]);
            }

            int resolution = (int)Properties.Settings.Default["NoteResolution"];
            Music.Initialize(Theme.CurrentTheme.SFX.Name, resolution,
                new Music.TimerTickDelegate[] { MarshallingUpdateFrame });

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
                Properties.Settings.Default["Volume" + theme.Name] = theme.SFX.Volume; // _volume[theme.Name];
                Properties.Settings.Default["NoteResolution"] = Music.NoteResolution;
            }
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
                BeginInvoke(new Music.TimerTickDelegate(UpdateFrame));
            }
        }

        /// <summary>
        /// 매 프레임마다 화면의 시각 효과를 업데이트하기 위해, 동기화된 리듬을 갖는 음악 타이머에 맞춰 호출됩니다.
        /// </summary>
        private void UpdateFrame()
        {
            List<ParticleSystem> deadParticleSystem = new List<ParticleSystem>();

            // 각 파티클 시스템 객체의 Update 함수 호출
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

        /*
         * TODO
         * 현재 availableThemes에 있는 목록에 따라서 동적으로 ToolStripMenuItem을 변경해야 한다.
         * ToolStripMenuItem_Click 이벤트의 콜백 함수에 인자로 해당 버튼의 테마 이름을 넘겨주도록 해야 한다.
         */ 
        private void 가을산책ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Theme.CurrentTheme.Name != "Autumn")
            {
                SetTheme(Theme.AvailableThemes.Find((theme) => theme.Name.Equals("Autumn")));
            }
        }

        private void 비오는날ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Theme.CurrentTheme.Name != "Rain")
            {
                SetTheme(Theme.AvailableThemes.Find((theme) => theme.Name.Equals("Rain")));
            }
        }

        private void 별헤는밤ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Theme.CurrentTheme.Name != "Star")
            {
                SetTheme(Theme.AvailableThemes.Find((theme) => theme.Name.Equals("Star")));
            }
        }

        /// <summary>
        /// 현재 테마를 설정하고 UI와 시각 효과를 이에 맞게 변화시킵니다.
        /// </summary>
        /// <param name="theme"></param>
        private void SetTheme(Theme theme)
        {
            가을산책ToolStripMenuItem.CheckState = CheckState.Unchecked;
            비오는날ToolStripMenuItem.CheckState = CheckState.Unchecked;
            별헤는밤ToolStripMenuItem.CheckState = CheckState.Unchecked;

            Theme.CurrentTheme = theme;
            switch (theme.Name)
            {
                /* TODO */
                case "Autumn":
                    가을산책ToolStripMenuItem.CheckState = CheckState.Checked;
                    break;
                case "Rain":
                    비오는날ToolStripMenuItem.CheckState = CheckState.Checked;
                    break;
                case "Star":
                    별헤는밤ToolStripMenuItem.CheckState = CheckState.Checked;
                    break;
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
            /*
             * TODO
             */

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
                    /*
                    if (syncPlayBuffer.Count > 0)
                    {
                        Score score = new Score();
                        foreach (KeyValuePair<Note, int> p in syncPlayBuffer)
                        {
                            score.PlayANote(outDevice, p.Key, (int)Math.Round(p.Value * volumeD));
                        }
                        syncPlayBuffer.Clear();
                    }
                    */
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
