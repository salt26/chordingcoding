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

namespace ChordingCoding.SFX.Test
{
    /// <summary>
    /// Test code for SFX.
    /// To run some tests, you can simply uncomment the line `new Test.SFXTest();` in Start() method of "Music.cs".
    /// </summary>
    class SFXTest
    {
        /// <summary>
        /// Run some tests.
        /// </summary>
        public SFXTest()
        {
            //ChordRecognitionTest();
            //RhythmPatternEditTest();
            RhythmPatternDistanceTest();
        }

        private void ChordRecognitionTest()
        {
            Console.WriteLine("Chord Recognition Test");
            int Pitch(Chord.Root root, int octave)
            {
                return (int)root + (octave + 1) * 12;
            }
            Score s1 = new Score();
            s1.AddNote(Pitch(Chord.Root.C, 5), 127, 16, 0, 0, 0);
            s1.AddNote(Pitch(Chord.Root.G, 5), 127, 16, 0, 0, 0);
            s1.AddNote(Pitch(Chord.Root.G, 5), 127, 12, 0, 16, 0);
            s1.AddNote(Pitch(Chord.Root.E, 5), 127, 4, 0, 28, 0);
            s1.AddNote(Pitch(Chord.Root.G, 5), 127, 8, 0, 32, 0);
            s1.AddNote(Pitch(Chord.Root.B, 5), 127, 16, 0, 40, 0);
            s1.AddNote(Pitch(Chord.Root.E, 5), 127, 16, 0, 40, 0);
            s1.AddNote(Pitch(Chord.Root.A, 5), 127, 8, 0, 56, 0);
            s1.AddNote(Pitch(Chord.Root.Db, 5), 127, 8, 0, 56, 0);

            s1.AddNote(Pitch(Chord.Root.C, 3), 127, 8, 0, 0, 1);
            s1.AddNote(Pitch(Chord.Root.G, 3), 127, 8, 0, 8, 1);
            s1.AddNote(Pitch(Chord.Root.E, 3), 127, 8, 0, 16, 1);
            s1.AddNote(Pitch(Chord.Root.G, 3), 127, 8, 0, 24, 1);
            s1.AddNote(Pitch(Chord.Root.C, 3), 127, 8, 0, 32, 1);
            s1.AddNote(Pitch(Chord.Root.G, 3), 127, 8, 0, 40, 1);
            s1.AddNote(Pitch(Chord.Root.E, 3), 127, 8, 0, 48, 1);
            s1.AddNote(Pitch(Chord.Root.A, 3), 127, 8, 0, 56, 1);

            Chord c11 = Chord.RecognizeChordFromScore(s1);
            Chord c14 = Chord.RecognizeChordFromScore(s1, true);
            Console.WriteLine("1 harmonic: " + c11.root + "" + c11.type + ", 4 harmonics: " + c14.root + "" + c14.type);

            //

            s1 = new Score();
            //s1.AddNote(Pitch(Chord.Root.A, 5), 127, 24, 0, 0, 0);
            //s1.AddNote(Pitch(Chord.Root.Db, 5), 127, 24, 0, 0, 0);
            s1.AddNote(Pitch(Chord.Root.Bb, 5), 127, 16, 0, 24, 0);
            s1.AddNote(Pitch(Chord.Root.F, 5), 127, 16, 0, 24, 0);
            s1.AddNote(Pitch(Chord.Root.Db, 5), 127, 16, 0, 24, 0);
            s1.AddNote(Pitch(Chord.Root.A, 5), 127, 8, 0, 40, 0);
            s1.AddNote(Pitch(Chord.Root.G, 5), 127, 8, 0, 48, 0);
            s1.AddNote(Pitch(Chord.Root.E, 5), 127, 8, 0, 56, 0);

            //s1.AddNote(Pitch(Chord.Root.A, 2), 127, 8, 0, 0, 1);
            //s1.AddNote(Pitch(Chord.Root.E, 3), 127, 8, 0, 8, 1);
            //s1.AddNote(Pitch(Chord.Root.A, 2), 127, 8, 0, 16, 1);
            s1.AddNote(Pitch(Chord.Root.E, 3), 127, 8, 0, 24, 1);
            s1.AddNote(Pitch(Chord.Root.Db, 3), 127, 8, 0, 32, 1);
            s1.AddNote(Pitch(Chord.Root.E, 3), 127, 8, 0, 40, 1);
            s1.AddNote(Pitch(Chord.Root.Db, 3), 127, 8, 0, 48, 1);
            s1.AddNote(Pitch(Chord.Root.E, 3), 127, 8, 0, 56, 1);

            c11 = Chord.RecognizeChordFromScore(s1);
            c14 = Chord.RecognizeChordFromScore(s1, true);
            Console.WriteLine("1 harmonic: " + c11.root + "" + c11.type + ", 4 harmonics: " + c14.root + "" + c14.type);

            //
            Console.WriteLine();

            s1 = new Score();
            s1.AddNote(Pitch(Chord.Root.C, 5), 127, 32, 0, 0, 0);
            s1.AddNote(Pitch(Chord.Root.E, 5), 127, 16, 0, 32, 0);
            s1.AddNote(Pitch(Chord.Root.G, 5), 127, 16, 0, 48, 0);

            s1.AddNote(Pitch(Chord.Root.C, 4), 127, 8, 0, 0, 1);
            s1.AddNote(Pitch(Chord.Root.G, 4), 127, 8, 0, 8, 1);
            s1.AddNote(Pitch(Chord.Root.E, 4), 127, 8, 0, 16, 1);
            s1.AddNote(Pitch(Chord.Root.G, 4), 127, 8, 0, 24, 1);
            s1.AddNote(Pitch(Chord.Root.C, 4), 127, 8, 0, 32, 1);
            s1.AddNote(Pitch(Chord.Root.G, 4), 127, 8, 0, 40, 1);
            s1.AddNote(Pitch(Chord.Root.E, 4), 127, 8, 0, 48, 1);
            s1.AddNote(Pitch(Chord.Root.G, 4), 127, 8, 0, 56, 1);

            c11 = Chord.RecognizeChordFromScore(s1);
            c14 = Chord.RecognizeChordFromScore(s1, true);
            Console.WriteLine("1 harmonic: " + c11.root + "" + c11.type + ", 4 harmonics: " + c14.root + "" + c14.type);

            s1 = new Score();
            s1.AddNote(Pitch(Chord.Root.B, 4), 127, 24, 0, 0, 0);
            s1.AddNote(Pitch(Chord.Root.C, 5), 127, 4, 0, 24, 0);
            s1.AddNote(Pitch(Chord.Root.D, 5), 127, 4, 0, 28, 0);
            s1.AddNote(Pitch(Chord.Root.C, 5), 127, 16, 0, 32, 0);

            s1.AddNote(Pitch(Chord.Root.D, 4), 127, 8, 0, 0, 1);
            s1.AddNote(Pitch(Chord.Root.G, 4), 127, 8, 0, 8, 1);
            s1.AddNote(Pitch(Chord.Root.F, 4), 127, 8, 0, 16, 1);
            s1.AddNote(Pitch(Chord.Root.G, 4), 127, 8, 0, 24, 1);
            s1.AddNote(Pitch(Chord.Root.C, 4), 127, 8, 0, 32, 1);
            s1.AddNote(Pitch(Chord.Root.G, 4), 127, 8, 0, 40, 1);
            s1.AddNote(Pitch(Chord.Root.E, 4), 127, 8, 0, 48, 1);
            s1.AddNote(Pitch(Chord.Root.G, 4), 127, 8, 0, 56, 1);

            c11 = Chord.RecognizeChordFromScore(s1);
            c14 = Chord.RecognizeChordFromScore(s1, true);
            Console.WriteLine("1 harmonic: " + c11.root + "" + c11.type + ", 4 harmonics: " + c14.root + "" + c14.type);

            s1 = new Score();
            s1.AddNote(Pitch(Chord.Root.A, 5), 127, 32, 0, 0, 0);
            s1.AddNote(Pitch(Chord.Root.G, 5), 127, 16, 0, 32, 0);
            s1.AddNote(Pitch(Chord.Root.C, 6), 127, 16, 0, 48, 0);

            s1.AddNote(Pitch(Chord.Root.C, 4), 127, 8, 0, 0, 1);
            s1.AddNote(Pitch(Chord.Root.A, 4), 127, 8, 0, 8, 1);
            s1.AddNote(Pitch(Chord.Root.F, 4), 127, 8, 0, 16, 1);
            s1.AddNote(Pitch(Chord.Root.A, 4), 127, 8, 0, 24, 1);
            s1.AddNote(Pitch(Chord.Root.C, 4), 127, 8, 0, 32, 1);
            s1.AddNote(Pitch(Chord.Root.G, 4), 127, 8, 0, 40, 1);
            s1.AddNote(Pitch(Chord.Root.E, 4), 127, 8, 0, 48, 1);
            s1.AddNote(Pitch(Chord.Root.G, 4), 127, 8, 0, 56, 1);

            c11 = Chord.RecognizeChordFromScore(s1);
            c14 = Chord.RecognizeChordFromScore(s1, true);
            Console.WriteLine("1 harmonic: " + c11.root + "" + c11.type + ", 4 harmonics: " + c14.root + "" + c14.type);

            s1 = new Score();
            s1.AddNote(Pitch(Chord.Root.G, 5), 127, 16, 0, 0, 0);
            s1.AddNote(Pitch(Chord.Root.F, 5), 127, 4, 0, 16, 0);
            s1.AddNote(Pitch(Chord.Root.G, 6), 127, 2, 0, 20, 0);
            s1.AddNote(Pitch(Chord.Root.F, 6), 127, 2, 0, 22, 0);
            s1.AddNote(Pitch(Chord.Root.E, 6), 127, 4, 0, 24, 0);
            s1.AddNote(Pitch(Chord.Root.F, 6), 127, 4, 0, 28, 0);
            s1.AddNote(Pitch(Chord.Root.E, 6), 127, 16, 0, 32, 0);

            s1.AddNote(Pitch(Chord.Root.B, 3), 127, 8, 0, 0, 1);
            s1.AddNote(Pitch(Chord.Root.G, 4), 127, 8, 0, 8, 1);
            s1.AddNote(Pitch(Chord.Root.D, 4), 127, 8, 0, 16, 1);
            s1.AddNote(Pitch(Chord.Root.G, 4), 127, 8, 0, 24, 1);
            s1.AddNote(Pitch(Chord.Root.C, 4), 127, 8, 0, 32, 1);
            s1.AddNote(Pitch(Chord.Root.G, 4), 127, 8, 0, 40, 1);
            s1.AddNote(Pitch(Chord.Root.E, 4), 127, 8, 0, 48, 1);
            s1.AddNote(Pitch(Chord.Root.G, 4), 127, 8, 0, 56, 1);

            c11 = Chord.RecognizeChordFromScore(s1);
            c14 = Chord.RecognizeChordFromScore(s1, true);
            Console.WriteLine("1 harmonic: " + c11.root + "" + c11.type + ", 4 harmonics: " + c14.root + "" + c14.type);

            //
            Console.WriteLine();


            s1 = new Score();
            s1.AddNote(Pitch(Chord.Root.F, 5), 127, 32, 0, 0, 0);
            s1.AddNote(Pitch(Chord.Root.Ab, 5), 127, 32, 0, 16, 0);
            s1.AddNote(Pitch(Chord.Root.Db, 6), 127, 24, 0, 20, 0);
            s1.AddNote(Pitch(Chord.Root.F, 6), 127, 24, 0, 22, 0);

            s1.AddNote(Pitch(Chord.Root.F, 4), 127, 64, 0, 8, 1);
            s1.AddNote(Pitch(Chord.Root.Ab, 4), 127, 64, 0, 8, 1);

            c11 = Chord.RecognizeChordFromScore(s1);
            c14 = Chord.RecognizeChordFromScore(s1, true);
            Console.WriteLine("1 harmonic: " + c11.root + "" + c11.type + ", 4 harmonics: " + c14.root + "" + c14.type);

            s1 = new Score();
            s1.AddNote(Pitch(Chord.Root.Db, 6), 127, 8, 0, 20, 0);
            s1.AddNote(Pitch(Chord.Root.F, 6), 127, 8, 0, 22, 0);
            s1.AddNote(Pitch(Chord.Root.C, 6), 127, 8, 0, 20, 0);
            s1.AddNote(Pitch(Chord.Root.Eb, 6), 127, 8, 0, 22, 0);
            s1.AddNote(Pitch(Chord.Root.Db, 6), 127, 8, 0, 20, 0);
            s1.AddNote(Pitch(Chord.Root.F, 6), 127, 8, 0, 22, 0);
            s1.AddNote(Pitch(Chord.Root.C, 6), 127, 48, 0, 20, 0);
            s1.AddNote(Pitch(Chord.Root.Eb, 6), 127, 48, 0, 22, 0);

            s1.AddNote(Pitch(Chord.Root.Gb, 4), 127, 72, 0, 8, 1);
            s1.AddNote(Pitch(Chord.Root.A, 4), 127, 72, 0, 8, 1);

            c11 = Chord.RecognizeChordFromScore(s1);
            c14 = Chord.RecognizeChordFromScore(s1, true);
            Console.WriteLine("1 harmonic: " + c11.root + "" + c11.type + ", 4 harmonics: " + c14.root + "" + c14.type);

            s1 = new Score();
            s1.AddNote(Pitch(Chord.Root.C, 6), 127, 8, 0, 20, 0);
            s1.AddNote(Pitch(Chord.Root.Eb, 6), 127, 8, 0, 22, 0);
            s1.AddNote(Pitch(Chord.Root.Bb, 5), 127, 8, 0, 20, 0);
            s1.AddNote(Pitch(Chord.Root.Db, 6), 127, 8, 0, 22, 0);
            s1.AddNote(Pitch(Chord.Root.C, 6), 127, 8, 0, 20, 0);
            s1.AddNote(Pitch(Chord.Root.Eb, 6), 127, 8, 0, 22, 0);
            s1.AddNote(Pitch(Chord.Root.Bb, 5), 127, 12, 0, 20, 0);
            s1.AddNote(Pitch(Chord.Root.Db, 6), 127, 12, 0, 22, 0);
            s1.AddNote(Pitch(Chord.Root.F, 6), 127, 24, 0, 22, 0);
            s1.AddNote(Pitch(Chord.Root.Db, 6), 127, 12, 0, 22, 0);

            s1.AddNote(Pitch(Chord.Root.F, 4), 127, 72, 0, 8, 1);
            s1.AddNote(Pitch(Chord.Root.Ab, 4), 127, 72, 0, 8, 1);

            c11 = Chord.RecognizeChordFromScore(s1);
            c14 = Chord.RecognizeChordFromScore(s1, true);
            Console.WriteLine("1 harmonic: " + c11.root + "" + c11.type + ", 4 harmonics: " + c14.root + "" + c14.type);

            s1 = new Score();
            s1.AddNote(Pitch(Chord.Root.Db, 6), 127, 8, 0, 22, 0);
            s1.AddNote(Pitch(Chord.Root.C, 6), 127, 8, 0, 22, 0);
            s1.AddNote(Pitch(Chord.Root.Ab, 5), 127, 8, 0, 22, 0);
            s1.AddNote(Pitch(Chord.Root.Db, 6), 127, 8, 0, 22, 0);
            s1.AddNote(Pitch(Chord.Root.Bb, 5), 127, 8, 0, 22, 0);
            s1.AddNote(Pitch(Chord.Root.C, 6), 127, 48, 0, 22, 0);
            s1.AddNote(Pitch(Chord.Root.Ab, 5), 127, 48, 0, 22, 0);

            s1.AddNote(Pitch(Chord.Root.Eb, 4), 127, 72, 0, 8, 1);
            s1.AddNote(Pitch(Chord.Root.Gb, 4), 127, 72, 0, 8, 1);

            c11 = Chord.RecognizeChordFromScore(s1);
            c14 = Chord.RecognizeChordFromScore(s1, true);
            Console.WriteLine("1 harmonic: " + c11.root + "" + c11.type + ", 4 harmonics: " + c14.root + "" + c14.type);

        }

        private void RhythmPatternEditTest()
        {
            Console.WriteLine("RhythmPattern Edit Test");
            RhythmPattern rp = new RhythmPattern(0);
            int cost = 0;
            cost += rp.InsertNote(rp.noteList.Count, new RhythmPatternNote(16, 0));
            Console.WriteLine(cost);
            cost += rp.InsertNote(rp.noteList.Count, 16, -1);
            Console.WriteLine(cost);
            cost += rp.InsertNote(rp.noteList.Count, new RhythmPatternNote(8, -2));
            Console.WriteLine(cost);
            cost += rp.InsertNote(rp.noteList.Count, 8, -1);
            Console.WriteLine(cost);
            cost += rp.InsertNote(rp.noteList.Count, new RhythmPatternNote(8, 0));
            Console.WriteLine(cost);
            cost += rp.InsertNote(rp.noteList.Count, 8, -4);
            Console.WriteLine(cost);
            rp.Print();

            cost = 0;
            cost += rp.ReplaceNote(5, new RhythmPatternNote(8, 4));
            Console.WriteLine(cost);
            cost += rp.ReplaceNote(4, 8, 3);
            Console.WriteLine(cost);
            cost += rp.ReplaceNote(3, new RhythmPatternNote(8, 0));
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(1)");
            // 음표와 음표 사이에 삽입
            cost = rp.InsertNote(2, new RhythmPatternNote(16, rp.GetNewClusterNumber(1)));
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(2)");
            // 잘못된 인덱스에 삽입
            cost = rp.InsertNote(10, new RhythmPatternNote(24, rp.GetNewClusterNumber(3)));
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(3)");
            // 인덱스는 유지하고 길이를 바꾸는 교체 (맨 앞 음표라서 클러스터 순위 변경 비용이 없음)
            cost = rp.ReplaceNote(0, new RhythmPatternNote(4, rp.GetNewClusterNumber(0)));
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(4)");
            // 맨 앞에 삽입
            cost = rp.InsertNote(0, new RhythmPatternNote(4, 1));
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(5)");
            // 직전 음과 같은 음 높이를 갖는 음표 삽입
            cost = rp.InsertNote(1, new RhythmPatternNote(4, 1));
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(6)");
            // 없는 음표로 교체
            cost = rp.ReplaceNote(0, new RhythmPatternNote(0, 0));
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(7)");
            // 없는 음표를 교체 1
            cost = rp.ReplaceNote(-1, new RhythmPatternNote(1, 3));
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(8)");
            // 없는 음표를 교체 2
            cost = rp.ReplaceNote(10, new RhythmPatternNote(1, 3));
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(9)");
            // 같은 음표로 교체
            cost = rp.ReplaceNote(0, new RhythmPatternNote(4, 1));
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(10)");
            // 길이와 음 높이를 모두 바꾸도록 교체 1 (맨 앞 음표라서 클러스터 순위 변경 비용이 없음)
            cost = rp.ReplaceNote(0, new RhythmPatternNote(1, 3));
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(11)");
            // 길이와 음 높이를 모두 바꾸도록 교체 2
            cost = rp.ReplaceNote(3, new RhythmPatternNote(15, rp.GetExistingClusterNumber(0)));
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(12)");
            // 직후 음표와 길이가 같고 클러스터가 다르도록 교체 (맨 앞 음표라서 클러스터 순위 변경 비용이 없음)
            cost = rp.ReplaceNote(2, new RhythmPatternNote(15, rp.GetExistingClusterNumber(1)));
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(13)");
            // 음 높이 변화와 길이를 유지하고 클러스터 번호를 바꾸지만 클러스터 순위를 바꾸지 않도록 교체
            // (기존 클러스터가 사라지지 않음)
            cost = rp.ReplaceNote(4, 16, -1);
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(14)");
            // 음 높이 변화와 길이를 유지하면서 클러스터 번호를 바꾸지만 클러스터 순위를 바꾸지 않도록 교체
            // (기존 클러스터는 사라짐)
            cost = rp.ReplaceNote(4, 16, 1);
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(15)");
            // 음 높이 변화는 유지하면서 클러스터 순위만 다르도록 교체
            cost = rp.ReplaceNote(4, 16, 3);
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(16)");
            // 중간에 있는 기존 음표 제거
            cost = rp.DeleteNote(4);
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(17)");
            // 맨 끝의 기존 음표 제거
            cost = rp.DeleteNote(7);
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(18)");
            // 맨 앞의 기존 음표 제거
            cost = rp.DeleteNote(0);
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(19)");
            // 맨 처음 쉼표 길이 조정 1
            cost = rp.DelayNotes(8);
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(20)");
            // 제거했던 위치에 새로운 음표 삽입
            cost = rp.InsertNote(0, 8, 1);
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(21)");
            // 직전 음표와 같은 음을 갖는 음표 제거
            cost = rp.DeleteNote(1);
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(22)");
            // 없는 음표 제거
            cost = rp.DeleteNote(22);
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(23)");
            // 맨 처음 쉼표 길이 조정 2
            cost = rp.DelayNotes(4);
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(24)");
            // 맨 처음 쉼표 길이를 잘못되게 조정
            cost = rp.DelayNotes(-1);
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(25)");
            // 맨 처음 쉼표 길이를 이전 상태와 같게 조정
            cost = rp.DelayNotes(4);
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(26)");
            // 비용이 2가 되도록 연산 수행
            cost = rp.ReplaceNote(5, 8, -2);
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(27)");
            // 맨 처음 쉼표 길이 조정 3
            cost = rp.DelayNotes(0);
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(28)");
            // 연산 정보를 이용해 음표와 음표 사이에 삽입
            RhythmPattern.OperationInfo op1 = new RhythmPattern.OperationInfo(
                RhythmPattern.OperationInfo.Type.Insert, 3, new RhythmPatternNote(),
                new RhythmPatternNote(2, rp.GetExistingClusterNumber(0)));
            cost = rp.PerformOperation(op1);
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(29)");
            // 연산 정보를 이용해 음표 제거
            RhythmPattern.OperationInfo op2 = new RhythmPattern.OperationInfo(
                RhythmPattern.OperationInfo.Type.Delete, 2,
                new RhythmPatternNote(15, rp.GetExistingClusterNumber(0)), new RhythmPatternNote());
            cost = rp.PerformOperation(op2);
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(30)");
            // 연산 정보를 이용해 음표 교체
            RhythmPattern.OperationInfo op3 = new RhythmPattern.OperationInfo(
                RhythmPattern.OperationInfo.Type.Replace, 1,
                new RhythmPatternNote(15, -2), new RhythmPatternNote(16, 2));
            cost = rp.PerformOperation(op3);
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(31)");
            // 연산 정보를 이용해 리듬 패턴의 맨 앞 쉼표 길이 조정
            RhythmPattern.OperationInfo op4 = new RhythmPattern.OperationInfo(0, 16);
            cost = rp.PerformOperation(op4);
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(32)");
            // 쉼표 길이를 조정하는 연산의 역연산 수행
            cost = rp.PerformOperation(op4.Inverse());
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(33)");
            // 음표를 교체하는 연산의 역연산 수행
            cost = rp.PerformOperation(op3.Inverse());
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(34)");
            // 음표를 제거하는 연산의 역연산 수행
            cost = rp.PerformOperation(op2.Inverse());
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(35)");
            // 음표를 삽입하는 연산의 역연산 수행
            cost = rp.PerformOperation(op1.Inverse());
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(36)");
            // 상황에 맞지 않는 연산 정보 1
            op1 = new RhythmPattern.OperationInfo(RhythmPattern.OperationInfo.Type.Replace,
                5, new RhythmPatternNote(16, -2), new RhythmPatternNote(16, -3));
            cost = rp.PerformOperation(op1);
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(37)");
            // 상황에 맞지 않는 연산 정보 2
            op1 = new RhythmPattern.OperationInfo(RhythmPattern.OperationInfo.Type.Delete,
                5, new RhythmPatternNote(16, -2), new RhythmPatternNote());
            cost = rp.PerformOperation(op1);
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(38)");
            // 상황에 맞지 않는 연산 정보 3
            op1 = new RhythmPattern.OperationInfo(4, 0);
            cost = rp.PerformOperation(op1);
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(39)");
            // 상황에 맞지 않는 연산 정보 4
            op1 = new RhythmPattern.OperationInfo(RhythmPattern.OperationInfo.Type.Delete,
                5, new RhythmPatternNote(8, -3), new RhythmPatternNote());
            cost = rp.PerformOperation(op1);
            Console.WriteLine(cost);
            rp.Print();
        }

        private void RhythmPatternDistanceTest()
        {
            Console.WriteLine("RhythmPattern Distance Test");

            RhythmPattern rp = new RhythmPattern(0);
            int cost = 0;
            rp.InsertNote(0, 16, 0);
            rp.InsertNote(1, 16, -1);
            rp.InsertNote(2, 8, -2);
            rp.InsertNote(3, 8, -1);
            rp.InsertNote(4, 8, 0);
            rp.InsertNote(5, 8, -4);
            rp.Print();

            Console.WriteLine("(1)");
            RhythmPattern rp2 = rp.Copy();
            cost = rp2.ReplaceNote(5, new RhythmPatternNote(8, 4));
            Console.WriteLine(cost);
            cost = rp2.ReplaceNote(4, 8, 3);
            Console.WriteLine(cost);
            cost = rp2.ReplaceNote(3, new RhythmPatternNote(8, 0));
            Console.WriteLine(cost);

            rp.Print();
            Console.WriteLine();
            rp2.Print();
            Console.WriteLine("distance: " + rp.DistanceWithDirection(rp2));
            Console.WriteLine("inverse distance: " + rp2.DistanceWithDirection(rp));

            Console.WriteLine("(2)");
            rp2 = rp.Copy();
            cost = rp2.ReplaceNote(4, 8, 3);
            Console.WriteLine(cost);
            cost = rp2.ReplaceNote(3, 12, 1);
            Console.WriteLine(cost);
            cost = rp2.ReplaceNote(1, 16, 3);
            Console.WriteLine(cost);

            rp.Print();
            Console.WriteLine();
            rp2.Print();
            Console.WriteLine("distance: " + rp.DistanceWithDirection(rp2));
            Console.WriteLine("inverse distance: " + rp2.DistanceWithDirection(rp));

            Console.WriteLine("(3)");
            RhythmPattern rp3 = rp2.Copy();
            rp2 = rp3.Copy();
            cost = rp2.InsertNote(5, 8, -2);
            Console.WriteLine(cost);
            cost = rp2.DeleteNote(2);
            Console.WriteLine(cost);
            cost = rp2.ReplaceNote(3, 8, 3);
            Console.WriteLine(cost);
            cost = rp2.ReplaceNote(1, 24, 1);
            Console.WriteLine(cost);

            rp3.Print();
            Console.WriteLine();
            rp2.Print();
            Console.WriteLine("distance: " + rp3.DistanceWithDirection(rp2));
            Console.WriteLine("inverse distance: " + rp2.DistanceWithDirection(rp3));

            Console.WriteLine("(4)");
            rp2 = rp.Copy();
            cost = rp2.InsertNote(5, 8, -2);
            Console.WriteLine(cost);
            cost = rp2.DeleteNote(2);
            Console.WriteLine(cost);
            cost = rp2.ReplaceNote(3, 8, 3);
            Console.WriteLine(cost);
            cost = rp2.ReplaceNote(1, 24, 1);
            Console.WriteLine(cost);

            rp.Print();
            Console.WriteLine();
            rp2.Print();
            Console.WriteLine("distance: " + rp.DistanceWithDirection(rp2));
            Console.WriteLine("inverse distance: " + rp2.DistanceWithDirection(rp));

            Console.WriteLine("(5)");
            rp = new RhythmPattern(0,
                new RhythmPatternNote(8, 0),
                new RhythmPatternNote(6, 0),
                new RhythmPatternNote(2, -2),
                new RhythmPatternNote(4, 0),
                new RhythmPatternNote(8, 4),
                new RhythmPatternNote(8, 3),
                new RhythmPatternNote(8, 1),
                new RhythmPatternNote(8, 0),
                new RhythmPatternNote(4, 1),
                new RhythmPatternNote(4, 0),
                new RhythmPatternNote(4, -1));
            rp2 = new RhythmPattern(0,
                new RhythmPatternNote(8, -2),
                new RhythmPatternNote(6, -2),
                new RhythmPatternNote(2, -2),
                new RhythmPatternNote(4, -2),
                new RhythmPatternNote(8, 0),
                new RhythmPatternNote(36, -1));
            rp.Print();
            Console.WriteLine();
            rp2.Print();
            Console.WriteLine("distance: " + rp.DistanceWithDirection(rp2));
            rp2.Print();
            Console.WriteLine();
            rp.Print();
            Console.WriteLine("inverse distance: " + rp2.DistanceWithDirection(rp));

            Console.WriteLine("(6)");
            rp2 = new RhythmPattern(0,
                new RhythmPatternNote(4, -2),
                new RhythmPatternNote(4, 0),
                new RhythmPatternNote(4, 2),
                new RhythmPatternNote(8, 5),
                new RhythmPatternNote(12, 7),
                new RhythmPatternNote(4, -1),
                new RhythmPatternNote(4, 1),
                new RhythmPatternNote(4, 3),
                new RhythmPatternNote(8, 6),
                new RhythmPatternNote(12, 5));
            rp.Print();
            Console.WriteLine();
            rp2.Print();
            Console.WriteLine("distance: " + rp.DistanceWithDirection(rp2));
            Console.WriteLine("inverse distance: " + rp2.DistanceWithDirection(rp));

            Console.WriteLine("(7)");
            rp2 = new RhythmPattern(0,
                new RhythmPatternNote(8, -1),
                new RhythmPatternNote(6, -1),
                new RhythmPatternNote(2, -3),
                new RhythmPatternNote(4, -1),
                new RhythmPatternNote(8, 3),
                new RhythmPatternNote(8, 2),
                new RhythmPatternNote(8, 0),
                new RhythmPatternNote(8, -1),
                new RhythmPatternNote(4, 0),
                new RhythmPatternNote(4, -1),
                new RhythmPatternNote(4, -3));
            rp.Print();
            Console.WriteLine();
            rp2.Print();
            Console.WriteLine("distance: " + rp.DistanceWithDirection(rp2));
            Console.WriteLine("inverse distance: " + rp2.DistanceWithDirection(rp));
            Console.WriteLine("min: " + rp.Distance(rp2));

            Console.WriteLine("(8)");
            rp = new RhythmPattern(0);
            rp.InsertNote(0, 16, 0);
            rp.InsertNote(1, 16, -1);
            rp.InsertNote(2, 8, -2);
            rp.InsertNote(3, 8, -1);
            rp.InsertNote(4, 8, 0);
            rp.InsertNote(5, 8, -4);
            rp2 = new RhythmPattern(0,
                new RhythmPatternNote(4, -2),
                new RhythmPatternNote(4, 0),
                new RhythmPatternNote(4, 2),
                new RhythmPatternNote(8, 5),
                new RhythmPatternNote(12, 7),
                new RhythmPatternNote(4, -1),
                new RhythmPatternNote(4, 1),
                new RhythmPatternNote(4, 3),
                new RhythmPatternNote(8, 6),
                new RhythmPatternNote(12, 5));
            rp.Print();
            Console.WriteLine();
            rp2.Print();
            Console.WriteLine("distance: " + rp.DistanceWithDirection(rp2));
            Console.WriteLine("inverse distance: " + rp2.DistanceWithDirection(rp));
            Console.WriteLine("min: " + rp.Distance(rp2));

            Console.WriteLine("(9)");
            rp = new RhythmPattern(0,
                new RhythmPatternNote(8, 0),
                new RhythmPatternNote(8, 0));
            rp2 = new RhythmPattern(4,
                new RhythmPatternNote(4, 0),
                new RhythmPatternNote(4, 1),
                new RhythmPatternNote(4, 0));
            rp.Print();
            Console.WriteLine();
            rp2.Print();
            Console.WriteLine("distance: " + rp.DistanceWithDirection(rp2));
            Console.WriteLine("inverse distance: " + rp2.DistanceWithDirection(rp));
            Console.WriteLine("min: " + rp.Distance(rp2));
        }
    }
}
