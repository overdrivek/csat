#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2008-2014 mjt
 * This notice may not be removed from any source distribution.
 * See csat-license.txt for licensing details.
 */
#endregion
using System.Collections.Generic;
using System.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace CSatEng
{
    public class GLSLShader
    {
        static string shaderFileName = "default.shader";
        static string shaderFlags = "";

        static Dictionary<string, GLSLShader> shaders = new Dictionary<string, GLSLShader>();

        /// <summary>
        /// käytetään esim shadow mappingissa, kun vaihdettu käyttöön depth-shader,
        /// mutta VBO rendaus ei sitä tiedä joten ei voida käyttää VBO:n Shader:ia.
        /// </summary>
        public static GLSLShader CurrentShader;

        int vertexObject = -1, fragmentObject = -1;
        public int ProgramID = -1;
        public string ShaderName = "";

        int[] uniformLoc = new int[100];
        enum U
        {
            vertex, normal, uv, uv2,
            projMatrix, modelMatrix, normalMatrix, textureMatrix,
            texMap, depthMap, lightmaskMap,
            matDiffuse, matSpec, matAmb,
            lightDiffuse, lightSpec, lightAmb, lightShininess, lightEnabled,
            fogColor, fogDensity,
            particlePower,
            light,
            light0, light1, light2, light3,
            last
        };
        static string[] uniformNames = { 
                                       "glVertex", "glNormal", "glTexCoord", "glTexCoord2", // attribs
                                       "glProjectionMatrix", "glModelViewMatrix", "glNormalMatrix", "glTextureMatrix",
                                       "textureMap", "depthMap", "lightmaskMap", 
                                       "materialDiffuse", "materialSpecular", "materialAmbient", 
                                       "lightDiffuse", "lightSpecular", "lightAmbient", "shininess", "enabled",
                                       "glFogColor", "glFogDensity",
                                       "particlePower",
                                       "glLight",
                                       "glLight[0]",
                                       "glLight[1]",
                                       "glLight[2]",
                                       "glLight[3]",
                                       };

        /// <summary>
        /// flagsit määrää mitä shaderista ladataan, esim "LIGHTING", "PERPIXEL", "FOG", "SHADOWS".
        /// pitää asettaa ennen 3d-mallin lataamista.
        /// </summary>
        public static void SetShader(string shaderName, string flags)
        {
            shaderFileName = shaderName;
            shaderFlags = flags;
        }
        public static GLSLShader Load()
        {
            if (shaderFileName == "")
                shaderFileName = "default.shader";

            return Load(GLSLShader.shaderFileName + ":" + GLSLShader.shaderFlags);
        }

        /// <summary>
        /// lataa glsl shader (vertex ja fragment shader samassa tiedostossa).
        /// asettaaa ladatun shaderin käyttöön.
        /// </summary>
        public static GLSLShader Load(string fileName)
        {
            if (fileName == "" || fileName == ":") return null;

            GLSLShader shader = new GLSLShader();
            shader.LoadShader(fileName);

            return shader;
        }

        void LoadShader(string shaderFileName)
        {
            string file = shaderFileName;
            if (shaderFileName.Contains(":")) file = shaderFileName.Substring(0, shaderFileName.IndexOf(':'));
            ShaderName = shaderFileName;

            using (StreamReader shd = new StreamReader(Settings.ShaderDir + file))
            {
                string shader = shd.ReadToEnd();
                CreateShaders(shaderFileName, shader);

                UseProgram();

                // hae attrib paikat (0-3)
                for (int q = 0; q < 4; q++)
                    uniformLoc[q] = GL.GetAttribLocation(ProgramID, uniformNames[q]);

                // hae uniformien paikat
                for (int q = 4; q != (int)U.last; q++)
                {
                    uniformLoc[q] = GL.GetUniformLocation(ProgramID, uniformNames[q]);
                }
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

            Log.WriteLine("Shader: " + shaderFileName);
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

#if DEBUG
#if SHOWSHADERS
            System.Console.WriteLine("----VS------");
            string[] lines = vs.Split('\n');
            for (int q = 0; q < lines.Length; q++)
                System.Console.WriteLine("" + (q + 1) + ": " + lines[q]);

            System.Console.WriteLine("----FS------");
            lines = fs.Split('\n');
            for (int q = 0; q < lines.Length; q++)
                System.Console.WriteLine("" + (q + 1) + ": " + lines[q]);

            System.Console.Write("\n");
#endif
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
            if (GL.GetProgramInfoLog(ProgramID).Contains("error"))
                Log.Error("GLSL compiling error.");

            GLExt.CheckGLError("GLSL");
        }

        public void SetUniform(string name, float val)
        {
            GL.Uniform1(GL.GetUniformLocation(ProgramID, name), val);
        }

        public void SetUniforms()
        {
            GL.UniformMatrix4(uniformLoc[(int)U.projMatrix], false, ref GLExt.ProjectionMatrix);
            GL.UniformMatrix4(uniformLoc[(int)U.modelMatrix], false, ref GLExt.ModelViewMatrix);

            if (VBO.FastRenderPass == true) return;

            if (uniformLoc[(int)U.normalMatrix] != -1)
            {
                Matrix4 normMatrix = GLExt.ModelViewMatrix;
                normMatrix.Row3 = new Vector4(0, 0, 0, 1);
                normMatrix = Matrix4.Transpose(Matrix4.Invert(normMatrix));
                GL.UniformMatrix4(uniformLoc[(int)U.normalMatrix], false, ref normMatrix);
            }
            if (uniformLoc[(int)U.textureMatrix] != -1) GL.UniformMatrix4(uniformLoc[(int)U.textureMatrix], false, ref GLExt.TextureMatrix);
            if (uniformLoc[(int)U.texMap] != -1) GL.Uniform1(uniformLoc[(int)U.texMap], Settings.COLOR_TEXUNIT);
            if (uniformLoc[(int)U.matDiffuse] != -1) GL.Uniform4(uniformLoc[(int)U.matDiffuse], GLExt.Color);

            if (uniformLoc[(int)U.fogColor] != -1)
            {
                GL.Uniform3(uniformLoc[(int)U.fogColor], Fog.Color);
                GL.Uniform1(uniformLoc[(int)U.fogDensity], Fog.Density);
            }

            if (uniformLoc[(int)U.depthMap] != -1)
            {
                GL.Uniform1(uniformLoc[(int)U.depthMap], Settings.DEPTH_TEXUNIT);
            }
            if (uniformLoc[(int)U.lightmaskMap] != -1)
            {
                ShadowMapping.BindLightMask();
                GL.Uniform1(uniformLoc[(int)U.lightmaskMap], Settings.LIGHTMASK_TEXUNIT);
            }
            if (uniformLoc[(int)U.particlePower] != -1)
            {
                GL.Uniform1(uniformLoc[(int)U.particlePower], Particles.ParticlePower);
            }


            // TODO multiple lights
            if (uniformLoc[(int)U.light] != -1)
            {
                GL.Uniform3(uniformLoc[(int)U.light], Light.Lights[0].Matrix.Row3.Xyz);

                GL.Uniform4(uniformLoc[(int)U.lightDiffuse], Light.Lights[0].Diffuse);
                GL.Uniform4(uniformLoc[(int)U.lightAmb], Light.Lights[0].Ambient);

                if (uniformLoc[(int)U.lightShininess] != -1) // shininess (jos löytyy, käytä perpixel lightingiä)
                {
                    GL.Uniform4(uniformLoc[(int)U.matSpec], Material.CurrentMaterial.SpecularColor);
                    GL.Uniform4(uniformLoc[(int)U.matAmb], Material.CurrentMaterial.AmbientColor);

                    GL.Uniform4(uniformLoc[(int)U.lightSpec], Light.Lights[0].Specular);
                    GL.Uniform1(uniformLoc[(int)U.lightShininess], Light.Lights[0].Shininess);
                }
            }
        }

        public void SetAttributes()
        {
            if (CurrentShader == null) return;

            GL.VertexAttribPointer(uniformLoc[(int)U.vertex], 3, VertexAttribPointerType.Float, true, Vertex.Size, 0);
            GL.EnableVertexAttribArray(uniformLoc[(int)U.vertex]);
            if (uniformLoc[(int)U.normal] != -1)
            {
                GL.VertexAttribPointer(uniformLoc[(int)U.normal], 3, VertexAttribPointerType.Float, true, Vertex.Size, Vector3.SizeInBytes);
                GL.EnableVertexAttribArray(uniformLoc[(int)U.normal]);
            }
            if (uniformLoc[(int)U.uv] != -1)
            {
                GL.VertexAttribPointer(uniformLoc[(int)U.uv], 3, VertexAttribPointerType.Float, true, Vertex.Size, 2 * Vector3.SizeInBytes);
                GL.EnableVertexAttribArray(uniformLoc[(int)U.uv]);
            }
            // TODO uv2
        }

        public void UseProgram()
        {
            if (CurrentShader != this)
            {
                CurrentShader = this;
                GL.UseProgram(ProgramID);
            }
        }

        public static void UnBindShader()
        {
            CurrentShader = null;
            GL.UseProgram(0);
        }

        public void Dispose()
        {
            if (shaders.ContainsKey(ShaderName) && ProgramID != -1)
            {
                if (fragmentObject != -1) GL.DeleteShader(fragmentObject);
                if (vertexObject != -1) GL.DeleteShader(vertexObject);
                GL.DeleteProgram(ProgramID);
                shaders.Remove(ShaderName);
                Log.WriteLine("Disposed: " + ShaderName, false);
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
