// csat dialog test

using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace CSatEng
{
    class Game : GameClass
    {
        Texture2D back;

        public override void Init()
        {
            back = Texture2D.Load("bg.jpg");
            Camera.Set2D();
            base.Init();
        }

        public override void Dispose()
        {
            back.Dispose();
            ClearArrays();
            BaseGame.Running = false;

            base.Dispose();
        }

        public override void Update(float time)
        {
            if (Keyboard[Key.Escape])
                Dispose();

            base.Update(time);
        }

        public override void Render()
        {
            GL.Clear(ClearFlags);

            GLExt.Color4((float)Mouse.X / Settings.Width, 
                (float)Mouse.Y / Settings.Height, 
                1, 1);
            back.DrawFullScreen(0, 0);
            GLExt.Color4(1, 1, 1, 1);

            base.Render();
        }
    }
}
