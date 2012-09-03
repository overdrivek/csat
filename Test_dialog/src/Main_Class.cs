#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2008-2012 mjt
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing details.
 */
#endregion
using System;
using System.Windows.Forms;
using OpenTK.Graphics;

namespace CSatEng
{
    class Main_Class
    {
        [STAThread]
        static void Main()
        {
            Log.Create("log.txt");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Menu());

        }
    }
}
