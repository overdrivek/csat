#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2011 mjt[matola@sci.fi]
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
            Billboard bb = new Billboard();
            bb.billBoard = Texture2D.Load(fileName);
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
            GL.PushAttrib(AttribMask.ColorBufferBit | AttribMask.EnableBit | AttribMask.PolygonBit);
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.Lighting);
            if (blend)
            {
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            }
            else
            {
                GL.Enable(EnableCap.AlphaTest);
                GL.AlphaFunc(AlphaFunction.Greater, Texture2D.AlphaMin);
            }

            GL.PushMatrix();
            GL.Translate(x, y, z);
            size *= 0.1f;
            GL.Scale(size, size, size);
            GL.Rotate(zrot, 0, 0, 1);

            Matrix4 matrix = Matrix4.Identity, modelViemMatrix;
            GL.GetFloat(GetPName.ModelviewMatrix, out modelViemMatrix);
            matrix.Row3 = modelViemMatrix.Row3;
            GL.LoadMatrix(ref matrix);

            billBoard.Vbo.Render();
            GL.PopAttrib();
            GL.PopMatrix();
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
                Log.WriteLine("Disposed: Billboard", true);
            }
        }
    }
}
