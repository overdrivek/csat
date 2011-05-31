#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2011 mjt[matola@sci.fi]
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
    public class ShadowMapping
    {
        public static bool UseShadowMapping = false;
        public static bool ShadowPass = false;
        static Texture lightMask;
        FBO fbo;

        public ShadowMapping(FBO fbo)
        {
            Create(fbo, "lightmask.png");
        }
        public ShadowMapping(FBO fbo, string lightMaskFileName)
        {
            Create(fbo, lightMaskFileName);
        }
        void Create(FBO fbo, string lightMaskFileName)
        {
            if (FBO.IsSupported == false)
            {
                Log.WriteLine("FBO not supported so no shadow mapping.");
                UseShadowMapping = false;
                return;
            }
            this.fbo = fbo;
            UseShadowMapping = true;
            lightMask = Texture.Load(lightMaskFileName);
        }

        public static void BindLightMask()
        {
            lightMask.Bind(BaseGame.LIGHTMASK_TEXUNIT);
        }
        public static void UnBindLightMask()
        {
            Texture.UnBind(BaseGame.LIGHTMASK_TEXUNIT);
        }

        /// <summary>
        /// aseta kuvakulma valosta päin
        /// </summary>
        void RenderFromLight(SceneNode light)
        {
            GL.LoadMatrix(ref light.OrigOrientationMatrix);
            GL.Translate(-light.Position);
        }

        public void SetupShadows(SceneNode world, int lightNo)
        {
            if (UseShadowMapping == false) return;

            if (Light.Lights.Count == 0)
            {
                Log.WriteLine("SetupShadows requires at least one light source!", true);
                return;
            }
            fbo.BindFBO();
            GL.Clear(fbo.ClearFlags);
            GLSLShader.UseProgram(0);

            GL.Disable(EnableCap.Lighting);
            GL.Disable(EnableCap.Blend);
            GL.ShadeModel(ShadingModel.Flat);
            GL.ColorMask(false, false, false, false);
            GL.CullFace(CullFaceMode.Front);

            RenderFromLight(Light.Lights[lightNo]);
            SetTextureMatrix();
            Frustum.CalculateFrustum();

            ShadowPass = true;
            world.Render();
            ShadowPass = false;

            GL.CullFace(CullFaceMode.Back);
            GL.ColorMask(true, true, true, true);
            GL.ShadeModel(ShadingModel.Smooth);
            fbo.UnBindFBO();

            GL.Clear(fbo.ClearFlags);
            Settings.NumOfObjects = 0;
        }

        /// <summary>
        /// aseta texturematriisit shadowmapping shaderia varten
        /// </summary>
        void SetTextureMatrix()
        {
            // ota projection ja Modelview matriisit
            Matrix4 projMatrix, modelMatrix;
            GL.GetFloat(GetPName.ProjectionMatrix, out projMatrix);
            GL.GetFloat(GetPName.ModelviewMatrix, out modelMatrix);

            fbo.BindDepth();
            GL.MatrixMode(MatrixMode.Texture);
            GL.LoadIdentity();
            GL.Translate(0.5f, 0.5f, 0.5f); // remap from [-1,1]^2 to [0,1]^2
            GL.Scale(0.5f, 0.5f, 0.5f);

            GL.MultMatrix(ref projMatrix);
            GL.MultMatrix(ref modelMatrix);

            GL.MatrixMode(MatrixMode.Modelview);
        }
    }
}
