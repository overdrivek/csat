#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2011 mjt[matola@sci.fi]
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing details.
 */
#endregion
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace CSatEng
{
    public class Sky : SceneNode
    {
        OgreMesh[] skyboxSides = new OgreMesh[6];
        public static bool IsSky = false;
        static GLSLShader shader;

        public override void Dispose()
        {
            if (Name != "")
            {
                for (int q = 0; q < 6; q++) skyboxSides[q].Dispose();
                Name = "";
            }
            IsSky = false;
        }

        /// <summary>
        /// lataa skybox.
        /// </summary>
        /// <param name="skyName">skyboxin nimi, eli esim plainsky_  jos tiedostot on plainsky_front.jpg, plainsky_back.jpg jne</param>
        /// <param name="ext">tiedoston p‰‰te, eli jpg, png, dds, ..</param>
        /// <param name="scale"></param>
        public static Sky Load(string skyName, string ext)
        {
            Sky sky = new Sky();
            IsSky = true;
            string[] sideStr = { "top", "bottom", "left", "right", "front", "back" };
            SceneNode skyNode = new SceneNode();
            DotScene ds = DotScene.Load("sky/sky.scene", skyNode);
            skyNode.GetList(true);
            shader = GLSLShader.Load("model.shader:SKY");

            int side = 0;
            TextureLoaderParameters.WrapModeS = TextureWrapMode.ClampToEdge;
            TextureLoaderParameters.WrapModeT = TextureWrapMode.ClampToEdge;
            TextureLoaderParameters.FlipImages = false;
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
                    m.Material = MaterialInfo.GetMaterial(fileName + "_material");
                    m.Material.DiffuseTex = Texture.Load(m.MaterialName);
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
            if (ShadowMapping.ShadowPass) return;
            Settings.NumOfObjects++;

            GL.PushMatrix();
            Matrix4 modelMatrix;
            GL.GetFloat(GetPName.ModelviewMatrix, out modelMatrix);
            modelMatrix.M41 = modelMatrix.M42 = modelMatrix.M43 = 0;
            GL.LoadMatrix(ref modelMatrix);

            GL.Disable(EnableCap.Lighting);
            GL.Disable(EnableCap.DepthTest);
            GL.DepthMask(false); // ei kirjoiteta z-bufferiin
            shader.UseProgram();

            GL.Scale(10, 10, 10);
            for (int q = 0; q < 6; q++)
            {
                OgreMesh m = skyboxSides[q];
                m.Material.SetMaterial();
                m.Vbo.Render();
            }

            GL.DepthMask(true);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Lighting);

            GL.PopMatrix();
        }
    }
}
