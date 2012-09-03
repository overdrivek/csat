#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2008-2012 mjt
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing details.
 */
#endregion
using System;
using System.Collections.Generic;
using System.Xml;
using OpenTK;
using OpenTK.Graphics.OpenGL;

/*
 * lataa .obj tiedostosta 2d dataa
 */

namespace CSatEng
{
    public class ObjData
    {
        public List<ObjMesh> Meshes = new List<ObjMesh>();

        public ObjData() { }
        public ObjData(string fileName)
        {
            LoadMesh(fileName);
        }

        public static ObjData Load(string fileName)
        {
            ObjData mesh;
            mesh = new ObjData(fileName);
            return mesh;
        }

        void LoadMesh(string fileName)
        {
            string data = null;
            try
            {
                using (System.IO.StreamReader file = new System.IO.StreamReader(Settings.ModelDir + fileName))
                {
                    // tiedosto muistiin
                    data = file.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
            }

            //
            string[] lines = data.Split('\n');

            int line = 0;
            while (true)
            {
                // new mesh
                if (lines[line].StartsWith("o"))
                {
                    ObjMesh mesh = new ObjMesh();
                    mesh.Name = lines[line].Split(' ')[1];

                    while (true)
                    {
                        if (lines[line + 1].StartsWith("v ") == false)
                            break;

                        string[] vert = lines[line + 1].Split(' ');
                        // [-1,1] -> [screenwidth, screenheight]
                        float x = ((MathExt.GetFloat(vert[1]) + 1) * 0.5f) * Settings.Width;
                        float y = MathExt.GetFloat(vert[2]); // not needed
                        float z = Settings.Height - (((MathExt.GetFloat(vert[3]) + 1) * 0.5f) * Settings.Height);
                        mesh.Vertices.Add(new Vector3(x, y, z));
                        line++;
                    }

                    // seuraava mesh
                    if (lines[line + 1].StartsWith("o "))
                    {
                        Meshes.Add(mesh);
                        continue;
                    }

                    while (lines[line + 1].StartsWith("usemtl ") == false)
                        line++;


                    string[] mat = lines[line + 1].Split(' ');
                    if (mat[1].StartsWith("_"))
                    {

                        mesh.UseMat = mat[1].Substring(1);
                    }
                    Meshes.Add(mesh);

                    // luo texture jos meshill‰ on materiaali
                    if (mesh.UseMat != "")
                    {
                        mesh.Tex = Texture2D.Load(Settings.TextureDir + mesh.UseMat);

                        // lev ja kor
                        float w = mesh.Vertices[0].X - mesh.Vertices[1].X;
                        float h = mesh.Vertices[0].Z - mesh.Vertices[2].Z;

                        // tuo on se koko mill‰ se pit‰‰ piirt‰‰ esim 20x20
                        // mutta jos oikea kuva on 100x100, se pit‰‰ skaalata viidesosaks eli 0.2
                        // eli  20/100 = 0.2 eli  w/img.w

                        mesh.ScaleX = w / (float)mesh.Tex.Width;
                        mesh.ScaleY = h / (float)mesh.Tex.Height;
                    }

                    if (mesh.Name.Contains("START"))
                    {
                        SX = mesh.Vertices[0].X;
                        SY = mesh.Vertices[0].Z;
                    }

                }

                line++;
                if (line == lines.Length)
                    break;
            }
        }

        public float SX, SY;

        public void Draw()
        {
            foreach (ObjMesh o in Meshes)
            {
                if (o.Tex != null)
                {
                    if (o.Name.Contains("BG"))
                        o.Tex.DrawFullScreen(0, 0);
                    else
                    {
                        o.Tex.Draw((int)o.Vertices[3].X, (int)o.Vertices[3].Z,
                            0.1f,  // z
                            0, o.ScaleX, o.ScaleY, true);
                    }
                }
            }
        }


        /// <summary>
        /// tarkista onko xy kohta polygonin sis‰ll‰.
        /// http://local.wasp.uwa.edu.au/~pbourke/geometry/insidepoly/
        /// </summary>
        public bool PointInPolygon(int x, int y)
        {
            bool c = false;
            foreach (ObjMesh o in Meshes)
            {
                int i, j;
                c = false;
                for (i = 0, j = o.Vertices.Count - 1; i < o.Vertices.Count; j = i++)
                {
                    Vector3 v1 = o.Vertices[i];
                    Vector3 v2 = o.Vertices[j];

                    if ((((v1.Z <= y) && (y < v2.Z)) || ((v2.Z <= y) && (y < v1.Z))) &&
                          (x < (v2.X - v1.X) * (y - v1.Z) / (v2.Z - v1.Z) + v1.X))
                        c = !c;
                }

                if (c)
                {
                    if (o.Name.Contains("BLOCK"))
                        continue;
                    if (o.Name.Contains("BG"))
                        continue;
                    if (o.Name.Contains("GOTO"))
                        continue;

                    string txt = o.Name.Substring(0, o.Name.IndexOf('_'));


                    Console.WriteLine("found:  " + txt);

// TODO

                }

            }

            return c;
        }

    }

    public class ObjMesh
    {
        public string Name;
        public List<Vector3> Vertices = new List<Vector3>();
        public string UseMat = "";
        public Texture2D Tex;
        public float ScaleX, ScaleY;
    }
}
