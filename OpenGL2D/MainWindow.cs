
using System;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
using OpenGL2D.Core;
using OpenGL2D.Geometry;
using OpenGL2D.Helpers;
using System.Windows.Forms;

namespace OpenGL2D
{
    public sealed class MainWindow : GameWindow
    {
        private Matrix4 _viewMatrix = new Matrix4();
        private Matrix4 _projectionMatrix = new Matrix4();
        private Matrix4 _viewProjectionMatrix = new Matrix4();

        private static MainWindow S_WINDOW;
        private int _defaultTextureId = -1;

        private Renderer _renderProgram;
        private World _currentWorld = null;

        public int DefaultTexture
        {
            get
            {
                return _defaultTextureId;
            }
        }

        public static MainWindow Window
        {
            get
            {
                return S_WINDOW;
            }
        }

        public Renderer RenderProgram
        {
            get
            {
                return _renderProgram;
            }
        }

        public MainWindow(int w, int h)
            : base(w,
                h,
                GraphicsMode.Default,
                "OpenGL2D Setup", 
                GameWindowFlags.Default,
                DisplayDevice.Default,
                4, 
                3,
                GraphicsContextFlags.ForwardCompatible)
        {
            Title += " (Version: " + GL.GetString(StringName.Version) + ")";
            S_WINDOW = this;

            X = Screen.PrimaryScreen.WorkingArea.Width / 2 - Width / 2;
            Y = Screen.PrimaryScreen.WorkingArea.Height / 2 - Height / 2;
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            if (!Focused)
                return;

            KeyboardState keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Key.Escape))
            {
                Debug.WriteLine("ESC");
                Exit();
            }

            if (_currentWorld != null)
            {

                foreach (Actor a in _currentWorld.GetCurrentObjects())
                {
                    a.Act(keyboardState, Mouse.GetState());
                }

                _currentWorld.SortActorList();
            }

        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            _viewProjectionMatrix = _viewMatrix * _projectionMatrix;

            if (_currentWorld != null)
            {
                foreach (Actor a in _currentWorld.GetCurrentObjects())
                {
                    a.Draw();
                }
            }

            SwapBuffers();
        }

        internal Matrix4 GetViewProjectionMatrix()
        {
            return _viewProjectionMatrix;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            _renderProgram = new Renderer();
            GeoQuad.InitialiseStatic();
            _viewMatrix = Matrix4.LookAt(0, 0, 1, 0, 0, 0, 0, 1, 0);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            GL.Disable(EnableCap.DepthTest);

            GL.UseProgram(_renderProgram.GetProgramId());

            _defaultTextureId = HelperTexture.LoadTexture("default.png");

            Color4 backColor = new Color4();
            backColor.A = 1.0f;
            backColor.R = (62.0f / 255.0f);
            backColor.G = (27.0f / 255.0f);
            backColor.B = (89.0f / 255.0f);
            GL.ClearColor(backColor);

            LoadDefaultWorld();
        }

        private void LoadDefaultWorld()
        {
            _currentWorld = new WorldExample();
            _currentWorld.Prepare();
            _currentWorld.SetDrawPriority(typeof(ActorExample));
            _currentWorld.SortActorList();
        }

        protected override void OnResize(EventArgs e)
        {
            _projectionMatrix = Matrix4.CreateOrthographicOffCenter(0, Width, Height,0, -1, 1);
        }
    }

}
