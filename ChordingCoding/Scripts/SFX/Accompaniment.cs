using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChordingCoding.SFX
{
    /// <summary>
    /// 반주 패턴들을 관리하는 클래스입니다.
    /// </summary>
    class Accompaniment
    {
        /// <summary>
        /// 사용 가능한 반주 패턴 목록 (Key는 Score, Value는 반주 패턴의 총 길이를 tick 수로 환산한 수(예: 1마디 = 온음표 = 2초 = 64 tick))
        /// </summary>
        public static List<KeyValuePair<Score, int>> availablePatterns = new List<KeyValuePair<Score, int>>();

        /// <summary>
        /// 현재 재생할 반주 패턴 (Key는 Score, Value는 반주 패턴의 총 길이를 tick 수로 환산한 수(예: 1마디 = 온음표 = 2초 = 64 tick))
        /// </summary>
        public static KeyValuePair<Score, int> currentPattern;

        public static bool IsReady { get; private set; } = false;

        /// <summary>
        /// 사용 가능한 반주 패턴 목록과 현재 재생할 반주 패턴을 초기화합니다.
        /// </summary>
        public static void Initialize()
        {
            Score score;

            score = new Score();
            score.AddNote(() => Music.chord.NotesInChord()[0], 4, 0, 0, 7);
            score.AddNote(() => Music.chord.NotesInChord()[2], 4, 0, 4, 7);
            score.AddNote(() => Music.chord.NotesInChord()[1], 4, 0, 8, 7);
            score.AddNote(() => Music.chord.NotesInChord()[2], 4, 0, 12, 7);
            availablePatterns.Add(new KeyValuePair<Score, int>(score, 64));  // TODO

            SetNewCurrentPattern();

            IsReady = true;
        }

        /// <summary>
        /// 현재 반주 패턴을 사용 가능한 반주 패턴들 중에서 랜덤으로 설정합니다.
        /// </summary>
        public static void SetNewCurrentPattern()
        {
            Random r = new Random();
            currentPattern = availablePatterns[r.Next(availablePatterns.Count)];
        }
    }
}
