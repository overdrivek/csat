#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2011 mjt[matola@sci.fi]
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing details.
 */
#endregion
using System.Collections.Generic;
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

        public static float ParticlePower = 1f;
        static List<int> particleLocs;
        public static void ParticleShaderCallBack(int programID)
        {
            if (particleLocs == null) particleLocs = MakeUniformLists(programID, new string[] { "textureMap", "depthMap", "power" });
            GL.Uniform1(particleLocs[0], Settings.COLOR_TEXUNIT);
            GL.Uniform1(particleLocs[1], Settings.DEPTH_TEXUNIT);
            GL.Uniform1(particleLocs[2], ParticlePower);
        }

        static List<int> shadowLocs;
        public static void ShadowShaderCallBack(int programID)
        {
            if (shadowLocs == null) shadowLocs = MakeUniformLists(programID,
                new string[] { "diffuseMap", "shadowMap", "lightmask", "lightEnergy", "ambient" });

            ShadowMapping.BindLightMask();
            GL.Uniform1(shadowLocs[0], Settings.COLOR_TEXUNIT);
            GL.Uniform1(shadowLocs[1], Settings.SHADOW_TEXUNIT);
            GL.Uniform1(shadowLocs[2], Settings.LIGHTMASK_TEXUNIT);
            GL.Uniform1(shadowLocs[3], 2f);
            GL.Uniform1(shadowLocs[4], 0.8f);
        }
    }
}
