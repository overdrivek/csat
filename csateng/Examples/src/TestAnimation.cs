#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2008-2014 mjt
 * This notice may not be removed from any source distribution.
 * See csat-license.txt for licensing details.
 */
#endregion
using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace CSatEng
{
    class TestAnimation : GameClass
    {
        Model[] actors = new Model[10];
        Model scene = new Model();
        Billboard lightImg;

        public override void Init()
        {
            depthFBO = new FBO(0, 0, 1, true);
            ShadowMapping.Create(depthFBO, "lightmask.png");

            font = BitmapFont.Load("fonts/comic12.png");

            skybox = Sky.Load("sky/sky_", "jpg");
            world.Add(skybox);

            GLSLShader.SetShader("shadowmapping.shader", "SHADOWS");
            DotScene ds = DotScene.Load("scene1/scene1.scene", scene);
            world.Add(scene);

            actors[0] = AnimatedModel.Load("ugly/ukko.mesh");
            actors[0].LoadMD5Animation("act1", "ugly/ukko_action1.anim");
            actors[0].LoadMD5Animation("act2", "ugly/ukko_action2.anim");
            actors[0].LoadMD5Animation("act3", "ugly/ukko_action3.anim");
            actors[0].LoadMD5Animation("walk", "ugly/ukko_walk.anim");
            actors[0].SetAnimation("act2"); // idle anim
            actors[0].Scale = new Vector3(5, 5, 5);
            world.Add(actors[0]);

            lightImg = Billboard.Load("lightimg.png");

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


            base.Update(time);
        }

        public override void Render()
        {
            ShadowMapping.SetupShadows(world, 0, false);
            GL.Clear(ClearFlags);
            camera.SetFPSCamera();
            world.Render();

            if (Keyboard[Key.Space])
            {
                AnimatedModel self = actors[0] as AnimatedModel;
                self.RenderSkeleton(); // note: ei näy gl3:ssa
            }

            lightImg.RenderBillboard(Light.Lights[0].Position, 0, 50, true);
            Camera.Set2D();
            font.Write("Arrow keys: move the ugly.\n" + (Settings.UseGL3 == false ? "Space: show skeleton.\n" : "") + "A,D,W,S: move the camera.\nHold left mouse button to rotate the camera.", 0, 0);
            Camera.Set3D();

            base.Render();
        }
    }
}
