// csat test game

using DragonOgg.Interactive;
using DragonOgg.MediaPlayer;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace CSatEng
{
    class Game : GameClass
    {
        Texture2D man;
        ObjData room;

        public override void Init()
        {
            Settings.TextureDir = "../../data/texture/";
            man = Texture2D.Load("head.png");
            font = BitmapFont.Load("fonts/comic12.png");

            room = ObjData.Load("../../data/bg0.obj");
            x = room.SX;
            y = room.SY;

            Camera.Set2D();
            base.Init();
        }

        public override void Dispose()
        {
            //room.Dispose(); TODO
            man.Dispose();
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

            room.PointInPolygon(Mouse.X, Settings.Height - Mouse.Y);



            base.Update(time);
        }

        float x, y;
        public override void Render()
        {
            GL.Clear(ClearFlags);

            if (Mouse[MouseButton.Left])
            {
            }

            room.Draw();

            man.Draw((int)x, (int)y, 0, 1, 1, true);


            font.Write("Hellurei", 0, 0);

            base.Render();
        }
    }
}
