#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2008-2014 mjt
 * This notice may not be removed from any source distribution.
 * See csat-license.txt for licensing details.
 */
#endregion
using System;
using OpenTK.Graphics.OpenGL;

namespace CSatEng
{
    public class FBO
    {
        uint[] colorTexture;
        uint depthTexture;
        uint fboHandle = 0;
        public Texture2D DepthTexture;
        public Texture2D[] ColorTextures;
        public int Width, Height;
        public static int WidthS = 512, HeightS = 512;
        public static bool IsSupported = true;
        public static float ColorZNear = 1, ColorZFar = 1000;
        public static float DepthZNear = 1, DepthZFar = 1000;
        ClearBufferMask clearFlags = 0;

        /// <summary>
        /// luo fbo. jos width ja height on 0, käytetään arvoja jotka on jo Width ja Height:ssä 
        /// (ladattu settings.xml tiedostosta)
        /// </summary>
        public FBO(int width, int height, int createColorBuffers, bool createDepthBuffer)
        {
            if (IsSupported == false) return;

            if (fboHandle != 0) return;
            if (width != 0 && height != 0)
            {
                Width = width;
                Height = height;
            }
            else
            {
                Width = WidthS; // aseta settings.xml tiedostossa annetut arvot
                Height = HeightS;
            }

            GL.Ext.GenFramebuffers(1, out fboHandle);
            GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, fboHandle);

            if (createColorBuffers > 0) // Create Color Tex
            {
                clearFlags = ClearBufferMask.ColorBufferBit;
                colorTexture = new uint[createColorBuffers];
                PixelInternalFormat[] modes = { PixelInternalFormat.Rgba32f, PixelInternalFormat.Rgba16f, PixelInternalFormat.Rgba8 };

                for (int q = 0; q < createColorBuffers; q++)
                {
                    int c;
                    if (Texture.IsFloatTextureSupported) c = 0;
                    else c = 2;

                    // koitetaan luoda texture
                    for (; c < modes.Length; c++)
                    {
                        GL.GenTextures(1, out colorTexture[q]);
                        GL.BindTexture(TextureTarget.Texture2D, colorTexture[q]);
                        if (c < 2)
                            GL.TexImage2D(TextureTarget.Texture2D, 0, modes[c], Width, Height, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
                        else
                            GL.TexImage2D(TextureTarget.Texture2D, 0, modes[c], Width, Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
                        if (colorTexture[q] > 0 && GL.GetError() == ErrorCode.NoError)
                        {
                            if (c < 2) Log.WriteLine("FBO: float color texture created " + modes[c]);
                            else Log.WriteLine("FBO: color texture created " + modes[c]);
                            break;
                        }
                    }
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);
                    GL.Ext.GenerateMipmap(GenerateMipmapTarget.Texture2D);
                    GL.Ext.FramebufferTexture2D(FramebufferTarget.FramebufferExt, FramebufferAttachment.ColorAttachment0Ext + q, TextureTarget.Texture2D, colorTexture[q], 0);
                }
            }
            else
            {
                GL.DrawBuffer(DrawBufferMode.None);
                GL.ReadBuffer(ReadBufferMode.None);
            }

            if (createDepthBuffer) // Create Depth Tex
            {
                clearFlags |= ClearBufferMask.DepthBufferBit;
                PixelInternalFormat[] modes ={PixelInternalFormat.DepthComponent32f,
                        PixelInternalFormat.DepthComponent32, PixelInternalFormat.DepthComponent24,
                        PixelInternalFormat.DepthComponent16, PixelInternalFormat.DepthComponent };

                int c;
                if (Texture.IsFloatTextureSupported) c = 0;
                else c = 1;

                // koitetaan luoda texture
                for (; c < modes.Length; c++)
                {
                    GL.GenTextures(1, out depthTexture);
                    GL.BindTexture(TextureTarget.Texture2D, depthTexture);
                    if (c == 0)
                        GL.TexImage2D(TextureTarget.Texture2D, 0, modes[c], Width, Height, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
                    else
                        GL.TexImage2D(TextureTarget.Texture2D, 0, modes[c], Width, Height, 0, PixelFormat.DepthComponent, PixelType.UnsignedInt, IntPtr.Zero);
                    if (depthTexture > 0 && GL.GetError() == ErrorCode.NoError)
                    {
                        if (c == 0) Log.WriteLine("FBO: float depth texture created " + modes[c]);
                        else Log.WriteLine("FBO: depth texture created " + modes[c]);
                        break;
                    }
                }
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);
                GL.Ext.GenerateMipmap(GenerateMipmapTarget.Texture2D);
                GL.Ext.FramebufferTexture2D(FramebufferTarget.FramebufferExt, FramebufferAttachment.DepthAttachmentExt, TextureTarget.Texture2D, depthTexture, 0);

                // This is to allow usage of shadow2DProj function in the shader
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureCompareMode, (int)TextureCompareMode.CompareRToTexture);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureCompareFunc, (int)All.Lequal);

                if (Settings.UseGL3 == false) GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.DepthTextureMode, (int)All.Intensity);
            }

            bool ok = CheckFBOError();

            // using FBO might have changed states, e.g. the FBO might not support stereoscopic views or double buffering
            if (Settings.UseGL3 == false)
            {
                int[] queryinfo = new int[6];
                GL.GetInteger(GetPName.MaxColorAttachmentsExt, out queryinfo[0]);
                GL.GetInteger(GetPName.AuxBuffers, out queryinfo[1]);
                GL.GetInteger(GetPName.MaxDrawBuffers, out queryinfo[2]);
                GL.GetInteger(GetPName.Stereo, out queryinfo[3]);
                GL.GetInteger(GetPName.Samples, out queryinfo[4]);
                GL.GetInteger(GetPName.Doublebuffer, out queryinfo[5]);
                Log.WriteLine("Max ColorBuffers: " + queryinfo[0] + " / Max AuxBuffers: " + queryinfo[1] + " / Max DrawBuffers: " + queryinfo[2] +
                    " / Stereo: " + (queryinfo[3] > 0 ? "true" : "false") + " / Samples: " + queryinfo[4] + " / DoubleBuffer: " + queryinfo[5]);
            }

            if (ok == false)
            {
                IsSupported = false;
                Log.WriteLine("Error: " + GL.GetError());
                return;
            }
            else IsSupported = true;

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, 0); // disable rendering into the FBO
            GL.BindTexture(TextureTarget.Texture2D, 0);

            CreateTextures();
        }

        void CreateTextures()
        {
            if (IsSupported == false) return;

            if (depthTexture != 0) DepthTexture = Texture2D.CreateDrawableTexture(Width, Height, depthTexture);
            if (colorTexture != null)
            {
                ColorTextures = new Texture2D[colorTexture.Length];
                for (int q = 0; q < colorTexture.Length; q++)
                    ColorTextures[q] = Texture2D.CreateDrawableTexture(Width, Height, colorTexture[q]);
            }
        }

        bool CheckFBOError()
        {
            #region messages
            bool ok = false;
            switch (GL.Ext.CheckFramebufferStatus(FramebufferTarget.FramebufferExt))
            {
                case FramebufferErrorCode.FramebufferCompleteExt:
                    {
                        Log.WriteLine("FBO: The framebuffer is complete and valid for rendering.");
                        if (Settings.UseGL3 == false)
                        {
                            int bit;
                            GL.GetInteger(GetPName.DepthBits, out bit);
                            Log.WriteLine("Depthbits: " + bit);
                        }
                        ok = true;
                        break;
                    }
                case FramebufferErrorCode.FramebufferIncompleteAttachmentExt:
                    {
                        Log.WriteLine("FBO: One or more attachment points are not framebuffer attachment complete. This could mean there’s no texture attached or the format isn’t renderable. For color textures this means the base format must be RGB or RGBA and for depth textures it must be a DEPTH_COMPONENT format. Other causes of this error are that the width or height is zero or the z-offset is out of range in case of render to volume.");
                        break;
                    }
                case FramebufferErrorCode.FramebufferIncompleteMissingAttachmentExt:
                    {
                        Log.WriteLine("FBO: There are no attachments.");
                        break;
                    }
                case FramebufferErrorCode.FramebufferIncompleteDimensionsExt:
                    {
                        Log.WriteLine("FBO: Attachments are of different size. All attachments must have the same width and height.");
                        break;
                    }
                case FramebufferErrorCode.FramebufferIncompleteFormatsExt:
                    {
                        Log.WriteLine("FBO: The color attachments have different format. All color attachments must have the same format.");
                        break;
                    }
                case FramebufferErrorCode.FramebufferIncompleteDrawBufferExt:
                    {
                        Log.WriteLine("FBO: An attachment point referenced by GL.DrawBuffers() doesn’t have an attachment.");
                        break;
                    }
                case FramebufferErrorCode.FramebufferIncompleteReadBufferExt:
                    {
                        Log.WriteLine("FBO: The attachment point referenced by GL.ReadBuffers() doesn’t have an attachment.");
                        break;
                    }
                case FramebufferErrorCode.FramebufferUnsupportedExt:
                    {
                        Log.WriteLine("FBO: This particular FBO configuration is not supported by the implementation.");
                        break;
                    }
                default:
                    {
                        Log.WriteLine("FBO: Status unknown (fbo disabled)");
                        break;
                    }
            }
            return ok;
            #endregion
        }

        public void Dispose()
        {
            if (fboHandle != 0)
            {
                // Clean up what we allocated before exiting
                if (colorTexture != null)
                    for (int q = 0; q < colorTexture.Length; q++)
                    {
                        if (colorTexture[q] != 0) GL.DeleteTextures(1, ref colorTexture[q]);
                        colorTexture[q] = 0;
                    }
                if (depthTexture != 0) GL.DeleteTextures(1, ref depthTexture);
                GL.Ext.DeleteFramebuffers(1, ref fboHandle);
                depthTexture = fboHandle = 0;
                Log.WriteLine("Disposed: FBO", false);
            }
        }

        /// <summary>
        /// aloita piirtämään fbo:hon.
        /// </summary>
        public void BindFBO()
        {
            if (IsSupported == false) return;
            GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, fboHandle);
            GL.Viewport(0, 0, Width, Height);
        }

        public void UnBindFBO()
        {
            if (IsSupported == false) return;
            GL.Viewport(0, 0, Settings.Width, Settings.Height);
            GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, 0);
        }

        public void BindDepth()
        {
            if (IsSupported == false) return;
            Texture.Bind(Settings.DEPTH_TEXUNIT, TextureTarget.Texture2D, depthTexture);
        }
        public void UnBindDepth()
        {
            if (IsSupported == false) return;
            Texture.UnBind(Settings.DEPTH_TEXUNIT);
        }
        public void BindColorBuffer(int colorBufferNo, int textureUnit)
        {
            if (IsSupported == false) return;
            Texture.Bind(textureUnit, TextureTarget.Texture2D, colorTexture[colorBufferNo]);
        }
        public void UnBindColorBuffer(int textureUnit)
        {
            if (IsSupported == false) return;
            Texture.UnBind(textureUnit);
        }

        public void Clear()
        {
            GL.Clear(clearFlags);
        }

    }

}
