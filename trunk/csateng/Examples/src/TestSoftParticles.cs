using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace CSatEng
{
    class TestSoftParticles : BaseGame
    {
        Model[] actors = new Model[10];
        Model scene = new Model();
        Billboard lightImg;
        const int PART = 100;
        Particles explosion = new Particles();
        Particles smoke = new Particles();
        Texture2D screen;
        GLSLShader depth;

        public override void Init()
        {
            depthColorFBO = new FBO(512, 512, 1, true);
            colorFBO = new FBO(512, 512, 1, true);
            depthFBO = new FBO(512, 512, 0, true);
            shadows = new ShadowMapping(depthFBO);
            screen = colorFBO.CreateDrawableColorTexture(0);

            Particles.SetSoftParticles(true, new ShaderCallback(CallBacks.ParticleShaderCallBack));
            depth = GLSLShader.Load("depth.shader", null);
            

            font = BitmapFont.Load("fonts/comic12.png");

            skybox = Sky.Load("sky/sky_", "jpg");
            world.Add(skybox);

            lightImg = Billboard.Load("lightimg.png");

            DotScene ds = DotScene.Load("scene1/scene1.scene", scene);
            GLSLShader.LoadShader(scene, "shadow.shader", new ShaderCallback(CallBacks.ShadowShaderCallBack));
            world.Add(scene);

            actors[0] = AnimatedModelMD5.Load("ugly/ukko.mesh");
            actors[0].LoadMD5Animation("act1", "ugly/ukko_action1.anim");
            actors[0].LoadMD5Animation("act2", "ugly/ukko_action2.anim");
            actors[0].LoadMD5Animation("act3", "ugly/ukko_action3.anim");
            actors[0].LoadMD5Animation("walk", "ugly/ukko_walk.anim");
            actors[0].SetAnimation("act2"); // idle anim
            actors[0].Position.Y = -0.5f;
            actors[0].Scale = new Vector3(5, 5, 5);
            GLSLShader.LoadShader(actors[0], "shadow.shader", new ShaderCallback(CallBacks.ShadowShaderCallBack));
            world.Add(actors[0]);

            explosion.SetParticle(Billboard.Load("fire.png"), true, false, new ParticleCallback(RenderParticleCallback));
            smoke.SetParticle(Billboard.Load("smoke.png"), true, true, null);
            smoke.Position.Y = 3;
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
            GL.Color4(1f, tc, tc, tc);
        }

        void UpdateParticles(float time)
        {
            if (explosion.NumOfParticles == 0) SetupParticles(true, false);
            explosion.Update(time);

            Vector3 pos = new Vector3(0, 0, 0);
            Vector3 dir = new Vector3(-0.05f + (float)(Rnd.NextDouble() * 0.1f), 0.2f, -0.05f + (float)(Rnd.NextDouble() * 0.1f));
            Vector3 grav = new Vector3(0, 0, 0);
            float life = 1;
            float size = (float)Rnd.NextDouble() + 0.5f;
            float zrot = (float)(Rnd.NextDouble() * 360);
            float zrotAdder = (float)(Rnd.NextDouble());
            smoke.AddParticle(ref pos, ref dir, ref grav, life, zrot, zrotAdder, size, new Vector4(0.5f, 0.5f, 0.5f, 0.1f));
            smoke.Update(time);
        }

        void SetupParticles(bool explosion, bool smoke)
        {
            if (explosion)
            {
                for (int q = 0; q < PART; q++)
                {
                    Vector3 pos = new Vector3(50 + (float)(Rnd.NextDouble() * 1), 5 + (float)(Rnd.NextDouble() * 1), 0);
                    Vector3 dir = new Vector3(0.5f * (0.5f - (float)(Rnd.NextDouble())), 0.5f * (0.5f - (float)(Rnd.NextDouble())), 0.5f * (0.5f - (float)(Rnd.NextDouble())));
                    Vector3 grav = new Vector3(0, 0, 0);
                    float life = 2;
                    float size = (float)(Rnd.NextDouble() * 2 + 1);
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
                    float life = (float)(Rnd.NextDouble() * 2);
                    float size = 1;
                    float zrot = (float)(Rnd.NextDouble() * 360);
                    float zrotAdder = (float)(Rnd.NextDouble());
                    this.smoke.AddParticle(ref pos, ref dir, ref grav, life, zrot, zrotAdder, size, new Vector4(0.4f, 0.4f, 0.4f, 0.1f));
                }
            }
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
            shadows.SetupShadows(world, 0, true);
            GL.Clear(GameLoop.ClearFlags);
            camera.SetFPSCamera();
            Light.UpdateLights();
            Frustum.CalculateFrustum();

            if (Particles.SoftParticles)
            {
                // rendaa skenen depth colorbufferiin, ei textureita/materiaaleja
                depthColorFBO.BeginDrawing(false);
                {
                    GL.ClampColor(ClampColorTarget.ClampVertexColor, ClampColorMode.False);
                    GL.Arb.ClampColor(ArbColorBufferFloat.ClampVertexColorArb, (ArbColorBufferFloat)ClampColorMode.False);
                    GL.Arb.ClampColor(ArbColorBufferFloat.ClampFragmentColorArb, (ArbColorBufferFloat)ClampColorMode.False);
                    GL.Arb.ClampColor(ArbColorBufferFloat.ClampReadColorArb, (ArbColorBufferFloat)ClampColorMode.False);
                    GL.ClampColor(ClampColorTarget.ClampFragmentColor, ClampColorMode.False);
                    GL.ClampColor(ClampColorTarget.ClampReadColor, ClampColorMode.False);
                    GL.ClearColor(float.MaxValue, float.MaxValue, float.MaxValue, float.MaxValue);
                    depthColorFBO.Clear();
                    GL.ClearColor(0, 0, 0, 0);

                    ShadowMapping.ShadowPass = true;
                    depth.UseProgram();
                    world.Render();
                    ShadowMapping.ShadowPass = false;

                    GL.Arb.ClampColor(ArbColorBufferFloat.ClampVertexColorArb, (ArbColorBufferFloat)ClampColorMode.True);
                    GL.Arb.ClampColor(ArbColorBufferFloat.ClampFragmentColorArb, (ArbColorBufferFloat)ClampColorMode.True);
                    GL.Arb.ClampColor(ArbColorBufferFloat.ClampReadColorArb, (ArbColorBufferFloat)ClampColorMode.True);
                }
                depthColorFBO.EndDrawing();

                // rendaa skene uudelleen textureineen
                colorFBO.BeginDrawing(false);
                {
                    colorFBO.Clear();
                    world.RenderAgain();
                    depthColorFBO.BindColor(0, Settings.DEPTH_TEXUNIT);
                    Particles.Render();
                    depthColorFBO.UnBindColor(Settings.DEPTH_TEXUNIT);
                }
                colorFBO.EndDrawing();
            }
            else
            {
                // rendaa skene textureineen
                colorFBO.BeginDrawing(false);
                {
                    colorFBO.Clear();
                    world.Render();
                    Particles.Render();
                }
                colorFBO.EndDrawing();
            }

            Camera.Set2D();
            {
                screen.DrawFullScreen(0, 0);
                font.Write("Soft particles");
            }
            Camera.Set3D();

            base.Render();
        }

    }
}
