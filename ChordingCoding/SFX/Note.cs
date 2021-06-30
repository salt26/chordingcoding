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
using System.Collections.Generic;

namespace ChordingCoding.SFX
{
    /// <summary>
    /// 음표 클래스입니다.
    /// </summary>
    public class Note
    {
        /// <summary>
        /// 음 높이(1 ~ 127)를 반환하는 함수 대리자입니다.
        /// </summary>
        /// <returns></returns>
        public delegate int PitchGenerator();
        
        /// <summary>
        /// 음 높이(1 ~ 127)를 반환하는 함수.
        /// 예) 60: C4 / 64: E4 / 67: G4 / 72: C5
        /// </summary>
        private PitchGenerator pitch;

        /// <summary>
        /// 음 높이(1 ~ 127). 고정된 값이 아니므로 읽을 때마다 값이 달라질 수도 있습니다.
        /// 예) 60: C4 / 64: E4 / 67: G4 / 72: C5
        /// </summary>
        public int Pitch
        {
            get { return pitch(); }
        }

        private int rhythm;

        /// <summary>
        /// 음표의 길이(1 이상). 4/4박에서 한 마디를 64등분한 길이를 기준으로 합니다.
        /// 예) 64: 온음표 / 16: 4분음표 / 4: 16분음표 / 1: 64분음표
        /// </summary>
        public int Rhythm
        {
            get { return rhythm; }
        }

        private long measure;

        /// <summary>
        /// 음표가 위치한 마디 번호(0부터 시작).
        /// </summary>
        public long Measure
        {
            get { return measure; }
        }

        private int position;

        /// <summary>
        /// 음표의 마디 내 위치(0 ~ 63). 4/4박에서 한 마디를 64등분한 길이를 기준으로 합니다.
        /// </summary>
        public int Position
        {
            get { return position; }
        }

        private int staff;

        /// <summary>
        /// 음표가 놓일 Staff 번호(0 ~ 15). 9번 Staff는 타악기 전용 Staff입니다.
        /// </summary>
        public int Staff
        {
            get { return staff; }
        }

        private int velocity;

        /// <summary>
        /// 음 세기(1 ~ 127). 재생 시의 실제 세기와는 다를 수 있습니다.
        /// </summary>
        public int Velocity
        {
            get { return velocity; }
        }

        /// <summary>
        /// 음표를 생성합니다.
        /// </summary>
        /// <param name="pitch">음 높이(1 ~ 127). 예) 60: C4 / 64: E4 / 67: G4 / 72: C5</param>
        /// <param name="velocity">음 세기(1 ~ 127).</param>
        /// <param name="rhythm">음표의 길이(1 이상). 4/4박에서 한 마디를 64등분한 길이를 기준으로 합니다. 예) 64: 온음표 / 16: 4분음표 / 4: 16분음표 / 1: 64분음표</param>
        /// <param name="measure">음표가 위치한 마디 번호(0부터 시작).</param>
        /// <param name="position">음표의 마디 내 위치(0 ~ 63). 4/4박에서 한 마디를 64등분한 길이를 기준으로 합니다.</param>
        /// <param name="staff">음표가 놓일 Staff 번호(0 ~ 15). 9번 Staff는 타악기 전용 Staff입니다.</param>
        public Note(int pitch, int velocity, int rhythm, long measure, int position, int staff = 0)
        {
            if (pitch < 1 || pitch > 127) pitch = 60;
            this.pitch = () => pitch;

            if (velocity < 1 || velocity > 127) velocity = 127;
            this.velocity = velocity;

            if (rhythm < 1) rhythm = 16;
            this.rhythm = rhythm;

            if (position < 0 || position > 63) measure += position / 64;
            if (measure < 0) measure = 0;
            this.measure = measure;

            if (position < 0 || position > 63) position %= 64;
            this.position = position;

            if (staff < 0 || staff > 15) staff = 0;
            this.staff = staff;
        }

        /// <summary>
        /// 음표를 생성합니다.
        /// </summary>
        /// <param name="pitch">음 높이(1 ~ 127)를 반환하는 함수. 예) () => 60: C4 / () => 64: E4 / () = > 67: G4 / () => 72: C5</param>
        /// <param name="velocity">음 세기(1 ~ 127).</param>
        /// <param name="rhythm">음표의 길이(1 이상). 4/4박에서 한 마디를 64등분한 길이를 기준으로 합니다. 예) 64: 온음표 / 16: 4분음표 / 4: 16분음표 / 1: 64분음표</param>
        /// <param name="measure">음표가 위치한 마디 번호(0부터 시작).</param>
        /// <param name="position">음표의 마디 내 위치(0 ~ 63). 4/4박에서 한 마디를 64등분한 길이를 기준으로 합니다.</param>
        /// <param name="staff">음표가 놓일 Staff 번호(0 ~ 15). 9번 Staff는 타악기 전용 Staff입니다.</param>
        public Note(PitchGenerator pitch, int velocity, int rhythm, long measure, int position, int staff = 0)
        {
            this.pitch = pitch;

            if (velocity < 1 || velocity > 127) velocity = 127;
            this.velocity = velocity;

            if (rhythm < 1) rhythm = 16;
            this.rhythm = rhythm;

            if (position < 0 || position > 63) measure += position / 64;
            if (measure < 0) measure = 0;
            this.measure = measure;

            if (position < 0 || position > 63) position %= 64;
            this.position = position;

            if (staff < 0 || staff > 15) staff = 0;
            this.staff = staff;
        }

        /// <summary>
        /// 이 음표를 연주하기 위해 Midi message pair 리스트로 변환합니다.
        /// (이 Pair들은 재생하거나 저장할 때 Message로 번역됩니다.)
        /// </summary>
        /// <returns></returns>
        public List<KeyValuePair<float, int>> ToMidi()
        {
            int pitch = this.pitch();
            if (pitch < 1 || pitch > 127) pitch = 60;

            // KeyValuePair의 float 값은 타이밍, int 값은 음 높이와 Staff 번호에 해당합니다.
            List<KeyValuePair<float, int>> res = new List<KeyValuePair<float, int>>
            {
                // Note on message pair 생성(Value가 양수)
                new KeyValuePair<float, int>(measure * 64f + position, pitch | staff << 16),

                // Note off message pair 생성(Value가 음수)
                new KeyValuePair<float, int>(measure * 64f + (position + rhythm * 6f / 7f), -(pitch | staff << 16))
            };
            return res;
        }
    }
}
