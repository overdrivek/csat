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
    public class Model : SceneNode
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
    }
}
