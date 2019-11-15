
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
    /// <summary>
    /// Window class for this engine
    /// </summary>
    public sealed class MainWindow : GameWindow
    {
        private Matrix4 _viewMatrix = new Matrix4();
        private Matrix4 _projectionMatrix = new Matrix4();
        private Matrix4 _viewProjectionMatrix = new Matrix4();

        private static MainWindow S_WINDOW;
        private int _defaultTextureId = -1;

        private Renderer _renderProgram;
        private World _currentWorld = null;

        /// <summary>
        /// ID of default texture
        /// </summary>
        public int DefaultTexture
        {
            get
            {
                return _defaultTextureId;
            }
        }

        /// <summary>
        /// Reference to the main window
        /// </summary>
        public static MainWindow Window
        {
            get
            {
                return S_WINDOW;
            }
        }

        /// <summary>
        /// Reference to the render program (shader program)
        /// </summary>
        public Renderer RenderProgram
        {
            get
            {
                return _renderProgram;
            }
        }

        /// <summary>
        /// Initializes the main game window
        /// </summary>
        /// <param name="width">window width</param>
        /// <param name="height">window height</param>
        public MainWindow(int width, int height)
            : base(width,
                height,
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

        /// <summary>
        /// Game render logic
        /// </summary>
        /// <param name="e">Stopwatch event</param>
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

        /// <summary>
        /// Game render logic
        /// </summary>
        /// <param name="e">stopwatch event</param>
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

        /// <summary>
        /// Loading of basic assets and setup
        /// </summary>
        /// <param name="e">Event arguments</param>
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

            _defaultTextureId = HelperTexture.LoadTextureFromAssembly("default.png");

            Color4 backColor = new Color4();
            backColor.A = 1.0f;
            backColor.R = (62.0f / 255.0f);
            backColor.G = (27.0f / 255.0f);
            backColor.B = (89.0f / 255.0f);
            GL.ClearColor(backColor);

            LoadDefaultWorld();
        }

        /// <summary>
        /// Loads the example world as first world
        /// </summary>
        private void LoadDefaultWorld()
        {
            _currentWorld = new WorldExample();
            _currentWorld.Prepare();
            _currentWorld.SetDrawPriority(typeof(ActorExample));
            _currentWorld.SortActorList();
        }

        /// <summary>
        /// Resizes the projection matrix when window gets resized
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected override void OnResize(EventArgs e)
        {
            _projectionMatrix = Matrix4.CreateOrthographicOffCenter(0, Width, Height,0, -1, 1);
        }
    }

}
