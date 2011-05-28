#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2011 mjt[matola@sci.fi]
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing details.
 */
#endregion
using System;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace CSatEng
{
    public class Fog
    {
        public static Vector3 Color = new Vector3(0.3f, 0.2f, 0.5f);

        /// <summary>
        /// luo sumu
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="density"></param>
        public static void CreateFog(FogMode mode, float start, float end, float density)
        {
            GL.Fog(FogParameter.FogMode, (int)mode);

            GL.Fog(FogParameter.FogColor, new float[] { Color.X, Color.Y, Color.Z });
            GL.Fog(FogParameter.FogDensity, density);
            GL.Hint(HintTarget.FogHint, HintMode.DontCare);

            GL.Fog(FogParameter.FogStart, start);
            GL.Fog(FogParameter.FogEnd, end);
            GL.Enable(EnableCap.Fog);
        }

        public static void DisableFog()
        {
            GL.Disable(EnableCap.Fog);
        }
    }
}