using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using ChordingCoding.SFX;
using ChordingCoding.VFX;
using ChordingCoding.UI;

namespace ChordingCoding
{
    /// <summary>
    /// SFXTheme과 시각 효과 속성을 포함하는 테마 클래스입니다.
    /// </summary>
    public class Theme
    {
        /*
         * Theme 객체가 가질 수 있는 속성들입니다.
         */
        #region Theme attributes

        public string Name { get; }

        /// <summary>
        /// 테마의 한글(보여질) 이름
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// 기본 파티클 시스템
        /// </summary>
        public ParticleSystem BasicParticleSystem { get; }

        /// <summary>
        /// 공백 문자를 입력할 때 추가로 생성될 파티클 시스템
        /// </summary>
        public ParticleSystem ParticleSystemForWhitespace { get; }

        /// <summary>
        /// 일반 문자를 입력할 때 기본 파티클 시스템 안에 생성할 파티클의 정보
        /// </summary>
        public ParticleInfo ParticleInfoForCharacter { get; }

        /// <summary>
        /// 음악과 관련된 테마 속성
        /// </summary>
        public SFXTheme SFX { get; }

        #endregion

        private static List<Theme> _availableThemes = new List<Theme>();
        public static List<Theme> AvailableThemes
        {
            get
            {
                return _availableThemes;
            }
        }
        public static bool IsReady { get; private set; } = false;

        private static Theme _theme;
        public static Theme CurrentTheme
        {
            get
            {
                return _theme;
            }
            set
            {
                _theme = value;
                SFXTheme.CurrentSFXTheme = _theme.SFX;
            }
        }

        public struct ParticleInfo
        {
            public delegate Color PitchToColor(Chord.Root pitch);

            public Particle.Type particleType;
            public int particleLifetime;
            public PitchToColor pitchToParticleColor;
            public float particleSize;

            public ParticleInfo(Particle.Type type, int lifetime, PitchToColor pitchToColor, float size)
            {
                particleType = type;
                particleLifetime = lifetime;
                pitchToParticleColor = pitchToColor;
                particleSize = size;
            }
        }
        
        /// <summary>
        /// 새 테마를 생성합니다.
        /// Theme.Initialize()가 호출된 적이 없으면 name이 null인 테마가 생성됩니다.
        /// </summary>
        /// <param name="name">내부적으로 사용될 이름</param>
        /// <param name="displayName">표시할 이름</param>
        /// <param name="basicPS">기본 파티클 시스템</param>
        /// <param name="PSForWhitespace">공백 문자 입력 시 생성될 파티클 시스템 (nullable)</param>
        /// <param name="PIForCharacter">일반 문자 입력 시 기본 파티클 시스템 안에 생성될 파티클 정보</param>
        /// <param name="transition">공백 문자 입력 시의 화음 전이 확률 분포</param>
        /// <param name="SFXThemeName">음악 테마 이름</param>
        public Theme(string name, string displayName, ParticleSystem basicPS, ParticleSystem PSForWhitespace, ParticleInfo PIForCharacter, string SFXThemeName)
        {
            if (!SFXTheme.IsReady)
            {
                this.Name = null;
                return;
            }

            this.Name = name;
            this.DisplayName = displayName;
            BasicParticleSystem = basicPS;
            ParticleSystemForWhitespace = PSForWhitespace;
            ParticleInfoForCharacter = PIForCharacter;
            SFX = SFXTheme.FindSFXTheme(SFXThemeName);
            Form1.form1.AddNewThemeToolStripMenuItem(this);
        }

        /// <summary>
        /// 음악 테마를 초기화한 후에 기본 테마들을 생성합니다. 새 테마를 생성하는 어떤 코드보다도 먼저 호출되어야 합니다.
        /// </summary>
        public static void Initialize()
        {
            if (IsReady) return;

            SFXTheme.Initialize();

            _availableThemes = new List<Theme>();

            _availableThemes.Add(new Theme(
                "Autumn", "가을 산책",
                new ParticleSystem(/*cNum*/ 1, /*cRange*/ 0,
                                   ParticleSystem.CreateFunction.TopRandom,
                                   Particle.Type.leaf, () => Color.White,
                                   /*pSize*/ 1f, /*pLife*/ 128),
                null, new Theme.ParticleInfo(Particle.Type.leaf, (Form1.form1.Size.Height + 150) / 4, (pitch) => Color.White, 1f),
                "Autumn"));

            /*
            _availableThemes.Add(new Theme(
                "Rain", "비 오는 날",
                new ParticleSystem(1, 0,
                                   ParticleSystem.CreateFunction.TopRandom,
                                   Particle.Type.rain, () => Color.White,
                                   0.1f, (Form1.form1.Size.Height + 150) / 30),
                new ParticleSystem(() => 0,
                                   () => 0,
                                   0, 0, 160,
                                   1, 0,
                                   ParticleSystem.CreateFunction.TopRandom,
                                   Particle.Type.rain, () => Color.White,
                                   0.1f, (Form1.form1.Size.Height + 150) / 30),
                new Theme.ParticleInfo(Particle.Type.note, (Form1.form1.Size.Height + 150) / 15, (pitch) => Chord.PitchColor(pitch), 0.1f),
                "Rain"));
            */

            _availableThemes.Add(new Theme(
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
                                   Particle.Type.dot, () => Music.chord.ChordColor(),
                                   /*pSize*/ 1, /*pLife*/ 10),
                new Theme.ParticleInfo(Particle.Type.star, 32, (pitch) => Chord.PitchColor(pitch), 1f),
                "Star"));

            /*
            _availableThemes.Add(new Theme(
                "Forest", "숲 속 아침",
                new ParticleSystem(4, 0,
                                   ParticleSystem.CreateFunction.TopRandom,
                                   Particle.Type.leaf, () => Color.White,
                                   0.1f, 128),
                new ParticleSystem(() => (float)(new Random().NextDouble() * Form1.form1.Size.Width),
                                   () => (float)(new Random().NextDouble() * Form1.form1.Size.Height * 5 / 6 - Form1.form1.Size.Height / 12),
                                   0, 16, 38,
                                   2, 200,
                                   ParticleSystem.CreateFunction.Gaussian,
                                   Particle.Type.leaf, () => Music.chord.ChordColor(),
                                   0.7f, 40),
                new Theme.ParticleInfo(Particle.Type.rain, (Form1.form1.Size.Height + 150) / 4, (pitch) => Color.White, 0.2f),
                "Forest"));
            */
            
            _availableThemes.Add(new Theme(
                "Pianoforte", "피아노포르테",
                new ParticleSystem(/*cNum*/ 4, /*cRange*/ 0,
                                   ParticleSystem.CreateFunction.TopRandom,
                                   Particle.Type.leaf, () => Color.White,
                                   /*pSize*/ 0.1f, /*pLife*/ 128),
                new ParticleSystem(/*posX*/ () => (float)(new Random().NextDouble() * Form1.form1.Size.Width),
                                   /*posY*/ () => (float)(new Random().NextDouble() * Form1.form1.Size.Height * 5 / 6 - Form1.form1.Size.Height / 12),
                                   /*velX*/ 0, /*velY*/ 16, /*life*/ 38,
                                   /*cNum*/ 2, /*cRange*/ 200,
                                   ParticleSystem.CreateFunction.Gaussian,
                                   Particle.Type.leaf, () => Music.chord.ChordColor(),
                                   /*pSize*/ 0.7f, /*pLife*/ 40),
                new Theme.ParticleInfo(Particle.Type.rain, (Form1.form1.Size.Height + 150) / 4, (pitch) => Color.White, 0.2f),
                "Pianoforte"));
            
            _availableThemes.Add(new Theme(
                "Sky", "구름 너머",
                new ParticleSystem(/*cNum*/ 1, /*cRange*/ 0,
                                   ParticleSystem.CreateFunction.Random,
                                   Particle.Type.star, () => Color.Black,
                                   /*pSize*/ 1f, /*pLife*/ 64),
                new ParticleSystem(/*posX*/ () => (float)(new Random().NextDouble() * Form1.form1.Size.Width),
                                   /*posY*/ () => (float)(new Random().NextDouble() * Form1.form1.Size.Height * 5 / 6 - Form1.form1.Size.Height / 12),
                                   /*velX*/ 2, /*velY*/ 8, /*life*/ 38,
                                   /*cNum*/ 7, /*cRange*/ 4,
                                   ParticleSystem.CreateFunction.Gaussian,
                                   Particle.Type.dot, () => Music.chord.ChordColor(),
                                   /*pSize*/ 1, /*pLife*/ 10),
                new Theme.ParticleInfo(Particle.Type.star, 32, (pitch) => Chord.PitchColor(pitch), 1f),
                "Sky"));
            
            /*
            _availableThemes.Add(new Theme(
                "Medieval", "중세 탐방",
                new ParticleSystem(1, 0,
                                   ParticleSystem.CreateFunction.TopRandom,
                                   Particle.Type.rain, () => Color.White,
                                   0.1f, (Form1.form1.Size.Height + 150) / 30),
                new ParticleSystem(() => 0,
                                   () => 0,
                                   0, 0, 160,
                                   1, 0,
                                   ParticleSystem.CreateFunction.TopRandom,
                                   Particle.Type.rain, () => Color.White,
                                   0.1f, (Form1.form1.Size.Height + 150) / 30),
                new Theme.ParticleInfo(Particle.Type.note, (Form1.form1.Size.Height + 150) / 15, (pitch) => Chord.PitchColor(pitch), 0.1f),
                "Medieval"));
            */

            _availableThemes.Add(new Theme(
                "Medieval ruins", "중세 유적지",
                new ParticleSystem(1, 0,
                                   ParticleSystem.CreateFunction.TopRandom,
                                   Particle.Type.rain, () => Color.White,
                                   0.1f, (Form1.form1.Size.Height + 150) / 30),
                new ParticleSystem(() => 0,
                                   () => 0,
                                   0, 0, 160,
                                   1, 0,
                                   ParticleSystem.CreateFunction.TopRandom,
                                   Particle.Type.rain, () => Color.White,
                                   0.1f, (Form1.form1.Size.Height + 150) / 30),
                new Theme.ParticleInfo(Particle.Type.note, (Form1.form1.Size.Height + 150) / 15, (pitch) => Chord.PitchColor(pitch), 0.1f),
                "Medieval ruins"));

            IsReady = true;
        }

        /// <summary>
        /// AvailableThemes에서 name이 일치하는 테마를 찾아 반환합니다.
        /// 만약 찾지 못하면, Name이 null인 Theme이 반환됩니다.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Theme FindTheme(string name)
        {
            foreach (Theme t in AvailableThemes)
            {
                if (name != null && name.Equals(t.Name))
                {
                    return t;
                }
            }
            return new Theme(null, null, null, null, new ParticleInfo(), null);
        }

        /// <summary>
        /// AvailableThemes에 있는 모든 Theme들의 이름을 반환합니다.
        /// </summary>
        /// <returns></returns>
        public static List<string> GetAllThemeName()
        {
            List<string> list = new List<string>();
            foreach (Theme t in AvailableThemes)
            {
                if (t.Name != null)
                {
                    list.Add(t.Name);
                }
            }
            return list;
        }
    }
}
