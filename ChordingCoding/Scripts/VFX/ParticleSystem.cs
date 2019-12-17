using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using ChordingCoding.UI;

namespace ChordingCoding.VFX
{
    public class ParticleSystem
    {
        public enum CreateFunction { Gaussian, DiracDelta, TopRandom, Random, BottomRandom }
        public delegate Color ParticleColor();
        public delegate float StartPosition();

        public StartPosition startPositionX;
        public StartPosition startPositionY;
        public float positionX;
        public float positionY;
        public float velocityX;
        public float velocityY;
        public int lifetime;                                            // 이 파티클 시스템의 남은 수명
        public int createNumber;
        public float createRange;
        public CreateFunction createFunction = CreateFunction.Gaussian; // 생성하는 파티클의 초기 위치 분포 함수
        public Particle.Type particleType = Particle.Type.dot;
        public ParticleColor particleColor;
        public float particleSize;
        public int particleLifetime;                                    // 각 파티클의 초기 수명
        public bool isBasicParticleSystem = false;

        public List<Particle> particles = new List<Particle>();         // 생성한 파티클 목록

        private Random random = new Random();

        /// <summary>
        /// 새 파티클 시스템을 생성합니다.
        /// </summary>
        public ParticleSystem(StartPosition startPosX, StartPosition startPosY, float velocityX, float velocityY, int lifetime, 
            int createNumber, float createRange, CreateFunction createFunction,
            Particle.Type particleType, ParticleColor particleColor, float particleSize, int particleLifetime)
        {
            //Random r = new Random();
            this.startPositionX = startPosX;
            this.startPositionY = startPosY;
            this.positionX = startPosX();//(float)r.NextDouble() * (maxPosX - minPosX) + minPosX;
            this.positionY = startPosY();
            this.velocityX = velocityX;
            this.velocityY = velocityY;
            this.lifetime = lifetime;

            this.createNumber = createNumber;
            this.createRange = createRange;
            this.createFunction = createFunction;

            this.particleType = particleType;
            this.particleColor = particleColor;
            this.particleSize = particleSize;
            this.particleLifetime = particleLifetime;

            this.isBasicParticleSystem = false;
        }

        /// <summary>
        /// 테마에 맞는 기본 파티클 시스템을 생성합니다.
        /// 기본 파티클 시스템은 테마가 바뀔 때까지 사라지지 않고, 키보드 입력에 따라 파티클을 생성합니다.
        /// </summary>
        public ParticleSystem(
            int createNumber, float createRange, CreateFunction createFunction,
            Particle.Type particleType, ParticleColor particleColor, float particleSize, int particleLifetime)
        {
            //Random r = new Random();
            this.startPositionX = () => 0;
            this.startPositionY = () => 0;
            this.positionX = 0;
            this.positionY = 0;
            this.velocityX = 0;
            this.velocityY = 0;
            this.lifetime = -1;

            this.createNumber = createNumber;
            this.createRange = createRange;
            this.createFunction = createFunction;

            this.particleType = particleType;
            this.particleColor = particleColor;
            this.particleSize = particleSize;
            this.particleLifetime = particleLifetime;

            this.isBasicParticleSystem = true;
        }

        /// <summary>
        /// 매 프레임마다 호출될 함수입니다.
        /// 수명이 다한 파티클을 처리하고 파티클들을 업데이트하며 새 파티클을 생성합니다.
        /// 한 파티클 시스템 당 한 번에 최대 200개까지의 파티클만 활성화할 수 있습니다.
        /// </summary>
        public void Update()
        {
            List<Particle> deadParticles = new List<Particle>();
            foreach (Particle p in particles)
            {
                if (p.lifetime <= 0)
                {
                    // 수명이 다한 파티클 처리
                    deadParticles.Add(p);
                    continue;
                }
                else
                {
                    // 수명이 남아 있는 파티클 업데이트
                    p.Update();
                }
            }
            foreach (Particle dead in deadParticles)
            {
                // 수명이 다한 파티클의 레퍼런스를 파티클 목록에서 삭제
                particles.Remove(dead);
            }

            if (lifetime > 0 && !isBasicParticleSystem)
            {
                // 새 파티클 생성
                for (int i = 0; i < createNumber; i++)
                {
                    // 한 파티클 시스템 당 최대 파티클 개수 200개로 제한
                    if (particles.Count >= 200)
                    {
                        particles.RemoveAt(0);
                    }
                    ParticlePosition(out float posX, out float posY);
                    particles.Add(new Particle(particleType, posX, posY, particleLifetime, particleColor(), particleSize));
                }

                // 위치 이동
                positionX += velocityX;
                positionY += velocityY;

                // 파티클 시스템의 수명 감소
                lifetime--;
            }
            else if (isBasicParticleSystem && particleType == Particle.Type.rain)
            {
                for (int i = 0; i < createNumber; i++)
                {
                    // 한 파티클 시스템 당 최대 파티클 개수 200개로 제한
                    if (particles.Count >= 200)
                    {
                        particles.RemoveAt(0);
                    }
                    ParticlePosition(out float posX, out float posY);
                    particles.Add(new Particle(particleType, posX, posY, particleLifetime, particleColor(), particleSize));
                }
            }
        }

        /// <summary>
        /// 화면에 이 파티클 시스템의 파티클들을 그릴 때 호출됩니다.
        /// </summary>
        public void Draw(Graphics g)
        {
            try
            {
                foreach (Particle p in particles)
                {
                    p.Draw(g);
                }
            }
            catch (InvalidOperationException)
            {

            }
        }
        
        /// <summary>
        /// 이 파티클 시스템에 대한 레퍼런스를 삭제해도 되는지 알려줍니다.
        /// </summary>
        /// <returns></returns>
        public bool CanDestroy()
        {
            return lifetime <= 0 && particles.Count == 0 && !isBasicParticleSystem;
        }

        /// <summary>
        /// 기본 파티클 시스템에 새 파티클을 createNumber만큼 추가합니다.
        /// </summary>
        public void AddParticleInBasic(Particle.Type particleType, int particleLifetime, Color particleColor, float particleSize)
        {
            if (!isBasicParticleSystem) return;
            //Console.WriteLine(particleType.ToString() + " " + particleLifetime);

            // 새 파티클 생성
            for (int i = 0; i < createNumber; i++)
            {
                // 한 파티클 시스템 당 최대 파티클 개수 200개로 제한
                if (particles.Count >= 200)
                {
                    particles.RemoveAt(0);
                }
                ParticlePosition(out float posX, out float posY);
                particles.Add(new Particle(particleType, posX, posY, particleLifetime, particleColor, particleSize));
            }
        }

        /// <summary>
        /// 새로 생성될 파티클의 위치를 결정해 주는 함수입니다.
        /// createFunction으로 계산됩니다.
        /// posX, posY로 결과가 반환됩니다.
        /// </summary>
        /// <param name="posX">새 파티클의 X좌표</param>
        /// <param name="posY">새 파티클의 Y좌표</param>
        private void ParticlePosition(out float posX, out float posY)
        {
            posX = positionX;
            posY = positionY;

            switch (createFunction)
            {
                case CreateFunction.Gaussian:
                    float r1 = Util.GaussianRandom(random);
                    float r2 = Util.GaussianRandom(random);
                    posX = positionX + createRange * r1;
                    posY = positionY + createRange * r2;
                    break;
                case CreateFunction.DiracDelta:
                    posX = positionX;
                    posY = positionY;
                    break;
                case CreateFunction.TopRandom:
                    posX = (float)(random.NextDouble() * Form1.form1.Size.Width);
                    posY = -50f - 25f * (float)random.NextDouble();
                    break;
                case CreateFunction.Random:
                    posX = (float)(random.NextDouble() * Form1.form1.Size.Width);
                    posY = (float)(random.NextDouble() * Form1.form1.Size.Height);
                    break;
                case CreateFunction.BottomRandom:
                    posX = (float)(random.NextDouble() * Form1.form1.Size.Width);
                    posY = Form1.form1.Size.Height + 50f + 25f * (float)random.NextDouble();
                    break;
            }
        }

        
    }
}
