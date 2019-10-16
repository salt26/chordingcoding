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
        // 1마디 = 온음표 = 2초 = 64 tick

        /// <summary>
        /// 사용 가능한 반주 패턴 목록
        /// </summary>
        public static List<Pattern> availablePatterns = new List<Pattern>();

        /// <summary>
        /// 현재 재생할 반주 패턴
        /// </summary>
        public static Pattern currentPattern;

        public static bool IsReady { get; private set; } = false;

        /// <summary>
        /// 반주 패턴 구조체입니다.
        /// </summary>
        public struct Pattern
        {
            /// <summary>
            /// 반주 패턴의 영어 이름
            /// </summary>
            public string name;

            /// <summary>
            /// 반주 패턴의 한글(보여질) 이름
            /// </summary>
            public string displayName;

            /// <summary>
            /// 반주 패턴 악보. 재생할 음표들이 담겨 있습니다.
            /// </summary>
            public Score score;

            /// <summary>
            /// 반주 패턴 악보의 길이 (16분음표로 채우는 개수, 예: 1마디 = 16)
            /// </summary>
            public int length;

            /// <summary>
            /// 반주 패턴을 연속으로 재생할 반복 횟수
            /// </summary>
            public int iteration;

            public Pattern(string name, string displayName, Score score, int length, int iteration = 1)
            {
                this.name = name;
                this.displayName = displayName;
                this.score = score;
                this.length = length;
                this.iteration = iteration;
            }
        }

        /// <summary>
        /// 사용 가능한 반주 패턴 목록과 현재 재생할 반주 패턴을 초기화합니다.
        /// </summary>
        public static void Initialize()
        {
            Score score;

            score = new Score();
            score.AddNote(() => Music.chord.NotesInChord()[0], 4, 0, 0, 7);
            score.AddNote(() => Music.chord.NotesInChord()[2] - 12, 8, 0, 0, 7);
            score.AddNote(() => Music.chord.NotesInChord()[1] - 12, 8, 0, 0, 7);
            score.AddNote(() => Music.chord.NotesInChord()[0] - 12, 8, 0, 0, 7);
            score.AddNote(() => Music.chord.NotesInChord()[2], 4, 0, 4, 7);
            score.AddNote(() => Music.chord.NotesInChord()[1], 4, 0, 8, 7);
            score.AddNote(() => Music.chord.NotesInChord()[0], 4, 0, 8, 7);
            score.AddNote(() => Music.chord.NotesInChord()[2], 4, 0, 12, 7);
            availablePatterns.Add(new Pattern("4", "4", score, 16, 4)); // TODO 이름 바꾸기

            score = new Score();
            score.AddNote(() => Music.chord.NotesInChord()[2] - 12, 8, 0, 0, 7);
            score.AddNote(() => Music.chord.NotesInChord()[0] - 12, 8, 0, 0, 7);
            score.AddNote(() => Music.chord.NotesInChord()[0], 2, 0, 2, 7);
            score.AddNote(() => Music.chord.NotesInChord()[1], 2, 0, 4, 7);
            score.AddNote(() => Music.chord.NotesInChord()[2], 2, 0, 6, 7);
            score.AddNote(() => Music.chord.NotesInChord()[0] + 12, 4, 0, 8, 7);
            score.AddNote(() => Music.chord.NotesInChord()[2], 2, 0, 10, 7);
            score.AddNote(() => Music.chord.NotesInChord()[1], 4, 0, 12, 7);
            availablePatterns.Add(new Pattern("7", "7", score, 16, 4)); // TODO 이름 바꾸기

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
