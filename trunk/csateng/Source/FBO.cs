#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2011 mjt[matola@sci.fi]
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing details.
 */
#endregion
using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace CSatEng
{
    public class FBO
    {
        uint[] colorTexture;
        uint depthTexture;
        uint fboHandle = 0;
        ClearBufferMask clearFlags = 0;
        public static int Width, Height;
        public static bool IsSupported = false;
        public static float ColorZNear = 1, ColorZFar = 1000;
        public static float DepthZNear = 500, DepthZFar = 800;

        public FBO(int width, int height, int createColorBuffers, bool createDepthBuffer)
        {
            if (IsSupported == false) return;

            if (fboHandle != 0) return;
            Width = width;
            Height = height;

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
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.DepthTextureMode, (int)All.Intensity);
            }

            bool ok = CheckFBOError();

            // using FBO might have changed states, e.g. the FBO might not support stereoscopic views or double buffering
            int[] queryinfo = new int[6];
            GL.GetInteger(GetPName.MaxColorAttachmentsExt, out queryinfo[0]);
            GL.GetInteger(GetPName.AuxBuffers, out queryinfo[1]);
            GL.GetInteger(GetPName.MaxDrawBuffers, out queryinfo[2]);
            GL.GetInteger(GetPName.Stereo, out queryinfo[3]);
            GL.GetInteger(GetPName.Samples, out queryinfo[4]);
            GL.GetInteger(GetPName.Doublebuffer, out queryinfo[5]);
            Log.WriteLine("Max ColorBuffers: " + queryinfo[0] + " / Max AuxBuffers: " + queryinfo[1] + " / Max DrawBuffers: " + queryinfo[2] +
                               " / Stereo: " + queryinfo[3] + " / Samples: " + queryinfo[4] + " / DoubleBuffer: " + queryinfo[5]);

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
                        int bit;
                        GL.GetInteger(GetPName.DepthBits, out bit);
                        Log.WriteLine("Depthbits: " + bit);
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
                Log.WriteLine("Disposed: FBO", true);
            }
        }

        /// <summary>
        /// aloita piirtämään fbo:hon. jos drawtodepth==true, asetetaan pienemmät ZNear ja ZFar arvot.
        /// </summary>
        public void BeginDrawing(bool drawToDepth)
        {
            if (IsSupported == false) return;
            GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, fboHandle);

            float ZNear, ZFar;
            if (drawToDepth)
            {
                ZNear = DepthZNear;
                ZFar = DepthZFar;
            }
            else
            {
                ZNear = ColorZNear;
                ZFar = ColorZFar;
            }

            GL.MatrixMode(MatrixMode.Projection);
            GL.PushMatrix();
            Matrix4 perpective = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(Camera.Fov), (float)Width / (float)Height, ZNear, ZFar);
            GL.LoadMatrix(ref perpective);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushAttrib(AttribMask.ViewportBit);
            GL.Viewport(0, 0, Width, Height);
        }

        public void EndDrawing()
        {
            if (IsSupported == false) return;
            GL.PopAttrib();
            GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, 0);
            GL.MatrixMode(MatrixMode.Projection);
            GL.PopMatrix();
            GL.MatrixMode(MatrixMode.Modelview);
        }

        public void BindDepth()
        {
            Texture.Bind(Settings.DEPTH_TEXUNIT, TextureTarget.Texture2D, depthTexture);
        }
        public void UnBindDepth()
        {
            Texture.UnBind(Settings.DEPTH_TEXUNIT);
        }
        public void BindColor(int colorBufferNo, int textureUnit)
        {
            Texture.Bind(textureUnit, TextureTarget.Texture2D, colorTexture[colorBufferNo]);
        }
        public void UnBindColor(int textureUnit)
        {
            Texture.UnBind(textureUnit);
        }

        public void Clear()
        {
            GL.Clear(clearFlags);
        }
        public Texture2D CreateDrawableColorTexture(int tex)
        {
            return Texture2D.CreateDrawableTexture(Width, Height, colorTexture[tex]);
        }
        public Texture2D CreateDrawableDepthTexture()
        {
            return Texture2D.CreateDrawableTexture(Width, Height, depthTexture);
        }

    }

}
