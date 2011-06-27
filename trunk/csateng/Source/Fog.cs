#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2011 mjt
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
        public static Vector3 Color = new Vector3(0.3f, 0.2f, 0.5f);

        public static void CreateFog(float start, float end, float density)
        {
            if (Settings.UseGL3) return;
            GL.Enable(EnableCap.Fog);
            GL.Fog(FogParameter.FogMode, (int)FogMode.Exp2);
            GL.Fog(FogParameter.FogColor, new float[] { Color.X, Color.Y, Color.Z });
            GL.Fog(FogParameter.FogDensity, density);
            GL.Fog(FogParameter.FogStart, start);
            GL.Fog(FogParameter.FogEnd, end);
        }

        public static void DisableFog()
        {
            if (Settings.UseGL3) return;
            GL.Disable(EnableCap.Fog);
        }
    }
}
