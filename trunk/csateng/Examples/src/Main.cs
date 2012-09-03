#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2008-2012 mjt
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing details.
 */
#endregion
using System;
using OpenTK.Graphics;

namespace CSatEng
{
    class MainClass
    {
        [STAThread]
        static void Main()
        {
            Log.Create("log.txt");
            Settings.ReadXML("settings.xml");

            Settings.ModelDir = "../../data/model/";
            Settings.TextureDir = "../../data/texture/";
            Settings.ShaderDir = "../../data/shader/";
            Settings.ParticleDir = "../../data/particles/";

            GraphicsContextFlags flags;
            if (Settings.UseGL3 == false) flags = GraphicsContextFlags.Default;
            else flags = GraphicsContextFlags.ForwardCompatible;

#if DEBUG
            flags |= GraphicsContextFlags.Debug;
#endif

            using (BaseGame bgame = new BaseGame("Project XYZ", 3, 0, flags))
            {
#if !DEBUG
                try
#endif
                {
                    GameClass game = new Tests();
                    BaseGame.SetGame(game);
                    bgame.Run(120.0);
                }
#if !DEBUG
                catch (Exception e)
                {
                    Log.WriteLine("Main(): " + e.ToString());
                }
#endif
            }

            Log.WriteLine("Exiting..");
#if DEBUG
            //Console.ReadKey();
#endif
        }
    }

    public class Tests : GameClass
    {
        GameClass game;
        int testNo = 1;

        public static bool NextTest = true;

        public override void Update(float time)
        {
            if (NextTest == true && Keyboard[OpenTK.Input.Key.Escape] == false) // jos testissä painettu ESC
            {
                if (game != null) game.Dispose();

                //testNo = Rnd.Next(5) + 1; // debug

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
                        game = new TestAnimation();
                        game.Init();
                        break;
                    case 5:
                        game = new TestSoftParticles();
                        game.Init();
                        break;

                    case 6:
                        BaseGame.Running = false;
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
