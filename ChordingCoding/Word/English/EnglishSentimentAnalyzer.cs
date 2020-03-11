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
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

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
        private int[] aggregateStateIntensity;
        private int[] aggregateEmotion;
        private int[] aggregateJudgment;
        private int[] aggregateAgreement;
        private int[] aggregateIntention;

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

            #region Load English sentiment dictionary
            /*
             * EnglishSentiment.csv is created by MorphemeParser(https://github.com/salt26/morpheme-parser).
             * The original data source is Inquirer Dictionary(http://www.wjh.harvard.edu/~inquirer/homecat.htm).
             */
            englishSentimentCSV = new Util.CSVReader("EnglishSentiment.csv", true);
            englishSentimentDictionary = new Dictionary<string, EnglishWordSentiment>();

            foreach (List<string> row in englishSentimentCSV.GetData())
            {
                englishSentimentDictionary.Add(row[0], new EnglishWordSentiment(
                    row[0], row[1], row[2], row[3], row[4], row[5], row[6]));
            }

            Util.TaskQueue.Add("aggregateEnglishSentiment", InitializeAggregate);
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
            if (!IsReady) return;

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
            WordSentiment ret = new EnglishWordSentiment("", "", "", "", "", "", "");

            void GetAggregate(object[] args)
            {
                Random r = new Random();

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
            aggregateValence = new int[4] { 0, 0, 0, 0 };
            aggregateStateIntensity = new int[3] { 0, 0, 0 };
            aggregateEmotion = new int[4] { 0, 0, 0, 0 };
            aggregateJudgment = new int[4] { 0, 0, 0, 0 };
            aggregateAgreement = new int[4] { 0, 0, 0, 0 };
            aggregateIntention = new int[4] { 0, 0, 0, 0 };
        }
    }
}
