using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChordingCoding.Word
{
    /// <summary>
    /// An abstract structure that contains word sentiment information.
    /// You can inherit and implement it for your own language.
    /// </summary>
    public abstract class WordSentiment
    {
        // English: http://www.wjh.harvard.edu/~inquirer/homecat.htm
        // Korean: http://ling.snu.ac.kr/kosac/pub/PACLIC26.pdf

        public enum Valence
        {
            Neutral = 0, Positive = 1, Negative = 2, Complex = 3, NULL = -1
        };

        public enum StateIntensity
        {
            Normal = 0, Overstated = 1, Understated = 2, NULL = -1
        };

        public enum Emotion
        {
            Neutral = 0, Pleasure = 1, Pain = 2, Complex = 3, NULL = -1
        };

        public enum Judgment
        {
            Neutral = 0, Virtue = 1, Vice = 2, Complex = 3, NULL = -1
        };

        public enum Agreement
        {
            Neutral = 0, Affiliation = 1, Hostility = 2, Complex = 3, NULL = -1
        };

        public enum Intention
        {
            Neutral = 0, Active = 1, Passive = 2, Complex = 3, NULL = -1
        };

        public string Word { get; protected set; }

        public abstract Valence GetValence();
        public abstract StateIntensity GetStateIntensity();
        public abstract Emotion GetEmotion();
        public abstract Judgment GetJudgment();
        public abstract Agreement GetAgreement();
        public abstract Intention GetIntention();

        /// <summary>
        /// Print the state of this structure.
        /// Implementing this method is not required.
        /// </summary>
        public virtual void Print()
        {
            string s = "Valence: " + GetValence().ToString() + "\tStateIntensity: " + GetStateIntensity().ToString() +
                "\tEmotion: " + GetEmotion().ToString() + "\tJudgment: " +
                GetJudgment().ToString() + "\tAgreement: " + GetAgreement().ToString() +
                "\tIntention: " + GetIntention().ToString();
            Console.WriteLine(s);
        }
    }
}
