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
    public sealed class Renderer
    {
        private int mProgramId = -1;
        private int mShaderFragmentId = -1;
        private int mShaderVertexId = -1;

        private int mAttribute_vpos = -1;
        private int mAttribute_vnormal = -1;
        private int mAttribute_vnormaltangent = -1;
        private int mAttribute_vnormalbitangent = -1;
        private int mAttribute_vtexture = -1;

        private int mUniform_MVP = -1;
        private int mUniform_NormalMatrix = -1;
        private int mUniform_ModelMatrix = -1;
        private int mUniform_Texture = -1;

        /// <summary>
        /// Initializes the standard render program (shader compilation)
        /// </summary>
        public Renderer()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceNameVertexShader = "OpenGL2D.Shaders.shader_vertex.glsl";
            string resourceNameFragmentShader = "OpenGL2D.Shaders.shader_fragment.glsl";

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
                GL.BindAttribLocation(mProgramId, 1, "aNormal");
                GL.BindAttribLocation(mProgramId, 2, "aTexture");
                GL.BindAttribLocation(mProgramId, 3, "aNormalTangent");
                GL.BindAttribLocation(mProgramId, 4, "aNormalBiTangent");
                GL.BindFragDataLocation(mProgramId, 0, "color");
                GL.LinkProgram(mProgramId);
            }
            else
            {
                throw new Exception("Creating and linking shaders failed.");
            }

            mAttribute_vpos = GL.GetAttribLocation(mProgramId, "aPosition");
            mAttribute_vtexture = GL.GetAttribLocation(mProgramId, "aTexture");
            mAttribute_vnormal = GL.GetAttribLocation(mProgramId, "aNormal");
            mAttribute_vnormaltangent = GL.GetAttribLocation(mProgramId, "aNormalTangent");
            mAttribute_vnormalbitangent = GL.GetAttribLocation(mProgramId, "aNormalBiTangent");
            mUniform_MVP = GL.GetUniformLocation(mProgramId, "uMVP"); // MVP = model-view-projection matrix
            mUniform_NormalMatrix = GL.GetUniformLocation(mProgramId, "uNormalMatrix"); // Points to the normal matrix
            mUniform_ModelMatrix = GL.GetUniformLocation(mProgramId, "uModelMatrix"); // Points to the normal matrix
            mUniform_Texture = GL.GetUniformLocation(mProgramId, "uTexture"); // Points to a texture

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
        /// <param name="quad">quad instance</param>
        /// <param name="vp">view-projection-matrix</param>
        /// <param name="normalMatrix">the quad's normal matrix</param>
        /// <param name="modelMatrix">the quad's model matrix</param>
        internal void Draw(GeoQuad quad, ref Matrix4 vp, ref Matrix4 normalMatrix, ref Matrix4 modelMatrix)
        {
            Matrix4 mvp = modelMatrix * vp;
            GL.UniformMatrix4(mUniform_MVP, false, ref mvp);
            GL.UniformMatrix4(mUniform_NormalMatrix, false, ref normalMatrix);
            GL.UniformMatrix4(mUniform_ModelMatrix, false, ref modelMatrix);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, quad.mTextureHandle);
            GL.Uniform1(mUniform_Texture, 0);

            GL.BindVertexArray(GeoQuad.VAO);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

            GL.BindVertexArray(0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }
    }
}
