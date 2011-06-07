#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2011 mjt[matola@sci.fi]
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing details.
 */
#endregion
using System;
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;

namespace CSatEng
{

    public class BaseGame
    {
        public static Random Rnd = new Random();
        public static KeyboardDevice Keyboard;
        public static MouseDevice Mouse; //public static OpenTK.Input.Mouse mouse;

        protected Sky skybox;
        protected static ShadowMapping shadows;
        protected FBO colorFBO, depthFBO, depthColorFBO;
        protected SceneNode world = new SceneNode("World");
        protected Camera camera = new Camera();
        protected BitmapFont font = null;
        protected int oldMouseX, oldMouseY;
        protected bool mouseLeftPressed = false, mouseRightPressed = false, mouseMiddlePressed = false;

        public BaseGame()
        {
            Particles.SoftParticles = false;
            GameLoop.Running = true;
        }

        public virtual void Dispose()
        {
            if (font != null) font.Dispose();
            if (colorFBO != null) colorFBO.Dispose();
            if (depthFBO != null) depthFBO.Dispose();
            if (depthColorFBO != null) depthColorFBO.Dispose();
            if (skybox != null) skybox.Dispose();
            skybox = null;
            colorFBO = null;
            depthFBO = null;
            depthColorFBO = null;
            font = null;
            world = null;
            GLSLShader.UseProgram(0);
            SceneNode.ObjectCount = 0;
        }

        /// <summary>
        /// putsaa listat ja poista gl-datat
        /// </summary>
        public void ClearArrays()
        {
            Texture.DisposeAll();
            GLSLShader.DisposeAll();
            MaterialInfo.DisposeAll();
            Particles.DisposeAll();

            Light.Lights.Clear();
            Path.Paths.Clear();

            world = null;
            world = new SceneNode("World");
            SceneNode.ObjectCount = 0;
            for (int q = 0; q < Texture.MaxTextures; q++) Texture.UnBind(q);
            Texture.UnBind(0);

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public virtual void Init()
        {
        }

        public virtual void Update(float time)
        {
            if (Mouse[MouseButton.Left]) mouseLeftPressed = true; else mouseLeftPressed = false;
            if (Mouse[MouseButton.Right]) mouseRightPressed = true; else mouseRightPressed = false;
            if (Mouse[MouseButton.Middle]) mouseMiddlePressed = true; else mouseMiddlePressed = false;

            oldMouseX = Mouse.X;
            oldMouseY = Mouse.Y;
        }

        public virtual void Render()
        {
        }

    }

}
