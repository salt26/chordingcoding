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
            return NormalizeAndCumulate(basic[(int)mode][prevChord]);
        }

        public int SampleRomanNumeralFromBasic(MusicalKey.Mode mode, int prevChord)
        {
            List<double> vector = GetBasicChordProbVector(mode, prevChord);

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
