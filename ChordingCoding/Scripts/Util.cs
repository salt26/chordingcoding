using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChordingCoding
{
    class Util
    {
        /// <summary>
        /// 평균이 0, 표준편차가 1인 표준정규분포를 따르는 랜덤한 값을 생성합니다.
        /// </summary>
        /// <returns></returns>
        public static float GaussianRandom(Random r)
        {
            double u1 = 1.0 - r.NextDouble(); // uniform (0,1] random doubles
            double u2 = 1.0 - r.NextDouble();
            float randStdNormal = (float)(Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2)); // random normal(0,1)
            return randStdNormal;
        }
    }
}
