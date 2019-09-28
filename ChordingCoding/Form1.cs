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
        static List<ChordingCoding.Theme> availableThemes = new List<ChordingCoding.Theme>();
        static OutputDevice outDevice;
        static bool _isReady = false;
        static Dictionary<string, int> _opacity = new Dictionary<string, int>();
        static Dictionary<string, int> _volume = new Dictionary<string, int>();
        static int noteResolution = 4;
        static int frameNumber = 0;         // 실행 후 지금까지 지난 프레임 수
        static ChordingCoding.Theme _theme;
        static List<ParticleSystem> particleSystems = new List<ParticleSystem>();
        static ParticleSystem basicParticleSystem = null;
        static List<KeyValuePair<Note, int>> syncPlayBuffer = new List<KeyValuePair<Note, int>>();
        Bitmap bitmap;
        public static Form1 form1;
        public const float frame = 32f;     // 1초에 전환되는 화면의 프레임 수
        public static Chord chord;

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
                return _opacity[theme.name];
            }
            set
            {
                if (value < 0) _opacity[theme.name] = 0;
                else if (value > 100) _opacity[theme.name] = 100;
                else _opacity[theme.name] = value;
            }
        }

        public static int volume
        {
            get
            {
                return _volume[theme.name];
            }
            set
            {
                if (value < 0) _volume[theme.name] = 0;
                else if (value > 100) _volume[theme.name] = 100;
                else _volume[theme.name] = value;
            }
        }

        public static double volumeD
        {
            get
            {
                return _volume[theme.name] / 100D;
            }
        }
        
        public static ChordingCoding.Theme theme
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
            
            outDevice = new OutputDevice(0);
            ChordingCoding.Theme.Initialize();

            availableThemes = new List<ChordingCoding.Theme>();

            availableThemes.Add(new ChordingCoding.Theme(
                "Autumn", "가을 산책",
                new ParticleSystem(/*cNum*/ 1, /*cRange*/ 0,
                                   ParticleSystem.CreateFunction.TopRandom,
                                   Particle.Type.leaf, () => Color.White,
                                   /*pSize*/ 1f, /*pLife*/ 128),
                null, new ChordingCoding.Theme.ParticleInfo(Particle.Type.leaf, (Form1.form1.Size.Height + 150) / 4, (pitch) => Color.White, 1f),
                ChordingCoding.Theme.ChordTransition.SomewhatHappy, "Guitar", "Bird", null));

            availableThemes.Add(new ChordingCoding.Theme(
                "Rain", "비 오는 날",
                new ParticleSystem(/*cNum*/ 1, /*cRange*/ 0,
                                   ParticleSystem.CreateFunction.TopRandom,
                                   Particle.Type.rain, () => Color.White,
                                   /*pSize*/ 0.1f, /*pLife*/ (Form1.form1.Size.Height + 150) / 30),
                new ParticleSystem(/*posX*/ () => 0,
                                    /*posY*/ () => 0,
                                    /*velX*/ 0, /*velY*/ 0, /*life*/ 160,
                                    /*cNum*/ 1, /*cRange*/ 0,
                                    ParticleSystem.CreateFunction.TopRandom,
                                    Particle.Type.rain, () => Color.White,
                                    /*pSize*/ 0.1f, /*pLife*/ (Form1.form1.Size.Height + 150) / 30),
                new ChordingCoding.Theme.ParticleInfo(Particle.Type.note, (Form1.form1.Size.Height + 150) / 15, (pitch) => Chord.PitchColor(pitch), 0.1f),
                ChordingCoding.Theme.ChordTransition.SomewhatBlue, "Forest", "Rain", null));

            availableThemes.Add(new ChordingCoding.Theme(
                "Star", "별 헤는 밤",
                new ParticleSystem(/*cNum*/ 1, /*cRange*/ 0,
                                   ParticleSystem.CreateFunction.Random,
                                   Particle.Type.star, () => Color.Black,
                                   /*pSize*/ 1f, /*pLife*/ 64),
                new ParticleSystem(/*posX*/ () => (float)(new Random().NextDouble() * Form1.form1.Size.Width),
                                   /*posY*/ () => (float)(new Random().NextDouble() * Form1.form1.Size.Height * 5 / 6 - Form1.form1.Size.Height / 12),
                                   /*velX*/ 2, /*velY*/ 8, /*life*/ 38,
                                   /*cNum*/ 7, /*cRange*/ 4,
                                   ParticleSystem.CreateFunction.Gaussian,
                                   Particle.Type.dot, () => Form1.chord.ChordColor(),
                                   /*pSize*/ 1, /*pLife*/ 10),
                new ChordingCoding.Theme.ParticleInfo(Particle.Type.star, 32, (pitch) => Chord.PitchColor(pitch), 1f),
                ChordingCoding.Theme.ChordTransition.SimilarOne, "Star", null, null));

            bool themeExist = false;
            foreach (ChordingCoding.Theme t in availableThemes)
            {
                _opacity[t.name] = (int)Properties.Settings.Default["Opacity" + t.name];
                _volume[t.name] = (int)Properties.Settings.Default["Volume" + t.name];

                if (t.name.Equals((string)Properties.Settings.Default["Theme"]))
                {
                    themeExist = true;
                    SetTheme(t);
                }
            }
            if (!themeExist && availableThemes.Count > 0)
            {
                SetTheme(availableThemes[0]);
            }

            chord = new Chord(_theme.chordTransition);

            noteResolution = (int)Properties.Settings.Default["NoteResolution"];
            SetNoteResolution(noteResolution);

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
            Properties.Settings.Default["Theme"] = theme.name;
            foreach (ChordingCoding.Theme theme in availableThemes)
            {
                Properties.Settings.Default["Opacity" + theme.name] = _opacity[theme.name];
                Properties.Settings.Default["Volume" + theme.name] = _volume[theme.name];
                Properties.Settings.Default["NoteResolution"] = noteResolution;
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
            if (theme.instrumentSet.instruments.ContainsKey(3) && theme.instrumentSet.instruments.ContainsKey(4))
            {
                if (frameNumber % ((int)frame * 10) == 0)
                {
                    StopPlaying(3);
                    frameNumber = 0;
                    Score score = new Score();
                    ChordingCoding.Theme.InstrumentInfo inst = theme.instrumentSet.instruments[3];
                    Note note = new Note(inst.sfxPitchModulator(45), inst.sfxRhythm, 0, 0, 3);
                    score.PlayANoteForever(outDevice, note, (int)Math.Round(inst.sfxVolume * volumeD));     // 기본 빗소리 (사라지지 않아야 함)
                }
                if (frameNumber % ((int)frame * 10) == (int)frame * 5)
                {
                    StopPlaying(4);
                    Score score = new Score();
                    ChordingCoding.Theme.InstrumentInfo inst = theme.instrumentSet.instruments[4];
                    Note note = new Note(inst.sfxPitchModulator(45), inst.sfxRhythm, 0, 0, 4);
                    score.PlayANoteForever(outDevice, note, (int)Math.Round(inst.sfxVolume * volumeD));     // 기본 빗소리 (사라지지 않아야 함)
                }
            }

            // 동기화된 박자(최소 리듬 단위)에 맞춰 버퍼에 저장되어 있던 음표 재생
            if (noteResolution > 0 && frameNumber % noteResolution == 0 && syncPlayBuffer.Count > 0)
            {
                Score score = new Score();
                foreach (KeyValuePair<Note, int> p in syncPlayBuffer)
                {
                    score.PlayANote(outDevice, p.Key, (int)Math.Round(p.Value * volumeD));
                }
                syncPlayBuffer.Clear();
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
            if (basicParticleSystem == null) return;

            basicParticleSystem.AddParticleInBasic(
                theme.particleInfoForCharacter.particleType,
                theme.particleInfoForCharacter.particleLifetime,
                theme.particleInfoForCharacter.pitchToParticleColor(pitch),
                theme.particleInfoForCharacter.particleSize);
        }

        /// <summary>
        /// 음표 하나를 버퍼에 저장했다가 다음 박자에 맞춰 재생 장치에서 재생합니다.
        /// </summary>
        /// <param name="pitch"></param>
        /// <param name="rhythm"></param>
        /// <param name="staff"></param>
        /// <param name="velocity"></param>
        public static void PlayANoteSync(int pitch, int rhythm, int staff, int velocity)
        {
            if (!_isReady) return;
            Note note = new Note(pitch, rhythm, 0, 0, staff);

            if (noteResolution == 0)
            {
                Score score = new Score();
                score.PlayANote(outDevice, note, (int)Math.Round(velocity * volumeD));
            }
            else
            {
                syncPlayBuffer.Add(new KeyValuePair<Note, int>(note, velocity));
            }
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

        /*
         * TODO
         * 현재 availableThemes에 있는 목록에 따라서 동적으로 ToolStripMenuItem을 변경해야 한다.
         * ToolStripMenuItem_Click 이벤트의 콜백 함수에 인자로 해당 버튼의 테마 이름을 넘겨주도록 해야 한다.
         */ 
        private void 가을산책ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_theme.name != "Autumn")
            {
                SetTheme(availableThemes.Find((theme) => theme.name.Equals("Autumn")));
            }
        }

        private void 비오는날ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_theme.name != "Rain")
            {
                SetTheme(availableThemes.Find((theme) => theme.name.Equals("Rain")));
            }
        }

        private void 별헤는밤ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_theme.name != "Star")
            {
                SetTheme(availableThemes.Find((theme) => theme.name.Equals("Star")));
            }
        }

        private void SetTheme(ChordingCoding.Theme theme)
        {
            가을산책ToolStripMenuItem.CheckState = CheckState.Unchecked;
            비오는날ToolStripMenuItem.CheckState = CheckState.Unchecked;
            별헤는밤ToolStripMenuItem.CheckState = CheckState.Unchecked;

            _theme = theme;
            switch (theme.name)
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
            테마ToolStripMenuItem.Text = "테마 (" + theme.displayName + ")";
            basicParticleSystem = theme.basicParticleSystem;
            particleSystems = new List<ParticleSystem>();
            for (int i = 0; i <= 6; i++) StopPlaying(i);

            foreach (KeyValuePair<int, ChordingCoding.Theme.InstrumentInfo> p in theme.instrumentSet.instruments)
            {
                outDevice.Send(new ChannelMessage(ChannelCommand.ProgramChange, p.Key, p.Value.instrumentCode));
            }

            frameNumber = 0;

            Opacity = opacity / 100D;
            trackBarMenuItem1.Value = opacity / 5;
            trackBarMenuItem2.Value = volume / 5;
            불투명도ToolStripMenuItem.Text = "불투명도 (" + opacity + "%)";
            음량ToolStripMenuItem.Text = "음량 (" + volume + "%)";
            chord = new Chord(theme.chordTransition);
        }

        private void SetNoteResolution(int resolution)
        {
            if (!new int[] { 0, 2, 4, 8, 16 }.Contains(resolution)) return;

            noteResolution = resolution;
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
                    if (syncPlayBuffer.Count > 0)
                    {
                        Score score = new Score();
                        foreach (KeyValuePair<Note, int> p in syncPlayBuffer)
                        {
                            score.PlayANote(outDevice, p.Key, (int)Math.Round(p.Value * volumeD));
                        }
                        syncPlayBuffer.Clear();
                    }
                    break;
            }
        }

        private void _4분음표ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (noteResolution != 16)
            {
                SetNoteResolution(16);
            }
        }

        private void _8분음표ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (noteResolution != 8)
            {
                SetNoteResolution(8);
            }
        }

        private void _16분음표ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (noteResolution != 4)
            {
                SetNoteResolution(4);
            }
        }

        private void _32분음표toolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (noteResolution != 2)
            {
                SetNoteResolution(2);
            }
        }

        private void _제한없음ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (noteResolution != 0)
            {
                SetNoteResolution(0);
            }
        }
    }
}
