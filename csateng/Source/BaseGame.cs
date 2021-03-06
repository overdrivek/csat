﻿#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2008-2014 mjt
 * This notice may not be removed from any source distribution.
 * See csat-license.txt for licensing details.
 */
#endregion
using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace CSatEng
{
    public class BaseGame : GameWindow
    {
        public static bool Running = true;
        public static GameClass Game;
        public static GameWindow Instance;

        public BaseGame(string projectName, int glVersionMajor, int glVersionMinor, GraphicsContextFlags flags)
            : base(Settings.Width, Settings.Height,
            new GraphicsMode(Settings.Bpp, Settings.DepthBpp, 0, Settings.FSAA, 0, 2, false),
            projectName, 0, DisplayDevice.Default,
            glVersionMajor, glVersionMinor,
            flags)
        {
            Init();
            Running = true;
        }

        void Init()
        {
            Instance = this;

            Log.WriteLine("CSatEng 0.9.2 // (c) mjt, 2008-2014");
            Log.WriteLine("OS: " + System.Environment.OSVersion.ToString());
            Log.WriteLine("Renderer: " + GL.GetString(StringName.Renderer));
            Log.WriteLine("Vendor: " + GL.GetString(StringName.Vendor));
            Log.WriteLine("Version: " + GL.GetString(StringName.Version));
            Log.WriteLine(".Net: " + Environment.Version);

            string version = GL.GetString(StringName.Version);
            if (version.Contains("Compatibility")) Settings.UseGL3 = false;
            int major = (int)version[0];
            int minor = (int)version[2];
            if (major < 2) Log.Error("You need at least OpenGL 2.0 to run this program. Please update your drivers.");

            string ext = "";
            if (Settings.UseGL3 == false) ext = GL.GetString(StringName.Extensions);
            else
            {
                int extC;
                GL.GetInteger(GetPName.NumExtensions, out extC);
                for (int q = 0; q < extC; q++) ext += GL.GetString(StringNameIndexed.Extensions, q) + " ";
            }

            Log.WriteLine("--------------------------------------------");
            Log.WriteLine("Extensions:\n" + ext);
            Log.WriteLine("--------------------------------------------");

            if (ext.Contains("texture_non_power_of_two"))
            {
                if (Settings.DisableNPOTTextures)
                {
                    Log.WriteLine("NPOT textures supported but disabled.");
                    Texture.IsNPOTSupported = false;
                }
                else
                {
                    Log.WriteLine("NPOT textures supported.");
                    Texture.IsNPOTSupported = true;
                }
            }
            else
            {
                Log.WriteLine("NPOT textures not supported.");
                Texture.IsNPOTSupported = false;
            }

            // löytyykö float texture extension
            if (ext.Contains("texture_float") && ext.Contains("color_buffer_float"))
            {
                if (Settings.DisableFloatTextures)
                {
                    Log.WriteLine("Float textures supported but disabled.");
                    Texture.IsFloatTextureSupported = false;
                }
                else
                {
                    Log.WriteLine("Float textures supported.");
                    Texture.IsFloatTextureSupported = true;
                }
            }
            else
            {
                Log.WriteLine("Float textures not supported.");
                Texture.IsFloatTextureSupported = false;
            }

            // tarkista voidaanko shadereita käyttää.
            if (!ext.Contains("vertex_shader") || !ext.Contains("fragment_shader"))
                Log.Error("Shaders not supported. Please update your drivers.");

            if (ext.Contains("EXT_framebuffer_object"))
            {
                if (Settings.DisableFbo)
                {
                    Log.WriteLine("FBOs supported but disabled.");
                    FBO.IsSupported = false;
                }
                else
                {
                    Log.WriteLine("FBOs supported.");
                    FBO.IsSupported = true;
                }
            }
            else
            {
                Log.WriteLine("FBOs not supported.");
                FBO.IsSupported = false;
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
            GL.ClearColor(0.0f, 0.0f, 0.1f, 0);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            if (Settings.UseGL3 == false)
            {
                GL.Enable(EnableCap.Texture2D);
                GL.Enable(EnableCap.ColorMaterial);
                GL.Enable(EnableCap.Normalize);
            }
            GameClass.Keyboard = Keyboard;
            GameClass.Mouse = Mouse;

            GLSLShader.SetShader("default.shader", "");

            Loading();
        }

        void Loading()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            try
            {
                BitmapFont font = BitmapFont.Load("fonts/comic12.png");
                Texture.UnBind(0);
                Camera.Set2D();
                font.Write("Loading...", 0, 0);
                font.Dispose();
                Texture.UnBind(0);
            }
            catch (Exception) { }
            SwapBuffers();
        }

        public static void SetGame(GameClass game)
        {
            if (Game != null)
                Game.Dispose();

            Game = game;
            if (game != null)
                game.Init();
        }

        public override void Dispose()
        {
            if (Settings.FullScreen) Settings.Device.RestoreResolution();
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
                Game = null;
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
            if (Game == null || Running == false) return;
            GameClass.NumOfObjects = 0;
            Game.Render();
            SwapBuffers();

#if DEBUG
            this.Title = "Test project [objs: " + GameClass.NumOfObjects + "]   FPS: " + (1 / e.Time).ToString("0.");
#endif
        }
    }
}
