#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2008-2012 mjt
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing details.
 */
#endregion
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace CSatEng
{
    public class Camera : Node
    {
        public static Camera cam;

        static bool is3D = false;
        public static float Near = 1f, Far = 1000, Fov = 45;

        public Camera()
        {
            Name = "camera";
            cam = this;
        }

        /// <summary>
        /// käytä fps kamerassa
        /// </summary>
        public void SetFPSCamera()
        {
            GLExt.LoadIdentity();
            GLExt.RotateX(-Rotation.X);
            GLExt.RotateY(-Rotation.Y);
            GLExt.RotateZ(-Rotation.Z);
            GLExt.Translate(-Position.X, -Position.Y, -Position.Z);
        }

        /// <summary>
        /// käytä esim jos kamera on pathissa
        /// </summary>
        public void SetCameraMatrix()
        {
            GLExt.LoadMatrix(ref OrigOrientationMatrix);
        }

        public static void Set2D(int width, int height)
        {
            is3D = false;
            Settings.Width = width;
            Settings.Height = height;

            GLExt.SetProjectionMatrix(Matrix4.CreateOrthographicOffCenter(0, width, 0, height, -1, 1));
            GLExt.LoadIdentity();
            GL.Viewport(0, 0, width, height);
            GL.Disable(EnableCap.CullFace);
            GLExt.SetLighting(false);
        }

        public static void Set3D(int width, int height, float near, float far)
        {
            is3D = true;
            Settings.Width = width;
            Settings.Height = height;
            Near = near;
            Far = far;

            GLExt.SetProjectionMatrix(Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(Fov), 
                (float)width / (float)height, near, far));
            GLExt.LoadIdentity();
            GL.Viewport(0, 0, width, height);
            GL.Enable(EnableCap.CullFace);
            GLExt.SetLighting(true);
        }

        public static void Set2D()
        {
            Set2D(Settings.Width, Settings.Height);
        }
        public static void Set3D()
        {
            Set3D(Settings.Width, Settings.Height, Camera.Near, Camera.Far);
        }

        public static void Resize(int width, int height, float near, float far)
        {
            if (is3D) Set3D(width, height, near, far);
            else Set2D(width, height);
        }
        public static void Resize()
        {
            Resize(Settings.Width, Settings.Height, Near, Far);
        }
    }
}
