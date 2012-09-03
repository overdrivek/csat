#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2008-2012 mjt
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing details.
 */
#endregion
using System;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace CSatEng
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Vertex
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector4 UV;
        public static int Size;

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
        public enum VertexMode { UV1, UV2 };
        VertexMode vertexFlags = VertexMode.UV1;
        BufferUsageHint usage = BufferUsageHint.StaticDraw;
        public GLSLShader Shader;

        /// <summary>
        /// käytetään kun renderoidaan depth-bufferiin esim varjostusta varten
        /// </summary>
        public static bool FastRenderPass = false;

        int vertexID = -1, indexID = -1, vaoID = -1;
        int numOfIndices = 0;

        public VBO()
        {
        }
        public VBO(BufferUsageHint usage)
        {
            this.usage = usage;
        }

        public void Dispose()
        {
            if (vertexID != -1) GL.DeleteBuffers(1, ref vertexID);
            if (indexID != -1) GL.DeleteBuffers(1, ref indexID);
            if (vaoID != -1) GL.DeleteVertexArrays(1, ref vaoID);
            if (Shader != null) Shader.Dispose();
            vertexID = indexID = vaoID = -1;
            Shader = null;

            if (numOfIndices > 0) Log.WriteLine("Disposed: VBO", false);
            numOfIndices = 0;
        }

        public void DataToVBO(Vertex[] vertices, ushort[] indices, VertexMode mode)
        {
            if (numOfIndices > 0) Dispose();
            int size;
            vertexFlags = mode;
            numOfIndices = indices.Length;
            Vertex.Size = BlittableValueType.StrideOf(vertices);

            GL.GenBuffers(1, out vertexID);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexID);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertices.Length * Vertex.Size), vertices, usage);
            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
            if (vertices.Length * BlittableValueType.StrideOf(vertices) != size) Log.Error("DataToVBO: Vertex data not uploaded correctly");

            GL.GenBuffers(1, out indexID);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexID);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indices.Length * sizeof(ushort)), indices, usage);
            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);
            if (indices.Length * sizeof(short) != size) throw new ApplicationException("DataToVBO: Element data not uploaded correctly");

            if (GLSLShader.IsSupported)
            {
                Shader = GLSLShader.Load();

                if (Shader != null)
                {
                    if (Settings.UseGL3)
                    {
                        GL.GenVertexArrays(1, out vaoID);
                        GL.BindVertexArray(vaoID);
                    }
                    GL.BindBuffer(BufferTarget.ArrayBuffer, vertexID);
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexID);
                    Shader.SetAttributes();
                    if (Settings.UseGL3) GL.BindVertexArray(0);
                }
            }
            GLExt.CheckGLError("DataToVBO");
        }

        public void Update(Vertex[] verts)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexID);
            GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)0, (IntPtr)(verts.Length * Vertex.Size), verts);
        }

        void BeginRender_noShaders()
        {
            if (vertexID == -1 || indexID == -1) Log.Error("VBO destroyed!");
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexID);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexID);
            GL.EnableClientState(ArrayCap.NormalArray);
            GL.NormalPointer(NormalPointerType.Float, Vertex.Size, (IntPtr)Vector3.SizeInBytes);
            if (vertexFlags == VertexMode.UV1 || vertexFlags == VertexMode.UV2)
            {
                GL.ClientActiveTexture(TextureUnit.Texture0);
                GL.EnableClientState(ArrayCap.TextureCoordArray);
                GL.TexCoordPointer(2, TexCoordPointerType.Float, Vertex.Size, (IntPtr)(2 * Vector3.SizeInBytes));
                if (vertexFlags == VertexMode.UV2)
                {
                    GL.ClientActiveTexture(TextureUnit.Texture1);
                    GL.EnableClientState(ArrayCap.TextureCoordArray);
                    GL.TexCoordPointer(2, TexCoordPointerType.Float, Vertex.Size, (IntPtr)(2 * Vector3.SizeInBytes + Vector2.SizeInBytes));
                }
            }
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.VertexPointer(3, VertexPointerType.Float, Vertex.Size, IntPtr.Zero);
        }

        void EndRender_noShaders()
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

        /// <summary>
        /// renderoi vbo
        /// </summary>
        public void Render()
        {
            GL.ActiveTexture(TextureUnit.Texture0);

            if (GLSLShader.IsSupported == false) // jos gl 1.5 tai shaderit otettu pois käytöstä
            {
                GLSLShader.UnBindShader();

                GL.LoadMatrix(ref GLExt.ModelViewMatrix);
                BeginRender_noShaders();
                GL.DrawElements(BeginMode.Triangles, numOfIndices, DrawElementsType.UnsignedShort, IntPtr.Zero);
                EndRender_noShaders();
                return;
            }

            if (VBO.FastRenderPass == false)
                if (Shader != null)
                    Shader.UseProgram();

            if (Settings.UseGL3 == true)
            {
                GL.BindVertexArray(vaoID);
                GL.BindBuffer(BufferTarget.ArrayBuffer, vertexID);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexID);
                if (GLSLShader.CurrentShader != null)
                {
                    GLSLShader.CurrentShader.SetUniforms();
                    GLSLShader.CurrentShader.SetAttributes();
                }
                GL.DrawElements(BeginMode.Triangles, numOfIndices, DrawElementsType.UnsignedShort, IntPtr.Zero);
                GL.BindVertexArray(0);
                return;
            }
            else
            {
                // gl2
                GL.BindBuffer(BufferTarget.ArrayBuffer, vertexID);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexID);
                if (GLSLShader.CurrentShader != null)
                {
                    GLSLShader.CurrentShader.SetUniforms();
                    GLSLShader.CurrentShader.SetAttributes();
                }
                else
                    GLSLShader.UnBindShader();

                GL.DrawElements(BeginMode.Triangles, numOfIndices, DrawElementsType.UnsignedShort, IntPtr.Zero);
            }
        }
    }
}
