using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChordingCoding.Word
{
    /// <summary>
    /// 한글을 자음과 모음 단위로 분해하거나 재결합하는 클래스입니다.
    /// </summary>
    class Hangul
    {
        /// <summary>
        /// 한글이 포함된 문자열을 자음과 모음 단위로 분해합니다.
        /// 쌍자음("ㅃ" 등)은 분해하지 않고 겹자음("ㄳ" 등)과 겹모음("ㅙ" 등)은 분해합니다.
        /// (예: "괜찮아" -> "ㄱㅗㅐㄴㅊㅏㄴㅎㅇㅏ")
        /// </summary>
        /// <param name="sentence">문자열</param>
        /// <param name="onlyHangul">이 값이 true이면 한글이 아닌 문자를 모두 제거합니다.</param>
        /// <returns>분해한 문자열</returns>
        public static string Disassemble(string sentence, bool onlyHangul = false)
        {
            string ret = "";
            char[] c = sentence.ToCharArray();
            for (int i = 0; i < c.Length; i++)
            {
                int j = c[i];
                if (j >= 44032 && j < 55204)
                {
                    // c[i] is Hangul
                    int choseong = (j - 44032) / 588;
                    int jungseong = ((j - 44032) % 588) / 28;
                    int jongseong = ((j - 44032) % 588) % 28;
                    ret += Choseong(choseong) + Jungseong(jungseong) + Jongseong(jongseong);
                }
                else if (j >= 12593 && j < 12644)
                {
                    ret += Separate(j);
                }
                else if (!onlyHangul)
                {
                    ret += c[i];
                }
            }
            return ret;
        }

        /// <summary>
        /// 한글이 포함된 문자열을 자음과 모음 단위로 분해했다가 재결합합니다.
        /// (예: "이ㅂ닏ㅏ" -> "입니다")
        /// </summary>
        /// <param name="sentence">문자열</param>
        /// <param name="onlyHangul">이 값이 true이면 한글이 아닌 문자를 모두 제거합니다.</param>
        /// <returns>재결합한 문자열</returns>
        public static string Assemble(string sentence, bool onlyHangul = false)
        {
            string s = Disassemble(sentence, onlyHangul);
            string ret = "";
            string token = "";
            char[] c = s.ToCharArray();
            for (int i = 0; i < c.Length; i++)
            {
                int j = c[i];
                if (j >= 12593 && j < 12644)
                {
                    // c[i] is Hangul consonant or vowel
                    token += c[i];
                }
                else
                {
                    if (token.Length > 0)
                    {
                        ret += AssembleOnlyHangul(token);
                    }
                    token = "";
                    ret += c[i];
                }
            }
            if (token.Length > 0)
            {
                ret += AssembleOnlyHangul(token);
            }
            return ret;
        }

        /// <summary>
        /// 두벌식 키보드 기준으로 영어 글자를 한글 글자로 변환합니다.
        /// </summary>
        /// <param name="english"></param>
        /// <returns></returns>
        public static string EnglishToKorean(string english, bool assemble = false)
        {
            string korean = "";
            for (int i = 0; i < english.Length; i++)
            {
                switch (english[i])
                {
                    case 'q': korean += 'ㅂ'; break;
                    case 'Q': korean += 'ㅃ'; break;
                    case 'w': korean += 'ㅈ'; break;
                    case 'W': korean += 'ㅉ'; break;
                    case 'e': korean += 'ㄷ'; break;
                    case 'E': korean += 'ㄸ'; break;
                    case 'r': korean += 'ㄱ'; break;
                    case 'R': korean += 'ㄲ'; break;
                    case 't': korean += 'ㅅ'; break;
                    case 'T': korean += 'ㅆ'; break;
                    case 'y':
                    case 'Y': korean += 'ㅛ'; break;
                    case 'u':
                    case 'U': korean += 'ㅕ'; break;
                    case 'i':
                    case 'I': korean += 'ㅑ'; break;
                    case 'o': korean += 'ㅐ'; break;
                    case 'O': korean += 'ㅒ'; break;
                    case 'p': korean += 'ㅔ'; break;
                    case 'P': korean += 'ㅖ'; break;
                    case 'a':
                    case 'A': korean += 'ㅁ'; break;
                    case 's':
                    case 'S': korean += 'ㄴ'; break;
                    case 'd':
                    case 'D': korean += 'ㅇ'; break;
                    case 'f':
                    case 'F': korean += 'ㄹ'; break;
                    case 'g':
                    case 'G': korean += 'ㅎ'; break;
                    case 'h':
                    case 'H': korean += 'ㅗ'; break;
                    case 'j':
                    case 'J': korean += 'ㅓ'; break;
                    case 'k':
                    case 'K': korean += 'ㅏ'; break;
                    case 'l':
                    case 'L': korean += 'ㅣ'; break;
                    case 'z':
                    case 'Z': korean += 'ㅋ'; break;
                    case 'x':
                    case 'X': korean += 'ㅌ'; break;
                    case 'c':
                    case 'C': korean += 'ㅊ'; break;
                    case 'v':
                    case 'V': korean += 'ㅍ'; break;
                    case 'b':
                    case 'B': korean += 'ㅠ'; break;
                    case 'n':
                    case 'N': korean += 'ㅜ'; break;
                    case 'm':
                    case 'M': korean += 'ㅡ'; break;
                    default: korean += english[i]; break;
                }
            }
            if (assemble)
                return Assemble(korean);
            else
                return korean;
        }

        #region Helper methods

        static string Choseong(int index)
        {
            switch (index)
            {
                case 0: return "ㄱ";
                case 1: return "ㄲ";
                case 2: return "ㄴ";
                case 3: return "ㄷ";
                case 4: return "ㄸ";
                case 5: return "ㄹ";
                case 6: return "ㅁ";
                case 7: return "ㅂ";
                case 8: return "ㅃ";
                case 9: return "ㅅ";
                case 10: return "ㅆ";
                case 11: return "ㅇ";
                case 12: return "ㅈ";
                case 13: return "ㅉ";
                case 14: return "ㅊ";
                case 15: return "ㅋ";
                case 16: return "ㅌ";
                case 17: return "ㅍ";
                case 18: return "ㅎ";
                default: return "";
            }
        }

        static string Jungseong(int index)
        {
            switch (index)
            {
                case 0: return "ㅏ";
                case 1: return "ㅐ";
                case 2: return "ㅑ";
                case 3: return "ㅒ";
                case 4: return "ㅓ";
                case 5: return "ㅔ";
                case 6: return "ㅕ";
                case 7: return "ㅖ";
                case 8: return "ㅗ";
                case 9: return "ㅗㅏ";
                case 10: return "ㅗㅐ";
                case 11: return "ㅗㅣ";
                case 12: return "ㅛ";
                case 13: return "ㅜ";
                case 14: return "ㅜㅓ";
                case 15: return "ㅜㅔ";
                case 16: return "ㅜㅣ";
                case 17: return "ㅠ";
                case 18: return "ㅡ";
                case 19: return "ㅡㅣ";
                case 20: return "ㅣ";
                default: return "";
            }
        }

        static string Jongseong(int index)
        {
            switch (index)
            {
                case 0: return "";
                case 1: return "ㄱ";
                case 2: return "ㄲ";
                case 3: return "ㄱㅅ";
                case 4: return "ㄴ";
                case 5: return "ㄴㅈ";
                case 6: return "ㄴㅎ";
                case 7: return "ㄷ";
                case 8: return "ㄹ";
                case 9: return "ㄹㄱ";
                case 10: return "ㄹㅁ";
                case 11: return "ㄹㅂ";
                case 12: return "ㄹㅅ";
                case 13: return "ㄹㅌ";
                case 14: return "ㄹㅍ";
                case 15: return "ㄹㅎ";
                case 16: return "ㅁ";
                case 17: return "ㅂ";
                case 18: return "ㅂㅅ";
                case 19: return "ㅅ";
                case 20: return "ㅆ";
                case 21: return "ㅇ";
                case 22: return "ㅈ";
                case 23: return "ㅊ";
                case 24: return "ㅋ";
                case 25: return "ㅌ";
                case 26: return "ㅍ";
                case 27: return "ㅎ";
                default: return "";
            }
        }

        /// <summary>
        /// 겹자음과 겹모음을 각각 단자음과 단모음으로 분해합니다.
        /// </summary>
        /// <param name="charcode"></param>
        /// <returns></returns>
        static string Separate(int charcode)
        {
            switch (charcode)
            {
                case 12595: return "ㄱㅅ";
                case 12597: return "ㄴㅈ";
                case 12598: return "ㄴㅎ";
                case 12602: return "ㄹㄱ";
                case 12603: return "ㄹㅁ";
                case 12604: return "ㄹㅂ";
                case 12605: return "ㄹㅅ";
                case 12606: return "ㄹㅌ";
                case 12607: return "ㄹㅍ";
                case 12608: return "ㄹㅎ";
                case 12612: return "ㅂㅅ";
                case 12632: return "ㅗㅏ";
                case 12633: return "ㅗㅐ";
                case 12634: return "ㅗㅣ";
                case 12637: return "ㅜㅓ";
                case 12638: return "ㅜㅔ";
                case 12639: return "ㅜㅣ";
                case 12642: return "ㅡㅣ";
                default: return "" + (char)charcode;
            }
        }

        static bool IsVowel(int charcode)
        {
            if (charcode >= 12623 && charcode < 12644)
                return true;
            return false;
        }

        static bool IsConsonant(int charcode)
        {
            if (charcode >= 12593 && charcode < 12623)
                return true;
            return false;
        }

        /// <summary>
        /// 단자음(쌍자음 포함)을 초성 인덱스로 변환합니다.
        /// 겹자음이나 기타 문자가 들어오면 -1을 반환합니다.
        /// </summary>
        /// <param name="charcode"></param>
        /// <returns></returns>
        static int ChoseongToIndex(int charcode)
        {
            // 단자음만 입력으로 들어온다고 가정
            switch (charcode)
            {
                case 12593: return 0;
                case 12594: return 1;
                case 12596: return 2;
                case 12599: return 3;
                case 12600: return 4;
                case 12601: return 5;
                case 12609: return 6;
                case 12610: return 7;
                case 12611: return 8;
                case 12613: return 9;
                case 12614: return 10;
                case 12615: return 11;
                case 12616: return 12;
                case 12617: return 13;
                case 12618: return 14;
                case 12619: return 15;
                case 12620: return 16;
                case 12621: return 17;
                case 12622: return 18;
                default: return -1;
            }
        }

        /// <summary>
        /// 모음을 중성 인덱스로 변환합니다.
        /// 기타 문자가 들어오면 -1을 반환합니다.
        /// </summary>
        /// <param name="charcode"></param>
        /// <returns></returns>
        static int JungseongToIndex(int charcode)
        {
            // 단모음 또는 겹모음만 입력으로 들어온다고 가정
            switch (charcode)
            {
                case 12623: return 0;   // ㅏ
                case 12624: return 1;   // ㅐ
                case 12625: return 2;   // ㅑ
                case 12626: return 3;   // ㅒ
                case 12627: return 4;   // ㅓ
                case 12628: return 5;   // ㅔ
                case 12629: return 6;   // ㅕ
                case 12630: return 7;   // ㅖ
                case 12631: return 8;   // ㅗ
                case 12632: return 9;   // ㅘ
                case 12633: return 10;  // ㅙ
                case 12634: return 11;  // ㅚ
                case 12635: return 12;  // ㅛ
                case 12636: return 13;  // ㅜ
                case 12637: return 14;  // ㅝ
                case 12638: return 15;  // ㅞ
                case 12639: return 16;  // ㅟ
                case 12640: return 17;  // ㅠ
                case 12641: return 18;  // ㅡ
                case 12642: return 19;  // ㅢ
                case 12643: return 20;  // ㅣ
                default: return -1;
            }
        }

        /// <summary>
        /// 종성이 될 수 있는 자음을 종성 인덱스로 변환합니다.
        /// 종성이 없는 경우 charcode로 0을 넣어주면 됩니다.
        /// 'ㄸ', 'ㅃ', 'ㅉ'이나 자음이 아닌 문자가 들어오면 -1을 반환합니다.
        /// </summary>
        /// <param name="charcode"></param>
        /// <returns></returns>
        static int JongseongToIndex(int charcode)
        {
            // 종성으로 가능한 자음만 입력으로 들어온다고 가정
            switch (charcode)
            {
                case 0: return 0;       // 종성 없음
                case 12593: return 1;
                case 12594: return 2;
                case 12595: return 3;
                case 12596: return 4;
                case 12597: return 5;
                case 12598: return 6;
                case 12599: return 7;
                case 12601: return 8;
                case 12602: return 9;
                case 12603: return 10;
                case 12604: return 11;
                case 12605: return 12;
                case 12606: return 13;
                case 12607: return 14;
                case 12608: return 15;
                case 12609: return 16;
                case 12610: return 17;
                case 12612: return 18;
                case 12613: return 19;
                case 12614: return 20;
                case 12615: return 21;
                case 12616: return 22;
                case 12618: return 23;
                case 12619: return 24;
                case 12620: return 25;
                case 12621: return 26;
                case 12622: return 27;
                default: return -1;
            }
        }

        /// <summary>
        /// 겹자음과 겹모음을 하나로 합칩니다.
        /// 자음이나 모음이 종성이나 중성으로 오지 않고 단독으로 올 때 사용됩니다.
        /// 결합에 실패하면 빈 문자열을 반환합니다.
        /// </summary>
        /// <param name="twoCharacters">결합될 수 있는 두 단자음 또는 두 단모음</param>
        /// <returns></returns>
        static string Merge(string twoCharacters)
        {
            switch (twoCharacters)
            {
                case "ㄱㅅ": return "ㄳ";
                case "ㄴㅈ": return "ㄵ";
                case "ㄴㅎ": return "ㄶ";
                case "ㄹㄱ": return "ㄺ";
                case "ㄹㅁ": return "ㄻ";
                case "ㄹㅂ": return "ㄼ";
                case "ㄹㅅ": return "ㄽ";
                case "ㄹㅌ": return "ㄾ";
                case "ㄹㅍ": return "ㄿ";
                case "ㄹㅎ": return "ㅀ";
                case "ㅂㅅ": return "ㅄ";
                case "ㅗㅏ": return "ㅘ";
                case "ㅗㅐ": return "ㅙ";
                case "ㅗㅣ": return "ㅚ";
                case "ㅜㅓ": return "ㅝ";
                case "ㅜㅔ": return "ㅞ";
                case "ㅜㅣ": return "ㅟ";
                case "ㅡㅣ": return "ㅢ";
                default: return "";
            }
        }

        /// <summary>
        /// 두 단자음 또는 두 단모음이 결합하여 겹자음 또는 겹모음이 될 수 있는지 확인합니다.
        /// </summary>
        /// <param name="twoCharacters">결합될 수 있는 두 단자음 또는 두 단모음</param>
        /// <returns></returns>
        static bool CanMerge(string twoCharacters)
        {
            switch (twoCharacters)
            {
                case "ㄱㅅ":
                case "ㄴㅈ":
                case "ㄴㅎ":
                case "ㄹㄱ":
                case "ㄹㅁ":
                case "ㄹㅂ":
                case "ㄹㅅ":
                case "ㄹㅌ":
                case "ㄹㅍ":
                case "ㄹㅎ":
                case "ㅂㅅ":
                case "ㅗㅏ":
                case "ㅗㅐ":
                case "ㅗㅣ":
                case "ㅜㅓ":
                case "ㅜㅔ":
                case "ㅜㅣ":
                case "ㅡㅣ":
                    return true;
                default: return false;
            }
        }

        #endregion

        #region Character building methods

        /// <summary>
        /// 초성, 중성, 종성을 합친 글자 하나를 반환합니다.
        /// 입력된 인덱스가 잘못된 경우 빈 문자열을 반환합니다.
        /// </summary>
        /// <param name="choseong">초성 인덱스</param>
        /// <param name="jungseong">중성 인덱스. 겹모음 가능.</param>
        /// <param name="jongseong">종성 인덱스. 겹자음 가능. 없을 경우 0.</param>
        /// <returns></returns>
        static string BuildFromIndex(int choseong, int jungseong, int jongseong = 0)
        {
            if (choseong < 0 || choseong >= 19 ||
                jungseong < 0 || jungseong >= 21 ||
                jongseong < 0 || jongseong >= 28)
            {
                return "";
            }
            char c = (char)(44032 + 588 * choseong + 28 * jungseong + jongseong);
            return "" + c;
        }

        /// <summary>
        /// 초성, 중성, 종성을 합친 글자 하나를 반환합니다.
        /// 입력된 char 코드가 잘못된 경우 빈 문자열을 반환합니다.
        /// </summary>
        /// <param name="choseong">초성 char</param>
        /// <param name="jungseong">중성 char. 단모음.</param>
        /// <param name="mergedJungseong">결합 중성 char. 단모음. 없을 경우 0.</param>
        /// <param name="jongseong">종성 char. 단자음. 없을 경우 0.</param>
        /// <param name="mergedJongseong">결합 종성 char. 단자음. 없을 경우 0.</param>
        /// <returns></returns>
        static string Build(int choseongCharcode,
            int jungseongCharcode, int mergedJungseongCharcode,
            int jongseongCharcode, int mergedJongseongCharcode)
        {
            int choseong = ChoseongToIndex(choseongCharcode);
            if (choseong == -1) return "";

            int jungseong;
            if (mergedJungseongCharcode == 0)
            {
                jungseong = JungseongToIndex(jungseongCharcode);
            }
            else
            {
                string jungseongStr = "" + (char)jungseongCharcode + (char)mergedJungseongCharcode;
                jungseong = JungseongToIndex(Merge(jungseongStr)[0]);
            }
            if (jungseong == -1) return "";

            int jongseong;
            if (mergedJongseongCharcode == 0)
            {
                jongseong = JongseongToIndex(jongseongCharcode);
            }
            else
            {
                string jongseongStr = "" + (char)jongseongCharcode + (char)mergedJongseongCharcode;
                jongseong = JongseongToIndex(Merge(jongseongStr)[0]);
            }
            if (jongseong == -1) return "";

            return BuildFromIndex(choseong, jungseong, jongseong);
        }

        /// <summary>
        /// 단독 자음 또는 단독 모음을 결합하여 반환합니다.
        /// 입력된 char 코드가 잘못된 경우 빈 문자열을 반환합니다.
        /// </summary>
        /// <param name="soloCharcode">단독 자음/모음 char</param>
        /// <param name="mergedSoloCharcode">결합 단독 자음/모음 char. 없을 경우 0.</param>
        /// <returns></returns>
        static string Build(int soloCharcode, int mergedSoloCharcode)
        {
            if (IsConsonant(soloCharcode) || IsVowel(soloCharcode))
            {
                if (mergedSoloCharcode == 0)
                    return "" + (char)soloCharcode;
                else
                    return Merge("" + (char)soloCharcode + (char)mergedSoloCharcode);
            }
            else return "";
        }

        #endregion

        /// <summary>
        /// 한글 재결합 시 쓰이는, 확정되지 않은 초성/중성/종성 상태
        /// </summary>
        enum ChoJungJongState
        {
            None = 0,
            VowelSolo = 1, VS = 1,                              // 단독 모음
            VowelJungseong = 2, VJ = 2,                         // 중성
            VowelMergedSolo = 3, VMS = 3,                       // 결합 단독 모음 (앞의 모음과 결합)
            VowelMergedJungseong = 4, VMJ = 4,                  // 결합 중성 (앞의 모음과 결합)
            ConsonantSoloOrChoseong = 5, CSOC = 5,              // 단독 자음 또는 초성
            ConsonantJongseongOrChoseong = 6, CJOC = 6,         // 종성 또는 초성
            ConsonantMergedSoloOrChoseong = 7, CMSOC = 7,       // 결합 단독 자음 (앞의 자음과 결합) 또는 초성
            ConsonantMergedJongseongOrChoseong = 8, CMJOC = 8   // 결합 종성 (앞의 자음과 결합) 또는 초성
        };

        /// <summary>
        /// 한글 재결합 시 쓰이는, 확정된 초성/중성/종성 상태
        /// </summary>
        enum ChoJungJong
        {
            Unknown,                // 오류 (나오면 안 되는 상태)
            Choseong,               // 초성
            Jungseong,              // 중성
            MergedJungseong,        // 결합 중성
            Jongseong,              // 종성
            MergedJongseong,        // 결합 종성
            ConsonantSolo,          // 단독 자음
            ConsonantMergedSolo,    // 결합 단독 자음
            VowelSolo,              // 단독 모음
            VowelMergedSolo         // 결합 단독 모음
        }

        static string AssembleOnlyHangul(string s)
        {
            // 입력으로는 단자음과 단모음만으로 구성된 문자열이 들어온다.
            ChoJungJongState state = ChoJungJongState.None;
            char[] c = s.ToCharArray();
            //bool[] isFirst = new bool[c.Length];    // 조합된 각 글자의 첫 번째 자음 또는 모음(단독 모음의 경우)의 위치에 true
            ChoJungJong[] whatType = new ChoJungJong[c.Length];

            for (int i = 0; i < c.Length; i++)
            {
                //isFirst[i] = false;
                whatType[i] = ChoJungJong.Unknown;
            }

            #region Analysis phase

            for (int i = 0; i < c.Length; i++)
            {
                int j = c[i];
                char last;
                if (IsVowel(j))
                {
                    // 모음이 새로 들어옴
                    switch (state)
                    {
                        case ChoJungJongState.None:
                            // 앞에 아무 것도 없어서 단독 모음이 됨
                            state = ChoJungJongState.VS;
                            //isFirst[i] = true;
                            whatType[i] = ChoJungJong.VowelSolo;
                            break;
                        case ChoJungJongState.CSOC:
                        case ChoJungJongState.CJOC:
                            // 앞에 자음이 있어서 중성이 됨
                            state = ChoJungJongState.VJ;
                            //isFirst[i] = false;
                            whatType[i] = ChoJungJong.Jungseong;
                            //isFirst[i - 1] = true;  // 앞의 자음은 초성이 됨
                            whatType[i - 1] = ChoJungJong.Choseong;
                            break;
                        case ChoJungJongState.CMSOC:
                            // 앞에 자음이 있어서 중성이 됨
                            state = ChoJungJongState.VJ;
                            //isFirst[i] = false;
                            whatType[i] = ChoJungJong.Jungseong;
                            //isFirst[i - 1] = true;  // 앞의 자음은 초성이 됨
                            whatType[i - 1] = ChoJungJong.Choseong;
                            whatType[i - 2] = ChoJungJong.ConsonantSolo;
                            break;
                        case ChoJungJongState.CMJOC:
                            // 앞에 자음이 있어서 중성이 됨
                            state = ChoJungJongState.VJ;
                            //isFirst[i] = false;
                            whatType[i] = ChoJungJong.Jungseong;
                            //isFirst[i - 1] = true;  // 앞의 자음은 초성이 됨
                            whatType[i - 1] = ChoJungJong.Choseong;
                            whatType[i - 2] = ChoJungJong.Jongseong;
                            break;
                        case ChoJungJongState.VJ:
                            // 앞에 중성인 모음이 있는 경우
                            last = c[i - 1];
                            if (CanMerge("" + last + c[i]))
                            {
                                // 앞의 모음과 결합할 수 있는 경우 중성으로 합쳐짐
                                state = ChoJungJongState.VMJ;
                                //isFirst[i] = false;
                                whatType[i] = ChoJungJong.MergedJungseong;
                            }
                            else
                            {
                                // 앞의 모음과 결합할 수 없는 경우 단독 모음이 됨
                                state = ChoJungJongState.VS;
                                //isFirst[i] = true;
                                whatType[i] = ChoJungJong.VowelSolo;
                            }
                            break;
                        case ChoJungJongState.VS:
                            // 앞에 단독 모음이 있는 경우
                            last = c[i - 1];
                            if (CanMerge("" + last + c[i]))
                            {
                                // 앞의 모음과 결합할 수 있는 경우 단독 모음으로 합쳐짐
                                state = ChoJungJongState.VMS;
                                //isFirst[i] = false;
                                whatType[i] = ChoJungJong.VowelMergedSolo;
                            }
                            else
                            {
                                // 앞의 모음과 결합할 수 없는 경우 단독 모음이 됨
                                state = ChoJungJongState.VS;
                                //isFirst[i] = true;
                                whatType[i] = ChoJungJong.VowelSolo;
                            }
                            break;
                        case ChoJungJongState.VMJ:
                        case ChoJungJongState.VMS:
                            // 앞에 겹모음이 있는 경우 단독 모음이 됨
                            state = ChoJungJongState.VS;
                            //isFirst[i] = true;
                            whatType[i] = ChoJungJong.VowelSolo;
                            break;
                    }
                }
                else if (IsConsonant(j))
                {
                    switch (state)
                    {
                        case ChoJungJongState.None:
                            // 앞에 아무 것도 없어서 초성 또는 단독 자음이 될 수 있음
                            state = ChoJungJongState.CSOC;
                            //isFirst[i] = true;
                            break;
                        case ChoJungJongState.CJOC:
                            // 앞에 종성 또는 초성이 될 수 있는 자음이 있는 경우
                            //isFirst[i - 1] = false; // 앞의 자음은 종성이 됨
                            whatType[i - 1] = ChoJungJong.Jongseong;
                            last = c[i - 1];
                            if (CanMerge("" + last + c[i]))
                            {
                                // 앞의 자음과 결합할 수 있는 경우 종성으로 합쳐지거나 초성이 될 수 있음
                                state = ChoJungJongState.CMJOC;
                            }
                            else
                            {
                                // 앞의 자음과 결합할 수 없는 경우 초성 또는 단독 자음이 될 수 있음
                                state = ChoJungJongState.CSOC;
                            }
                            break;
                        case ChoJungJongState.CSOC:
                            // 앞에 단독 자음 또는 초성이 될 수 있는 자음이 있는 경우
                            last = c[i - 1];
                            if (CanMerge("" + last + c[i]))
                            {
                                // 앞의 자음과 결합할 수 있는 경우 단독 자음으로 합쳐지거나 초성이 될 수 있음
                                state = ChoJungJongState.CMSOC;
                            }
                            else
                            {
                                // 앞의 자음과 결합할 수 없는 경우 초성 또는 단독 자음이 될 수 있음
                                state = ChoJungJongState.CSOC;
                                //isFirst[i] = true;
                                whatType[i - 1] = ChoJungJong.ConsonantSolo;
                            }
                            break;
                        case ChoJungJongState.CMJOC:
                            // 앞에 겹자음 종성이 있는 경우 초성 또는 단독 자음이 될 수 있음
                            state = ChoJungJongState.CSOC;
                            //isFirst[i] = true;
                            //isFirst[i - 1] = false;  // 앞의 자음은 그 앞의 자음과 결합하며 초성이 아니게 됨
                            whatType[i - 1] = ChoJungJong.MergedJongseong;
                            whatType[i - 2] = ChoJungJong.Jongseong;
                            break;
                        case ChoJungJongState.CMSOC:
                            // 앞에 단독 겹자음이 있는 경우 초성 또는 단독 자음이 될 수 있음
                            state = ChoJungJongState.CSOC;
                            //isFirst[i] = true;
                            //isFirst[i - 1] = false;  // 앞의 자음은 그 앞의 자음과 결합하며 초성이 아니게 됨
                            whatType[i - 1] = ChoJungJong.ConsonantMergedSolo;
                            whatType[i - 2] = ChoJungJong.ConsonantSolo;
                            break;
                        case ChoJungJongState.VJ:
                        case ChoJungJongState.VMJ:
                            // 앞에 중성인 모음이 있는 경우
                            if (j == 12600 || j == 12611 || j == 12617)
                            {
                                // 종성이 될 수 없는 'ㄸ', 'ㅃ', 'ㅉ'인 경우 초성 또는 단독 자음이 될 수 있음
                                state = ChoJungJongState.CSOC;
                                //isFirst[i] = true;
                            }
                            else
                            {
                                // 종성이 될 수 있으면 종성 또는 초성이 될 수 있음
                                state = ChoJungJongState.CJOC;
                            }
                            break;
                        case ChoJungJongState.VS:
                        case ChoJungJongState.VMS:
                            // 앞에 단독 모음이 있는 경우 초성 또는 단독 자음이 될 수 있음
                            state = ChoJungJongState.CSOC;
                            //isFirst[i] = true;
                            break;
                    }
                }
                else
                {
                    state = ChoJungJongState.None;
                    //isFirst[i] = true;
                    Console.WriteLine("Warning: AssembleOnlyHangul");
                }
            }

            // 마지막에 자음으로 끝나는 경우 isFirst를 정해주어야 한다.
            if (state == ChoJungJongState.CJOC)
            {
                //isFirst[c.Length - 1] = false;
                whatType[c.Length - 1] = ChoJungJong.Jongseong;
            }
            else if (state == ChoJungJongState.CSOC)
            {
                //isFirst[c.Length - 1] = true;
                whatType[c.Length - 1] = ChoJungJong.ConsonantSolo;
            }
            else if (state == ChoJungJongState.CMJOC)
            {
                //isFirst[c.Length - 1] = false;
                whatType[c.Length - 1] = ChoJungJong.MergedJongseong;
                whatType[c.Length - 2] = ChoJungJong.Jongseong;
            }
            else if (state == ChoJungJongState.CMSOC)
            {
                //isFirst[c.Length - 1] = false;
                whatType[c.Length - 1] = ChoJungJong.ConsonantMergedSolo;
                whatType[c.Length - 2] = ChoJungJong.ConsonantSolo;
            }

            #endregion

            #region Building phase

            string ret = "";
            bool isSolo = false;        // 단독 자음/모음을 만드는 중이면 true, 글자를 만드는 중이면 false
            int[] builder = new int[5]; // 결합할 자모들을 임시 저장하는 버퍼
                                        // builder의 인덱스 0은 초성, 1은 중성, 2는 결합 중성, 3은 종성, 4는 결합 종성
                                        // builder의 값은 12593(ㄱ) ~ 12643(ㅣ) 사이의 단자음 또는 단모음 char 코드

            for (int i = 0; i < c.Length; i++)
            {
                if (whatType[i] == ChoJungJong.Choseong)
                {
                    //Console.Write("; ");

                    // 이전 버퍼의 것 빌드
                    if (isSolo)
                    {
                        ret += Build(builder[0], builder[1]);
                    }
                    else
                    {
                        ret += Build(builder[0], builder[1], builder[2],
                            builder[3], builder[4]);
                    }
                    isSolo = false;
                    for (int j = 0; j < 5; j++) builder[j] = 0;
                }
                else if (whatType[i] == ChoJungJong.ConsonantSolo ||
                    whatType[i] == ChoJungJong.VowelSolo)
                {
                    //Console.Write("; ");

                    // 이전 버퍼의 것 빌드
                    if (isSolo)
                    {
                        ret += Build(builder[0], builder[1]);
                    }
                    else
                    {
                        ret += Build(builder[0], builder[1], builder[2],
                            builder[3], builder[4]);
                    }
                    isSolo = true;
                    for (int j = 0; j < 5; j++) builder[j] = 0;
                }
                //Console.Write(c[i]);
                switch (whatType[i])
                {
                    case ChoJungJong.ConsonantSolo:
                        //Console.Write("(단자) ");
                        builder[0] = c[i];
                        break;
                    case ChoJungJong.ConsonantMergedSolo:
                        //Console.Write("(결합단자) ");
                        builder[1] = c[i];
                        break;
                    case ChoJungJong.VowelSolo:
                        //Console.Write("(단모) ");
                        builder[0] = c[i];
                        break;
                    case ChoJungJong.VowelMergedSolo:
                        //Console.Write("(결합단모) ");
                        builder[1] = c[i];
                        break;
                    case ChoJungJong.Choseong:
                        //Console.Write("(초성) ");
                        builder[0] = c[i];
                        break;
                    case ChoJungJong.Jungseong:
                        //Console.Write("(중성) ");
                        builder[1] = c[i];
                        break;
                    case ChoJungJong.MergedJungseong:
                        //Console.Write("(결합중성) ");
                        builder[2] = c[i];
                        break;
                    case ChoJungJong.Jongseong:
                        //Console.Write("(종성) ");
                        builder[3] = c[i];
                        break;
                    case ChoJungJong.MergedJongseong:
                        //Console.Write("(결합종성) ");
                        builder[4] = c[i];
                        break;
                    case ChoJungJong.Unknown:
                        Console.Write(c[i]);
                        Console.Write("(오류!) ");
                        break;
                }
            }

            // 마지막에 한 번 더 빌드하기
            if (isSolo)
            {
                ret += Build(builder[0], builder[1]);
            }
            else
            {
                ret += Build(builder[0], builder[1], builder[2],
                    builder[3], builder[4]);
            }

            //Console.WriteLine();

            #endregion

            return ret;
        }
    }
}
