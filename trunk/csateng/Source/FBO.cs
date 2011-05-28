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
        public static bool FboSupported = false;
        public ClearBufferMask ClearFlags = 0;

        public FBO(int width, int height, bool color, bool depth)
        {
            if (fboHandle != 0) return;
            Width = width;
            Height = height;

            if (!GL.GetString(StringName.Extensions).Contains("EXT_framebuffer_object"))
            {
                FboSupported = false;
                Log.WriteLine("FBOs not supported! Your video card does not support Framebuffer Objects. Please update your drivers.");
                return;
            }

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
                // Create Depth Tex
                GL.GenTextures(1, out depthTexture);
                GL.BindTexture(TextureTarget.Texture2D, depthTexture);
                GL.TexImage2D(TextureTarget.Texture2D, 0, (PixelInternalFormat)All.DepthComponent, Width, Height, 0, PixelFormat.DepthComponent, PixelType.UnsignedInt, IntPtr.Zero);
                // things go horribly wrong if DepthComponent's Bitcount does not match the main Framebuffer's Depth
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
            Log.WriteLine("max ColorBuffers: " + queryinfo[0] + " max AuxBuffers: " + queryinfo[1] + " max DrawBuffers: " + queryinfo[2] +
                               " Stereo: " + queryinfo[3] + " Samples: " + queryinfo[4] + " DoubleBuffer: " + queryinfo[5]);

            if (ok == false)
            {
                FboSupported = false;
                Log.WriteLine("Last GL Error: " + GL.GetError());
            }

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, 0); // disable rendering into the FBO
            GL.BindTexture(TextureTarget.Texture2D, 0);

            FboSupported = true;
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

        public void BindFBO()
        {
            GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, fboHandle);
            GL.Viewport(0, 0, Width, Height);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            Matrix4 perpective = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(Camera.Fov), (float)Width / (float)Height, Camera.Near, Camera.Far);
            GL.LoadMatrix(ref perpective);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
        }

        public void UnBindFBO()
        {
            GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, 0);
            Camera.Set3D();
        }

        int _texUnit;
        public void BindDepth(int texUnit)
        {
            _texUnit = texUnit;
            GL.ActiveTexture(TextureUnit.Texture0 + texUnit);
            GL.BindTexture(TextureTarget.Texture2D, depthTexture);
        }
        public void UnBindDepth()
        {
            GL.ActiveTexture(TextureUnit.Texture0 + _texUnit);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }


#if DEBUG
        // DEBUG : renderoi color ja depth buffer ruudulle
        Texture2D d = null, c;
        public void DEBUGRENDER()
        {
            if (d == null)
            {
                d = new Texture2D();
                c = new Texture2D();
                d.CreateDrawableTexture(Width, Height, depthTexture);
                c.CreateDrawableTexture(Width, Height, colorTexture);
            }
            Camera.Set2D();
            c.Draw(0, 0);
            d.Draw(800, 100);
            Camera.Set3D();
        }
#endif

    }
}
