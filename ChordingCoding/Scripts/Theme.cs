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
        public enum ChordTransition { SomewhatHappy, SomewhatBlue, SimilarOne };
        public struct ParticleInfo
        {
            public Particle.Type particleType;
            public int particleLifetime;
            public Color particleColor;
            public float particleSize;
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
        }
        /* InstrumentSet
         * 일반 문자 입력 시 사용하는 악기 채널은 항상 0, 1
         * 공백 문자 입력 시 사용하는 악기 채널은 항상 0, 1, 2
         * 특정 테마일 때 항상 들리는 효과음의 악기 채널은 항상 3, 4
         * 생성되는 반주의 악기 채널은 항상 5, 6
         */
        public struct InstrumentSet
        {
            public string name;
            public string displayName;
            public Dictionary<int, InstrumentInfo> instruments; // Key는 악기가 사용될 채널(staff), Value는 악기 정보
        }

        public string name;                                 // 테마의 영어 이름
        public string displayName;                          // 테마의 한글(보여질) 이름
        public ParticleSystem basicParticleSystem;          // 기본 파티클 시스템
        public ParticleSystem particleSystemForWhitespace;  // 공백 문자를 입력할 때 추가로 생성될 파티클 시스템
        public ParticleInfo particleInfoForCharacter;       // 일반 문자를 입력할 때 기본 파티클 시스템 안에 생성할 파티클의 정보

        public InstrumentSet instrumentSetForCharacter;     // 일반 문자 입력 시 사용될 악기 세트
        public InstrumentSet instrumentSetForWhitespace;    // 공백 문자 입력 시 추가로 사용될 악기 세트
        public InstrumentSet instrumentSetForThemeSFX;      // 특정 테마의 효과음으로 사용될 악기 세트
        public InstrumentSet instrumentSetForAccompaniment; // 반주 생성에 사용될 악기 세트

        public ChordTransition chordTransition;             // 화음 전이 확률 분포
        
    }
}
