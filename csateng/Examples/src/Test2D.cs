using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

/*note:
 * images origo (0,0) is center or left-lower corner.
 * screen origo (0,0) is left-lower corner.
 * 
 * however, when writing text, origo is left-top corner.
 */

namespace CSatEng
{
    class Test2D : GameClass
    {
        Texture2D back, img;
        int[] x = new int[100];
        int[] y = new int[100];
        float[] z = new float[100];

        public override void Init()
        {
            // random positions for images
            for (int q = 0; q < 100; q++)
            {
                x[q] = Rnd.Next(600) + 100;
                y[q] = Rnd.Next(300) + 100;
                z[q] = (float)Rnd.NextDouble();
            }

            BaseGame.Instance.CursorVisible = false;

            back = Texture2D.Load("photo.jpg");
            img = Texture2D.Load("mousecursor.png", true); // center origo (because rotating cursor)
            font = BitmapFont.Load("fonts/comic12.png");

            Camera.Set2D();
            base.Init();
        }

        public override void Dispose()
        {
            BaseGame.Instance.CursorVisible = true;

            back.Dispose();
            img.Dispose();
            font.Dispose();

            ClearArrays();
            base.Dispose();
        }

        int lastWheel = 0;
        public override void Update(float time)
        {
            if (Keyboard[Key.Escape]) Tests.NextTest = true;

            if (Mouse.Wheel > lastWheel && mouseZ < 1)
                mouseZ += 0.1f;
            if (Mouse.Wheel < lastWheel && mouseZ > 0)
                mouseZ -= 0.1f;
            if (mouseZ >= 1.0f)
                mouseZ = 0.9999f;
            if (mouseZ < 0.0f)
                mouseZ = 0;

            lastWheel = Mouse.Wheel;

            base.Update(time);
        }

        float mouseZ = 1;
        float ang = 0;
        public override void Render()
        {
            ang += 1;
            GL.Clear(ClearFlags);

            back.DrawFullScreen(0, 0);

            GLExt.Color4(0.5f, 0.5f, 0.5f, 0.5f);
            // images at random z (0.0 - 1.0)
            for (int q = 0; q < 100; q++)
                back.Draw(x[q], y[q], z[q], 0, 0.2f, 0.2f, false); // no blending
            GLExt.Color4(1, 1, 1, 1);


            GLExt.Color4(1, 1, 1, (float)Math.Abs(Math.Sin(ang * 0.01f)));
            back.Draw(10, 10, 0, 1, 1, false); // no blending
            back.Draw(Settings.Width / 2 + 10, 10, 0, 1, 1, true); // blending
            GLExt.Color4(1, 1, 1, 1);

            // must invert mouse y
            img.Draw(Mouse.X, Settings.Height - Mouse.Y,
                mouseZ,
                ang, 1, 1, true);

            font.Write("2D-test.", 0, 0);
            font.Write("\nUse mouse wheel to move mouse pointer at Z-axis.\n\nPress ESC\nto start the next test.", 10, 20);

            //Console.WriteLine("> " + Mouse.X + "  " + Mouse.Y);

            base.Render();
        }
    }
}
