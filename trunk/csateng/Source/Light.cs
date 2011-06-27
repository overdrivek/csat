#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2011 mjt
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing details.
 */
#endregion
using System;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using System.Collections.Generic;

namespace CSatEng
{
    public class Light : SceneNode
    {
        public static List<Light> Lights = new List<Light>();

        public enum LightTypes { Point, Spot, Directional };
        public LightTypes Type = LightTypes.Directional;

        public Vector3 Diffuse = new Vector3(0.8f, 0.8f, 0.8f);
        public Vector3 Specular = new Vector3(0.5f, 0.5f, 0.5f);
        public Vector3 Ambient = new Vector3(0.1f, 0.1f, 0.1f);
        public bool UpdateColors = true;

        public Light() { }
        public Light(string name) { Name = name; }

        /// <summary>
        /// päivitä kaikki valot
        /// (päivitä myös väriarvot jos Light.UpdateColors == true)
        /// </summary>
        public static void UpdateLights()
        {
            if (Settings.UseGL3) return;

            GL.LoadMatrix(ref GLExt.ModelViewMatrix);
            for (int q = 0; q < Lights.Count; q++)
            {
                Light light = Lights[q];
                GL.Enable(EnableCap.Light0 + q);
                GL.Light(LightName.Light0 + q, LightParameter.Position, new float[] { light.Position.X, light.Position.Y, light.Position.Z, 1 });
                if (light.UpdateColors == true)
                {
                    GL.Light(LightName.Light0 + q, LightParameter.Ambient, new float[] { light.Ambient.X, light.Ambient.Y, light.Ambient.Z });
                    GL.Light(LightName.Light0 + q, LightParameter.Diffuse, new float[] { light.Diffuse.X, light.Diffuse.Y, light.Diffuse.Z });
                    GL.Light(LightName.Light0 + q, LightParameter.Specular, new float[] { light.Specular.X, light.Specular.Y, light.Specular.Z });
                    light.UpdateColors = false;
                }
            }
        }
    }
}
