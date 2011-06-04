#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2011 mjt[matola@sci.fi]
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing details.
 */
#endregion
/*
Yksinkertainen .particle.xml formaatti:
* annetaan vakioarvo ja jos annetaan _max arvo, randomilla luku vakio->max v‰lilt‰.

<particles>
        <particle name="smoke" billboard="smoke.png" translucent="true" count="100" life="10" life_max="100" zrotation="0", zrotation_max="360"
          zrotation_adder="0.5" zrotation_adder_max="1" size="50" size_max="200">
                <position x="1" y="1" z="1" x_max="10" y_max="100" z_max="10">
                <direction x="0" y="1" z="0" x_max="0" y_max="2" z_max="0">
                <gravitation x="0" y="-1" z="0">
                <color r="0" g="0" b="0" a="0" r_max="1" g_max="1" b_max="1" a_max="1">
        </particle>
</particles>
*/
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
        public float zrot; // k‰‰nnˆskulma z-akselin ymp‰ri
        public float zrotAdder; // t‰m‰ lis‰t‰‰n zrottiin updatessa
        public ParticleCallback callBack;
    }

    public class Particles : SceneNode
    {
        public static List<Particles> ParticleGroups = new List<Particles>();
        public static float AlphaMin = 0.1f;
        
        /// <summary>
        /// jos t‰m‰ true, particles.shader ladataan kun luodaan partikkelit.
        /// softparticles vaatii fbo:n & depth texturen jotka pit‰‰ bindata
        /// ennen skenen renderointia. sen j‰lkeen renderoidaan partikkelit.
        /// </summary>
        public static bool SoftParticles = false;

        public bool IsTransparent = false;
        Billboard particleTex = null;
        public ParticleCallback callBack = null;
        public float Size = 10;

        List<Particle> particles = new List<Particle>();
        static GLSLShader softParticles;

        public Particles()
        {
            //if (softParticles == null) softParticles = GLSLShader.Load("particles.shader");
        }

        /// <summary>
        ///  TODO
        /// </summary>
        public static Particles Load(string particleFileName)
        {
            Particles part = new Particles();



            // TODO

            part.Name = particleFileName;
            return part;
        }

        public Particle GetParticle(int q)
        {
            return particles[q];
        }

        public int NumOfParticles
        {
            get { return particles.Count; }
        }

        public override void Dispose()
        {
            if (particleTex != null) particleTex.Dispose();
            if (softParticles != null) softParticles.Dispose();
            particleTex = null;
            softParticles = null;
            particles.Clear();

            Log.WriteLine("Disposed: Particles", true);
        }

        public void AddParticle(ref Vector3 pos, ref Vector3 dir, ref Vector3 gravity, float life, float zrot, float zrotAdder, float size, Vector4 color)
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
            p.zrot = zrot;
            p.zrotAdder = zrotAdder;
            this.Size = size * 0.01f;
            particles.Add(p);
        }

        /// <summary>
        /// aseta partikkelikuva ja callback-metodi
        /// </summary>
        public void SetParticle(Billboard tex, bool isTransparent, ParticleCallback particleCallback)
        {
            this.particleTex = tex;
            this.IsTransparent = isTransparent;
            this.callBack = particleCallback;
            ParticleGroups.Add(this);
        }

        /// <summary>
        /// p‰ivit‰ partikkelit
        /// </summary>
        /// <param name="time"></param>
        public void Update(float time)
        {
            for (int q = 0; q < particles.Count; q++)
            {
                Particle p = (Particle)particles[q];

                p.life -= time;
                if (p.life < 0) // kuoleeko partikkeli
                {
                    particles.RemoveAt(q); // poista se
                    continue;
                }
                p.pos += p.dir;
                p.dir += p.gravity;
                p.zrot += p.zrotAdder;

                particles.RemoveAt(q);
                particles.Insert(q, p);
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
            RenderGroups();
        }

        /// <summary>
        /// renderoi partikkelit, sorttaa l‰pin‰kyv‰t.
        /// </summary>
        public void RenderGroups()
        {
            List<SortedList_Particles> slist = new List<SortedList_Particles>();
            if (SoftParticles == true && softParticles != null)
            {
                softParticles.UseProgram();
            }
            GL.PushAttrib(AttribMask.AllAttribBits);
            GL.Disable(EnableCap.Lighting);
            GL.Enable(EnableCap.AlphaTest);
            GL.AlphaFunc(AlphaFunction.Greater, AlphaMin);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusDstAlpha);

            // j‰rjestet‰‰n taulukko kauimmaisesta l‰himp‰‰n. pit‰‰ rendata siin‰ j‰rjestyksess‰.
            // vain l‰pikuultavat pit‰‰ j‰rjest‰‰. t‰ysin n‰kyv‰t renderoidaan samantien.
            for (int q = 0; q < ParticleGroups.Count; q++)
            {
                GL.PushMatrix();
                Particles curpar = ParticleGroups[q];
                GL.Translate(curpar.Position);

                for (int w = 0; w < curpar.NumOfParticles; w++)
                {
                    Particle p = curpar.GetParticle(w);
                    if (p.isTransparent == true) // listaan renderoitavaks myˆhemmin
                    {
                        Vector3 rp = p.pos + curpar.Position;
                        float len = (Camera.cam.Position - rp).LengthSquared;
                        slist.Add(new SortedList_Particles(len, p));
                    }
                    else // rendataan se nyt, ei lis‰t‰ sortattavaks
                    {
                        p.partTex.BillboardBegin(p.pos.X, p.pos.Y, p.pos.Z, p.zrot, p.size);
                        GL.Color4(p.color);
                        if (p.callBack != null) p.callBack(p);
                        p.partTex.BillboardRender();
                        p.partTex.BillboardEnd();
                    }
                }
                GL.PopMatrix();
            }
            slist.Sort(delegate(SortedList_Particles z1, SortedList_Particles z2) { return z2.Len.CompareTo(z1.Len); });
            GL.Disable(EnableCap.AlphaTest);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One);
            // rendataan l‰pikuultavat
            GL.DepthMask(false); // ei kirjoiteta zbufferiin
            for (int q = 0; q < slist.Count; q++)
            {
                Particle p = slist[q].Part;
                GL.Color4(p.color);
                p.partTex.BillboardBegin(p.pos.X, p.pos.Y, p.pos.Z, p.zrot, p.size);
                if (p.callBack != null) p.callBack(p);
                p.partTex.BillboardRender();
                p.partTex.BillboardEnd();
            }
            GL.DepthMask(true);
            GL.PopAttrib();
            GL.Color4(1f, 1, 1, 1f);
        }
    }
}
