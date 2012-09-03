// csat test game

using DragonOgg.Interactive;
using DragonOgg.MediaPlayer;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace CSatEng
{
    class GameMenu : GameClass
    {
        Texture2D back, crosshair;

        public override void Init()
        {
            Settings.TextureDir = "../../data/texture/";

            back = Texture2D.Load("menu.jpg");
            crosshair = Texture2D.Load("cross.png", true);

            font = BitmapFont.Load("fonts/comic12.png");

            BaseGame.Instance.CursorVisible = false; // hide mouse cursor

            Camera.Set2D();
            base.Init();
        }

        public override void Dispose()
        {
            back.Dispose();
            crosshair.Dispose();
            font.Dispose();
            ClearArrays();

            base.Dispose();
        }

        public override void Update(float time)
        {

            if (Mouse[MouseButton.Left])
            {
                float x = Mouse.X;
                float y = Mouse.Y;
                // orig size is 1024x768 (where coordinates below are taken)
                // so calculate x & y incase resolution is different than orig
                x = x / (float)Settings.Width * 1024f;
                y = y / (float)Settings.Height * 768f;

                if (y > 400 && y < 500)
                {
                    // start
                    if (x > 80 && x < 260)
                    {
                        GameClass game = new Game();  // start game
                        BaseGame.SetGame(game);
                        return;
                    }

                    // exit
                    if (x > 780 && x < 940)
                    {
                        Dispose();
                        BaseGame.Running = false; // end game
                        return;
                    }

                }
            }

            base.Update(time);
        }

        public override void Render()
        {
            GL.Clear(ClearFlags);
            back.DrawFullScreen(0, 0);

            crosshair.Draw(Mouse.X, Settings.Height - Mouse.Y);

            base.Render();
        }
    }
}
