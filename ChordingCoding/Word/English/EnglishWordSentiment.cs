using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChordingCoding.Word.English
{
    public class EnglishWordSentiment : WordSentiment
    {
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
    }
}
