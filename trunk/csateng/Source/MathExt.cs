#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2008-2014 mjt
 * This notice may not be removed from any source distribution.
 * See csat-license.txt for licensing details.
 */
#endregion
using System;
using OpenTK;
using System.Globalization;

namespace CSatEng
{
    public static class MathExt
    {
        public static readonly float RadToDeg = (float)(180 / Math.PI);
        public static readonly float DegToRad = (float)(Math.PI / 180);

        /// <summary>
        /// palauttaa str:stä float luvun
        /// </summary>
        public static float GetFloat(string str)
        {
            return float.Parse(str, CultureInfo.InvariantCulture);
            /*
            float n;
            if (float.TryParse(str, out n) == true) return n;
            str = str.Replace('.', ','); // pisteet pilkuiksi
            if (float.TryParse(str, out n) == true) return n;
            Log.Error("GetFloat failed: " + str);
            return 0;
             */ 
        }

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
            float t = 1.0f - (q.X * q.X) - (q.Y * q.Y) - (q.Z * q.Z);
            if (t < 0.0f) q.W = 0.0f;
            else q.W = (float)-Math.Sqrt(t);
        }

        public static Vector3 RotatePoint(Quaternion q, Vector3 v)
        {
            Vector3 outv;
            Quaternion inv = new Quaternion();
            inv.X = -q.X;
            inv.Y = -q.Y;
            inv.Z = -q.Z;
            inv.W = q.W;
            Quaternion norminv = Quaternion.Normalize(inv);
            Quaternion m = MultVec(q, v);
            Quaternion qm = Quaternion.Multiply(m, norminv);
            outv.X = qm.X;
            outv.Y = qm.Y;
            outv.Z = qm.Z;
            return outv;
        }

        public static Quaternion MultVec(Quaternion q, Vector3 v)
        {
            Quaternion outq = new Quaternion();
            outq.W = -(q.X * v.X) - (q.Y * v.Y) - (q.Z * v.Z);
            outq.X = ((q.W * v.X) + (q.Y * v.Z)) - (q.Z * v.Y);
            outq.Y = ((q.W * v.Y) + (q.Z * v.X)) - (q.X * v.Z);
            outq.Z = ((q.W * v.Z) + (q.X * v.Y)) - (q.Y * v.X);
            return outq;
        }

        public static float DotProduct(Quaternion qa, Quaternion qb)
        {
            return ((qa.W * qb.W) + (qa.X * qb.X) + (qa.Y * qb.Y) + (qa.Z * qb.Z));
        }

        public static Quaternion Slerp(Quaternion qa, Quaternion qb, float t)
        {
            // check for out-of range parameter and return edge points if so
            if (t <= 0.0) return qa;
            if (t >= 1.0) return qb;

            // compute "cosine of angle between quaternions" using dot product
            float cosOmega = DotProduct(qa, qb);

            // if negative dot, use -q1. two quaternions q and -q
            // represent the same Rotation, but may produce
            // different slerp. we chose q or -q to rotate using
            // the acute angle.
            float q1w = qb.W;
            float q1x = qb.X;
            float q1y = qb.Y;
            float q1z = qb.Z;

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
                Log.WriteLine("Slerp error (cosOmega " + cosOmega);
                return Quaternion.Identity;
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
            return new Quaternion(
                (k0 * qa.X) + (k1 * q1x),
                (k0 * qa.Y) + (k1 * q1y),
                (k0 * qa.Z) + (k1 * q1z),
                (k0 * qa.W) + (k1 * q1w));
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

}
