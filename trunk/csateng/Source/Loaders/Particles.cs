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

namespace CSatEng
{
    public delegate void ParticleCallback(Particle part);

    class SortedList_Particles
    {
        public float Len = 0;
        public Particle Part;
        public Matrix4 Matrix;
        public SortedList_Particles(float l, Particle p, Matrix4 mat)
        {
            Len = l;
            Part = p;
            Matrix = mat;
        }
    }

    public struct Particle
    {
        public Billboard partTex;
        public Vector3 pos; // paikka
        public Vector3 dir; // suunta (ja nopeus)
        public Vector3 gravity;  // mihin suuntaan vedet‰‰n
        public float life; // kauanko partikkeli el‰‰
        public float size; // partikkelin koko
        public bool isTransparent; // onko l‰pin‰kyv‰ (jos on, pit‰‰ sortata)
        public Vector4 color; // v‰ri
        public float zrot; // k‰‰nnˆskulma z-akselin ymp‰ri
        public float zrotAdder; // t‰m‰ lis‰t‰‰n zrottiin updatessa
        public ParticleCallback callBack;
    }

    // n‰m‰ ladataan xml tiedostosta joiden perusteella lasketaan Particle structin arvot
    struct OrigValues
    {
        public int count;
        public float life, life_max, size, size_max;
        public float zrotation, zrotation_max, zrotation_adder, zrotation_adder_max;
        public Vector3 pos, posMax, dir, dirMax, grav;
        public Vector4 color, colorMax;
    }

    public class Particles : Renderable
    {
        public static float ParticlePower = 0.1f;
        public static List<Particles> ParticleGroups = new List<Particles>();
        public bool CastShadow = false;
        public bool IsTransparent = false;
        public ParticleCallback callBack = null;
        static GLSLShader depthShader;
        Billboard particleTex = null;
        OrigValues origValues;

        static bool softParticles = false;
        public static bool SoftParticles
        {
            get { return softParticles; }
        }
        List<Particle> particles = new List<Particle>();
        public int NumOfParticles
        {
            get { return particles.Count; }
        }

        public static void EnableSoftParticles()
        {
            if (Settings.DisableSoftParticles)
            {
                softParticles = false;
                return;
            }

            if (Texture.IsFloatTextureSupported == false || GLSLShader.IsSupported == false)
            {
                string ext;
                if (Texture.IsFloatTextureSupported == false) ext = "Float textures"; else ext = "Shaders";
                Log.WriteLine(ext + " not supported so no soft particles.");
                softParticles = false;
                return;
            }
            depthShader = GLSLShader.Load("depth.shader:DEPTH_W");
            softParticles = true;
        }

        public static void DisableSoftParticles()
        {
            softParticles = false;
        }

        public static void SetDepthProgram()
        {
            depthShader.UseProgram();
        }

        public static Particles Load(string fileName, ParticleCallback particleCallback)
        {
            Particles part = new Particles();
            part.LoadParticles(fileName, particleCallback);
            return part;
        }

        void LoadParticles(string fileName, ParticleCallback particleCallback)
        {
            XmlDocument XMLDoc = null;
            XmlElement XMLRoot;
            try
            {
                using (System.IO.StreamReader file = new System.IO.StreamReader(Settings.ParticleDir + fileName))
                {
                    // tiedosto muistiin
                    string data = file.ReadToEnd();
                    XMLDoc = new XmlDocument();
                    XMLDoc.LoadXml(data);
                }
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
            }
            // Validate the File
            XMLRoot = XMLDoc.DocumentElement;
            if (XMLRoot.Name != "particles")
            {
                Log.Error("Error [" + fileName + "] Invalid .particles.xml File. Missing <particles>");
            }

            Name = XML.GetAttrib(XMLRoot, "name");
            string billboard = XML.GetAttrib(XMLRoot, "billboard");
            bool translucent = XML.GetAttribBool(XMLRoot, "translucent", false);
            bool castshadows = XML.GetAttribBool(XMLRoot, "castshadows", false);
            origValues.count = (int)XML.GetAttribFloat(XMLRoot, "count", 0);
            origValues.life = XML.GetAttribFloat(XMLRoot, "life", 0);
            origValues.life_max = XML.GetAttribFloat(XMLRoot, "life_max", origValues.life);
            origValues.zrotation = XML.GetAttribFloat(XMLRoot, "zrotation", 0);
            origValues.zrotation_max = XML.GetAttribFloat(XMLRoot, "zrotation_max", origValues.zrotation);
            origValues.zrotation_adder = XML.GetAttribFloat(XMLRoot, "zrotation_adder", 0);
            origValues.zrotation_adder_max = XML.GetAttribFloat(XMLRoot, "zrotation_adder_max", origValues.zrotation_adder);
            origValues.size = XML.GetAttribFloat(XMLRoot, "size", 0);
            origValues.size_max = XML.GetAttribFloat(XMLRoot, "size_max", origValues.size);

            XmlElement node = (XmlElement)XMLRoot.SelectSingleNode("position");
            origValues.pos = new Vector3(XML.GetAttribFloat(node, "x", 0),
                                    XML.GetAttribFloat(node, "y", 0),
                                    XML.GetAttribFloat(node, "z", 0));
            origValues.posMax = new Vector3(XML.GetAttribFloat(node, "x_max", origValues.pos.X),
                                        XML.GetAttribFloat(node, "y_max", origValues.pos.Y),
                                        XML.GetAttribFloat(node, "z_max", origValues.pos.Z));


            node = (XmlElement)XMLRoot.SelectSingleNode("direction");
            origValues.dir = new Vector3(XML.GetAttribFloat(node, "x", 0),
                                    XML.GetAttribFloat(node, "y", 0),
                                    XML.GetAttribFloat(node, "z", 0));
            origValues.dirMax = new Vector3(XML.GetAttribFloat(node, "x_max", origValues.dir.X),
                                        XML.GetAttribFloat(node, "y_max", origValues.dir.Y),
                                        XML.GetAttribFloat(node, "z_max", origValues.dir.Z));

            node = (XmlElement)XMLRoot.SelectSingleNode("gravitation");
            origValues.grav = new Vector3(XML.GetAttribFloat(node, "x", 0),
                                    XML.GetAttribFloat(node, "y", 0),
                                    XML.GetAttribFloat(node, "z", 0));

            node = (XmlElement)XMLRoot.SelectSingleNode("color");
            origValues.color = new Vector4(XML.GetAttribFloat(node, "r", 0),
                                    XML.GetAttribFloat(node, "g", 0),
                                    XML.GetAttribFloat(node, "b", 0),
                                    XML.GetAttribFloat(node, "a", 0));
            origValues.colorMax = new Vector4(XML.GetAttribFloat(node, "r_max", origValues.color.X),
                                        XML.GetAttribFloat(node, "g_max", origValues.color.Y),
                                        XML.GetAttribFloat(node, "b_max", origValues.color.Z),
                                        XML.GetAttribFloat(node, "a_max", origValues.color.W));

            SetParticle(Billboard.Load(billboard), translucent, castshadows, particleCallback);
            Reset();
        }

        Vector3 Random(Vector3 min, Vector3 max)
        {
            Vector3 f = min;
            Vector3 r = max - min;
            f.X += r.X * (float)GameClass.Rnd.NextDouble();
            f.Y += r.Y * (float)GameClass.Rnd.NextDouble();
            f.Z += r.Z * (float)GameClass.Rnd.NextDouble();
            return f;
        }

        public void Reset()
        {
            for (int q = 0; q < origValues.count; q++)
            {
                ResetOneParticle();
            }
        }

        public void ResetOneParticle()
        {
            Vector3 pos, dir;
            Vector4 col;
            float life, zrot, zrotAdder, size;
            pos = Random(origValues.pos, origValues.posMax);
            dir = Random(origValues.dir, origValues.dirMax);
            col = new Vector4(Random(origValues.color.Xyz, origValues.colorMax.Xyz), 1);
            col.W = origValues.color.W + ((origValues.colorMax.W - origValues.color.W) * (float)GameClass.Rnd.NextDouble());
            life = origValues.life + ((origValues.life_max - origValues.life) * (float)GameClass.Rnd.NextDouble());
            size = origValues.size + ((origValues.size_max - origValues.size) * (float)GameClass.Rnd.NextDouble());
            zrot = origValues.zrotation + ((origValues.zrotation_max - origValues.zrotation) * (float)GameClass.Rnd.NextDouble());
            zrotAdder = origValues.zrotation_adder + ((origValues.zrotation_adder_max - origValues.zrotation_adder) * (float)GameClass.Rnd.NextDouble());
            AddParticle(ref pos, ref dir, ref origValues.grav, life, zrot, zrotAdder, size, col);
        }

        public static void DisposeAll()
        {
            foreach (Particles p in ParticleGroups)
                p.Dispose();
        }
        public override void Dispose()
        {
            if (particleTex != null) particleTex.Dispose();
            if (depthShader != null) depthShader.Dispose();
            particleTex = null;
            depthShader = null;

            particles.Clear();
            Log.WriteLine("Disposed: Particles", false);
        }

        public void AddParticle(ref Vector3 pos, ref Vector3 dir, ref Vector3 gravity, float life, float zrot, float zrotAdder, float size, Vector4 color)
        {
            Particle p;
            p.pos = pos;
            p.dir = dir;
            p.gravity = gravity;
            p.life = life;
            p.size = size * 0.1f;
            p.partTex = particleTex;
            p.callBack = callBack;
            p.isTransparent = IsTransparent;
            p.color = color;
            p.zrot = zrot;
            p.zrotAdder = zrotAdder;
            particles.Add(p);
        }

        /// <summary>
        /// aseta partikkelikuva ja callback-metodi
        /// </summary>
        public void SetParticle(Billboard tex, bool isTransparent, bool castShadow, ParticleCallback particleCallback)
        {
            this.particleTex = tex;
            this.IsTransparent = isTransparent;
            this.callBack = particleCallback;
            this.CastShadow = castShadow;
            ParticleGroups.Add(this);
        }

        /// <summary>
        /// p‰ivit‰ partikkelit
        /// </summary>
        /// <param name="time"></param>
        public void Update(float time)
        {
            float utime = time * 50;
            for (int q = 0; q < particles.Count; q++)
            {
                Particle p = (Particle)particles[q];

                p.life -= time;
                if (p.life < 0) // kuoleeko partikkeli
                {
                    particles.RemoveAt(q); // poista se
                    continue;
                }
                p.pos += p.dir * utime;
                p.dir += p.gravity * utime;
                p.zrot += p.zrotAdder * utime;

                particles.RemoveAt(q);
                particles.Insert(q, p);
            }
        }

        /// <summary>
        /// renderoi partikkelit, sorttaa l‰pin‰kyv‰t.
        /// </summary>
        public static new void Render()
        {
            GLExt.Color4(1f, 1, 1, 1f);
            GLExt.PushMatrix();
            GLExt.SetLighting(false);
            
            List<SortedList_Particles> slist = new List<SortedList_Particles>();

            GL.Disable(EnableCap.CullFace);

            if (GLSLShader.IsSupported == false)
            {
                GL.Enable(EnableCap.AlphaTest);
                GL.AlphaFunc(AlphaFunction.Greater, 0.1f);
            }
            
            int c = 0;
            // j‰rjestet‰‰n taulukko kauimmaisesta l‰himp‰‰n. pit‰‰ rendata siin‰ j‰rjestyksess‰.
            // vain l‰pikuultavat pit‰‰ j‰rjest‰‰. t‰ysin n‰kyv‰t renderoidaan samantien.
            for (int q = 0; q < ParticleGroups.Count; q++)
            {
                Particles curpar = ParticleGroups[q];
                if (curpar.particles.Count <= 0) continue;
                if (VBO.FastRenderPass == true)
                {
                    if (curpar.CastShadow == false) continue;
                }

                curpar.particles[0].partTex.Bind(0);

                GLExt.PushMatrix();
                GLExt.MultMatrix(ref curpar.WorldMatrix);

                for (int w = 0; w < curpar.NumOfParticles; w++)
                {
                    Particle p = curpar.particles[w];
                    GLExt.PushMatrix();
                    GLExt.Translate(p.pos.X, p.pos.Y, p.pos.Z);
                    Matrix4 matrix = Matrix4.Identity;
                    matrix.Row3 = GLExt.ModelViewMatrix.Row3;
                    GLExt.ModelViewMatrix = matrix;

                    Vector3 v = curpar.WorldMatrix.Row3.Xyz + curpar.Position + p.pos;
                    if (Frustum.SphereInFrustum(v.X, v.Y, v.Z, 10) != 0)
                    {
                        if (VBO.FastRenderPass == true) // renderoi partikkeli depthbufferiin (varjostusta varten)
                        {
                            GLExt.Scale(p.size, p.size, p.size);
                            GLExt.RotateZ(p.zrot);
                            p.partTex.RenderBillboard();
                        }
                        else
                        {
                            c++;
                            if (p.isTransparent == true) // listaan renderoitavaks myˆhemmin
                            {
                                float len = (Camera.cam.Position - matrix.Row3.Xyz).LengthSquared;
                                slist.Add(new SortedList_Particles(len, p, matrix));
                            }
                            else // rendataan se nyt, ei lis‰t‰ sortattavaks
                            {
                                GLExt.Scale(p.size, p.size, p.size);
                                GLExt.RotateZ(p.zrot);
                                GLExt.Color4(p.color.X, p.color.Y, p.color.Z, p.color.W); ;
                                if (p.callBack != null) p.callBack(p);
                                p.partTex.RenderBillboard();
                            }
                        }
                    }
                    GLExt.PopMatrix();
                }
                GLExt.PopMatrix();
            }

            if (GLSLShader.IsSupported == false) GL.Disable(EnableCap.AlphaTest);

            if (VBO.FastRenderPass == false)
            {
                slist.Sort(delegate(SortedList_Particles z1, SortedList_Particles z2) { return z2.Len.CompareTo(z1.Len); });
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One);

                // rendataan l‰pikuultavat
                GL.DepthMask(false); // ei kirjoiteta zbufferiin
                for (int q = 0; q < slist.Count; q++)
                {
                    Particle p = slist[q].Part;
                    if (VBO.FastRenderPass == false)
                    {
                        p.partTex.Bind(0);
                        GLExt.Color4(p.color.X, p.color.Y, p.color.Z, p.color.W);
                        if (p.callBack != null) p.callBack(p);
                    }
                    GLExt.LoadMatrix(ref slist[q].Matrix);
                    GLExt.Scale(p.size, p.size, p.size);
                    GLExt.RotateZ(p.zrot);
                    p.partTex.RenderBillboard();
                }
                GL.DepthMask(true);
                GL.Disable(EnableCap.Blend);
            }
            GLExt.PopMatrix();

            GL.Enable(EnableCap.CullFace);
            GLExt.Color4(1, 1, 1, 1);
            GLExt.SetLighting(true);
            GameClass.NumOfObjects += c;
        }

    }
}
