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
        public enum Theme { Autumn, Rain, Star }
        static OutputDevice outDevice;
        static bool _isReady = false;
        static int[] _opacity = { 80, 80, 100 };
        static int[] _volume = { 100, 100, 100 };
        static int frameNumber = 0;         // 실행 후 지금까지 지난 프레임 수
        static Theme _theme = Theme.Autumn;
        static List<ParticleSystem> particleSystems = new List<ParticleSystem>();
        static ParticleSystem basicParticleSystem = null;
        Bitmap bitmap;
        public static Form1 form1;
        public const float frame = 32f;     // 1초에 전환되는 화면의 프레임 수
        public static Chord chord = new Chord(_theme);

        public delegate void TimerTickDelegate();

        /*
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
                return _opacity[(int)theme];
            }
            set
            {
                if (value < 0) _opacity[(int)theme] = 0;
                else if (value > 100) _opacity[(int)theme] = 100;
                else _opacity[(int)theme] = value;
            }
        }

        public static int volume
        {
            get
            {
                return _volume[(int)theme];
            }
            set
            {
                if (value < 0) _volume[(int)theme] = 0;
                else if (value > 100) _volume[(int)theme] = 100;
                else _volume[(int)theme] = value;
            }
        }

        public static double volumeD
        {
            get
            {
                return _volume[(int)theme] / 100D;
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
            form1 = this;

            for (int i = 0; i < Enum.GetNames(typeof(Theme)).Length; i++)
            {
                _opacity[i] = (int)Properties.Settings.Default["Opacity" + i.ToString()];
                _volume[i] = (int)Properties.Settings.Default["Volume" + i.ToString()];
            }

            /*
            Opacity = opacity / 100D;
            trackBarMenuItem1.Value = opacity / 5;
            trackBarMenuItem2.Value = volume / 5;
            불투명도ToolStripMenuItem.Text = "불투명도 (" + opacity + "%)";
            음량ToolStripMenuItem.Text = "음량 (" + volume + "%)";
            */
            outDevice = new OutputDevice(0);

            switch ((string)Properties.Settings.Default["Theme"])
            {
                case "Autumn":
                    SetTheme(Theme.Autumn);
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

            _isReady = true;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_isReady)
                outDevice.Close();
            Properties.Settings.Default["Theme"] = theme.ToString();
            for (int i = 0; i < Enum.GetNames(typeof(Theme)).Length; i++)
            {
                Properties.Settings.Default["Opacity" + i.ToString()] = _opacity[i];
                Properties.Settings.Default["Volume" + i.ToString()] = _volume[i];
            }
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
            if (_isReady)
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

            // 기본 빗소리가 멈추는 것을 대비하여 
            if (theme == Theme.Rain)
            {
                if (frameNumber % ((int)frame * 10) == 0)
                {
                    StopPlaying(3);
                    frameNumber = 0;
                    Score score = new Score();
                    Note note = new Note(45, 1, 0, 0, 3);
                    score.PlayANoteForever(outDevice, note, (int)Math.Round(24 * volumeD));     // 기본 빗소리 (사라지지 않아야 함)
                }
                if (frameNumber % ((int)frame * 10) == (int)frame * 5)
                {
                    StopPlaying(4);
                    Score score = new Score();
                    Note note = new Note(45, 1, 0, 0, 4);
                    score.PlayANoteForever(outDevice, note, (int)Math.Round(24 * volumeD));     // 기본 빗소리 (사라지지 않아야 함)
                }
            }

            frameNumber++;
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
        /// 기본 파티클 시스템에 파티클을 추가합니다.
        /// </summary>
        public static void AddParticleToBasicParticleSystem(Chord.Root pitch)
        {
            if (basicParticleSystem == null) return;

            if (theme == Theme.Autumn)
            {
                basicParticleSystem.AddParticleInBasic(Particle.Type.leaf, (Form1.form1.Size.Height + 150) / 4, Color.White, 1f);
            }
            else if (theme == Theme.Rain)
            {
                basicParticleSystem.AddParticleInBasic(Particle.Type.note, (Form1.form1.Size.Height + 150) / 15, Chord.PitchColor(pitch), 0.1f);
            }
            else if (theme == Theme.Star)
            {
                basicParticleSystem.AddParticleInBasic(Particle.Type.star, 32, Chord.PitchColor(pitch), 1f);
            }
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

        private void 가을산책ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_theme != Theme.Autumn)
            {
                SetTheme(Theme.Autumn);
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
                case Theme.Autumn:
                    _theme = Theme.Autumn;
                    가을산책ToolStripMenuItem.CheckState = CheckState.Checked;
                    비오는날ToolStripMenuItem.CheckState = CheckState.Unchecked;
                    별헤는밤ToolStripMenuItem.CheckState = CheckState.Unchecked;
                    테마ToolStripMenuItem.Text = "테마 (가을 산책)";
                    basicParticleSystem = new ParticleSystem(
                                        /*cNum*/ 1, /*cRange*/ 0,
                                        ParticleSystem.CreateFunction.TopRandom,
                                        Particle.Type.leaf, Color.White,
                                        /*pSize*/ 1f, /*pLife*/ 128);

                    StopPlaying(0);
                    StopPlaying(1);
                    StopPlaying(2);
                    StopPlaying(3);
                    StopPlaying(4);
                    particleSystems = new List<ParticleSystem>();
                    
                    // TODO
                    outDevice.Send(new ChannelMessage(ChannelCommand.ProgramChange, 0, 32));   // 사운드이펙트(고블린) -> 분위기를 만드는 역할
                    outDevice.Send(new ChannelMessage(ChannelCommand.ProgramChange, 1, 24));    // 어쿠스틱 기타(나일론) -> 주 멜로디
                    outDevice.Send(new ChannelMessage(ChannelCommand.ProgramChange, 2, 123));   // 새 지저귀는 소리 -> 효과음
                    break;
                case Theme.Rain:
                    _theme = Theme.Rain;
                    가을산책ToolStripMenuItem.CheckState = CheckState.Unchecked;
                    비오는날ToolStripMenuItem.CheckState = CheckState.Checked;
                    별헤는밤ToolStripMenuItem.CheckState = CheckState.Unchecked;
                    테마ToolStripMenuItem.Text = "테마 (비 오는 날)";
                    basicParticleSystem = new ParticleSystem(
                                        /*cNum*/ 1, /*cRange*/ 0,
                                        ParticleSystem.CreateFunction.TopRandom,
                                        Particle.Type.rain, Color.White,
                                        /*pSize*/ 0.1f, /*pLife*/ (Form1.form1.Size.Height + 150) / 30);

                    StopPlaying(0);
                    StopPlaying(1);
                    StopPlaying(2);
                    StopPlaying(3);
                    StopPlaying(4);
                    particleSystems = new List<ParticleSystem>();

                    outDevice.Send(new ChannelMessage(ChannelCommand.ProgramChange, 0, 101));   // 사운드이펙트(고블린) -> 분위기를 만드는 역할
                    outDevice.Send(new ChannelMessage(ChannelCommand.ProgramChange, 1, 12));    // 마림바 -> 주 멜로디
                    outDevice.Send(new ChannelMessage(ChannelCommand.ProgramChange, 2, 126));   // 박수 소리 -> 빗소리
                    outDevice.Send(new ChannelMessage(ChannelCommand.ProgramChange, 3, 126));   // 박수 소리 -> 빗소리
                    outDevice.Send(new ChannelMessage(ChannelCommand.ProgramChange, 4, 126));   // 박수 소리 -> 빗소리

                    frameNumber = 0;    // UpdateFrame()에서 기본 빗소리를 재생하도록 함
                    Score score = new Score();
                    Note note = new Note(45, 1, 0, 0, 4);
                    break;
                case Theme.Star:
                    _theme = Theme.Star;
                    가을산책ToolStripMenuItem.CheckState = CheckState.Unchecked;
                    비오는날ToolStripMenuItem.CheckState = CheckState.Unchecked;
                    별헤는밤ToolStripMenuItem.CheckState = CheckState.Checked;
                    테마ToolStripMenuItem.Text = "테마 (별 헤는 밤)";
                    basicParticleSystem = new ParticleSystem(
                                        /*cNum*/ 1, /*cRange*/ 0,
                                        ParticleSystem.CreateFunction.Random,
                                        Particle.Type.star, Color.Black,
                                        /*pSize*/ 1f, /*pLife*/ 64);

                    StopPlaying(0);
                    StopPlaying(1);
                    StopPlaying(2);
                    StopPlaying(3);
                    StopPlaying(4);
                    particleSystems = new List<ParticleSystem>();
                    
                    outDevice.Send(new ChannelMessage(ChannelCommand.ProgramChange, 0, 49));    // 현악 합주 2 -> 분위기를 만드는 역할
                    outDevice.Send(new ChannelMessage(ChannelCommand.ProgramChange, 1, 11));    // 비브라폰 -> 주 멜로디
                    
                    break;
            }

            Opacity = opacity / 100D;
            trackBarMenuItem1.Value = opacity / 5;
            trackBarMenuItem2.Value = volume / 5;
            불투명도ToolStripMenuItem.Text = "불투명도 (" + opacity + "%)";
            음량ToolStripMenuItem.Text = "음량 (" + volume + "%)";
            chord = new Chord(theme);
        }
    }
}
