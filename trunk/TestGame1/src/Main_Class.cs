#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2008-2014 mjt
 * This notice may not be removed from any source distribution.
 * See csat-license.txt for licensing details.
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

            int version;
            GraphicsContextFlags flags;
            if (Settings.UseGL3 == false)
            {
                flags = GraphicsContextFlags.Default;
                version = 2;
            }
            else
            {
                flags = GraphicsContextFlags.ForwardCompatible;
                version = 3;
            }
#if DEBUG
            flags |= GraphicsContextFlags.Debug;
#endif

            using (BaseGame bgame = new BaseGame("Project XYZ", version, 0, flags))
            {
                BaseGame.Instance.WindowBorder = OpenTK.WindowBorder.Fixed;

#if !DEBUG
                try
#endif
                {
                    GameClass game = new GameMenu();
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
