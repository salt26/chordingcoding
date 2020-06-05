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
            //RhythmPatternEditTest2();
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
            RhythmPattern rp = new RhythmPattern();
            int cost = 0;
            cost += rp.InsertNote(new RhythmPatternNote(0, 0));
            Console.WriteLine(cost);
            cost += rp.InsertNote(16, -1);
            Console.WriteLine(cost);
            cost += rp.InsertNote(new RhythmPatternNote(32, -2));
            Console.WriteLine(cost);
            cost += rp.InsertNote(40, -1);
            Console.WriteLine(cost);
            cost += rp.InsertNote(new RhythmPatternNote(48, 0));
            Console.WriteLine(cost);
            cost += rp.InsertNote(56, -4);
            Console.WriteLine(cost);
            rp.Print();

            cost = 0;
            cost += rp.MoveNote(56, new RhythmPatternNote(56, 4));
            Console.WriteLine(cost);
            cost += rp.MoveNote(48, 48, 3);
            Console.WriteLine(cost);
            cost += rp.MoveNote(40, new RhythmPatternNote(40, 0));
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(1)");
            // 음표와 음표 사이에 삽입
            cost = rp.InsertNote(new RhythmPatternNote(24, rp.GetNewClusterNumber(1)));
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(2)");
            // 이미 음표가 있는 위치에 삽입
            cost = rp.InsertNote(new RhythmPatternNote(24, rp.GetNewClusterNumber(3)));
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(3)");
            // 인덱스는 유지하고 onset을 바꾸는 옮기기
            cost = rp.MoveNote(0, new RhythmPatternNote(8, rp.GetNewClusterNumber(0)));
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(4)");
            // 맨 앞에 삽입
            cost = rp.InsertNote(new RhythmPatternNote(0, 1));
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(5)");
            // 직전 음과 같은 음 높이를 갖는 음표 삽입
            cost = rp.InsertNote(new RhythmPatternNote(4, 1));
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(6)");
            // 인덱스를 넘어서서 옮기기
            cost = rp.MoveNote(0, new RhythmPatternNote(7, 0));
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(7)");
            // 없는 음표를 옮기기 1
            cost = rp.MoveNote(0, new RhythmPatternNote(1, 3));
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(8)");
            // 없는 음표를 옮기기 2
            cost = rp.MoveNote(1, new RhythmPatternNote(1, 3));
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(9)");
            // 같은 음표로 옮기기
            cost = rp.MoveNote(0, new RhythmPatternNote(0, 1));
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(10)");
            // Onset과 음 높이를 모두 바꾸도록 옮기기 1
            cost = rp.MoveNote(0, new RhythmPatternNote(1, 3));
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(11)");
            // Onset과 음 높이를 모두 바꾸도록 옮기기 2
            cost = rp.MoveNote(16, new RhythmPatternNote(15, rp.GetExistingClusterNumber(0)));
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(12)");
            // 이웃한 다른 음표와 Onset이 같고 클러스터가 다르도록 옮기기
            cost = rp.MoveNote(15, new RhythmPatternNote(24, rp.GetExistingClusterNumber(0)));
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(13)");
            // 음 높이 변화를 유지하고 클러스터 번호를 바꾸지만 클러스터 위상을 바꾸지 않도록 옮기기
            cost = rp.MoveNote(24, 26, rp.GetNewClusterNumber(2));
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(14)");
            // 음 높이 변화는 유지하면서 클러스터 위상만 다르도록 옮기기 (기존 클러스터는 사라짐)
            cost = rp.MoveNote(26, 26, rp.GetExistingClusterNumber(4));
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(15)");
            // 중간에 있는 기존 음표 제거
            cost = rp.DeleteNote(26);
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(16)");
            // 맨 끝의 기존 음표 제거
            cost = rp.DeleteNote(56);
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(17)");
            // 맨 앞의 기존 음표 제거
            cost = rp.DeleteNote(1);
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(18)");
            // 직전 음표와 같은 음을 갖는 음표 제거
            cost = rp.DeleteNote(15);
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(19)");
            // 없는 음표 제거
            cost = rp.DeleteNote(19);
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(20)");
            // 제거했던 위치에 새로운 음표 삽입
            cost = rp.InsertNote(15, -4);
            Console.WriteLine(cost);
            rp.Print();
        }

        private void RhythmPatternEditTest2()
        {
            Console.WriteLine("RhythmPattern Edit Test 2");
            RhythmPattern rp = new RhythmPattern();
            int cost = 0;
            cost += rp.InsertNote(new RhythmPatternNote(0, 0));
            Console.WriteLine(cost);
            cost += rp.InsertNote(16, -1);
            Console.WriteLine(cost);
            cost += rp.InsertNote(new RhythmPatternNote(32, -2));
            Console.WriteLine(cost);
            cost += rp.InsertNote(40, -1);
            Console.WriteLine(cost);
            cost += rp.InsertNote(new RhythmPatternNote(48, 0));
            Console.WriteLine(cost);
            cost += rp.InsertNote(56, -4);
            Console.WriteLine(cost);
            rp.Print();

            cost = 0;
            cost += rp.MoveNote(56, new RhythmPatternNote(56, 4));
            Console.WriteLine(cost);
            cost += rp.MoveNote(48, 48, 3);
            Console.WriteLine(cost);
            cost += rp.MoveNote(40, new RhythmPatternNote(40, 0));
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(1*)");
            // 음표와 음표 사이에 삽입
            RhythmPattern.OperationInfo op = new RhythmPattern.OperationInfo(
                RhythmPattern.OperationInfo.Type.Insert, new RhythmPatternNote(),
                new RhythmPatternNote(24, rp.GetNewClusterNumber(1)));
            cost = rp.PerformOperation(op);
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(2*)");
            // 이미 음표가 있는 위치에 삽입
            op = new RhythmPattern.OperationInfo(
                RhythmPattern.OperationInfo.Type.Insert, new RhythmPatternNote(),
                new RhythmPatternNote(24, rp.GetNewClusterNumber(3)));
            cost = rp.PerformOperation(op);
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(3*^)");
            // 인덱스는 유지하고 onset을 바꾸는 옮기기
            op = new RhythmPattern.OperationInfo(
                RhythmPattern.OperationInfo.Type.Move,
                new RhythmPatternNote(8, rp.GetNewClusterNumber(0)), rp.noteList.First.Value);
            cost = rp.PerformOperation(op.Inverse());
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(4*^)");
            // 맨 앞에 삽입
            op = new RhythmPattern.OperationInfo(
                RhythmPattern.OperationInfo.Type.Delete,
                new RhythmPatternNote(0, 1), new RhythmPatternNote());
            cost = rp.PerformOperation(op.Inverse());
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(5)");
            // 직전 음과 같은 음 높이를 갖는 음표 삽입
            cost = rp.InsertNote(new RhythmPatternNote(4, 1));
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(6*)");
            // 인덱스를 넘어서서 옮기기
            op = new RhythmPattern.OperationInfo(
                RhythmPattern.OperationInfo.Type.Move, rp.noteList.First.Value,
                new RhythmPatternNote(7, 0));
            cost = rp.PerformOperation(op);
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(7)");
            // 없는 음표를 옮기기 1
            cost = rp.MoveNote(0, new RhythmPatternNote(1, 3));
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(8)");
            // 없는 음표를 옮기기 2
            cost = rp.MoveNote(1, new RhythmPatternNote(1, 3));
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(9)");
            // 같은 음표로 옮기기
            cost = rp.MoveNote(0, new RhythmPatternNote(0, 1));
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(10)");
            // Onset과 음 높이를 모두 바꾸도록 옮기기 1
            cost = rp.MoveNote(0, new RhythmPatternNote(1, 3));
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(11)");
            // Onset과 음 높이를 모두 바꾸도록 옮기기 2
            cost = rp.MoveNote(16, new RhythmPatternNote(15, rp.GetExistingClusterNumber(0)));
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(12)");
            // 이웃한 다른 음표와 Onset이 같고 클러스터가 다르도록 옮기기
            cost = rp.MoveNote(15, new RhythmPatternNote(24, rp.GetExistingClusterNumber(0)));
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(13)");
            // 음 높이 변화를 유지하고 클러스터 번호를 바꾸지만 클러스터 위상을 바꾸지 않도록 옮기기
            cost = rp.MoveNote(24, 26, rp.GetNewClusterNumber(2));
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(14)");
            // 음 높이 변화는 유지하면서 클러스터 위상만 다르도록 옮기기 (기존 클러스터는 사라짐)
            cost = rp.MoveNote(26, 26, rp.GetExistingClusterNumber(4));
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(15*)");
            // 중간에 있는 기존 음표 제거
            op = new RhythmPattern.OperationInfo(
                RhythmPattern.OperationInfo.Type.Delete, new RhythmPatternNote(26, rp.GetExistingClusterNumber(3)),
                new RhythmPatternNote());
            cost = rp.PerformOperation(op);
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(16*^)");
            // 맨 끝의 기존 음표 제거
            op = new RhythmPattern.OperationInfo(
                RhythmPattern.OperationInfo.Type.Insert,
                new RhythmPatternNote(), new RhythmPatternNote(56, 4));
            cost = rp.PerformOperation(op.Inverse());
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(17)");
            // 맨 앞의 기존 음표 제거
            cost = rp.DeleteNote(1);
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(18)");
            // 직전 음표와 같은 음을 갖는 음표 제거
            cost = rp.DeleteNote(15);
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(19)");
            // 없는 음표 제거
            cost = rp.DeleteNote(19);
            Console.WriteLine(cost);
            rp.Print();

            Console.WriteLine("(20)");
            // 제거했던 위치에 새로운 음표 삽입
            cost = rp.InsertNote(15, -4);
            Console.WriteLine(cost);
            rp.Print();
        }

        private void RhythmPatternDistanceTest()
        {
            Console.WriteLine("RhythmPattern Distance Test");
            /*
            RhythmPattern rp = new RhythmPattern();
            int cost = 0;
            rp.Print();

            Console.WriteLine("(01)");
            RhythmPattern rp2 = rp.Copy();
            rp2.InsertNote(4, 0);
            rp2.Print();
            Console.WriteLine("distance(->): " + rp.Distance(rp2));
            Console.WriteLine("distance(<-): " + rp2.Distance(rp));
            */

            RhythmPattern rp = new RhythmPattern();
            int cost = 0;
            rp.InsertNote(0, 0);
            rp.InsertNote(16, -1);
            rp.InsertNote(32, -2);
            rp.InsertNote(40, -1);
            rp.InsertNote(48, 0);
            rp.InsertNote(56, -4);
            rp.Print();

            Console.WriteLine("(1)");
            RhythmPattern rp2 = rp.Copy();
            cost = rp2.MoveNote(56, new RhythmPatternNote(56, 4));
            Console.WriteLine(cost);
            cost = rp2.MoveNote(48, 48, 3);
            Console.WriteLine(cost);
            cost = rp2.MoveNote(40, new RhythmPatternNote(40, 0));
            Console.WriteLine(cost);

            rp.Print();
            Console.WriteLine();
            rp2.Print();
            Console.WriteLine("distance: " + rp.Distance(rp2));

            Console.WriteLine("(2)");
            rp2 = rp.Copy();
            cost = rp2.MoveNote(48, 48, 3);
            Console.WriteLine(cost);
            cost = rp2.MoveNote(40, 36, 1);
            Console.WriteLine(cost);
            cost = rp2.MoveNote(16, 16, 3);
            Console.WriteLine(cost);

            rp.Print();
            Console.WriteLine();
            rp2.Print();
            Console.WriteLine("distance: " + rp.Distance(rp2));

            Console.WriteLine("(3)");
            rp2 = rp.Copy();
            cost = rp2.InsertNote(60, -2);
            Console.WriteLine(cost);
            cost = rp2.MoveNote(40, 36, 0);
            Console.WriteLine(cost);
            cost = rp2.DeleteNote(32);
            Console.WriteLine(cost);
            cost = rp2.MoveNote(16, 8, 1);
            Console.WriteLine(cost);

            rp.Print();
            Console.WriteLine();
            rp2.Print();
            Console.WriteLine("distance: " + rp.Distance(rp2));

            /*
            Console.WriteLine("(4)");
            rp2 = new RhythmPattern(
                new RhythmPatternNote(0, 0),
                new RhythmPatternNote(8, -1),
                new RhythmPatternNote(16, -2),
                new RhythmPatternNote(20, -1),
                new RhythmPatternNote(24, 0),
                new RhythmPatternNote(28, -4)
                );

            rp.Print();
            Console.WriteLine();
            rp2.Print();
            Console.WriteLine("distance: " + rp.Distance(rp2));

            Console.WriteLine("(4-1)");
            RhythmPattern rp3 = new RhythmPattern(
                new RhythmPatternNote(0, 0),
                new RhythmPatternNote(16, -1),
                new RhythmPatternNote(32, -2)
                );

            rp2 = new RhythmPattern(
                new RhythmPatternNote(0, 0),
                new RhythmPatternNote(8, -1),
                new RhythmPatternNote(16, -2)
                );

            rp3.Print();
            Console.WriteLine();
            rp2.Print();
            Console.WriteLine("distance: " + rp3.Distance(rp2));

            Console.WriteLine("(5)");
            rp2 = new RhythmPattern(
                new RhythmPatternNote(0, 0),
                new RhythmPatternNote(32, -1),
                new RhythmPatternNote(64, -2),
                new RhythmPatternNote(80, -1),
                new RhythmPatternNote(96, 0),
                new RhythmPatternNote(112, -4)
                );

            rp.Print();
            Console.WriteLine();
            rp2.Print();
            Console.WriteLine("distance: " + rp.Distance(rp2));

            Console.WriteLine("(6)");
            rp2 = rp.Copy();
            cost = rp2.DeleteNote(0);
            Console.WriteLine(cost);
            cost = rp2.DeleteNote(40);
            Console.WriteLine(cost);
            cost = rp2.MoveNote(48, 52, -4);
            Console.WriteLine(cost);

            rp.Print();
            Console.WriteLine();
            rp2.Print();
            Console.WriteLine("distance: " + rp.Distance(rp2));

            Console.WriteLine("(7)");
            rp2 = new RhythmPattern(
                new RhythmPatternNote(0, 0),
                new RhythmPatternNote(4, -3),
                new RhythmPatternNote(8, -1),
                new RhythmPatternNote(12, -3),
                new RhythmPatternNote(16, -3),
                new RhythmPatternNote(20, -1),
                new RhythmPatternNote(24, -3),
                new RhythmPatternNote(28, 0)
                );

            rp.Print();
            Console.WriteLine();
            rp2.Print();
            Console.WriteLine("distance: " + rp.Distance(rp2));
            */
        }
    }
}
