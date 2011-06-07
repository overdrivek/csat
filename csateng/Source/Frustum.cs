#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2011 mjt[matola@sci.fi]
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing details.
 */
#endregion
/* 
 * tutoriaali:
 * http://www.crownandcutlass.com/features/technicaldetails/frustum.html
 *
 */

using System;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace CSatEng
{
    public class Frustum
    {
        public static float[] ProjMatrix = new float[16];
        public static float[] ModelMatrix = new float[16];
        public static float[] ClipMatrix = new float[16];

        static float[,] frustum = new float[6, 4];
        const int RIGHT = 0, LEFT = 1, BOTTOM = 2, TOP = 3, BACK = 4, FRONT = 5;

        static void NormalizePlane(float[,] frustum, int side)
        {
            float magnitude = (float)Math.Sqrt((frustum[side, 0] * frustum[side, 0]) + (frustum[side, 1] * frustum[side, 1])
                                                + (frustum[side, 2] * frustum[side, 2]));

            frustum[side, 0] /= magnitude;
            frustum[side, 1] /= magnitude;
            frustum[side, 2] /= magnitude;
            frustum[side, 3] /= magnitude;
        }

        public static void CalculateFrustum()
        {
            // ota projection ja Modelview matriisit
            GL.GetFloat(GetPName.ProjectionMatrix, ProjMatrix);
            GL.GetFloat(GetPName.ModelviewMatrix, ModelMatrix);

            ClipMatrix[0] = (ModelMatrix[0] * ProjMatrix[0]) + (ModelMatrix[1] * ProjMatrix[4]) + (ModelMatrix[2] * ProjMatrix[8]) + (ModelMatrix[3] * ProjMatrix[12]);
            ClipMatrix[1] = (ModelMatrix[0] * ProjMatrix[1]) + (ModelMatrix[1] * ProjMatrix[5]) + (ModelMatrix[2] * ProjMatrix[9]) + (ModelMatrix[3] * ProjMatrix[13]);
            ClipMatrix[2] = (ModelMatrix[0] * ProjMatrix[2]) + (ModelMatrix[1] * ProjMatrix[6]) + (ModelMatrix[2] * ProjMatrix[10]) + (ModelMatrix[3] * ProjMatrix[14]);
            ClipMatrix[3] = (ModelMatrix[0] * ProjMatrix[3]) + (ModelMatrix[1] * ProjMatrix[7]) + (ModelMatrix[2] * ProjMatrix[11]) + (ModelMatrix[3] * ProjMatrix[15]);

            ClipMatrix[4] = (ModelMatrix[4] * ProjMatrix[0]) + (ModelMatrix[5] * ProjMatrix[4]) + (ModelMatrix[6] * ProjMatrix[8]) + (ModelMatrix[7] * ProjMatrix[12]);
            ClipMatrix[5] = (ModelMatrix[4] * ProjMatrix[1]) + (ModelMatrix[5] * ProjMatrix[5]) + (ModelMatrix[6] * ProjMatrix[9]) + (ModelMatrix[7] * ProjMatrix[13]);
            ClipMatrix[6] = (ModelMatrix[4] * ProjMatrix[2]) + (ModelMatrix[5] * ProjMatrix[6]) + (ModelMatrix[6] * ProjMatrix[10]) + (ModelMatrix[7] * ProjMatrix[14]);
            ClipMatrix[7] = (ModelMatrix[4] * ProjMatrix[3]) + (ModelMatrix[5] * ProjMatrix[7]) + (ModelMatrix[6] * ProjMatrix[11]) + (ModelMatrix[7] * ProjMatrix[15]);

            ClipMatrix[8] = (ModelMatrix[8] * ProjMatrix[0]) + (ModelMatrix[9] * ProjMatrix[4]) + (ModelMatrix[10] * ProjMatrix[8]) + (ModelMatrix[11] * ProjMatrix[12]);
            ClipMatrix[9] = (ModelMatrix[8] * ProjMatrix[1]) + (ModelMatrix[9] * ProjMatrix[5]) + (ModelMatrix[10] * ProjMatrix[9]) + (ModelMatrix[11] * ProjMatrix[13]);
            ClipMatrix[10] = (ModelMatrix[8] * ProjMatrix[2]) + (ModelMatrix[9] * ProjMatrix[6]) + (ModelMatrix[10] * ProjMatrix[10]) + (ModelMatrix[11] * ProjMatrix[14]);
            ClipMatrix[11] = (ModelMatrix[8] * ProjMatrix[3]) + (ModelMatrix[9] * ProjMatrix[7]) + (ModelMatrix[10] * ProjMatrix[11]) + (ModelMatrix[11] * ProjMatrix[15]);

            ClipMatrix[12] = (ModelMatrix[12] * ProjMatrix[0]) + (ModelMatrix[13] * ProjMatrix[4]) + (ModelMatrix[14] * ProjMatrix[8]) + (ModelMatrix[15] * ProjMatrix[12]);
            ClipMatrix[13] = (ModelMatrix[12] * ProjMatrix[1]) + (ModelMatrix[13] * ProjMatrix[5]) + (ModelMatrix[14] * ProjMatrix[9]) + (ModelMatrix[15] * ProjMatrix[13]);
            ClipMatrix[14] = (ModelMatrix[12] * ProjMatrix[2]) + (ModelMatrix[13] * ProjMatrix[6]) + (ModelMatrix[14] * ProjMatrix[10]) + (ModelMatrix[15] * ProjMatrix[14]);
            ClipMatrix[15] = (ModelMatrix[12] * ProjMatrix[3]) + (ModelMatrix[13] * ProjMatrix[7]) + (ModelMatrix[14] * ProjMatrix[11]) + (ModelMatrix[15] * ProjMatrix[15]);

            // laske frustumin tasot ja normalisoi ne
            frustum[RIGHT, 0] = ClipMatrix[3] - ClipMatrix[0];
            frustum[RIGHT, 1] = ClipMatrix[7] - ClipMatrix[4];
            frustum[RIGHT, 2] = ClipMatrix[11] - ClipMatrix[8];
            frustum[RIGHT, 3] = ClipMatrix[15] - ClipMatrix[12];
            NormalizePlane(frustum, RIGHT);

            frustum[LEFT, 0] = ClipMatrix[3] + ClipMatrix[0];
            frustum[LEFT, 1] = ClipMatrix[7] + ClipMatrix[4];
            frustum[LEFT, 2] = ClipMatrix[11] + ClipMatrix[8];
            frustum[LEFT, 3] = ClipMatrix[15] + ClipMatrix[12];
            NormalizePlane(frustum, LEFT);

            frustum[BOTTOM, 0] = ClipMatrix[3] + ClipMatrix[1];
            frustum[BOTTOM, 1] = ClipMatrix[7] + ClipMatrix[5];
            frustum[BOTTOM, 2] = ClipMatrix[11] + ClipMatrix[9];
            frustum[BOTTOM, 3] = ClipMatrix[15] + ClipMatrix[13];
            NormalizePlane(frustum, BOTTOM);

            frustum[TOP, 0] = ClipMatrix[3] - ClipMatrix[1];
            frustum[TOP, 1] = ClipMatrix[7] - ClipMatrix[5];
            frustum[TOP, 2] = ClipMatrix[11] - ClipMatrix[9];
            frustum[TOP, 3] = ClipMatrix[15] - ClipMatrix[13];
            NormalizePlane(frustum, TOP);

            frustum[BACK, 0] = ClipMatrix[3] - ClipMatrix[2];
            frustum[BACK, 1] = ClipMatrix[7] - ClipMatrix[6];
            frustum[BACK, 2] = ClipMatrix[11] - ClipMatrix[10];
            frustum[BACK, 3] = ClipMatrix[15] - ClipMatrix[14];
            NormalizePlane(frustum, BACK);

            frustum[FRONT, 0] = ClipMatrix[3] + ClipMatrix[2];
            frustum[FRONT, 1] = ClipMatrix[7] + ClipMatrix[6];
            frustum[FRONT, 2] = ClipMatrix[11] + ClipMatrix[10];
            frustum[FRONT, 3] = ClipMatrix[15] + ClipMatrix[14];
            NormalizePlane(frustum, FRONT);
        }

        /// <summary>
        /// tasojen normaalit osoittaa sisäänpäin joten jos testattava vertex on
        /// kaikkien tasojen "edessä", se on ruudulla ja rendataan
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static bool PointInFrustum(float x, float y, float z)
        {
            // tasoyhtälö: A*x + B*y + C*z + D = 0
            // ABC on normaalin X, Y ja Z
            // D on tason etäisyys origosta
            // =0 vertex on tasolla
            // <0 tason takana
            // >0 tason edessä
            for (int a = 0; a < 6; a++)
            {
                // jos vertex jonkun tason takana, niin palauta false (ei rendata)
                if (((frustum[a, 0] * x) + (frustum[a, 1] * y) + (frustum[a, 2] * z) + frustum[a, 3]) <= 0)
                {
                    return false;
                }
            }

            // ruudulla
            return true;
        }

        /// <summary>
        /// palauttaa etäisyyden kameraan jos pallo frustumissa, muuten 0.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public static float SphereInFrustum(float x, float y, float z, float radius)
        {
            float d = 0;
            for (int p = 0; p < 6; p++)
            {
                d = frustum[p, 0] * x + frustum[p, 1] * y + frustum[p, 2] * z + frustum[p, 3];
                if (d <= -radius) // jos pallo ei ole ruudulla
                {
                    return 0;
                }
            }
            // kaikkien tasojen edessä eli näkyvissä.
            return d + radius; // palauta matka kameraan
        }

        public static bool ObjectInFrustum(Vector3 position, BoundingSphere bound, Vector3 scale)
        {
            return ObjectInFrustum(position.X, position.Y, position.Z, bound, scale);
        }

        public static bool ObjectInFrustum(float x, float y, float z, BoundingSphere bound, Vector3 scale)
        {
            if (bound == null) return true;
            float max = scale.X;
            if (scale.Y > max) max = scale.Y;
            if (scale.Z > max) max = scale.Z;
            if (SphereInFrustum(x, y, z, bound.R * max) == 0) return false;
            return true;
        }
    }

    public class BoundingSphere
    {
        public Vector3 Min = new Vector3(99999, 99999, 99999);
        public Vector3 Max = new Vector3(-99999, -99999, -99999);
        public float R = 0;

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
        }

        public void CreateBoundingVolume(Model mesh, Vector3 min, Vector3 max)
        {
            Min = min;
            Max = max;
            Vector3 dist = Max - Min;
            R = dist.Length;
            mesh.ObjCenter = Min + (dist / 2); // objektin keskikohta
        }
    }
}
