#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2008-2014 mjt
 * This notice may not be removed from any source distribution.
 * See csat-license.txt for licensing details.
 */
#endregion
using System;
using System.Collections.Generic;
using System.Xml;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace CSatEng
{
    public class OgreMesh : Model
    {
        public static Dictionary<string, OgreMesh> meshes = new Dictionary<string, OgreMesh>();

        #region ogremesh loader
        public OgreMesh() { }
        public OgreMesh(string name, string fileName)
        {
            LoadMesh(name, fileName);
        }

        public static OgreMesh Load(string name, string fileName)
        {
            OgreMesh mesh;
            // jos mesh on jo ladattu, kloonaa se
            meshes.TryGetValue(fileName, out mesh);
            if (mesh != null)
                return (OgreMesh)mesh.Clone();

            mesh = new OgreMesh(name, fileName);
            return mesh;
        }

        void LoadMesh(string name, string fileName)
        {
            Name = name;
            XmlDocument XMLDoc = null;
            XmlElement XMLRoot;
            try
            {
                using (System.IO.StreamReader file = new System.IO.StreamReader(Settings.ModelDir + fileName))
                {
                    // tiedosto muistiin
                    string data = file.ReadToEnd();
                    XMLDoc = new XmlDocument();
                    XMLDoc.LoadXml(data);
                }
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
            }

            // Validate the File
            XMLRoot = XMLDoc.DocumentElement;
            if (XMLRoot.Name != "mesh")
            {
                Log.Error("Error [" + fileName + "] Invalid .mesh.xml File. Missing <mesh>");
            }

            bool isPath = false; // jos meshi on pathi
            if (name.StartsWith("Path_")) isPath = true;

            // Process the mesh
            processMesh(XMLRoot, isPath);

            if (isPath == false)
            {
                Vbo = new VBO();
                Vbo.DataToVBO(VertexBuffer, IndexBuffer, VBO.VertexMode.UV1);

                Boundings = new BoundingSphere();
                Boundings.CreateBoundingVolume(this);

                // lataa shader
                string shader = Material.ShaderName;
                if (shader != "")
                {
                    Vbo.Shader = GLSLShader.Load(shader);
                }
            }
            else
            {
                Path path = new Path();
                path.AddPath(name, VertexBuffer);
            }
        }

        void processMesh(XmlElement XMLRoot, bool path)
        {
            XmlElement pElement = (XmlElement)XMLRoot.SelectSingleNode("submeshes");
            if (pElement != null) processSubmeshes(pElement, path);
        }

        void processSubmeshes(XmlElement XMLNode, bool path)
        {
            XmlElement pElement = (XmlElement)XMLNode.SelectSingleNode("submesh");
            while (pElement != null)
            {
                processSubmesh(pElement, null, path);
                XmlNode nextNode = pElement.NextSibling;
                pElement = nextNode as XmlElement;
                while (pElement == null && nextNode != null)
                {
                    nextNode = nextNode.NextSibling;
                    pElement = nextNode as XmlElement;
                }
            }
        }

        void processSubmesh(XmlElement XMLNode, Node pParent, bool path)
        {
            XmlElement pElement;
            if (path == false)
            {
                if (MaterialName == "")
                {
                    MaterialName = XML.GetAttrib(XMLNode, "material");
                    Material = Material.GetMaterial(MaterialName);
                }

                pElement = (XmlElement)XMLNode.SelectSingleNode("faces");
                if (pElement != null)
                {
                    processFaces(pElement);
                }
            }

            pElement = (XmlElement)XMLNode.SelectSingleNode("geometry");
            if (pElement != null)
            {
                processGeometry(pElement, path);
            }
        }

        void processFaces(XmlElement XMLNode)
        {
            int numOfFaces = (int)XML.GetAttribFloat(XMLNode, "count");
            IndexBuffer = new ushort[numOfFaces * 3];
            XmlElement pElement = (XmlElement)XMLNode.SelectSingleNode("face");
            for (int q = 0; q < numOfFaces; q++)
            {
                Vector3 f = XML.ParseFace(pElement);
                IndexBuffer[q * 3] = (ushort)f.X;
                IndexBuffer[q * 3 + 1] = (ushort)f.Y;
                IndexBuffer[q * 3 + 2] = (ushort)f.Z;
                pElement = (XmlElement)pElement.NextSibling;
            }
        }

        void processGeometry(XmlElement XMLNode, bool path)
        {
            int numOfVerts = (int)XML.GetAttribFloat(XMLNode, "vertexcount");
            VertexBuffer = new Vertex[numOfVerts];
            XmlElement pElement = (XmlElement)XMLNode.SelectSingleNode("vertexbuffer");
            pElement = (XmlElement)pElement.SelectSingleNode("vertex");
            for (int q = 0; q < numOfVerts; q++)
            {
                processVertex(pElement, q);
                pElement = (XmlElement)pElement.NextSibling;
            }
        }
        void processVertex(XmlElement XMLNode, int vert)
        {
            XmlElement pElement = (XmlElement)XMLNode.SelectSingleNode("position");
            if (pElement != null) VertexBuffer[vert].Position = XML.ParseVector3(pElement);

            pElement = (XmlElement)XMLNode.SelectSingleNode("normal");
            if (pElement != null) VertexBuffer[vert].Normal = XML.ParseVector3(pElement);

            pElement = (XmlElement)XMLNode.SelectSingleNode("texcoord");
            if (pElement != null)
            {
                Vector2 uv = XML.ParseUV(pElement);
                VertexBuffer[vert].UV.X = uv.X;
                VertexBuffer[vert].UV.Y = uv.Y;
            }

        }
        #endregion

        public override void Dispose()
        {
            if (Name != "")
            {
                if (Material != null) Material.Dispose();
                if (Vbo != null) Vbo.Dispose();
                Log.WriteLine("Disposed: " + Name + " (mesh)", false);
                Name = "";
            }
        }

        protected override void RenderModel()
        {
            GLExt.LoadMatrix(ref Matrix);
            RenderMesh();
        }

        public override void Render()
        {
            base.Render(); // renderoi objektin ja kaikki siihen liitetyt objektit
        }


        public void RenderMesh()
        {
            if (Vbo == null) return;

            if (DoubleSided) GL.Disable(EnableCap.CullFace);
            if (VBO.FastRenderPass)
            {
                if (CastShadow) Vbo.Render();
            }
            else
            {
                Material.SetMaterial();
                if (WorldMatrix != null)
                {
                    GLExt.MatrixMode(MatrixMode.Texture);
                    GLExt.PushMatrix();
                    GLExt.MultMatrix(ref WorldMatrix);
                    GLExt.MatrixMode(MatrixMode.Modelview);
                }

                Vbo.Render();

                if (WorldMatrix != null)
                {
                    GLExt.MatrixMode(MatrixMode.Texture);
                    GLExt.PopMatrix();
                    GLExt.MatrixMode(MatrixMode.Modelview);
                }
            }
            if (DoubleSided) GL.Enable(EnableCap.CullFace);
        }
    }
}
