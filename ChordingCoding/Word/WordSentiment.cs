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
#define USE_NEW_SCHEME
using System;
using ChordingCoding.Sentiment;

namespace ChordingCoding.Word
{
    /// <summary>
    /// An abstract structure that contains word sentiment information.
    /// You can inherit and implement it for your own language.
    /// </summary>
    public abstract class WordSentiment
    {
#if USE_NEW_SCHEME
        // English: https://link.springer.com/article/10.3758/s13428-012-0314-x#SecESM1
        // Korean: http://ling.snu.ac.kr/kosac/pub/PACLIC26.pdf

        public string Word { get; protected set; }

        public abstract SentimentState.Valence GetValence();
        public abstract SentimentState.Arousal GetArousal();

        public virtual void Print()
        {
            string s = "Valence: " + GetValence().ToString() + "\tArousal: " + GetArousal().ToString();
            Console.WriteLine(s);
        }
#else
        // English(old): http://www.wjh.harvard.edu/~inquirer/homecat.htm
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
#endif
    }
}
