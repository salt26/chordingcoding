using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChordingCoding.Word
{
    public abstract class SentimentAnalyzer
    {
        public bool IsReady { get; protected set; } = false;

        public abstract void Initialize();
        public abstract void Analyze(string input);
        public abstract WordSentiment GetSentimentAndFlush();
    }
}
