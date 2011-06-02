#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2011 mjt[matola@sci.fi]
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing details.
 */
#endregion
using System;
using OpenTK.Input;

namespace CSatEng
{

    public class BaseGame
    {
        public static readonly int DIFFUSE_TEXUNIT = 0;
        public static readonly int LIGHTMAP_TEXUNIT = 1;
        public static readonly int BUMP_TEXUNIT = 2;
        public static readonly int LIGHTMASK_TEXUNIT = 6;
        public static readonly int SHADOW_TEXUNIT = 7;
        public static readonly int DEPTH_TEXUNIT = SHADOW_TEXUNIT;

        public static Random Rnd = new Random();
        public static KeyboardDevice Keyboard;
        public static MouseDevice Mouse; //public static OpenTK.Input.Mouse mouse;

        protected Sky skybox;
        protected static ShadowMapping shadows;
        protected FBO fbo;
        protected SceneNode world = new SceneNode("World");
        protected Camera camera = new Camera();
        protected BitmapFont font = null;
        protected int oldMouseX, oldMouseY;
        protected bool mouseLeftPressed = false, mouseRightPressed = false, mouseMiddlePressed = false;

        public BaseGame()
        {
            GameLoop.Running = true;
        }

        public virtual void Dispose()
        {
            if (font != null) font.Dispose();
            if (fbo != null) fbo.Dispose();
            if (skybox != null) skybox.Dispose();
            skybox = null;
            fbo = null;
            font = null;
            world = null;
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
            Light.Lights.Clear();
            Path.Paths.Clear();
            world = null;
            world = new SceneNode("World");
            SceneNode.ObjectCount = 0;
            for (int q = 0; q < Texture.MaxTextures; q++) Texture.UnBind(q);

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
