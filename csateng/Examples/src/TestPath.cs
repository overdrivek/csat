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
    class TestPath : BaseGame
    {
        Path camPath;

        public override void Init()
        {
            fbo = new FBO(512, 512, false, true);
            shadows = new ShadowMapping(fbo);

            Sky skybox = Sky.Load("sky/sky2_", "jpg");
            world.Add(skybox); // lisää ekana jolloin edellinen ruutu häviää tämän 'alle' (ruutua kun ei putsata)

            Model scene = new Model();
            DotScene ds = DotScene.Load("scene1/scene1.scene", scene);
            GLSLShader.LoadShader(scene, "shadow.shader");
            world.Add(scene);

            camPath = Path.GetPath("Path_camera");
            camPath.Attach(camera, true, true);

            font = BitmapFont.Load("fonts/comic12.png");

            Camera.Set3D();
            base.Init();
        }

        public override void Dispose()
        {
            Util.ClearArrays();
            fbo.Dispose();

            base.Dispose();
        }

        int counter = 0;
        string str = "Looking at next point";
        Vector3 lookAt = Vector3.Zero;
        public override void Update(float time)
        {
            if (Keyboard[Key.Escape]) GameLoop.Running = false;

            counter++;
            if (counter == 100)
            {
                camPath.LookAtNextPoint = false;
                str = "Looking at origin";
                lookAt = Vector3.Zero;
            }
            if (counter == 200)
            {
                camPath.LookAtNextPoint = false;
                str = "Looking at random point";
                lookAt = new Vector3((float)Rnd.NextDouble() * 100, (float)Rnd.NextDouble() * 50, (float)Rnd.NextDouble() * 100); 
            }
            if (counter == 300)
            {
                camPath.LookAtNextPoint = true;
                counter = 0;
                str = "Looking at next point";
            }

            camPath.Update(time, lookAt);

            base.Update(time);
        }

        public override void Render()
        {
            shadows.SetupShadows(world);

            camera.SetCameraMatrix();

            Light.UpdateLights();
            Frustum.CalculateFrustum();
            world.Render();

            Camera.Set2D();
            font.Write(str);
            Camera.Set3D();

            base.Render();
        }

    }
}
