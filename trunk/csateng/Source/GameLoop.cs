#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2011 mjt[matola@sci.fi]
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing details.
 */
#endregion
using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;

namespace CSatEng
{
    public class GameLoop : GameWindow
    {
        public static bool Running = true;
        BaseGame game;

        public GameLoop(string projectName, bool hideMouseCursor)
            : base(Settings.Width, Settings.Height, new GraphicsMode(Settings.Bpp, 0, 0, Settings.FSAA), projectName)
        {
            Log.WriteLine("CSatEng 0.8 log   // (c) mjt, 2011 [build " + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day + "]");
            Log.WriteLine("OS: " + System.Environment.OSVersion.ToString());
            Log.WriteLine("Renderer: " + GL.GetString(StringName.Renderer));
            Log.WriteLine("Vendor: " + GL.GetString(StringName.Vendor));
            Log.WriteLine("Version: " + GL.GetString(StringName.Version));
            Log.WriteLine(".Net: " + Environment.Version);
            Log.WriteLine("--------------------------------------------");
            Log.WriteLine("Extensions:\n" + GL.GetString(StringName.Extensions));
            Log.WriteLine("--------------------------------------------");

            if (GL.GetString(StringName.Extensions).Contains("texture_non_power_of_two"))
            {
                Log.WriteLine("NPOT supported.");
                Texture.IsNPOTSupported = true;
            }
            else Log.WriteLine("NPOT not supported.");

            VSync = Settings.VSync ? VSyncMode.On : VSyncMode.Off;
            Settings.Device = DisplayDevice.Default;
            if (Settings.FullScreen)
            {
                Settings.Device.ChangeResolution(Settings.Device.SelectResolution(Settings.Width, Settings.Height, Settings.Bpp, 60f));
                WindowState = OpenTK.WindowState.Fullscreen;
            }

            GL.ClearColor(System.Drawing.Color.Black);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Lequal);
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);
            GL.Enable(EnableCap.PolygonSmooth);
            GL.Hint(HintTarget.PolygonSmoothHint, HintMode.Nicest);
            GL.ShadeModel(ShadingModel.Smooth);
            GL.Enable(EnableCap.Lighting);
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.ColorMaterial);

            BaseGame.Keyboard = Keyboard;
            BaseGame.Mouse = Mouse;

            if (hideMouseCursor)
            {
                //CursorVisible = false; // OTK bug - TODO: ota käyttöön kun toimii oikein
                System.Windows.Forms.Cursor.Hide();
            }
        }

        public void SetGame(BaseGame game)
        {
            this.game = game;
            game.Init();
        }

        public override void Dispose()
        {
            if (Settings.FullScreen) Settings.Device.RestoreResolution();
            if (game != null) game.Dispose();
            game = null;
            base.Dispose();
        }

        protected override void OnResize(EventArgs e)
        {
            if (game == null) return;
            Settings.Width = Width;
            Settings.Height = Height;
            Camera.Resize();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            if (Running == false)
            {
                this.Exit();
                return;
            }
            if (game == null) return;

            game.Update((float)e.Time);

            if (Keyboard[Key.AltLeft] && Keyboard[Key.Enter])
            {
                if (this.WindowState == WindowState.Fullscreen)
                    this.WindowState = WindowState.Normal;
                else
                    this.WindowState = WindowState.Fullscreen;
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            if (game == null) return;

            if (Sky.IsSky) GL.Clear(ClearBufferMask.DepthBufferBit); // ei putsata colorbufferia kun skybox piirretään ekana
            else GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Settings.NumOfObjects = 0;

            game.Render();

            SwapBuffers();

#if DEBUG
            this.Title = "Test project [objs: " + Settings.NumOfObjects + "]   FPS: " + (1 / e.Time).ToString("0.");
#endif

        }
    }
}
