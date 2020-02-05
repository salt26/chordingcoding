using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Sanford.Multimedia.Midi;

namespace ChordingCoding.SFX
{
    /// <summary>
    /// 음악 테마 클래스입니다. 악기들과 화음 전이 확률 분포, 음량을 가지고 있습니다.
    /// </summary>
    public class SFXTheme
    {
        public enum ChordTransitionType { SomewhatHappy, SomewhatBlue, SimilarOne };    // TODO 이것도 클래스화하여 List로 보관

        /*
         * SFXTheme 객체가 가질 수 있는 속성들입니다.
         */
        #region SFXTheme attributes

        /// <summary>
        /// 테마의 영어 이름
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 테마의 한글(보여질) 이름
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// 화음 전이 확률 분포
        /// </summary>
        public ChordTransitionType ChordTransition { get; }
        
        /// <summary>
        /// 악기들. Key는 악기가 사용될 채널(staff), Value는 악기 정보
        /// </summary>
        public Dictionary<int, InstrumentInfo> Instruments { get; }

        /// <summary>
        /// 이 악기 테마를 구성하는 악기 세트들의 이름
        /// </summary>
        public Dictionary<InstrumentSet.Type, string> InstrumentSetNames { get; }

        private int _volume;

        /// <summary>
        /// 테마의 음량 (0 이상 100 이하, 변경 가능)
        /// </summary>
        public int Volume
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

        private bool _hasAccompanied;

        /// <summary>
        /// 자동 반주 여부 (변경 가능)
        /// </summary>
        public bool hasAccompanied
        {
            get
            {
                return _hasAccompanied;
            }
            set
            {
                _hasAccompanied = value;
                /*
                if (_hasAccompanied)
                {
                    void StartAccompaniment(object[] args)
                    {
                        Music.ResetAccompaniment();
                    }
                    Util.TaskQueue.Add("play", StartAccompaniment);
                }
                */
            }
        }

        /// <summary>
        /// 테마의 채널 1에 사용되는 악기가 낼 수 있는 음의 최소 옥타브
        /// </summary>
        public int MinOctave { get; private set; }

        /// <summary>
        /// 테마의 채널 1에 사용되는 악기가 낼 수 있는 음의 최대 옥타브
        /// </summary>
        public int MaxOctave { get; private set; }

        /// <summary>
        /// 테마의 채널 7에 사용되는 악기가 낼 수 있는 음의 최소 옥타브
        /// </summary>
        public int MinOctaveInAccompaniment { get; private set; }

        /// <summary>
        /// 테마의 채널 7에 사용되는 악기가 낼 수 있는 음의 최소 옥타브
        /// </summary>
        public int MaxOctaveInAccompaniment { get; private set; }

        #endregion

        /*
         * SFXTheme의 정적 필드들입니다.
         */
        #region SFXTheme static fields
        
        /// <summary>
        /// 사용 가능한 모든 SFXTheme들의 목록
        /// </summary>
        public static List<SFXTheme> availableSFXThemes = new List<SFXTheme>();

        /// <summary>
        /// 새 SFXTheme을 만들 때 사용할 수 있는 모든 악기 세트들의 목록
        /// </summary>
        public static List<InstrumentSet> availableInstrumentSets = new List<InstrumentSet>();

        public static bool IsReady { get; private set; } = false;

        private static bool isInstrumentsReady = false;

        private static SFXTheme _theme;

        public static SFXTheme CurrentSFXTheme
        {
            get
            {
                return _theme;
            }
            set
            {
                _theme = value;

                if (Music.IsReady)
                {
                    Music.ThemeChanged();
                }
            }
        }

        #endregion


        /*
         * InstrumentInfo 정의
         */
        #region InstrumentInfo 정의

        public abstract class InstrumentInfo
        {
            public delegate int PitchModulator(int pitch);

            public int instrumentCode;
            public PitchModulator characterPitchModulator;
            public int characterRhythm;
            public int characterVolume;
            public PitchModulator whitespacePitchModulator;
            public int whitespaceRhythm;
            public int whitespaceVolume;
            public PitchModulator sfxPitchModulator;
            public int sfxRhythm;
            public int sfxVolume;
            public PitchModulator accompanimentPitchModulator;
            public int accompanimentRhythm;
            public int accompanimentVolume;
        }

        /// <summary>
        /// 일반 문자 또는 공백 문자를 입력할 때 재생되고 반주로도 재생될, 채널(staff) 0, 1, 2의 악기 정보입니다.
        /// </summary>
        public class CharacterInstrumentInfo : InstrumentInfo
        {
            /// <param name="code">MIDI 악기 번호</param>
            /// <param name="cPitchModulator">일반 문자 음 높이 변경 함수</param>
            /// <param name="cRhythm">일반 문자 음 길이 (64분음표: 1, 온음표: 64)</param>
            /// <param name="cVolume">일반 문자 음량 (0 ~ 127)</param>
            /// <param name="wPitchModulator">공백 문자 음 높이 변경 함수</param>
            /// <param name="wRhythm">공백 문자 음 길이 (64분음표: 1, 온음표: 64)</param>
            /// <param name="wVolume">공백 문자 음량 (0 ~ 127)</param>
            /// <param name="aPitchModulator">반주 음 높이 변경 함수</param>
            /// <param name="aRhythm">반주 음 길이 (64분음표: 1, 온음표: 64)</param>
            /// <param name="aVolume">반주 음량 (0 ~ 127)</param>
            public CharacterInstrumentInfo(int code,
                PitchModulator cPitchModulator, int cRhythm, int cVolume,
                PitchModulator wPitchModulator, int wRhythm, int wVolume,
                PitchModulator aPitchModulator = null, int aRhythm = 0, int aVolume = 0)
            {
                if (cPitchModulator == null) cPitchModulator = (pitch) => pitch;
                if (cRhythm < 0) cRhythm = 0;
                if (cVolume < 0) cVolume = 0;
                if (cVolume > 127) cVolume = 127;
                if (wPitchModulator == null) wPitchModulator = (pitch) => pitch;
                if (wRhythm < 0) wRhythm = 0;
                if (wVolume < 0) wVolume = 0;
                if (wVolume > 127) wVolume = 127;
                if (aPitchModulator == null) aPitchModulator = (pitch) => pitch;
                if (aRhythm < 0) aRhythm = 0;
                if (aVolume < 0) aVolume = 0;
                if (aVolume > 127) aVolume = 127;

                instrumentCode = code;
                characterPitchModulator = cPitchModulator;
                characterRhythm = cRhythm;
                characterVolume = cVolume;
                whitespacePitchModulator = wPitchModulator;
                whitespaceRhythm = wRhythm;
                whitespaceVolume = wVolume;
                sfxPitchModulator = (pitch) => pitch;
                sfxRhythm = 0;
                sfxVolume = 0;
                accompanimentPitchModulator = (pitch) => pitch;
                accompanimentRhythm = 0;
                accompanimentVolume = 0;
            }
        }

        /// <summary>
        /// 공백 문자를 입력할 때 재생될, 채널(staff) 3, 4의 악기 정보입니다.
        /// </summary>
        public class WhitespaceInstrumentInfo : InstrumentInfo
        {
            /// <param name="code">MIDI 악기 번호</param>
            /// <param name="wPitchModulator">공백 문자 음 높이 변경 함수</param>
            /// <param name="wRhythm">공백 문자 음 길이 (64분음표: 1, 온음표: 64)</param>
            /// <param name="wVolume">공백 문자 음량 (0 ~ 127)</param>
            public WhitespaceInstrumentInfo(int code, PitchModulator wPitchModulator, int wRhythm, int wVolume)
            {
                if (wPitchModulator == null) wPitchModulator = (pitch) => pitch;
                if (wRhythm < 0) wRhythm = 0;
                if (wVolume < 0) wVolume = 0;
                if (wVolume > 127) wVolume = 127;

                instrumentCode = code;
                characterPitchModulator = (pitch) => pitch;
                characterRhythm = 0;
                characterVolume = 0;
                whitespacePitchModulator = wPitchModulator;
                whitespaceRhythm = wRhythm;
                whitespaceVolume = wVolume;
                sfxPitchModulator = (pitch) => pitch;
                sfxRhythm = 0;
                sfxVolume = 0;
                accompanimentPitchModulator = (pitch) => pitch;
                accompanimentRhythm = 0;
                accompanimentVolume = 0;
            }
        }

        /// <summary>
        /// 항상 재생될 효과음인, 채널(staff) 5, 6의 악기 정보입니다.
        /// </summary>
        public class SFXInstrumentInfo : InstrumentInfo
        {
            /// <param name="code">MIDI 악기 번호</param>
            /// <param name="sPitchModulator">효과음 높이 변경 함수</param>
            /// <param name="sRhythm">효과음 길이 (64분음표: 1, 온음표: 64)</param>
            /// <param name="sVolume">효과음 음량 (0 ~ 127)</param>
            public SFXInstrumentInfo(int code, PitchModulator sPitchModulator, int sRhythm, int sVolume)
            {
                if (sPitchModulator == null) sPitchModulator = (pitch) => pitch;
                if (sRhythm < 0) sRhythm = 0;
                if (sVolume < 0) sVolume = 0;
                if (sVolume > 127) sVolume = 127;

                instrumentCode = code;
                characterPitchModulator = (pitch) => pitch;
                characterRhythm = 0;
                characterVolume = 0;
                whitespacePitchModulator = (pitch) => pitch;
                whitespaceRhythm = 0;
                whitespaceVolume = 0;
                sfxPitchModulator = sPitchModulator;
                sfxRhythm = sRhythm;
                sfxVolume = sVolume;
                accompanimentPitchModulator = (pitch) => pitch;
                accompanimentRhythm = 0;
                accompanimentVolume = 0;
            }
        }

        /// <summary>
        /// 반주로 재생될, 채널(staff) 7, 8의 악기 정보입니다.
        /// </summary>
        public class AccompanimentInstrumentInfo : InstrumentInfo
        {
            /// <param name="code">MIDI 악기 번호</param>
            /// <param name="aPitchModulator">반주 음 높이 변경 함수</param>
            /// <param name="aRhythm">반주 음 길이 (64분음표: 1, 온음표: 64)</param>
            /// <param name="aVolume">반주 음량 (0 ~ 127)</param>
            public AccompanimentInstrumentInfo(int code, PitchModulator aPitchModulator, int aRhythm, int aVolume)
            {
                if (aPitchModulator == null) aPitchModulator = (pitch) => pitch;
                if (aRhythm < 0) aRhythm = 0;
                if (aVolume < 0) aVolume = 0;
                if (aVolume > 127) aVolume = 127;

                instrumentCode = code;
                characterPitchModulator = (pitch) => pitch;
                characterRhythm = 0;
                characterVolume = 0;
                whitespacePitchModulator = (pitch) => pitch;
                whitespaceRhythm = 0;
                whitespaceVolume = 0;
                sfxPitchModulator = (pitch) => pitch;
                sfxRhythm = 0;
                sfxVolume = 0;
                accompanimentPitchModulator = aPitchModulator;
                accompanimentRhythm = aRhythm;
                accompanimentVolume = aVolume;
            }
        }
        #endregion

        /* InstrumentSet
         * 일반 문자 입력 시 사용하는 악기 채널은 항상 0, 1, 2 (type == Type.character, CharacterInstrumentInfo)
         * 공백 문자 입력 시 추가로 사용하는 악기 채널은 항상 3, 4 (type == Type.whitespace, WhitespaceInstrumentInfo)
         * 특정 테마일 때 항상 들리는 효과음의 악기 채널은 항상 5, 6 (type = Type.whitespace, SFXInstrumentInfo)
         * 생성되는 반주의 악기 채널은 항상 7, 8 (type = Type.accompaniment, AccompanimentInstrumentInfo)
         */
        public struct InstrumentSet
        {
            public enum Type { general, character, whitespace, accompaniment };

            /// <summary>
            /// 악기 세트의 영어 이름
            /// </summary>
            public string name;

            /// <summary>
            /// 악기 세트의 한글(보여질) 이름
            /// </summary>
            public string displayName;

            /// <summary>
            /// 악기 세트 종류. 여러 종류의 악기를 담는 악기 세트의 type은 Type.general
            /// </summary>
            public Type type;

            /// <summary>
            /// 악기가 연주할 수 있는 음의 최소 옥타브 (type이 character 또는 accompaniment일 때에만 유효, 0 ~ 9 사이의 값)
            /// </summary>
            public int minOctave;

            /// <summary>
            /// 악기가 연주할 수 있는 음의 최대 옥타브 (type이 character 또는 accompaniment일 때에만 유효, 0 ~ 9 사이의 값)
            /// </summary>
            public int maxOctave;

            /// <summary>
            /// 악기들. Key는 악기가 사용될 채널(staff), Value는 악기 정보
            /// </summary>
            public Dictionary<int, InstrumentInfo> instruments;

            public InstrumentSet(string name, string displayName, Dictionary<int, InstrumentInfo> instruments, Type type = Type.general, int minOctave = -1, int maxOctave = -1)
            {
                this.name = name;
                this.displayName = displayName;
                this.instruments = instruments;
                this.type = type;
                if (type == Type.character || type == Type.accompaniment)
                {
                    if (minOctave < 0) minOctave = 0;
                    if (minOctave > 9) minOctave = 9;
                    if (maxOctave > 9) maxOctave = 9;
                    if (maxOctave < minOctave) maxOctave = minOctave;
                    this.minOctave = minOctave;
                    this.maxOctave = maxOctave;
                }
                else
                {
                    this.minOctave = -1;
                    this.maxOctave = -1;
                }
            }
        }


        /// <summary>
        /// 새 음악 테마를 생성합니다.
        /// SFXTheme.Initialize()가 호출된 적이 없으면 Name이 null이고 Instruments가 빈 테마가 생성됩니다.
        /// </summary>
        /// <param name="name">내부적으로 사용될 이름</param>
        /// <param name="displayName">표시할 이름</param>
        /// <param name="transition">공백 문자 입력 시의 화음 전이 확률 분포</param>
        /// <param name="instrumentSetNameForCharacter">일반 문자 입력 시에 사용될, Theme.availableInstrumentSet 안의 악기 세트 이름</param>
        /// <param name="instrumentSetNameForWhitespace">공백 문자 입력 시와 지속 효과음 재생 시에 사용될, Theme.availableInstrumentSet 안의 악기 세트 이름 (nullable)</param>
        /// <param name="instrumentSetNameForAccompaniment">반주 생성에 사용될, Theme.availableInstrumentSet 안의 악기 세트 이름 (nullable)</param>
        public SFXTheme(string name, string displayName, ChordTransitionType transition,
            string instrumentSetNameForCharacter, string instrumentSetNameForWhitespace, string instrumentSetNameForAccompaniment)
        {
            if (!isInstrumentsReady || name == null)
            {
                this.Name = null;
                Instruments = new Dictionary<int, InstrumentInfo>();
                return;
            }

            this.Name = name;
            this.DisplayName = displayName;
            ChordTransition = transition;
            Instruments = new Dictionary<int, InstrumentInfo>();
            ConcatenateInstrumentSet(Instruments, FindInstrumentSet(instrumentSetNameForCharacter, InstrumentSet.Type.character));
            ConcatenateInstrumentSet(Instruments, FindInstrumentSet(instrumentSetNameForWhitespace, InstrumentSet.Type.whitespace));
            ConcatenateInstrumentSet(Instruments, FindInstrumentSet(instrumentSetNameForAccompaniment, InstrumentSet.Type.accompaniment));
            InstrumentSetNames = new Dictionary<InstrumentSet.Type, string>();
            InstrumentSetNames.Add(InstrumentSet.Type.character, instrumentSetNameForCharacter);
            InstrumentSetNames.Add(InstrumentSet.Type.whitespace, instrumentSetNameForWhitespace);
            InstrumentSetNames.Add(InstrumentSet.Type.accompaniment, instrumentSetNameForAccompaniment);

            //Console.WriteLine("minOctave " + MinOctave + ", maxOctave " + MaxOctave);
            Volume = 100;
        }

        /// <summary>
        /// 사용할 수 있는 악기 세트를 초기화하고 기본 음악 테마를 생성합니다. 새 음악 테마를 생성하는 어떤 코드보다도 먼저 호출되어야 합니다.
        /// </summary>
        public static void Initialize()
        {
            if (IsReady) return;

            /* [악기가 적용될 채널(staff)에 따른, 새 InsturmentInfo 만드는 법]
             * 채널 0 또는 1 또는 2 : CharacterInstrumentInfo
             * 채널 3 또는 4        : WhitespaceInstrumentInfo
             * 채널 5 또는 6        : SFXInstrumentInfo
             * 채널 7 또는 8        : AccompanimentInstrumentInfo
             *
             * [PitchModulator와 옥타브 범위와의 관계]
             * channel 1로 쓰일 악기는 항상 PitchModulator가 `(pitch) => pitch`이어야 합니다.
             * 대신 minOctave와 maxOctave를 Type이 character인 InstrumentSet을 만들 때 지정합니다.
             * 그리고 이 옥타브 범위를 그대로 따릅니다.
             * 
             * channel 1 외의 다른 악기에서 나오는 pitch는 기본적으로
             * Type이 character인 InstrumentSet에서 지정한 minOctave와 maxOctave 사이의 임의의 음입니다.
             * 즉, channel 1로 함께 쓰인 악기의 옥타브 범위에 영향을 받습니다.
             * 이들의 음 옥타브 범위를 따로 지정하려면 PitchModulator를 활용해야 합니다.
             * 
             * [악기 번호(code)]
             * -1인 경우 현재 테마의 channel 1에서 사용 중인 악기의 종류를 참조하여 그대로 따른다는 뜻입니다.
             * 하지만 악기 번호가 -1이어도 음량(volume)은 channel 1을 따르지 않으므로 자유롭게 설정 가능합니다.
             * 악기 번호 -1은 channel 7과 channel 8에서만 유효합니다. 다른 채널에서는 사용하지 마십시오.
             */
            // availableInstruments에 새 악기 정보를 추가할 때에는 맨 뒤에 추가바람. (순서가 중요!)
            List<InstrumentInfo> availableInstruments = new List<InstrumentInfo>();
            availableInstruments.Add(new CharacterInstrumentInfo(32, (pitch) => pitch, 64, 127, (pitch) => pitch, 64, 96));                         // [0] Autumn channel 0, Acoustic bass
            availableInstruments.Add(new CharacterInstrumentInfo(24, (pitch) => pitch, 64, 127, (pitch) => pitch, 64, 127));                        // [1] Autumn channel 1, Acoustic guitar(nylon)
            availableInstruments.Add(new WhitespaceInstrumentInfo(123, (pitch) => pitch % 12 + 54, 16, 0));                                         // [2] Autumn channel 3, Bird tweet => Disabled
            availableInstruments.Add(new CharacterInstrumentInfo(101, (pitch) => pitch - 12, 56, 80, (pitch) => (pitch + 6) % 12 + 54, 56, 80));    // [3] Rain channel 0, SFX(goblin)
            availableInstruments.Add(new CharacterInstrumentInfo(12, (pitch) => pitch, 64, 127, (pitch) => pitch, 64, 127));                        // [4] Rain channel 1, Marimba
            availableInstruments.Add(new WhitespaceInstrumentInfo(126, (pitch) => (pitch + 5) % 7 + 46, 256, 24));                                  // [5] Rain channel 3, Applause
            availableInstruments.Add(new SFXInstrumentInfo(126, (pitch) => 45, 4, 24));                                                             // [6] Rain channel 5/6, Applause
            availableInstruments.Add(new CharacterInstrumentInfo(49, (pitch) => pitch - 12, 64, 60, (pitch) => (pitch + 6) % 12 + 54, 64, 48));     // [7] Star channel 0, String ensemble 2
            availableInstruments.Add(new CharacterInstrumentInfo(11, (pitch) => pitch, 64, 127, (pitch) => pitch, 64, 127));                        // [8] Star channel 1, Vibraphone
            availableInstruments.Add(new CharacterInstrumentInfo(0, (pitch) => pitch % 24 + 36, 128, 80, (pitch) => pitch % 24 + 24, 128, 80));     // [9] Pianoforte channel 0, Acoustic grand piano
            availableInstruments.Add(new CharacterInstrumentInfo(0, (pitch) => pitch, 128, 127, (pitch) => pitch, 128, 127));                       // [10] Pianoforte channel 1, Acoustic grand piano
            availableInstruments.Add(new CharacterInstrumentInfo(46, (pitch) => pitch, 64, 127, (pitch) => pitch, 64, 127));                        // [11] Sky channel 1, Orchestral harp
            availableInstruments.Add(new CharacterInstrumentInfo(68, (pitch) => pitch, 64, 96, (pitch) => pitch, 64, 96));                          // [12] ??? channel 1, Oboe
            availableInstruments.Add(new CharacterInstrumentInfo(19, (pitch) => pitch, 48, 96, (pitch) => pitch, 48, 96));                          // [13] Medieval channel 1, Church organ
            availableInstruments.Add(new WhitespaceInstrumentInfo(9, (pitch) => pitch % 12 + 72, 256, 80));                                         // [14] Medieval channel 3, Glockenspiel
            availableInstruments.Add(new WhitespaceInstrumentInfo(9, (pitch) => (pitch + 7) % 12 + 48, 256, 80));                                   // [15] Medieval channel 4, Glockenspiel
            availableInstruments.Add(new AccompanimentInstrumentInfo(0, (pitch) => pitch % 24 + 24, 128, 100));                                     // [16] Pianoforte channel 7, Acoustic grand piano
            availableInstruments.Add(new AccompanimentInstrumentInfo(0, (pitch) => pitch, 128, 80));                                                // [17] Pianoforte channel 8, Acoustic grand piano
            availableInstruments.Add(new AccompanimentInstrumentInfo(11, (pitch) => pitch % 24 + 36, 128, 60));                                     // [18] ??? channel 7, Vibraphone
            availableInstruments.Add(new AccompanimentInstrumentInfo(12, (pitch) => pitch, 128, 100));                                              // [19] ??? channel 8, Marimba
            availableInstruments.Add(new AccompanimentInstrumentInfo(-1, (pitch) => pitch, 0, 48));                                                 // [20] channel 7 또는 8에서 1번 악기를 참조하여 재생할 때 사용
            availableInstruments.Add(new AccompanimentInstrumentInfo(-1, (pitch) => pitch, 0, 96));                                                 // [21] channel 7 또는 8에서 1번 악기를 참조하여 재생할 때 사용
            availableInstruments.Add(new CharacterInstrumentInfo(101, (pitch) => pitch - 12, 56, 0, (pitch) => (pitch + 6) % 12 + 66, 56, 72));     // [22] Medieval channel 0, SFX(goblin)
            availableInstruments.Add(new CharacterInstrumentInfo(19, (pitch) => (pitch + 8) % 12 + 40, 64, 60, (pitch) => (pitch) % 12 + 48, 64, 72));          // [23] Choir channel 0, Church organ
            availableInstruments.Add(new CharacterInstrumentInfo(52, (pitch) => (pitch + 19) % 24 + 53, 48, 127, (pitch) => (pitch + 19) % 24 + 53, 48, 0));    // [24] Choir channel 1, Choir aahs (Alto)
            availableInstruments.Add(new CharacterInstrumentInfo(52, (pitch) => (pitch + 5) % 24 + 43, 48, 127, (pitch) => (pitch + 5) % 24 + 43, 48, 0));      // [25] Choir channel 2, Choir aahs (Bariton)
            availableInstruments.Add(new AccompanimentInstrumentInfo(47, (pitch) => ((pitch + 6) % 12) + 18, 64, 100));                                         // [26] Choir channel 7, Timpani

            Dictionary<int, InstrumentInfo> instruments;

            /* 
             * InstrumentSet.Type.character (minOctave와 maxOctave 지정)
             */
            instruments = new Dictionary<int, InstrumentInfo>();
            instruments.Add(0, availableInstruments[0]);
            instruments.Add(1, availableInstruments[1]);
            availableInstrumentSets.Add(new InstrumentSet("Guitar", "기타", instruments, InstrumentSet.Type.character, 4, 5));

            instruments = new Dictionary<int, InstrumentInfo>();
            instruments.Add(0, availableInstruments[3]);
            instruments.Add(1, availableInstruments[4]);
            availableInstrumentSets.Add(new InstrumentSet("Forest", "숲", instruments, InstrumentSet.Type.character, 5, 8));

            instruments = new Dictionary<int, InstrumentInfo>();
            instruments.Add(0, availableInstruments[7]);
            instruments.Add(1, availableInstruments[8]);
            availableInstrumentSets.Add(new InstrumentSet("Star", "별", instruments, InstrumentSet.Type.character, 5, 7));

            instruments = new Dictionary<int, InstrumentInfo>();
            instruments.Add(0, availableInstruments[9]);
            instruments.Add(1, availableInstruments[10]);
            availableInstrumentSets.Add(new InstrumentSet("Piano", "피아노", instruments, InstrumentSet.Type.character, 3, 7));

            instruments = new Dictionary<int, InstrumentInfo>();
            instruments.Add(0, availableInstruments[9]);
            instruments.Add(1, availableInstruments[10]);
            availableInstrumentSets.Add(new InstrumentSet("Piano_low", "피아노 저음", instruments, InstrumentSet.Type.character, 2, 6));
            
            instruments = new Dictionary<int, InstrumentInfo>();
            instruments.Add(0, availableInstruments[9]);
            instruments.Add(1, availableInstruments[10]);
            availableInstrumentSets.Add(new InstrumentSet("Piano_high", "피아노 고음", instruments, InstrumentSet.Type.character, 5, 8));

            instruments = new Dictionary<int, InstrumentInfo>();
            instruments.Add(0, availableInstruments[7]);
            instruments.Add(1, availableInstruments[11]);
            availableInstrumentSets.Add(new InstrumentSet("Sky", "하늘", instruments, InstrumentSet.Type.character, 4, 7));

            instruments = new Dictionary<int, InstrumentInfo>();
            instruments.Add(0, availableInstruments[22]);
            instruments.Add(1, availableInstruments[13]);
            availableInstrumentSets.Add(new InstrumentSet("Medieval", "중세", instruments, InstrumentSet.Type.character, 3, 6));

            instruments = new Dictionary<int, InstrumentInfo>();
            instruments.Add(0, availableInstruments[23]);
            instruments.Add(1, availableInstruments[24]);
            instruments.Add(2, availableInstruments[25]);
            availableInstrumentSets.Add(new InstrumentSet("Choir", "합창", instruments, InstrumentSet.Type.character, 4, 5));

            /* 
             * InstrumentSet.Type.whitespace
             */
            /*
           instruments = new Dictionary<int, InstrumentInfo>();
           instruments.Add(3, availableInstruments[2]);
           availableInstrumentSets.Add(new InstrumentSet("Bird", "새", instruments, InstrumentSet.Type.whitespace));
           */

            instruments = new Dictionary<int, InstrumentInfo>();
            instruments.Add(3, availableInstruments[5]);
            instruments.Add(5, availableInstruments[6]);
            instruments.Add(6, availableInstruments[6]);
            availableInstrumentSets.Add(new InstrumentSet("Rain", "비", instruments, InstrumentSet.Type.whitespace));

            instruments = new Dictionary<int, InstrumentInfo>();
            instruments.Add(3, availableInstruments[14]);
            instruments.Add(4, availableInstruments[15]);
            availableInstrumentSets.Add(new InstrumentSet("Bell", "종", instruments, InstrumentSet.Type.whitespace));

            /* 
             * InstrumentSet.Type.accompaniment
             */
            instruments = new Dictionary<int, InstrumentInfo>();
            instruments.Add(7, availableInstruments[16]);
            instruments.Add(8, availableInstruments[17]);
            availableInstrumentSets.Add(new InstrumentSet("Piano", "피아노", instruments, InstrumentSet.Type.accompaniment, 3, 7));
            
            instruments = new Dictionary<int, InstrumentInfo>();
            instruments.Add(7, availableInstruments[20]);
            instruments.Add(8, availableInstruments[21]);
            availableInstrumentSets.Add(new InstrumentSet("Melody", "멜로디", instruments, InstrumentSet.Type.accompaniment, 3, 7));   // 1번 채널의 악기를 사용

            instruments = new Dictionary<int, InstrumentInfo>();
            instruments.Add(7, availableInstruments[26]);
            instruments.Add(8, availableInstruments[21]);
            availableInstrumentSets.Add(new InstrumentSet("Timpani", "팀파니", instruments, InstrumentSet.Type.accompaniment, 1, 3));

            instruments = new Dictionary<int, InstrumentInfo>();
            instruments.Add(7, availableInstruments[21]);
            //instruments.Add(8, availableInstruments[21]);
            availableInstrumentSets.Add(new InstrumentSet("Guitar", "기타", instruments, InstrumentSet.Type.accompaniment, 4, 5));   // 1번 채널의 악기를 사용, 밑반주 패턴이 "멜로디"와 다름

            // TODO
            isInstrumentsReady = true;

            /*
             * Default SFXTheme
             * Note: Please do not use ' '(space) on theme name.
             */
            availableSFXThemes = new List<SFXTheme>();
            availableSFXThemes.Add(new SFXTheme("Autumn", "가을 산책", ChordTransitionType.SomewhatHappy, "Guitar", null, "Guitar"));
            availableSFXThemes.Add(new SFXTheme("Rain", "비 오는 날", ChordTransitionType.SomewhatBlue, "Forest", "Rain", null));                           // Disabled
            availableSFXThemes.Add(new SFXTheme("Star", "별 헤는 밤", ChordTransitionType.SimilarOne, "Star", "Bell", null));
            availableSFXThemes.Add(new SFXTheme("Forest", "숲 속 아침", ChordTransitionType.SimilarOne, "Forest", null, "Melody"));                         // Disabled
            availableSFXThemes.Add(new SFXTheme("Pianoforte", "피아노포르테", ChordTransitionType.SimilarOne, "Piano", null, "Piano"));
            availableSFXThemes.Add(new SFXTheme("Pianoforte_low", "피아노포르테 저음", ChordTransitionType.SomewhatBlue, "Piano_low", null, "Piano"));      // Disabled
            availableSFXThemes.Add(new SFXTheme("Pianoforte_high", "피아노포르테 고음", ChordTransitionType.SomewhatHappy, "Piano_high", null, "Piano"));   // Disabled
            availableSFXThemes.Add(new SFXTheme("Sky", "구름 너머", ChordTransitionType.SomewhatHappy, "Sky", null, "Melody"));
            availableSFXThemes.Add(new SFXTheme("Medieval", "중세 탐방", ChordTransitionType.SomewhatBlue, "Medieval", "Bell", "Piano"));                   // Disabled
            availableSFXThemes.Add(new SFXTheme("Medieval_ruins", "중세 유적지", ChordTransitionType.SomewhatBlue, "Choir", "Bell", "Timpani"));

            IsReady = true;
        }

        /// <summary>
        /// availableSFXThemes에서 name이 일치하는 음악 테마를 찾아 반환합니다.
        /// 만약 찾지 못하면, Name이 null이고 Instruments가 빈 SFXTheme이 반환됩니다.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static SFXTheme FindSFXTheme(string name)
        {
            SFXTheme empty = new SFXTheme(null, null, ChordTransitionType.SomewhatHappy, null, null, null);
            if (!IsReady || name == null) return empty;
            
            foreach (SFXTheme t in availableSFXThemes)
            {
                if (name.Equals(t.Name))
                {
                    return t;
                }
            }
            return empty;
        }

        /// <summary>
        /// availableSFXThemes에 있는 모든 SFXTheme들의 이름을 반환합니다.
        /// </summary>
        /// <returns></returns>
        public static List<string> GetAllSFXThemeName()
        {
            List<string> list = new List<string>();
            if (!IsReady) return list;

            foreach (SFXTheme t in availableSFXThemes)
            {
                if (t.Name != null)
                {
                    list.Add(t.Name);
                }
            }
            return list;
        }

        /// <summary>
        /// availableInstrumentSet에서 name과 type이 모두 일치하는 악기 세트를 찾아 반환합니다.
        /// 만약 찾지 못하면, name이 null이고 instruments가 비어 있는 InstrumentSet이 반환됩니다.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private InstrumentSet FindInstrumentSet(string name, InstrumentSet.Type type)
        {
            InstrumentSet empty = new InstrumentSet(null, null, new Dictionary<int, InstrumentInfo>(), InstrumentSet.Type.character);
            if (!isInstrumentsReady) return empty;

            foreach (InstrumentSet i in availableInstrumentSets)
            {
                if (i.name.Equals(name) && i.type.Equals(type))
                {
                    return i;
                }
            }
            return empty;
        }

        /// <summary>
        /// main에 additional의, 채널(Staff)이 겹치지 않는 instruments를 추가합니다.
        /// </summary>
        /// <param name="main">기존 악기 목록</param>
        /// <param name="additional">추가될 악기 세트</param>
        private void ConcatenateInstrumentSet(Dictionary<int, InstrumentInfo> main, InstrumentSet additional)
        {
            foreach (KeyValuePair<int, InstrumentInfo> p in additional.instruments)
            {
                if (!main.ContainsKey(p.Key))
                    main.Add(p.Key, p.Value);
            }
            if (additional.type == InstrumentSet.Type.character && additional.name != null)
            {
                MinOctave = additional.minOctave;
                MaxOctave = additional.maxOctave;
            }
            else if (additional.type == InstrumentSet.Type.accompaniment && additional.name != null)
            {
                MinOctaveInAccompaniment = additional.minOctave;
                MaxOctaveInAccompaniment = additional.maxOctave;
            }
        }
    }
}
