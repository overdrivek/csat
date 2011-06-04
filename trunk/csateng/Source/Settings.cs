#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2011 mjt[matola@sci.fi]
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
        public static string ModelDir = "../../data/model/";
        public static string TextureDir = "../../data/texture/";
        public static string ShaderDir = "../../data/shader/";

        public static DisplayDevice Device;
        public static int Width, Height, Bpp, FSAA, DepthBpp;
        public static bool FullScreen;
        public static bool VSync;

        public static int NumOfObjects = 0;

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
        }
    }
}
