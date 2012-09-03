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
    class Main_Class
    {
        [STAThread]
        static void Main()
        {
            Log.Create("log.txt");
            Settings.ReadXML("settings.xml");

            GraphicsContextFlags flags;
            if (Settings.UseGL3 == false) flags = GraphicsContextFlags.Default;
            else flags = GraphicsContextFlags.ForwardCompatible;

#if DEBUG
            flags |= GraphicsContextFlags.Debug;
#endif

            using (BaseGame bgame = new BaseGame("Project XYZ", 3, 0, flags))
            {
                BaseGame.Instance.WindowBorder = OpenTK.WindowBorder.Fixed;

#if !DEBUG
                try
#endif
                {
                    GameClass game = new Game();
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
}
