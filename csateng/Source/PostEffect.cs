#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2011 mjt
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
        List<GLSLShader> effects = new List<GLSLShader>();
        float param = 0;
        public static float EffParam = 0;
        static FBO destinationFbo;
        static int effCount = 0;

        public static PostEffect Load(string shaderFileName, string flags, float effParam)
        {
            PostEffect eff = new PostEffect();
            eff.param = effParam;
            if (flags == "") eff.effects.Add(GLSLShader.Load(shaderFileName, new ShaderCallback(CallBacks.EffectShaderCallBack)));
            else
            {
                string[] par = flags.Split(' ');
                for (int q = 0; q < par.Length; q++)
                {
                    eff.effects.Add(GLSLShader.Load(shaderFileName + ":" + par[q], new ShaderCallback(CallBacks.EffectShaderCallBack)));
                }
            }
            return eff;
        }

        public void RenderEffect()
        {
            int curTex = 0;
            foreach (GLSLShader eff in effects)
            {
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
                PostEffect.EffParam = param;
                eff.UseProgram();
                destinationFbo.ColorTextures[curTex].DrawFullScreen(0, 0);
                PostEffect.effCount++;
            }
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
