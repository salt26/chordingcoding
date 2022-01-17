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
using System.Linq;
using ChordingCoding.Utility;
using ChordingCoding.Word;

namespace ChordingCoding.Sentiment
{
    public class SentimentState
    {
        public enum Valence
        {
            Low = 0, Medium = 1, High = 2, NULL = -1
        };

        public enum Arousal
        {
            Low = 0, Medium = 1, High = 2, NULL = -1
        };

        public static bool IsReady
        {
            get;
            private set;
        } = false;

        private static List<Valence> valenceState;
        private static List<Arousal> arousalState;
        private const int CAPACITY = 10;

        public static void Initialize()
        {
            void InitializeState(object[] args)
            {
                valenceState = new List<Valence>();
                arousalState = new List<Arousal>();
                IsReady = true;
            }
            Util.TaskQueue.Add("SentimentState", InitializeState);
        }

        public static void UpdateState(Valence v, Arousal a)
        {
            if (!IsReady) return;
            else if (v == Valence.NULL && a == Arousal.NULL) return;

            if (v == Valence.NULL) v = Valence.Medium;
            if (a == Arousal.NULL) a = Arousal.Medium;

            void Enqueue(object[] args)
            {
                if (valenceState.Count >= CAPACITY)
                {
                    valenceState.RemoveAt(0);
                }
                if (arousalState.Count >= CAPACITY)
                {
                    arousalState.RemoveAt(0);
                }
                valenceState.Add((Valence)args[0]);
                arousalState.Add((Arousal)args[1]);
            }

            Util.TaskQueue.Add("SentimentState", Enqueue, v, a);
        }

        public static void UpdateState(WordSentiment wordSentiment)
        {
            UpdateState(wordSentiment.GetValence(), wordSentiment.GetArousal());
        }

        public static Valence GetShortTermValence()
        {
            Valence v = Valence.NULL;
            if (!IsReady) return v;

            void ShortValence(object[] args)
            {
                if (valenceState.Count > 0)
                    v = valenceState.Last();
                else
                    v = Valence.NULL;
            }

            Util.TaskQueue.Add("SentimentState", ShortValence);
            return v;
        }

        public static Valence GetShortTermPrevValence()
        {
            Valence v = Valence.NULL;
            if (!IsReady) return v;

            void ShortPrevValence(object[] args)
            {
                if (valenceState.Count > 1)
                    v = valenceState[valenceState.Count - 2];
                else
                    v = Valence.NULL;
            }

            Util.TaskQueue.Add("SentimentState", ShortPrevValence);
            return v;
        }

        public static Valence GetLongTermValence()
        {
            Valence v = Valence.NULL;
            if (!IsReady) return v;

            void LongValence(object[] args)
            {
                Random r = new Random();
                int[] aggregateValence = new int[3] { 0, 0, 0 };

                foreach (Valence valence in valenceState)
                {
                    aggregateValence[(int)valence]++;
                }

                if (aggregateValence.Sum() <= 0)
                {
                    v = Valence.NULL;
                }
                else
                {
                    int max = 0;
                    List<int> argmax = new List<int>();
                    for (int k = 0; k < aggregateValence.Length; k++)
                    {
                        if (aggregateValence[k] > max)
                        {
                            max = aggregateValence[k];
                            argmax = new List<int>() { k };
                        }
                        else if (aggregateValence[k] == max)
                        {
                            argmax.Add(k);
                        }
                    }
                    v = (Valence)argmax[r.Next(argmax.Count)];
                }
            }

            Util.TaskQueue.Add("SentimentState", LongValence);
            return v;
        }

        public static Arousal GetShortTermArousal()
        {
            Arousal a = Arousal.NULL;
            if (!IsReady) return a;

            void ShortArousal(object[] args)
            {
                if (arousalState.Count > 0)
                    a = arousalState.Last();
                else
                    a = Arousal.NULL;
            }

            Util.TaskQueue.Add("SentimentState", ShortArousal);
            return a;
        }

        public static Arousal GetShortTermPrevArousal()
        {
            Arousal a = Arousal.NULL;
            if (!IsReady) return a;

            void ShortPrevArousal(object[] args)
            {
                if (arousalState.Count > 1)
                    a = arousalState[arousalState.Count - 2];
                else
                    a = Arousal.NULL;
            }

            Util.TaskQueue.Add("SentimentState", ShortPrevArousal);
            return a;
        }

        public static Arousal GetLongTermArousal()
        {
            Arousal a = Arousal.NULL;
            if (!IsReady) return a;

            void LongArousal(object[] args)
            {
                Random r = new Random();
                int[] aggregateArousal = new int[3] { 0, 0, 0 };

                foreach (Arousal arousal in arousalState)
                {
                    aggregateArousal[(int)arousal]++;
                }

                if (aggregateArousal.Sum() <= 0)
                {
                    a = Arousal.NULL;
                }
                else
                {
                    int max = 0;
                    List<int> argmax = new List<int>();
                    for (int k = 0; k < aggregateArousal.Length; k++)
                    {
                        if (aggregateArousal[k] > max)
                        {
                            max = aggregateArousal[k];
                            argmax = new List<int>() { k };
                        }
                        else if (aggregateArousal[k] == max)
                        {
                            argmax.Add(k);
                        }
                    }
                    a = (Arousal)argmax[r.Next(argmax.Count)];
                }
            }

            Util.TaskQueue.Add("SentimentState", LongArousal);

            return a;
        }

        public static int SentimentIndexFromVA(Valence v, Arousal a)
        {
            if (v == Valence.NULL || a == Arousal.NULL)
            {
                return -1;
            }
            else
            {
                return 3 * (int)v + (int)a;
            }
        }
    }
}
