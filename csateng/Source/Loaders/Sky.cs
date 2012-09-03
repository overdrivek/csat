#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2008-2012 mjt
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing details.
 */
#endregion
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace CSatEng
{
    public class Sky : Renderable
    {
        OgreMesh[] skyboxSides = new OgreMesh[6];

        public override void Dispose()
        {
            if (Name != "")
            {
                for (int q = 0; q < 6; q++) skyboxSides[q].Dispose();
                Name = "";
            }
        }

        public void SetSkyShader(string shaderFileName)
        {
            GetList(true);
            for (int q = 0; q < ObjList.Count; q++)
            {
                OgreMesh m = ObjList[q] as OgreMesh;
                m.Vbo.Shader = GLSLShader.Load(shaderFileName);
            }
        }

        /// <summary>
        /// lataa skybox (ei cubemap). skyName on nimen alkuosa eli esim plainsky_  jos tiedostot on plainsky_front.jpg, plainsky_back.jpg jne
        /// ext on tiedoston p‰‰te eli esim jpg, dds
        /// </summary>
        public static Sky Load(string skyName, string ext)
        {
            Sky sky = new Sky();
            string[] sideStr = { "top", "bottom", "left", "right", "front", "back" };
            Node skyNode = new Node();
            DotScene ds = DotScene.Load("sky/sky.scene", skyNode);

            int side = 0;
            TextureLoaderParameters.WrapModeS = TextureWrapMode.ClampToEdge;
            TextureLoaderParameters.WrapModeT = TextureWrapMode.ClampToEdge;
            TextureLoaderParameters.FlipImages = false;
            skyNode.GetList(true);
            for (int q = 0; q < ObjList.Count; q++)
            {
                OgreMesh m = ObjList[q] as OgreMesh;

                if (m != null)
                {
                    sky.skyboxSides[side] = m;
                    m.Boundings = null;
                    m.CastShadow = false;

                    string fileName = skyName + sideStr[side] + "." + ext;
                    m.MaterialName = fileName;
                    m.Material = Material.GetMaterial(fileName + "_material");
                    m.Material.Textures[Settings.COLOR_TEXUNIT].Tex = Texture.Load(m.MaterialName);
                    side++;
                }
            }
            TextureLoaderParameters.FlipImages = true;
            TextureLoaderParameters.WrapModeS = TextureWrapMode.Repeat;
            TextureLoaderParameters.WrapModeT = TextureWrapMode.Repeat;

            return sky;
        }

        public override void Render()
        {
            base.Render(); // renderoi objektin ja kaikki siihen liitetyt objektit
        }

        protected override void RenderModel()
        {
            if (VBO.FastRenderPass) return;
            GameClass.NumOfObjects++;

            GL.Disable(EnableCap.DepthTest);
            GL.DepthMask(false); // ei kirjoiteta z-bufferiin
            GLExt.SetLighting(false);

            GLExt.PushMatrix();
            GLExt.ModelViewMatrix.Row3.X = GLExt.ModelViewMatrix.Row3.Y = GLExt.ModelViewMatrix.Row3.Z = 0;
            GLExt.Scale(10, 10, 10);
            for (int q = 0; q < 6; q++)
            {
                OgreMesh m = skyboxSides[q];
                m.Material.SetMaterial();
                m.Vbo.Render();
            }
            GLExt.PopMatrix();

            GLExt.SetLighting(true);
            GL.DepthMask(true);
            GL.Enable(EnableCap.DepthTest);
        }
    }
}
