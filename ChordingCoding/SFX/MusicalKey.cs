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
using ChordingCoding.Utility;

namespace ChordingCoding.SFX
{
    class MusicalKey
    {
        public enum Mode { Minor = 0, Major = 1 }

        public enum Tonic { C = 0, Db = 1, D = 2, Eb = 3, E = 4, F = 5,
                            Gb = 6, G = 7, Ab = 8, A = 9, Bb = 10, B = 11 }

        public enum ModePolicy { FavorMinor = 0, FavorMajor = 1, Auto = 2 }

        public const bool VERBOSE = true;

        public Mode mode;
        public Tonic tonic;

        private ModePolicy _policy;

        public ModePolicy Policy
        {
            get
            {
                return _policy;
            }
            set
            {
                _policy = value;
                void ChangeMode(object[] args)
                {
                    Transpose();
                }
                Util.TaskQueue.Add("Play", ChangeMode);
            }
        }

        public MusicalKey(ModePolicy policy = ModePolicy.Auto)
        {
            Random r = new Random();

            Policy = policy;
            switch (Policy) {
                case ModePolicy.Auto:
                    mode = Mode.Major;
                    if (r.NextDouble() < 0.5) mode = Mode.Minor;
                    break;
                case ModePolicy.FavorMajor:
                    mode = Mode.Major;
                    break;
                case ModePolicy.FavorMinor:
                    mode = Mode.Minor;
                    break;
            }

            tonic = (Tonic)r.Next(0, 12);
            if (VERBOSE)
                Console.WriteLine("Key: " + tonic.ToString() + " " + mode.ToString());
        }

        public MusicalKey(Mode mode, Tonic tonic, ModePolicy policy = ModePolicy.Auto)
        {
            this.tonic = tonic;
            Policy = policy;

            switch (Policy)
            {
                case ModePolicy.Auto:
                    this.mode = mode;
                    break;
                case ModePolicy.FavorMajor:
                    this.mode = Mode.Major;
                    break;
                case ModePolicy.FavorMinor:
                    this.mode = Mode.Minor;
                    break;
            }
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
                    switch (Policy) {
                        case ModePolicy.Auto:
                            mode = Mode.Major;
                            if (r.NextDouble() < 0.5) mode = Mode.Minor;
                            break;
                        case ModePolicy.FavorMajor:
                            mode = Mode.Major;
                            break;
                        case ModePolicy.FavorMinor:
                            mode = Mode.Minor;
                            break;
                    }
                    break;
            }

            tonic = (Tonic)r.Next(0, 12);

            if (VERBOSE)
            {
                Console.WriteLine("Transpose key!");
                Console.WriteLine("Key: " + tonic.ToString() + " " + mode.ToString());
            }
        }

        public static ModePolicy IntToModePolicy(int policy)
        {
            switch (policy)
            {
                case 0:
                    return ModePolicy.FavorMinor;
                case 1:
                    return ModePolicy.FavorMajor;
                default:
                    return ModePolicy.Auto;
            }
        }
    }
}
