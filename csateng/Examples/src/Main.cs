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

namespace CSatEng
{
    class MainClass
    {
        [STAThread]
        static void Main()
        {
            Log.Open("log.txt");
            Settings.ReadXML("settings.xml");
            using (GameLoop gameLoop = new GameLoop("Project XYZ", false))
            {
                try
                {
                    BaseGame game = new Tests();
                    GameLoop.SetGame(game);
                    gameLoop.Run(60.0);
                }
                catch (Exception e)
                {
                    Log.WriteLine("Main(): " + e.ToString());
                }
            }

            Log.WriteLine("Exiting..");
#if DEBUG
            Console.ReadKey();
#endif
        }
    }

    public class Tests : BaseGame
    {
        BaseGame game;
        int testNo = 1;
        public static bool NextTest = true;

        public override void Update(float time)
        {
            if (NextTest == true && Keyboard[OpenTK.Input.Key.Escape] == false) // jos testissä painettu ESC
            {
                if (game != null) game.Dispose();
                switch (testNo)
                {
                    case 1:
                        game = new Test2D();
                        game.Init();
                        break;
                    case 2:
                        game = new TestParticles();
                        game.Init();
                        break;
                    case 3:
                        game = new TestPath();
                        game.Init();
                        break;
                    case 4:
                        game = new TestMD5();
                        game.Init();
                        break;
                    case 5:
                        game = new TestSoftParticles();
                        game.Init();
                        break;

                    case 6:
                        GameLoop.Running = false;
                        game.Dispose();
                        game = null;
                        return;
                }
                testNo++;
                NextTest = false;
            }
            game.Update(time);
            base.Update(time);
        }

        public override void Render()
        {
            if (game != null) game.Render();
            base.Render();
        }
    }
}
