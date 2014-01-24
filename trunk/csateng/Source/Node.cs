#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2008-2014 mjt
 * This notice may not be removed from any source distribution.
 * See csat-license.txt for licensing details.
 */
#endregion

// note: Node this == Childs[0]

using System;
using System.Collections.Generic;
using OpenTK;

namespace CSatEng
{
    public class SortedList_Model
    {
        public float Len = 0;
        public Model model;
        public SortedList_Model(float len, Model model)
        {
            Len = len;
            this.model = model;
        }
    }

    public class Node
    {
        /// <summary>
        /// nodeen liitetyt toiset nodet
        /// </summary>
        public List<Node> Childs = new List<Node>();
        static protected List<Node> visibleObjects = new List<Node>();
        static protected List<SortedList_Model> transparentObjects = new List<SortedList_Model>();
        public static uint ObjectCount = 0;

        public string Name;
        public string UserDataID;

        public Vector3 Position, Scale = Vector3.One;
        public Vector3 Rotation;
        public Matrix4 OrigOrientationMatrix = Matrix4.Identity; // alkup asento .scene tiedostosta (quat->matrix)

        /// <summary>
        /// objektin keskikohta. frustum culling tarvii tätä
        /// </summary>
        public Vector3 ObjCenter = new Vector3(0, 0, 0);

        /// <summary>
        /// objektin paikka ja asento kamerasta katsottuna
        /// </summary>
        public Matrix4 Matrix;

        /// <summary>
        /// objektin paikka ja asento world coordinaateissa (frustum culling vaatii tämän)
        /// </summary>
        public Matrix4 WorldMatrix = Matrix4.Identity;

        public Node()
        {
            Name = "node" + ObjectCount++;
            Childs.Add(this);
        }
        public Node(string name)
        {
            Name = name;
            Childs.Add(this);
        }

        public Node Search(string name)
        {
            GetList(true);
            foreach (Node node in ObjList)
            {
                if (node.Name == name) return node;
            }
            return null;
        }

        public virtual void Dispose()
        {
            if (Name != "")
            {
                GetList(true);
                for (int q = 0; q < ObjList.Count; q++)
                {
                    ObjList[q].Dispose();
                }
                Log.WriteLine("Disposed: " + Name, false);
                Name = "";
            }
        }

        public void Add(Node obj)
        {
            Childs.Add(obj);

            if (obj is Light)
            {
                Light.Lights.Add((Light)obj);
            }
            else if (obj is Camera)
            {
                Camera.cam = (Camera)obj;
            }

            Log.WriteLine(obj.Name + " added to " + Name + ".", false);
        }

        public void Remove(Node obj)
        {
            if (obj == null) return;
            if (obj is Light)
            {
                Light.Lights.Remove((Light)obj);
            }
            else if (obj is Camera)
            {
                Camera.cam = new Camera(); // luo uusi kamera niin vanha häviää
            }
            Childs.Remove(obj);

            Log.WriteLine(obj.Name + " removed from " + Name + ".", false);
        }

        // XZ-tasolla liikkuminen

        public void MoveXZ(float forward, float strafe)
        {
            if (forward != 0) MoveXZ(forward);
            if (strafe != 0) StrafeXZ(strafe);
        }
        public void MoveXZ(float f)
        {
            Position.X -= ((float)Math.Sin(Rotation.Y * MathExt.DegToRad) * f);
            Position.Z -= ((float)Math.Cos(Rotation.Y * MathExt.DegToRad) * f);
        }
        public void StrafeXZ(float f)
        {
            Position.X += ((float)Math.Cos(-Rotation.Y * MathExt.DegToRad) * f);
            Position.Z += ((float)Math.Sin(-Rotation.Y * MathExt.DegToRad) * f);
        }

        // 6DOF liikkuminen

        public void Move(float spd)
        {
            Vector3 rotation = Rotation * MathExt.DegToRad;
            Position.X -= (float)Math.Sin(rotation.Y) * (float)Math.Cos(-rotation.X) * spd;
            Position.Y -= (float)Math.Sin(-rotation.X) * spd;
            Position.Z -= (float)Math.Cos(rotation.Y) * (float)Math.Cos(-rotation.X) * spd;
        }
        public void Strafe(float spd)
        {
            Vector3 rotation = Rotation * MathExt.DegToRad;
            Position.X -= (float)Math.Sin(rotation.Y - 90f * MathExt.DegToRad) * spd;
            Position.Z -= (float)Math.Cos(rotation.Y - 90f * MathExt.DegToRad) * spd;
        }
        public void Strafe(float spd, float angle)
        {
            angle *= MathExt.DegToRad;
            Vector3 rotation = Rotation * MathExt.DegToRad;
            Position.X -= (float)Math.Sin(rotation.Y + angle) * spd;
            Position.Z -= (float)Math.Cos(rotation.Y + angle) * spd;
        }

        void Translate(Node node)
        {
            Node obj = node;
            if (node == null) obj = this;

            GLExt.Translate(obj.Position.X, obj.Position.Y, obj.Position.Z);
            GLExt.RotateZ(Rotation.Z);
            GLExt.RotateY(Rotation.Y);
            GLExt.RotateX(Rotation.X);
            GLExt.MultMatrix(ref OrigOrientationMatrix);
            GLExt.Scale(obj.Scale.X, obj.Scale.Y, obj.Scale.Z);
        }

        /// <summary>
        /// luo visible ja transparent listat näkyvistä objekteista.
        /// </summary>
        public void MakeLists()
        {
            foreach (Node o in Childs)
            {
                if (o == this) continue;

                Model m = o as Model;

                // jos objekti on Model
                if (m != null)
                {
                    if (m.Visible)
                    {
                        // tarkista onko objekti näkökentässä
                        Vector3 cent = m.ObjCenter;
                        cent.X += m.WorldMatrix.M41;
                        cent.Y += m.WorldMatrix.M42;
                        cent.Z += m.WorldMatrix.M43;
                        if (Frustum.ObjectInFrustum(cent, m.Boundings, m.Scale))
                        {
                            GameClass.NumOfObjects++;

                            if (m.IsTransparent == false) visibleObjects.Add(m);
                            else
                            {
                                float len = (Camera.cam.Position - m.Position).LengthSquared;
                                transparentObjects.Add(new SortedList_Model(len, m));
                            }
                        }
                    }
                }
                else
                {
                    if (o is Sky) visibleObjects.Add(o);
                }

                if (o.Childs.Count > 0) o.MakeLists();
            }
        }

        public static List<Node> ObjList = new List<Node>();
        /// <summary>
        /// luo ObjList-listan kaikista childeistä
        /// </summary>
        /// <param name="setTrue"></param>
        public void GetList(bool setTrue)
        {
            if (setTrue) ObjList.Clear();
            foreach (Node o in Childs)
            {
                if (o == this) continue;

                ObjList.Add(o);

                if (o.Childs.Count > 0) o.GetList(false);
            }

            if (setTrue && ObjList.Count == 0) ObjList.Add(this);
        }

        /// <summary>
        /// laskee joka objektin paikan ja ottaa sen talteen joko Matrix tai WMatrix taulukkoon
        /// </summary>
        /// <param name="getWMatrix"></param>
        public void CalcPositions(bool getWMatrix)
        {
            GLExt.PushMatrix();
            foreach (Node o in Childs)
            {
                if (o == this) continue;
                GLExt.PushMatrix();

                if (getWMatrix)
                {
                    o.Translate(o);
                    o.WorldMatrix = GLExt.ModelViewMatrix;
                }
                else
                {
                    o.Translate(null);
                    o.Matrix = GLExt.ModelViewMatrix;
                }

                if (o.Childs.Count > 0)
                    o.CalcPositions(getWMatrix);

                GLExt.PopMatrix();
            }
            GLExt.PopMatrix();
        }

        protected void CalculatePositions()
        {
            CalcPositions(false);

            GLExt.PushMatrix();
            GLExt.LoadIdentity();
            CalcPositions(true);
            GLExt.PopMatrix();

            MakeLists();

            // järjestä läpinäkyvät listassa olevat objektit etäisyyden mukaan, kauimmaiset ekaks
            transparentObjects.Sort(delegate(SortedList_Model z1, SortedList_Model z2) { return z2.Len.CompareTo(z1.Len); });
        }

    }
}
