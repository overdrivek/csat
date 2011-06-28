#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2011 mjt
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing details.
 */
#endregion
#define SHOWSHADERS

using System.Collections.Generic;
using System.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace CSatEng
{
    public delegate void ShaderCallback(int programID);

    public class GLSLShader
    {
        static Dictionary<string, GLSLShader> shaders = new Dictionary<string, GLSLShader>();
        public static bool IsSupported = true;
        public static GLSLShader CurrentShader;

        int vertexObject = -1, fragmentObject = -1;
        public int ProgramID = -1;
        public string ShaderName = "";

        ShaderCallback callBack;

        int projMatrixLoc = -1, modelMatrixLoc = -1, normalMatrixLoc = -1;
        int texMapLoc, lightLoc, vertexLoc, normalLoc, UVLoc, textureMatrixLoc, materialDiffuseLoc;
        int fogColorLoc, fogDensityLoc;

        /// <summary>
        /// lataa glsl shader (vertex ja fragment shader samassa tiedostossa).
        /// asettaaa ladatun shaderin käyttöön.
        /// </summary>
        public static GLSLShader Load(string fileName, ShaderCallback callback)
        {
            GLSLShader shader = new GLSLShader();
            shader.LoadShader(fileName);

            shader.callBack = callback;
            if (fileName.Contains("SHADOWS")) shader.callBack = CallBacks.ShadowShaderCallBack;
            if (fileName.Contains("LIGHTING")) shader.callBack = CallBacks.LightingShaderCallBack;

            shader.UseProgram();
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
                UseProgram();
                vertexLoc = GL.GetAttribLocation(ProgramID, "glVertex");
                normalLoc = GL.GetAttribLocation(ProgramID, "glNormal");
                UVLoc = GL.GetAttribLocation(ProgramID, "glTexCoord");
                projMatrixLoc = GL.GetUniformLocation(ProgramID, "glProjectionMatrix");
                modelMatrixLoc = GL.GetUniformLocation(ProgramID, "glModelViewMatrix");
                normalMatrixLoc = GL.GetUniformLocation(ProgramID, "glNormalMatrix");
                textureMatrixLoc = GL.GetUniformLocation(ProgramID, "glTextureMatrix");
                texMapLoc = GL.GetUniformLocation(ProgramID, "textureMap");
                lightLoc = GL.GetUniformLocation(ProgramID, "glLight");
                materialDiffuseLoc = GL.GetUniformLocation(ProgramID, "materialDiffuse"); // tätä väriä voidaan muuttaa GLExt.Color metodilla
                fogColorLoc = GL.GetUniformLocation(ProgramID, "glFogColor");
                fogDensityLoc = GL.GetUniformLocation(ProgramID, "glFogDensity");
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

            // jos shader jo ladattu, käytetään sitä
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

            // käy flagsit läpi, #define flags
            string[] flag = flags.Split(':');
            for (int q = 0; q < flag.Length; q++)
            {
                if (flag[q].Length > 0) set += "\n#define " + flag[q];
            }

            // käy [SETUP] blokki läpi, aseta oikeat definet
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

            // konvaa gl2:n glsl koodit gl3:sen glsl:ksi
            if (Settings.UseGL3)
            {
                string gl3 = @"#version 130
                            precision highp float;
                            ";
                vs = gl3 + vs;
                vs = vs.Replace("attribute ", "in ");
                vs = vs.Replace("varying ", "out ");

                fs = gl3 + "out vec4 glFragColor;\n" + fs;
                fs = fs.Replace("gl_FragColor", "glFragColor");
                fs = fs.Replace("varying ", "in ");
                fs = fs.Replace("texture2D", "texture");
                fs = fs.Replace("shadow2DProj", "textureProj");
            }

#if SHOWSHADERS
            Log.WriteLine("----VS------");
            string[] lines = vs.Split('\n');
            for (int q = 0; q < lines.Length; q++) Log.WriteLine("" + (q + 1) + ": " + lines[q]);

            Log.WriteLine("----FS------");
            lines = fs.Split('\n');
            for (int q = 0; q < lines.Length; q++) Log.WriteLine("" + (q + 1) + ": " + lines[q]);
#endif

            vertexObject = GL.CreateShader(ShaderType.VertexShader);
            fragmentObject = GL.CreateShader(ShaderType.FragmentShader);

            // Compile vertex shader
            GL.ShaderSource(vertexObject, vs);
            GL.CompileShader(vertexObject);
            GL.GetShaderInfoLog(vertexObject, out info);
            GL.GetShader(vertexObject, ShaderParameter.CompileStatus, out statusCode);
            if (statusCode != 1) Log.Error(shaderFileName + ":\n" + info);
            else Log.WriteLine(info);

            if (info.IndexOf('.') > 0) Log.WriteLine(info.Substring(0, info.IndexOf('.')));

            // Compile fragment shader
            GL.ShaderSource(fragmentObject, fs);
            GL.CompileShader(fragmentObject);
            GL.GetShaderInfoLog(fragmentObject, out info);
            GL.GetShader(fragmentObject, ShaderParameter.CompileStatus, out statusCode);
            if (statusCode != 1) Log.Error(shaderFileName + ":\n" + info);
            else Log.WriteLine(info);

            if (info.IndexOf('.') > 0) Log.WriteLine(info.Substring(0, info.IndexOf('.')));

            ProgramID = GL.CreateProgram();
            GL.AttachShader(ProgramID, fragmentObject);
            GL.AttachShader(ProgramID, vertexObject);
            GL.LinkProgram(ProgramID);
            Log.WriteLine(GL.GetProgramInfoLog(ProgramID));

            GLExt.CheckGLError("GLSL");
        }

        public void SetUniforms()
        {
            GL.UniformMatrix4(projMatrixLoc, false, ref GLExt.ProjectionMatrix);
            GL.UniformMatrix4(modelMatrixLoc, false, ref GLExt.ModelViewMatrix);

            if (VBO.FastRenderPass == true) return;

            if (normalMatrixLoc != -1)
            {
                Matrix4 normMatrix = GLExt.ModelViewMatrix;
                normMatrix.Row3 = new Vector4(0, 0, 0, 1);
                normMatrix = Matrix4.Transpose(Matrix4.Invert(normMatrix));
                GL.UniformMatrix4(normalMatrixLoc, false, ref normMatrix);
            }
            if (textureMatrixLoc != -1) GL.UniformMatrix4(textureMatrixLoc, false, ref GLExt.TextureMatrix);
            if (texMapLoc != -1) GL.Uniform1(texMapLoc, Settings.COLOR_TEXUNIT);
            if (materialDiffuseLoc != -1) GL.Uniform4(materialDiffuseLoc, GLExt.Color);

            if (fogColorLoc != -1)
            {
                GL.Uniform3(fogColorLoc, Fog.Color);
                GL.Uniform1(fogDensityLoc, Fog.Density);
            }

            // todo: multiple lights
            if (lightLoc != -1)
            {
                Vector3 lp = Light.Lights[0].Matrix.Row3.Xyz;
                GL.Uniform3(lightLoc, lp);
            }
        }

        public void SetAttributes()
        {
            GL.VertexAttribPointer(vertexLoc, 3, VertexAttribPointerType.Float, true, Vertex.Size, 0);
            GL.EnableVertexAttribArray(vertexLoc);
            if (normalLoc != -1)
            {
                GL.VertexAttribPointer(normalLoc, 3, VertexAttribPointerType.Float, true, Vertex.Size, Vector3.SizeInBytes);
                GL.EnableVertexAttribArray(normalLoc);
            }
            if (UVLoc != -1)
            {
                GL.VertexAttribPointer(UVLoc, 3, VertexAttribPointerType.Float, true, Vertex.Size, 2 * Vector3.SizeInBytes);
                GL.EnableVertexAttribArray(UVLoc);
            }
        }

        public void UseProgram()
        {
            if (IsSupported == false) return;
            if (CurrentShader != this)
            {
                CurrentShader = this;
                GL.UseProgram(ProgramID);
            }
            if (callBack != null) callBack(ProgramID);
        }

        public void Dispose()
        {
            if (shaders.ContainsKey(ShaderName) && ProgramID != -1)
            {
                if (fragmentObject != -1) GL.DeleteShader(fragmentObject);
                if (vertexObject != -1) GL.DeleteShader(vertexObject);
                GL.DeleteProgram(ProgramID);
                shaders.Remove(ShaderName);
                Log.WriteLine("Disposed: " + ShaderName, true);
            }
            ProgramID = fragmentObject = vertexObject = -1;
            ShaderName = "";
        }
        public static void DisposeAll()
        {
            List<string> shader = new List<string>();
            foreach (KeyValuePair<string, GLSLShader> dta in shaders) shader.Add(dta.Key);
            for (int q = 0; q < shader.Count; q++) shaders[shader[q]].Dispose();
            shaders.Clear();
        }
    }
}
