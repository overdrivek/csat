using System;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using System.Collections.Generic;

namespace CSatEng
{
    public class SortedList_Models
    {
        public float Len = 0;
        public Model model;
        public SortedList_Models(float len, Model model)
        {
            Len = len;
            this.model = model;
        }
    }

    public class SceneNode
    {
        static uint objectCount = 0;
        public string Name;
        public string UserDataID;

        public Vector3 Position, Scale = Vector3.One;
        public Vector3 Rotation;
        //public Matrix4 RotationMatrix = Matrix4.Identity;
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
        public Matrix4 WorldMatrix;

        /// <summary>
        /// scenenodeen liitetyt toiset scenenodet
        /// </summary>
        public List<SceneNode> Childs = new List<SceneNode>();

        static protected List<SceneNode> visibleObjects = new List<SceneNode>();
        static protected List<SortedList_Models> transparentObjects = new List<SortedList_Models>();

        /// <summary>
        /// kaikki objektit menee tähän,joten saadaan helposti poistettua kaikki datat
        /// </summary>
        static protected List<SceneNode> allObjects = new List<SceneNode>();

        public SceneNode()
        {
            Name = "node" + objectCount++;
            allObjects.Add(this);

            Childs.Add(this);
        }
        public SceneNode(string name)
        {
            Name = name;
            allObjects.Add(this);

            Childs.Add(this);
        }

        public SceneNode Search(string name)
        {
            GetList(true);
            foreach (SceneNode node in ObjList)
            {
                if (node.Name == name) return node;
            }
            return null;
        }

        static public void DisposeAll()
        {
            for (int q = 0; q < allObjects.Count; q++)
            {
                //allObjects[q].Dispose();
                
            }
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
                Log.WriteLine("Disposed: " + Name, true);
                Name = "";
            }
        }

        public void Add(SceneNode obj)
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

            Log.WriteLine(obj.Name + " added to " + Name + ".", true);
        }

        public void Remove(SceneNode obj)
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

            Log.WriteLine(obj.Name + " removed from " + Name + ".", true);
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

        void Translate(SceneNode node)
        {
            SceneNode obj = node;
            if (node == null) obj = this;

            GL.Translate(obj.Position);
            GL.Rotate(Rotation.Z, 0, 0, 1);
            GL.Rotate(Rotation.Y, 0, 1, 0);
            GL.Rotate(Rotation.X, 1, 0, 0);
            //GL.MultMatrix(ref RotationMatrix);
            GL.MultMatrix(ref OrigOrientationMatrix);

            GL.Scale(obj.Scale);
        }

        /// <summary>
        /// luo visible ja transparent listat näkyvistä objekteista.
        /// listoista renderoitavat obut ovat Modelit, partikkelit ja skybox.
        /// </summary>
        public void MakeLists()
        {
            foreach (SceneNode o in Childs)
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
                            Settings.NumOfObjects++;

                            if (m.IsTransparent == false) visibleObjects.Add(m);
                            else
                            {
                                float len = (Camera.cam.Position - m.Position).LengthSquared;
                                transparentObjects.Add(new SortedList_Models(len, m));
                            }
                        }
                    }
                }
                else
                {
                    if (o is Sky || o is Particles)
                        visibleObjects.Add(o);
                }

                if (o.Childs.Count > 0) o.MakeLists();
            }
        }

        public static List<SceneNode> ObjList = new List<SceneNode>();
        /// <summary>
        /// luo ObjList-listan kaikista childeistä
        /// </summary>
        /// <param name="setTrue"></param>
        public void GetList(bool setTrue)
        {
            if (setTrue) ObjList.Clear();
            foreach (SceneNode o in Childs)
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
            GL.PushMatrix();
            foreach (SceneNode o in Childs)
            {
                if (o == this) continue;
                GL.PushMatrix();

                if (getWMatrix)
                {
                    o.Translate(o);
                    GL.GetFloat(GetPName.ModelviewMatrix, out o.WorldMatrix);
                }
                else
                {
                    o.Translate(null);
                    GL.GetFloat(GetPName.ModelviewMatrix, out o.Matrix);
                }

                if (o.Childs.Count > 0)
                    o.CalcPositions(getWMatrix);

                GL.PopMatrix();
            }
            GL.PopMatrix();
        }

        protected void CalculatePositions()
        {
            CalcPositions(false);

            GL.PushMatrix();
            GL.LoadIdentity();
            CalcPositions(true);
            GL.PopMatrix();

            MakeLists();

            // järjestä läpinäkyvät listassa olevat objektit etäisyyden mukaan, kauimmaiset ekaks
            transparentObjects.Sort(delegate(SortedList_Models z1, SortedList_Models z2) { return z2.Len.CompareTo(z1.Len); });
        }

        protected virtual void RenderModel()
        {
        }

        public virtual void Render()
        {
            GL.PushMatrix();

            // lasketaan kaikkien objektien paikat valmiiksi. 
            // näkyvät objektit asetetaan visible ja transparent listoihin
            CalculatePositions();

            // renderointi
            foreach (SceneNode o in visibleObjects)
            {
                o.RenderModel();
            }

            foreach (SortedList_Models o in transparentObjects)
            {
                Model m = o.model;
                m.RenderModel();
            }

            visibleObjects.Clear();
            transparentObjects.Clear();

            GL.PopMatrix();
        }

        protected void Render(SceneNode obj)
        {
            obj.Render();
        }

    }
}
