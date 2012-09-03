#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2008-2012 mjt
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing details.
 */
#endregion
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace CSatEng
{
    public static class Fog
    {
        public static Vector3 Color = new Vector3(0.0f, 0.0f, 0.4f);
        public static float Density = 0.01f;

        public static void CreateFog(float density, Vector3 color)
        {
            Fog.Color = color;
            Fog.Density = density;
            if (GLSLShader.IsSupported == true) return;
            GL.Enable(EnableCap.Fog);
            GL.Fog(FogParameter.FogMode, (int)FogMode.Exp2);
            GL.Fog(FogParameter.FogColor, new float[] { Color.X, Color.Y, Color.Z });
            GL.Fog(FogParameter.FogDensity, density);
        }

        public static void DisableFog()
        {
            Fog.Density = 0;
            if (GLSLShader.IsSupported == true) return;
            GL.Disable(EnableCap.Fog);
        }
    }
}
