using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moda.Korean.TwitterKoreanProcessorCS;

namespace ChordingCoding.Word
{
    /// <summary>
    /// 한글 문자열에 대한 감정 분석기 클래스입니다.
    /// </summary>
    class SentimentAnalyzer
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
        // 토큰을 하나, 둘 또는 셋씩 묶어서 Dictionary에서 검색해 본다.
        // 검색 결과를 가지고 입력된 문자열에서 발생한 감정 이벤트를 일으킨다.
        // (예: 다음 화음의 성격 결정, 빠르기 변화, 조성 변화, 반주 패턴 변화, 악기 변화 등)

        // TODO 영어 감정 사전도 지원하기

        static Util.CSVReader hangulSentimentCSV1;
        static Util.CSVReader hangulSentimentCSV2;
        static Util.CSVReader hangulSentimentCSV3;
        static List<Dictionary<string, WordSentiment>> koreanSentimentDictionary;

        // aggregateSentiment
        static int[] aggregatePolarity;
        static int[] aggregateIntensity;
        static int[] aggregateSubjectivityType;
        static int[] aggregateSubjectivityPolarity;

        public static bool IsReady { get; private set; } = false;

        /// <summary>
        /// 감정 사전을 로딩하여 감정 분석기를 초기화합니다.
        /// </summary>
        public static void Initialize()
        {
            System.Threading.Tasks.Task.Run(() => TwitterKoreanProcessorCS.Normalize("초기화"));     // Initial loading

            #region Load Korean dictionaries
            hangulSentimentCSV1 = new Util.CSVReader("HangulSentiment1.csv", true);
            hangulSentimentCSV2 = new Util.CSVReader("HangulSentiment2.csv", true);
            hangulSentimentCSV3 = new Util.CSVReader("HangulSentiment3.csv", true);
            koreanSentimentDictionary = new List<Dictionary<string, WordSentiment>>
            {
                new Dictionary<string, WordSentiment>(),
                new Dictionary<string, WordSentiment>(),
                new Dictionary<string, WordSentiment>()
            };
            foreach (List<string> row in hangulSentimentCSV1.GetData())
            {
                koreanSentimentDictionary[0].Add(row[0], new WordSentiment(row[0], row[1], row[2], row[3], row[4]));
            }
            foreach (List<string> row in hangulSentimentCSV2.GetData())
            {
                koreanSentimentDictionary[1].Add(row[0], new WordSentiment(row[0], row[1], row[2], row[3], row[4]));
            }
            foreach (List<string> row in hangulSentimentCSV3.GetData())
            {
                koreanSentimentDictionary[2].Add(row[0], new WordSentiment(row[0], row[1], row[2], row[3], row[4]));
            }
            #endregion
            
            Util.TaskQueue.Add("aggregateSentiment", InitializeAggregate);

            IsReady = true;
            
            string sample1 = "그는 밝은 미소를 지으며 의미심장한 말을 꺼냈다.";
            foreach (string s in sample1.Split(' '))
            {
                AnalyzeKorean(s);
                GetSentimentAndFlush().Print();
            }
            Console.WriteLine(sample1);
        }

        public static void AnalyzeKorean(string input)
        {
            if (!IsReady) return;

            string a = Hangul.Assemble(input, true);
            a = TwitterKoreanProcessorCS.Normalize(a);
            var b = TwitterKoreanProcessorCS.Tokenize(a);
            //b = TwitterKoreanProcessorCS.Stem(b);
            var c = TwitterKoreanProcessorCS.TokensToStrings(b);
            int count = c.Count();
            List<string> tokens = c.ToList();

            for (int j = 1; j <= 3; j++)
            {
                for (int i = 0; i < count - j + 1; i++)
                {
                    string word = String.Join(";", tokens.GetRange(i, j));
                    Console.WriteLine(word);
                    if (koreanSentimentDictionary[j - 1].ContainsKey(word))
                    {
                        void UpdateAggregate(object[] args)
                        {
                            WordSentiment sentiment = args[0] as WordSentiment;
                            int weight = 1;
                            aggregatePolarity[(int)sentiment.Polarity] += weight;
                            aggregateIntensity[(int)sentiment.Intensity] += weight;
                            aggregateSubjectivityType[(int)sentiment.SubjectivityType] += weight;
                            aggregateSubjectivityPolarity[(int)sentiment.SubjectivityPolarity] += weight;
                        }
                        Util.TaskQueue.Add("aggregateSentiment", UpdateAggregate,
                            koreanSentimentDictionary[j - 1][word]);
                    }
                }
            }
        }
        
        /// <summary>
        /// 현재까지 누적된 단어들의 감정 분석 결과를 반환하고 이를 초기화합니다.
        /// </summary>
        /// <returns>Polarity, Intensity, SubjectivityType, SubjectivityPolarity가 들어있는 감정 분석 결과</returns>
        public static WordSentiment GetSentimentAndFlush()
        {
            WordSentiment.PolarityValue p = WordSentiment.PolarityValue.NULL;
            WordSentiment.IntensityValue i = WordSentiment.IntensityValue.NULL;
            WordSentiment.SubjectivityTypeValue st = WordSentiment.SubjectivityTypeValue.NULL;
            WordSentiment.SubjectivityPolarityValue sp = WordSentiment.SubjectivityPolarityValue.NULL;

            void GetAggregate(object[] args)
            {
                List<WordSentiment.PolarityValue> pBox = new List<WordSentiment.PolarityValue>();
                List<WordSentiment.IntensityValue> iBox = new List<WordSentiment.IntensityValue>();
                Random r = new Random();

                // Polarity -> drawing a lot from cubic frequencies
                if (aggregatePolarity.Sum() <= 0)
                {
                    p = WordSentiment.PolarityValue.NULL;
                }
                else
                {
                    for (int j = 0; j < aggregatePolarity.Length; j++)
                    {
                        for (int k = 0; k < aggregatePolarity[j] * aggregatePolarity[j] * aggregatePolarity[j]; k++)
                        {
                            pBox.Add((WordSentiment.PolarityValue)j);
                        }
                    }
                    p = pBox[r.Next(pBox.Count)];
                }

                // Intensity -> drawing a lot from cubic frequencies
                if (aggregateIntensity.Sum() <= 0)
                {
                    i = WordSentiment.IntensityValue.NULL;
                }
                else
                {
                    for (int j = 0; j < aggregateIntensity.Length; j++)
                    {
                        for (int k = 0; k < aggregateIntensity[j] * aggregateIntensity[j] * aggregateIntensity[j]; k++)
                        {
                            iBox.Add((WordSentiment.IntensityValue)j);
                        }
                    }
                    i = iBox[r.Next(iBox.Count)];
                }

                // SubjectivityType -> take argmax
                if (aggregateSubjectivityType.Sum() <= 0)
                {
                    st = WordSentiment.SubjectivityTypeValue.NULL;
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
                    st = (WordSentiment.SubjectivityTypeValue)argmax[r.Next(argmax.Count)];
                }

                // SubjectivityPolarity -> take argmax
                if (aggregateSubjectivityPolarity.Sum() <= 0)
                {
                    sp = WordSentiment.SubjectivityPolarityValue.NULL;
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
                    sp = (WordSentiment.SubjectivityPolarityValue)argmax[r.Next(argmax.Count)];
                }
            }

            Util.TaskQueue.Add("aggregateSentiment", GetAggregate);
            Util.TaskQueue.Add("aggregateSentiment", InitializeAggregate);

            return new WordSentiment("", p, i, st, sp);
        }

        private static void PrintAggregate(object[] args)
        {
            string s = "";
            for (int i = 0; i < aggregatePolarity.Length; i++)
            {
                s += ((WordSentiment.PolarityValue)i).ToString() + ": " + aggregatePolarity[i] + "\t";
            }
            s += "\n";
            for (int i = 0; i < aggregateIntensity.Length; i++)
            {
                s += ((WordSentiment.IntensityValue)i).ToString() + ": " + aggregateIntensity[i] + "\t";
            }
            s += "\n";
            for (int i = 0; i < aggregateSubjectivityType.Length; i++)
            {
                s += ((WordSentiment.SubjectivityTypeValue)i).ToString() + ": " + aggregateSubjectivityType[i] + "\t";
            }
            s += "\n";
            for (int i = 0; i < aggregateSubjectivityPolarity.Length; i++)
            {
                s += ((WordSentiment.SubjectivityPolarityValue)i).ToString() + ": " + aggregateSubjectivityPolarity[i] + "\t";
            }
            Console.WriteLine(s);
        }

        /// <summary>
        /// 감정 분석 총계를 초기화합니다.
        /// 반드시 "aggregateSentiment"라는 lockName의 Util.TaskQueue로 실행되어야 합니다.
        /// </summary>
        private static void InitializeAggregate(object[] args)
        {
            aggregatePolarity = new int[5] { 0, 0, 0, 0, 0 };
            aggregateIntensity = new int[4] { 0, 0, 0, 0 };
            aggregateSubjectivityType = new int[7] { 0, 0, 0, 0, 0, 0, 0 };
            aggregateSubjectivityPolarity = new int[4] { 0, 0, 0, 0 };
        }
    }
    
    /// <summary>
    /// 단어의 감정 정보를 담는 구조체입니다.
    /// </summary>
    public class WordSentiment
    {
        public enum PolarityValue { COMP = 0, NEG = 1, NEUT = 2, NONE = 3, POS = 4, NULL = -1 };
        public enum IntensityValue { High = 0, Low = 1, Medium = 2, None = 3, NULL = -1 };
        public enum SubjectivityTypeValue {
            Agreement = 0, Argument = 1, Emotion = 2, Intention = 3,
            Judgment = 4, Others = 5, Speculation = 6, NULL = -1
        };
        public enum SubjectivityPolarityValue { COMP = 0, NEG = 1, NEUT = 2, POS = 3, NULL = -1 };

        public string Word { get; }
        public PolarityValue Polarity { get; }
        public IntensityValue Intensity { get; }
        public SubjectivityTypeValue SubjectivityType { get; }
        public SubjectivityPolarityValue SubjectivityPolarity { get; }

        public WordSentiment(string word, PolarityValue polarity,
            IntensityValue intensity, SubjectivityTypeValue subjectivityType,
            SubjectivityPolarityValue subjectivityPolarity)
        {
            Word = word;
            Polarity = polarity;
            Intensity = intensity;
            SubjectivityType = subjectivityType;
            SubjectivityPolarity = subjectivityPolarity;
        }

        public WordSentiment(string word, string polarity, string intensity,
            string subjectivityType, string subjectivityPolarity)
        {
            Word = word;
            Polarity = StringToPolarity(polarity);
            Intensity = StringToIntensity(intensity);
            SubjectivityType = StringToSubjectivityType(subjectivityType);
            SubjectivityPolarity = StringToSubjectivityPolarity(subjectivityPolarity);
        }

        public PolarityValue StringToPolarity(string polarity)
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
        
        public IntensityValue StringToIntensity(string intensity)
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

        public SubjectivityTypeValue StringToSubjectivityType(string subjectivityType)
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

        public SubjectivityPolarityValue StringToSubjectivityPolarity(string subjectivityPolarity)
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

        public void Print()
        {
            string s = "Polarity: " + Polarity.ToString() + "\tIntensity: " + Intensity.ToString() +
                "\tSubjectivityType: " + SubjectivityType.ToString() + "\tSubjectivityPolarity: " +
                SubjectivityPolarity.ToString();
            Console.WriteLine(s);
        }
    }
}
