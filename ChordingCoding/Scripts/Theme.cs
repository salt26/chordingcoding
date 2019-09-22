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

        public string name;                                // 테마의 영어 이름
        public string displayName;                         // 테마의 한글(보여질) 이름
        public ParticleSystem basicParticleSystem;         // 기본 파티클 시스템
        public ParticleSystem whitespaceParticleSystem;    // 공백 문자를 입력할 때 추가로 생성될 파티클 시스템
        public ParticleInfo characterParticleInfo;         // 일반 문자를 입력할 때 기본 파티클 시스템 안에 생성할 파티클의 정보

        /* instrumentSets
         * 일반 문자 입력 시 사용하는 악기 채널은 항상 0, 1
         * 공백 문자 입력 시 사용하는 악기 채널은 항상 0, 1, 2
         * 특정 테마일 때 항상 들리는 효과음의 악기 채널은 항상 3, 4
         * 생성되는 반주의 악기 채널은 항상 5, 6
         */
        public Dictionary<int, int> instrumentSets;        // Key는 채널(Staff) 번호, Value는 MIDI의 악기 번호
        public ChordTransition chordTransition;            // 화음 전이 확률 분포
        
    }
}
