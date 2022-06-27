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
using System;
using System.Text;
using System.IO;

namespace ChordingCoding.UI.Logging
{
    public class Logger
    {
        public enum ContextLogType { Process = 0, Key = 1, Mouse = 2, UI = 3, Music = 4, IME = 5 }

        private static string IMECache = "";

        public static void AppendContextLog(ContextLogType type, params object[] messages)
        {
#pragma warning disable CS0162 // 접근할 수 없는 코드가 있습니다.
            if (!MainForm.ENABLE_CONTEXT_LOGGING) return;

            if (!Directory.Exists("Logs"))
            {
                Directory.CreateDirectory("Logs");
            }

            if (type == ContextLogType.IME && messages.Length > 0)
            {
                if (messages[0].ToString().Equals(IMECache))
                    return;
                else
                    IMECache = messages[0].ToString();
            }

            string s = "," + type.ToString() + "," + DateTime.Now.Year + "," + DateTime.Now.Month +
                "," + DateTime.Now.Day + "," + DateTime.Now.Hour + "," + DateTime.Now.Minute +
                "," + DateTime.Now.Second + "," + DateTime.Now.Millisecond;
            foreach (object message in messages)
            {
                if (message is null)
                {
                    s += ",";
                }
                else
                {
                    s += "," + message.ToString();
                }
            }
            string contextSavePath = @"Logs\ChordingCoding_context_log_" + DateTime.Now.ToString("yyMMdd") + ".csv";
            try
            {
                if (!File.Exists(contextSavePath))
                {
                    File.AppendAllText(contextSavePath, "dummy,id,year,month,day,hour,minute,second,ms,e,w1,w2,w3\n", Encoding.UTF8);
                }
                File.AppendAllText(contextSavePath, s + "\n", Encoding.UTF8);
            }
            catch (Exception e)
            {
                if (!(e is IOException || e is UnauthorizedAccessException))
                    throw;
            }
#pragma warning restore CS0162 // 접근할 수 없는 코드가 있습니다.
        }

        public static void AppendSentimentLog(params object[] messages)
        {
#pragma warning disable CS0162 // 접근할 수 없는 코드가 있습니다.
            if (!MainForm.ENABLE_CONTEXT_LOGGING) return;

            if (!Directory.Exists("Logs"))
            {
                Directory.CreateDirectory("Logs");
            }

            string s = "," + DateTime.Now.Year + "," + DateTime.Now.Month +
                "," + DateTime.Now.Day + "," + DateTime.Now.Hour + "," + DateTime.Now.Minute +
                "," + DateTime.Now.Second + "," + DateTime.Now.Millisecond;
            foreach (object message in messages)
            {
                if (message is null)
                {
                    s += ",";
                }
                else
                {
                    s += "," + message.ToString();
                }
            }
            string sentimentSavePath = @"Logs\ChordingCoding_sentiment_log_" + DateTime.Now.ToString("yyMMdd") + ".csv";
            try
            {
                if (!File.Exists(sentimentSavePath))
                {
                    File.AppendAllText(sentimentSavePath, "dummy,year,month,day,hour,minute,second,ms,word,valence,arousal,shortValence,shortArousal,longValence,longArousal,prevValence,prevArousal\n", Encoding.UTF8);
                }
                File.AppendAllText(sentimentSavePath, s + "\n", Encoding.UTF8);
            }
            catch (Exception e)
            {
                if (!(e is IOException || e is UnauthorizedAccessException))
                    throw;
            }
#pragma warning restore CS0162 // 접근할 수 없는 코드가 있습니다.
        }

        public static void AppendSentimentLog2(params object[] messages)
        {
#pragma warning disable CS0162 // 접근할 수 없는 코드가 있습니다.
            if (!MainForm.ENABLE_CONTEXT_LOGGING) return;

            if (!Directory.Exists("Logs"))
            {
                Directory.CreateDirectory("Logs");
            }

            string s = "";
            foreach (object message in messages)
            {
                if (message is null)
                {
                    s += ",";
                }
                else
                {
                    s += "," + message.ToString();
                }
            }
            string sentimentSavePath2 = @"Logs\ChordingCoding_sentiment_word_log_" + DateTime.Now.ToString("yyMMdd") + ".csv";
            try
            {
                if (!File.Exists(sentimentSavePath2))
                {
                    File.AppendAllText(sentimentSavePath2, "dummy,index,word,valence,valenceValue,arousal,arousalValue,label\n", Encoding.UTF8);
                }
                File.AppendAllText(sentimentSavePath2, s + "\n", Encoding.UTF8);
            }
            catch (Exception e)
            {
                if (!(e is IOException || e is UnauthorizedAccessException))
                    throw;
            }
#pragma warning restore CS0162 // 접근할 수 없는 코드가 있습니다.
        }
    }
}
