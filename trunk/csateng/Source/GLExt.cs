#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2011 mjt
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing details.
 */
#endregion
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;

// note: jos texturematrix moodi, silloin modelviewmatrix on texture matriisi eli sitä ei pidä käyttää

namespace CSatEng
{
    public static class GLExt
    {
        static bool usingTextureMatrix = false;
        static List<Matrix4> matrixStack = new List<Matrix4>();
        public static Matrix4 ModelViewMatrix = Matrix4.Identity, TextureMatrix = Matrix4.Identity;
        public static Matrix4 ProjectionMatrix = Matrix4.Identity;
        public static Vector4 Color = Vector4.One;

        public static void Color4(float r, float g, float b, float a)
        {
            if (GLSLShader.IsSupported == false)
            {
                GL.Color4(r, g, b, a);
            }
            else
                Color = new Vector4(r, g, b, a);
        }

        public static void SetLighting(bool active)
        {
            if (Settings.UseGL3 == false)
                if (active == true) GL.Enable(EnableCap.Lighting);
                else GL.Disable(EnableCap.Lighting);
        }

        public static void SetProjectionMatrix(Matrix4 projMatrix)
        {
            if (GLSLShader.IsSupported == false)
            {
                GL.MatrixMode(OpenTK.Graphics.OpenGL.MatrixMode.Projection);
                GL.LoadMatrix(ref projMatrix);
                GL.MatrixMode(OpenTK.Graphics.OpenGL.MatrixMode.Modelview);
            }
            ProjectionMatrix = projMatrix;
        }

        public static void Translate(float x, float y, float z)
        {
            ModelViewMatrix = Matrix4.CreateTranslation(x, y, z) * ModelViewMatrix;
        }

        public static void RotateX(float angle)
        {
            ModelViewMatrix = Matrix4.CreateRotationX(angle * MathExt.DegToRad) * ModelViewMatrix;
        }
        public static void RotateY(float angle)
        {
            ModelViewMatrix = Matrix4.CreateRotationY(angle * MathExt.DegToRad) * ModelViewMatrix;
        }
        public static void RotateZ(float angle)
        {
            ModelViewMatrix = Matrix4.CreateRotationZ(angle * MathExt.DegToRad) * ModelViewMatrix;
        }

        public static void Scale(float x, float y, float z)
        {
            ModelViewMatrix = Matrix4.Scale(x, y, z) * ModelViewMatrix;
        }

        public static void LoadIdentity()
        {
            ModelViewMatrix = Matrix4.Identity;
        }

        public static void MultMatrix(ref Matrix4 matrix)
        {
            ModelViewMatrix = matrix * ModelViewMatrix;
        }
        public static void MultMatrix(float[] matrix)
        {
            Matrix4 mat = new Matrix4(matrix[0], matrix[1], matrix[2], matrix[3],
                                    matrix[4], matrix[5], matrix[6], matrix[7],
                                    matrix[8], matrix[9], matrix[10], matrix[11],
                                    matrix[12], matrix[13], matrix[14], matrix[15]);
            ModelViewMatrix = mat * ModelViewMatrix;
        }

        public static void LoadMatrix(ref Matrix4 matrix)
        {
            ModelViewMatrix = matrix;
        }

        public static void PushMatrix()
        {
            matrixStack.Add(ModelViewMatrix);
        }

        public static void PopMatrix()
        {
#if DEBUG
            if (matrixStack.Count == 0) Log.Error("PopMatrix: stack is empty");
#endif
            ModelViewMatrix = matrixStack[matrixStack.Count - 1];
            matrixStack.RemoveAt(matrixStack.Count - 1);
        }

        static Matrix4 _tempModelViewMatrix;
        public static void MatrixMode(OpenTK.Graphics.OpenGL.MatrixMode mode)
        {
            if (mode == OpenTK.Graphics.OpenGL.MatrixMode.Texture && usingTextureMatrix == false)
            {
                usingTextureMatrix = true;
                _tempModelViewMatrix = ModelViewMatrix;
                ModelViewMatrix = TextureMatrix;
            }
            else if (mode == OpenTK.Graphics.OpenGL.MatrixMode.Modelview && usingTextureMatrix == true)
            {
                usingTextureMatrix = false;
                TextureMatrix = ModelViewMatrix;
                ModelViewMatrix = _tempModelViewMatrix;
            }
            else if (mode == OpenTK.Graphics.OpenGL.MatrixMode.Projection) Log.Error("Use SetProjectionMatrix() method instead of GLExt.MatrixMode(MatrixMode.Projection)");
        }

        public static void CheckGLError(string str)
        {
            GL.Finish();
            ErrorCode error = error = GL.GetError();
            if (error != ErrorCode.NoError) Log.Error(str + "Error: " + error);
        }
    }
}
