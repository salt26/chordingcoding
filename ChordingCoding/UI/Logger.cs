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
using System.Text;
using System.IO;

namespace ChordingCoding.UI.Logging
{
    public class Logger
    {
        public enum LogType { Process = 0, Key = 1, Mouse = 2, UI = 3, Music = 4 }

        private const string contextSavePath = "WorkingContext.csv";
        private static long prevTicks = DateTime.Now.Ticks;

        private const string sentimentSavePath = "SentimentLog.csv";
        private static long prevTicks2 = DateTime.Now.Ticks;

        private const string sentimentSavePath2 = "SentimentWordLog.csv";

        public static void AppendContextLog(LogType type, params object[] messages)
        {
#pragma warning disable CS0162 // 접근할 수 없는 코드가 있습니다.
            if (!MainForm.ENABLE_CONTEXT_LOGGING) return;

            long deltaTicks = DateTime.Now.Ticks - prevTicks;
            prevTicks = DateTime.Now.Ticks;

            string s = type.ToString() + "," + DateTime.Now.Year + "," + DateTime.Now.Month +
                "," + DateTime.Now.Day + "," + DateTime.Now.Hour + "," + DateTime.Now.Minute +
                "," + DateTime.Now.Second + "," + DateTime.Now.Millisecond + "," + (deltaTicks / 10000000f);
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
            try
            {
                if (!File.Exists(contextSavePath))
                {
                    File.AppendAllText(contextSavePath, "id,year,month,day,hour,minute,second,ms,delta,e,w1,w2,w3\n", Encoding.UTF8);
                }
                File.AppendAllText(contextSavePath, s + "\n", Encoding.UTF8);
            }
            catch (Exception e)
            {
                if (!(e is IOException))
                    throw;
            }
#pragma warning restore CS0162 // 접근할 수 없는 코드가 있습니다.
        }

        public static void AppendSentimentLog(params object[] messages)
        {
#pragma warning disable CS0162 // 접근할 수 없는 코드가 있습니다.
            if (!MainForm.ENABLE_CONTEXT_LOGGING) return;

            long deltaTicks = DateTime.Now.Ticks - prevTicks2;
            prevTicks2 = DateTime.Now.Ticks;

            string s = DateTime.Now.Year + "," + DateTime.Now.Month +
                "," + DateTime.Now.Day + "," + DateTime.Now.Hour + "," + DateTime.Now.Minute +
                "," + DateTime.Now.Second + "," + DateTime.Now.Millisecond + "," + (deltaTicks / 10000000f);
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
            try
            {
                if (!File.Exists(sentimentSavePath))
                {
                    File.AppendAllText(sentimentSavePath, "year,month,day,hour,minute,second,ms,delta,word,valence,arousal,shortValence,shortArousal,longValence,longArousal,prevValence,prevArousal\n", Encoding.UTF8);
                }
                File.AppendAllText(sentimentSavePath, s + "\n", Encoding.UTF8);
            }
            catch (Exception e)
            {
                if (!(e is IOException))
                    throw;
            }
#pragma warning restore CS0162 // 접근할 수 없는 코드가 있습니다.
        }

        public static void AppendSentimentLog2(params object[] messages)
        {
#pragma warning disable CS0162 // 접근할 수 없는 코드가 있습니다.
            if (!MainForm.ENABLE_CONTEXT_LOGGING) return;

            string s = " ";
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
                if (!(e is IOException))
                    throw;
            }
#pragma warning restore CS0162 // 접근할 수 없는 코드가 있습니다.
        }
    }
}
