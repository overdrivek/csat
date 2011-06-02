#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2011 mjt[matola@sci.fi]
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing details.
 */
#endregion
using System;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace CSatEng
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
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

        int vertexID = 0, indexID = 0;
        BufferUsageHint usage = BufferUsageHint.StaticDraw;

        /// <summary>
        /// mit‰ dataa k‰ytet‰‰n (normal, uv, uv2)
        /// </summary>
        VertexMode vertexFlags = VertexMode.Normal;
        int numOfIndices = 0;
        int vertexSize = 0;

        public VBO()
        {
        }
        public VBO(BufferUsageHint usage)
        {
            this.usage = usage;
        }

        public void Dispose()
        {
            if (vertexID != 0)
            {
                GL.DeleteBuffers(1, ref vertexID);
                Log.WriteLine("Disposed: VBO", true);
            }
            if (indexID != 0) GL.DeleteBuffers(1, ref indexID);
            vertexID = indexID = 0;
        }

        public void DataToVBO(Vertex[] vertices, ushort[] indices, VertexMode mode)
        {
            int size;
            vertexFlags = mode;
            numOfIndices = indices.Length;
            vertexSize = BlittableValueType.StrideOf(vertices);

            GL.GenBuffers(1, out vertexID);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexID);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertices.Length * vertexSize), vertices, usage);
            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
            if (vertices.Length * BlittableValueType.StrideOf(vertices) != size) Util.Error("DataToVBO: Vertex data not uploaded correctly");

            GL.GenBuffers(1, out indexID);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexID);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indices.Length * sizeof(ushort)), indices, usage);
            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);
            if (indices.Length * sizeof(short) != size) throw new ApplicationException("DataToVBO: Element data not uploaded correctly");

            Util.CheckGLError("DataToVBO");
        }

        public void Update(Vertex[] verts)
        {
            if (usage == BufferUsageHint.DynamicDraw)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, vertexID);
                GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)0, (IntPtr)(vertexSize * numOfIndices), verts);
                Util.CheckGLError("Update");
            }
        }

        /// <summary>
        /// tilat p‰‰lle. pit‰‰ kutsua ennen Renderi‰.
        /// </summary>
        public void BeginRender()
        {
            if (vertexID == 0 || indexID == 0) Util.Error("VBO destroyed!");

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexID);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexID);
            GL.EnableClientState(ArrayCap.NormalArray);
            GL.NormalPointer(NormalPointerType.Float, vertexSize, (IntPtr)(3 * sizeof(float)));

            if (vertexFlags == VertexMode.UV1 || vertexFlags == VertexMode.UV2)
            {
                GL.ClientActiveTexture(TextureUnit.Texture0);
                GL.EnableClientState(ArrayCap.TextureCoordArray);
                GL.TexCoordPointer(2, TexCoordPointerType.Float, vertexSize, (IntPtr)(6 * sizeof(float)));
                if (vertexFlags == VertexMode.UV2)
                {
                    GL.ClientActiveTexture(TextureUnit.Texture1);
                    GL.EnableClientState(ArrayCap.TextureCoordArray);
                    GL.TexCoordPointer(2, TexCoordPointerType.Float, vertexSize, (IntPtr)(8 * sizeof(float)));
                }
            }
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.VertexPointer(3, VertexPointerType.Float, vertexSize, (IntPtr)(0));
        }

        /// <summary>
        /// renderoi vbo
        /// </summary>
        public void Render()
        {
            BeginRender();
            GL.DrawElements(BeginMode.Triangles, numOfIndices, DrawElementsType.UnsignedShort, IntPtr.Zero);
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
            if (vertexFlags == VertexMode.UV1 || vertexFlags == VertexMode.UV2)
            {
                GL.ClientActiveTexture(TextureUnit.Texture0);
                GL.DisableClientState(ArrayCap.TextureCoordArray);
                if (vertexFlags == VertexMode.UV2)
                {
                    GL.ClientActiveTexture(TextureUnit.Texture1);
                    GL.DisableClientState(ArrayCap.TextureCoordArray);
                }
            }
        }
    }
}
