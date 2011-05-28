#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2011 mjt[matola@sci.fi]
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing details.
 */
#endregion
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using OpenTK;


namespace CSatEng
{
    public class MaterialInfo
    {
        static Dictionary<string, MaterialInfo> materials = new Dictionary<string, MaterialInfo>();

        string materialName = "";
        static string curMaterial = "No material";

        /// <summary>
        /// glsl ohjelman nimi. objektille voidaan asettaa shader editoimalla materiaalitiedostoa
        /// ja lisäämällä   shader shaderinnimi.shader
        /// </summary>
        public string ShaderName = "";

        /*
        /// texturet
         */
        public Texture DiffuseTex = null; // texunit 0
        public Texture LightmapTex = null;   // texunit 1
        public Texture BumpTex = null;    // texunit 2

        /*
        /// väriarvot
         */
        public Vector4 DiffuseColor = new Vector4(0.5f, 0.5f, 0.5f, 1); // Diffuse color
        public Vector4 AmbientColor = new Vector4(0.1f, 0.1f, 0.1f, 1); // Ambient color
        public Vector4 SpecularColor = new Vector4(0.5f, 0.5f, 0.5f, 1); // Specular color
        public Vector4 EmissionColor = new Vector4(0.1f, 0.1f, 0.1f, 1);

        public MaterialInfo() { }
        public MaterialInfo(string fileName)
        {
            LoadMaterial(fileName);
        }

        /// <summary>
        /// luo uusi materiaali.
        /// jos materiaaliName niminen materiaali on luotu jo, palauta se.
        /// </summary>
        /// <param name="materialName"></param>
        /// <returns></returns>
        static MaterialInfo CreateMaterial(string materialName)
        {
            if (materials.ContainsKey(materialName))
            {
                return materials[materialName];
            }

            MaterialInfo mat = new MaterialInfo();
            mat.materialName = materialName;
            materials.Add(materialName, mat);

            return mat;
        }

        public void Dispose()
        {
            if (materialName != "")
            {
                Log.WriteLine("Disposed: " + materialName, true);
                materials.Remove(materialName);
                materialName = "";
                if (DiffuseTex != null) DiffuseTex.Dispose();
                if (LightmapTex != null) LightmapTex.Dispose();
                if (BumpTex != null) BumpTex.Dispose();
            }
        }
        public static void DisposeAll()
        {
            List<string> mat = new List<string>();
            foreach (KeyValuePair<string, MaterialInfo> dta in materials) mat.Add(dta.Key);
            for (int q = 0; q < mat.Count; q++) materials[mat[q]].Dispose();
            materials.Clear();
        }

        /// <summary>
        /// lataa materiaalitiedot .material tiedostosta (ogre materiaali)
        /// </summary>
        public static MaterialInfo Load(string fileName)
        {
            MaterialInfo mat = new MaterialInfo(fileName);
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

                MaterialInfo mat = new MaterialInfo();
                int tex = 0;

                for (int q = 0; q < lines.Length; q++)
                {
                    string line = lines[q];
                    line = line.Trim('\r', '\t', ' ');
                    if (line.StartsWith("//")) continue;
                    string[] ln = line.Split(' '); // pilko datat

                    if (ln[0] == "material")
                    {
                        tex = 0;
                        mat = MaterialInfo.CreateMaterial(ln[1]);
                        Log.WriteLine("MaterialName: " + mat.materialName, true);
                        continue;
                    }

                    if (ln[0] == "shader")
                    {
                        mat.ShaderName = ln[1]; // ota shaderin nimi
                    }

                    // Diffuse color texture map
                    if (ln[0] == "texture" && tex == 0)
                    {
                        tex++;
                        if (ln[1] == "none") continue;
                        mat.DiffuseTex = Texture.Load(ln[1].ToLower());
                        continue;
                    }

                    // Ambient color texture map (lightmap)
                    if (ln[0] == "texture" && tex == 1)
                    {
                        tex++;
                        if (ln[1] == "none") continue;
                        mat.LightmapTex = Texture.Load(ln[1].ToLower());
                        continue;
                    }
                    // Bump color texture map
                    if (ln[0] == "texture" && tex == 2)
                    {
                        tex++;
                        if (ln[1] == "none") continue;
                        mat.BumpTex = Texture.Load(ln[1].ToLower());
                        continue;
                    }

                    // Ambient color
                    if (ln[0] == "ambient")
                    {
                        mat.AmbientColor = new Vector4(Util.GetFloat(ln[1]), Util.GetFloat(ln[2]), Util.GetFloat(ln[3]), 1);
                        continue;
                    }
                    // Diffuse color
                    if (ln[0] == "diffuse")
                    {
                        mat.DiffuseColor = new Vector4(Util.GetFloat(ln[1]), Util.GetFloat(ln[2]), Util.GetFloat(ln[3]), 1);
                        continue;
                    }
                    // Specular color
                    if (ln[0] == "specular")
                    {
                        mat.SpecularColor = new Vector4(Util.GetFloat(ln[1]), Util.GetFloat(ln[2]), Util.GetFloat(ln[3]), Util.GetFloat(ln[4]));
                        continue;
                    }
                    if (ln[0] == "emissive")
                    {
                        mat.EmissionColor = new Vector4(Util.GetFloat(ln[1]), Util.GetFloat(ln[2]), Util.GetFloat(ln[3]), 1);
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
            curMaterial = "";
            SetMaterial();
        }

        /// <summary>
        /// aseta materiaali ellei jo käytössä
        /// </summary>
        public void SetMaterial()
        {
            if (curMaterial == materialName) return;
            curMaterial = materialName;

            if (DiffuseTex != null) DiffuseTex.Bind(BaseGame.DIFFUSE_TEXUNIT);
            if (LightmapTex != null) LightmapTex.Bind(BaseGame.LIGHTMAP_TEXUNIT);
            if (BumpTex != null) BumpTex.Bind(BaseGame.BUMP_TEXUNIT);

            GL.Material(MaterialFace.Front, MaterialParameter.Ambient, AmbientColor);
            GL.Material(MaterialFace.Front, MaterialParameter.Diffuse, DiffuseColor);
            GL.Material(MaterialFace.Front, MaterialParameter.Specular, SpecularColor);
            GL.Material(MaterialFace.Front, MaterialParameter.Emission, EmissionColor);
        }

        public static void SetMaterial(string materialName)
        {
            MaterialInfo mat = materials[materialName];
            if (mat != null)
            {
                mat.SetMaterial();
            }
        }
        public static MaterialInfo GetMaterial(string materialName)
        {
            return MaterialInfo.CreateMaterial(materialName);
        }

    }
}
