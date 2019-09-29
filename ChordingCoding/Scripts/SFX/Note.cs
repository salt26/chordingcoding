using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChordingCoding.SFX
{
    /// <summary>
    /// 음표 클래스입니다.
    /// </summary>
    class Note
    {
        /// <summary>
        /// 음 높이(0 ~ 127).
        /// 예) 60: C4 / 64: E4 / 67: G4 / 72: C5
        /// </summary>
        private int pitch;

        /// <summary>
        /// 음표의 길이(1 이상). 4/4박에서 한 마디를 16등분한 길이를 기준으로 합니다.
        /// 예) 16: 온음표 / 4: 4분음표 / 1: 16분음표
        /// </summary>
        private int rhythm;

        /// <summary>
        /// 음표가 위치한 마디 번호(0부터 시작).
        /// </summary>
        private int measure;

        /// <summary>
        /// 음표의 마디 내 위치(0 ~ 15). 4/4박에서 한 마디를 16등분한 길이를 기준으로 합니다.
        /// </summary>
        private int position;

        /// <summary>
        /// 음표가 놓일 Staff 번호(0 ~ 15). 9번 Staff는 타악기 전용 Staff입니다.
        /// </summary>
        private int staff;

        /// <summary>
        /// 음표를 생성합니다.
        /// </summary>
        /// <param name="pitch">음 높이(1 ~ 127). 예) 60: C4 / 64: E4 / 67: G4 / 72: C5</param>
        /// <param name="rhythm">음표의 길이(1 이상). 4/4박에서 한 마디를 16등분한 길이를 기준으로 합니다. 예) 16: 온음표 / 4: 4분음표 / 1: 16분음표</param>
        /// <param name="measure">음표가 위치한 마디 번호(0부터 시작).</param>
        /// <param name="position">음표의 마디 내 위치(0 ~ 15). 4/4박에서 한 마디를 16등분한 길이를 기준으로 합니다.</param>
        /// <param name="staff">음표가 놓일 Staff 번호(0 ~ 15). 9번 Staff는 타악기 전용 Staff입니다.</param>
        public Note(int pitch, int rhythm, int measure, int position, int staff = 0)
        {
            if (pitch < 1 || pitch > 127) pitch = 60;
            this.pitch = pitch;

            if (rhythm < 1) rhythm = 4;
            this.rhythm = rhythm;

            if (position < 0 || position > 15) measure += position / 16;
            if (measure < 0) measure = 0;
            this.measure = measure;

            if (position < 0 || position > 15) position %= 16;
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
            // KeyValuePair의 float 값은 타이밍, int 값은 음 높이와 Staff 번호에 해당합니다.
            List<KeyValuePair<float, int>> res = new List<KeyValuePair<float, int>>
            {
                // Note on message pair 생성(Value가 양수)
                new KeyValuePair<float, int>(measure * 4f + position / 4f, pitch | staff << 16),

                // Note off message pair 생성(Value가 음수)
                new KeyValuePair<float, int>(measure * 4f + (position + rhythm * 6f / 7f) / 4f, -(pitch | staff << 16))
            };
            return res;
        }
    }
}
