#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2008-2012 mjt
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
    public class Light : Node
    {
        public static List<Light> Lights = new List<Light>();

        public enum LightTypes { Point, Spot, Directional };
        public LightTypes Type = LightTypes.Directional;

        public Vector4 Diffuse = new Vector4(0.8f, 0.8f, 0.8f, 1);
        public Vector4 Specular = new Vector4(0.5f, 0.5f, 0.5f, 1);
        public Vector4 Ambient = new Vector4(0.1f, 0.1f, 0.1f, 1);
        public float Shininess = 100;
        public bool UpdateColors = true;
        public bool Enabled = false;
        //public List<int> LightingUniforms;

        public Light() { }
        public Light(string name) 
        { 
            Name = name;
            Enabled = true;
        }

        /// <summary>
        /// päivitä kaikki valot
        /// (päivitä myös väriarvot jos Light.UpdateColors == true)
        /// vain gl1.5
        /// </summary>
        public static void UpdateLights()
        {
            if (GLSLShader.IsSupported == true) return;

            GL.LoadMatrix(ref GLExt.ModelViewMatrix);
            for (int q = 0; q < Lights.Count; q++)
            {
                Light light = Lights[q];
                if (light.Enabled)
                    GL.Enable(EnableCap.Light0 + q);
                else
                {
                    GL.Disable(EnableCap.Light0 + q);
                    continue;
                }

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
