/*
MIT License

Copyright (c) 2019 salt26

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
using System.Collections.Generic;
using System.Linq;
using System.IO;
using ChordingCoding.Utility;
using ChordingCoding.Sentiment;

namespace ChordingCoding.Word.English
{
    /// <summary>
    /// A class that analyzes sentiment in English string.
    /// </summary>
    public class EnglishSentimentAnalyzer : SentimentAnalyzer
    {
        private Util.CSVReader englishSentimentCSV;
        private Dictionary<string, EnglishWordSentiment> englishSentimentDictionary;
        private SymSpell.SymSpell symSpell;

        // aggregateEnglishSentiment
        private int[] aggregateValence;
#if USE_NEW_SCHEME
        private int[] aggregateArousal;
        private int aggregateCount;
        private double aggregateValenceValue;
        private double aggregateArousalValue;
#else
        private int[] aggregateStateIntensity;
        private int[] aggregateEmotion;
        private int[] aggregateJudgment;
        private int[] aggregateAgreement;
        private int[] aggregateIntention;
#endif

        public static EnglishSentimentAnalyzer instance = null;

        /// <summary>
        /// Initialize English sentiment analyzer.
        /// Only a single EnglishSentimentAnalyzer instance can exist.
        /// Using "EnglishSentimentAnalyzer.instance" to access to it.
        /// </summary>
        public EnglishSentimentAnalyzer()
        {
            Initialize();
        }

        /// <summary>
        /// Load sentiment dictionary to initialize sentiment analyzer.
        /// </summary>
        public override void Initialize()
        {
            if (!(instance is null)) return;
            instance = this;

            #region Load English sentiment dictionary (old)
            /*
             * EnglishSentiment.csv is created by MorphemeParser(https://github.com/salt26/morpheme-parser).
             * The original data source is Inquirer Dictionary(http://www.wjh.harvard.edu/~inquirer/homecat.htm).
             */
#if !USE_NEW_SCHEME
            englishSentimentCSV = new Util.CSVReader("EnglishSentiment.csv", true);
            englishSentimentDictionary = new Dictionary<string, EnglishWordSentiment>();

            foreach (List<string> row in englishSentimentCSV.GetData())
            {
                englishSentimentDictionary.Add(row[0], new EnglishWordSentiment(
                    row[0], row[1], row[2], row[3], row[4], row[5], row[6]));
            }

            Util.TaskQueue.Add("aggregateEnglishSentiment", InitializeAggregate);
#endif
            #endregion

            #region Load English sentiment dictionary (new)
            /*
             * EnglishSentiment2.csv is created by Modality Changer(https://github.com/salt26/modality-changer).
             * The original data source is Warriner et al. (2013)(https://link.springer.com/article/10.3758/s13428-012-0314-x#SecESM1).
             */
#if USE_NEW_SCHEME
            englishSentimentCSV = new Util.CSVReader("EnglishSentiment2.csv", true);
            englishSentimentDictionary = new Dictionary<string, EnglishWordSentiment>();

            foreach (List<string> row in englishSentimentCSV.GetData())
            {
                englishSentimentDictionary.Add(row[0], new EnglishWordSentiment(
                    row[0], row[1], row[3], row[2], row[4]));
            }

            Util.TaskQueue.Add("aggregateEnglishSentiment", InitializeAggregate);
#endif
            #endregion

            #region Load SymSpell (spelling correction engine)
            symSpell = new SymSpell.SymSpell(82998, 2);
            string dictionaryPath = "EnglishFrequencyDictionary.txt";
            if (!symSpell.LoadDictionary(dictionaryPath, 0, 1))
            {
                throw new FileNotFoundException("EnglishFrequencyDictionary.txt not found!");
            }
            var a = symSpell.WordSegmentation("initialize", 0);
            #endregion

            IsReady = true;
        }

        /// <summary>
        /// Analyze sentiment of English input.
        /// The results of analysis are accumulated until the next call of GetSentimentAndFlush().
        /// </summary>
        /// <param name="input">English string</param>
        public override void Analyze(string input)
        {
            if (!HasStart) return;

            // attach spelling correction engine
            var suggestion = symSpell.WordSegmentation(input.ToLowerInvariant(), 2);

            Console.WriteLine(suggestion.correctedString);

            List<string> tokens = suggestion.correctedString.Split(' ').ToList();
            foreach (string word in tokens)
            {
                if (englishSentimentDictionary.ContainsKey(word))
                {
                    void UpdateAggregate(object[] args)
                    {
                        EnglishWordSentiment sentiment = args[0] as EnglishWordSentiment;
                        int weight = 1;

#if USE_NEW_SCHEME
                        if (sentiment.GetValence() != SentimentState.Valence.NULL)
                        {
                            aggregateValence[(int)sentiment.GetValence()] += weight;
                        }
                        if (sentiment.GetArousal() != SentimentState.Arousal.NULL)
                        {
                            aggregateArousal[(int)sentiment.GetArousal()] += weight;
                        }
                        if (sentiment.valenceValue != -2 && sentiment.arousalValue != -2)
                        {
                            aggregateCount += weight;
                            aggregateValenceValue += sentiment.valenceValue * weight;
                            aggregateArousalValue += sentiment.arousalValue * weight;
                        }
#else
                        if (sentiment.GetValence() != WordSentiment.Valence.NULL)
                        {
                            aggregateValence[(int)sentiment.GetValence()] += weight;
                        }
                        if (sentiment.GetStateIntensity() != WordSentiment.StateIntensity.NULL)
                        {
                            aggregateStateIntensity[(int)sentiment.GetStateIntensity()] += weight;
                        }
                        if (sentiment.GetEmotion() != WordSentiment.Emotion.NULL)
                        {
                            aggregateEmotion[(int)sentiment.GetEmotion()] += weight;
                        }
                        if (sentiment.GetJudgment() != WordSentiment.Judgment.NULL)
                        {
                            aggregateJudgment[(int)sentiment.GetJudgment()] += weight;
                        }
                        if (sentiment.GetAgreement() != WordSentiment.Agreement.NULL)
                        {
                            aggregateAgreement[(int)sentiment.GetAgreement()] += weight;
                        }
                        if (sentiment.GetIntention() != WordSentiment.Intention.NULL)
                        {
                            aggregateIntention[(int)sentiment.GetIntention()] += weight;
                        }
#endif
                    }
                    Util.TaskQueue.Add("aggregateEnglishSentiment", UpdateAggregate,
                        englishSentimentDictionary[word]);
                }
            }
        }

        /// <summary>
        /// Returns accumulated sentiment analysis results and reset them.
        /// </summary>
        /// <returns>The results of sentiment analysis</returns>
        public override WordSentiment GetSentimentAndFlush()
        {
#if USE_NEW_SCHEME
            WordSentiment ret = new EnglishWordSentiment("", "", "", "", "");
#else
            WordSentiment ret = new EnglishWordSentiment("", "", "", "", "", "", "");
#endif

            if (!HasStart) return ret;

            void GetAggregate(object[] args)
            {
                Random r = new Random();

#if USE_NEW_SCHEME
                SentimentState.Valence v;
                SentimentState.Arousal a;
                double vv;
                double av;

                if (aggregateValence.Sum() <= 0)
                {
                    v = SentimentState.Valence.NULL;
                }
                else
                {
                    int max = 0;
                    List<int> argmax = new List<int>();
                    for (int k = 0; k < aggregateValence.Length; k++)
                    {
                        if (aggregateValence[k] > max)
                        {
                            max = aggregateValence[k];
                            argmax = new List<int>() { k };
                        }
                        else if (aggregateValence[k] == max)
                        {
                            argmax.Add(k);
                        }
                    }
                    v = (SentimentState.Valence)argmax[r.Next(argmax.Count)];
                }

                if (aggregateArousal.Sum() <= 0)
                {
                    a = SentimentState.Arousal.NULL;
                }
                else
                {
                    int max = 0;
                    List<int> argmax = new List<int>();
                    for (int k = 0; k < aggregateArousal.Length; k++)
                    {
                        if (aggregateArousal[k] > max)
                        {
                            max = aggregateArousal[k];
                            argmax = new List<int>() { k };
                        }
                        else if (aggregateArousal[k] == max)
                        {
                            argmax.Add(k);
                        }
                    }
                    a = (SentimentState.Arousal)argmax[r.Next(argmax.Count)];
                }

                if (aggregateCount == 0)
                {
                    vv = -2;
                    av = -2;
                }
                else
                {
                    vv = aggregateValenceValue / aggregateCount;
                    av = aggregateArousalValue / aggregateCount;
                }

                ret = new EnglishWordSentiment("", vv, av, v, a);
#else
                WordSentiment.Valence v;
                WordSentiment.StateIntensity si;
                WordSentiment.Emotion e;
                WordSentiment.Judgment j;
                WordSentiment.Agreement a;
                WordSentiment.Intention i;

                if (aggregateValence.Sum() <= 0)
                {
                    v = WordSentiment.Valence.NULL;
                }
                else
                {
                    int max = 0;
                    List<int> argmax = new List<int>();
                    for (int k = 0; k < aggregateValence.Length; k++)
                    {
                        if (aggregateValence[k] > max)
                        {
                            max = aggregateValence[k];
                            argmax = new List<int>() { k };
                        }
                        else if (aggregateValence[k] == max)
                        {
                            argmax.Add(k);
                        }
                    }
                    v = (WordSentiment.Valence)argmax[r.Next(argmax.Count)];
                }

                if (aggregateStateIntensity.Sum() <= 0)
                {
                    si = WordSentiment.StateIntensity.NULL;
                }
                else
                {
                    int max = 0;
                    List<int> argmax = new List<int>();
                    for (int k = 0; k < aggregateStateIntensity.Length; k++)
                    {
                        if (aggregateStateIntensity[k] > max)
                        {
                            max = aggregateStateIntensity[k];
                            argmax = new List<int>() { k };
                        }
                        else if (aggregateStateIntensity[k] == max)
                        {
                            argmax.Add(k);
                        }
                    }
                    si = (WordSentiment.StateIntensity)argmax[r.Next(argmax.Count)];
                }

                if (aggregateEmotion.Sum() <= 0)
                {
                    e = WordSentiment.Emotion.NULL;
                }
                else
                {
                    int max = 0;
                    List<int> argmax = new List<int>();
                    for (int k = 0; k < aggregateEmotion.Length; k++)
                    {
                        if (aggregateEmotion[k] > max)
                        {
                            max = aggregateEmotion[k];
                            argmax = new List<int>() { k };
                        }
                        else if (aggregateEmotion[k] == max)
                        {
                            argmax.Add(k);
                        }
                    }
                    e = (WordSentiment.Emotion)argmax[r.Next(argmax.Count)];
                }

                if (aggregateJudgment.Sum() <= 0)
                {
                    j = WordSentiment.Judgment.NULL;
                }
                else
                {
                    int max = 0;
                    List<int> argmax = new List<int>();
                    for (int k = 0; k < aggregateJudgment.Length; k++)
                    {
                        if (aggregateJudgment[k] > max)
                        {
                            max = aggregateJudgment[k];
                            argmax = new List<int>() { k };
                        }
                        else if (aggregateJudgment[k] == max)
                        {
                            argmax.Add(k);
                        }
                    }
                    j = (WordSentiment.Judgment)argmax[r.Next(argmax.Count)];
                }

                if (aggregateAgreement.Sum() <= 0)
                {
                    a = WordSentiment.Agreement.NULL;
                }
                else
                {
                    int max = 0;
                    List<int> argmax = new List<int>();
                    for (int k = 0; k < aggregateAgreement.Length; k++)
                    {
                        if (aggregateAgreement[k] > max)
                        {
                            max = aggregateAgreement[k];
                            argmax = new List<int>() { k };
                        }
                        else if (aggregateAgreement[k] == max)
                        {
                            argmax.Add(k);
                        }
                    }
                    a = (WordSentiment.Agreement)argmax[r.Next(argmax.Count)];
                }

                if (aggregateIntention.Sum() <= 0)
                {
                    i = WordSentiment.Intention.NULL;
                }
                else
                {
                    int max = 0;
                    List<int> argmax = new List<int>();
                    for (int k = 0; k < aggregateIntention.Length; k++)
                    {
                        if (aggregateIntention[k] > max)
                        {
                            max = aggregateIntention[k];
                            argmax = new List<int>() { k };
                        }
                        else if (aggregateIntention[k] == max)
                        {
                            argmax.Add(k);
                        }
                    }
                    i = (WordSentiment.Intention)argmax[r.Next(argmax.Count)];
                }

                ret = new EnglishWordSentiment("", v, si, e, j, a, i);
#endif
            }

            Util.TaskQueue.Add("aggregateEnglishSentiment", GetAggregate);
            Util.TaskQueue.Add("aggregateEnglishSentiment", InitializeAggregate);

            return ret;
        }

        /// <summary>
        /// Reset accumulated results of sentiment analysis.
        /// Must be called using Util.TaskQueue whose lockName is "aggregateEnglishSentiment".
        /// </summary>
        /// <param name="args"></param>
        private void InitializeAggregate(object[] args)
        {
            aggregateValence = new int[3] { 0, 0, 0 };
#if USE_NEW_SCHEME
            aggregateArousal = new int[3] { 0, 0, 0 };
            aggregateCount = 0;
            aggregateValenceValue = 0;
            aggregateArousalValue = 0;
#else
            aggregateStateIntensity = new int[3] { 0, 0, 0 };
            aggregateEmotion = new int[4] { 0, 0, 0, 0 };
            aggregateJudgment = new int[4] { 0, 0, 0, 0 };
            aggregateAgreement = new int[4] { 0, 0, 0, 0 };
            aggregateIntention = new int[4] { 0, 0, 0, 0 };
#endif
        }
    }
}
