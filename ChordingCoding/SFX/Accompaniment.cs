/*
MIT License

Copyright (c) 2019 salt26

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
using System;
using System.Collections.Generic;

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
        public static AvailablePatterns availablePatterns = new AvailablePatterns();

        /// <summary>
        /// 현재 재생할 반주 패턴 (Key는 staff 번호)
        /// </summary>
        public static Dictionary<int, Pattern> currentPatterns = new Dictionary<int, Pattern>();

        private static bool IsReady { get; set; } = false;
        public static bool HasStart { get; private set; } = false;


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
            /// 반주 패턴 악보의 길이 (64분음표로 채우는 개수, 예: 1마디 = 64)
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

        public class AvailablePatterns
        {
            Dictionary<string, List<Pattern>> availablePatterns;

            public AvailablePatterns()
            {
                this.availablePatterns = new Dictionary<string, List<Pattern>>();
            }

            /// <summary>
            /// 사용 가능한 반주 패턴 목록에 새 패턴을 추가합니다.
            /// 패턴들은 반주 악기 세트의 이름이 같은 것끼리 묶여서 관리됩니다.
            /// </summary>
            /// <param name="pattern">반주 패턴</param>
            /// <param name="accompanimentInstSetNames">이 반주 패턴이 사용될, SFXTheme.InstrumentSet.Type이 accompaniment인 악기 세트들의 영어 이름</param>
            public void Add(Pattern pattern, params string[] accompanimentInstSetNames)
            {
                foreach (string s in accompanimentInstSetNames)
                {
                    if (!availablePatterns.ContainsKey(s))
                    {
                        availablePatterns.Add(s, new List<Pattern>());
                    }
                    availablePatterns[s].Add(pattern);
                }
            }

            /// <summary>
            /// 사용 가능한 반주 패턴 중 주어진 반주 악기 세트 이름으로 묶인 패턴 하나를 랜덤으로 반환합니다.
            /// </summary>
            /// <param name="accompanimentInstSetName">이 반주 패턴이 사용될, SFXTheme.InstrumentSet.Type이 accompaniment인 악기 세트들의 영어 이름</param>
            /// <returns>선택된 반주 패턴</returns>
            public Pattern Get(string accompanimentInstSetName)
            {
                if (!availablePatterns.ContainsKey(accompanimentInstSetName))
                {
                    return new Pattern("null", "null", new Score(), 0, 0);
                }
                int length = availablePatterns[accompanimentInstSetName].Count;
                Random r = new Random();
                return availablePatterns[accompanimentInstSetName][r.Next(length)];
            }
        }

        /// <summary>
        /// 사용 가능한 반주 패턴 목록과 현재 재생할 반주 패턴을 초기화합니다.
        /// </summary>
        public static void Initialize()
        {
            availablePatterns = new AvailablePatterns();
            currentPatterns = new Dictionary<int, Pattern>();
            Score score;

            /*
            score = new Score();
            score.AddNoteInAccompaniment(() => Music.chord.GetNoteInAccompaniment(3, 0), 32, 0, 0, 7);
            score.AddNoteInAccompaniment(() => Music.chord.GetNoteInAccompaniment(2, 0), 32, 0, 0, 7);
            score.AddNoteInAccompaniment(() => Music.chord.GetNoteInAccompaniment(1, 0), 32, 0, 0, 7);
            score.AddNoteInAccompaniment(() => Music.chord.GetNoteInAccompaniment(0, 0), 32, 0, 0, 7);
            score.AddNoteInAccompaniment(() => Music.chord.GetNoteInAccompaniment(2, 1), 32, 0, 32, 7);
            score.AddNoteInAccompaniment(() => Music.chord.GetNoteInAccompaniment(1, 1), 32, 0, 32, 7);
            availablePatterns.Add(new Pattern("2 2", "2 2", score, 64, 4), "Timpani"); // TODO 이름 바꾸기
            */

            score = new Score();
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(3, 0), 16, 0, 0, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 0), 32, 0, 0, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 0), 32, 0, 0, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 0), 32, 0, 0, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 1), 16, 0, 16, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 1), 16, 0, 32, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 1), 16, 0, 32, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 1), 16, 0, 48, 7);
            availablePatterns.Add(new Pattern("4 4 4 4", "4 4 4 4", score, 64, 4), "Timpani");

            score = new Score();
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(3, 0), 16, 0, 0, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 0), 32, 0, 0, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 0), 32, 0, 0, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 0), 32, 0, 0, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 1), 16, 0, 16, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 0), 16, 0, 32, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 0), 16, 0, 32, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 0), 8, 0, 48, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(3, 0), 8, 0, 56, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 0), 8, 0, 56, 7);
            availablePatterns.Add(new Pattern("4 4 4 8 8", "4 4 4 8 8", score, 64, 4), "Timpani");

            score = new Score();
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(3, 0), 16, 0, 0, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 0), 32, 0, 0, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 0), 32, 0, 0, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 0), 32, 0, 0, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(3, 0), 16, 0, 16, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 0), 16, 0, 16, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 0), 12, 0, 32, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 0), 12, 0, 32, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 0), 12, 0, 44, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 0), 12, 0, 44, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 1), 8, 0, 56, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 0), 8, 0, 56, 7);
            availablePatterns.Add(new Pattern("4 4 8. 8. 8", "4 4 8. 8. 8", score, 64, 4), "Timpani");

            score = new Score();
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(3, 0), 8, 0, 0, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 0), 32, 0, 0, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 0), 32, 0, 0, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 0), 32, 0, 0, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 1), 8, 0, 8, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 1), 8, 0, 16, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 1), 8, 0, 24, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 1), 8, 0, 32, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 0), 32, 0, 32, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 1), 8, 0, 40, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 1), 8, 0, 48, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 1), 8, 0, 56, 7);
            availablePatterns.Add(new Pattern("8 8 8 8 8 8 8 8", "8 8 8 8 8 8 8 8", score, 64, 4), "Timpani");

            /*
            score = new Score();
            score.AddNoteInAccompaniment(() => Music.chord.NotesInChord()[2] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave) * 12, 32, 0, 0, 7);
            score.AddNoteInAccompaniment(() => Music.chord.NotesInChord()[0] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave) * 12, 32, 0, 0, 7);
            score.AddNoteInAccompaniment(() => Music.chord.NotesInChord()[0] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 8, 0, 8, 7);
            score.AddNoteInAccompaniment(() => Music.chord.NotesInChord()[1] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 8, 0, 16, 7);
            score.AddNoteInAccompaniment(() => Music.chord.NotesInChord()[2] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 8, 0, 24, 7);
            score.AddNoteInAccompaniment(() => Music.chord.NotesInChord()[0] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 2) * 12, 32, 0, 32, 7);
            score.AddNoteInAccompaniment(() => Music.chord.NotesInChord()[2] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 8, 0, 40, 7);
            score.AddNoteInAccompaniment(() => Music.chord.NotesInChord()[1] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 16, 0, 48, 7);
            availablePatterns.Add(new Pattern("7", "7", score, 64, 2)); // TODO 이름 바꾸기
            

            score = new Score();
            score.AddNoteInAccompaniment(() => Music.chord.NotesInChord()[0] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 4, 0, 0, 7);
            score.AddNoteInAccompaniment(() => Music.chord.NotesInChord()[1] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 4, 0, 4, 7);
            score.AddNoteInAccompaniment(() => Music.chord.NotesInChord()[2] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 4, 0, 8, 7);
            score.AddNoteInAccompaniment(() => Music.chord.NotesInChord()[0] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 4, 0, 12, 7);
            score.AddNoteInAccompaniment(() => Music.chord.NotesInChord()[1] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 4, 0, 16, 7);
            score.AddNoteInAccompaniment(() => Music.chord.NotesInChord()[2] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 4, 0, 20, 7);
            score.AddNoteInAccompaniment(() => Music.chord.NotesInChord()[0] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 4, 0, 24, 7);
            score.AddNoteInAccompaniment(() => Music.chord.NotesInChord()[1] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 4, 0, 28, 7);
            score.AddNoteInAccompaniment(() => Music.chord.NotesInChord()[2] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 4, 0, 32, 7);
            score.AddNoteInAccompaniment(() => Music.chord.NotesInChord()[0] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 4, 0, 36, 7);
            score.AddNoteInAccompaniment(() => Music.chord.NotesInChord()[1] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 4, 0, 40, 7);
            score.AddNoteInAccompaniment(() => Music.chord.NotesInChord()[2] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 4, 0, 44, 7);
            score.AddNoteInAccompaniment(() => Music.chord.NotesInChord()[1] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 4, 0, 48, 7);
            score.AddNoteInAccompaniment(() => Music.chord.NotesInChord()[2] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 4, 0, 52, 7);
            score.AddNoteInAccompaniment(() => Music.chord.NotesInChord()[1] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 4, 0, 56, 7);
            score.AddNoteInAccompaniment(() => Music.chord.NotesInChord()[2] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 4, 0, 60, 7);
            availablePatterns.Add(new Pattern("16bits", "16비트", score, 64, 2)); // TODO 이름 바꾸기

            score = new Score();
            score.AddNoteInAccompaniment(() => Music.chord.NotesInChord()[0] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 4, 0, 0, 7);
            score.AddNoteInAccompaniment(() => Music.chord.NotesInChord()[1] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 4, 0, 4, 7);
            score.AddNoteInAccompaniment(() => Music.chord.NotesInChord()[2] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 4, 0, 8, 7);
            score.AddNoteInAccompaniment(() => Music.chord.NotesInChord()[0] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 4, 0, 16, 7);
            score.AddNoteInAccompaniment(() => Music.chord.NotesInChord()[1] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 4, 0, 20, 7);
            score.AddNoteInAccompaniment(() => Music.chord.NotesInChord()[2] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 4, 0, 24, 7);
            score.AddNoteInAccompaniment(() => Music.chord.NotesInChord()[0] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 4, 0, 32, 7);
            score.AddNoteInAccompaniment(() => Music.chord.NotesInChord()[1] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 4, 0, 36, 7);
            score.AddNoteInAccompaniment(() => Music.chord.NotesInChord()[2] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 4, 0, 40, 7);
            score.AddNoteInAccompaniment(() => Music.chord.NotesInChord()[0] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 4, 0, 48, 7);
            score.AddNoteInAccompaniment(() => Music.chord.NotesInChord()[1] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 4, 0, 52, 7);
            score.AddNoteInAccompaniment(() => Music.chord.NotesInChord()[2] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 4, 0, 56, 7);
            availablePatterns.Add(new Pattern("16bits2", "16비트2", score, 64, 2)); // TODO 이름 바꾸기

            score = new Score();
            score.AddNoteInAccompaniment(() => Music.chord.NotesInChord()[0] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 4, 0, 0, 7);
            score.AddNoteInAccompaniment(() => Music.chord.NotesInChord()[2] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave) * 12, 4, 0, 0, 7);
            score.AddNoteInAccompaniment(() => Music.chord.NotesInChord()[0] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 4, 0, 4, 7);
            score.AddNoteInAccompaniment(() => Music.chord.NotesInChord()[2] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave) * 12, 4, 0, 4, 7);
            score.AddNoteInAccompaniment(() => Music.chord.NotesInChord()[0] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 4, 0, 16, 7);
            score.AddNoteInAccompaniment(() => Music.chord.NotesInChord()[2] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave) * 12, 4, 0, 16, 7);
            score.AddNoteInAccompaniment(() => Music.chord.NotesInChord()[0] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 4, 0, 20, 7);
            score.AddNoteInAccompaniment(() => Music.chord.NotesInChord()[2] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave) * 12, 4, 0, 20, 7);
            score.AddNoteInAccompaniment(() => Music.chord.NotesInChord()[0] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 4, 0, 32, 7);
            score.AddNoteInAccompaniment(() => Music.chord.NotesInChord()[2] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave) * 12, 4, 0, 32, 7);
            score.AddNoteInAccompaniment(() => Music.chord.NotesInChord()[0] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 4, 0, 36, 7);
            score.AddNoteInAccompaniment(() => Music.chord.NotesInChord()[2] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave) * 12, 4, 0, 36, 7);
            score.AddNoteInAccompaniment(() => Music.chord.NotesInChord()[0] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 4, 0, 48, 7);
            score.AddNoteInAccompaniment(() => Music.chord.NotesInChord()[2] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave) * 12, 4, 0, 48, 7);
            score.AddNoteInAccompaniment(() => Music.chord.NotesInChord()[0] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave + 1) * 12, 4, 0, 52, 7);
            score.AddNoteInAccompaniment(() => Music.chord.NotesInChord()[2] % 12 + (SFXTheme.CurrentSFXTheme.MinOctave) * 12, 4, 0, 52, 7);
            availablePatterns.Add(new Pattern("Accent", "악센트", score, 64, 2)); // TODO 이름 바꾸기
            */


            score = new Score();
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 0), 4, 0, 0, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 0), 4, 0, 4, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 0), 4, 0, 8, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 1), 4, 0, 12, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 1), 4, 0, 16, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 1), 4, 0, 20, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 1), 4, 0, 24, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 2), 4, 0, 28, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 2), 4, 0, 32, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 2), 4, 0, 36, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 2), 4, 0, 40, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 3), 4, 0, 44, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 3), 4, 0, 48, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 3), 4, 0, 52, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 3), 4, 0, 56, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 4), 4, 0, 60, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 4), 4, 1, 0, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 4), 4, 1, 4, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 4), 4, 1, 8, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 3), 4, 1, 12, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 3), 4, 1, 16, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 3), 4, 1, 20, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 3), 4, 1, 24, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 2), 4, 1, 28, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 2), 4, 1, 32, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 2), 4, 1, 36, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 2), 4, 1, 40, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 1), 4, 1, 44, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 1), 4, 1, 48, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 1), 4, 1, 52, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 1), 4, 1, 56, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 0), 4, 1, 60, 7);
            availablePatterns.Add(new Pattern("Mountain0121-2101", "산0121-2101", score, 128, 1), "Piano", "Melody");

            score = new Score();
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 1), 4, 0, 0, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 0), 4, 0, 4, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 1), 4, 0, 8, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 1), 4, 0, 12, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 2), 4, 0, 16, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 1), 4, 0, 20, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 2), 4, 0, 24, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 2), 4, 0, 28, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 3), 4, 0, 32, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 2), 4, 0, 36, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 3), 4, 0, 40, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 3), 4, 0, 44, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 4), 4, 0, 48, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 3), 4, 0, 52, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 4), 4, 0, 56, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 4), 4, 0, 60, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 4), 4, 1, 0, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 4), 4, 1, 4, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 3), 4, 1, 8, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 3), 4, 1, 12, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 3), 4, 1, 16, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 3), 4, 1, 20, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 2), 4, 1, 24, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 2), 4, 1, 28, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 2), 4, 1, 32, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 2), 4, 1, 36, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 1), 4, 1, 40, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 1), 4, 1, 44, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 1), 4, 1, 48, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 1), 4, 1, 52, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 0), 4, 1, 56, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 0), 4, 1, 60, 7);
            availablePatterns.Add(new Pattern("Mountain0212-2021", "산0212-2021", score, 128, 1), "Piano", "Melody");
            
            score = new Score();
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 0), 4, 0, 0, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 0), 4, 0, 4, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 1), 4, 0, 8, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 1), 4, 0, 12, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 1), 4, 0, 16, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 1), 4, 0, 20, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 2), 4, 0, 24, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 2), 4, 0, 28, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 2), 4, 0, 32, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 2), 4, 0, 36, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 3), 4, 0, 40, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 3), 4, 0, 44, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 3), 4, 0, 48, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 3), 4, 0, 52, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 4), 4, 0, 56, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 4), 4, 0, 60, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 5), 4, 1, 0, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 4), 4, 1, 4, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 4), 4, 1, 8, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(3, 3), 4, 1, 12, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 4), 4, 1, 16, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 3), 4, 1, 20, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 3), 4, 1, 24, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(3, 2), 4, 1, 28, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 3), 4, 1, 32, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 2), 4, 1, 36, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 2), 4, 1, 40, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(3, 1), 4, 1, 44, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 2), 4, 1, 48, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 1), 4, 1, 52, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 1), 4, 1, 56, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(3, 0), 4, 1, 60, 7);
            availablePatterns.Add(new Pattern("Mountain1202-0213", "산1202-0213", score, 128, 1), "Piano", "Melody");

            score = new Score();
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(3, 0), 4, 0, 0, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 0), 4, 0, 4, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 1), 4, 0, 8, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 1), 4, 0, 12, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(3, 1), 4, 0, 16, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 1), 4, 0, 20, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 2), 4, 0, 24, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 2), 4, 0, 28, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(3, 2), 4, 0, 32, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 2), 4, 0, 36, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 3), 4, 0, 40, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 3), 4, 0, 44, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(3, 3), 4, 0, 48, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 3), 4, 0, 52, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 4), 4, 0, 56, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 4), 4, 0, 60, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 4), 4, 1, 0, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 4), 4, 1, 4, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(3, 3), 4, 1, 8, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 3), 4, 1, 12, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 3), 4, 1, 16, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 3), 4, 1, 20, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(3, 2), 4, 1, 24, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 2), 4, 1, 28, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 2), 4, 1, 32, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 2), 4, 1, 36, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(3, 1), 4, 1, 40, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 1), 4, 1, 44, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 1), 4, 1, 48, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 1), 4, 1, 52, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(3, 0), 4, 1, 56, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 0), 4, 1, 60, 7);
            availablePatterns.Add(new Pattern("Mountain3201-2031", "산3201-2031", score, 128, 1), "Piano", "Melody");

            score = new Score();
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 0), 4, 0, 0, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 0), 4, 0, 4, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 0), 4, 0, 8, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 1), 4, 0, 12, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 1), 4, 0, 16, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 1), 4, 0, 20, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 1), 4, 0, 24, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 2), 4, 0, 28, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 2), 4, 0, 32, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 2), 4, 0, 36, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 2), 4, 0, 40, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 3), 4, 0, 44, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 3), 4, 0, 48, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 3), 4, 0, 52, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 3), 4, 0, 56, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 4), 4, 0, 60, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 4), 4, 1, 0, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 4), 4, 1, 4, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 3), 4, 1, 8, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 3), 4, 1, 12, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 3), 4, 1, 16, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 3), 4, 1, 20, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 2), 4, 1, 24, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 2), 4, 1, 28, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 2), 4, 1, 32, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 2), 4, 1, 36, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 1), 4, 1, 40, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 1), 4, 1, 44, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 1), 4, 1, 48, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 1), 4, 1, 52, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 0), 4, 1, 56, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 0), 4, 1, 60, 7);
            availablePatterns.Add(new Pattern("Mountain1020-1020", "산1020-1020", score, 128, 1), "Piano", "Melody");

            score = new Score();
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 0), 4, 0, 0, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 0), 4, 0, 4, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 0), 4, 0, 8, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(3, 0), 4, 0, 12, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 1), 4, 0, 16, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 1), 4, 0, 20, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 1), 4, 0, 24, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(3, 1), 4, 0, 28, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 2), 4, 0, 32, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 2), 4, 0, 36, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 2), 4, 0, 40, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(3, 2), 4, 0, 44, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 3), 4, 0, 48, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 3), 4, 0, 52, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 3), 4, 0, 56, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(3, 3), 4, 0, 60, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 4), 4, 1, 0, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(3, 3), 4, 1, 4, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 3), 4, 1, 8, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 3), 4, 1, 12, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 3), 4, 1, 16, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(3, 2), 4, 1, 20, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 2), 4, 1, 24, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 2), 4, 1, 28, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 2), 4, 1, 32, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(3, 1), 4, 1, 36, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 1), 4, 1, 40, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 1), 4, 1, 44, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 1), 4, 1, 48, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(3, 0), 4, 1, 52, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 0), 4, 1, 56, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 0), 4, 1, 60, 7);
            availablePatterns.Add(new Pattern("Mountain0123-1312", "산0123-1312", score, 128, 1), "Piano", "Melody");

            score = new Score();
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 1), 4, 0, 0, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 0), 4, 0, 4, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 1), 4, 0, 8, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 1), 4, 0, 12, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 2), 4, 0, 16, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 1), 4, 0, 20, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 2), 4, 0, 24, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 2), 4, 0, 28, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 3), 4, 0, 32, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 2), 4, 0, 36, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 3), 4, 0, 40, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 3), 4, 0, 44, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 4), 4, 0, 48, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 3), 4, 0, 52, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 4), 4, 0, 56, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 4), 4, 0, 60, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 4), 4, 1, 0, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 4), 4, 1, 4, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(3, 3), 4, 1, 8, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 4), 4, 1, 12, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 3), 4, 1, 16, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 3), 4, 1, 20, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(3, 2), 4, 1, 24, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 3), 4, 1, 28, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 2), 4, 1, 32, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 2), 4, 1, 36, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(3, 1), 4, 1, 40, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 2), 4, 1, 44, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 1), 4, 1, 48, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 1), 4, 1, 52, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(3, 0), 4, 1, 56, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 1), 4, 1, 60, 7);
            availablePatterns.Add(new Pattern("Mountain0202-2131", "산0202-2131", score, 128, 1), "Piano", "Melody");

            //

            score = new Score();
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 1), 4, 0, 0, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 0), 8, 0, 0, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 1), 4, 0, 4, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 1), 8, 0, 8, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 0), 8, 0, 8, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(3, 0), 8, 0, 8, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 1), 8, 0, 16, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 1), 8, 0, 16, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 1), 4, 0, 24, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 1), 8, 0, 28, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 0), 8, 0, 28, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 1), 4, 0, 36, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 1), 8, 0, 40, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 0), 8, 0, 40, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 0), 8, 0, 48, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 0), 8, 0, 48, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 1), 4, 0, 56, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(3, 0), 8, 0, 56, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 1), 4, 0, 60, 7);
            availablePatterns.Add(new Pattern("16 16 8 8 16 8 16 8 8 16 16", "16 16 8 8 16 8 16 8 8 16 16", score, 64, 4), "Guitar");

            score = new Score();
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 1), 8, 0, 0, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 0), 8, 0, 0, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 0), 32, 0, 0, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 0), 8, 0, 8, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 1), 8, 0, 12, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 0), 8, 0, 16, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 1), 8, 0, 24, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 0), 8, 0, 24, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 0), 4, 0, 32, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 0), 8, 0, 32, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 1), 8, 0, 36, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(3, 0), 8, 0, 36, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 0), 8, 0, 40, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 1), 16, 0, 48, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 0), 8, 0, 48, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 0), 8, 0, 56, 7);
            availablePatterns.Add(new Pattern("8 16 16 8 8 16 16 8 8 8", "8 16 16 8 8 16 16 8 8 8", score, 64, 4), "Guitar");

            score = new Score();
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 1), 8, 0, 0, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 0), 8, 0, 0, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 0), 8, 0, 0, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 0), 8, 0, 8, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 1), 8, 0, 12, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 1), 8, 0, 16, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 1), 16, 0, 24, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 1), 16, 0, 24, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 0), 16, 0, 24, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 1), 8, 0, 40, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 1), 8, 0, 40, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 0), 8, 0, 40, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(3, 0), 8, 0, 48, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 1), 8, 0, 52, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 0), 8, 0, 52, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 1), 8, 0, 56, 7);
            availablePatterns.Add(new Pattern("8 16 16 8 4 8 16 16 8", "8 16 16 8 4 8 16 16 8", score, 64, 4), "Guitar");

            score = new Score();
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 0), 8, 0, 0, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 0), 16, 0, 0, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 1), 8, 0, 12, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 0), 16, 0, 16, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(3, 0), 16, 0, 16, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 1), 8, 0, 24, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 0), 16, 0, 32, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 1), 8, 0, 36, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 1), 8, 0, 48, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 0), 16, 0, 48, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 0), 16, 0, 48, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 1), 8, 0, 56, 7);
            availablePatterns.Add(new Pattern("8. 16 8 8 16 8. 8 8", "8. 16 8 8 16 8. 8 8", score, 64, 4), "Guitar");

            //

            score = new Score();
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 1), 48, 0, 0, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 0), 48, 0, 0, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 0), 48, 0, 0, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 1), 16, 0, 48, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 1), 48, 0, 64, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 0), 48, 0, 64, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 1), 16, 0, 112, 7);
            availablePatterns.Add(new Pattern("2. 4 2. 4", "2. 4 2. 4", score, 128, 1), "Star");

            score = new Score();
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 1), 48, 0, 0, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(3, 0), 48, 0, 0, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 1), 16, 0, 48, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 0), 32, 0, 64, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 0), 32, 0, 64, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 1), 32, 0, 64, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 0), 32, 0, 96, 7);
            availablePatterns.Add(new Pattern("2. 4 2 2", "2. 4 2 2", score, 128, 1), "Star");

            score = new Score();
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 1), 48, 0, 0, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(3, 0), 48, 0, 0, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 1), 16, 0, 48, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 1), 24, 0, 64, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 0), 24, 0, 64, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 0), 24, 0, 88, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 0), 16, 0, 112, 7);
            availablePatterns.Add(new Pattern("2. 4 4. 4. 4", "2. 4 4. 4. 4", score, 128, 1), "Star");

            score = new Score();
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 1), 16, 0, 0, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 0), 16, 0, 0, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 0), 16, 0, 16, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 1), 16, 0, 32, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 1), 16, 0, 48, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 1), 16, 0, 48, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 1), 16, 0, 64, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(3, 0), 16, 0, 64, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 1), 16, 0, 80, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 1), 16, 0, 96, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 1), 16, 0, 112, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(3, 0), 16, 0, 112, 7);
            availablePatterns.Add(new Pattern("4 4 4 4 4 4 4 4", "4 4 4 4 4 4 4 4", score, 128, 1), "Star");

            score = new Score();
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 1), 32, 0, 0, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 0), 32, 0, 0, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 1), 32, 0, 32, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(3, 0), 32, 0, 32, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 0), 32, 0, 64, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 0), 32, 0, 64, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(3, 0), 32, 0, 96, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 0), 32, 0, 96, 7);
            availablePatterns.Add(new Pattern("2 2 2 2", "2 2 2 2", score, 128, 1), "Star");

            score = new Score();
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 1), 64, 0, 0, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 0), 64, 0, 0, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 0), 64, 0, 0, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 1), 64, 0, 64, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(1, 1), 64, 0, 64, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(3, 0), 64, 0, 64, 7);
            availablePatterns.Add(new Pattern("1 1", "1 1", score, 128, 1), "Star");

            score = new Score();
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 1), 64, 0, 0, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(2, 0), 64, 0, 0, 7);
            score.AddNoteInAccompaniment(() => Music.Chord.GetNoteInAccompaniment(0, 0), 64, 0, 0, 7);
            availablePatterns.Add(new Pattern("1 rest", "1 쉼표", score, 128, 1), "Star");

            SetNewCurrentPattern(7);
            SetNewCurrentPattern(8);

            IsReady = true;
        }

        /// <summary>
        /// 반주 패턴 재생 기능이 작동하기 위해서는
        /// Initialize()가 호출된 후에 반드시 호출되어야 합니다.
        /// </summary>
        public static void Start()
        {
            if (IsReady && !HasStart)
            {
                HasStart = true;
            }
        }

        /// <summary>
        /// 특정 staff의 현재 반주 패턴을 새로 설정하고 재생합니다.
        /// 현재 음악 테마의 자동 반주가 꺼진 상태이더라도 호출되어야 합니다.
        /// </summary>
        /// <param name="staff">반주 staff 번호 (7 또는 8)</param>
        public static void SetNewCurrentPattern(int staff)
        {
            if (!SFXTheme.CurrentSFXTheme.Instruments.ContainsKey(staff)) return;
            if (staff == 7)
                SelectRandomAvailablePattern();
            else if (staff == 8)
            {
                GenerateRhythmPattern(Music.NoteResolution);
            }

            if (HasStart) {
                Score.Play(Accompaniment.currentPatterns[staff].score, "Accompaniment", SFXTheme.CurrentSFXTheme.Instruments[staff].accompanimentVolume / 127f);
            }
        }

        /// <summary>
        /// 7번 staff의 현재 반주 패턴을 사용 가능한 반주 패턴 중에서 랜덤으로 골라 설정합니다.
        /// </summary>
        private static void SelectRandomAvailablePattern()
        {
            // TODO 현재 테마의 반주 악기 세트 이름에 따라 결정하도록
            currentPatterns[7] = availablePatterns.Get(SFXTheme.CurrentSFXTheme.InstrumentSetNames[SFXTheme.InstrumentSet.Type.accompaniment]);
        }

        /// <summary>
        /// 단위 리듬이 16분음표 이상인 무작위 리듬으로 한 마디의 리듬 패턴을 생성합니다.
        /// 그리고 8번 staff의 현재 반주 패턴을 현재 화음에 맞는 이 리듬의 패턴으로 설정합니다.
        /// </summary>
        /// <param name="noteResolution">단위 리듬의 길이(예: 64분음표 = 1, 16분음표 = 4). 4 이하의 값을 넣으면 단위 리듬이 16분음표가 됩니다.</param>
        private static void GenerateRhythmPattern(int noteResolution = 4)
        {
            Random r = new Random();
            List<int> rhythmsLength = new List<int>();
            Score score = new Score();
            bool[] notes = new bool[17];
            int i;

            if (noteResolution < 4) noteResolution = 4;

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
                if (j % (noteResolution / 4) != 0) continue;
                score.AddNoteInAccompaniment(() => Music.Chord.NextNote(), j * 4, 0, count2 * 4, 8);
                count2 += j;
            }
            currentPatterns[8] = new Pattern("Generated", "생성됨", score, 64, 1);
        }

    }
}
