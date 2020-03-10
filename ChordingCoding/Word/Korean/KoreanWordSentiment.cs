using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChordingCoding.Word.Korean
{
    /// <summary>
    /// 한국어 단어의 감정 정보를 담는 구조체입니다.
    /// </summary>
    public class KoreanWordSentiment : WordSentiment
    {
        public enum PolarityValue { COMP = 0, NEG = 1, NEUT = 2, NONE = 3, POS = 4, NULL = -1 };
        public enum IntensityValue { High = 0, Low = 1, Medium = 2, None = 3, NULL = -1 };
        public enum SubjectivityTypeValue
        {
            Agreement = 0, Argument = 1, Emotion = 2, Intention = 3,
            Judgment = 4, Others = 5, Speculation = 6, NULL = -1
        };
        public enum SubjectivityPolarityValue { COMP = 0, NEG = 1, NEUT = 2, POS = 3, NULL = -1 };
        
        public PolarityValue Polarity { get; }
        public IntensityValue Intensity { get; }
        public SubjectivityTypeValue SubjectivityType { get; }
        public SubjectivityPolarityValue SubjectivityPolarity { get; }

        public KoreanWordSentiment(string word, PolarityValue polarity,
            IntensityValue intensity, SubjectivityTypeValue subjectivityType,
            SubjectivityPolarityValue subjectivityPolarity)
        {
            Word = word;
            Polarity = polarity;
            Intensity = intensity;
            SubjectivityType = subjectivityType;
            SubjectivityPolarity = subjectivityPolarity;
        }

        public KoreanWordSentiment(string word, string polarity, string intensity,
            string subjectivityType, string subjectivityPolarity)
        {
            Word = word;
            Polarity = StringToPolarity(polarity);
            Intensity = StringToIntensity(intensity);
            SubjectivityType = StringToSubjectivityType(subjectivityType);
            SubjectivityPolarity = StringToSubjectivityPolarity(subjectivityPolarity);
        }

        private PolarityValue StringToPolarity(string polarity)
        {
            switch (polarity)
            {
                case "COMP": return PolarityValue.COMP;
                case "NEG": return PolarityValue.NEG;
                case "NEUT": return PolarityValue.NEUT;
                case "NONE": return PolarityValue.NONE;
                case "POS": return PolarityValue.POS;
                default: //throw new ArgumentException("Invalid Polarity string");
                    return PolarityValue.NULL;
            }
        }

        private IntensityValue StringToIntensity(string intensity)
        {
            switch (intensity)
            {
                case "High": return IntensityValue.High;
                case "Low": return IntensityValue.Low;
                case "Medium": return IntensityValue.Medium;
                case "None": return IntensityValue.None;
                default: //throw new ArgumentException("Invalid Intensity string");
                    return IntensityValue.NULL;
            }
        }

        private SubjectivityTypeValue StringToSubjectivityType(string subjectivityType)
        {
            switch (subjectivityType)
            {
                case "Agreement": return SubjectivityTypeValue.Agreement;
                case "Argument": return SubjectivityTypeValue.Argument;
                case "Emotion": return SubjectivityTypeValue.Emotion;
                case "Intention": return SubjectivityTypeValue.Intention;
                case "Judgment": return SubjectivityTypeValue.Judgment;
                case "Others": return SubjectivityTypeValue.Others;
                case "Speculation": return SubjectivityTypeValue.Speculation;
                default: //throw new ArgumentException("Invalid SubjectivityType string");
                    return SubjectivityTypeValue.NULL;
            }
        }

        private SubjectivityPolarityValue StringToSubjectivityPolarity(string subjectivityPolarity)
        {
            switch (subjectivityPolarity)
            {
                case "COMP": return SubjectivityPolarityValue.COMP;
                case "NEG": return SubjectivityPolarityValue.NEG;
                case "NEUT": return SubjectivityPolarityValue.NEUT;
                case "POS": return SubjectivityPolarityValue.POS;
                default: //throw new ArgumentException("Invalid SubjectivityPolarity string");
                    return SubjectivityPolarityValue.NULL;
            }
        }

        public override void Print()
        {
            string s = "Polarity: " + Polarity.ToString() + "\tIntensity: " + Intensity.ToString() +
                "\tSubjectivityType: " + SubjectivityType.ToString() + "\tSubjectivityPolarity: " +
                SubjectivityPolarity.ToString();
            Console.WriteLine(s);
            base.Print();
        }

        public override Valence GetValence()
        {
            switch (Polarity)
            {
                case PolarityValue.COMP: return Valence.Complex;
                case PolarityValue.NEG: return Valence.Negative;
                case PolarityValue.NEUT: return Valence.Neutral;
                case PolarityValue.NONE: return Valence.NULL;
                case PolarityValue.POS: return Valence.Positive;
                default: return Valence.NULL;
            }
        }

        public override StateIntensity GetStateIntensity()
        {
            switch (Intensity)
            {
                case IntensityValue.High: return StateIntensity.Overstated;
                case IntensityValue.Low: return StateIntensity.Understated;
                case IntensityValue.Medium: return StateIntensity.Normal;
                case IntensityValue.None: return StateIntensity.NULL;
                default: return StateIntensity.NULL;
            }
        }

        public override Emotion GetEmotion()
        {
            if (SubjectivityType == SubjectivityTypeValue.Emotion)
            {
                switch (SubjectivityPolarity)
                {
                    case SubjectivityPolarityValue.COMP: return Emotion.Complex;
                    case SubjectivityPolarityValue.NEG: return Emotion.Pain;
                    case SubjectivityPolarityValue.NEUT: return Emotion.Neutral;
                    case SubjectivityPolarityValue.POS: return Emotion.Pleasure;
                    default: return Emotion.NULL;
                }
            }
            else
            {
                return Emotion.NULL;
            }
        }

        public override Judgment GetJudgment()
        {
            if (SubjectivityType == SubjectivityTypeValue.Judgment)
            {
                switch (SubjectivityPolarity)
                {
                    case SubjectivityPolarityValue.COMP: return Judgment.Complex;
                    case SubjectivityPolarityValue.NEG: return Judgment.Vice;
                    case SubjectivityPolarityValue.NEUT: return Judgment.Neutral;
                    case SubjectivityPolarityValue.POS: return Judgment.Virtue;
                    default: return Judgment.NULL;
                }
            }
            else
            {
                return Judgment.NULL;
            }
        }

        public override Agreement GetAgreement()
        {
            if (SubjectivityType == SubjectivityTypeValue.Agreement)
            {
                switch (SubjectivityPolarity)
                {
                    case SubjectivityPolarityValue.COMP: return Agreement.Complex;
                    case SubjectivityPolarityValue.NEG: return Agreement.Hostility;
                    case SubjectivityPolarityValue.NEUT: return Agreement.Neutral;
                    case SubjectivityPolarityValue.POS: return Agreement.Affiliation;
                    default: return Agreement.NULL;
                }
            }
            else
            {
                return Agreement.NULL;
            }
        }

        public override Intention GetIntention()
        {
            if (SubjectivityType == SubjectivityTypeValue.Intention)
            {
                switch (SubjectivityPolarity)
                {
                    case SubjectivityPolarityValue.COMP: return Intention.Complex;
                    case SubjectivityPolarityValue.NEG: return Intention.Passive;
                    case SubjectivityPolarityValue.NEUT: return Intention.Neutral;
                    case SubjectivityPolarityValue.POS: return Intention.Active;
                    default: return Intention.NULL;
                }
            }
            else
            {
                return Intention.NULL;
            }
        }
    }
}
