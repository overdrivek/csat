#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2008-2012 mjt
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing details.
 */
#endregion
using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace CSatEng
{
    public class Billboard
    {
        Texture2D billBoard;

        public static Billboard Load(string fileName)
        {
            return Load(fileName, true);
        }
        public static Billboard Load(string fileName, bool softParticle)
        {
            if (Particles.SoftParticles == false) softParticle = false;

            Billboard bb = new Billboard();
            bb.billBoard = Texture2D.Load(fileName, true);
            bb.billBoard.Vbo.Shader = GLSLShader.Load("particles.shader" + (softParticle ? ":SOFT" : ""));
            return bb;
        }

        public void Bind(int texUnit)
        {
            billBoard.Bind(texUnit);
        }

        /// <summary>
        /// renderoi billboard texture (ei aseta tiloja ym)
        /// </summary>
        public void RenderBillboard()
        {
            billBoard.Vbo.Render();
        }

        /// <summary>
        /// renderoi yhden billboardin.
        /// </summary>
        public void RenderBillboard(float x, float y, float z, float zrot, float size, bool blend)
        {
            billBoard.Bind(0);
            GL.Disable(EnableCap.CullFace);

            if (blend)
            {
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            }
            else
            {
                if (GLSLShader.IsSupported == false)
                {
                    GL.Enable(EnableCap.AlphaTest);
                    GL.AlphaFunc(AlphaFunction.Greater, 0.1f);
                }
            }

            GLExt.PushMatrix();
            {
                GLExt.Translate(x, y, z);
                size *= 0.1f;
                GLExt.Scale(size, size, size);
                GLExt.RotateZ(zrot);
                Matrix4 matrix = Matrix4.Identity;
                matrix.Row3 = GLExt.ModelViewMatrix.Row3;
                GLExt.ModelViewMatrix = matrix;

                GLExt.SetLighting(false);
                billBoard.Vbo.Render();
                GLExt.SetLighting(true);
            }
            GLExt.PopMatrix();

            if (blend) GL.Disable(EnableCap.Blend);
            else if (GLSLShader.IsSupported == false) GL.Disable(EnableCap.AlphaTest);
        }

        public void RenderBillboard(Vector3 pos, float zrot, float size, bool blend)
        {
            RenderBillboard(pos.X, pos.Y, pos.Z, zrot, size, blend);
        }

        public void Dispose()
        {
            if (billBoard.Vbo != null)
            {
                billBoard.Vbo.Dispose();
                billBoard.Vbo = null;
                Log.WriteLine("Disposed: Billboard", false);
            }
        }
    }
}
