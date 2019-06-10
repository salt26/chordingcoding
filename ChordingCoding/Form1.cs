using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Sanford.Multimedia.Midi;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Timers;

namespace ChordingCoding
{
    public partial class Form1 : Form
    {
        public enum Theme { Forest, Rain, Star }
        static OutputDevice outDevice;
        static bool _isReady = false;
        static int _opacity = 80;
        static int _volume = 100;
        static int frameNumber = 0;         // 실행 후 지금까지 지난 프레임 수
        static Theme _theme = Theme.Forest;
        static List<ParticleSystem> particleSystems = new List<ParticleSystem>();
        static ParticleSystem basicParticleSystem = null;
        Bitmap bitmap;
        public static Form1 form1;
        public const float frame = 32f;     // 1초에 전환되는 화면의 프레임 수
        public static Chord chord = new Chord(_theme);

        public delegate void TimerTickDelegate();

        #region 프로퍼티 정의 (isReady, opacity, volume, volumeD, theme)
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
                return _opacity;
            }
            set
            {
                if (value < 0) _opacity = 0;
                else if (value > 100) _opacity = 100;
                else _opacity = value;
            }
        }

        public static int volume
        {
            get
            {
                return _volume;
            }
            set
            {
                if (value < 0) _volume = 0;
                else if (value > 100) _volume = 100;
                else _volume = value;
            }
        }

        public static double volumeD
        {
            get
            {
                return _volume / 100D;
            }
        }

        public static Theme theme
        {
            get
            {
                return _theme;
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

            opacity = (int)Properties.Settings.Default["Opacity"];
            volume = (int)Properties.Settings.Default["Volume"];

            Opacity = opacity / 100D;
            trackBarMenuItem1.Value = opacity / 5;
            trackBarMenuItem2.Value = volume / 5;
            불투명도ToolStripMenuItem.Text = "불투명도 (" + opacity + "%)";
            음량ToolStripMenuItem.Text = "음량 (" + volume + "%)";

            switch ((string)Properties.Settings.Default["Theme"])
            {
                case "Forest":
                    SetTheme(Theme.Forest);
                    break;
                case "Rain":
                    SetTheme(Theme.Rain);
                    break;
                case "Star":
                    SetTheme(Theme.Star);
                    break;
            }

            bitmap = new Bitmap(Width, Height);
            
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = 1000f / frame;
            timer.Elapsed += new ElapsedEventHandler(Timer_Elapsed);
            timer.Start();

            notifyIcon1.ShowBalloonTip(8);
            outDevice = new OutputDevice(0);
            chord = new Chord(theme);
            outDevice.Send(new ChannelMessage(ChannelCommand.ProgramChange, 0, 101));   // 사운드이펙트(고블린) -> 분위기를 만드는 역할
            outDevice.Send(new ChannelMessage(ChannelCommand.ProgramChange, 1, 12));    // 마림바 -> 주 멜로디
            outDevice.Send(new ChannelMessage(ChannelCommand.ProgramChange, 2, 123));   // 새 지저귀는 소리 -> 효과음

            form1 = this;
            _isReady = true;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_isReady)
                outDevice.Close();
            Properties.Settings.Default["Theme"] = theme.ToString();
            Properties.Settings.Default["Opacity"] = opacity;
            Properties.Settings.Default["Volume"] = volume;
            Properties.Settings.Default.Save();
            notifyIcon1.Dispose();
            _isReady = false;
        }
        
        /// <summary>
        /// Cross-thread 환경에서 Marshalling을 통해 안전하게 함수를 호출하게 합니다.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            BeginInvoke(new TimerTickDelegate(UpdateFrame));
        }

        /// <summary>
        /// 매 프레임마다 화면을 업데이트하기 위해 호출됩니다.
        /// 시각 효과만 업데이트되고, 음악은 프레임과 관련이 없습니다.
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
        /// 파티클 시스템은 한 번에 5개까지만 활성화될 수 있습니다.
        /// </summary>
        public static void AddParticleSystem(
            float startPosX, float startPosY,
            float velocityX, float velocityY, int lifetime,
            int createNumber, float createRange,
            ParticleSystem.CreateFunction createFunction,
            Particle.Type particleType, Color particleColor,
            float particleSize, int particleLifetime)
        {
            // 한 번에 활성화되는 파티클 시스템 수를 5개로 제한
            if (particleSystems.Count > 5)
            {
                particleSystems.RemoveAt(0);
            }
            ParticleSystem ps = new ParticleSystem(startPosX, startPosY, velocityX, velocityY, lifetime,
                                                       createNumber, createRange, createFunction,
                                                       particleType, particleColor, particleSize, particleLifetime);
            particleSystems.Add(ps);
        }

        /// <summary>
        /// 기본 파티클 시스템에 파티클을 추가합니다.
        /// </summary>
        public static void AddParticleToBasicParticleSystem(Chord.Root pitch = Chord.Root.C)
        {
            if (basicParticleSystem == null) return;
            if (theme == Theme.Rain)
            {
                basicParticleSystem.AddParticleInBasic(Particle.Type.note, 64, Chord.PitchColor(pitch), 0.1f);
            }
            else if (theme == Theme.Star)
                basicParticleSystem.AddParticleInBasic(Particle.Type.star, 32, Chord.PitchColor(pitch), 1f);
        }

        /// <summary>
        /// 재생 장치에서 음표 하나를 재생합니다.
        /// </summary>
        /// <param name="pitch"></param>
        /// <param name="rhythm"></param>
        /// <param name="staff"></param>
        /// <param name="velocity"></param>
        public static void PlayANote(int pitch, int rhythm, int staff, int velocity = 127)
        {
            if (!_isReady) return;
            Score score = new Score();

            Note note = new Note(pitch, rhythm, 0, 0, staff);
            score.PlayANote(outDevice, note, (int)Math.Round(velocity * volumeD));
        }

        /// <summary>
        /// 재생 장치에서 재생 중인 모든 음을 멈춥니다.
        /// </summary>
        /// <param name="staff"></param>
        public static void StopPlaying(int staff)
        {
            if (!_isReady) return;
            Score score = new Score();
            score.Stop(outDevice, staff);
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
            volume = trackBarMenuItem2.Value * 5;
            음량ToolStripMenuItem.Text = "음량 (" + volume + "%)";
        }

        private void 종료ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void 숲속아침ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_theme != Theme.Forest)
            {
                SetTheme(Theme.Forest);
            }
        }

        private void 비오는날ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_theme != Theme.Rain)
            {
                SetTheme(Theme.Rain);
            }
        }

        private void 별헤는밤ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_theme != Theme.Star)
            {
                SetTheme(Theme.Star);
            }
        }

        private void SetTheme(Theme theme)
        {
            switch (theme)
            {
                case Theme.Forest:
                    숲속아침ToolStripMenuItem.CheckState = CheckState.Checked;
                    비오는날ToolStripMenuItem.CheckState = CheckState.Unchecked;
                    별헤는밤ToolStripMenuItem.CheckState = CheckState.Unchecked;
                    _theme = Theme.Forest;
                    basicParticleSystem = null;

                    StopPlaying(0);
                    StopPlaying(1);
                    StopPlaying(2);
                    particleSystems = new List<ParticleSystem>();
                    break;
                case Theme.Rain:
                    숲속아침ToolStripMenuItem.CheckState = CheckState.Unchecked;
                    비오는날ToolStripMenuItem.CheckState = CheckState.Checked;
                    별헤는밤ToolStripMenuItem.CheckState = CheckState.Unchecked;
                    _theme = Theme.Rain;
                    basicParticleSystem = new ParticleSystem(
                                        /*cNum*/ 1, /*cRange*/ 0,
                                        ParticleSystem.CreateFunction.TopRandom,
                                        Particle.Type.rain, Color.CornflowerBlue,
                                        /*pSize*/ 0.3f, /*pLife*/ 32);

                    StopPlaying(0);
                    StopPlaying(1);
                    StopPlaying(2);
                    particleSystems = new List<ParticleSystem>();
                    break;
                case Theme.Star:
                    숲속아침ToolStripMenuItem.CheckState = CheckState.Unchecked;
                    비오는날ToolStripMenuItem.CheckState = CheckState.Unchecked;
                    별헤는밤ToolStripMenuItem.CheckState = CheckState.Checked;
                    _theme = Theme.Star;
                    basicParticleSystem = new ParticleSystem(
                                        /*cNum*/ 1, /*cRange*/ 0,
                                        ParticleSystem.CreateFunction.Random,
                                        Particle.Type.star, Color.Black,
                                        /*pSize*/ 1f, /*pLife*/ 64);

                    StopPlaying(0);
                    StopPlaying(1);
                    StopPlaying(2);
                    particleSystems = new List<ParticleSystem>();
                    break;
            }
        }
    }
}
