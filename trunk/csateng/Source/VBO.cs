#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2011 mjt[matola@sci.fi]
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing details.
 */
#endregion
using System;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace CSatEng
{
    public struct Vertex
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector4 UV;

        public Vertex(Vector3 position)
        {
            this.Position = position;
            this.Normal = new Vector3(0, 0, 1);
            this.UV = new Vector4(1, 1, 1, 1);
        }
        public Vertex(Vector3 position, Vector3 normal)
        {
            this.Position = position;
            this.Normal = normal;
            this.UV = new Vector4(1, 1, 1, 1);
        }
        public Vertex(Vector3 position, Vector3 normal, Vector2 uv)
        {
            this.Position = position;
            this.Normal = normal;
            this.UV.X = uv.X;
            this.UV.Y = uv.Y;
            this.UV.Z = uv.X;
            this.UV.W = uv.Y;
        }
        public Vertex(Vector3 position, Vector3 normal, Vector2 uv, Vector2 uv2)
        {
            this.Position = position;
            this.Normal = normal;
            this.UV.X = uv.X;
            this.UV.Y = uv.Y;
            this.UV.Z = uv2.X;
            this.UV.W = uv2.Y;
        }
    }

    public class VBO
    {
        public enum VertexMode { Normal, UV1, UV2 }; // mit‰ tietoja vertexiss‰

        static private int vertexSize = 40;

        private int vertexID = 0, indexID = 0;
        private BufferUsageHint usage = BufferUsageHint.StaticDraw;

        /// <summary>
        /// mit‰ textureunittei k‰ytet‰‰n. bitti0 niin ykkˆst‰, bitti1 niin kakkosta
        /// </summary>
        static int useTexUnits = 1;

        /// <summary>
        /// mit‰ dataa k‰ytet‰‰n (normal, uv, uv2)
        /// </summary>
        private VertexMode vertexFlags = VertexMode.Normal;
        int numOfIndices = 0;

        public VBO()
        {
        }
        public VBO(BufferUsageHint usage)
        {
            this.usage = usage;
        }

        /// <summary>
        /// luo VBO. monelleko vertexille ja indexille varataan tilaa
        /// </summary>
        /// <param name="vertSize"></param>
        /// <param name="indSize"></param>
        void AllocVBO(int vertSize, int indSize)
        {
            GL.GenBuffers(1, out vertexID);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexID);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertSize * vertexSize), (IntPtr)null, usage);

            GL.GenBuffers(1, out indexID);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexID);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indSize * sizeof(int)), (IntPtr)null, usage);

            numOfIndices = indSize;

            Log.WriteLine("AllocVBO: [verts:" + vertSize + "] [indices:" + indSize + "]", true);
        }

        public void Dispose()
        {
            if (vertexID != 0)
            {
                if (indexID != 0) GL.DeleteBuffers(1, ref indexID);
                GL.DeleteBuffers(1, ref vertexID);

                Log.WriteLine("Disposed: VBO", true);
            }
            vertexID = indexID = 0;
        }

        /// <summary>
        /// kopioi objekti vbo:hon.
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="indices"></param>
        /// <param name="normals"></param>
        /// <param name="uvs"></param>
        public void DataToVBO(Vector3[] vertices, int[] indices, Vector3[] normals, Vector2[] uvs)
        {
            DataToVBO(vertices, indices, normals, uvs, null);
        }

        /// <summary>
        /// kopioi objekti vbo:hon.
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="indices"></param>
        /// <param name="normals"></param>
        /// <param name="uvs"></param>
        public void DataToVBO(Vector3[] vertices, int[] indices, Vector3[] normals, Vector2[] uvs, Vector2[] uvs2)
        {
            Vertex[] verts = new Vertex[vertices.Length];

            if (normals != null) vertexFlags = VertexMode.Normal;
            if (uvs != null) vertexFlags = VertexMode.UV1;
            if (uvs2 != null) vertexFlags = VertexMode.UV2;

            // koppaa vertex infot Vertexiin
            switch (vertexFlags)
            {
                case VertexMode.Normal:
                    for (int q = 0; q < vertices.Length; q++)
                    {
                        verts[q] = new Vertex(vertices[q], normals[q]);
                    }
                    break;

                case VertexMode.UV1:
                    for (int q = 0; q < vertices.Length; q++)
                    {
                        verts[q] = new Vertex(vertices[q], normals[q], uvs[q]);
                    }
                    break;

                case VertexMode.UV2:
                    for (int q = 0; q < vertices.Length; q++)
                    {
                        verts[q] = new Vertex(vertices[q], normals[q], uvs[q], uvs2[q]);
                    }
                    break;
            }

            AllocVBO(verts.Length, indices.Length);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexID);
            GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)0, (IntPtr)(verts.Length * vertexSize), verts);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexID);
            GL.BufferSubData(BufferTarget.ElementArrayBuffer, (IntPtr)0, (IntPtr)(indices.Length * sizeof(int)), indices);

            Util.CheckGLError("VBO");
        }

        public void DataToVBO(Vertex[] verts, int[] indices, VertexMode mode)
        {
            vertexFlags = mode;

            AllocVBO(verts.Length, indices.Length);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexID);
            GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)0, (IntPtr)(verts.Length * vertexSize), verts);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexID);
            GL.BufferSubData(BufferTarget.ElementArrayBuffer, (IntPtr)0, (IntPtr)(indices.Length * sizeof(int)), indices);

            Util.CheckGLError("VBO");
        }

        public void Update(Vertex[] verts)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexID);
            GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)0, (IntPtr)(numOfIndices * vertexSize), verts);
        }

        /// <summary>
        /// tilat p‰‰lle. pit‰‰ kutsua ennen Renderi‰.
        /// </summary>
        public void BeginRender()
        {
            if (vertexID == 0 || indexID == 0)
            {
                Util.Error("VBO destroyed!");
            }

            GL.ActiveTexture(TextureUnit.Texture0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexID);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexID);

            GL.EnableClientState(ArrayCap.NormalArray);
            GL.EnableClientState(ArrayCap.NormalArray);
            GL.NormalPointer(NormalPointerType.Float, vertexSize, (IntPtr)(3 * sizeof(float)));

            if (vertexFlags == VertexMode.UV1 || vertexFlags == VertexMode.UV2) // v‰hint‰‰n yhdet texcoordsit objektilla
            {
                if ((useTexUnits & 1) == 1)
                {

                    GL.ClientActiveTexture(TextureUnit.Texture0);
                    GL.EnableClientState(ArrayCap.TextureCoordArray);
                    GL.TexCoordPointer(2, TexCoordPointerType.Float, vertexSize, (IntPtr)(6 * sizeof(float)));
                }
                if ((useTexUnits & 2) == 2)
                {
                    GL.ClientActiveTexture(TextureUnit.Texture1);
                    GL.EnableClientState(ArrayCap.TextureCoordArray);
                    GL.TexCoordPointer(2, TexCoordPointerType.Float, vertexSize, (IntPtr)(8 * sizeof(float)));
                }

            }

            GL.EnableClientState(ArrayCap.VertexArray);
            GL.VertexPointer(3, VertexPointerType.Float, vertexSize, (IntPtr)(0));
        }

        public static void UseTexUnits(bool tu1, bool tu2)
        {
            useTexUnits = 0;
            if (tu1) useTexUnits = 1;
            if (tu2) useTexUnits += 2;
        }

        /// <summary>
        /// renderoi vbo
        /// </summary>
        public void Render()
        {
            BeginRender();
            GL.DrawElements(BeginMode.Triangles, numOfIndices, DrawElementsType.UnsignedInt, IntPtr.Zero);
            EndRender();
        }

        /// <summary>
        /// tilat pois p‰‰lt‰
        /// </summary>
        public void EndRender()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            GL.DisableClientState(ArrayCap.NormalArray);
            GL.DisableClientState(ArrayCap.VertexArray);

            GL.ClientActiveTexture(TextureUnit.Texture1);
            GL.DisableClientState(ArrayCap.TextureCoordArray);

            GL.ClientActiveTexture(TextureUnit.Texture0);
            GL.DisableClientState(ArrayCap.TextureCoordArray);
        }

        /// <summary>
        /// aseta aktiivinen textureunitti
        /// </summary>
        /// <param name="textureUnit"></param>
        public static void UseTextureUnit(int textureUnit)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + textureUnit);
        }

    }
}
