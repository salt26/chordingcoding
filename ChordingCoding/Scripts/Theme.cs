using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace ChordingCoding
{
    [System.Serializable]
    public class Theme
    {
        public enum ChordTransition { SomewhatHappy, SomewhatBlue, SimilarOne };    // TODO 이것도 클래스화하여 List로 보관
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
        public struct InstrumentInfo
        {
            public delegate int PitchModulator(int pitch);

            public int instrumentCode;
            public PitchModulator characterPitchModulator;
            public int characterRhythm;
            public int characterVolume;
            public PitchModulator whitespacePitchModulator;
            public int whitespaceRhythm;
            public int whitespaceVolume;

            public InstrumentInfo(int code, PitchModulator cPitchModulator, int cRhythm, int cVolume,
                PitchModulator wPitchModulator = null, int wRhythm = 0, int wVolume = 0)
            {
                instrumentCode = code;
                characterPitchModulator = cPitchModulator;
                characterRhythm = cRhythm;
                characterVolume = cVolume;
                whitespacePitchModulator = wPitchModulator;
                whitespaceRhythm = wRhythm;
                whitespaceVolume = wVolume;
            }
        }
        /* InstrumentSet
         * 일반 문자 입력 시 사용하는 악기 채널은 항상 0, 1 (character 타입)
         * 공백 문자 입력 시 사용하는 악기 채널은 항상 0, 1, 2 (whitespace 타입)
         * 특정 테마일 때 항상 들리는 효과음의 악기 채널은 항상 3, 4 (whitespace 타입)
         * 생성되는 반주의 악기 채널은 항상 5, 6 (accompaniment 타입)
         */
        public struct InstrumentSet
        {
            public enum Type { character, whitespace, accompaniment };

            public string name;
            public string displayName;
            public Type type;
            public Dictionary<int, InstrumentInfo> instruments; // Key는 악기가 사용될 채널(staff), Value는 악기 정보

            public InstrumentSet(string name, string displayName, Type type, Dictionary<int, InstrumentInfo> instruments)
            {
                this.name = name;
                this.displayName = displayName;
                this.type = type;
                this.instruments = instruments;
            }
        }

        public static List<InstrumentSet> availableInstrumentSets = new List<InstrumentSet>();
        public static bool isReady = false;

        /* 
         * Theme이 가질 수 있는 속성들입니다.
         */
        #region Theme Attributes

        public string name;                                 // 테마의 영어 이름
        public string displayName;                          // 테마의 한글(보여질) 이름
        public ParticleSystem basicParticleSystem;          // 기본 파티클 시스템
        public ParticleSystem particleSystemForWhitespace;  // 공백 문자를 입력할 때 추가로 생성될 파티클 시스템
        public ParticleInfo particleInfoForCharacter;       // 일반 문자를 입력할 때 기본 파티클 시스템 안에 생성할 파티클의 정보

        public ChordTransition chordTransition;             // 화음 전이 확률 분포

        public InstrumentSet instrumentSetForCharacter;     // 일반 문자 입력 시 사용될 악기 세트
        public InstrumentSet instrumentSetForWhitespace;    // 공백 문자 입력 시 추가로 사용되거나 특정 테마의 효과음으로 사용될 악기 세트
        public InstrumentSet instrumentSetForAccompaniment; // 반주 생성에 사용될 악기 세트

        #endregion

        /// <summary>
        /// 새 테마를 생성합니다.
        /// Theme.Initialize()가 호출된 적이 없으면 name이 null인 테마가 생성됩니다.
        /// </summary>
        /// <param name="name">내부적으로 사용될 이름</param>
        /// <param name="displayName">표시할 이름</param>
        /// <param name="basicPS">기본 파티클 시스템</param>
        /// <param name="PSForWhitespace">공백 문자 입력 시 생성될 파티클 시스템</param>
        /// <param name="PIForCharacter">일반 문자 입력 시 기본 파티클 시스템 안에 생성될 파티클 정보</param>
        /// <param name="transition">공백 문자 입력 시의 화음 전이 확률 분포</param>
        /// <param name="instrumentSetNameForCharacter">일반 문자 입력 시 사용될 악기 세트 이름</param>
        /// <param name="instrumentSetNameForWhitespace">공백 문자 입력 시와 특정 테마 효과음으로 사용될 악기 세트 이름</param>
        /// <param name="instrumentSetNameForAccompaniment">반주 생성에 사용될 악기 세트 이름</param>
        public Theme(string name, string displayName, ParticleSystem basicPS, ParticleSystem PSForWhitespace, ParticleInfo PIForCharacter,
            ChordTransition transition, string instrumentSetNameForCharacter, string instrumentSetNameForWhitespace, string instrumentSetNameForAccompaniment)
        {
            if (!isReady)
            {
                this.name = null;
                return;
            }

            this.name = name;
            this.displayName = displayName;
            basicParticleSystem = basicPS;
            particleSystemForWhitespace = PSForWhitespace;
            particleInfoForCharacter = PIForCharacter;
            chordTransition = transition;
            instrumentSetForCharacter = FindInstrumentSet(instrumentSetNameForCharacter, InstrumentSet.Type.character);
            instrumentSetForWhitespace = FindInstrumentSet(instrumentSetNameForWhitespace, InstrumentSet.Type.whitespace);
            instrumentSetForAccompaniment = FindInstrumentSet(instrumentSetNameForAccompaniment, InstrumentSet.Type.accompaniment);
        }

        /// <summary>
        /// 사용할 수 있는 악기 세트를 초기화합니다. 새 테마를 초기화하는 어떤 코드보다도 먼저 호출되어야 합니다.
        /// </summary>
        public static void Initialize()
        {
            if (isReady) return;

            /* 악기가 적용될 채널(staff)에 따른, 새 InsturmentInfo 만드는 법
             * 채널 0 또는 1 : new InstrumentInfo(악기 번호, 일반 문자 음 높이 변경 함수, 일반 문자 음 길이, 일반 문자 음량, 공백 문자 음 높이 변경 함수, 공백 문자 음 길이, 공백 문자 음량)
             * 채널 2        : new InstrumentInfo(악기 번호, null, 0, 0, 공백 문자 음 높이 변경 함수, 공백 문자 음 길이, 공백 문자 음량)
             * 채널 3 또는 4 : new InstrumentInfo(악기 번호, 항상 들리는 효과음 높이 변경 함수, 1, 항상 들리는 효과음 음량)
             */
            // availableInstruments에 새 악기 정보를 추가할 때에는 맨 뒤에 추가바람. (순서가 중요!)
            List<InstrumentInfo> availableInstruments = new List<InstrumentInfo>();
            availableInstruments.Add(new InstrumentInfo(32, (pitch) => pitch, 16, 127, (pitch) => pitch, 16, 64));                      // [0] autumn channel 0, Acoustic bass
            availableInstruments.Add(new InstrumentInfo(24, (pitch) => pitch, 16, 127, (pitch) => pitch, 16, 127));                     // [1] autumn channel 1, Acoustic guitar(nylon)
            availableInstruments.Add(new InstrumentInfo(123, null, 0, 0, (pitch) => pitch % 12 + 54, 4, 54));                           // [2] autumn channel 2, Bird tweet
            availableInstruments.Add(new InstrumentInfo(101, (pitch) => pitch - 12, 14, 96, (pitch) => (pitch + 6) % 12 + 54, 14, 96)); // [3] rain channel 0, SFX(goblin)
            availableInstruments.Add(new InstrumentInfo(12, (pitch) => pitch, 16, 127, (pitch) => pitch, 16, 127));                     // [4] rain channel 1, Marimba
            availableInstruments.Add(new InstrumentInfo(126, null, 0, 0, (pitch) => (pitch + 5) % 7 + 46, 64, 24));                     // [5] rain channel 2, Applause
            availableInstruments.Add(new InstrumentInfo(126, (pitch) => 45, 1, 24));                                                    // [6] rain channel 3/4, Applause
            availableInstruments.Add(new InstrumentInfo(49, (pitch) => pitch - 12, 16, 72, (pitch) => (pitch + 6) % 12 + 54, 16, 72));  // [7] star channel 0, String ensemble 2
            availableInstruments.Add(new InstrumentInfo(11, (pitch) => pitch, 16, 127, (pitch) => pitch, 16, 127));                     // [8] star channel 1, Vibraphone

            Dictionary<int, InstrumentInfo> instruments;

            instruments = new Dictionary<int, InstrumentInfo>();
            instruments.Add(0, availableInstruments[0]);
            instruments.Add(1, availableInstruments[1]);
            availableInstrumentSets.Add(new InstrumentSet("Guitar", "기타", InstrumentSet.Type.character, instruments));

            instruments = new Dictionary<int, InstrumentInfo>();
            instruments.Add(0, availableInstruments[3]);
            instruments.Add(1, availableInstruments[4]);
            availableInstrumentSets.Add(new InstrumentSet("Forest", "숲", InstrumentSet.Type.character, instruments));
            
            instruments = new Dictionary<int, InstrumentInfo>();
            instruments.Add(0, availableInstruments[7]);
            instruments.Add(1, availableInstruments[8]);
            availableInstrumentSets.Add(new InstrumentSet("Star", "별", InstrumentSet.Type.character, instruments));
            
            instruments = new Dictionary<int, InstrumentInfo>();
            instruments.Add(2, availableInstruments[2]);
            availableInstrumentSets.Add(new InstrumentSet("Bird", "새", InstrumentSet.Type.whitespace, instruments));

            instruments = new Dictionary<int, InstrumentInfo>();
            instruments.Add(2, availableInstruments[5]);
            instruments.Add(3, availableInstruments[6]);
            instruments.Add(4, availableInstruments[6]);
            availableInstrumentSets.Add(new InstrumentSet("Rain", "비", InstrumentSet.Type.whitespace, instruments));

            isReady = true;
        }

        private InstrumentSet FindInstrumentSet(string name, InstrumentSet.Type type)
        {
            foreach (InstrumentSet i in availableInstrumentSets)
            {
                if (i.name.Equals(name) && i.type.Equals(type))
                {
                    return i;
                }
            }
            return new InstrumentSet(null, null, InstrumentSet.Type.character, new Dictionary<int, InstrumentInfo>());
        }
    }
}
