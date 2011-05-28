#region --- License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2006-2008 the OpenTK Team.
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing details.
 * 
 * modified by mjt, 2011
 */
#endregion

using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL;

namespace CSatEng
{
    public static class ImageGDI
    {
        public static int RealWidth, RealHeight;

        public static void LoadFromDisk(string filename, out uint texturehandle, out TextureTarget dimension)
        {
            LoadFromDisk(filename, out texturehandle, out dimension,
                         TextureLoaderParameters.MinificationFilter, TextureLoaderParameters.MagnificationFilter,
                         TextureLoaderParameters.WrapModeS, TextureLoaderParameters.WrapModeT);
        }

        public static void LoadFromDisk(string filename, out uint texturehandle, out TextureTarget dimension, TextureMinFilter minFilter, TextureMagFilter magFilter, TextureWrapMode wrapS, TextureWrapMode wrapT)
        {
            dimension = (TextureTarget)0;
            texturehandle = TextureLoaderParameters.OpenGLDefaultTexture;
            ErrorCode GLError = ErrorCode.NoError;

            Bitmap CurrentBitmap = null;

            try // Exceptions will be thrown if any problem occurs while working on the file.
            {
                CurrentBitmap = new Bitmap(filename);
                if (TextureLoaderParameters.FlipImages)
                    CurrentBitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);

                if (CurrentBitmap.Height > 1)
                    dimension = TextureTarget.Texture2D;
                else
                    dimension = TextureTarget.Texture1D;

                GL.GenTextures(1, out texturehandle);
                GL.BindTexture(dimension, texturehandle);

                #region Load Texture
                OpenTK.Graphics.OpenGL.PixelInternalFormat pif;
                OpenTK.Graphics.OpenGL.PixelFormat pf;
                OpenTK.Graphics.OpenGL.PixelType pt;

                if (TextureLoaderParameters.Verbose)
                    Trace.WriteLine("File: " + filename + " Format: " + CurrentBitmap.PixelFormat);

                switch (CurrentBitmap.PixelFormat)
                {
                    case System.Drawing.Imaging.PixelFormat.Format8bppIndexed: // misses glColorTable setup
                        pif = OpenTK.Graphics.OpenGL.PixelInternalFormat.Rgb8;
                        pf = OpenTK.Graphics.OpenGL.PixelFormat.ColorIndex;
                        pt = OpenTK.Graphics.OpenGL.PixelType.Bitmap;
                        break;
                    case System.Drawing.Imaging.PixelFormat.Format16bppArgb1555:
                    case System.Drawing.Imaging.PixelFormat.Format16bppRgb555: // does not work
                        pif = OpenTK.Graphics.OpenGL.PixelInternalFormat.Rgb5A1;
                        pf = OpenTK.Graphics.OpenGL.PixelFormat.Bgr;
                        pt = OpenTK.Graphics.OpenGL.PixelType.UnsignedShort5551Ext;
                        break;
                    case System.Drawing.Imaging.PixelFormat.Format24bppRgb: // works
                        pif = OpenTK.Graphics.OpenGL.PixelInternalFormat.Rgb8;
                        pf = OpenTK.Graphics.OpenGL.PixelFormat.Bgr;
                        pt = OpenTK.Graphics.OpenGL.PixelType.UnsignedByte;
                        break;
                    case System.Drawing.Imaging.PixelFormat.Format32bppRgb: // has alpha too? wtf?
                    case System.Drawing.Imaging.PixelFormat.Canonical:
                    case System.Drawing.Imaging.PixelFormat.Format32bppArgb: // works
                        pif = OpenTK.Graphics.OpenGL.PixelInternalFormat.Rgba;
                        pf = OpenTK.Graphics.OpenGL.PixelFormat.Bgra;
                        pt = OpenTK.Graphics.OpenGL.PixelType.UnsignedByte;
                        break;
                    default:
                        Util.Error("ERROR: Unsupported Pixel Format " + CurrentBitmap.PixelFormat);
                        pif = OpenTK.Graphics.OpenGL.PixelInternalFormat.Rgba;
                        pf = OpenTK.Graphics.OpenGL.PixelFormat.Bgra;
                        pt = OpenTK.Graphics.OpenGL.PixelType.UnsignedByte;
                        break;
                }

                RealWidth = CurrentBitmap.Width;
                RealHeight = CurrentBitmap.Height;

                if (Texture.IsNPOTSupported == false)
                {
                    // tarkista onko texturen koko oikeanlainen (64, 128, 256, jne)
                    int test = 1, w = 0, h = 0;
                    bool wOK = false, hOK = false;
                    for (int q = 0; q < 20; q++)
                    {
                        test *= 2;
                        if (test == CurrentBitmap.Width) { w = test; wOK = true; }
                        if (test == CurrentBitmap.Height) { h = test; hOK = true; }
                        if (test > CurrentBitmap.Width && w == 0) w = test / 2;
                        if (test > CurrentBitmap.Height && h == 0) h = test / 2;
                        if (wOK && hOK) break;
                    }
                    if (wOK == false || hOK == false)
                    {
                        Log.WriteLine("Converting texture [" + CurrentBitmap.Width + ", " + CurrentBitmap.Height + "] -> [" + w + ", " + h + "]");

                        Bitmap bm = new Bitmap(w, h, CurrentBitmap.PixelFormat);
                        Graphics g = Graphics.FromImage(bm);

                        g.DrawImage(CurrentBitmap, new Rectangle(0, 0, w, h));

                        CurrentBitmap.Dispose();
                        CurrentBitmap = bm;
                    }
                }

                BitmapData Data = CurrentBitmap.LockBits(new Rectangle(0, 0, CurrentBitmap.Width, CurrentBitmap.Height), ImageLockMode.ReadOnly, CurrentBitmap.PixelFormat);
                if (Data.Height > 1) // image is 2D
                {
                    if (TextureLoaderParameters.BuildMipmapsForUncompressed)
                        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

                    GL.TexImage2D(dimension, 0, pif, Data.Width, Data.Height, TextureLoaderParameters.Border, pf, pt, Data.Scan0);
                }
                else // image is 1D
                {
                    GL.TexImage1D(dimension, 0, pif, Data.Width, TextureLoaderParameters.Border, pf, pt, Data.Scan0);
                }

                GL.Finish();
                GLError = GL.GetError();
                if (GLError != ErrorCode.NoError)
                {
                    Util.Error("Error building TexImage. GL Error: " + GLError);
                }

                CurrentBitmap.UnlockBits(Data);
                #endregion Load Texture

                #region Set Texture Parameters
                GL.TexParameter(dimension, TextureParameterName.TextureMinFilter, (int)minFilter); //(int)TextureLoaderParameters.MinificationFilter);
                GL.TexParameter(dimension, TextureParameterName.TextureMagFilter, (int)magFilter); //(int)TextureLoaderParameters.MagnificationFilter);

                GL.TexParameter(dimension, TextureParameterName.TextureWrapS, (int)wrapS); //(int)TextureLoaderParameters.WrapModeS);
                GL.TexParameter(dimension, TextureParameterName.TextureWrapT, (int)wrapT); //(int)TextureLoaderParameters.WrapModeT);

                GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)TextureLoaderParameters.EnvMode);

                GLError = GL.GetError();
                if (GLError != ErrorCode.NoError)
                {
                    Util.Error("Error setting Texture Parameters. GL Error: " + GLError);
                }
                #endregion Set Texture Parameters

                return; // success
            }
            catch (Exception e)
            {
                dimension = (TextureTarget)0;
                texturehandle = TextureLoaderParameters.OpenGLDefaultTexture;
                Util.Error("Texture Loading Error: Failed to read file " + filename + ".\n" + e);
                // return; // failure
            }
            finally
            {
                CurrentBitmap = null;
            }
        }

    }
}
