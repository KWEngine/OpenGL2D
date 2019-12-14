using OpenGL2D.Geometry;
using OpenGL2D.Helpers;
using OpenTK;
using OpenTK.Input;
using System;

namespace OpenGL2D.Core
{
    abstract class Actor : IComparable
    {
        private int _number;
        public string Name { get; set; } = "undefined";
        private World _currentWorld = null;

        private Matrix4 _modelMatrix = new Matrix4();
        private Matrix4 _normalMatrix = new Matrix4();
        private GeoQuad _quad = new GeoQuad();

        private Vector3 _position = new Vector3(128, 128, 0);
        private Quaternion _rotation = new Quaternion(0, 0, 0, 1);
        private float _rotationDegrees = 0;
        private Vector3 _scale = new Vector3(256, 256, 1);
        private Vector4 _bloom = new Vector4(0, 0, 0, 0);

        public Vector2 GetScale()
        {
            return new Vector2(_scale.X, _scale.Y);
        }

        public Vector2 GetPosition()
        {
            return new Vector2(_position.X, _position.Y);
        }

        public float GetRotation()
        {
            return _rotationDegrees;
        }

        public MainWindow CurrentWindow
        {
            get
            {
                return MainWindow.Window;
            }
        }

        public void SetScale(float x, float y)
        {
            _scale.X = x >= 0 ? x : 64;
            _scale.Y = y >= 0 ? y : 64;
            _scale.Z = 1;
        }

        public void SetScale(Vector2 s)
        {
            SetScale(s.X, s.Y);
        }

        public void SetPosition(float x, float y)
        {
            _position.X = x;
            _position.Y = y;
            _position.Z = 0;
        }

        public void SetPosition(Vector2 pos)
        {
            SetPosition(pos.X, pos.Y);
        }

        public void SetRotation(float rotationDegrees)
        {
            _rotationDegrees = rotationDegrees;
            _rotation = Quaternion.FromAxisAngle(Vector3.UnitZ, MathHelper.DegreesToRadians(rotationDegrees % 360));
        }

        public void SetBloom(float red, float green, float blue, float intensity)
        {
            _bloom.X = HelperGL.Clamp(red, 0, 1);
            _bloom.Y = HelperGL.Clamp(green, 0, 1);
            _bloom.Z = HelperGL.Clamp(blue, 0, 1);
            _bloom.W = HelperGL.Clamp(intensity, 0, 1);
        }

        public void SetTexture(string texture)
        {
            _quad.SetTexture(HelperTexture.LoadTextureFromDisk(texture, out int w, out int h));
            SetScale(w, h);
        }

        internal void SetWorld(World w)
        {
            _currentWorld = w;
        }

        public World GetWorld()
        {
            return _currentWorld;
        }

        internal void SetNumber(int n)
        {
            _number = n;
        }

        public int CompareTo(object obj)
        {
            if(obj != null && obj is Actor)
            {
                Actor a = (Actor)obj;
                int aNumber = a._number + a.GetWorld().GetDrawPriorityForType(a.GetType()) * 100000;
                int thisNumber = this._number + this.GetWorld().GetDrawPriorityForType(this.GetType()) * 100000;
                return thisNumber - aNumber;
            }
            throw new Exception("Could not compare non-Actor object with Actor.");
        }

        public abstract void Act(KeyboardState keyboardState, MouseState mouseState);
      
        internal void Draw()
        {
            _modelMatrix = Matrix4.CreateScale(_scale) * Matrix4.CreateFromQuaternion(_rotation) * Matrix4.CreateTranslation(_position);
            _normalMatrix = Matrix4.Invert(Matrix4.Transpose(_modelMatrix));
            Matrix4 currentViewProjectionMatrix = CurrentWindow.GetViewProjectionMatrix();

            CurrentWindow.RenderProgram.Draw(_quad, ref currentViewProjectionMatrix, ref _normalMatrix, ref _modelMatrix, ref _bloom);
        }
    }
}
