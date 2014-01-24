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
    class TestPath : GameClass
    {
        Path camPath;

        public override void Init()
        {
            depthFBO = new FBO(0, 0, 1, true);
            ShadowMapping.Create(depthFBO, "lightmask.png");

            Fog.CreateFog(0.005f, Fog.Color);
            GLSLShader.SetShader("default.shader", "FOG");
            skybox = Sky.Load("sky/sky2_", "jpg");
            world.Add(skybox);

            GLSLShader.SetShader("default.shader", "LIGHTING:PERPIXEL:FOG");
            Model scene = new Model();
            DotScene ds = DotScene.Load("scene1/scene1.scene", scene);
            world.Add(scene);

            camPath = Path.GetPath("Path_camera");
            camPath.Attach(camera, true, true);

            font = BitmapFont.Load("fonts/comic12.png");

            Camera.Set3D();
            base.Init();
        }

        public override void Dispose()
        {
            Fog.DisableFog();
            ClearArrays();
            base.Dispose();
        }

        float counter = 0;
        string str = "Looking at next point";
        Vector3 lookAt = Vector3.Zero;
        int curAnim = 0;
        public override void Update(float time)
        {
            if (Keyboard[Key.Escape]) Tests.NextTest = true;

            counter+=time;
            if ((int)counter == 4 && curAnim!=(int)counter)
            {
                curAnim = (int)counter;
                camPath.LookAtNextPoint = false;
                str = "Looking at origin";
                lookAt = Vector3.Zero;
            }
            if ((int)counter == 8 && curAnim != (int)counter)
            {
                curAnim = (int)counter;
                camPath.LookAtNextPoint = false;
                str = "Looking at random point";
                lookAt = new Vector3((float)Rnd.NextDouble() * 100, (float)Rnd.NextDouble() * 50, (float)Rnd.NextDouble() * 100);
            }
            if ((int)counter == 12 && curAnim != (int)counter)
            {
                curAnim = (int)counter;
                camPath.LookAtNextPoint = true;
                counter = 0;
                str = "Looking at next point";
            }

            camPath.Update(time, lookAt);

            base.Update(time);
        }

        public override void Render()
        {
            ShadowMapping.SetupShadows(world, 0, false);
            GL.Clear(ClearFlags);
            camera.SetCameraMatrix();
            world.Render();

            Camera.Set2D();
            font.Write(str);
            Camera.Set3D();

            base.Render();
        }

    }
}
