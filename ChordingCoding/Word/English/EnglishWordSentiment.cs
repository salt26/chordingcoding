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

namespace ChordingCoding.Word.English
{
    public class EnglishWordSentiment : WordSentiment
    {
#if USE_NEW_SCHEME
        public double valenceValue;
        public double arousalValue;
        private SentimentState.Valence valence;
        private SentimentState.Arousal arousal;

        public EnglishWordSentiment(string word, double valenceValue, double arousalValue,
            SentimentState.Valence valence, SentimentState.Arousal arousal)
        {
            Word = word;
            this.valenceValue = valenceValue;
            this.arousalValue = arousalValue;
            this.valence = valence;
            this.arousal = arousal;
        }

        public EnglishWordSentiment(string word, string valenceValue, string arousalValue,
            string valence, string arousal)
        {
            Word = word;
            this.valence = StringToValence(valence);
            this.arousal = StringToArousal(arousal);
            if (!Double.TryParse(valenceValue, out this.valenceValue))
            {
                this.valenceValue = -2;
                this.valence = SentimentState.Valence.NULL;
            }
            if (!Double.TryParse(arousalValue, out this.arousalValue))
            {
                this.arousalValue = -2;
                this.arousal = SentimentState.Arousal.NULL;
            }
        }

        private SentimentState.Valence StringToValence(string valence)
        {
            switch (valence)
            {
                case "L": return SentimentState.Valence.Low;
                case "M": return SentimentState.Valence.Medium;
                case "H": return SentimentState.Valence.High;
                default: //throw new ArgumentException("Invalid Valence string");
                    return SentimentState.Valence.NULL;
            }
        }

        private SentimentState.Arousal StringToArousal(string arousal)
        {
            switch (arousal)
            {
                case "L": return SentimentState.Arousal.Low;
                case "M": return SentimentState.Arousal.Medium;
                case "H": return SentimentState.Arousal.High;
                default: //throw new ArgumentException("Invalid Arousal string");
                    return SentimentState.Arousal.NULL;
            }
        }

        public override void Print()
        {
            string s = "ValenceValue: " + valenceValue.ToString() + "\tArousalValue: " + arousalValue.ToString();
            Console.WriteLine(s);
            base.Print();
        }

        public override SentimentState.Valence GetValence()
        {
            return valence;
        }

        public override SentimentState.Arousal GetArousal()
        {
            return arousal;
        }
#else
        private Valence valence;
        private StateIntensity stateIntensity;
        private Emotion emotion;
        private Judgment judgment;
        private Agreement agreement;
        private Intention intention;

        public EnglishWordSentiment(string word, Valence valence,
            StateIntensity stateIntensity, Emotion emotion, Judgment judgment,
            Agreement agreement, Intention intention)
        {
            Word = word;
            this.valence = valence;
            this.stateIntensity = stateIntensity;
            this.emotion = emotion;
            this.judgment = judgment;
            this.agreement = agreement;
            this.intention = intention;
        }

        public EnglishWordSentiment(string word, string valence,
            string stateIntensity, string emotion, string judgment,
            string agreement, string intention)
        {
            Word = word;
            if (!Enum.TryParse(valence, out this.valence))
            {
                this.valence = Valence.NULL;
            }
            if (!Enum.TryParse(stateIntensity, out this.stateIntensity))
            {
                this.stateIntensity = StateIntensity.NULL;
            }
            if (!Enum.TryParse(emotion, out this.emotion))
            {
                this.emotion = Emotion.NULL;
            }
            if (!Enum.TryParse(judgment, out this.judgment))
            {
                this.judgment = Judgment.NULL;
            }
            if (!Enum.TryParse(agreement, out this.agreement))
            {
                this.agreement = Agreement.NULL;
            }
            if (!Enum.TryParse(intention, out this.intention))
            {
                this.intention = Intention.NULL;
            }
        }

        public override Agreement GetAgreement()
        {
            return agreement;
        }

        public override Emotion GetEmotion()
        {
            return emotion;
        }

        public override Intention GetIntention()
        {
            return intention;
        }

        public override Judgment GetJudgment()
        {
            return judgment;
        }

        public override StateIntensity GetStateIntensity()
        {
            return stateIntensity;
        }

        public override Valence GetValence()
        {
            return valence;
        }
#endif
    }
}
