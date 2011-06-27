#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2011 mjt
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing details.
 */
#endregion
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace CSatEng
{
    public static class CallBacks
    {
        static List<int> MakeUniformLists(int programID, string[] parameters)
        {
            List<int> list = new List<int>();
            for (int q = 0; q < parameters.Length; q++)
            {
                list.Add(GL.GetUniformLocation(programID, parameters[q]));
            }
            return list;
        }

        public static void Clear()
        {
            if (particleLocs != null) particleLocs.Clear();
            if (shadowLocs != null) shadowLocs.Clear();
            if (effectLocs != null) effectLocs.Clear();
            particleLocs = shadowLocs = effectLocs = null;
        }

        static List<int> particleLocs;
        public static void ParticleShaderCallBack(int programID)
        {
            if (particleLocs == null) particleLocs = MakeUniformLists(programID, new string[] { "depthMap", "power" });
            if (particleLocs[0] != -1)
            {
                GL.Uniform1(particleLocs[0], Settings.DEPTH_TEXUNIT); // käytetään vain softpartikkeleissa
                GL.Uniform1(particleLocs[1], Particles.ParticlePower);
            }
        }

        static List<int> shadowLocs;
        public static void ShadowShaderCallBack(int programID)
        {
            if (shadowLocs == null) shadowLocs = MakeUniformLists(programID, new string[] { "depthMap", "lightmaskMap" });
            ShadowMapping.BindLightMask();
            GL.Uniform1(shadowLocs[0], Settings.SHADOW_TEXUNIT);
            GL.Uniform1(shadowLocs[1], Settings.LIGHTMASK_TEXUNIT);
        }

        static List<int> effectLocs;
        public static void EffectShaderCallBack(int programID)
        {
            if (effectLocs == null) effectLocs = MakeUniformLists(programID, new string[] { "size" });
            GL.Uniform1(effectLocs[0], PostEffect.EffParam);
        }

        static List<int> lightingLocs;
        public static void LightingShaderCallBack(int programID)
        {
            if (lightingLocs == null) lightingLocs = MakeUniformLists(programID, new string[] { 
                "materialSpecular", "materialAmbient", "lightDiffuse", "lightSpecular", "lightAmbient", "shininess" });

            if (lightingLocs[5] != -1) // shininess eli jos tämä löytyy shaderista, käytetään phongia
            {
                GL.Uniform4(lightingLocs[0], new Vector4(0.5f, 0.5f, 0.5f, 1));
                GL.Uniform4(lightingLocs[1], new Vector4(0.5f, 0.5f, 0.5f, 1));
                GL.Uniform4(lightingLocs[2], new Vector4(0.5f, 0.5f, 0.5f, 1));
                GL.Uniform4(lightingLocs[3], new Vector4(0.5f, 0.5f, 0.5f, 1));
                GL.Uniform4(lightingLocs[4], new Vector4(0.5f, 0.5f, 0.5f, 1));
                GL.Uniform1(lightingLocs[5], 100f);
            }
        }
    }
}
