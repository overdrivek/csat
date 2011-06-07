#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2011 mjt[matola@sci.fi]
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing details.
 */
#endregion
using System;
using System.IO;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;

namespace CSatEng
{
    public delegate void ShaderCallback(int programID);

    public class GLSLShader
    {
        static Dictionary<string, GLSLShader> shaders = new Dictionary<string, GLSLShader>();
        public static bool IsSupported = false;
        static int currentShaderID = -1;

        int vertexObject, fragmentObject;
        public int ProgramID = 0;
        string ShaderName = "";

        ShaderCallback callBack;

        /// <summary>
        /// lataa glsl shader (vertex ja fragment shader samassa tiedostossa)
        /// </summary>
        public static GLSLShader Load(string fileName, ShaderCallback callback)
        {
            GLSLShader shader = new GLSLShader();
            shader.LoadShader(fileName);
            shader.callBack = callback;
            return shader;
        }

        void LoadShader(string shaderFileName)
        {
            if (IsSupported == false) return;

            string file = shaderFileName;
            if (shaderFileName.Contains(":")) file = shaderFileName.Substring(0, shaderFileName.IndexOf(':'));
            ShaderName = shaderFileName;

            using (StreamReader shd = new StreamReader(Settings.ShaderDir + file))
            {
                string shader = shd.ReadToEnd();
                CreateShaders(shaderFileName, shader);
            }
        }

        void CreateShaders(string shaderFileName, string shaderStr)
        {
            int statusCode;
            string info;

            string flags = "";
            if (shaderFileName.Contains(":")) // jos flagit (esim :TEXTURE)
            {
                int start = shaderFileName.IndexOf(':');
                flags = shaderFileName.Substring(start, shaderFileName.Length - start);
            }

            // jos shader jo ladattu, k‰ytet‰‰n sit‰
            if (shaders.ContainsKey(shaderFileName))
            {
                ProgramID = shaders[shaderFileName].ProgramID;
                vertexObject = shaders[shaderFileName].vertexObject;
                fragmentObject = shaders[shaderFileName].fragmentObject;
                return;
            }

            Log.WriteLine("Shader: " + shaderFileName, true);
            shaders.Add(shaderFileName, this);
            if (shaderStr.Contains("[SETUP]") == false) shaderStr = "[SETUP]\n" + shaderStr;
            int s = shaderStr.IndexOf("[SETUP]") + 8;
            int v = shaderStr.IndexOf("[VERTEX]") + 9;
            int f = shaderStr.IndexOf("[FRAGMENT]") + 11;
            string set = shaderStr.Substring(s, v - s - 9);
            string vs = shaderStr.Substring(v, f - v - 11);
            string fs = shaderStr.Substring(f, shaderStr.Length - f);

            // k‰y flagsit l‰pi, #define flags
            string[] flag = flags.Split(':');
            for (int q = 0; q < flag.Length; q++)
            {
                if (flag[q].Length > 0) set += "\n#define " + flag[q];
            }

            // k‰y [SETUP] blokki l‰pi, aseta oikeat definet
            string[] setup = set.Split('\n');
            for (int q = 0; q < setup.Length; q++)
            {
                if (setup[q].StartsWith("//"))
                    continue; // skippaa kommentit
                else if (setup[q].StartsWith("#define"))
                {
                    vs = setup[q] + "\n" + vs;
                    fs = setup[q] + "\n" + fs;
                }
            }

            vertexObject = GL.CreateShader(ShaderType.VertexShader);
            fragmentObject = GL.CreateShader(ShaderType.FragmentShader);

            // Compile vertex Shader
            GL.ShaderSource(vertexObject, vs);
            GL.CompileShader(vertexObject);
            GL.GetShaderInfoLog(vertexObject, out info);
            GL.GetShader(vertexObject, ShaderParameter.CompileStatus, out statusCode);
            if (statusCode != 1)
            {
                Util.Error("GLSL: " + info);
            }

            if (info.IndexOf('.') > 0) Log.WriteLine(info.Substring(0, info.IndexOf('.')));

            // Compile vertex Shader
            GL.ShaderSource(fragmentObject, fs);
            GL.CompileShader(fragmentObject);
            GL.GetShaderInfoLog(fragmentObject, out info);
            GL.GetShader(fragmentObject, ShaderParameter.CompileStatus, out statusCode);
            if (statusCode != 1)
            {
                Util.Error("GLSL: " + info);
            }

            if (info.IndexOf('.') > 0) Log.WriteLine(info.Substring(0, info.IndexOf('.')));

            ProgramID = GL.CreateProgram();
            GL.AttachShader(ProgramID, fragmentObject);
            GL.AttachShader(ProgramID, vertexObject);
            GL.LinkProgram(ProgramID);

            Util.CheckGLError("GLSL");
        }

        /// <summary>
        /// k‰yt‰ shaderia
        /// </summary>
        public void UseProgram()
        {
            if (IsSupported == false) return;
            if (currentShaderID == ProgramID) return;
            currentShaderID = ProgramID;
            GL.UseProgram(ProgramID);
            if (callBack != null) callBack(ProgramID);
        }

        public static void UseProgram(int shaderID)
        {
            if (IsSupported == false) return;
            if (currentShaderID == shaderID) return;
            currentShaderID = shaderID;
            GL.UseProgram(shaderID);
        }

        public void Dispose()
        {
            if (shaders.ContainsKey(ShaderName) && ProgramID != 0)
            {
                if (fragmentObject != 0) GL.DeleteShader(fragmentObject);
                if (vertexObject != 0) GL.DeleteShader(vertexObject);
                GL.DeleteProgram(ProgramID);
                shaders.Remove(ShaderName);
                Log.WriteLine("Disposed: " + ShaderName, true);
            }
            ProgramID = fragmentObject = vertexObject = 0;
            ShaderName = "";
        }
        public static void DisposeAll()
        {
            List<string> shader = new List<string>();
            foreach (KeyValuePair<string, GLSLShader> dta in shaders) shader.Add(dta.Key);
            for (int q = 0; q < shader.Count; q++) shaders[shader[q]].Dispose();
            shaders.Clear();
        }

        /// <summary>
        /// lataa shaderit.
        /// jos meshnamessa on * merkki, ladataan shaderi kaikkiin mesheihin
        /// joissa on meshname nimess‰, eli esim  box*  lataa box1, box2, jne mesheihin shaderin.
        /// </summary>
        public static void LoadShader(Model model, string meshName, string shaderFileName, ShaderCallback callback)
        {
            if (GLSLShader.IsSupported == false) return;
            for (int q = 0; q < model.Childs.Count; q++)
            {
                Model child = (Model)model.Childs[q];

                if (meshName.Contains("*"))
                {
                    meshName = meshName.Trim('*');
                    if (child.Name.Contains(meshName))
                    {
                        child.Shader = new GLSLShader();
                        child.Shader = GLSLShader.Load(shaderFileName, callback);
                    }
                }
                else if (child.Name.Equals(meshName))
                {
                    child.Shader = new GLSLShader();
                    child.Shader = GLSLShader.Load(shaderFileName, callback);
                }
            }
        }

        /// <summary>
        /// lataa shaderit ja k‰yt‰ koko objektissa.
        /// </summary>
        public static void LoadShader(Model model, string shaderFileName, ShaderCallback callback)
        {
            if (GLSLShader.IsSupported == false) return;
            bool use = true;
            if (shaderFileName == "")
            {
                use = false;
            }
            model.GetList(true);

            for (int q = 0; q < SceneNode.ObjList.Count; q++)
            {
                if (SceneNode.ObjList[q] is Model)
                {
                    Model child = (Model)SceneNode.ObjList[q];
                    if (use == true)
                    {
                        if (child.Shader != null) child.Shader.Dispose();
                        child.Shader = GLSLShader.Load(shaderFileName, callback);
                    }
                    else
                    {
                        child.Shader = null;
                    }
                }
            }
        }
    }
}
