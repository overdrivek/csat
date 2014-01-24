#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2008-2014 mjt
 * This notice may not be removed from any source distribution.
 * See csat-license.txt for licensing details.
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

        public Light() { }
        public Light(string name) 
        { 
            Name = name;
            Enabled = true;
        }

    }
}
