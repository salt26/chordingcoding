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
        /// 현재 재생할 반주 패턴 (Key는 staff 번호)
        /// </summary>
        public static Dictionary<int, Pattern> currentPatterns = new Dictionary<int, Pattern>();

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
            availablePatterns = new List<Pattern>();
            currentPatterns = new Dictionary<int, Pattern>();
            Score score;

            /*
            score = new Score();
            score.AddNote(() => Music.chord.NotesInChord()[0] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 8, 0, 0, 7);
            score.AddNote(() => Music.chord.NotesInChord()[2] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave) * 12, 8, 0, 0, 7);
            score.AddNote(() => Music.chord.NotesInChord()[1] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave) * 12, 8, 0, 0, 7);
            score.AddNote(() => Music.chord.NotesInChord()[0] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave) * 12, 8, 0, 0, 7);
            score.AddNote(() => Music.chord.NotesInChord()[2] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 8, 0, 8, 7);
            score.AddNote(() => Music.chord.NotesInChord()[1] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 8, 0, 8, 7);
            availablePatterns.Add(new Pattern("2bits", "2비트", score, 16, 4)); // TODO 이름 바꾸기
            
            score = new Score();
            score.AddNote(() => Music.chord.NotesInChord()[0] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 4, 0, 0, 7);
            score.AddNote(() => Music.chord.NotesInChord()[2] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave) * 12, 8, 0, 0, 7);
            score.AddNote(() => Music.chord.NotesInChord()[1] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave) * 12, 8, 0, 0, 7);
            score.AddNote(() => Music.chord.NotesInChord()[0] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave) * 12, 8, 0, 0, 7);
            score.AddNote(() => Music.chord.NotesInChord()[2] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 4, 0, 4, 7);
            score.AddNote(() => Music.chord.NotesInChord()[1] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 4, 0, 8, 7);
            score.AddNote(() => Music.chord.NotesInChord()[0] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 4, 0, 8, 7);
            score.AddNote(() => Music.chord.NotesInChord()[2] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 4, 0, 12, 7);
            availablePatterns.Add(new Pattern("4bits", "4비트", score, 16, 4)); // TODO 이름 바꾸기

            score = new Score();
            score.AddNote(() => Music.chord.NotesInChord()[2] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave) * 12, 8, 0, 0, 7);
            score.AddNote(() => Music.chord.NotesInChord()[0] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave) * 12, 8, 0, 0, 7);
            score.AddNote(() => Music.chord.NotesInChord()[0] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 2, 0, 2, 7);
            score.AddNote(() => Music.chord.NotesInChord()[1] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 2, 0, 4, 7);
            score.AddNote(() => Music.chord.NotesInChord()[2] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 2, 0, 6, 7);
            score.AddNote(() => Music.chord.NotesInChord()[0] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 2) * 12, 8, 0, 8, 7);
            score.AddNote(() => Music.chord.NotesInChord()[2] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 2, 0, 10, 7);
            score.AddNote(() => Music.chord.NotesInChord()[1] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 4, 0, 12, 7);
            availablePatterns.Add(new Pattern("7", "7", score, 16, 4)); // TODO 이름 바꾸기
            
            score = new Score();
            score.AddNote(() => Music.chord.NotesInChord()[0] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 2, 0, 0, 7);
            score.AddNote(() => Music.chord.NotesInChord()[2] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave) * 12, 8, 0, 0, 7);
            score.AddNote(() => Music.chord.NotesInChord()[1] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave) * 12, 8, 0, 0, 7);
            score.AddNote(() => Music.chord.NotesInChord()[0] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave) * 12, 8, 0, 0, 7);
            score.AddNote(() => Music.chord.NotesInChord()[2] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 2, 0, 2, 7);
            score.AddNote(() => Music.chord.NotesInChord()[1] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 2, 0, 4, 7);
            score.AddNote(() => Music.chord.NotesInChord()[2] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 2, 0, 6, 7);
            score.AddNote(() => Music.chord.NotesInChord()[0] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 2, 0, 8, 7);
            score.AddNote(() => Music.chord.NotesInChord()[2] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave) * 12, 8, 0, 8, 7);
            score.AddNote(() => Music.chord.NotesInChord()[2] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 2, 0, 10, 7);
            score.AddNote(() => Music.chord.NotesInChord()[1] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 2, 0, 12, 7);
            score.AddNote(() => Music.chord.NotesInChord()[2] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 2, 0, 14, 7);
            availablePatterns.Add(new Pattern("8bits", "8비트", score, 16, 4)); // TODO 이름 바꾸기

            score = new Score();
            score.AddNote(() => Music.chord.NotesInChord()[0] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 1, 0, 0, 7);
            score.AddNote(() => Music.chord.NotesInChord()[1] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 1, 0, 1, 7);
            score.AddNote(() => Music.chord.NotesInChord()[2] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 1, 0, 2, 7);
            score.AddNote(() => Music.chord.NotesInChord()[0] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 1, 0, 3, 7);
            score.AddNote(() => Music.chord.NotesInChord()[1] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 1, 0, 4, 7);
            score.AddNote(() => Music.chord.NotesInChord()[2] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 1, 0, 5, 7);
            score.AddNote(() => Music.chord.NotesInChord()[0] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 1, 0, 6, 7);
            score.AddNote(() => Music.chord.NotesInChord()[1] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 1, 0, 7, 7);
            score.AddNote(() => Music.chord.NotesInChord()[2] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 1, 0, 8, 7);
            score.AddNote(() => Music.chord.NotesInChord()[0] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 1, 0, 9, 7);
            score.AddNote(() => Music.chord.NotesInChord()[1] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 1, 0, 10, 7);
            score.AddNote(() => Music.chord.NotesInChord()[2] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 1, 0, 11, 7);
            score.AddNote(() => Music.chord.NotesInChord()[1] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 1, 0, 12, 7);
            score.AddNote(() => Music.chord.NotesInChord()[2] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 1, 0, 13, 7);
            score.AddNote(() => Music.chord.NotesInChord()[1] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 1, 0, 14, 7);
            score.AddNote(() => Music.chord.NotesInChord()[2] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 1, 0, 15, 7);
            availablePatterns.Add(new Pattern("16bits", "16비트", score, 16, 4)); // TODO 이름 바꾸기

            score = new Score();
            score.AddNote(() => Music.chord.NotesInChord()[0] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 1, 0, 0, 7);
            score.AddNote(() => Music.chord.NotesInChord()[1] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 1, 0, 1, 7);
            score.AddNote(() => Music.chord.NotesInChord()[2] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 1, 0, 2, 7);
            score.AddNote(() => Music.chord.NotesInChord()[0] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 1, 0, 4, 7);
            score.AddNote(() => Music.chord.NotesInChord()[1] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 1, 0, 5, 7);
            score.AddNote(() => Music.chord.NotesInChord()[2] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 1, 0, 6, 7);
            score.AddNote(() => Music.chord.NotesInChord()[0] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 1, 0, 8, 7);
            score.AddNote(() => Music.chord.NotesInChord()[1] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 1, 0, 9, 7);
            score.AddNote(() => Music.chord.NotesInChord()[2] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 1, 0, 10, 7);
            score.AddNote(() => Music.chord.NotesInChord()[0] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 1, 0, 12, 7);
            score.AddNote(() => Music.chord.NotesInChord()[1] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 1, 0, 13, 7);
            score.AddNote(() => Music.chord.NotesInChord()[2] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 1, 0, 14, 7);
            availablePatterns.Add(new Pattern("16bits2", "16비트2", score, 16, 4)); // TODO 이름 바꾸기

            score = new Score();
            score.AddNote(() => Music.chord.NotesInChord()[0] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 1, 0, 0, 7);
            score.AddNote(() => Music.chord.NotesInChord()[2] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave) * 12, 1, 0, 0, 7);
            score.AddNote(() => Music.chord.NotesInChord()[0] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 1, 0, 1, 7);
            score.AddNote(() => Music.chord.NotesInChord()[2] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave) * 12, 1, 0, 1, 7);
            score.AddNote(() => Music.chord.NotesInChord()[0] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 1, 0, 4, 7);
            score.AddNote(() => Music.chord.NotesInChord()[2] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave) * 12, 1, 0, 4, 7);
            score.AddNote(() => Music.chord.NotesInChord()[0] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 1, 0, 5, 7);
            score.AddNote(() => Music.chord.NotesInChord()[2] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave) * 12, 1, 0, 5, 7);
            score.AddNote(() => Music.chord.NotesInChord()[0] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 1, 0, 8, 7);
            score.AddNote(() => Music.chord.NotesInChord()[2] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave) * 12, 1, 0, 8, 7);
            score.AddNote(() => Music.chord.NotesInChord()[0] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 1, 0, 9, 7);
            score.AddNote(() => Music.chord.NotesInChord()[2] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave) * 12, 1, 0, 9, 7);
            score.AddNote(() => Music.chord.NotesInChord()[0] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 1, 0, 12, 7);
            score.AddNote(() => Music.chord.NotesInChord()[2] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave) * 12, 1, 0, 12, 7);
            score.AddNote(() => Music.chord.NotesInChord()[0] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 1, 0, 13, 7);
            score.AddNote(() => Music.chord.NotesInChord()[2] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave) * 12, 1, 0, 13, 7);
            availablePatterns.Add(new Pattern("Accent", "악센트", score, 16, 4)); // TODO 이름 바꾸기
            */

            score = new Score();
            score.AddNote(() => Music.chord.GetNote(0, 0), 1, 0, 0, 7);
            score.AddNote(() => Music.chord.GetNote(1, 0), 1, 0, 1, 7);
            score.AddNote(() => Music.chord.GetNote(2, 0), 1, 0, 2, 7);
            score.AddNote(() => Music.chord.GetNote(1, 1), 1, 0, 3, 7);
            score.AddNote(() => Music.chord.GetNote(0, 1), 1, 0, 4, 7);
            score.AddNote(() => Music.chord.GetNote(1, 1), 1, 0, 5, 7);
            score.AddNote(() => Music.chord.GetNote(2, 1), 1, 0, 6, 7);
            score.AddNote(() => Music.chord.GetNote(1, 2), 1, 0, 7, 7);
            score.AddNote(() => Music.chord.GetNote(0, 2), 1, 0, 8, 7);
            score.AddNote(() => Music.chord.GetNote(1, 2), 1, 0, 9, 7);
            score.AddNote(() => Music.chord.GetNote(2, 2), 1, 0, 10, 7);
            score.AddNote(() => Music.chord.GetNote(1, 3), 1, 0, 11, 7);
            score.AddNote(() => Music.chord.GetNote(0, 3), 1, 0, 12, 7);
            score.AddNote(() => Music.chord.GetNote(1, 3), 1, 0, 13, 7);
            score.AddNote(() => Music.chord.GetNote(2, 3), 1, 0, 14, 7);
            score.AddNote(() => Music.chord.GetNote(1, 4), 1, 0, 15, 7);
            score.AddNote(() => Music.chord.GetNote(2, 4), 1, 1, 0, 7);
            score.AddNote(() => Music.chord.GetNote(1, 4), 1, 1, 1, 7);
            score.AddNote(() => Music.chord.GetNote(0, 4), 1, 1, 2, 7);
            score.AddNote(() => Music.chord.GetNote(1, 3), 1, 1, 3, 7);
            score.AddNote(() => Music.chord.GetNote(2, 3), 1, 1, 4, 7);
            score.AddNote(() => Music.chord.GetNote(1, 3), 1, 1, 5, 7);
            score.AddNote(() => Music.chord.GetNote(0, 3), 1, 1, 6, 7);
            score.AddNote(() => Music.chord.GetNote(1, 2), 1, 1, 7, 7);
            score.AddNote(() => Music.chord.GetNote(2, 2), 1, 1, 8, 7);
            score.AddNote(() => Music.chord.GetNote(1, 2), 1, 1, 9, 7);
            score.AddNote(() => Music.chord.GetNote(0, 2), 1, 1, 10, 7);
            score.AddNote(() => Music.chord.GetNote(1, 1), 1, 1, 11, 7);
            score.AddNote(() => Music.chord.GetNote(2, 1), 1, 1, 12, 7);
            score.AddNote(() => Music.chord.GetNote(1, 1), 1, 1, 13, 7);
            score.AddNote(() => Music.chord.GetNote(0, 1), 1, 1, 14, 7);
            score.AddNote(() => Music.chord.GetNote(1, 0), 1, 1, 15, 7);
            availablePatterns.Add(new Pattern("Mountain1", "산1", score, 32, 1)); // TODO 이름 바꾸기

            SetNewCurrentPattern(7);
            SetNewCurrentPattern(8);

            IsReady = true;
        }

        /// <summary>
        /// 특정 staff의 현재 반주 패턴을 새로 설정합니다.
        /// </summary>
        /// <param name="staff">반주 staff 번호 (7 또는 8)</param>
        public static void SetNewCurrentPattern(int staff)
        {
            if (staff == 7)
                SelectRandomAvailablePattern();
            else if (staff == 8)
                GenerateRhythmPattern();
        }

        /// <summary>
        /// 7번 staff의 현재 반주 패턴을 사용 가능한 반주 패턴 중에서 랜덤으로 골라 설정합니다.
        /// </summary>
        private static void SelectRandomAvailablePattern()
        {
            Random r = new Random();
            currentPatterns[7] = availablePatterns[r.Next(availablePatterns.Count)];
        }

        /// <summary>
        /// 무작위 리듬으로 한 마디의 리듬 패턴을 생성합니다.
        /// 그리고 8번 staff의 현재 반주 패턴을 현재 화음에 맞는 이 리듬의 패턴으로 설정합니다.
        /// </summary>
        private static void GenerateRhythmPattern()
        {
            Random r = new Random();
            List<int> rhythmsLength = new List<int>();
            Score score = new Score();
            bool[] notes = new bool[17];
            int i;
            notes[0] = notes[16] = true;
            while (true)
            {
                for (i = 1; i < 16; i++) notes[i] = false;
                for (i = 0; i < 7; i++)
                {
                    if (r.Next(0, 2) == 1)
                    {
                        notes[i * 2 + 2] = true;
                    }
                }
                for (i = 0; i < 16; i += 2)
                {
                    if (notes[i] && notes[i + 2] && r.Next(0, 4) == 1)
                    {
                        notes[i + 1] = true;
                    }
                }
                for (i = 0; i < 14; i += 2)
                {
                    if (notes[i] && !notes[i + 2] && notes[i + 4])
                    {
                        int t = r.Next(0, 8);
                        switch (t)
                        {
                            case 1:
                                notes[i + 1] = true;
                                break;
                            case 2:
                                notes[i + 3] = true;
                                break;
                            case 3:
                                notes[i + 1] = true;
                                notes[i + 3] = true;
                                break;
                        }
                    }
                }
                for (i = 0; i < 10; i += 2)
                {
                    if (notes[i] && !notes[i + 2] && !notes[i + 4] && notes[i + 6] && r.Next(0, 4) == 1)
                    {
                        notes[i + 3] = true;
                    }
                }
                int count = 0;
                rhythmsLength.Clear();
                for (i = 1; i <= 16; i++)
                {
                    count++;
                    if (notes[i])
                    {
                        if (count != 1 && count != 2 && count != 3 && count != 4 && count != 6 && count != 8 && count != 12 && count != 16) break;
                        rhythmsLength.Add(count);
                        count = 0;
                    }
                }
                if (i > 16) break;
            }
            int count2 = 0;
            foreach (int j in rhythmsLength)
            {
                score.AddNote(() => Music.chord.NextNote(), j, 0, count2, 8);
                count2 += j;
            }
            currentPatterns[8] = new Pattern("Generated", "생성됨", score, 16, 1);
        }

    }
}
