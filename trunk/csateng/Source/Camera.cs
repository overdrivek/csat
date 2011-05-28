#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2011 mjt[matola@sci.fi]
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing details.
 */
#endregion
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace CSatEng
{
    public class Camera : SceneNode
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
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.Rotate(-Rotation.X, 1.0f, 0, 0);
            GL.Rotate(-Rotation.Y, 0, 1.0f, 0);
            GL.Rotate(-Rotation.Z, 0, 0, 1.0f);
            GL.Translate(-Position);

#if false
            System.Console.WriteLine("Camrot: "+ (-Rotation) );
#endif
        }

        /// <summary>
        /// käytä esim jos kamera on pathissa
        /// </summary>
        public void SetCameraMatrix()
        {
            GL.LoadMatrix(ref OrigOrientationMatrix);
        }

        public static void Set2D(int width, int height)
        {
            is3D = false;
            Settings.Width = width;
            Settings.Height = height;

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, width, 0, height, -1, 1);
            GL.Viewport(0, 0, width, height);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);
        }
        public static void Set3D(int width, int height, float near, float far)
        {
            is3D = true;
            Settings.Width = width;
            Settings.Height = height;
            Near = near;
            Far = far;

            GL.Viewport(0, 0, width, height);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            Matrix4 perpective = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(Fov), (float)width / (float)height, near, far);
            GL.LoadMatrix(ref perpective);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
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

