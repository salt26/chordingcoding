/*
MIT License

Copyright (c) 2019 Dantae An

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
    class ChordTransitionMatrix
    {
        public const bool VERBOSE = true;

        public List<List<List<double>>> basic;
        public List<List<List<List<double>>>> sentimental;
        public List<List<List<List<List<double>>>>> sentimentVariational;

        public List<double> GetBasicChordProbVector(MusicalKey.Mode mode, int prevChord)
        {
            return Normalize(basic[(int)mode][prevChord]);
        }

        public int SampleRomanNumeralFromBasic(MusicalKey.Mode mode, int prevChord)
        {
            List<double> vector = NormalizeAndCumulate(basic[(int)mode][prevChord]);

            Random r = new Random();
            double p = r.NextDouble();

            // Binary search
            int low = 0;
            int high = vector.Count - 1;
            while (low < high)
            {
                int mid = (low + high) / 2;
                if (mid == 0)
                {
                    if (VERBOSE)
                        Console.WriteLine(vector[0]);
                    return mid;
                }
                else if (vector[mid - 1] <= p && p < vector[mid])
                {
                    if (VERBOSE)
                        Console.WriteLine(vector[mid] - vector[mid - 1]);
                    return mid;
                }
                else if(vector[mid] <= p)
                {
                    low = mid + 1;
                }
                else
                {
                    high = mid - 1;
                }
            }
            if (VERBOSE)
            {
                if (low == 0)
                    Console.WriteLine(vector[0]);
                else
                    Console.WriteLine(vector[low] - vector[low - 1]);
            }
            return low;
        }

        public List<double> GetSentimentalChordProbVector(int currSentimentIndex, MusicalKey.Mode mode, int prevChord)
        {
            return Normalize(sentimental[currSentimentIndex][(int)mode][prevChord]);
        }

        public List<double> GetSentimentVariationalChordProbVector(int prevSentimentIndex, int currSentimentIndex, MusicalKey.Mode mode, int prevChord)
        {
            return Normalize(sentimentVariational[prevSentimentIndex][currSentimentIndex][(int)mode][prevChord]);
        }

        /*
        public List<double> RomanNumeralFromAll(int prevSentimentIndex, int currSentimentIndex, MusicalKey.Mode mode, int prevChord, int sentimentAwareness)
        {
            if (sentimentAwareness < 0) sentimentAwareness = 0;
            else if (sentimentAwareness > 99) sentimentAwareness = 99;

            List<double> vector = new List<double>();
            List<double> basicVector = GetBasicChordProbVector(mode, prevChord);
            List<double> sentimentalVector = GetSentimentalChordProbVector(currSentimentIndex, mode, prevChord);
            List<double> sentimentVariationalVector;
            if (prevSentimentIndex >= 0)
            {
                sentimentVariationalVector = GetSentimentVariationalChordProbVector(prevSentimentIndex, currSentimentIndex, mode, prevChord);
                for (int i = 0; i < basicVector.Count; i++)
                {
                    double interpolatedProb = (100 - sentimentAwareness) / 100f * basicVector[i] + sentimentAwareness / 200f * (sentimentalVector[i] + sentimentVariationalVector[i]);
                    vector.Add(interpolatedProb);
                }
            }
            else
            {
                for (int i = 0; i < basicVector.Count; i++)
                {
                    double interpolatedProb = (100 - sentimentAwareness) / 100f * basicVector[i] + sentimentAwareness / 100f * sentimentalVector[i];
                    vector.Add(interpolatedProb);
                }
            }
            vector = Normalize(vector);
            return vector;
        }
        */

        public int SampleRomanNumeralFromAll(int prevSentimentIndex, int currSentimentIndex, MusicalKey.Mode mode, int prevChord, int sentimentAwareness)
        {
            if (sentimentAwareness < 0) sentimentAwareness = 0;
            else if (sentimentAwareness > 99) sentimentAwareness = 99;

            if (currSentimentIndex < 0 || sentimentAwareness == 0)
            {
                return SampleRomanNumeralFromBasic(mode, prevChord);
            }

            List<double> vector = new List<double>();
            List<double> basicVector = GetBasicChordProbVector(mode, prevChord);
            List<double> sentimentalVector = GetSentimentalChordProbVector(currSentimentIndex, mode, prevChord);
            List<double> sentimentVariationalVector;
            if (prevSentimentIndex >= 0)
            {
                sentimentVariationalVector = GetSentimentVariationalChordProbVector(prevSentimentIndex, currSentimentIndex, mode, prevChord);
                for (int i = 0; i < basicVector.Count; i++)
                {
                    double interpolatedProb = (100 - sentimentAwareness) / 100f * basicVector[i] + sentimentAwareness / 200f * (sentimentalVector[i] + sentimentVariationalVector[i]);
                    vector.Add(interpolatedProb);
                }
            }
            else
            {
                for (int i = 0; i < basicVector.Count; i++)
                {
                    double interpolatedProb = (100 - sentimentAwareness) / 100f * basicVector[i] + sentimentAwareness / 100f * sentimentalVector[i];
                    vector.Add(interpolatedProb);
                }
            }
            vector = NormalizeAndCumulate(vector);

            Random r = new Random();
            double p = r.NextDouble();

            // Binary search
            int low = 0;
            int high = vector.Count - 1;
            while (low < high)
            {
                int mid = (low + high) / 2;
                if (mid == 0)
                {
                    if (VERBOSE)
                        Console.WriteLine(vector[0]);
                    return mid;
                }
                else if (vector[mid - 1] <= p && p < vector[mid])
                {
                    if (VERBOSE)
                        Console.WriteLine(vector[mid] - vector[mid - 1]);
                    return mid;
                }
                else if (vector[mid] <= p)
                {
                    low = mid + 1;
                }
                else
                {
                    high = mid - 1;
                }
            }
            if (VERBOSE)
            {
                if (low == 0)
                    Console.WriteLine(vector[0]);
                else
                    Console.WriteLine(vector[low] - vector[low - 1]);
            }
            return low;
        }

        public static Chord RomanNumeralToChord(int romanNumeral, MusicalKey.Tonic tonic, int octave = 0)
        {
            if (octave == 0)
                octave = SFXTheme.CurrentSFXTheme.MinOctave;

            if (romanNumeral == 0)
            {
                // No Chord
                return new Chord((Chord.Root)(int)tonic, Chord.Type.NULL, octave);
            }
            int root = (romanNumeral - 1 + (int)tonic) % 12;
            int type = (romanNumeral - 1) / 12;
            return new Chord((Chord.Root)root, (Chord.Type)type, octave);
        }

        public static int ChordToRomanNumeral(Chord chord, MusicalKey.Tonic tonic)
        {
            switch (chord.type)
            {
                case Chord.Type.Major:
                case Chord.Type.minor:
                case Chord.Type.aug:
                case Chord.Type.dim:
                case Chord.Type.sus4:
                case Chord.Type.dom7:
                case Chord.Type.m7:
                    return 1 + 12 * (int)chord.type + (((int)chord.root + 12 - (int)tonic) % 12);
                case Chord.Type.sus2:
                    return 1 + 12 * (int)Chord.Type.sus4 + (((int)chord.root + 19 - (int)tonic) % 12);
                default: // Chord.Type.NULL:
                    return 0;
            }
        }

        private List<double> Normalize(List<double> probVector)
        {
            List<double> normalizedVector = new List<double>();
            double sum = 0.0;

            for (int i = 0; i < probVector.Count; i++)
            {
                sum += probVector[i];
            }

            if (sum == 0f) return probVector;

            for (int i = 0; i < probVector.Count; i++)
            {
                normalizedVector.Add(probVector[i] / sum);
            }
            return normalizedVector;
        }

        private List<double> NormalizeAndCumulate(List<double> probVector)
        {
            List<double> cumulativeVector = new List<double>();
            double sum = 0.0;

            for (int i = 0; i < probVector.Count; i++)
            {
                sum += probVector[i];
            }

            if (sum == 0f) return probVector;

            double cumul = 0f;
            for (int i = 0; i < probVector.Count; i++)
            {
                cumul += probVector[i] / sum;
                cumulativeVector.Add(cumul);
            }
            return cumulativeVector;
        }
    }
}
