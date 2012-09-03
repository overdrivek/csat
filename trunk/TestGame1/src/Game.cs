// csat test game

using DragonOgg.Interactive;
using DragonOgg.MediaPlayer;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace CSatEng
{
    class Game : GameClass
    {
        Texture2D back, crosshair;
        Texture2D[] img = new Texture2D[2];
        AudioClip snd1, snd2;
        public static OggPlayerFBN player;

        public override void Init()
        {
            Settings.TextureDir = "../../data/texture/";
            string AudioDir = "../../data/";

            back = Texture2D.Load("bg.jpg");
            img[0] = Texture2D.Load("head.png", true);
            img[1] = Texture2D.Load("ball.png", true);
            crosshair = Texture2D.Load("cross.png", true);

            font = BitmapFont.Load("fonts/comic12.png");

            BaseGame.Instance.CursorVisible = false; // hide mouse cursor
            //System.Windows.Forms.Cursor.Hide();

            if (player == null)
            {
                player = new OggPlayerFBN();
                player.SetCurrentFile(AudioDir + "music.ogg");

            }
            player.Play();

            snd1 = new AudioClip(AudioDir + "snd2.ogg");
            snd2 = new AudioClip(AudioDir + "snd1.ogg");

            Camera.Set2D();
            base.Init();
        }

        public override void Dispose()
        {
            back.Dispose();
            img[0].Dispose();
            img[1].Dispose();
            crosshair.Dispose();
            font.Dispose();

            ClearArrays();

            //player.Stop();
            //player.Dispose();

            base.Dispose();
        }

        public override void Update(float time)
        {
            visibleTime += time;

            if (player.PlayerState == OggPlayerStatus.Stopped)
                player.Play();

            if (tex == -1 || visibleTime > 1)
            {
                if (visibleTime < 100) misses++;

                visibleTime = 0;
                x = Rnd.Next(Settings.Width - 100);
                y = Rnd.Next(Settings.Height - 100);
                tex = Rnd.Next(2);
            }

            // end game. back to menu
            if (Keyboard[Key.Escape])
            {
                GameClass game = new GameMenu();  // menu
                BaseGame.SetGame(game);
                return;
            }

            base.Update(time);
        }

        int x, y;
        float visibleTime = 0;
        float ang = 0;
        int score = 0, misses = 0;
        int tex = -1;
        bool clicked = true;
        public override void Render()
        {
            ang += 1;
            GL.Clear(ClearFlags);

            back.DrawFullScreen(0, 0);

            int mouseY = Settings.Height - Mouse.Y; // invert mouse

            if (Mouse[MouseButton.Left])
            {
                if (!clicked)
                {
                    clicked = true;

                    if (Mouse.X >= x && Mouse.X < x + img[tex].Width &&
                        mouseY >= y && mouseY < y + img[tex].Height)
                    {
                        visibleTime = 100;
                        score++;
                        snd1.Play();
                    }
                    else
                    {
                        misses++;
                        snd2.Play();
                    }

                }
            }
            else clicked = false;

            img[tex].Draw(x + img[tex].Width / 2, y + img[tex].Height / 2, ang, 1, 1, true);

            crosshair.Draw(Mouse.X, mouseY);

            font.Write("Hits: " + score + "  " + "Misses: " + misses, 0, 0);

            base.Render();
        }
    }
}
