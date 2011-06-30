using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.Collections.Generic;

namespace CSatEng
{
    class TestSoftParticles : BaseGame
    {
        Model[] actors = new Model[10];
        Model scene = new Model();
        Billboard lightImg;
        Particles explosion, smoke;
        PostEffect blur, bloom;

        public override void Init()
        {
            colorFBO = new FBO(0, 0, 2, true); // 2 colorbufferia
            depthFBO = new FBO(0, 0, 0, true);
            blur = PostEffect.Load("blur.shader", "HORIZ VERT", 1f / (float)colorFBO.Width);
            bloom = PostEffect.Load("bloom.shader", "", 0.001f);

            Particles.EnableSoftParticles();
            ShadowMapping.Create(depthFBO, "lightmask.png");

            font = BitmapFont.Load("fonts/comic12.png");

            skybox = Sky.Load("sky/sky2_", "jpg");
            world.Add(skybox);

            lightImg = Billboard.Load("lightimg.png");

            //VBO.Flags = "LIGHTING";
            //VBO.Flags = "LIGHTING:PHONG";
            VBO.ShaderFileName = "shadowmapping.shader";
            VBO.Flags = "SHADOWS";

            DotScene ds = DotScene.Load("scene1/scene1.scene", scene);
            world.Add(scene);

            actors[0] = AnimatedModelMD5.Load("ugly/ukko.mesh");
            actors[0].LoadMD5Animation("act1", "ugly/ukko_action1.anim");
            actors[0].LoadMD5Animation("act2", "ugly/ukko_action2.anim");
            actors[0].LoadMD5Animation("act3", "ugly/ukko_action3.anim");
            actors[0].LoadMD5Animation("walk", "ugly/ukko_walk.anim");
            actors[0].SetAnimation("act2"); // idle anim
            actors[0].Position.Y = -0.5f;
            actors[0].Scale = new Vector3(5, 5, 5);
            world.Add(actors[0]);

            explosion = Particles.Load("explosion.particles.xml", new ParticleCallback(RenderParticleCallback));
            smoke = Particles.Load("smoke.particles.xml", null);
            actors[0].Add(smoke);

            Camera.Set3D();
            base.Init();
        }

        public override void Dispose()
        {
            ClearArrays();
            base.Dispose();
        }

        // partikkeliengine kutsuu tätä, asetettu räjähdykseen (halutaan muuttaa sen väriä)       
        void RenderParticleCallback(Particle p)
        {
            // nyt voi tehdä joka partikkelille mitä haluaa, esim asettaa alphan lifeksi.
            float tc = p.life / 2;
            GLExt.Color4(1f, tc, tc, tc);
        }

        void UpdateParticles(float time)
        {
            if (explosion.NumOfParticles == 0) explosion.Reset();
            explosion.Update(time);

            smoke.ResetOneParticle();
            smoke.Update(time);
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


            Model self = actors[0];
            bool moving = false, turning = false;
            if (Keyboard[Key.Up])
            {
                self.SetAnimation("walk");
                self.MoveXZ(-spd);
                moving = true;
                self.Update(time * 5);
            }
            else if (Keyboard[Key.Down])
            {
                self.SetAnimation("walk");
                self.MoveXZ(spd);
                moving = true;
                self.Update(-time * 5);
            }
            if (Keyboard[Key.Left])
            {
                if (moving == false)
                {
                    self.SetAnimation("act1");
                    self.Update(time);
                }
                self.Rotation.Y += spd * 15;
                turning = true;

            }
            else if (Keyboard[Key.Right])
            {
                if (moving == false)
                {
                    self.SetAnimation("act3");
                    self.Update(time);
                }
                self.Rotation.Y -= spd * 15;
                turning = true;

            }
            if (moving == false && turning == false) // idle
            {
                self.SetAnimation("act2");
                self.Update(time);
            }


            // tiputaanko
            Vector3 end = new Vector3(self.Position.X, self.Position.Y - 10, self.Position.Z);
            Model floor = (Model)scene.Search("floor");
            if (Intersection.CheckIntersection(ref self.Position, ref end, ref floor) == false)
            {
                self.Position.Y--; // kiva tippua
            }

            UpdateParticles(time);
            base.Update(time);
        }

        public override void Render()
        {
            ShadowMapping.SetupShadows(world, 0, true);
            GL.Clear(ClearFlags);
            camera.SetFPSCamera();

            // render scene to colorFBO
            world.RenderSceneWithParticles(colorFBO);
            colorFBO.BindFBO();
            lightImg.RenderBillboard(Light.Lights[0].Position, 0, 100, true);
            colorFBO.UnBindFBO();

            Camera.Set2D();
            {
                PostEffect.Begin(colorFBO);
                if (Keyboard[Key.R]) blur.RenderEffect();
                if (Keyboard[Key.T]) bloom.RenderEffect();

                PostEffect.End().DrawFullScreen(0, 0);
                font.Write("Soft particles + effects (press R / T)");
            }
            Camera.Set3D();

            base.Render();
        }
    }
}
