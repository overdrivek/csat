#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2008-2014 mjt
 * This notice may not be removed from any source distribution.
 * See csat-license.txt for licensing details.
 */
#endregion
/* 
http://nehe.gamedev.net/data/lessons/lesson.asp?lesson=30
http://www.gamedev.net/reference/articles/article1026.asp
http://jgt.akpeters.com/papers/MollerTrumbore97/
*/
using System;
using OpenTK;

namespace CSatEng
{
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
