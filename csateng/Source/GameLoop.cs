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
        public static ClearBufferMask ClearFlags = ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit;
        public static BaseGame Game;

        public GameLoop(string projectName, bool hideMouseCursor)
            : base(Settings.Width, Settings.Height, new GraphicsMode(Settings.Bpp, Settings.DepthBpp, 0, Settings.FSAA, 0, 2, false), projectName)
        {
            Log.WriteLine("CSatEng 0.8 log // (c) mjt, 2011");
            Log.WriteLine("OS: " + System.Environment.OSVersion.ToString());
            Log.WriteLine("Renderer: " + GL.GetString(StringName.Renderer));
            Log.WriteLine("Vendor: " + GL.GetString(StringName.Vendor));
            Log.WriteLine("Version: " + GL.GetString(StringName.Version));
            Log.WriteLine(".Net: " + Environment.Version);

            string version = GL.GetString(StringName.Version);
            int major = (int)version[0];
            int minor = (int)version[2];
            if (major <= 1 && minor < 5) Util.Error("VBOs not supported. You need at least OpenGL 1.5.");

            Log.WriteLine("--------------------------------------------");
            Log.WriteLine("Extensions:\n" + GL.GetString(StringName.Extensions));
            Log.WriteLine("--------------------------------------------");

            if (GL.GetString(StringName.Extensions).Contains("texture_non_power_of_two"))
            {
                Log.WriteLine("NPOT supported.");
                Texture.IsNPOTSupported = true;
            }
            else
            {
                Log.WriteLine("NPOT not supported.");
                Texture.IsNPOTSupported = false;
            }

            // tarkista voidaanko shadereita käyttää.
            if (GL.GetString(StringName.Extensions).Contains("vertex_shader") &&
                GL.GetString(StringName.Extensions).Contains("fragment_shader"))
            {
                GLSLShader.IsSupported = true;
                Log.WriteLine("Shaders supported.");
            }
            else
            {
                GLSLShader.IsSupported = false;
                Log.WriteLine("Shaders not supported.");
            }

            if (GL.GetString(StringName.Extensions).Contains("EXT_framebuffer_object"))
            {
                FBO.IsSupported = true;
                Log.WriteLine("FBOs supported.");
            }
            else
            {
                FBO.IsSupported = false;
                Log.WriteLine("FBOs not supported.");
            }

            GL.GetInteger(GetPName.MaxCombinedTextureImageUnits, out Texture.MaxTextures);
            Log.WriteLine("Max textureUnits: " + Texture.MaxTextures);
            Texture.BindedTextures = new uint[Texture.MaxTextures];

            VSync = Settings.VSync ? VSyncMode.On : VSyncMode.Off;
            Settings.Device = DisplayDevice.Default;
            if (Settings.FullScreen)
            {
                Settings.Device.ChangeResolution(Settings.Device.SelectResolution(Settings.Width, Settings.Height, Settings.Bpp, 60f));
                WindowState = OpenTK.WindowState.Fullscreen;
            }

            GL.Enable(EnableCap.DepthTest);
            GL.ClearDepth(1.0);
            GL.DepthFunc(DepthFunction.Lequal);
            GL.ClearColor(0.0f, 0.0f, 0.2f, 1);
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);
            GL.Enable(EnableCap.PolygonSmooth);
            GL.Hint(HintTarget.PolygonSmoothHint, HintMode.Nicest);
            GL.ShadeModel(ShadingModel.Smooth);
            GL.Enable(EnableCap.Lighting);
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            GL.PolygonMode(MaterialFace.Front, PolygonMode.Fill);
            GL.FrontFace(FrontFaceDirection.Ccw);
            GL.Enable(EnableCap.ColorMaterial);
            GL.Enable(EnableCap.Normalize);
            BaseGame.Keyboard = Keyboard;
            BaseGame.Mouse = Mouse;

            if (hideMouseCursor)
            {
                //CursorVisible = false; // OTK bug - TODO: ota käyttöön kun toimii oikein
                System.Windows.Forms.Cursor.Hide();
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            try
            {
                BitmapFont font = BitmapFont.Load("fonts/comic12.png");
                Texture.UnBind(0);
                Camera.Set2D();
                font.Write("Loading...", 0, 0);
                font.Dispose();
            }
            catch (Exception) { }
            SwapBuffers();
        }


        public static void SetGame(BaseGame game)
        {
            Game = game;
            game.Init();
        }

        public override void Dispose()
        {
            if (Settings.FullScreen) Settings.Device.RestoreResolution();
            if (Game != null) Game.Dispose();
            Game = null;
            base.Dispose();
        }

        protected override void OnResize(EventArgs e)
        {
            if (Game == null) return;
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
            if (Game == null) return;

            Game.Update((float)e.Time);

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
            if (Game == null) return;
            Settings.NumOfObjects = 0;
            Game.Render();
            SwapBuffers();

#if DEBUG
            this.Title = "Test project [objs: " + Settings.NumOfObjects + "]   FPS: " + (1 / e.Time).ToString("0.");
#endif

        }
    }
}
