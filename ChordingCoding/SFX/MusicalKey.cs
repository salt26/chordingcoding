using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public void Transpose(double valence = 0.0)
        {

            Random r = new Random();

            mode = Mode.Major;
            if (r.NextDouble() < 0.397588 + 0.385408 * valence) mode = Mode.Minor;

            tonic = (Tonic)r.Next(0, 12);
            if (VERBOSE)
            {
                Console.WriteLine("Transpose key!");
                Console.WriteLine("Key: " + tonic.ToString() + " " + mode.ToString());
            }
        }
    }
}
