using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        static Dictionary<string, WordSentiment> hangulSentiment1;
        static Dictionary<string, WordSentiment> hangulSentiment2;
        static Dictionary<string, WordSentiment> hangulSentiment3;

        public static void Initialize()
        {
            #region Load Hangul dictionaries
            hangulSentimentCSV1 = new Util.CSVReader("HangulSentiment1.csv", true);
            hangulSentimentCSV2 = new Util.CSVReader("HangulSentiment2.csv", true);
            hangulSentimentCSV3 = new Util.CSVReader("HangulSentiment3.csv", true);
            hangulSentiment1 = new Dictionary<string, WordSentiment>();
            hangulSentiment2 = new Dictionary<string, WordSentiment>();
            hangulSentiment3 = new Dictionary<string, WordSentiment>();
            foreach (List<string> row in hangulSentimentCSV1.GetData())
            {
                hangulSentiment1.Add(row[0], new WordSentiment(row[0], row[1], row[2], row[3], row[4]));
            }
            foreach (List<string> row in hangulSentimentCSV2.GetData())
            {
                hangulSentiment2.Add(row[0], new WordSentiment(row[0], row[1], row[2], row[3], row[4]));
            }
            foreach (List<string> row in hangulSentimentCSV3.GetData())
            {
                hangulSentiment3.Add(row[0], new WordSentiment(row[0], row[1], row[2], row[3], row[4]));
            }
            #endregion
        }

    }
    
    /// <summary>
    /// 단어의 감정 정보를 담는 구조체입니다.
    /// </summary>
    public class WordSentiment
    {
        public enum PolarityValue { COMP, NEG, NEUT, NONE, POS };
        public enum IntensityValue { High, Low, Medium, None };
        public enum SubjectivityTypeValue { Agreement, Argument,
            Emotion, Intention, Judgment, Others, Speculation };
        public enum SubjectivityPolarityValue { COMP, NEG, NEUT, POS };

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
                default: throw new ArgumentException("Invalid Polarity string");
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
                default: throw new ArgumentException("Invalid Intensity string");
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
                default: throw new ArgumentException("Invalid SubjectivityType string");
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
                default: throw new ArgumentException("Invalid SubjectivityPolarity string");
            }
        }
    }
}
