#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2008-2012 mjt
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing details.
 */
#endregion
using System;
using System.IO;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace CSatEng
{
    public static class ShadowMapping
    {
        public static bool UseShadowMapping = false;
        static Texture lightMask;
        static GLSLShader depthShader, depthShaderAlphaTest;
        static FBO fbo;

        public static void Create(FBO fbo, string lightMaskFileName)
        {
            if (Settings.DisableShadowMapping)
            {
                UseShadowMapping = false;
                return;
            }

            if (FBO.IsSupported == false || GLSLShader.IsSupported == false)
            {
                string ext;
                if (FBO.IsSupported == false) ext = "FBOs"; else ext = "Shaders";
                Log.WriteLine(ext + " not supported so no shadow mapping.");
                UseShadowMapping = false;
                return;
            }

            ShadowMapping.fbo = fbo;
            UseShadowMapping = true;
            TextureLoaderParameters.WrapModeS = TextureWrapMode.ClampToEdge;
            TextureLoaderParameters.WrapModeT = TextureWrapMode.ClampToEdge;
            try
            {
                lightMask = Texture.Load(lightMaskFileName);
            }
            catch (Exception) { } // skipataan valitukset jos tiedostoa ei löydy
            TextureLoaderParameters.WrapModeS = TextureWrapMode.Repeat;
            TextureLoaderParameters.WrapModeT = TextureWrapMode.Repeat;
            depthShader = GLSLShader.Load("depth.shader");
            depthShaderAlphaTest = GLSLShader.Load("depth.shader:ALPHATEST");
        }

        public static void BindLightMask()
        {
            if (lightMask != null)
                lightMask.Bind(Settings.LIGHTMASK_TEXUNIT);
        }
        public static void UnBindLightMask()
        {
            Texture.UnBind(Settings.LIGHTMASK_TEXUNIT);
        }

        /// <summary>
        /// renderoi worldin valosta päin (pelkän depthin)
        /// </summary>
        public static void SetupShadows(Renderable world, int lightNo, bool withParticles)
        {
            if (UseShadowMapping == false) return;

            if (Light.Lights.Count == 0)
            {
                Log.WriteLine("SetupShadows requires at least one light source!", false);
                return;
            }
            GL.Disable(EnableCap.Blend);
            GL.ColorMask(false, false, false, false);
            GL.Disable(EnableCap.CullFace);
            GL.PolygonOffset(1, 1);
            GL.Enable(EnableCap.PolygonOffsetFill);

            fbo.BindDepth();
            fbo.BindFBO();
            fbo.Clear();

            // kuvakulma valosta päin
            GLExt.LoadMatrix(ref Light.Lights[lightNo].OrigOrientationMatrix);
            GLExt.Translate(-Light.Lights[lightNo].Position.X, -Light.Lights[lightNo].Position.Y, -Light.Lights[lightNo].Position.Z);

            SetTextureMatrix();
            Frustum.CalculateFrustum();

            VBO.FastRenderPass = true;
            depthShader.UseProgram();
            world.Render();
            if (withParticles)
            {
                depthShaderAlphaTest.UseProgram();
                Particles.Render();
                GLSLShader.UnBindShader();
            }
            VBO.FastRenderPass = false;
            fbo.UnBindFBO();

            GL.Disable(EnableCap.PolygonOffsetFill);
            GL.Enable(EnableCap.CullFace);
            GL.ColorMask(true, true, true, true);

            GLExt.LoadIdentity();
            GameClass.NumOfObjects = 0;

            ShadowMapping.UnBindLightMask();
        }

        /// <summary>
        /// aseta texturematriisit shadowmapping shaderia varten
        /// </summary>
        static void SetTextureMatrix()
        {
            Matrix4 projMatrix = GLExt.ProjectionMatrix, modelMatrix = GLExt.ModelViewMatrix;
            GLExt.MatrixMode(MatrixMode.Texture);
            GLExt.LoadIdentity();
            GLExt.Translate(0.5f, 0.5f, 0.5f); // remap from [-1,1]^2 to [0,1]^2
            GLExt.Scale(0.5f, 0.5f, 0.5f);
            GLExt.MultMatrix(ref projMatrix);
            GLExt.MultMatrix(ref modelMatrix);
            GLExt.MatrixMode(MatrixMode.Modelview);
        }
    }
}
