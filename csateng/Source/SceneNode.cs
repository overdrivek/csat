#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2011 mjt
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing details.
 */
#endregion

// note: SceneNode this == Childs[0]

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
        /// <summary>
        /// scenenodeen liitetyt toiset scenenodet
        /// </summary>
        public List<SceneNode> Childs = new List<SceneNode>();
        static protected List<SceneNode> visibleObjects = new List<SceneNode>();
        static protected List<SortedList_Models> transparentObjects = new List<SortedList_Models>();
        public static uint ObjectCount = 0;

        public string Name;
        public string UserDataID;

        public Vector3 Position, Scale = Vector3.One;
        public Vector3 Rotation;
        public Matrix4 OrigOrientationMatrix = Matrix4.Identity; // alkup asento .scene tiedostosta (quat->matrix)

        /// <summary>
        /// objektin keskikohta. frustum culling tarvii t‰t‰
        /// </summary>
        public Vector3 ObjCenter = new Vector3(0, 0, 0);

        /// <summary>
        /// objektin paikka ja asento kamerasta katsottuna
        /// </summary>
        public Matrix4 Matrix;

        /// <summary>
        /// objektin paikka ja asento world coordinaateissa (frustum culling vaatii t‰m‰n)
        /// </summary>
        public Matrix4 WorldMatrix = Matrix4.Identity;

        public SceneNode()
        {
            Name = "node" + ObjectCount++;
            Childs.Add(this);
        }
        public SceneNode(string name)
        {
            Name = name;
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
                Camera.cam = new Camera(); // luo uusi kamera niin vanha h‰vi‰‰
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

            GLExt.Translate(obj.Position.X, obj.Position.Y, obj.Position.Z);
            GLExt.RotateZ(Rotation.Z);
            GLExt.RotateY(Rotation.Y);
            GLExt.RotateX(Rotation.X);
            GLExt.MultMatrix(ref OrigOrientationMatrix);
            GLExt.Scale(obj.Scale.X, obj.Scale.Y, obj.Scale.Z);
        }

        /// <summary>
        /// luo visible ja transparent listat n‰kyvist‰ objekteista.
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
                        // tarkista onko objekti n‰kˆkent‰ss‰
                        Vector3 cent = m.ObjCenter;
                        cent.X += m.WorldMatrix.M41;
                        cent.Y += m.WorldMatrix.M42;
                        cent.Z += m.WorldMatrix.M43;
                        if (Frustum.ObjectInFrustum(cent, m.Boundings, m.Scale))
                        {
                            BaseGame.NumOfObjects++;

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
                    if (o is Sky) visibleObjects.Add(o);
                }

                if (o.Childs.Count > 0) o.MakeLists();
            }
        }

        public static List<SceneNode> ObjList = new List<SceneNode>();
        /// <summary>
        /// luo ObjList-listan kaikista childeist‰
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
            GLExt.PushMatrix();
            foreach (SceneNode o in Childs)
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

            // j‰rjest‰ l‰pin‰kyv‰t listassa olevat objektit et‰isyyden mukaan, kauimmaiset ekaks
            transparentObjects.Sort(delegate(SortedList_Models z1, SortedList_Models z2) { return z2.Len.CompareTo(z1.Len); });
        }

        protected virtual void RenderModel()
        {
        }

        /// <summary>
        /// lasketaan objektien paikka ja lis‰t‰‰n n‰kyv‰t objektit listoihin, sitten renderoidaan n‰kyv‰t.
        /// </summary>
        public virtual void Render()
        {
            Light.UpdateLights();
            Frustum.CalculateFrustum();

            visibleObjects.Clear();
            transparentObjects.Clear();

            GLExt.PushMatrix();

            // lasketaan kaikkien objektien paikat valmiiksi. 
            // n‰kyv‰t objektit asetetaan visible ja transparent listoihin
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
            Texture.UnBind(Settings.COLOR_TEXUNIT);
            GLExt.PopMatrix();
        }

        /// <summary>
        /// renderoidaan n‰kyv‰t objektit listoista jotka Render() metodi on luonut.
        /// </summary>
        public void RenderAgain()
        {
            Light.UpdateLights();
            GLExt.PushMatrix();

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
            Texture.UnBind(Settings.COLOR_TEXUNIT);
            GLExt.PopMatrix();
        }

        protected void Render(SceneNode obj)
        {
            obj.Render();
        }

        public void RenderSceneWithParticles(FBO destination)
        {
            if (Particles.SoftParticles)
            {
                if (destination.ColorTextures.Length < 2) Log.Error("RenderSceneWithParticles: fbo must have at least 2 colorbuffers.");

                // rendaa skenen depth colorbufferiin, ei textureita/materiaaleja
                destination.BindFBO();
                {
                    GL.ReadBuffer(ReadBufferMode.ColorAttachment1);
                    GL.DrawBuffer(DrawBufferMode.ColorAttachment1);

                    GL.ClearColor(float.MaxValue, float.MaxValue, float.MaxValue, float.MaxValue);
                    destination.Clear();
                    GL.ClearColor(0.0f, 0.0f, 0.1f, 0);
                    VBO.FastRenderPass = true;
                    Particles.SetDepthProgram();
                    Render();
                    VBO.FastRenderPass = false;

                    // rendaa skene uudelleen textureineen
                    GL.ReadBuffer(ReadBufferMode.ColorAttachment0);
                    GL.DrawBuffer(DrawBufferMode.ColorAttachment0);

                    destination.Clear();
                    RenderAgain();

                    GL.ReadBuffer(ReadBufferMode.ColorAttachment1);
                    destination.BindColorBuffer(1, Settings.DEPTH_TEXUNIT);
                    Particles.Render();
                    destination.UnBindColorBuffer(Settings.DEPTH_TEXUNIT);
                    GL.ReadBuffer(ReadBufferMode.ColorAttachment0);
                }
                destination.UnBindFBO();
            }
            else
            {
                // rendaa skene textureineen
                destination.BindFBO();
                {
                    destination.Clear();
                    Render();
                    Particles.Render();
                }
                destination.UnBindFBO();
            }
        }

    }
}
