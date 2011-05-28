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
using System.Collections.Generic;

namespace CSatEng
{
    public class BoundingVolume
    {
        public enum TestMode { None, Box, Sphere };
        public Vector3 Min = new Vector3(99999, 99999, 99999);
        public Vector3 Max = new Vector3(-99999, -99999, -99999);
        public TestMode Mode = TestMode.Sphere;
        public float R = 0;
        public Vector3[] Corner = new Vector3[8]; // bboxin kulmat

        public BoundingVolume()
        {
        }
        public BoundingVolume(TestMode mode)
        {
            Mode = mode;
        }

        public void CreateBoundingVolume(Model mesh)
        {
            for (int q = 0; q < mesh.VertexBuffer.Length; q++)
            {
                if (mesh.VertexBuffer[q].Position.X < Min.X) Min.X = mesh.VertexBuffer[q].Position.X;
                if (mesh.VertexBuffer[q].Position.Y < Min.Y) Min.Y = mesh.VertexBuffer[q].Position.Y;
                if (mesh.VertexBuffer[q].Position.Z < Min.Z) Min.Z = mesh.VertexBuffer[q].Position.Z;

                if (mesh.VertexBuffer[q].Position.X > Max.X) Max.X = mesh.VertexBuffer[q].Position.X;
                if (mesh.VertexBuffer[q].Position.Y > Max.Y) Max.Y = mesh.VertexBuffer[q].Position.Y;
                if (mesh.VertexBuffer[q].Position.Z > Max.Z) Max.Z = mesh.VertexBuffer[q].Position.Z;
            }

            Vector3 dist = Max - Min;
            R = dist.Length;
            mesh.ObjCenter = Min + (dist / 2); // objektin keskikohta
            SetCorners();
        }

        public void CreateBoundingVolume(Model mesh, Vector3 min, Vector3 max)
        {
            Min = min;
            Max = max;
            Vector3 dist = Max - Min;
            R = dist.Length;
            mesh.ObjCenter = Min + (dist / 2); // objektin keskikohta
            SetCorners();
        }

        void SetCorners()
        {
            // aseta kulmat
            Corner[0] = Min;

            Corner[1] = Min;
            Corner[1].X = Max.X;

            Corner[2] = Min;
            Corner[2].X = Max.X;
            Corner[2].Y = Max.Y;

            Corner[3] = Min;
            Corner[3].Y = Max.Y;

            Corner[4] = Max;

            Corner[5] = Max;
            Corner[5].X = Min.X;

            Corner[6] = Max;
            Corner[6].X = Min.X;
            Corner[6].Y = Min.Y;

            Corner[7] = Max;
            Corner[7].Y = Min.Y;
        }

    }
}
