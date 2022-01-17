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
using ChordingCoding.Sentiment;

namespace ChordingCoding.SFX
{
    class MusicalKey
    {
        public enum Mode { Minor = 0, Major = 1 }

        public enum Tonic { C = 0, Db = 1, D = 2, Eb = 3, E = 4, F = 5,
                            Gb = 6, G = 7, Ab = 8, A = 9, Bb = 10, B = 11 }

        public const bool VERBOSE = true;

        public Mode mode;
        public Tonic tonic;

        public MusicalKey()
        {
            Random r = new Random();

            mode = Mode.Major;
            if (r.NextDouble() < 0.5) mode = Mode.Minor;

            tonic = (Tonic)r.Next(0, 12);
            if (VERBOSE)
                Console.WriteLine("Key: " + tonic.ToString() + " " + mode.ToString());
        }

        public MusicalKey(Mode mode, Tonic tonic)
        {
            this.mode = mode;
            this.tonic = tonic;
        }

        public void Transpose(SentimentState.Valence valence = SentimentState.Valence.NULL)
        {

            Random r = new Random();

            switch (valence)
            {
                case SentimentState.Valence.High:
                    mode = Mode.Major;
                    break;
                case SentimentState.Valence.Low:
                    mode = Mode.Minor;
                    break;
                default:
                    mode = Mode.Major;
                    if (r.NextDouble() < 0.5) mode = Mode.Minor;
                    break;
            }

            tonic = (Tonic)r.Next(0, 12);
            if (VERBOSE)
            {
                Console.WriteLine("Transpose key!");
                Console.WriteLine("Key: " + tonic.ToString() + " " + mode.ToString());
            }
        }
    }
}
