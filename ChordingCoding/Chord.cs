using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace ChordingCoding
{
    public class Chord
    {
        public enum Root { C, Db, D, Eb, E, F, Gb, G, Ab, A, Bb, B }
        public enum Type { Major, minor, sus2, sus4, dim, aug, M7, m7 }

        public Root root;
        public Type type;
        public int octave;  // TODO 이전 화음에서 이어지도록
        private int maxOctave = 8;
        private int minOctave = 5;

        /// <summary>
        /// 랜덤으로 화음을 초기화합니다.
        /// </summary>
        public Chord(Form1.Theme theme)
        {
            Random r = new Random();
            int rand;

            switch (theme)
            {
                case Form1.Theme.Autumn:
                    #region Autumn Chord
                    maxOctave = 5;
                    minOctave = 4;
                    root = (Root)r.Next(12);

                    rand = r.Next(15);
                    switch (rand)
                    {
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                        case 5:
                        case 6:
                            type = Type.Major;
                            break;
                        case 7:
                        case 8:
                        case 9:
                        case 10:
                            type = Type.minor;
                            break;
                        case 11:
                            type = Type.sus2;
                            break;
                        case 12:
                        case 13:
                            type = Type.sus4;
                            break;
                        case 14:
                            type = Type.M7;
                            break;
                        default:
                            type = Type.m7;
                            break;
                    }

                    rand = r.Next(15);
                    octave = 5;
                    switch (rand)
                    {
                        case 0:
                            octave -= 2;
                            break;
                        case 1:
                        case 2:
                        case 3:
                            octave -= 1;
                            break;
                        case 4:
                        case 5:
                        case 6:
                        case 7:
                        case 8:
                        case 9:
                        case 10:
                        case 11:
                            break;
                        case 12:
                        case 13:
                        case 14:
                            octave += 1;
                            break;
                        default:
                            octave += 2;
                            break;
                    }
                    if (octave > maxOctave) octave = maxOctave;
                    if (octave < minOctave) octave = minOctave;

                    break;
                    #endregion
                case Form1.Theme.Rain:
                    #region Rain Chord
                    maxOctave = 8;
                    minOctave = 5;
                    root = (Root)r.Next(12);

                    rand = r.Next(15);
                    switch (rand)
                    {
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                            type = Type.minor;
                            break;
                        case 5:
                        case 6:
                            type = Type.m7;
                            break;
                        case 7:
                            type = Type.Major;
                            break;
                        case 8:
                        case 9:
                        case 10:
                            type = Type.M7;
                            break;
                        case 11:
                            type = Type.sus2;
                            break;
                        case 12:
                        case 13:
                            type = Type.sus4;
                            break;
                        case 14:
                            type = Type.aug;
                            break;
                        default:
                            type = Type.dim;
                            break;
                    }

                    rand = r.Next(15);
                    octave = 6;
                    switch (rand)
                    {
                        case 0:
                            octave -= 2;
                            break;
                        case 1:
                        case 2:
                        case 3:
                            octave -= 1;
                            break;
                        case 4:
                        case 5:
                        case 6:
                        case 7:
                        case 8:
                        case 9:
                        case 10:
                        case 11:
                            break;
                        case 12:
                        case 13:
                        case 14:
                            octave += 1;
                            break;
                        default:
                            octave += 2;
                            break;
                    }
                    if (octave > maxOctave) octave = maxOctave;
                    if (octave < minOctave) octave = minOctave;

                    break;
                    #endregion
                case Form1.Theme.Star:
                    #region Star Chord
                    maxOctave = 8;
                    minOctave = 5;
                    root = (Root)r.Next(12);

                    rand = r.Next(15);
                    switch (rand)
                    {
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                        case 5:
                            type = Type.Major;
                            break;
                        case 6:
                        case 7:
                        case 8:
                        case 9:
                        case 10:
                            type = Type.minor;
                            break;
                        case 11:
                            type = Type.sus2;
                            break;
                        case 12:
                        case 13:
                            type = Type.sus4;
                            break;
                        case 14:
                            type = Type.aug;
                            break;
                        default:
                            type = Type.dim;
                            break;
                    }

                    rand = r.Next(15);
                    octave = 6;
                    switch (rand)
                    {
                        case 0:
                            octave -= 2;
                            break;
                        case 1:
                        case 2:
                        case 3:
                            octave -= 1;
                            break;
                        case 4:
                        case 5:
                        case 6:
                        case 7:
                        case 8:
                        case 9:
                        case 10:
                        case 11:
                            break;
                        case 12:
                        case 13:
                        case 14:
                            octave += 1;
                            break;
                        default:
                            octave += 2;
                            break;
                    }
                    if (octave > maxOctave) octave = maxOctave;
                    if (octave < minOctave) octave = minOctave;

                    break;
                    #endregion
            }
        }

        /// <summary>
        /// 주어진 인자로 화음을 초기화합니다.
        /// </summary>
        /// <param name="root">근음의 음이름</param>
        /// <param name="type">화음의 종류</param>
        public Chord(Root root, Type type, int octave)
        {
            this.root = root;
            this.type = type;
            this.octave = octave;
        }

        /// <summary>
        /// 주어진 화음과 자연스럽게 이어지도록 새 화음을 초기화합니다.
        /// </summary>
        /// <param name="c"></param>
        public Chord(Form1.Theme theme, Chord c)
        {
            Random r = new Random();
            int rand;
            int oldP, p;

            switch (theme)
            {
                case Form1.Theme.Autumn:
                    #region Autumn Chord
                    maxOctave = 5;
                    minOctave = 4;
                    rand = r.Next(17);
                    switch (rand)
                    {
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                        case 5:
                            type = Type.Major;
                            break;
                        case 8:
                        case 9:
                        case 10:
                            type = Type.minor;
                            break;
                        case 11:
                            type = Type.sus2;
                            break;
                        case 12:
                        case 13:
                            type = Type.sus4;
                            break;
                        case 14:
                        case 6:
                            type = Type.M7;
                            break;
                        case 15:
                        case 7:
                            type = Type.m7;
                            break;
                        default:
                            type = c.type;
                            break;
                    }

                    //root = (Root)r.Next(12);
                    rand = r.Next(10);
                    switch (rand)
                    {
                        case 1:
                        case 2:
                        case 3:
                            root = c.root;
                            break;
                        case 4:
                        case 5:
                        case 6:
                            root = (Root)(((int)c.root + 7) % 12);
                            break;
                        case 7:
                        case 8:
                        case 9:
                            root = (Root)(((int)c.root + 5) % 12);
                            break;
                        case 10:
                            if (c.type == Type.Major)
                            {
                                root = (Root)(((int)c.root - 3) % 12);
                                type = Type.minor;
                            }
                            else if (c.type == Type.minor)
                            {
                                root = (Root)(((int)c.root + 3) % 12);
                                type = Type.Major;
                            }
                            else if (c.type == Type.dim)
                            {
                                type = c.type;
                                rand = r.Next(2);
                                if (rand == 0)
                                    root = (Root)(((int)c.root + 3) % 12);
                                else
                                    root = (Root)(((int)c.root - 3) % 12);
                            }
                            else if (c.type == Type.aug)
                            {
                                type = c.type;
                                rand = r.Next(2);
                                if (rand == 0)
                                    root = (Root)(((int)c.root + 4) % 12);
                                else
                                    root = (Root)(((int)c.root - 4) % 12);
                            }
                            else
                            {
                                rand = r.Next(2);
                                if (rand == 0)
                                {
                                    root = (Root)(((int)c.root + 3) % 12);
                                    type = Type.Major;
                                }
                                else
                                {
                                    root = (Root)(((int)c.root - 3) % 12);
                                    type = Type.minor;
                                }
                            }
                            break;
                        default:    // Unused
                            root = (Root)r.Next(12);
                            break;
                    }

                    rand = r.Next(15);
                    octave = c.octave;
                    switch (rand)
                    {
                        case 0:
                            octave -= 1;
                            if (octave < 3) octave = 4;
                            break;
                        case 1:
                        case 2:
                        case 3:
                            octave -= 1;
                            if (octave < 3) octave = 3;
                            break;
                        case 4:
                        case 5:
                        case 6:
                        case 7:
                        case 8:
                        case 9:
                        case 10:
                        case 11:
                            break;
                        case 12:
                        case 13:
                        case 14:
                            octave += 1;
                            if (octave > 7) octave = 7;
                            break;
                        default:
                            octave += 1;
                            if (octave > 7) octave = 6;
                            break;
                    }
                    if (octave > maxOctave) octave = maxOctave;
                    if (octave < minOctave) octave = minOctave;

                    oldP = (int)c.root + c.octave * 12;
                    p = (int)root + octave * 12;
                    while (oldP + 12 < p)
                    {
                        octave--;
                        p = (int)root + octave * 12;
                    }
                    while (oldP - 12 > p)
                    {
                        octave++;
                        p = (int)root + octave * 12;
                    }
                    if (octave > maxOctave) octave = maxOctave;
                    if (octave < minOctave) octave = minOctave;

                    break;
                    #endregion
                case Form1.Theme.Rain:
                    #region Rain Chord
                    maxOctave = 8;
                    minOctave = 5;
                    rand = r.Next(17);
                    switch (rand)
                    {
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                            type = Type.minor;
                            break;
                        case 5:
                        case 6:
                            type = Type.m7;
                            break;
                        case 7:
                            type = Type.Major;
                            break;
                        case 8:
                        case 9:
                        case 10:
                            type = Type.M7;
                            break;
                        case 11:
                            type = Type.sus2;
                            break;
                        case 12:
                        case 13:
                            type = Type.sus4;
                            break;
                        case 14:
                            type = Type.aug;
                            break;
                        case 15:
                            type = Type.dim;
                            break;
                        default:
                            type = c.type;
                            break;
                    }

                    //root = (Root)r.Next(12);
                    rand = r.Next(10);
                    switch (rand)
                    {
                        case 1:
                        case 2:
                        case 3:
                            root = c.root;
                            break;
                        case 4:
                        case 5:
                        case 6:
                            root = (Root)(((int)c.root + 7) % 12);
                            break;
                        case 7:
                        case 8:
                        case 9:
                            root = (Root)(((int)c.root + 5) % 12);
                            break;
                        case 10:
                            if (c.type == Type.Major)
                            {
                                root = (Root)(((int)c.root - 3) % 12);
                                type = Type.minor;
                            }
                            else if (c.type == Type.minor)
                            {
                                root = (Root)(((int)c.root + 3) % 12);
                                type = Type.Major;
                            }
                            else if (c.type == Type.dim)
                            {
                                type = c.type;
                                rand = r.Next(2);
                                if (rand == 0)
                                    root = (Root)(((int)c.root + 3) % 12);
                                else
                                    root = (Root)(((int)c.root - 3) % 12);
                            }
                            else if (c.type == Type.aug)
                            {
                                type = c.type;
                                rand = r.Next(2);
                                if (rand == 0)
                                    root = (Root)(((int)c.root + 4) % 12);
                                else
                                    root = (Root)(((int)c.root - 4) % 12);
                            }
                            else
                            {
                                rand = r.Next(2);
                                if (rand == 0)
                                {
                                    root = (Root)(((int)c.root + 3) % 12);
                                    type = Type.Major;
                                }
                                else
                                {
                                    root = (Root)(((int)c.root - 3) % 12);
                                    type = Type.minor;
                                }
                            }
                            break;
                        default:    // Unused
                            root = (Root)r.Next(12);
                            break;
                    }

                    rand = r.Next(15);
                    octave = c.octave;
                    switch (rand)
                    {
                        case 0:
                            octave -= 1;
                            if (octave < 3) octave = 4;
                            break;
                        case 1:
                        case 2:
                        case 3:
                            octave -= 1;
                            if (octave < 3) octave = 3;
                            break;
                        case 4:
                        case 5:
                        case 6:
                        case 7:
                        case 8:
                        case 9:
                        case 10:
                        case 11:
                            break;
                        case 12:
                        case 13:
                        case 14:
                            octave += 1;
                            if (octave > 7) octave = 7;
                            break;
                        default:
                            octave += 1;
                            if (octave > 7) octave = 6;
                            break;
                    }
                    if (octave > maxOctave) octave = maxOctave;
                    if (octave < minOctave) octave = minOctave;

                    oldP = (int)c.root + c.octave * 12;
                    p = (int)root + octave * 12;
                    while (oldP + 12 < p)
                    {
                        octave--;
                        p = (int)root + octave * 12;
                    }
                    while (oldP - 12 > p)
                    {
                        octave++;
                        p = (int)root + octave * 12;
                    }
                    if (octave > maxOctave) octave = maxOctave;
                    if (octave < minOctave) octave = minOctave;

                    break;
                    #endregion
                case Form1.Theme.Star:
                    #region Star Chord
                    maxOctave = 8;
                    minOctave = 5;
                    rand = r.Next(17);
                    switch (rand)
                    {
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                        case 5:
                            type = Type.Major;
                            break;
                        case 6:
                        case 7:
                        case 8:
                        case 9:
                        case 10:
                            type = Type.minor;
                            break;
                        case 11:
                            type = Type.sus2;
                            break;
                        case 12:
                        case 13:
                            type = Type.sus4;
                            break;
                        case 14:
                            type = Type.aug;
                            break;
                        case 15:
                            type = Type.dim;
                            break;
                        default:
                            type = c.type;
                            break;
                    }

                    //root = (Root)r.Next(12);
                    rand = r.Next(10);
                    switch (rand)
                    {
                        case 1:
                        case 2:
                        case 3:
                            root = c.root;
                            break;
                        case 4:
                        case 5:
                        case 6:
                            root = (Root)(((int)c.root + 7) % 12);
                            break;
                        case 7:
                        case 8:
                        case 9:
                            root = (Root)(((int)c.root + 5) % 12);
                            break;
                        case 10:
                            if (c.type == Type.Major)
                            {
                                root = (Root)(((int)c.root - 3) % 12);
                                type = Type.minor;
                            }
                            else if (c.type == Type.minor)
                            {
                                root = (Root)(((int)c.root + 3) % 12);
                                type = Type.Major;
                            }
                            else if (c.type == Type.dim)
                            {
                                type = c.type;
                                rand = r.Next(2);
                                if (rand == 0)
                                    root = (Root)(((int)c.root + 3) % 12);
                                else
                                    root = (Root)(((int)c.root - 3) % 12);
                            }
                            else if (c.type == Type.aug)
                            {
                                type = c.type;
                                rand = r.Next(2);
                                if (rand == 0)
                                    root = (Root)(((int)c.root + 4) % 12);
                                else
                                    root = (Root)(((int)c.root - 4) % 12);
                            }
                            else
                            {
                                rand = r.Next(2);
                                if (rand == 0)
                                {
                                    root = (Root)(((int)c.root + 3) % 12);
                                    type = Type.Major;
                                }
                                else
                                {
                                    root = (Root)(((int)c.root - 3) % 12);
                                    type = Type.minor;
                                }
                            }
                            break;
                        default:    // Unused
                            root = (Root)r.Next(12);
                            break;
                    }

                    rand = r.Next(15);
                    octave = c.octave;
                    switch (rand)
                    {
                        case 0:
                            octave -= 1;
                            if (octave < 3) octave = 4;
                            break;
                        case 1:
                        case 2:
                        case 3:
                            octave -= 1;
                            if (octave < 3) octave = 3;
                            break;
                        case 4:
                        case 5:
                        case 6:
                        case 7:
                        case 8:
                        case 9:
                        case 10:
                        case 11:
                            break;
                        case 12:
                        case 13:
                        case 14:
                            octave += 1;
                            if (octave > 7) octave = 7;
                            break;
                        default:
                            octave += 1;
                            if (octave > 7) octave = 6;
                            break;
                    }
                    if (octave > maxOctave) octave = maxOctave;
                    if (octave < minOctave) octave = minOctave;

                    oldP = (int)c.root + c.octave * 12;
                    p = (int)root + octave * 12;
                    while (oldP + 12 < p)
                    {
                        octave--;
                        p = (int)root + octave * 12;
                    }
                    while (oldP - 12 > p)
                    {
                        octave++;
                        p = (int)root + octave * 12;
                    }
                    if (octave > maxOctave) octave = maxOctave;
                    if (octave < minOctave) octave = minOctave;

                    break;
                    #endregion
            }
        }

        /// <summary>
        /// 화음에 맞는 음 하나를 반환합니다.
        /// 반환값은 MIDI의 음 높이입니다.
        /// </summary>
        /// <returns></returns>
        public int NextNote()
        {
            int v = 0;
            int p = (int)root + octave * 12;
            Random r = new Random();
            if (type != Type.M7 && type != Type.m7)
            {
                switch (r.Next(18))
                {
                    case 0:
                    case 1:
                    case 2:
                    case 15:
                        v = 0;
                        break;
                    case 3:
                    case 4:
                    case 5:
                    case 16:
                        v = 1;
                        break;
                    case 6:
                    case 7:
                    case 8:
                    case 17:
                        v = 2;
                        break;
                    case 9:
                    case 10:
                        v = 0;
                        p += 12;
                        break;
                    case 11:
                    case 12:
                        v = 2;
                        p -= 12;
                        break;
                    case 13:
                        v = 1;
                        p += 12;
                        break;
                    case 14:
                        v = 1;
                        p -= 12;
                        break;
                }
            }
            else
            {
                switch (r.Next(24))
                {
                    case 0:
                    case 1:
                    case 2:
                    case 15:
                        v = 0;
                        break;
                    case 3:
                    case 4:
                    case 5:
                    case 16:
                        v = 1;
                        break;
                    case 6:
                    case 7:
                    case 8:
                    case 17:
                        v = 2;
                        break;
                    case 18:
                    case 19:
                    case 20:
                    case 21:
                        v = 3;
                        break;
                    case 9:
                    case 10:
                        v = 0;
                        p += 12;
                        break;
                    case 22:
                    case 23:
                        v = 3;
                        p -= 12;
                        break;
                    case 11:
                    case 12:
                        v = 2;
                        p -= 12;
                        break;
                    case 13:
                        v = 1;
                        p += 12;
                        break;
                    case 14:
                        v = 1;
                        p -= 12;
                        break;
                }
            }
            
            return (p + TypeToNote(v)) % 128;
        }

        /// <summary>
        /// 3화음 또는 7화음의 음들을 반환합니다.
        /// 반환값은 MIDI의 음 높이 배열입니다.
        /// </summary>
        /// <returns></returns>
        public int[] NextChord()
        {
            int[] r;
            if (type != Type.M7 && type != Type.m7)
            {
                r = new int[3] {  (octave) * 12 + (int)root + TypeToNote(1),
                                  (octave) * 12 + (int)root + TypeToNote(2),
                                  (octave) * 12 + (int)root + TypeToNote(0) };
            }
            else
            {
                r = new int[4] {  (octave) * 12 + (int)root + TypeToNote(1),
                                  (octave) * 12 + (int)root + TypeToNote(2),
                                  (octave) * 12 + (int)root + TypeToNote(3),
                                  (octave) * 12 + (int)root + TypeToNote(0) };
            }
            return r;
        }

        private int TypeToNote(int order)
        {
            if (order < 0 || ((type != Type.M7 && type != Type.m7) && order >= 3 ||
                (type == Type.M7 || type == Type.m7) && order >= 4)) return -1;
            int[] typeToNote;
            switch (type)
            {
                case Type.Major:
                    typeToNote = new int[3] { 0, 4, 7 };
                    break;
                case Type.M7:
                    typeToNote = new int[4] { 0, 4, 7, 10 };
                    break;
                case Type.minor:
                    typeToNote = new int[3] { 0, 3, 7 };
                    break;
                case Type.m7:
                    typeToNote = new int[4] { 0, 3, 7, 10 };
                    break;
                case Type.sus2:
                    typeToNote = new int[3] { 0, 2, 7 };
                    break;
                case Type.sus4:
                    typeToNote = new int[3] { 0, 5, 7 };
                    break;
                case Type.aug:
                    typeToNote = new int[3] { 0, 4, 8 };
                    break;
                default: // case Type.dim:
                    typeToNote = new int[3] { 0, 3, 6 };
                    break;
            }
            return typeToNote[order];
        }

        /// <summary>
        /// 화음 chord에 어울리는 색을 찾아서 반환합니다.
        /// </summary>
        /// <param name="chord"></param>
        /// <returns></returns>
        public static Color ChordColor(Chord chord)
        {
            int h = 0;
            float s = 1f, v = 1f;

            h = HueOfPitch(chord.root);

            switch (chord.type)
            {
                case Chord.Type.Major:
                    s = 0.7f;
                    v = 1f;
                    break;
                case Chord.Type.M7:
                    s = 0.7f;
                    v = 0.8f;
                    break;
                case Chord.Type.minor:
                    s = 0.5f;
                    v = 0.5f;
                    h = (h + 60) % 360;
                    break;
                case Chord.Type.m7:
                    s = 0.5f;
                    v = 0.4f;
                    h = (h + 60) % 360;
                    break;
                case Chord.Type.sus2:
                    s = 0.5f;
                    v = 0.9f;
                    h = (h - 15) % 360;
                    break;
                case Chord.Type.sus4:
                    s = 0.5f;
                    v = 0.9f;
                    h = (h + 15) % 360;
                    break;
                case Chord.Type.dim:
                    s = 0.2f;
                    v = 0.5f;
                    break;
                case Chord.Type.aug:
                    s = 0.3f;
                    v = 0.8f;
                    break;
            }

            return HSVToColor(h, s, v);
        }

        /// <summary>
        /// 현재 화음에 어울리는 색을 찾아서 반환합니다.
        /// </summary>
        /// <returns></returns>
        public Color ChordColor()
        {
            return ChordColor(this);
        }

        /// <summary>
        /// pitch의 음이름에 어울리는 색을 찾아서 반환합니다.
        /// </summary>
        /// <param name="pitch"></param>
        /// <returns></returns>
        public static Color PitchColor(Root pitch)
        {
            int h = HueOfPitch(pitch);
            return HSVToColor(h, 0.7f, 1f);
        }

        /// <summary>
        /// 주어진 색의 alpha(투명도)를 바꿔서 반환합니다.
        /// alpha는 0 이상 255 이하입니다.
        /// </summary>
        /// <param name="c"></param>
        /// <param name="alpha"></param>
        /// <returns></returns>
        public static Color SetAlpha(Color c, int alpha)
        {
            if (alpha < 0) alpha = 0;
            if (alpha > 255) alpha = 255;
            return Color.FromArgb(alpha, c);
        }

        /// <summary>
        /// pitch의 음이름에 따른 색의 Hue 값을 반환합니다.
        /// 반환하는 값은 0 이상 359 이하입니다.
        /// </summary>
        /// <param name="pitch"></param>
        /// <returns></returns>
        private static int HueOfPitch(Root pitch)
        {
            int h = 0;
            switch (pitch)
            {
                case Chord.Root.C:
                    h = 120;
                    break;
                case Chord.Root.G:
                    h = 90;
                    break;
                case Chord.Root.D:
                    h = 60;
                    break;
                case Chord.Root.A:
                    h = 30;
                    break;
                case Chord.Root.E:
                    h = 0;
                    break;
                case Chord.Root.B:
                    h = 330;
                    break;
                case Chord.Root.Gb:
                    h = 300;
                    break;
                case Chord.Root.Db:
                    h = 270;
                    break;
                case Chord.Root.Ab:
                    h = 240;
                    break;
                case Chord.Root.Eb:
                    h = 210;
                    break;
                case Chord.Root.Bb:
                    h = 180;
                    break;
                case Chord.Root.F:
                    h = 150;
                    break;
            }
            return h;
        }

        /// <summary>
        /// HSV를 RGB로 변환한 색을 반환합니다.
        /// </summary>
        /// <param name="h">Hue, 0 이상 359 이하</param>
        /// <param name="s">Saturation, 0.0f 이상 1.0f 이하</param>
        /// <param name="v">Value(Brightness), 0.0f 이상 1.0f 이하</param>
        /// <returns></returns>
        private static Color HSVToColor(int h, float s, float v)
        {
            h = h % 360;

            if (v < 0) v = 0;
            else if (v > 1) v = 1;

            if (s <= 0)
            {
                return Color.FromArgb((int)(v * 255), (int)(v * 255), (int)(v * 255));
            }
            else if (s > 1) s = 1;

            /* HSV to RGB */
            int ff = h % 60;
            float p = v * (1f - s);
            float q = v * (1f - (s * (ff / 60f)));
            float t = v * (1f - (s * (1f - (ff / 60f))));
            float r, g, b;
            if (h >= 0 && h < 60)
            {
                r = v;
                g = t;
                b = p;
            }
            else if (h >= 60 && h < 120)
            {
                r = q;
                g = v;
                b = p;
            }
            else if (h >= 120 && h < 180)
            {
                r = p;
                g = v;
                b = t;
            }
            else if (h >= 180 && h < 240)
            {
                r = p;
                g = q;
                b = v;
            }
            else if (h >= 240 && h < 300)
            {
                r = t;
                g = p;
                b = v;
            }
            else if (h >= 300 && h < 360)
            {
                r = v;
                g = p;
                b = q;
            }
            else
            {
                r = 0;
                g = 0;
                b = 0;
            }
            r *= 255;
            g *= 255;
            b *= 255;
            if (r < 0) r = 0;
            if (r > 255) r = 255;
            if (g < 0) g = 0;
            if (g > 255) g = 255;
            if (b < 0) b = 0;
            if (b > 255) b = 255;

            return Color.FromArgb((int)(r + 0.5f), (int)(g + 0.5f), (int)(b + 0.5f));
        }
    }

}
