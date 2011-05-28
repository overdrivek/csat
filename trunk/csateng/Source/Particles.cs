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
    class SortedList_Particles
    {
        public float Len = 0;
        public Particle Part;
        public SortedList_Particles(float l, Particle p)
        {
            Len = l;
            Part = p;
        }
    }

    public delegate void ParticleCallback(Particle part);

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
        public ParticleCallback callBack;
    }

    public class ParticleEngine
    {
        public static float AlphaMin = 0.1f;
        List<Particles> part = new List<Particles>();

        public void Add(Particles particles, ParticleCallback particleCallback)
        {
            part.Add(particles);
            particles.callBack = particleCallback;
        }

        public void Render()
        {
            Camera cam = Camera.cam;

            List<SortedList_Particles> slist = new List<SortedList_Particles>();

            GL.Enable(EnableCap.AlphaTest);
            GL.AlphaFunc(AlphaFunction.Greater, AlphaMin);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusDstAlpha);

            // j‰rjestet‰‰n taulukko kauimmaisesta l‰himp‰‰n. pit‰‰ rendata siin‰ j‰rjestyksess‰.
            // vain l‰pikuultavat pit‰‰ j‰rjest‰‰. l‰pikuultamattomat renderoidaan samantien.
            for (int q = 0; q < part.Count; q++)
            {
                GL.PushMatrix();
                Particles curpar = part[q];
                GL.Translate(curpar.Position);

                for (int w = 0; w < curpar.NumOfParticles; w++)
                {
                    Particle p = curpar.GetParticle(w);

                    if (p.isTransparent == true) // listaan renderoitavaks myˆhemmin
                    {
                        Vector3 rp = p.pos + curpar.Position;
                        float len = (cam.Position - rp).LengthSquared;
                        slist.Add(new SortedList_Particles(len, p));
                    }
                    else // rendataan se nyt, ei lis‰t‰ sortattavaks
                    {
                        p.partTex.BillboardBegin(p.pos.X, p.pos.Y, p.pos.Z, p.size);
                        GL.Color4(p.color);
                        if (p.callBack != null) p.callBack(p);
                        p.partTex.BillboardRender();
                        p.partTex.BillboardEnd();
                    }

                }
                GL.PopMatrix();

            }
            GL.Disable(EnableCap.AlphaTest);

            slist.Sort(delegate(SortedList_Particles z1, SortedList_Particles z2) { return z2.Len.CompareTo(z1.Len); });

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One);

            // rendataan l‰pikuultavat
            GL.DepthMask(false); // ei kirjoiteta zbufferiin
            for (int q = 0; q < slist.Count; q++)
            {
                Particle p = slist[q].Part;
                GL.Color4(p.color);
                p.partTex.BillboardBegin(p.pos.X, p.pos.Y, p.pos.Z, p.size);
                if (p.callBack != null) p.callBack(p);
                p.partTex.BillboardRender();
                p.partTex.BillboardEnd();
            }
            GL.DepthMask(true);

            GL.Disable(EnableCap.Blend);
            GL.Color4(1, 1, 1, 1f);
        }

    }

    public class Particles : SceneNode
    {
        public bool IsTransparent = false;
        Billboard particleTex = null;
        public ParticleCallback callBack = null;

        List<Particle> parts = new List<Particle>();

        public Particles(string name)
        {
            Name = name;
        }

        public Particle GetParticle(int q)
        {
            return parts[q];
        }

        public int NumOfParticles
        {
            get { return parts.Count; }
        }

        float size = 10;
        public float Size
        {
            get { return size; }
            set { size = value; }
        }

        public override void Dispose()
        {
            if (particleTex != null) particleTex.Dispose();
            particleTex = null;
            parts.Clear();

            Log.WriteLine("Disposed: Particles", true);
        }

        public void AddParticle(ref Vector3 pos, ref Vector3 dir, ref Vector3 gravity, float life, float size, Vector4 color)
        {
            Particle p;
            p.pos = pos;
            p.dir = dir;
            p.gravity = gravity;
            p.life = life;
            p.size = size;
            p.partTex = particleTex;
            p.callBack = callBack;
            p.isTransparent = IsTransparent;
            p.color = color;
            this.size = size * 0.01f;
            parts.Add(p);
        }

        /// <summary>
        /// aseta partikkelikuva
        /// </summary>
        public void SetObject(Billboard tex, bool isTransparent)
        {
            this.particleTex = tex;
            this.IsTransparent = isTransparent;
        }

        /// <summary>
        /// p‰ivit‰ partikkelit
        /// </summary>
        /// <param name="time"></param>
        public void Update(float time)
        {
            for (int q = 0; q < parts.Count; q++)
            {
                Particle p = (Particle)parts[q];

                p.life -= time;
                if (p.life < 0) // kuoleeko partikkeli
                {
                    parts.RemoveAt(q); // poista se
                    continue;
                }

                p.pos += p.dir;
                p.dir += p.gravity;

                parts.RemoveAt(q);
                parts.Insert(q, p);
            }
        }

        protected override void RenderModel()
        {
            RenderMesh();
        }

        /// <summary>
        /// renderoi partikkelit. jos l‰pin‰kyvi‰, j‰rjestet‰‰n partikkelit, muuten ei.
        /// </summary>
        public override void Render()
        {
            base.Render(); // renderoi objektin ja kaikki siihen liitetyt objektit
        }

        public void RenderMesh()
        {
            if (IsTransparent) RenderSorted();
            else RenderNotSorted();
        }

        /// <summary>
        /// piirr‰ partikkelit. ei sortata eik‰ ole callbackia.
        /// </summary>
        public void RenderNotSorted()
        {
            GL.PushAttrib(AttribMask.ColorBufferBit | AttribMask.EnableBit | AttribMask.PolygonBit);

            GL.DepthMask(false);
            particleTex.Bind(0);

            GL.Disable(EnableCap.CullFace);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One);

            int i, j;
            float[] Modelview = new float[16];

            GL.PushMatrix();
            GL.Translate(Position);

            for (int q = 0; q < parts.Count; q++)
            {
                GL.PushMatrix();
                Particle p = (Particle)parts[q];
                GL.Color4(p.color);
                GL.Translate(p.pos.X, p.pos.Y, p.pos.Z);
                GL.GetFloat(GetPName.ModelviewMatrix, Modelview);

                for (i = 0; i < 3; i++)
                {
                    for (j = 0; j < 3; j++)
                    {
                        if (i == j) Modelview[i * 4 + j] = 1;
                        else Modelview[i * 4 + j] = 0;
                    }
                }

                GL.LoadMatrix(Modelview);
                GL.Scale(size, size, size);

                particleTex.BillboardRender();
                GL.PopMatrix();
            }
            GL.PopMatrix();
            GL.DepthMask(true);
            GL.PopAttrib();
            GL.Color4(1f, 1, 1, 1);
        }

        /// <summary>
        /// piirr‰ partikkelit. sortataan ja callback.
        /// </summary>
        public void RenderSorted()
        {
            Camera cam = Camera.cam;

            List<SortedList_Particles> slist = new List<SortedList_Particles>();

            GL.Enable(EnableCap.AlphaTest);
            GL.AlphaFunc(AlphaFunction.Greater, ParticleEngine.AlphaMin);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusDstAlpha);

            // j‰rjestet‰‰n taulukko kauimmaisesta l‰himp‰‰n. pit‰‰ rendata siin‰ j‰rjestyksess‰.
            // vain l‰pikuultavat pit‰‰ j‰rjest‰‰. l‰pikuultamattomat renderoidaan samantien.
            GL.PushMatrix();
            Particles curpar = this;
            GL.Translate(curpar.Position);

            for (int w = 0; w < curpar.NumOfParticles; w++)
            {
                Particle p = curpar.GetParticle(w);

                if (p.isTransparent == true) // listaan renderoitavaks myˆhemmin
                {
                    Vector3 rp = p.pos + curpar.Position;
                    float len = (cam.Position - rp).LengthSquared;
                    slist.Add(new SortedList_Particles(len, p));
                }
                else // rendataan se nyt, ei lis‰t‰ sortattavaks
                {
                    p.partTex.BillboardBegin(p.pos.X, p.pos.Y, p.pos.Z, p.size);
                    GL.Color4(p.color);
                    if (p.callBack != null) p.callBack(p);
                    p.partTex.BillboardRender();
                    p.partTex.BillboardEnd();
                }

            }
            GL.PopMatrix();

            GL.Disable(EnableCap.AlphaTest);

            slist.Sort(delegate(SortedList_Particles z1, SortedList_Particles z2) { return z2.Len.CompareTo(z1.Len); });

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One);

            // rendataan l‰pikuultavat
            GL.DepthMask(false); // ei kirjoiteta zbufferiin
            for (int q = 0; q < slist.Count; q++)
            {
                Particle p = ((SortedList_Particles)slist[q]).Part;
                GL.Color4(p.color);
                p.partTex.BillboardBegin(p.pos.X, p.pos.Y, p.pos.Z, p.size);
                if (p.callBack != null) p.callBack(p);
                p.partTex.BillboardRender();
                p.partTex.BillboardEnd();
            }
            GL.DepthMask(true);

            GL.Disable(EnableCap.Blend);
        }

    }
}
