#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2011 mjt
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing details.
 */
#endregion
/*
Yksinkertainen .particle.xml formaatti:
* annetaan vakioarvo ja jos annetaan _max arvo, randomilla luku vakio->max v‰lilt‰.

<particles>
        <particle name="smoke" billboard="smoke.png" translucent="true" count="100" life="10" life_max="100" zrotation="0", zrotation_max="360"
          zrotation_adder="0.5" zrotation_adder_max="1" size="1" size_max="200">
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

    public class Particles : SceneNode
    {
        public static float ParticlePower = 1f;
        public static List<Particles> ParticleGroups = new List<Particles>();
        public static FBO Screen;
        public bool CastShadow = false;
        static bool softParticles = false;
        public static bool SoftParticles
        {
            get { return softParticles; }
        }
        public static void SetSoftParticles(FBO screen)
        {
            Screen = screen;
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
            if (screen.ColorTextures.Length < 2) Log.Error("Particles: fbo must have at least 2 colorbuffers.");
            depthShader = GLSLShader.Load("depth.shader:DEPTH_W", null);
            softParticles = true;
        }

        public bool IsTransparent = false;
        Billboard particleTex = null;
        public ParticleCallback callBack = null;

        List<Particle> particles = new List<Particle>();
        static GLSLShader depthShader;

        public static void SetDepthProgram()
        {
            depthShader.UseProgram();
        }

        /// <summary>
        ///  TODO
        /// </summary>
        public static Particles Load(string particleFileName)
        {
            Particles part = new Particles();




            part.Name = particleFileName;
            return part;
        }

        public int NumOfParticles
        {
            get { return particles.Count; }
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
            Log.WriteLine("Disposed: Particles", true);
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

            List<SortedList_Particles> slist = new List<SortedList_Particles>();

            GL.Disable(EnableCap.CullFace);
            if (Settings.UseGL3 == false)
            {
                GL.Disable(EnableCap.Lighting);
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
                else
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
                        c++;
                        if (p.isTransparent == true) // listaan renderoitavaks myˆhemmin
                        {
                            //float len = (Camera.cam.Position - (p.pos + curpar.Position)).LengthSquared;
                            float len = (Camera.cam.Position - matrix.Row3.Xyz).LengthSquared;
                            slist.Add(new SortedList_Particles(len, p, matrix));
                        }
                        else // rendataan se nyt, ei lis‰t‰ sortattavaks
                        {
                            GLExt.Scale(p.size, p.size, p.size);
                            GLExt.RotateZ(p.zrot);
                            if (VBO.FastRenderPass == false)
                            {
                                GLExt.Color4(p.color.X, p.color.Y, p.color.Z, p.color.W); ;
                                if (p.callBack != null) p.callBack(p);
                            }
                            p.partTex.RenderBillboard();
                        }
                    }
                    GLExt.PopMatrix();
                }
                GLExt.PopMatrix();
            }

            slist.Sort(delegate(SortedList_Particles z1, SortedList_Particles z2) { return z2.Len.CompareTo(z1.Len); });
            if (Settings.UseGL3 == false) GL.Disable(EnableCap.AlphaTest);
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
            GLExt.PopMatrix();
            GL.DepthMask(true);
            GL.Disable(EnableCap.Blend);
            GLExt.Color4(1, 1, 1, 1);
            GLExt.SetLighting(true);
            BaseGame.NumOfObjects += c;
        }

    }
}
