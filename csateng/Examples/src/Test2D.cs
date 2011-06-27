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
            back = Texture2D.Load("photo.jpg");
            img = Texture2D.Load("mousecursor.png");
            font = BitmapFont.Load("fonts/comic12.png");

            Camera.Set2D();
            base.Init();
        }

        public override void Dispose()
        {
            back.Dispose();
            img.Dispose();
            font.Dispose();

            ClearArrays();
            base.Dispose();
        }

        public override void Update(float time)
        {
            if (Keyboard[Key.Escape]) Tests.NextTest = true;


            base.Update(time);
        }

        float ang = 0;
        public override void Render()
        {
            ang += 1;
            GL.Clear(ClearFlags);

            back.DrawFullScreen(0, 0);

            GLExt.Color4(1, 1, 1, (float)Math.Abs(Math.Sin(ang * 0.01f)));
            back.Draw(10, 10, 0, 1, 1, false); // no blending
            back.Draw(Settings.Width / 2 + 10, 10, 0, 1, 1, true);
            GLExt.Color4(1, 1, 1, 1);

            img.Draw(Mouse.X, Mouse.Y, ang, 1, 1, true);

            font.Write("2D-test.");
            font.Write("Press ESC\nto start the next test.", 10, 20);

            base.Render();
        }
    }
}
