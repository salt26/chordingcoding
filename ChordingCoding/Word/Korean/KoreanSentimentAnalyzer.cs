using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moda.Korean.TwitterKoreanProcessorCS;

namespace ChordingCoding.Word.Korean
{
    /// <summary>
    /// 한글 문자열에 대한 감정 분석기 클래스입니다.
    /// </summary>
    public class KoreanSentimentAnalyzer : SentimentAnalyzer
    {
        #region Old design (deprecated)
        // TODO 사전과 악보 저장 기능 (Dictionary 사용)
        // 사전에 저장하는 단어는 일단 전부 영어로(한글의 경우 두벌식 키보드 기준 영어로 친 그대로) 하고, Shift는 누르지 않은 상태로 저장한다.
        // InterceptKeys.cs의 콜백 함수에서 새 일반 문자를 입력할 때마다 상태 문자열이 사전에 있는지 검색해야 한다.
        // 검색 함수에서 단어를 찾으면, 해당 악보를 재생해야 한다.
        // 악보를 재생하는 동안 주 멜로디 및 자동 반주의 음량을 상당히 줄여야 한다. (20 ~ 50% 수준)
        // 위의 기능들을 완성하면 InterceptKeys.cs의 디버깅 코드들은 주석처리하자.

        // 나중에 사용자가 코드 수정 없이 직접 단어를 추가할 수 있게 하려면,
        // Properties.Settings 변수 또는 별도의 저장 파일을 만들어서 단어 목록을 저장해야 한다.
        #endregion

        // TODO ChordingCoding이 시작할 때 CSV 파일을 읽어와서 Dictionary에 로딩*
        // InterceptKeys.cs에서 여기에 문자열을 넘기면(함수 호출),
        // 문자열을 TwitterKoreanProcessorCS로 분석하고
        // 토큰을 하나, 둘 또는 셋씩 묶어서 Dictionary에서 검색해 본다.*
        // 검색 결과를 가지고 입력된 문자열에서 발생한 감정 이벤트를 일으킨다.
        // (예: 다음 화음의 성격 결정, 빠르기 변화, 조성 변화, 반주 패턴 변화, 악기 변화 등)

        // TODO 영어 감정 사전도 지원하기

        const int NUM_OF_GRAMS = 1;

        private List<Util.CSVReader> hangulSentimentCSV;
        private List<Dictionary<string, KoreanWordSentiment>> koreanSentimentDictionary;

        // aggregateKoreanSentiment
        private int[] aggregatePolarity;
        private int[] aggregateIntensity;
        private int[] aggregateSubjectivityType;
        private int[] aggregateSubjectivityPolarity;

        public static KoreanSentimentAnalyzer instance = null;

        /// <summary>
        /// 한국어 감정 분석기를 초기화합니다.
        /// 한국어 감정 분석기는 하나만 존재할 수 있습니다.
        /// </summary>
        public KoreanSentimentAnalyzer()
        {
            Initialize();
        }

        /// <summary>
        /// 감정 사전을 로딩하여 감정 분석기를 초기화합니다.
        /// </summary>
        public override void Initialize()
        {
            if (!(instance is null)) return;
            instance = this;

            #region Load TwitterKoreanProcessor (morpheme analysis engine)
            System.Threading.Tasks.Task.Run(() => {
                var a = TwitterKoreanProcessorCS.Normalize("초기화");
                var b = TwitterKoreanProcessorCS.Tokenize(a);
                b = TwitterKoreanProcessorCS.Stem(b);
                var c = TwitterKoreanProcessorCS.TokensToStrings(b);
            });
            #endregion

            #region Load Korean sentiment dictionaries

            /*
             * HangulSentiment1.csv is created by MorphemeParser(https://github.com/salt26/morpheme-parser).
             * The original data source is Korean Sentiment Lexicon(http://word.snu.ac.kr/kosac/lexicon.php).
             */
            hangulSentimentCSV = new List<Util.CSVReader>();
            koreanSentimentDictionary = new List<Dictionary<string, KoreanWordSentiment>>();

            for (int i = 0; i < NUM_OF_GRAMS; i++)
            {
                hangulSentimentCSV.Add(new Util.CSVReader("HangulSentiment" + (i + 1) + ".csv", true));
                koreanSentimentDictionary.Add(new Dictionary<string, KoreanWordSentiment>());

                foreach (List<string> row in hangulSentimentCSV[i].GetData())
                {
                    koreanSentimentDictionary[i].Add(row[0], new KoreanWordSentiment(row[0], row[1], row[2], row[3], row[4]));
                }
            }
            
            Util.TaskQueue.Add("aggregateKoreanSentiment", InitializeAggregate);
            #endregion

            IsReady = true;
        }

        /// <summary>
        /// 한국어 input을 감정 분석합니다.
        /// 감정 분석 결과는 다음 GetSentimentAndFlush() 호출 시까지 누적됩니다.
        /// </summary>
        /// <param name="input">한국어 문자열</param>
        public override void Analyze(string input)
        {
            if (!IsReady) return;

            string a = Hangul.Assemble(input, true);
            a = TwitterKoreanProcessorCS.Normalize(a);
            var b = TwitterKoreanProcessorCS.Tokenize(a);
            b = TwitterKoreanProcessorCS.Stem(b);
            b = b.SkipWhile((e) => !(
                e.Pos == KoreanPos.Adjective ||
                e.Pos == KoreanPos.Adverb ||
                e.Pos == KoreanPos.Exclamation ||
                e.Pos == KoreanPos.Noun ||
                e.Pos == KoreanPos.NounPrefix ||
                e.Pos == KoreanPos.Verb ||
                e.Pos == KoreanPos.VerbPrefix
            ));
            var c = TwitterKoreanProcessorCS.TokensToStrings(b);
            int count = c.Count();
            List<string> tokens = c.ToList();

            for (int j = 1; j <= NUM_OF_GRAMS; j++)
            {
                for (int i = 0; i < count - j + 1; i++)
                {
                    string word = String.Join(";", tokens.GetRange(i, j));
                    Console.WriteLine(word);
                    if (koreanSentimentDictionary[j - 1].ContainsKey(word))
                    {
                        void UpdateAggregate(object[] args)
                        {
                            KoreanWordSentiment sentiment = args[0] as KoreanWordSentiment;
                            int weight = 1;
                            aggregatePolarity[(int)sentiment.Polarity] += weight;
                            aggregateIntensity[(int)sentiment.Intensity] += weight;
                            aggregateSubjectivityType[(int)sentiment.SubjectivityType] += weight;
                            aggregateSubjectivityPolarity[(int)sentiment.SubjectivityPolarity] += weight;
                        }
                        Util.TaskQueue.Add("aggregateKoreanSentiment", UpdateAggregate,
                            koreanSentimentDictionary[j - 1][word]);
                    }
                }
            }
        }
        
        /// <summary>
        /// 현재까지 누적된 단어들의 감정 분석 결과를 반환하고 이를 초기화합니다.
        /// </summary>
        /// <returns>감정 분석 결과. Get...() 함수로 원하는 값을 가져올 수 있습니다.</returns>
        public override WordSentiment GetSentimentAndFlush()
        {
            KoreanWordSentiment.PolarityValue p = KoreanWordSentiment.PolarityValue.NULL;
            KoreanWordSentiment.IntensityValue i = KoreanWordSentiment.IntensityValue.NULL;
            KoreanWordSentiment.SubjectivityTypeValue st = KoreanWordSentiment.SubjectivityTypeValue.NULL;
            KoreanWordSentiment.SubjectivityPolarityValue sp = KoreanWordSentiment.SubjectivityPolarityValue.NULL;

            void GetAggregate(object[] args)
            {
                Random r = new Random();

                // Polarity -> take argmax
                if (aggregatePolarity.Sum() <= 0)
                {
                    p = KoreanWordSentiment.PolarityValue.NULL;
                }
                else
                {
                    int max = 0;
                    List<int> argmax = new List<int>();
                    for (int j = 0; j < aggregatePolarity.Length; j++)
                    {
                        if (aggregatePolarity[j] > max)
                        {
                            max = aggregatePolarity[j];
                            argmax = new List<int>() { j };
                        }
                        else if (aggregatePolarity[j] == max)
                        {
                            argmax.Add(j);
                        }
                    }
                    p = (KoreanWordSentiment.PolarityValue)argmax[r.Next(argmax.Count)];
                }

                // Intensity -> take argmax
                if (aggregateIntensity.Sum() <= 0)
                {
                    i = KoreanWordSentiment.IntensityValue.NULL;
                }
                else
                {
                    int max = 0;
                    List<int> argmax = new List<int>();
                    for (int j = 0; j < aggregateIntensity.Length; j++)
                    {
                        if (aggregateIntensity[j] > max)
                        {
                            max = aggregateIntensity[j];
                            argmax = new List<int>() { j };
                        }
                        else if (aggregateIntensity[j] == max)
                        {
                            argmax.Add(j);
                        }
                    }
                    i = (KoreanWordSentiment.IntensityValue)argmax[r.Next(argmax.Count)];
                }

                // SubjectivityType -> take argmax
                if (aggregateSubjectivityType.Sum() <= 0)
                {
                    st = KoreanWordSentiment.SubjectivityTypeValue.NULL;
                }
                else
                {
                    int max = 0;
                    List<int> argmax = new List<int>();
                    for (int j = 0; j < aggregateSubjectivityType.Length; j++)
                    {
                        if (aggregateSubjectivityType[j] > max)
                        {
                            max = aggregateSubjectivityType[j];
                            argmax = new List<int>() { j };
                        }
                        else if (aggregateSubjectivityType[j] == max)
                        {
                            argmax.Add(j);
                        }
                    }
                    st = (KoreanWordSentiment.SubjectivityTypeValue)argmax[r.Next(argmax.Count)];
                }

                // SubjectivityPolarity -> take argmax
                if (aggregateSubjectivityPolarity.Sum() <= 0)
                {
                    sp = KoreanWordSentiment.SubjectivityPolarityValue.NULL;
                }
                else
                {
                    int max = 0;
                    List<int> argmax = new List<int>();
                    for (int j = 0; j < aggregateSubjectivityPolarity.Length; j++)
                    {
                        if (aggregateSubjectivityPolarity[j] > max)
                        {
                            max = aggregateSubjectivityPolarity[j];
                            argmax = new List<int>() { j };
                        }
                        else if (aggregateSubjectivityPolarity[j] == max)
                        {
                            argmax.Add(j);
                        }
                    }
                    sp = (KoreanWordSentiment.SubjectivityPolarityValue)argmax[r.Next(argmax.Count)];
                }
            }

            Util.TaskQueue.Add("aggregateKoreanSentiment", GetAggregate);
            //Util.TaskQueue.Add("aggregateKoreanSentiment", PrintAggregate);
            Util.TaskQueue.Add("aggregateKoreanSentiment", InitializeAggregate);

            return new KoreanWordSentiment("", p, i, st, sp);
        }

        private void PrintAggregate(object[] args)
        {
            string s = "";
            for (int i = 0; i < aggregatePolarity.Length; i++)
            {
                s += ((KoreanWordSentiment.PolarityValue)i).ToString() + ": " + aggregatePolarity[i] + "\t";
            }
            s += "\n";
            for (int i = 0; i < aggregateIntensity.Length; i++)
            {
                s += ((KoreanWordSentiment.IntensityValue)i).ToString() + ": " + aggregateIntensity[i] + "\t";
            }
            s += "\n";
            for (int i = 0; i < aggregateSubjectivityType.Length; i++)
            {
                s += ((KoreanWordSentiment.SubjectivityTypeValue)i).ToString() + ": " + aggregateSubjectivityType[i] + "\t";
            }
            s += "\n";
            for (int i = 0; i < aggregateSubjectivityPolarity.Length; i++)
            {
                s += ((KoreanWordSentiment.SubjectivityPolarityValue)i).ToString() + ": " + aggregateSubjectivityPolarity[i] + "\t";
            }
            Console.WriteLine(s);
        }

        /// <summary>
        /// 감정 분석 총계를 초기화합니다.
        /// 반드시 "aggregateKoreanSentiment"라는 lockName의 Util.TaskQueue로 실행되어야 합니다.
        /// </summary>
        private void InitializeAggregate(object[] args)
        {
            aggregatePolarity = new int[5] { 0, 0, 0, 0, 0 };
            aggregateIntensity = new int[4] { 0, 0, 0, 0 };
            aggregateSubjectivityType = new int[7] { 0, 0, 0, 0, 0, 0, 0 };
            aggregateSubjectivityPolarity = new int[4] { 0, 0, 0, 0 };
        }
    }
}
