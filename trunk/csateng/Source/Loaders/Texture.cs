#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2011 mjt[matola@sci.fi]
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing details.
 */
#endregion
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
        public static uint[] BindedTextures;
        public static bool IsNPOTSupported = false;
        public static bool IsFloatTextureSupported = false;
        public static int MaxTextures;

        protected string textureName;
        public uint TextureID;
        public int Width, Height, RealWidth, RealHeight;
        public TextureTarget Target = TextureTarget.Texture2D;

        /// <summary>
        /// aseta texture haluttuun textureunittiin
        /// </summary>
        /// <param name="textureUnit"></param>
        public void Bind(int textureUnit)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + textureUnit);
            if (Texture.BindedTextures[textureUnit] == TextureID) return; // on jo bindattu
            Texture.BindedTextures[textureUnit] = TextureID;
            GL.BindTexture(Target, TextureID);
        }

        public static void Bind(int textureUnit, TextureTarget target, uint textureID)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + textureUnit);
            if (BindedTextures[textureUnit] == textureID) return; // on jo bindattu
            BindedTextures[textureUnit] = textureID;
            GL.BindTexture(target, textureID);
        }

        /// <summary>
        /// poista texture käytöstä tietystä textureunitista
        /// </summary>
        public static void UnBind(int textureUnit)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + textureUnit);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            BindedTextures[textureUnit] = 0;
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

        public static Texture Load(string fileName)
        {
            return Load(fileName, true);
        }

        /// <summary>
        /// lataa texture
        /// </summary>
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
            TextureTarget target = TextureTarget.Texture2D;

            try
            {

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

            int pwidth, pheight;
            tex.Bind(0);
            GL.GetTexLevelParameter(TextureTarget.Texture2D, 0, GetTextureParameter.TextureWidth, out pwidth);
            GL.GetTexLevelParameter(TextureTarget.Texture2D, 0, GetTextureParameter.TextureHeight, out pheight);
            tex.Width = pwidth;
            tex.Height = pheight;
            tex.Target = target;

            if (fileName.Contains(".dds")) // dds tiedostoja ei skaalata ^2 kokoon
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
            UnBind(0);
            return tex;
        }

    }
    /// <summary>
    /// texture2d luokka renderoi 2d-kuvat
    /// </summary>
    public class Texture2D : Texture
    {
        public static float AlphaMin = 0.1f;
        public Vector2 Scale = new Vector2(1, 1);
        public VBO Vbo = null;

        void CreateVBO()
        {
            if (Vbo != null) Vbo.Dispose();

            ushort[] ind = new ushort[] { 0, 1, 3, 1, 2, 3 };
            int w = RealWidth / 2;
            int h = RealHeight / 2;
            Vertex[] vert =
            {
                new Vertex(new Vector3(-w, -h, 0), new Vector3(0, 0, 1), new Vector2(0,0)),
                new Vertex(new Vector3(-w, h, 0), new Vector3(0, 0, 1), new Vector2(0,1)),
                new Vertex(new Vector3(w, h, 0), new Vector3(0, 0, 1), new Vector2(1,1)),
                new Vertex(new Vector3(w, -h, 0), new Vector3(0, 0, 1), new Vector2(1,0))
            };
            Vbo = new VBO();
            Vbo.DataToVBO(vert, ind, VBO.VertexMode.UV1);
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
            tex.Target = t.Target;
            tex.CreateVBO();
            return tex;
        }

        public static Texture2D CreateDrawableTexture(int width, int height, uint textureID)
        {
            Texture2D tex = new Texture2D();
            tex.Width = width;
            tex.Height = height;
            tex.RealWidth = width;
            tex.RealHeight = height;
            tex.Target = TextureTarget.Texture2D;
            tex.TextureID = textureID;
            tex.CreateVBO();
            return tex;
        }

        public void Draw(int x, int y, float rotate, float sx, float sy, bool blend)
        {
            if (Vbo == null) CreateVBO();
            Bind(0);
            GL.PushAttrib(AttribMask.ColorBufferBit | AttribMask.EnableBit | AttribMask.PolygonBit);
            GL.Disable(EnableCap.Lighting);
            GL.Disable(EnableCap.DepthTest);
            GL.PushMatrix();
            {
                GL.Translate(x + RealWidth / 2, Settings.Height - y - RealHeight / 2, 0);
                GL.Rotate(rotate, 0, 0, 1);
                GL.Scale(sx, sy, 1);
                if (blend)
                {
                    GL.Enable(EnableCap.Blend);
                    GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
                    GL.Enable(EnableCap.AlphaTest);
                    GL.AlphaFunc(AlphaFunction.Greater, AlphaMin);
                }
                Vbo.Render();
            }
            GL.PopMatrix();
            GL.PopAttrib();
        }

        public void DrawFullScreen(int x, int y, float rotate)
        {
            float sx = (float)Settings.Width / (float)RealWidth, sy = (float)Settings.Height / (float)RealHeight;
            if (Vbo == null) CreateVBO();
            Bind(0);
            GL.PushAttrib(AttribMask.ColorBufferBit | AttribMask.EnableBit | AttribMask.PolygonBit);
            GL.Disable(EnableCap.Lighting);
            GL.Disable(EnableCap.DepthTest);
            GL.PushMatrix();
            {
                GL.Translate(sx * (x + RealWidth / 2), Settings.Height + sy * (y - RealHeight / 2), 0);
                GL.Rotate(rotate, 0, 0, 1);
                GL.Scale(sx, sy, 1);
                Vbo.Render();
            }
            GL.PopMatrix();
            GL.PopAttrib();
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
