#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2008-2012 mjt
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing details.
 */
#endregion
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using OpenTK;


namespace CSatEng
{
    public struct TextureInfo
    {
        public enum EnvMaps { None, Spherical, CubicReflection };
        public Texture Tex;
        public uint TexCoordSet;
        public EnvMaps EnvMap;
    }

    public class Material
    {
        static int MaxTextures = Texture.MaxTextures;
        
        static Dictionary<string, Material> materials = new Dictionary<string, Material>();

        string materialName = "";
        static string currentMaterialName = "No material";
        public static Material CurrentMaterial = null;

        /// <summary>
        /// glsl ohjelman nimi. objektille voidaan asettaa shader editoimalla materiaalitiedostoa
        /// ja lisäämällä   shader shaderinnimi.shader
        /// </summary>
        public string ShaderName = "";

        public TextureInfo[] Textures = new TextureInfo[MaxTextures];

        /*
        /// väriarvot
         */
        public Vector4 DiffuseColor = new Vector4(0.5f, 0.5f, 0.5f, 1); // Diffuse color
        public Vector4 AmbientColor = new Vector4(0.1f, 0.1f, 0.1f, 1); // Ambient color
        public Vector4 SpecularColor = new Vector4(0.5f, 0.5f, 0.5f, 1); // Specular color

        public Material() { }
        public Material(string fileName)
        {
            LoadMaterial(fileName);
        }

        /// <summary>
        /// luo uusi materiaali.
        /// jos materiaaliName niminen materiaali on luotu jo, palauta se.
        /// </summary>
        /// <param name="materialName"></param>
        /// <returns></returns>
        static Material CreateMaterial(string materialName)
        {
            if (materials.ContainsKey(materialName))
            {
                return materials[materialName];
            }
            Material mat = new Material();
            mat.materialName = materialName;
            materials.Add(materialName, mat);
            return mat;
        }

        public void Dispose()
        {
            if (materialName != "")
            {
                Log.WriteLine("Disposed: " + materialName, false);
                materials.Remove(materialName);
                materialName = "";
                for (int q = 0; q < MaxTextures; q++)
                {
                    if (Textures[q].Tex != null)
                        Textures[q].Tex.Dispose();
                }
            }
        }
        public static void DisposeAll()
        {
            List<string> mat = new List<string>();
            foreach (KeyValuePair<string, Material> dta in materials) mat.Add(dta.Key);
            for (int q = 0; q < mat.Count; q++) materials[mat[q]].Dispose();
            materials.Clear();
        }

        /// <summary>
        /// lataa materiaalitiedot .material tiedostosta (ogre materiaali)
        /// </summary>
        public static Material Load(string fileName)
        {
            Material mat = new Material(fileName);
            return mat;
        }

        void LoadMaterial(string fileName)
        {
            using (System.IO.StreamReader file = new System.IO.StreamReader(Settings.ModelDir + fileName))
            {
                // tiedosto muistiin
                string data = file.ReadToEnd();

                // pilko se
                string[] lines = data.Split('\n');

                Material mat = new Material();
                int curTexture = -1;

                for (int q = 0; q < lines.Length; q++)
                {
                    string line = lines[q];
                    line = line.Trim('\r', '\t', ' ');
                    if (line.StartsWith("//")) continue;
                    string[] ln = line.Split(' '); // pilko datat

                    if (ln[0] == "material")
                    {
                        curTexture = -1;
                        mat = Material.CreateMaterial(ln[1]);
                        Log.WriteLine("Material: " + mat.materialName, false);
                        continue;
                    }

                    if (ln[0] == "shader")
                    {
                        mat.ShaderName = ln[1]; // ota shaderin nimi
                        continue;
                    }

                    // lataa texture
                    if (ln[0] == "texture")
                    {
                        curTexture++;
                        if (ln[1] == "none") continue;
                        mat.Textures[curTexture].Tex = Texture.Load(ln[1]);
                        continue;
                    }

                    if (ln[0] == "tex_coord_set")
                    {
                        mat.Textures[curTexture].TexCoordSet = uint.Parse(ln[1]);
                        continue;
                    }

                    if (ln[0] == "env_map")
                    {
                        if (ln[1] == "spherical")
                            mat.Textures[curTexture].EnvMap = TextureInfo.EnvMaps.Spherical;
                        else if (ln[1] == "cubic_reflection")
                            mat.Textures[curTexture].EnvMap = TextureInfo.EnvMaps.CubicReflection;
                        else
                            mat.Textures[curTexture].EnvMap = TextureInfo.EnvMaps.None;
                        continue;
                    }

                    // Ambient color
                    if (ln[0] == "ambient")
                    {
                        mat.AmbientColor = new Vector4(MathExt.GetFloat(ln[1]), MathExt.GetFloat(ln[2]), MathExt.GetFloat(ln[3]), 1);
                        continue;
                    }
                    // Diffuse color
                    if (ln[0] == "diffuse")
                    {
                        mat.DiffuseColor = new Vector4(MathExt.GetFloat(ln[1]), MathExt.GetFloat(ln[2]), MathExt.GetFloat(ln[3]), 1);
                        continue;
                    }
                    // Specular color
                    if (ln[0] == "specular")
                    {
                        mat.SpecularColor = new Vector4(MathExt.GetFloat(ln[1]), MathExt.GetFloat(ln[2]), MathExt.GetFloat(ln[3]), MathExt.GetFloat(ln[4]));
                        continue;
                    }
                }
            }
        }

        /// <summary>
        /// jos materiaalin tietoja muuttaa, ja se on käytössä, tällä saa uudet asetukset käyttöön.
        /// </summary>
        public void ForceSetMaterial()
        {
            currentMaterialName = "";
            SetMaterial();
        }

        /// <summary>
        /// aseta materiaali ellei jo käytössä
        /// </summary>
        public void SetMaterial()
        {
            if (currentMaterialName == materialName) return;
            currentMaterialName = materialName;
            for (int q = 0; q < MaxTextures; q++)
                if (Textures[q].Tex != null)
                    Textures[q].Tex.Bind(q);
            
            CurrentMaterial = this;

            if (GLSLShader.IsSupported == false)
            {
                GL.Material(MaterialFace.Front, MaterialParameter.Ambient, AmbientColor);
                GL.Material(MaterialFace.Front, MaterialParameter.Diffuse, DiffuseColor);
                GL.Material(MaterialFace.Front, MaterialParameter.Specular, SpecularColor);
            }
        }

        public static void SetMaterial(string materialName)
        {
            Material mat = materials[materialName];
            if (mat != null)
            {
                mat.SetMaterial();
            }
        }
        public static Material GetMaterial(string materialName)
        {
            return Material.CreateMaterial(materialName);
        }

    }
}
