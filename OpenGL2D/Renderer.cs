using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenGL2D.Geometry;
using System;
using System.IO;
using System.Reflection;

namespace OpenGL2D
{
    public class Renderer
    {
        protected int mProgramId = -1;
        protected int mShaderFragmentId = -1;
        protected int mShaderVertexId = -1;

        protected int mAttribute_vpos = -1;
        protected int mAttribute_vnormal = -1;
        protected int mAttribute_vnormaltangent = -1;
        protected int mAttribute_vnormalbitangent = -1;
        protected int mAttribute_vtexture = -1;

        protected int mUniform_MVP = -1;
        protected int mUniform_NormalMatrix = -1;
        protected int mUniform_ModelMatrix = -1;
        protected int mUniform_Texture = -1;

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

        protected int LoadShader(Stream pFileStream, ShaderType pType, int pProgram)
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

        public int GetProgramId()
        {
            return mProgramId;
        }

        public int GetAttributeHandlePosition()
        {
            return mAttribute_vpos;
        }

        public int GetAttributeHandleNormals()
        {
            return mAttribute_vnormal;
        }

        public int GetAttributeHandleNormalTangents()
        {
            return mAttribute_vnormaltangent;
        }

        public int GetAttributeHandleNormalBiTangents()
        {
            return mAttribute_vnormalbitangent;
        }

        public int GetAttributeHandleTexture()
        {
            return mAttribute_vtexture;
        }

        public int GetUniformHandleMVP()
        {
            return mUniform_MVP;
        }

        public int GetUniformHandleTexture()
        {
            return mUniform_Texture;
        }

        public void Draw(GeoQuad quad, ref Matrix4 vp, ref Matrix4 normalMatrix, ref Matrix4 modelMatrix)
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
