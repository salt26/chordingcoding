using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace ChordingCoding
{
    public class Particle
    {
        public enum Type { dot, rain, note, star, leaf }
        public Type type;
        public Image image;
        public float positionX;
        public float positionY;
        public float velocityX;
        public float velocityY;
        public int initialLifetime;
        public int lifetime;
        public Color initialColor;
        public float size;
        public bool isBlackBase;

        private ImageAttributes imageAtt;

        public Particle(Type type, float startPosX, float startPosY, int lifetime, Color color, float size = 1)
        {
            this.size = size;
            this.type = type;

            switch (type)
            {
                case Type.dot:
                    image = Properties.Resources.Dot;
                    isBlackBase = false;
                    velocityX = 0;
                    velocityY = 0;
                    break;
                case Type.rain:
                    image = Properties.Resources.Rain_;
                    isBlackBase = false;
                    velocityX = 0;
                    velocityY = 30;
                    break;
                case Type.note:
                    image = Properties.Resources.Note8_;
                    isBlackBase = false;
                    velocityX = 0;
                    velocityY = 15;
                    break;
                case Type.star:
                    image = Properties.Resources.Star;
                    isBlackBase = true;
                    velocityX = 0;
                    velocityY = 0;
                    break;
                case Type.leaf:
                    Random r = new Random();
                    float f = (float)r.NextDouble();
                    isBlackBase = false;
                    velocityX = 0;
                    velocityY = 4 + 4 * f;
                    this.size = size * (1 + f);
                    switch (r.Next(18))
                    {
                        case 0:
                            image = Properties.Resources.Leaf11;
                            break;
                        case 1:
                            image = Properties.Resources.Leaf12;
                            break;
                        case 2:
                            image = Properties.Resources.Leaf13;
                            break;
                        case 3:
                            image = Properties.Resources.Leaf14;
                            break;
                        case 4:
                            image = Properties.Resources.Leaf15;
                            break;
                        case 5:
                            image = Properties.Resources.Leaf16;
                            break;
                        case 6:
                            image = Properties.Resources.Leaf21;
                            break;
                        case 7:
                            image = Properties.Resources.Leaf22;
                            break;
                        case 8:
                            image = Properties.Resources.Leaf23;
                            break;
                        case 9:
                            image = Properties.Resources.Leaf24;
                            break;
                        case 10:
                            image = Properties.Resources.Leaf25;
                            break;
                        case 11:
                            image = Properties.Resources.Leaf26;
                            break;
                        case 12:
                            image = Properties.Resources.Leaf31;
                            break;
                        case 13:
                            image = Properties.Resources.Leaf32;
                            break;
                        case 14:
                            image = Properties.Resources.Leaf33;
                            break;
                        case 15:
                            image = Properties.Resources.Leaf34;
                            break;
                        case 16:
                            image = Properties.Resources.Leaf35;
                            break;
                        case 17:
                            image = Properties.Resources.Leaf36;
                            break;
                    }
                    break;
            }

            this.positionX = startPosX;//(float)r.NextDouble() * (maxPosX - minPosX) + minPosX;
            this.positionY = startPosY;

            this.initialLifetime = lifetime;
            this.lifetime = lifetime;
            
            float alpha = color.A / 255f;
            float red = color.R / 255f;
            float green = color.G / 255f;
            float blue = color.B / 255f;

            initialColor = color;

            UpdateImageAtt(red, green, blue, alpha, isBlackBase);
        }

        /// <summary>
        /// 매 프레임마다 호출될 함수입니다.
        /// </summary>
        public void Update()
        {
            if (lifetime > 0)
            {
                // 위치 이동
                positionX += velocityX;
                positionY += velocityY;

                // 파티클의 수명 감소
                lifetime--;
            }
        }

        /// <summary>
        /// 화면에 이 파티클을 그리기 위해 호출되는 함수입니다.
        /// </summary>
        public void Draw(Graphics g)
        {
            // TODO
            // Additive Blending
            // https://stackoverflow.com/questions/12170894/drawing-image-with-additive-blending
            // https://stackoverflow.com/questions/726549/algorithm-for-additive-color-mixing-for-rgb-values

            if (lifetime > 0)
            {
                RectangleF rf = new RectangleF(positionX, positionY, image.Width * size, image.Height * size);
                if (type == Type.dot || type == Type.star)
                {
                    UpdateImageAtt(initialColor.R / 255f, initialColor.G / 255f, initialColor.B / 255f,
                        initialColor.A / 255f * lifetime * lifetime / initialLifetime / initialLifetime, isBlackBase);
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

        private void UpdateImageAtt(float red, float green, float blue, float alpha, bool isAdditive = false)
        {
            // Initialize the color matrix.
            // Note the value 0.8 in row 4, column 4.
            float[][] matrixItems;
            if (!isAdditive)
            {
                matrixItems = new float[][]{
                    new float[] {red, 0, 0, 0, 0},
                    new float[] {0, green, 0, 0, 0},
                    new float[] {0, 0, blue, 0, 0},
                    new float[] {0, 0, 0, alpha, 0},
                    new float[] {0, 0, 0, 0, 1}};       // add color value {r, g, b, a, 1}
            }
            else
            {
                matrixItems = new float[][]{
                    new float[] {1, 0, 0, 0, 0},
                    new float[] {0, 1, 0, 0, 0},
                    new float[] {0, 0, 1, 0, 0},
                    new float[] {0, 0, 0, alpha, 0},
                    new float[] {red, green, blue, 0, 1}};       // add color value {r, g, b, a, 1}
            }
            ColorMatrix colorMatrix = new ColorMatrix(matrixItems);

            // Create an ImageAttributes object and set its color matrix.
            imageAtt = new ImageAttributes();
            imageAtt.SetColorMatrix(
               colorMatrix,
               ColorMatrixFlag.Default,
               ColorAdjustType.Bitmap);
        }

        /*
         * semi-transparent image using GDI+
         * https://www.codeproject.com/Articles/29184/A-lovely-goldfish-desktop-pet-using-alpha-PNG-and
         * https://stackoverflow.com/questions/19591583/c-sharp-transparent-form-using-updatelayeredwindow-draw-controls
         * https://docs.microsoft.com/en-us/previous-versions/ms997507(v=msdn.10)#layerwin_topic2b
         * 
        public void SetBits(Bitmap bitmap)
        {
            if (!haveHandle) return;
            if (!Bitmap.IsCanonicalPixelFormat(bitmap.PixelFormat) ||
                !Bitmap.IsAlphaPixelFormat(bitmap.PixelFormat))
                throw new ApplicationException("The picture must be " +
                          "32bit picture with alpha channel");
            IntPtr oldBits = IntPtr.Zero;
            IntPtr screenDC = Win32.GetDC(IntPtr.Zero);
            IntPtr hBitmap = IntPtr.Zero;
            IntPtr memDc = Win32.CreateCompatibleDC(screenDC);
            try
            {
                Win32.Point topLoc = new Win32.Point(Left, Top);
                Win32.Size bitMapSize = new Win32.Size(bitmap.Width, bitmap.Height);
                Win32.BLENDFUNCTION blendFunc = new Win32.BLENDFUNCTION();
                Win32.Point srcLoc = new Win32.Point(0, 0);
                hBitmap = bitmap.GetHbitmap(Color.FromArgb(0));
                oldBits = Win32.SelectObject(memDc, hBitmap);
                blendFunc.BlendOp = Win32.AC_SRC_OVER;
                blendFunc.SourceConstantAlpha = 255;
                blendFunc.AlphaFormat = Win32.AC_SRC_ALPHA;
                blendFunc.BlendFlags = 0;
                Win32.UpdateLayeredWindow(Handle, screenDC, ref topLoc, ref bitMapSize,
                                 memDc, ref srcLoc, 0, ref blendFunc, Win32.ULW_ALPHA);
            }
            finally
            {
                if (hBitmap != IntPtr.Zero)
                {
                    Win32.SelectObject(memDc, oldBits);
                    Win32.DeleteObject(hBitmap);
                }
                Win32.ReleaseDC(IntPtr.Zero, screenDC);
                Win32.DeleteDC(memDc);
            }
        }
        */

    }
}
