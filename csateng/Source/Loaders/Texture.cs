#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2011 mjt[matola@sci.fi]
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing details.
 */
#endregion
/* 
 * texturen lataus.
 * 
 */
using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace CSatEng
{
    /// <summary>
    /// texture luokka lataa kuvan ja luo ogl texturen 
    /// </summary>
    public class Texture
    {
        /// <summary>
        /// texture taulukko jossa kaikki ladatut texturet
        /// </summary>
        public static Dictionary<string, Texture> textures = new Dictionary<string, Texture>();

        public static bool IsNPOTSupported = false;

        protected string textureName;
        public uint TextureID;
        public int Width, Height, RealWidth, RealHeight;

        /// <summary>
        /// aseta texture haluttuun textureunittiin
        /// </summary>
        /// <param name="textureUnit"></param>
        public void Bind(int textureUnit)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + textureUnit);
            GL.BindTexture(TextureTarget.Texture2D, TextureID);
        }

        /// <summary>
        /// poista texture käytöstä tietystä textureunitista
        /// </summary>
        /// <param name="textureUnit"></param>
        public void UnBind(int textureUnit)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + textureUnit);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public virtual void Dispose()
        {
            if (textures.ContainsKey(textureName) && TextureID != 0)
            {
                GL.DeleteTextures(1, ref TextureID);
                textures.Remove(textureName);
                Log.WriteLine("Disposed: " + textureName, true);
            }
            TextureID = 0;
            textureName = "";
        }

        public static void DisposeAll()
        {
            List<string> tex = new List<string>();
            foreach (KeyValuePair<string, Texture> dta in textures) tex.Add(dta.Key);
            for (int q = 0; q < tex.Count; q++) textures[tex[q]].Dispose();
            textures.Clear();
        }

        /// <summary>
        /// lataa texture
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static Texture Load(string fileName)
        {
            return Load(fileName, true);
        }

        public static Texture Load(string fileName, bool useTexDir)
        {
            Texture tex;
            // jos texture on jo ladattu, palauta se
            textures.TryGetValue(fileName, out tex);
            if (tex != null) return tex;

            tex = new Texture();

            tex.textureName = fileName;
            Log.WriteLine("Texture: " + tex.textureName, true);

            if (useTexDir) fileName = Settings.TextureDir + fileName;

            try
            {
                TextureTarget target;
                if (fileName.Contains(".dds")) // jos dds texture
                {
                    ImageDDS.LoadFromDisk(fileName, out tex.TextureID, out target);
                }
                else
                {
                    ImageGDI.LoadFromDisk(fileName, out tex.TextureID, out target);
                }
            }
            catch (Exception e)
            {
                Util.Error(e.ToString());
            }

            float[] pwidth = new float[1];
            float[] pheight = new float[1];
            GL.BindTexture(TextureTarget.Texture2D, tex.TextureID);
            GL.GetTexLevelParameter(TextureTarget.Texture2D, 0, GetTextureParameter.TextureWidth, pwidth);
            GL.GetTexLevelParameter(TextureTarget.Texture2D, 0, GetTextureParameter.TextureHeight, pheight);
            tex.Width = (int)pwidth[0];
            tex.Height = (int)pheight[0];

            if (fileName.Contains(".dds"))
            {
                tex.RealWidth = tex.Width;
                tex.RealHeight = tex.Height;
            }
            else
            {
                tex.RealWidth = ImageGDI.RealWidth;
                tex.RealHeight = ImageGDI.RealHeight;
            }

            textures.Add(tex.textureName, tex);

            return tex;
        }

    }
    /// <summary>
    /// texture2d luokka renderoi 2d-kuvat ja billboardit
    /// </summary>
    public class Texture2D : Texture
    {
        public static float AlphaMin = 0.1f;
        public Vector2 Scale = new Vector2(1, 1);
        public VBO Vbo = null;

        void CreateVBO()
        {
            int[] ind = new int[] { 0, 1, 3, 1, 2, 3 };
            int w = RealWidth / 2;
            int h = RealHeight / 2;

            Vector3[] vs = new Vector3[]
			{
				new Vector3(-w, -h, 0),
				new Vector3(-w, h, 0),
				new Vector3(w, h, 0),
				new Vector3(w, -h, 0)
			};

            Vector2[] uv = new Vector2[]
			{
				new Vector2(0,0),
				new Vector2(0,1),
				new Vector2(1,1),
				new Vector2(1,0)
			};

            Vector3[] norm = new Vector3[]
			{
				new Vector3(0, 0, 1),
				new Vector3(0, 0, 1),
				new Vector3(0, 0, 1),
				new Vector3(0, 0, 1)
			};

            Vbo = new VBO();
            Vbo.DataToVBO(vs, ind, norm, uv);

            // scale
            Scale.X = 1;
            Scale.Y = 1;
        }

        public static new Texture2D Load(string fileName)
        {
            Texture t = Texture.Load(fileName);
            Texture2D tex = new Texture2D();
            tex.textureName = fileName;
            tex.TextureID = t.TextureID;
            tex.Width = t.Width;
            tex.Height = t.Height;
            tex.RealWidth = t.RealWidth;
            tex.RealHeight = t.RealHeight;
            tex.CreateVBO();
            return tex;
        }

        public void CreateDrawableTexture(int width, int height, uint textureID)
        {
            Width = width;
            Height = height;
            RealWidth = width;
            RealHeight = height;
            CreateVBO();
            TextureID = textureID;
        }

        public void Draw(int x, int y, float rotate, float sx, float sy, bool blend)
        {
            if (Vbo == null) CreateVBO();
            GL.PushMatrix();
            GL.Translate(x + RealWidth / 2, Settings.Height - y - RealHeight / 2, 0);
            GL.Rotate(rotate, 0, 0, 1);
            GL.Scale(sx, sy, 1);
            GL.PushAttrib(AttribMask.ColorBufferBit | AttribMask.EnableBit | AttribMask.PolygonBit);
            GL.Disable(EnableCap.Lighting);
            if (blend)
            {
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
                GL.Enable(EnableCap.AlphaTest);
                GL.AlphaFunc(AlphaFunction.Greater, AlphaMin);
            }
            GL.BindTexture(TextureTarget.Texture2D, TextureID);
            Vbo.Render();
            GL.PopAttrib();
            GL.PopMatrix();
        }

        public void DrawFullScreen(int x, int y, float rotate)
        {
            float sx = (float)Settings.Width / (float)RealWidth, sy = (float)Settings.Height / (float)RealHeight;
            if (Vbo == null) CreateVBO();
            GL.PushMatrix();
            GL.Translate(sx * (x + RealWidth / 2), Settings.Height + sy * (y - RealHeight / 2), 0);
            GL.Rotate(rotate, 0, 0, 1);
            GL.Scale(sx, sy, 1);
            GL.PushAttrib(AttribMask.ColorBufferBit | AttribMask.EnableBit | AttribMask.PolygonBit);
            GL.Disable(EnableCap.Lighting);
            GL.BindTexture(TextureTarget.Texture2D, TextureID);
            Vbo.Render();
            GL.PopAttrib();
            GL.PopMatrix();
        }

        public void DrawFullScreen(int x, int y)
        {
            DrawFullScreen(x, y, 0);
        }
        public void Draw(int x, int y)
        {
            Draw(x, y, 0, 1, 1, true);
        }
        public void DrawNoBlending(int x, int y)
        {
            Draw(x, y, 0, 1, 1, false);
        }

        public override void Dispose()
        {
            if (Vbo != null)
            {
                Vbo.Dispose();
                Vbo = null;
                Log.WriteLine("Disposed: Texture2D", true);
            }
            base.Dispose();
        }

    }
}
