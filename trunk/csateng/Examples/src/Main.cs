#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2011 mjt[matola@sci.fi]
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing details.
 */
#endregion
using System;
using OpenTK;

namespace CSatEng
{
    class MainClass
    {
        [STAThread]
        static void Main()
        {
            Log.Open("log.txt");
            Settings.ReadXML("settings.xml");
            BaseGame game;
           
            using (GameLoop gameLoop = new GameLoop("Project XYZ", false))
            {
                game = new Test2D();
                gameLoop.SetGame(game);
                gameLoop.Run(60.0);
            }
            using (GameLoop gameLoop = new GameLoop("Project XYZ", false))
            {
                game = new TestParticles();
                gameLoop.SetGame(game);
                gameLoop.Run(60.0);
            }
            using (GameLoop gameLoop = new GameLoop("Project XYZ", false))
            {
                game = new TestPath();
                gameLoop.SetGame(game);
                gameLoop.Run(60.0);
            }
            using (GameLoop gameLoop = new GameLoop("Project XYZ", false))
            {
                game = new TestMD5();
                gameLoop.SetGame(game);
                gameLoop.Run(60.0);
            }

            Log.WriteLine("Exiting..");
#if DEBUG
            Console.ReadKey();
#endif

        }
    }
}
