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
        Particles test = new Particles();
        Particles explosion = new Particles();
        Particles smoke = new Particles();

        public override void Init()
        {
            test.SetParticle(Billboard.Load("earth.png"), false, null); // ei läpikuultava
            explosion.SetParticle(Billboard.Load("fire.png"), true, new ParticleCallback(RenderParticleCallback)); // läpikuultava
            smoke.SetParticle(Billboard.Load("smoke.png"), true, null); // kuten tämäkin
            world.Add(test);
            world.Add(explosion);
            world.Add(smoke);
            SetupParticles(true, true, true);

            font = BitmapFont.Load("fonts/comic12.png");
            
            camera.Position = new Vector3(0, 0, 150);
            Camera.Set3D();
            base.Init();
        }

        public override void Dispose()
        {
            ClearArrays();
            base.Dispose();
        }

        public override void Update(float time)
        {
            if (Keyboard[Key.Escape]) Tests.NextTest = true;

            // ohjaus
            float spd = time * 20;
            if (Keyboard[Key.ShiftLeft] || Keyboard[Key.ShiftRight]) spd *= 4;
            if (Keyboard[Key.W]) camera.Move(spd);
            if (Keyboard[Key.S]) camera.Move(-spd);
            if (Keyboard[Key.A]) camera.Strafe(-spd);
            if (Keyboard[Key.D]) camera.Strafe(spd);
            if (Mouse[MouseButton.Left])
            {
                camera.Rotation.Y -= Mouse.X - oldMouseX;
                camera.Rotation.X -= Mouse.Y - oldMouseY;
            }

            UpdateParticles(time);

            base.Update(time);
        }

        public override void Render()
        {
            GL.Clear(GameLoop.ClearFlags);

            camera.SetFPSCamera();
            world.Render();

            Camera.Set2D();
            font.Write("Particles");
            Camera.Set3D();
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
                    this.test.AddParticle(ref pos, ref dir, ref grav, life, 0, 0, size, new Vector4(1, 1, 1, 1));
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
                    float zrot = (float)(Rnd.NextDouble() * 360);
                    float zrotAdder = 0;
                    this.explosion.AddParticle(ref pos, ref dir, ref grav, life, zrot, zrotAdder, size, new Vector4(0.3f, 0, 0, 0.5f));
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
                    float zrot = (float)(Rnd.NextDouble() * 360);
                    float zrotAdder = (float)(Rnd.NextDouble());
                    this.smoke.AddParticle(ref pos, ref dir, ref grav, life, zrot, zrotAdder, size, new Vector4(0.4f, 0.4f, 0.4f, 0.1f));
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
                float zrot = (float)(Rnd.NextDouble() * 360);
                float zrotAdder = (float)(Rnd.NextDouble());
                smoke.AddParticle(ref pos, ref dir, ref grav, life, zrot, zrotAdder, size, new Vector4(0.5f, 0.5f, 0.5f, 0.1f));
            }
            smoke.Update(time);
        }

    }
}
