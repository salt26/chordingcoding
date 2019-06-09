using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

namespace ChordingCoding
{
    public class Particle
    {
        public enum Type { dot, rain }
        public Type type;
        public Image image;
        public float positionX;
        public float positionY;
        public int initialLifetime;
        public int lifetime;
        public Color initialColor;
        public float size;

        private ImageAttributes imageAtt;

        public Particle(Type type, float startPosX, float startPosY, int lifetime, Color color, float size = 1)
        {
            this.type = type;
            switch (type)
            {
                case Type.dot:
                    image = Properties.Resources.Dot;
                    break;
                case Type.rain:
                    image = Properties.Resources.Rain;
                    break;
            }

            //Random r = new Random();
            this.positionX = startPosX;//(float)r.NextDouble() * (maxPosX - minPosX) + minPosX;
            this.positionY = startPosY;

            this.initialLifetime = lifetime;
            this.lifetime = lifetime;
            this.size = size;
            
            float alpha = color.A / 255f;
            float red = color.R / 255f;
            float green = color.G / 255f;
            float blue = color.B / 255f;

            initialColor = color;

            UpdateImageAtt(red, green, blue, alpha);
        }

        /// <summary>
        /// 매 프레임마다 호출될 함수입니다.
        /// </summary>
        public void Update()
        {
            // TODO
            if (lifetime > 0)
            {
                lifetime--;
            }
        }

        /// <summary>
        /// 화면에 이 파티클을 그리기 위해 호출되는 함수입니다.
        /// </summary>
        public void Draw(Graphics g)
        {
            // TODO
            // Additive Alpha Blending
            // https://stackoverflow.com/questions/12170894/drawing-image-with-additive-blending

            if (lifetime > 0)
            {
                RectangleF rf = new RectangleF(positionX, positionY, image.Width * size, image.Height * size);
                if (type == Type.dot)
                {
                    UpdateImageAtt(initialColor.R / 255f, initialColor.G / 255f, initialColor.B / 255f,
                        initialColor.A / 255f * lifetime * lifetime / initialLifetime / initialLifetime);
                }
                g.DrawImage(image, GetPoints(rf), new RectangleF(0.0f, 0.0f, image.Width, image.Height), GraphicsUnit.Pixel, imageAtt);
            }
        }
        private PointF[] GetPoints(RectangleF rectangle)
        {
            return new PointF[3]
            {
            new PointF(rectangle.Left, rectangle.Top),
            new PointF(rectangle.Right, rectangle.Top),
            new PointF(rectangle.Left, rectangle.Bottom)
            };
        }

        private void UpdateImageAtt(float red, float green, float blue, float alpha)
        {
            // Initialize the color matrix.
            // Note the value 0.8 in row 4, column 4.
            float[][] matrixItems = {
                    new float[] {red, 0, 0, 0, 0},
                    new float[] {0, green, 0, 0, 0},
                    new float[] {0, 0, blue, 0, 0},
                    new float[] {0, 0, 0, alpha, 0},
                    new float[] {0, 0, 0, 0, 1}};       // add color value {r, g, b, a, 1}
            ColorMatrix colorMatrix = new ColorMatrix(matrixItems);

            // Create an ImageAttributes object and set its color matrix.
            imageAtt = new ImageAttributes();
            imageAtt.SetColorMatrix(
               colorMatrix,
               ColorMatrixFlag.Default,
               ColorAdjustType.Bitmap);
        }
    }
}
