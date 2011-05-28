#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2011 mjt[matola@sci.fi]
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing details.
 */
#endregion
using System.Collections.Generic;
using System;
using System.Collections;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace CSatEng
{
    public class Model : SceneNode, ICloneable
    {
        public Vertex[] VertexBuffer;
        public int[] IndexBuffer; // tämä on 0,1,2,3,4,..
        public VBO Vbo;
        public BoundingVolume Boundings;
        public GLSLShader Shader = null;
        public MaterialInfo Material = new MaterialInfo();
        public string MaterialName = "";

        public bool IsRenderable = true;
        public bool DoubleSided = false;
        public bool IsTransparent = false;
        public bool CastShadow = true;
        public bool Visible = true;

        public virtual void SetDoubleSided(string name, bool doublesided) { }

        public virtual void LoadMD5Animation(string animName, string fileName) { }
        public virtual void SetAnimation(string animName) { }
        public virtual void Update(float time) { }

        object ICloneable.Clone()
        {
            return this.Clone();
        }

        /// <summary>
        /// Palauttaa objektin kloonin.
        /// </summary>
        /// <returns></returns>
        public Model Clone()
        {
            Model clone = (Model)this.MemberwiseClone();

            // eri grouppi eli kloonattuihin objekteihin voi lisäillä muita objekteja
            // sen vaikuttamatta alkuperäiseen.
            //clone.Childs = new List<SceneNode>(Childs);
            //CloneTree(clone);

            return clone;
        }
        /*
        void CloneTree(Model clone)
        {
            for (int q = 1; q < Childs.Count; q++)
            {
                Model child = (Model)Childs[q];
                clone.Childs[q] = child.Clone();

                if (child.Childs.Count > 0) child.CloneTree(child);
            }
        }*/

    }
}
