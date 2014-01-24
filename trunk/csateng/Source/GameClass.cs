#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2008-2014 mjt
 * This notice may not be removed from any source distribution.
 * See csat-license.txt for licensing details.
 */
#endregion
using System;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace CSatEng
{
    public class GameClass
    {
        public static Random Rnd = new Random();
        public static KeyboardDevice Keyboard;
        public static MouseDevice Mouse;
        public static ClearBufferMask ClearFlags = ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit;
        public static int NumOfObjects = 0;

        protected Sky skybox;
        protected FBO colorFBO, depthFBO;
        protected Renderable world = new Renderable("World");
        protected Camera camera = new Camera();
        protected BitmapFont font = null;
        protected int oldMouseX, oldMouseY;

        public GameClass()
        {
            BaseGame.Running = true;
        }

        public virtual void Dispose()
        {
            if (font != null) font.Dispose();
            if (colorFBO != null) colorFBO.Dispose();
            if (depthFBO != null) depthFBO.Dispose();
            if (skybox != null) skybox.Dispose();
            skybox = null;
            colorFBO = null;
            depthFBO = null;
            font = null;
            world = null;
            Node.ObjectCount = 0;
            GLSLShader.UnBindShader();
            GLSLShader.SetShader("default.shader", "");
        }

        /// <summary>
        /// putsaa listat ja poista gl-datat
        /// </summary>
        public void ClearArrays()
        {
            Texture.DisposeAll();
            GLSLShader.DisposeAll();
            Material.DisposeAll();
            Particles.DisposeAll();

            Light.Lights.Clear();
            Path.Paths.Clear();

            world = null;
            world = new Renderable("World");
            Node.ObjectCount = 0;
            for (int q = 0; q < Texture.MaxTextures; q++) Texture.UnBind(q);
            GLSLShader.UnBindShader();

            GLSLShader.SetShader("default.shader", "");
            
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public virtual void Init()
        {
        }

        public virtual void Update(float time)
        {
            oldMouseX = Mouse.X;
            oldMouseY = Mouse.Y;
        }

        public virtual void Render()
        {
        }

    }

}
