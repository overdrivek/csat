#region --- License ---
/*
Copyright (C) 2008 mjt[matola@sci.fi]

This file is part of CSat - small C# 3D-library

CSat is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.
 
This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.
 
You should have received a copy of the GNU Lesser General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.


-mjt,  
email: matola@sci.fi
*/
#endregion

using System;
using OpenTK.Graphics;
using OpenTK.Math;
using System.Collections;

namespace CSat
{
    public static class Util
    {
        public static float[] ProjMatrix = new float[16];
        public static float[] ModelMatrix = new float[16];
        public static float[] ClipMatrix = new float[16];

        public static void CopyArray(ref float[] arrayIn, ref float[] arrayOut)
        {
            if (arrayOut == null) arrayOut = new float[arrayIn.Length];
            for (int q = 0; q < arrayIn.Length; q++)
            {
                arrayOut[q] = arrayIn[q];
            }
        }

        public static void ArrayToMatrix(float[] array, out Matrix4 matrix)
        {
            matrix.Row0.X = array[0];
            matrix.Row0.Y = array[1];
            matrix.Row0.Z = array[2];
            matrix.Row0.W = array[3];

            matrix.Row1.X = array[4];
            matrix.Row1.Y = array[5];
            matrix.Row1.Z = array[6];
            matrix.Row1.W = array[7];

            matrix.Row2.X = array[8];
            matrix.Row2.Y = array[9];
            matrix.Row2.Z = array[10];
            matrix.Row2.W = array[11];

            matrix.Row3.X = array[12];
            matrix.Row3.Y = array[13];
            matrix.Row3.Z = array[14];
            matrix.Row3.W = array[15];
        }

        public static void ClearArrays()
        {
            Texture.DisposeAll();
            Material.DisposeAll();
            Light.DisposeAll();
            GLSL.DisposeAll();

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        /// <summary>
        /// ristikkoviritelmä
        /// </summary>
        public static void RenderGrid()
        {
            GL.LineWidth(2);
            GL.Begin(BeginMode.Lines);
            GL.Vertex3(-100, 0, 0);
            GL.Vertex3(100, 0, 0);
            GL.Vertex3(0, 0, -100);
            GL.Vertex3(0, 0, 100);
            GL.Vertex3(0, -100, 0);
            GL.Vertex3(0, 100, 0);
            GL.End();
            GL.LineWidth(1);

            GL.Begin(BeginMode.Lines);
            for (int q = 0; q < 20; q++)
            {
                GL.Vertex3(-100, 0, q * 10 - 100);
                GL.Vertex3(100, 0, q * 10 - 100);

                GL.Vertex3(q * 10 - 100, 0, -100);
                GL.Vertex3(q * 10 - 100, 0, 100);
            }
            GL.End();

        }

        /// <summary>
        /// palauttaa str:stä float luvun. jos pisteen kanssa ei onnistu, kokeillaan pilkun kanssa.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static float GetFloat(string str)
        {
            float n;
            if (float.TryParse(str, out n) == true)
            {
                return n;
            }
            str = str.Replace('.', ','); // pisteet pilkuiksi
            if (float.TryParse(str, out n) == true)
            {
                return n;
            }
            throw new Exception("GetFloat failed: " + str);
        }

        static bool is3DMode = false;
        public static double Near = 0.1, Far = 1000, Fov = 45;
        public static int ScreenWidth = 800, ScreeenHeight = 600;
        public static void Set2DMode(int width, int height)
        {
            is3DMode = false;
            ScreenWidth = width;
            ScreeenHeight = height;

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, width, 0, height, -1, 1);
            GL.Viewport(0, 0, width, height);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);
        }
        public static void Set3DMode(int width, int height, double near, double far)
        {
            is3DMode = true;
            ScreenWidth = width;
            ScreeenHeight = height;
            Near = near;
            Far = far;

            GL.Viewport(0, 0, width, height);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            Glu.Perspective(Fov, (double)width / (double)height, near, far);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
        }

        public static void Resize(int width, int height, double near, double far)
        {
            if (is3DMode) Set3DMode(width, height, near, far);
            else Set2DMode(width, height);
        }
        public static void Resize()
        {
            Resize(ScreenWidth, ScreeenHeight, Near, Far);
        }
        public static void Set2DMode()
        {
            Set2DMode(ScreenWidth, ScreeenHeight);
        }
        public static void Set3DMode()
        {
            Set3DMode(ScreenWidth, ScreeenHeight, Near, Far);
        }

        public static void CheckGLError(string className)
        {
            ErrorCode error = ErrorCode.NoError;

            GL.Finish();

            error = GL.GetError();
            if (error != ErrorCode.NoError)
            {
                throw new ArgumentException(className + ": " + Glu.ErrorString(error) + " (" + error + ")");
            }
        }

    }

}
