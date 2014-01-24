#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2008-2014 mjt
 * This notice may not be removed from any source distribution.
 * See csat-license.txt for licensing details.
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
        }

        public static void DisableFog()
        {
            Fog.Density = 0;
        }
    }
}
