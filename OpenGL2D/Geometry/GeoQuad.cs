using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace OpenGL2D.Geometry
{
    /// <summary>
    /// Geometry class for a quad (two triangles)
    /// </summary>
    internal class GeoQuad
    {
        public GeoQuad()
        {
            mTextureHandle = MainWindow.Window.DefaultTexture;
        }

        internal void SetTexture(int id)
        {
            mTextureHandle = id;
        }

        private static readonly float[] VERTICES = {
            // front faces
            -0.5f, +0.5f, +0.0f,
            -0.5f, -0.5f, +0.0f,
            +0.5f, -0.5f, +0.0f,
            +0.5f, -0.5f, +0.0f,
            +0.5f, +0.5f, +0.0f,
            -0.5f, +0.5f, +0.0f,
        };

        internal readonly static float[] UVS = {
            0, 1,
            0, 0,
            1, 0,
            1, 0,
            1, 1,
            0, 1
        };

        private static readonly float[] NORMALS = {
            0, 0, 1,
            0, 0, 1,
            0, 0, 1,
            0, 0, 1,
            0, 0, 1,
            0, 0, 1,
        };

        internal static float[] TANGENTS = new float[NORMALS.Length];
        internal static float[] BITANGENTS = new float[NORMALS.Length];

        internal int mTextureHandle = -1;
        internal int mTextureHandleHeightMap = -1;
        internal int mTextureHandleNormalMap = -1;

        public static int VAO = -1;
        internal static int VBOVertex = -1;
        internal static int VBONormal = -1;
        internal static int VBOTexture = -1;
        internal static int VBOTangent = -1;
        internal static int VBOBitangent = -1;

        private static void BuildTangents()
        {

            int t = 0;
            for (int i = 0; i < VERTICES.Length; i += 9)
            {
                Vector3 normal = new Vector3(NORMALS[i + 0], NORMALS[i + 1], NORMALS[i + 2]);

                // Shortcuts for vertices
                Vector3 v0 = new Vector3(VERTICES[i + 0], VERTICES[i + 1], VERTICES[i + 2]);
                Vector3 v1 = new Vector3(VERTICES[i + 3], VERTICES[i + 4], VERTICES[i + 5]);
                Vector3 v2 = new Vector3(VERTICES[i + 6], VERTICES[i + 7], VERTICES[i + 8]);

                // Shortcuts for UVs
                Vector2 uv0 = new Vector2(UVS[t + 0], UVS[t + 1]);
                Vector2 uv1 = new Vector2(UVS[t + 2], UVS[t + 3]);
                Vector2 uv2 = new Vector2(UVS[t + 4], UVS[t + 5]);

                t += 6;

                // Edges of the triangle : postion delta
                Vector3 deltaPos1 = v1 - v0;
                Vector3 deltaPos2 = v2 - v0;

                Vector2 deltaUV1 = uv1 - uv0;
                Vector2 deltaUV2 = uv2 - uv0;

                float r = 1.0f / (deltaUV1.X * deltaUV2.Y - deltaUV1.Y * deltaUV2.X);
                Vector3 tmp1 = deltaPos1 * deltaUV2.Y;
                Vector3 tmp2 = deltaPos2 * deltaUV1.Y;
                Vector3 tmp3 = tmp1 - tmp2;

                Vector3 tangent = tmp3 * r;
                tmp3 = normal * Vector3.Dot(normal, tangent);
                tangent = tangent - tmp3;

                Vector3 bitangent = new Vector3();
                tmp1 = deltaPos2 * deltaUV1.X;
                tmp2 = deltaPos1 * deltaUV2.X;
                bitangent = tmp1 - tmp2;
                bitangent = bitangent * r;

                // Set the same tangent for all three vertices of the triangle.
                TANGENTS[i + 0] = tangent.X;
                TANGENTS[i + 1] = tangent.Y;
                TANGENTS[i + 2] = tangent.Z;
                TANGENTS[i + 3] = tangent.X;
                TANGENTS[i + 4] = tangent.Y;
                TANGENTS[i + 5] = tangent.Z;
                TANGENTS[i + 6] = tangent.X;
                TANGENTS[i + 7] = tangent.Y;
                TANGENTS[i + 8] = tangent.Z;

                // Same thing for binormals
                BITANGENTS[i + 0] = bitangent.X;
                BITANGENTS[i + 1] = bitangent.Y;
                BITANGENTS[i + 2] = bitangent.Z;
                BITANGENTS[i + 3] = bitangent.X;
                BITANGENTS[i + 4] = bitangent.Y;
                BITANGENTS[i + 5] = bitangent.Z;
                BITANGENTS[i + 6] = bitangent.X;
                BITANGENTS[i + 7] = bitangent.Y;
                BITANGENTS[i + 8] = bitangent.Z;
            }
        }

        internal static void InitialiseStatic()
        {
            if (VAO > -1)
            {
                GL.DeleteVertexArray(VAO);
                GL.DeleteBuffer(VBOVertex);
                GL.DeleteBuffer(VBONormal);
                GL.DeleteBuffer(VBOTexture);
                GL.DeleteBuffer(VBOTangent);
                GL.DeleteBuffer(VBOBitangent);
            }

            BuildTangents();

            VAO = GL.GenVertexArray();
            GL.BindVertexArray(VAO);
            //position:
            VBOVertex = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBOVertex);
            GL.BufferData(BufferTarget.ArrayBuffer, VERTICES.Length * 4, VERTICES, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);
            GL.EnableVertexAttribArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            //normals:
            VBONormal = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBONormal);
            GL.BufferData(BufferTarget.ArrayBuffer, NORMALS.Length * 4, NORMALS, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 0, 0);
            GL.EnableVertexAttribArray(1);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            //uvs:
            VBOTexture = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBOTexture);
            GL.BufferData(BufferTarget.ArrayBuffer, UVS.Length * 4, UVS, BufferUsageHint.DynamicDraw);
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 0, 0);
            GL.EnableVertexAttribArray(2);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            //tangents:
            VBOTangent = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBOTangent);
            GL.BufferData(BufferTarget.ArrayBuffer, TANGENTS.Length * 4, TANGENTS, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, false, 0, 0);
            GL.EnableVertexAttribArray(3);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            //bitangents:
            VBOBitangent = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBOBitangent);
            GL.BufferData(BufferTarget.ArrayBuffer, BITANGENTS.Length * 4, BITANGENTS, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(4, 3, VertexAttribPointerType.Float, false, 0, 0);
            GL.EnableVertexAttribArray(4);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
        }
    }
}

