#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2011 mjt[matola@sci.fi]
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing details.
 */
#endregion

// apuluokat, lisää laskukaavoja

using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace CSatEng
{
    public static class MathExt
    {
        public static readonly float RadToDeg = (float)(180 / Math.PI);
        public static readonly float DegToRad = (float)(Math.PI / 180);

        public static Vector3 VectorMatrixMult(ref Vector3 vec, ref Matrix4 mat)
        {
            Vector3 outv;
            outv.X = vec.X * mat.Row0.X + vec.Y * mat.Row0.Y + vec.Z * mat.Row0.Z;
            outv.Y = vec.X * mat.Row1.X + vec.Y * mat.Row1.Y + vec.Z * mat.Row1.Z;
            outv.Z = vec.X * mat.Row2.X + vec.Y * mat.Row2.Y + vec.Z * mat.Row2.Z;
            return outv;
        }

        public static void CalcPlane(ref Vector3 v1, ref Vector3 v2, ref Vector3 v3, out Vector4 outv)
        {
            outv.X = (v1.Y * (v2.Z - v3.Z)) + (v2.Y * (v3.Z - v1.Z)) + (v3.Y * (v1.Z - v2.Z));
            outv.Y = (v1.Z * (v2.X - v3.X)) + (v2.Z * (v3.X - v1.X)) + (v3.Z * (v1.X - v2.X));
            outv.Z = (v1.X * (v2.Y - v3.Y)) + (v2.X * (v3.Y - v1.Y)) + (v3.X * (v1.Y - v2.Y));
            outv.W = -((v1.X * ((v2.Y * v3.Z) - (v3.Y * v2.Z))) + (v2.X * ((v3.Y * v1.Z) - (v1.Y * v3.Z))) + (v3.X * ((v1.Y * v2.Z) - (v2.Y * v1.Z))));
        }

        /// <summary>
        /// laske tasolle normaali
        /// </summary>
        public static void CalcNormal(ref Vector3 v1, ref Vector3 v2, ref Vector3 v3, out Vector3 outv)
        {
            float RelX1 = v2.X - v1.X;
            float RelY1 = v2.Y - v1.Y;
            float RelZ1 = v2.Z - v1.Z;
            float RelX2 = v3.X - v1.X;
            float RelY2 = v3.Y - v1.Y;
            float RelZ2 = v3.Z - v1.Z;
            outv.X = (RelY1 * RelZ2) - (RelZ1 * RelY2);
            outv.Y = (RelZ1 * RelX2) - (RelX1 * RelZ2);
            outv.Z = (RelX1 * RelY2) - (RelY1 * RelX2);
        }

        /// <summary>
        /// laske vertexnormaalit
        /// </summary>
        public static void CalcNormals(ref Vector3[] pos, ref int[][] faces, ref Vector3[] normals, bool flipNormals)
        {
            int q, w, count = 0;
            Vector3 outv = new Vector3();
            Vector3 c = new Vector3();
            float len;

            for (q = 0; q < pos.Length; q++)
            {
                c.X = c.Y = c.Z = 0;

                for (w = 0; w < faces.Length; w++)
                {
                    // jos vertex on tässä kolmiossa
                    if ((faces[w][0] == q) || (faces[w][1] == q) || (faces[w][2] == q))
                    {
                        CalcNormal(ref pos[faces[w][0]], ref pos[faces[w][1]], ref pos[faces[w][2]], out outv);
                        len = outv.LengthSquared; // outv.Length;
                        if (len == 0) len = -1.0f;
                        c += (outv / len);
                        count++;
                    }

                    if (count > 0)
                    {
                        // laske vektorin pituus
                        len = c.LengthSquared; // c.Length;
                        if (flipNormals == true) len = -len;
                        if (len == 0) len = -1.0f;
                        normals[q] = new Vector3();
                        normals[q] = (c / len);
                        if (len != -1) normals[q].Normalize();
                    }
                }
            }
        }
    }


    public static class QuaternionExt
    {
        public static void ComputeW(ref Quaternion q)
        {
            float t = 1.0f - (q.Xyz.X * q.Xyz.X) - (q.Xyz.Y * q.Xyz.Y) - (q.Xyz.Z * q.Xyz.Z);
            if (t < 0.0f) q.W = 0.0f;
            else q.W = (float)-Math.Sqrt(t);
        }

        public static Vector3 RotatePoint(ref Quaternion q, ref Vector3 v)
        {
            Vector3 outv;
            Quaternion inv = new Quaternion();
            inv.X = -q.X;
            inv.Y = -q.Y;
            inv.Z = -q.Z;
            inv.W = q.W;
            Quaternion norminv = Quaternion.Normalize(inv);
            Quaternion m = MultVec(ref q, ref v);
            Quaternion qm = Quaternion.Multiply(m, norminv);
            outv.X = qm.Xyz.X;
            outv.Y = qm.Xyz.Y;
            outv.Z = qm.Xyz.Z;
            return outv;
        }

        public static Quaternion MultVec(ref Quaternion q, ref Vector3 v)
        {
            Quaternion outq = new Quaternion();
            outq.W = -(q.Xyz.X * v.X) - (q.Xyz.Y * v.Y) - (q.Xyz.Z * v.Z);
            outq.X = ((q.W * v.X) + (q.Xyz.Y * v.Z)) - (q.Xyz.Z * v.Y);
            outq.Y = ((q.W * v.Y) + (q.Xyz.Z * v.X)) - (q.Xyz.X * v.Z);
            outq.Z = ((q.W * v.Z) + (q.Xyz.X * v.Y)) - (q.Xyz.Y * v.X);
            return outq;
        }

        public static float DotProduct(ref Quaternion qa, ref Quaternion qb)
        {
            return ((qa.Xyz.X * qb.Xyz.X) + (qa.Xyz.Y * qb.Xyz.Y) + (qa.Xyz.Z * qb.Xyz.Z) + (qa.W * qb.W));
        }

        public static Quaternion Slerp(ref Quaternion qa, ref Quaternion qb, float t)
        {
            Quaternion outr = new Quaternion();

            // check for out-of range parameter and return edge points if so
            if (t <= 0.0)
            {
                return qa;
            }

            if (t >= 1.0)
            {
                return qb;
            }

            // compute "cosine of angle between quaternions" using dot product
            float cosOmega = DotProduct(ref qa, ref qb);

            // if negative dot, use -q1. two quaternions q and -q
            // represent the same Rotation, but may produce
            // different slerp. we chose q or -q to rotate using
            // the acute angle.
            float q1w = qb.W;
            float q1x = qb.Xyz.X;
            float q1y = qb.Xyz.Y;
            float q1z = qb.Xyz.Z;

            if (cosOmega < 0.0f)
            {
                q1w = -q1w;
                q1x = -q1x;
                q1y = -q1y;
                q1z = -q1z;
                cosOmega = -cosOmega;
            }

            // we should have two unit quaternions, so dot should be <= 1.0
            // assert( cosOmega < 1.1f );
            if (cosOmega >= 1.1f)
            {
                Log.WriteLine("Quaternion error: Slerp");
            }

            // compute interpolation fraction, checking for quaternions
            // almost exactly the same
            float k0;

            // compute interpolation fraction, checking for quaternions
            // almost exactly the same
            float k1;

            if (cosOmega > 0.9999f)
            {
                // very close - just use linear interpolation,
                // which will protect againt a divide by zero
                k0 = 1.0f - t;
                k1 = t;
            }
            else
            {
                // compute the sin of the angle using the
                // trig identity sin^2(omega) + cos^2(omega) = 1
                float sinOmega = (float)Math.Sqrt(1.0f - (cosOmega * cosOmega));

                // compute the angle from its sin and cosine
                float omega = (float)Math.Atan2(sinOmega, cosOmega);

                // compute inverse of denominator, so we only have to divide
                // once
                float oneOverSinOmega = 1.0f / sinOmega;

                // Compute interpolation parameters
                k0 = (float)Math.Sin((1.0f - t) * omega) * oneOverSinOmega;
                k1 = (float)Math.Sin(t * omega) * oneOverSinOmega;
            }

            // interpolate and return new quaternion
            outr.W = (k0 * qa.W) + (k1 * q1w);
            outr.X = (k0 * qa.Xyz.X) + (k1 * q1x);
            outr.Y = (k0 * qa.Xyz.Y) + (k1 * q1y);
            outr.Z = (k0 * qa.Xyz.Z) + (k1 * q1z);

            return outr;
        }
        
        public static Vector3 QuatToEuler(Quaternion q1)
        {
            double heading, attitude, bank;
            double test = q1.X * q1.Y + q1.Z * q1.W;
            if (test > 0.499) // singularity at north pole
            {
                heading = 2 * Math.Atan2(q1.X, q1.W);
                attitude = Math.PI / 2;
                bank = 0;
            }
            else if (test < -0.499) // singularity at south pole
            {
                heading = -2 * Math.Atan2(q1.X, q1.W);
                attitude = -Math.PI / 2;
                bank = 0;
            }
            else
            {
                double sqx = q1.X * q1.X;
                double sqy = q1.Y * q1.Y;
                double sqz = q1.Z * q1.Z;
                heading = Math.Atan2(2 * q1.Y * q1.W - 2 * q1.X * q1.Z, 1 - 2 * sqy - 2 * sqz);
                attitude = Math.Asin(2 * test);
                bank = Math.Atan2(2 * q1.X * q1.W - 2 * q1.Y * q1.Z, 1 - 2 * sqx - 2 * sqz);
            }
            Vector3 Rotation;
            Rotation.Y = (float)heading * MathExt.RadToDeg;
            Rotation.Z = (float)attitude * MathExt.RadToDeg;
            Rotation.X = (float)bank * MathExt.RadToDeg;
            return Rotation;
        }
    }

    public static class Matrix4Ext
    {
        public static void MatrixToEuler(ref Matrix4 matrix, out float heading, out float attitude, out float bank)
        {
            if (matrix.M21 > 0.998)
            {
                heading = (float)Math.Atan2(matrix.M13, matrix.M33);
                attitude = (float)Math.PI / 2;
                bank = 0;
                return;
            }
            if (matrix.M21 < -0.998)
            {
                heading = (float)Math.Atan2(matrix.M13, matrix.M33);
                attitude = (float)-Math.PI / 2;
                bank = 0;
                return;
            }
            heading = (float)Math.Atan2(-matrix.M31, matrix.M11);
            bank = (float)Math.Atan2(-matrix.M23, matrix.M22);
            attitude = (float)Math.Asin(matrix.M21);
        }

        public static void CreateFromQuaternion(ref Quaternion q, out Matrix4 m)
        {
            m = Matrix4.Identity;
            float X = q.X;
            float Y = q.Y;
            float Z = q.Z;
            float W = q.W;
            float xx = X * X;
            float xy = X * Y;
            float xz = X * Z;
            float xw = X * W;
            float yy = Y * Y;
            float yz = Y * Z;
            float yw = Y * W;
            float zz = Z * Z;
            float zw = Z * W;
            m.M11 = 1 - 2 * (yy + zz);
            m.M21 = 2 * (xy - zw);
            m.M31 = 2 * (xz + yw);
            m.M12 = 2 * (xy + zw);
            m.M22 = 1 - 2 * (xx + zz);
            m.M32 = 2 * (yz - xw);
            m.M13 = 2 * (xz - yw);
            m.M23 = 2 * (yz + xw);
            m.M33 = 1 - 2 * (xx + yy);
        }
    }



    /* 
     * http://nehe.gamedev.net/data/lessons/lesson.asp?lesson=30
     * http://www.gamedev.net/reference/articles/article1026.asp
     * http://jgt.akpeters.com/papers/MollerTrumbore97/
     */
    public static class Intersection
    {
        static float Epsilon = 0.00001f;
        public static float U, V, T;

        /// <summary>
        /// leikkauskohta 3d-maailmassa
        /// </summary>
        public static Vector3 IntersectionPoint;

        /// <summary>
        /// kuinka lähelle objektia päästään
        /// </summary>
        public static float DistAdder = 1.0f;

        /// <summary>
        /// tarkista osuuko start->end vektori johonkin polyyn. 
        /// palauttaa true jos osuu, muuten false.
        /// 
        /// ei toimi jos modelia käännetty tai skaalattu.
        /// </summary>
        public static bool CheckIntersection(ref Vector3 start, ref Vector3 end, ref Model obj)
        {
            Vector3 position = obj.Position;
            Vector3 dir = end - start;
            float len = dir.Length + DistAdder;
            dir.Normalize();
            Vector3[] v = new Vector3[3];
            for (int e = 0; e < obj.VertexBuffer.Length; e += 3)
            {
                v[0] = obj.VertexBuffer[e].Position;
                v[1] = obj.VertexBuffer[e + 1].Position;
                v[2] = obj.VertexBuffer[e + 2].Position;
                v[0] += position;
                v[1] += position;
                v[2] += position;
                if (IntersectTriangle(ref start, ref dir, ref v[0], ref v[1], ref v[2]) == true)
                {
                    if (Math.Abs(T) > len) continue;
                    return true;
                }
            }
            return false;
        }

        public static bool IntersectTriangle(ref Vector3 orig, ref Vector3 dir, ref Vector3 v0, ref Vector3 v1, ref Vector3 v2)
        {
            float det, inv_det;

            // find vectors for two edges sharing vert0
            Vector3 edge1 = v1 - v0;
            Vector3 edge2 = v2 - v0;

            // begin calculating determinant - also used to calculate U parameter
            Vector3 pvec = Vector3.Cross(dir, edge2);

            // if determinant is near zero, ray lies in plane of triangle
            det = Vector3.Dot(edge1, pvec);

            if (det < Epsilon)
            {
                return false;
            }

            // calculate distance from vert0 to ray origin
            Vector3 tvec = orig - v0;

            // calculate U parameter and test bounds
            U = Vector3.Dot(tvec, pvec);
            if (U < 0.0 || U > det)
            {
                return false;
            }

            // prepare to test V parameter
            Vector3 qvec = Vector3.Cross(tvec, edge1);

            // calculate V parameter and test bounds
            V = Vector3.Dot(dir, qvec);
            if (V < 0.0 || U + V > det)
            {
                return false;
            }

            // calculate T, scale parameters, ray intersects triangle
            T = Vector3.Dot(edge2, qvec);
            inv_det = 1.0f / det;

            U *= inv_det;
            V *= inv_det;
            T *= inv_det;

            IntersectionPoint = v0 + (edge1 * U) + (edge2 * V);

            return true;
        }
    }

}
