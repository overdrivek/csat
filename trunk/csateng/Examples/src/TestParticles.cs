#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2011 mjt[matola@sci.fi]
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing details.
 */
#endregion
using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace CSatEng
{
    class TestParticles : BaseGame
    {
        const int PART = 100;
        Particles test = new Particles("planets");
        Particles explosion = new Particles("explosion");
        Particles smoke = new Particles("smoke");
        ParticleEngine particles = new ParticleEngine(); // engine huolehtii että partikkelit renderoidaan oikeassa järjestyksessä

        public override void Init()
        {
            test.SetObject(Billboard.Load("earth.png"), false); // ei läpinäkyvä
            explosion.SetObject(Billboard.Load("fire.png"), true); // läpinäkyvä
            smoke.SetObject(Billboard.Load("smoke.png"), true); // kuten tämäkin

            particles.Add(test, null);
            particles.Add(explosion, new ParticleCallback(RenderParticleCallback));
            particles.Add(smoke, null);
            SetupParticles(true, true, true);

            Camera.Set3D();

            base.Init();
        }

        public override void Dispose()
        {
            Util.ClearArrays();
            base.Dispose();
        }

        public override void Update(float time)
        {
            if (Keyboard[Key.Escape]) GameLoop.Running = false;
            UpdateParticles(time);

            base.Update(time);
        }

        public override void Render()
        {
            GL.Disable(EnableCap.Lighting);
            GL.LoadIdentity();
            GL.Translate(0, 0, -150);

            // particles -partikkeliengine hoitaa sinne lisättyjen partikkelien renderoinnin. 
            // se sorttaa ne, hoitaa takaisinkutsut ym.
            particles.Render();

            base.Render();
        }

        void SetupParticles(bool test, bool explosion, bool smoke)
        {
            if (test)
            {
                for (int q = 0; q < PART; q++)
                {
                    Vector3 pos = new Vector3(-50 + (float)(Rnd.NextDouble() * 3), 5 + (float)(Rnd.NextDouble() * 3), 0);
                    Vector3 dir = new Vector3(0.4f + (float)(Rnd.NextDouble() * 0.1f), 0.4f + (float)(Rnd.NextDouble() * 0.1f), 0.4f + (float)(Rnd.NextDouble() * 0.1f));
                    Vector3 grav = new Vector3(0, -0.01f, 0);
                    float life = (float)(Rnd.NextDouble() * 1000 + 5000);
                    float size = 2;

                    this.test.AddParticle(ref pos, ref dir, ref grav, life, size, new Vector4(1, 1, 1, 1));
                }
            }
            if (explosion)
            {
                for (int q = 0; q < PART; q++)
                {
                    Vector3 pos = new Vector3(50 + (float)(Rnd.NextDouble() * 1), 5 + (float)(Rnd.NextDouble() * 1), 0);
                    Vector3 dir = new Vector3(0.5f * (0.5f - (float)(Rnd.NextDouble())), 0.5f * (0.5f - (float)(Rnd.NextDouble())), 0.5f * (0.5f - (float)(Rnd.NextDouble())));
                    Vector3 grav = new Vector3(0, 0, 0);
                    float life = 2;
                    float size = (float)(Rnd.NextDouble() * 10 + 6);

                    this.explosion.AddParticle(ref pos, ref dir, ref grav, life, size, new Vector4(0.3f, 0, 0, 0.5f));
                }
            }
            if (smoke)
            {
                for (int q = 0; q < PART; q++)
                {
                    Vector3 pos = new Vector3(0, 5 + (float)(Rnd.NextDouble() * 10), 0);
                    Vector3 dir = new Vector3(-0.05f + (float)(Rnd.NextDouble() * 0.1f), 0.1f, -0.05f + (float)(Rnd.NextDouble() * 0.1f));
                    Vector3 grav = new Vector3(0, 0, 0);
                    float life = 1 + (float)(Rnd.NextDouble() * 4);
                    float size = 10;
                    this.smoke.AddParticle(ref pos, ref dir, ref grav, life, size, new Vector4(0.4f, 0.4f, 0.4f, 0.2f));
                }
            }
        }

        // partikkeliengine kutsuu tätä, asetettu räjähdykseen (halutaan muuttaa sen väriä)       
        void RenderParticleCallback(Particle p)
        {
            // nyt voi tehdä joka partikkelille mitä haluaa, esim asettaa alphan lifeksi.
            float tc = p.life / 2;
            GL.Color4(1f, tc, tc, tc);
        }

        void UpdateParticles(float time)
        {
            if (test.NumOfParticles == 0) SetupParticles(true, false, false);
            if (explosion.NumOfParticles == 0) SetupParticles(false, true, false);

            test.Update(time * 1000);
            explosion.Update(time);

            if (smoke.NumOfParticles < PART)
            {
                Vector3 pos = new Vector3(0, 5 + (float)(Rnd.NextDouble() * 5), 0);
                Vector3 dir = new Vector3(-0.05f + (float)(Rnd.NextDouble() * 0.1f), 0.2f, -0.05f + (float)(Rnd.NextDouble() * 0.1f));
                Vector3 grav = new Vector3(0, 0, 0);
                float life = 3;
                float size = 15;
                smoke.AddParticle(ref pos, ref dir, ref grav, life, size, new Vector4(0.5f, 0.5f, 0.5f, 0.1f));
            }
            smoke.Update(time);
        }

    }
}
