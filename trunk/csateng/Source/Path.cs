#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2011 mjt
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing details.
 */
#endregion
using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;

namespace CSatEng
{
    public class Path
    {
        public static Dictionary<string, Path> Paths = new Dictionary<string, Path>();

        List<Vector3> path = null;
        public bool LookAtNextPoint = true;

        public bool Looping = true;
        public float Time = 0;

        /// <summary>
        /// mitä objektia liikutetaan
        /// </summary>
        SceneNode attachedObj;

        public void AddPath(string name, Vertex[] path)
        {
            this.path = new List<Vector3>();
            for (int q = 0; q < path.Length; q++)
            {
                if (q < path.Length - 1 && path[q].Position == path[q + 1].Position)
                    this.path.Add(path[q].Position);
            }

            Paths.Add(name, this);
        }

        public static Path GetPath(string name)
        {
#if DEBUG
            if (Paths.ContainsKey(name) == false)
            {
                Log.WriteLine(name + " not found!");
                return null;
            }
#endif
            return Paths[name];
        }

        public void MakeCurve(int lod)
        {
            if (path == null) return;

            for (int c = 0; c < lod; c++)
            {
                List<Vector3> newPath = new List<Vector3>();
                newPath.Add(path[0]); // eka vertex talteen

                for (int q = 0; q < path.Count - 1; q++)
                {
                    Vector3 p0 = path[q];
                    Vector3 p1 = path[q + 1];
                    Vector3 Q, R;

                    // average the 2 original points to create 2 new points. For each
                    // CV, another 2 verts are created.
                    Q.X = 0.75f * p0.X + 0.25f * p1.X;
                    Q.Y = 0.75f * p0.Y + 0.25f * p1.Y;
                    Q.Z = 0.75f * p0.Z + 0.25f * p1.Z;

                    R.X = 0.25f * p0.X + 0.75f * p1.X;
                    R.Y = 0.25f * p0.Y + 0.75f * p1.Y;
                    R.Z = 0.25f * p0.Z + 0.75f * p1.Z;

                    newPath.Add(Q);
                    newPath.Add(R);
                }

                newPath.Add(path[path.Count - 1]); // vika vertex
                if (Looping) newPath.Add(path[0]); // eka vertex 

                // korvataan alkuperäinen reitti uudella reitillä
                path = newPath;
            }
            Log.WriteLine("NewPath: " + path.Count, true);
        }

        /// <summary>
        /// aseta obj seuraamaan pathia
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="loop"></param>
        /// <param name="lookAtNextPoint"></param>
        public void Attach(SceneNode obj, bool loop, bool lookAtNextPoint)
        {
            attachedObj = obj;
            obj.Position = path[0];
            this.Looping = loop;
            this.LookAtNextPoint = lookAtNextPoint;
        }


        public void Update(float updateTime)
        {
            Update(updateTime, Vector3.Zero);
        }
        public void Update(float updateTime, Vector3 lookAt)
        {
            Time += updateTime;

            int v1 = (int)Time;
            int v2 = v1 + 1;
            if ((v1 >= path.Count || v2 >= path.Count) && Looping == false) return;
            v1 %= path.Count;
            v2 %= path.Count;

            // laske Position reitillä
            Vector3 p1 = path[v1];
            Vector3 p2 = path[v2];
            Vector3 p = p2 - p1;
            float d = Time - (int)Time;
            p *= d;
            attachedObj.Position = p1 + p;

            Vector3 to = lookAt;

            // laske kohta johon katsotaan
            if (LookAtNextPoint)
            {
                to = (path[(v2 + 1) % path.Count]) - p2;
                to = p2 + (to * d);
            }

            if (attachedObj is Camera || LookAtNextPoint)
            {
                attachedObj.OrigOrientationMatrix = Matrix4.LookAt(attachedObj.Position, to, Vector3.UnitY);
            }
        }

        /// <summary>
        /// käydään path läpi, joka vertexin kohdalla (xz) etsitään y ja lisätään siihen yp.
        /// </summary>
        /// <param name="yp"></param>
        /// <param name="obj"></param>
        public void FixPathY(int yp, ref Model obj)
        {
            Vector3 start, end;
            for (int q = 0; q < path.Count; q++)
            {
                start = path[q];
                end = path[q];
                end.Y = -1000;  // vektorin toinen pää kaukana alhaalla
                if (Intersection.CheckIntersection(ref start, ref end, ref obj))
                {
                    Vector3 nv = new Vector3(path[q].X, Intersection.IntersectionPoint.Y + yp, path[q].Z);
                    path.Remove(path[q]);
                    path.Insert(q, nv);
                }
            }
        }

    }
}
