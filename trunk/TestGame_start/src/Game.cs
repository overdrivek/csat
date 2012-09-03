// csat test game

using DragonOgg.Interactive;
using DragonOgg.MediaPlayer;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace CSatEng
{
    class Game : GameClass
    {
        Texture2D back;
        Texture2D[] img = new Texture2D[2];

        public override void Init()
        {
            Settings.TextureDir = "../../data/texture/";
            string AudioDir = "../../data/";

            back = Texture2D.Load("bg.jpg");
            img[0] = Texture2D.Load("head.png");
            font = BitmapFont.Load("fonts/comic12.png");

            Camera.Set2D();
            base.Init();
        }

        public override void Dispose()
        {
            back.Dispose();
            img[0].Dispose();
            font.Dispose();

            ClearArrays();

            base.Dispose();
        }

        public override void Update(float time)
        {
            
            if (Keyboard[Key.Escape])
            {
                Dispose();
                BaseGame.Running = false;
                return;
            }

            base.Update(time);
        }

        int x=100, y=100;
        public override void Render()
        {
            GL.Clear(ClearFlags);

            back.DrawFullScreen(0, 0);

            if (Mouse[MouseButton.Left])
            {
            }

            img[0].Draw(x, y, 0, 1, 1, true);


            font.Write("Hellurei", 0, 0);

            base.Render();
        }
    }
}
