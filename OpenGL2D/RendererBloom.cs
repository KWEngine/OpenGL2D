using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenGL2D.Geometry;
using System;
using System.IO;
using System.Reflection;

namespace OpenGL2D
{
    /// <summary>
    /// Renderer class
    /// </summary>
    public sealed class RendererBloom
    {
        private int mProgramId = -1;
        private int mShaderFragmentId = -1;
        private int mShaderVertexId = -1;

        private int mAttribute_vpos = -1;
        private int mAttribute_vtexture = -1;

        private int mUniform_MVP = -1;
        private int mUniform_TextureBloom = -1;
        private int mUniform_TextureScene = -1;
        private int mUniform_Merge = -1;
        private int mUniform_Horizontal = -1;
        private int mUniform_Resolution = -1;

        /// <summary>
        /// Initializes the standard render program (shader compilation)
        /// </summary>
        public RendererBloom()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceNameVertexShader = "OpenGL2D.Shaders.shader_vertex_bloom.glsl";
            string resourceNameFragmentShader = "OpenGL2D.Shaders.shader_fragment_bloom.glsl";

            mProgramId = GL.CreateProgram();
            using (Stream s = assembly.GetManifestResourceStream(resourceNameVertexShader))
            {
                mShaderVertexId = LoadShader(s, ShaderType.VertexShader, mProgramId);
                Console.WriteLine(GL.GetShaderInfoLog(mShaderVertexId));
            }
            using (Stream s = assembly.GetManifestResourceStream(resourceNameFragmentShader))
            {
                mShaderFragmentId = LoadShader(s, ShaderType.FragmentShader, mProgramId);
                Console.WriteLine(GL.GetShaderInfoLog(mShaderFragmentId));
            }



            if (mShaderFragmentId >= 0 && mShaderVertexId >= 0)
            {
                GL.BindAttribLocation(mProgramId, 0, "aPosition");
                GL.BindAttribLocation(mProgramId, 2, "aTexture");
                
                GL.BindFragDataLocation(mProgramId, 0, "color");

                GL.LinkProgram(mProgramId);
            }
            else
            {
                throw new Exception("Creating and linking shaders failed.");
            }

            mAttribute_vpos = GL.GetAttribLocation(mProgramId, "aPosition");
            mAttribute_vtexture = GL.GetAttribLocation(mProgramId, "aTexture");

            mUniform_MVP = GL.GetUniformLocation(mProgramId, "uMVP");
            mUniform_TextureScene = GL.GetUniformLocation(mProgramId, "uTextureScene");
            mUniform_TextureBloom = GL.GetUniformLocation(mProgramId, "uTextureBloom");
            mUniform_Merge = GL.GetUniformLocation(mProgramId, "uMerge");
            mUniform_Horizontal = GL.GetUniformLocation(mProgramId, "uHorizontal");
            mUniform_Resolution = GL.GetUniformLocation(mProgramId, "uResolution");
        }

        /// <summary>
        /// Loads a glsl shader file into memory
        /// </summary>
        /// <param name="pFileStream">file stream</param>
        /// <param name="pType">shader type (vertex or fragment)</param>
        /// <param name="pProgram">render program id</param>
        /// <returns></returns>
        private int LoadShader(Stream pFileStream, ShaderType pType, int pProgram)
        {
            int address = GL.CreateShader(pType);
            using (StreamReader sr = new StreamReader(pFileStream))
            {
                GL.ShaderSource(address, sr.ReadToEnd());
            }
            GL.CompileShader(address);
            GL.AttachShader(pProgram, address);
            return address;
        }

        /// <summary>
        /// Renderer program id
        /// </summary>
        /// <returns></returns>
        public int GetProgramId()
        {
            return mProgramId;
        }

        /// <summary>
        /// Draws a given quad with the current view-projection-matrix 
        /// </summary>
        /// <param name="quad">The quad to be rendered</param>
        /// <param name="mvp">the bloom model-view-projection matrix</param>
        /// <param name="bloomDirectionHorizontal">true if bloom is applied horizontally</param>
        /// <param name="merge">true if bloom will be merged with original scene</param>
        internal void DrawBloom(GeoQuad quad, ref Matrix4 mvp, bool bloomDirectionHorizontal, bool merge, int width, int height, int sceneTexture, int bloomTexture)
        {
            GL.UniformMatrix4(mUniform_MVP, false, ref mvp);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, bloomTexture);
            GL.Uniform1(mUniform_TextureBloom, 0);

            if (merge)
            {
                GL.ActiveTexture(TextureUnit.Texture1);
                GL.BindTexture(TextureTarget.Texture2D, sceneTexture);
                GL.Uniform1(mUniform_TextureScene, 1);

                GL.Uniform1(mUniform_Merge, 1);
            }
            else
            {
                GL.Uniform1(mUniform_Merge, 0);
            }
            
            GL.Uniform1(mUniform_Horizontal, bloomDirectionHorizontal ? 1 : 0);
            GL.Uniform2(mUniform_Resolution, (float)width, (float)height);

            GL.BindVertexArray(GeoQuad.VAO);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

            GL.BindVertexArray(0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }
    }
}
