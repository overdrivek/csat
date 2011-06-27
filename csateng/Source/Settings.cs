#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2011 mjt
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing details.
 */
#endregion
using System.Xml;
using OpenTK;

namespace CSatEng
{
    public static class Settings
    {
        public static readonly int COLOR_TEXUNIT = 0;
        public static readonly int LIGHTMAP_TEXUNIT = 1;
        public static readonly int BUMP_TEXUNIT = 2;
        public static readonly int LIGHTMASK_TEXUNIT = 6;
        public static readonly int SHADOW_TEXUNIT = 7;
        public static readonly int DEPTH_TEXUNIT = SHADOW_TEXUNIT;

        public static string ModelDir = "../../data/model/";
        public static string TextureDir = "../../data/texture/";
        public static string ShaderDir = "../../data/shader/";

        public static DisplayDevice Device;
        public static int Width, Height, Bpp, FSAA, DepthBpp;
        public static bool FullScreen;
        public static bool VSync;
        public static bool UseGL3;

        public static bool DisableFbo, DisableShadowMapping, DisableSoftParticles, DisableNPOTTextures, DisableShaders, DisableFloatTextures;

        public static void ReadXML(string fileName)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(fileName);

            XmlNode resolution = doc.SelectSingleNode("//settings/resolution/text()");
            XmlNode fsaa = doc.SelectSingleNode("//settings/fsaa/text()");
            XmlNode fullscreen = doc.SelectSingleNode("//settings/fullscreen/text()");
            XmlNode vsync = doc.SelectSingleNode("//settings/vsync/text()");

            string[] res = resolution.Value.Split('x');
            Width = int.Parse(res[0]);
            Height = int.Parse(res[1]);
            Bpp = int.Parse(res[2]);

            FSAA = int.Parse(fsaa.Value);
            FullScreen = fullscreen.Value == "true";
            VSync = vsync.Value == "true";

            XmlNode depth = doc.SelectSingleNode("//settings/depth/text()");
            DepthBpp = int.Parse(depth.Value);

            XmlNode mipmaps = doc.SelectSingleNode("//settings/mipmaps/text()");
            TextureLoaderParameters.BuildMipmapsForUncompressed = mipmaps.Value == "true";

            XmlNode fbores = doc.SelectSingleNode("//settings/fbo_size/text()");
            res = fbores.Value.Split('x');
            FBO.WidthS = int.Parse(res[0]);
            FBO.HeightS = int.Parse(res[1]);

            XmlNode dis = doc.SelectSingleNode("//settings/use_gl3/text()");
            UseGL3 = dis.Value == "true";

            dis = doc.SelectSingleNode("//settings/disable_fbo/text()");
            DisableFbo = dis.Value == "true";

            dis = doc.SelectSingleNode("//settings/disable_shadowmapping/text()");
            DisableShadowMapping = dis.Value == "true";

            dis = doc.SelectSingleNode("//settings/disable_softparticles/text()");
            DisableSoftParticles = dis.Value == "true";

            dis = doc.SelectSingleNode("//settings/disable_npot_textures/text()");
            DisableNPOTTextures = dis.Value == "true";

            dis = doc.SelectSingleNode("//settings/disable_shaders/text()");
            DisableShaders = dis.Value == "true";

            dis = doc.SelectSingleNode("//settings/disable_float_textures/text()");
            DisableFloatTextures = dis.Value == "true";
        }
    }
}
