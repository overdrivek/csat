#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2008-2012 mjt
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing details.
 */
#endregion
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;

namespace CSatEng
{
    public class PostEffect
    {
        GLSLShader effect;
        static FBO destinationFbo;
        static int effCount = 0;

        public static PostEffect Load(string shaderFileName, string flags)
        {
            PostEffect eff = new PostEffect();
            eff.effect = GLSLShader.Load(shaderFileName + (flags == "" ? "" : ":" + flags));
            return eff;
        }

        public void SetParameter(string name, float val)
        {
            if (effect == null) return;

            effect.SetUniform(name, val);
        }

        public void RenderEffect()
        {
            if (effect == null) return;

            int curTex = 0;
            if (effCount % 2 == 0)
            {
                GL.ReadBuffer(ReadBufferMode.ColorAttachment0);
                GL.DrawBuffer(DrawBufferMode.ColorAttachment1);
                curTex = 0;
            }
            else
            {
                GL.ReadBuffer(ReadBufferMode.ColorAttachment1);
                GL.DrawBuffer(DrawBufferMode.ColorAttachment0);
                curTex = 1;
            }
            effect.UseProgram();
            destinationFbo.ColorTextures[curTex].DrawFullScreen(0, 0);
            PostEffect.effCount++;

        }

        /// <summary>
        /// screen fbo:ssa pitää olla vähintään 2 colorbufferia,
        /// ja 1. colorbufferissa pitää olla rendattu skene johon efektit halutaan.
        /// </summary>
        public static void Begin(FBO screen)
        {
            if (screen.ColorTextures.Length < 2) Log.Error("PostEffect: fbo must have at least 2 colorbuffers.");
            VBO.FastRenderPass = true;
            effCount = 0;
            destinationFbo = screen;
            screen.BindFBO();
        }

        public static Texture2D End()
        {
            VBO.FastRenderPass = false;
            GL.ReadBuffer(ReadBufferMode.ColorAttachment0);
            GL.DrawBuffer(DrawBufferMode.ColorAttachment0);
            destinationFbo.UnBindFBO();

            if (effCount % 2 == 0)
            {
                return destinationFbo.ColorTextures[0];
            }
            else
            {
                return destinationFbo.ColorTextures[1];
            }
        }
    }
}
