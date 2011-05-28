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
    class TestMD5 : BaseGame
    {
        Model[] actors = new Model[10];
        Model scene = new Model();
        Billboard lightImg;

        public override void Init()
        {
            fbo = new FBO(512, 512, false, true);
            shadows = new ShadowMapping(fbo);

            font = BitmapFont.Load("fonts/comic12.png");

            Sky skybox = Sky.Load("sky/sky_", "jpg");
            world.Add(skybox); // lisää ekana jolloin edellinen ruutu häviää tämän 'alle' (ruutua kun ei putsata)

            DotScene ds = DotScene.Load("scene1/scene1.scene", scene);
            GLSLShader.LoadShader(scene, "shadow.shader");
            world.Add(scene);

            actors[0] = AnimatedModelMD5.Load("ugly/ukko.mesh");
            actors[0].LoadMD5Animation("act1", "ugly/ukko_action1.anim");
            actors[0].LoadMD5Animation("act2", "ugly/ukko_action2.anim");
            actors[0].LoadMD5Animation("act3", "ugly/ukko_action3.anim");
            actors[0].LoadMD5Animation("walk", "ugly/ukko_walk.anim");
            actors[0].SetAnimation("act2"); // idle anim
            actors[0].Scale = new Vector3(5, 5, 5);
            GLSLShader.LoadShader(actors[0], "shadow.shader");
            // "model.shader");
            // "model.shader:TEXTURE");
            // "shadow.shader:NO_NORMALS");
            world.Add(actors[0]);

            lightImg = Billboard.Load("lightimg.png");

            Camera.Set3D();

            base.Init();
        }

        public override void Dispose()
        {
            Util.ClearArrays();
            fbo.Dispose();

            base.Dispose();
        }

        public override void Update(float time)
        {
            if (Keyboard[Key.Escape]) GameLoop.Running = false;

            // ohjaus
            float spd = time * 20;
            if (Keyboard[Key.ShiftLeft]) spd *= 4;
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
                self.Update(time * 5);
            }

            // tiputaanko
            Vector3 end = new Vector3(self.Position.X, self.Position.Y - 10, self.Position.Z);
            Model floor = (Model)scene.Search("floor");
            if (Intersection.CheckIntersection(ref self.Position, ref end, ref floor) == false)
            {
                self.Position.Y--; // kiva tippua
            }


            if (Keyboard[Key.Left])
            {
                if (moving == false) self.SetAnimation("act1");
                self.Rotation.Y += spd * 15;
                turning = true;
                self.Update(time);
            }
            else if (Keyboard[Key.Right])
            {
                if (moving == false) self.SetAnimation("act3");
                self.Rotation.Y -= spd * 15;
                turning = true;
                self.Update(time);
            }

            if (moving == false && turning == false) // idle
            {
                self.SetAnimation("act2");
                self.Update(time);
            }

            base.Update(time);
        }

        public override void Render()
        {
            FBO.ZNear = 100;
            FBO.ZFar = 1000;
            shadows.SetupShadows(world);

            camera.SetFPSCamera();

            Light.UpdateLights();
            Frustum.CalculateFrustum();
            world.Render();

            lightImg.RenderBillboard(Light.Lights[0].Position, 50);


            Camera.Set2D();
            font.Write("Arrow keys: move the ugly.\nA,D,W,S: move the camera.\nHold left mouse button to rotate the camera.\n\nNo collision detection.", 0, 0);
            Camera.Set3D();

            base.Render();
        }

    }
}
