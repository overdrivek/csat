#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2008-2012
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing details.
 */
#endregion
using System;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics;

namespace CSatEng
{
    public partial class Menu : Form
    {
        public Menu()
        {
            InitializeComponent();
        }

        private void Form1_Load(Object sender, EventArgs e)
        {
            listBox1.Items.Add("2D test");
            listBox1.SelectedIndex = 0;

            textBox1.Lines = new string[] { "Blah blah" };

            DisplayDevice dev = DisplayDevice.Default;
            for (int q = 0; q < dev.AvailableResolutions.Count; q++)
            {
                if (dev.AvailableResolutions[q].BitsPerPixel >= 16)
                    comboBox1.Items.Add(dev.AvailableResolutions[q].Width + "x" +
                                        dev.AvailableResolutions[q].Height + "x" +
                                        dev.AvailableResolutions[q].BitsPerPixel);
            }
            int ind = comboBox1.FindString("800x600");
            comboBox1.SelectedIndex = ind;
        }

        // starttaa esimerkki
        private void button1_Click(Object sender, EventArgs e)
        {
            Hide();

            DisplayDevice dev = DisplayDevice.Default;
            int ind = comboBox1.SelectedIndex;

            string[] strs = ((string)(comboBox1.Items[ind])).Split('x');
            Settings.Width = int.Parse(strs[0]);
            Settings.Height = int.Parse(strs[1]);
            Settings.Bpp = int.Parse(strs[2]);

            // fullscreen?
            if (checkBox1.Checked)
                Settings.FullScreen = true;
            else
                Settings.FullScreen = false;

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
                    GameClass game = new Game();
                    BaseGame.SetGame(game);
                    bgame.Run(120.0);
                }
#if !DEBUG
                catch (Exception e)
                {
                    Log.WriteLine(e.ToString());
                }
#endif
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            Show();
        }
    }
}
