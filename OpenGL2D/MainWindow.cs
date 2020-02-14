
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
using System.Threading;

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

        private Matrix4 _modelViewProjectionMatrixBloom = new Matrix4();
        private GeoQuad _quadBloom;

        private static MainWindow S_WINDOW;
        private int _defaultTextureId = -1;

        private Renderer _renderProgram;
        private RendererBloom _renderProgramBloom;
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
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _framebufferIdMain);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            _viewProjectionMatrix = _viewMatrix * _projectionMatrix;

            // Render the scene with the standard shader program:
            GL.UseProgram(_renderProgram.GetProgramId());
            if (_currentWorld != null)
            {
                foreach (Actor a in _currentWorld.GetCurrentObjects())
                {
                    a.Draw();
                }
            }

            // Now do post-processing with bloom shader program:
            DownsampleFramebuffer();
            ApplyBloom();

            
            SwapBuffers();
        }

        private void ApplyBloom()
        {
            GL.UseProgram(_renderProgramBloom.GetProgramId());

            int loopCount = 4; // must be %2==0, but 4 will suffice
            int sourceTex; // this is the texture that the bloom will be read from
            for(int i = 0; i < loopCount; i++)
            {
                if(i % 2 == 0)
                {
                    GL.BindFramebuffer(FramebufferTarget.Framebuffer, _framebufferIdBloom1);
                    if (i == 0)
                        sourceTex = _framebufferTextureBloomDownsampled;
                    else
                        sourceTex = _framebufferTextureBloom2;
                }
                else
                {
                    sourceTex = _framebufferTextureBloom1;
                    if (i == loopCount - 1) // last iteration
                        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0); // choose screen as output
                    else
                        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _framebufferIdBloom2);
                }

                _renderProgramBloom.DrawBloom(
                    _quadBloom, 
                    ref _modelViewProjectionMatrixBloom, 
                    i % 2 == 0, 
                    i == loopCount - 1, 
                    Width, 
                    Height,
                    _framebufferTextureMainDownsampled,
                    sourceTex);
            }

            GL.UseProgram(0); // unload bloom shader program
        }

        private void DownsampleFramebuffer()
        {
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, _framebufferIdMain);
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, _framebufferIdMainDownsampled);

            GL.ReadBuffer(ReadBufferMode.ColorAttachment0);
            GL.DrawBuffer(DrawBufferMode.ColorAttachment0);
            GL.BlitFramebuffer(0, 0, Width, Height, 0, 0, Width, Height, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Linear);

            GL.ReadBuffer(ReadBufferMode.ColorAttachment1);
            GL.DrawBuffer(DrawBufferMode.ColorAttachment1);
            GL.BlitFramebuffer(0, 0, Width, Height, 0, 0, Width, Height, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Linear);
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
            _renderProgramBloom = new RendererBloom();

            GeoQuad.InitialiseStatic();
            _quadBloom = new GeoQuad();

            _viewMatrix = Matrix4.LookAt(0, 0, 1, 0, 0, 0, 0, 1, 0);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            GL.Disable(EnableCap.DepthTest);

            GL.UseProgram(_renderProgram.GetProgramId());

            _defaultTextureId = HelperTexture.LoadTextureFromAssembly("default.png");

            Color4 backColor = new Color4();
            backColor.A = 1.0f;
            backColor.R = (0f / 255.0f);
            backColor.G = (0f / 255.0f);
            backColor.B = (0f / 255.0f);
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
            GL.Viewport(ClientRectangle);
            _projectionMatrix = Matrix4.CreateOrthographicOffCenter(0, Width, Height, 0, 0.1f, 100f);

            _modelViewProjectionMatrixBloom = 
                  Matrix4.CreateScale(Width, Height, 1) 
                * Matrix4.LookAt(0, 0, 1, 0, 0, 0, 0, 1, 0)
                * Matrix4.CreateOrthographic(Width, Height, 0.1f, 100f);

            // Initialize frame buffers with 1x Anti-Aliasing
            // (<2 == no FSAA, you have to use 2 or 4 or 8 if you want FSAA)
            InitializeFramebuffers(0);
        }

        #region Framebuffers

        private int _framebufferIdMain = -1;
        private int _framebufferIdMainDownsampled = -1;
        private int _framebufferIdBloom1 = -1;
        private int _framebufferIdBloom2 = -1;

        private int _framebufferTextureMain = -1;
        private int _framebufferTextureMainDownsampled = -1;
        private int _framebufferTextureBloom = -1;
        private int _framebufferTextureBloom1 = -1;
        private int _framebufferTextureBloom2 = -1;
        private int _framebufferTextureBloomDownsampled = -1;

        private void InitializeFramebuffers(int fsaa)
        {
            DeleteFramebuffers();

            // Sometimes, frame buffer initialization fails
            // if the window gets resized too often.
            // I found no better way around this:
            Thread.Sleep(250);

            InitFramebufferMain(fsaa);
            InitFramebufferMainDownsampled();
            InitFramebufferBloom();
        }

        private void DeleteFramebuffers()
        {
            GL.DeleteTextures(6, new int[] { _framebufferTextureMain, _framebufferTextureMainDownsampled, _framebufferTextureBloom, _framebufferTextureBloom1, _framebufferTextureBloom2, _framebufferTextureBloomDownsampled });
            GL.DeleteFramebuffers(4, new int[] { _framebufferIdMain, _framebufferIdMainDownsampled, _framebufferIdBloom1, _framebufferIdBloom2 });
        }

        private void InitFramebufferMain(int fsaa)
        {
            int framebufferId = -1;
            int renderedTexture = -1;
            int renderedTextureAttachment = -1;
            int renderbufferFSAA = -1;
            int renderbufferFSAA2 = -1;

            framebufferId = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebufferId);

            renderedTexture = GL.GenTexture();
            renderedTextureAttachment = GL.GenTexture();


            GL.DrawBuffers(2, new DrawBuffersEnum[2] { DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1 });

            GL.BindTexture(TextureTarget.Texture2D, renderedTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8,
                Width, Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMinFilter.Nearest);

            
            renderbufferFSAA = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, renderbufferFSAA);
            GL.RenderbufferStorageMultisample(RenderbufferTarget.Renderbuffer, fsaa, RenderbufferStorage.Rgba8, Width, Height);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, RenderbufferTarget.Renderbuffer, renderbufferFSAA);

            GL.BindTexture(TextureTarget.Texture2D, renderedTextureAttachment);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8,
                Width, Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMinFilter.Nearest);

            renderbufferFSAA2 = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, renderbufferFSAA2);
            GL.RenderbufferStorageMultisample(RenderbufferTarget.Renderbuffer, fsaa, RenderbufferStorage.Rgba8, Width, Height);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment1, RenderbufferTarget.Renderbuffer, renderbufferFSAA2);

            FramebufferErrorCode code = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (code != FramebufferErrorCode.FramebufferComplete)
            {
                throw new Exception("GL_FRAMEBUFFER_COMPLETE failed. Cannot use FrameBuffer object.");
            }
            else
            {
                _framebufferIdMain = framebufferId;
                _framebufferTextureMain = renderedTexture;
                _framebufferTextureBloom = renderedTextureAttachment;
            }
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        private void InitFramebufferMainDownsampled()
        {
            int framebufferId = -1;
            int renderedTexture = -1;
            int renderedTextureAttachment = -1;

            framebufferId = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebufferId);

            renderedTexture = GL.GenTexture();
            renderedTextureAttachment = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, renderedTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8,
                Width, Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (float)TextureParameterName.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (float)TextureParameterName.ClampToEdge);


            GL.BindTexture(TextureTarget.Texture2D, renderedTextureAttachment);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8,
                Width, Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (float)TextureParameterName.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (float)TextureParameterName.ClampToEdge);

            GL.DrawBuffers(2, new DrawBuffersEnum[2] { DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1 });
            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, renderedTexture, 0);
            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment1, renderedTextureAttachment, 0);

            FramebufferErrorCode code = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (code != FramebufferErrorCode.FramebufferComplete)
            {
                throw new Exception("GL_FRAMEBUFFER_COMPLETE failed. Cannot use FrameBuffer object.");
            }
            else
            {
                _framebufferIdMainDownsampled = framebufferId;
                _framebufferTextureMainDownsampled = renderedTexture;
                _framebufferTextureBloomDownsampled = renderedTextureAttachment;
            }


        }
        private void InitFramebufferBloom()
        {
            int framebufferTempId = -1;
            int renderedTextureTemp = -1;

            // =========== TEMP #1 ===========
            framebufferTempId = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebufferTempId);

            renderedTextureTemp = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, renderedTextureTemp);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                Width, Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (float)TextureParameterName.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (float)TextureParameterName.ClampToEdge);

            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, renderedTextureTemp, 0);
            GL.DrawBuffer(DrawBufferMode.ColorAttachment0);
            FramebufferErrorCode code = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (code != FramebufferErrorCode.FramebufferComplete)
            {
                throw new Exception("GL_FRAMEBUFFER_COMPLETE failed. Cannot use FrameBuffer object.");
            }
            else
            {
                _framebufferIdBloom1 = framebufferTempId;
                _framebufferTextureBloom1 = renderedTextureTemp;
            }

            // =========== TEMP 2 ===========
            int framebufferId = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebufferId);

            int renderedTextureTemp2 = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, renderedTextureTemp2);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                Width, Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (float)TextureParameterName.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (float)TextureParameterName.ClampToEdge);

            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, renderedTextureTemp2, 0);
            GL.DrawBuffer(DrawBufferMode.ColorAttachment0);
            code = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (code != FramebufferErrorCode.FramebufferComplete)
            {
                throw new Exception("GL_FRAMEBUFFER_COMPLETE failed. Cannot use FrameBuffer object.");
            }
            else
            {
                _framebufferIdBloom2 = framebufferId;
                _framebufferTextureBloom2 = renderedTextureTemp2;
            }

            #endregion
        }
    }

}
