#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2011 mjt[matola@sci.fi]
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing details.
 */
#endregion
/* 
 * fonttitiedoston pitää olla .PNG (ja tausta kannattaa olla läpinäkyvänä).
 * 
 * BitmapFont font=BitmapFont.Load("times14.png");
 * font.Write("Hello!");
 * 
 * apuna:
 * http://www.codersource.net/csharp_image_Processing.aspx
 * 
 * 
 * 
 Fonttikuvat on luotu IrrFontToolilla  
 (tulee <a href="http://irrlicht.sourceforge.net/">Irrlicht</a> 3d-enginen mukana)
 jolloin kirjainten ei tarvitse olla saman levyisiä.
 Löytyy myös (ehkä): https://archon2160.pbwiki.com/f/IrrFontTool.exe
 * 
 */

using System;
using System.Drawing;
using System.Drawing.Imaging;

using OpenTK.Graphics.OpenGL;

namespace CSatEng
{
    class Rect
    {
        public float x, y, w, h;

        public Rect(float x, float y, float w, float h)
        {
            this.x = x;
            this.y = y;
            this.w = w;
            this.h = -h;
        }
    }

    public class BitmapFont
    {
        static string chars = " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_'abcdefghijklmnopqrstuvwxyz{|}                                                                      Ä                 Ö             ä                 ö";
        Texture texture = new Texture();
        Rect[] uv = new Rect[chars.Length];
        float charHeight = 0;

        float curX = 0, curY = 0;
        public float Size = 500;

        public void Dispose()
        {
            texture.Dispose();
        }

        public static BitmapFont Load(string fileName)
        {
            BitmapFont font = new BitmapFont(); ;
            try
            {
                // lataa fonttitexture
                font.texture = Texture.Load(fileName);

                Bitmap CurrentBitmap = new Bitmap(Settings.TextureDir + fileName);

                if (CurrentBitmap.PixelFormat == System.Drawing.Imaging.PixelFormat.Format24bppRgb || CurrentBitmap.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppArgb)
                {
                    BitmapData Data = CurrentBitmap.LockBits(new Rectangle(0, 0, CurrentBitmap.Width, CurrentBitmap.Height), ImageLockMode.ReadOnly, CurrentBitmap.PixelFormat);
                    font.SearchUV(ref Data, (CurrentBitmap.PixelFormat == System.Drawing.Imaging.PixelFormat.Format24bppRgb) ? 3 : 4);
                    CurrentBitmap.UnlockBits(Data);
                }
                else Util.Error("Font: wrong pixel format.");
            }
            catch (Exception e)
            {
                Util.Error("Font: error loading file " + fileName + "\n" + e.ToString());
            }
            return font;
        }


        long GetColor(byte r, byte g, byte b)
        {
            return (long)((r << 16) + (g << 8) + b);
        }

        void SearchUV(ref BitmapData data, int bpp)
        {
            int width = 0, height = 0, x = 0, y = 0;
            int ch = 0;

            unsafe
            {
                // ota ylätarkistusväri
                byte* ptr = (byte*)(data.Scan0);
                long color1 = GetColor(*ptr, *(ptr + 1), *(ptr + 2));
                ptr += bpp;
                // ota alatarkistusväri
                long color2 = GetColor(*ptr, *(ptr + 1), *(ptr + 2));

                // etsi korkeus
                ptr = (byte*)(data.Scan0);
                for (int i = 0; i < data.Height; i++)
                {
                    ptr += data.Width * bpp;
                    height++;
                    long curcol = GetColor(*ptr, *(ptr + 1), *(ptr + 2));
                    if (curcol == color1) break;
                }
                this.charHeight = (float)height / (float)data.Height;

                // etsi kirjainten koot
                ptr = (byte*)(data.Scan0);
                while (true) // kunnes joka rivi käyty läpi
                {
                    while (true) // joka kirjain rivillä
                    {
                        long curcol = GetColor(*ptr, *(ptr + 1), *(ptr + 2));
                        if (curcol == color1) // ylänurkka
                        {
                            long b = 0;
                            // haetaan alanurkka
                            ptr += data.Width * bpp * (height - 1);
                            b += data.Width * bpp * (height - 1);
                            width = 0;
                            while (true)
                            {
                                curcol = GetColor(*ptr, *(ptr + 1), *(ptr + 2));
                                if (curcol == color2)
                                {
                                    // kirjaimen tiedot talteen
                                    uv[ch] = new Rect((float)x / (float)data.Width,
                                        (float)(y - 2.5f) / (float)data.Height,
                                        (float)width / (float)data.Width,
                                        (float)(height - 4f) / (float)data.Height);
                                    break;
                                }
                                ptr += bpp;
                                b += bpp;
                                width++;
                            }
                            ptr -= b;
                            ch++;
                            if (ch >= chars.Length) break;
                        }
                        x++;
                        if (x >= data.Width) break;
                        ptr += bpp;
                    }
                    x = 0;
                    y -= height;
                    if (y >= data.Height) break;
                    ptr = (byte*)(data.Scan0);
                    ptr += data.Width * bpp * (-y);

                    if (ch >= chars.Length)
                    {
                        break;
                    }
                }
            }
        }

        void DrawChar(int ch)
        {
            float u = uv[ch].x;
            float v = uv[ch].y;
            float w = uv[ch].w;
            float h = uv[ch].h;
            float wm = w * Size;
            float hm = h * Size;
            float xp = 0, yp = 0;
            GL.Begin(BeginMode.Quads);
            GL.TexCoord2(u, v);
            GL.Vertex2(xp, yp);
            GL.TexCoord2(u + w, v);
            GL.Vertex2(xp + wm, yp);
            GL.TexCoord2(u + w, v + h);
            GL.Vertex2(xp + wm, yp + hm);
            GL.TexCoord2(u, v + h);
            GL.Vertex2(xp, yp + hm);
            GL.End();
        }


        public void Write(string str)
        {
            Write(str, curX, curY);
        }
        public void Write(string str, float x, float y)
        {
            texture.Bind(0);
            GL.PushAttrib(AttribMask.AllAttribBits);
            GL.PushMatrix();
            curX = x;
            curY = y;
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.Lighting);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.Enable(EnableCap.AlphaTest);
            GL.AlphaFunc(AlphaFunction.Greater, 0.1f);
            GL.Translate(x, (float)Settings.Height - y, 0);
            float xp = 0;
            for (int q = 0, ch; q < str.Length; q++)
            {
                // etsi kirjain
                for (ch = 0; ch < chars.Length; ch++)
                {
                    if (str[q] == chars[ch])
                    {
                        break;
                    }
                }
                if (str[q] == '\n')
                {
                    curY -= charHeight * Size;
                    GL.Translate(-xp, -charHeight * Size, 0);
                    xp = 0;
                    continue;
                }
                float w = uv[ch].w;
                float wm = w * Size;
                xp += wm;
                DrawChar(ch);
                GL.Translate(wm, 0, 0);
            }
            GL.PopMatrix();
            GL.PopAttrib();
        }
    }
}
