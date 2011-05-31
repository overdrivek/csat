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
    class Test2D : BaseGame
    {
        Texture2D back, img;

        public override void Init()
        {
            fbo = new FBO(512, 512, true, true);

            back = Texture2D.Load("photo.jpg");
            img = Texture2D.Load("mousecursor.png");
            font = BitmapFont.Load("fonts/comic12.png");

            Camera.Set2D();
            base.Init();
        }

        public override void Dispose()
        {
            ClearArrays();
            base.Dispose();
        }

        public override void Update(float time)
        {
            if (Keyboard[Key.Escape]) GameLoop.Running = false;


            base.Update(time);
        }

        float ang = 0;
        public override void Render()
        {
            ang += 1;
            back.DrawFullScreen(0, 0);

            GL.Color4(1f, 1, 1, (float)Math.Abs(Math.Sin(ang * 0.01f)));
            back.Draw(10, 10, 0, 1, 1, false); // no blending
            back.Draw(Settings.Width / 2 + 10, 10, 0, 1, 1, true);
            GL.Color4(1f, 1, 1, 1f);

            img.Draw(Mouse.X, Mouse.Y, ang, 1, 1, true);

            font.Write("Hi there! 2D-test.");
            font.Write("Press ESC\nto start the next test.", 10, 20);

            base.Render();
        }
    }
}
