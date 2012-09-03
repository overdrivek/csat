#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2008-2012 mjt
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing details.
 */
#endregion

using OpenTK.Graphics.OpenGL;

namespace CSatEng
{
    public class Renderable : Node
    {
        public Renderable()
        {
        }
        public Renderable(string name)
            : base(name)
        {
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
            foreach (Renderable o in visibleObjects)
            {
                o.RenderModel();
            }
            foreach (SortedList_Model o in transparentObjects)
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
            foreach (Renderable o in visibleObjects)
            {
                o.RenderModel();
            }
            foreach (SortedList_Model o in transparentObjects)
            {
                Model m = o.model;
                m.RenderModel();
            }
            Texture.UnBind(Settings.COLOR_TEXUNIT);
            GLExt.PopMatrix();
        }

        protected void Render(Renderable obj)
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
                    // TODO rendaa vain zbufferiin, tsekkaa miten se on tehty shadowmappingissa
                    //GL.Disable(EnableCap.Blend);
                    //GL.ColorMask(false, false, false, false);
                    //GL.Disable(EnableCap.CullFace);

                    GL.ReadBuffer(ReadBufferMode.ColorAttachment1);
                    GL.DrawBuffer(DrawBufferMode.ColorAttachment1);

                    GL.ClearColor(float.MaxValue, float.MaxValue, float.MaxValue, float.MaxValue);
                    destination.Clear();
                    GL.ClearColor(0.0f, 0.0f, 0.1f, 0);
                    VBO.FastRenderPass = true;
                    Particles.SetDepthProgram();
                    Render();
                    VBO.FastRenderPass = false;

                    // TODO rendaa vain zbufferiin, tsekkaa miten se on tehty shadowmappingissa
                    //GL.Enable(EnableCap.Blend);
                    //GL.ColorMask(true, true, true, true);
                    //GL.Enable(EnableCap.CullFace);

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
