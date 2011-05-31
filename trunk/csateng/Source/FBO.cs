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
        uint colorTexture;
        uint depthTexture;
        uint fboHandle = 0;

        public static int Width, Height;
        public static bool IsSupported = false;
        public ClearBufferMask ClearFlags = 0;

        public FBO(int width, int height, bool color, bool depth)
        {
            if (IsSupported == false) return;

            if (fboHandle != 0) return;
            Width = width;
            Height = height;

            if (color)
            {
                ClearFlags = ClearBufferMask.ColorBufferBit;
                // Create Color Tex
                GL.GenTextures(1, out colorTexture);
                GL.BindTexture(TextureTarget.Texture2D, colorTexture);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, Width, Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);
                // GL.Ext.GenerateMipmap( GenerateMipmapTarget.Texture2D );
            }

            if (depth)
            {
                ClearFlags |= ClearBufferMask.DepthBufferBit;

                PixelInternalFormat[] modes ={(PixelInternalFormat)All.DepthComponent32, (PixelInternalFormat)All.DepthComponent24,
                                                (PixelInternalFormat)All.DepthComponent16, (PixelInternalFormat)All.DepthComponent };

                // Create Depth Tex
                for (int q = 0; q < modes.Length; q++)
                {
                    GL.GenTextures(1, out depthTexture);
                    GL.BindTexture(TextureTarget.Texture2D, depthTexture);
                    GL.TexImage2D(TextureTarget.Texture2D, 0, modes[q], Width, Height, 0, PixelFormat.DepthComponent, PixelType.UnsignedInt, IntPtr.Zero);
                    if (depthTexture > 0 && GL.GetError() == ErrorCode.NoError) break;
                }

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);

                // This is to allow usage of shadow2DProj function in the shader
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureCompareMode, (int)TextureCompareMode.CompareRToTexture);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureCompareFunc, (int)All.Lequal);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.DepthTextureMode, (int)All.Intensity);
                // GL.Ext.GenerateMipmap( GenerateMipmapTarget.Texture2D );
            }

            // Create a FBO and attach the textures
            GL.Ext.GenFramebuffers(1, out fboHandle);
            GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, fboHandle);
            if (color) GL.Ext.FramebufferTexture2D(FramebufferTarget.FramebufferExt, FramebufferAttachment.ColorAttachment0Ext, TextureTarget.Texture2D, colorTexture, 0);
            else
            {
                GL.DrawBuffer(DrawBufferMode.None);
                GL.ReadBuffer(ReadBufferMode.None);
            }

            if (depth) GL.Ext.FramebufferTexture2D(FramebufferTarget.FramebufferExt, FramebufferAttachment.DepthAttachmentExt, TextureTarget.Texture2D, depthTexture, 0);

            #region messages
            bool ok = false;
            switch (GL.Ext.CheckFramebufferStatus(FramebufferTarget.FramebufferExt))
            {
                case FramebufferErrorCode.FramebufferCompleteExt:
                    {
                        Log.WriteLine("FBO: The framebuffer is complete and valid for rendering.");
                        if (depth)
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
            #endregion

            // using FBO might have changed states, e.g. the FBO might not support stereoscopic views or double buffering
            int[] queryinfo = new int[6];
            GL.GetInteger(GetPName.MaxColorAttachmentsExt, out queryinfo[0]);
            GL.GetInteger(GetPName.AuxBuffers, out queryinfo[1]);
            GL.GetInteger(GetPName.MaxDrawBuffers, out queryinfo[2]);
            GL.GetInteger(GetPName.Stereo, out queryinfo[3]);
            GL.GetInteger(GetPName.Samples, out queryinfo[4]);
            GL.GetInteger(GetPName.Doublebuffer, out queryinfo[5]);
            Log.WriteLine("Max ColorBuffers: " + queryinfo[0] + "\nMax AuxBuffers: " + queryinfo[1] + "\nMax DrawBuffers: " + queryinfo[2] +
                               "\nStereo: " + queryinfo[3] + "\nSamples: " + queryinfo[4] + "\nDoubleBuffer: " + queryinfo[5]);

            if (ok == false)
            {
                IsSupported = false;
                Log.WriteLine("Last GL Error: " + GL.GetError());
            }

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, 0); // disable rendering into the FBO
            GL.BindTexture(TextureTarget.Texture2D, 0);

            IsSupported = true;
        }

        public void Dispose()
        {
            if (fboHandle != 0)
            {
                // Clean up what we allocated before exiting
                if (colorTexture != 0) GL.DeleteTextures(1, ref colorTexture);
                if (depthTexture != 0) GL.DeleteTextures(1, ref depthTexture);
                GL.Ext.DeleteFramebuffers(1, ref fboHandle);
                colorTexture = depthTexture = fboHandle = 0;
                Log.WriteLine("Disposed: FBO", true);
            }
        }

        public static float ZNear = 100, ZFar = 1000;
        public void BindFBO()
        {
            if (IsSupported == false) return;
            GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, fboHandle);
            GL.Viewport(0, 0, Width, Height);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            Matrix4 perpective = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(Camera.Fov), (float)Width / (float)Height, ZNear, ZFar);
            GL.LoadMatrix(ref perpective);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
        }

        public void UnBindFBO()
        {
            if (IsSupported == false) return;
            GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, 0);
            Camera.Set3D();
        }

        public void BindDepth()
        {
            if (IsSupported == false) return;
            Texture.Bind(BaseGame.SHADOW_TEXUNIT, depthTexture);
        }
        public void UnBindDepth()
        {
            if (IsSupported == false) return;
            Texture.UnBind(BaseGame.SHADOW_TEXUNIT);
        }

        public Texture2D CreateDrawableColorTexture()
        {
            return Texture2D.CreateDrawableTexture(Width, Height, colorTexture);
        }
        public Texture2D CreateDrawableDepthTexture()
        {
            return Texture2D.CreateDrawableTexture(Width, Height, depthTexture);
        }

    }
}
