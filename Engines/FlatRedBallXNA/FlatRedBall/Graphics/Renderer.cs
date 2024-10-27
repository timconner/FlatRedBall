using System;
using System.Collections.Generic;
using System.Threading;
using FlatRedBall.Graphics.PostProcessing;
using FlatRedBall.Math;
using FlatRedBall.Math.Geometry;
using FlatRedBall.Performance.Measurement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Effect = Microsoft.Xna.Framework.Graphics.Effect;
using ShapeManager = FlatRedBall.Math.Geometry.ShapeManager;

namespace FlatRedBall.Graphics
{
    #region FillVertexLogic class

    class FillVertexLogic
    {
        public IList<Sprite> SpriteList;
        public List<VertexPositionColorTexture[]> VertexLists;
        public int StartIndex;
        public int Count;
        public int FirstSpriteInAllSimultaneousLogics;

        ManualResetEvent _manualResetEvent;

        public FillVertexLogic()
        {
            _manualResetEvent = new ManualResetEvent(false);
        }

        public void Reset()
        {
            _manualResetEvent.Reset();
        }

        public void Wait()
        {
            _manualResetEvent.WaitOne();
        }

        public void FillVertexList()
        {
            Reset();
            ThreadPool.QueueUserWorkItem(FillVertexListSync);
        }

        internal void FillVertexListSync(object notUsed)
        {
            int vertNum = 0;
            int vertexBufferNum = 0;
            int lastIndexExclusive = StartIndex + Count;
            var arrayAtIndex = VertexLists[vertexBufferNum];

            for (int unadjustedI = StartIndex; unadjustedI < lastIndexExclusive; unadjustedI++)
            {
                int i = unadjustedI - FirstSpriteInAllSimultaneousLogics;

                vertNum = (i * 6) % 6000;
                vertexBufferNum = i / 1000;
                arrayAtIndex = VertexLists[vertexBufferNum];

                var spriteAtIndex = SpriteList[unadjustedI];

                #region The Sprite doesn't have stored vertices (default) so we have to create them now

                if (spriteAtIndex.mAutomaticallyUpdated)
                {
                    spriteAtIndex.UpdateVertices();

                    #region Set the color
#if IOS
                    // If the Sprite's Texture is null, it will behave as if it's got its ColorOperation set to Color instead of Texture
                    if (spriteAtIndex.ColorOperation == ColorOperation.Texture && spriteAtIndex.Texture != null)
                    {
                        // If we are using the texture color, we want to ignore the Sprite's RGB values.
                        // The W component is Alpha, so we'll use full values for the others.
                        var value = (uint)(255 * spriteAtIndex.mVertices[3].Color.W);
                        arrayAtIndex[vertNum + 0].Color.PackedValue =
                            value +
                            (value << 8) +
                            (value << 16) +
                            (value << 24);
                    }
                    else
                    {
                        // If we are using the texture color, we 
                        arrayAtIndex[vertNum + 0].Color.PackedValue =
                            ((uint)(255 * spriteAtIndex.mVertices[3].Color.X)) +
                            (((uint)(255 * spriteAtIndex.mVertices[3].Color.Y)) << 8) +
                            (((uint)(255 * spriteAtIndex.mVertices[3].Color.Z)) << 16) +
                            (((uint)(255 * spriteAtIndex.mVertices[3].Color.W)) << 24);
                    }

                    arrayAtIndex[vertNum + 1].Color.PackedValue =
                        arrayAtIndex[vertNum + 0].Color.PackedValue;

                    arrayAtIndex[vertNum + 2].Color.PackedValue =
                        arrayAtIndex[vertNum + 0].Color.PackedValue;

                    arrayAtIndex[vertNum + 5].Color.PackedValue =
                        arrayAtIndex[vertNum + 0].Color.PackedValue;
#else
                    arrayAtIndex[vertNum + 0].Color.PackedValue =
                        ((uint)(255 * spriteAtIndex.mVertices[3].Color.X)) +
                        (((uint)(255 * spriteAtIndex.mVertices[3].Color.Y)) << 8) +
                        (((uint)(255 * spriteAtIndex.mVertices[3].Color.Z)) << 16) +
                        (((uint)(255 * spriteAtIndex.mVertices[3].Color.W)) << 24);

                    arrayAtIndex[vertNum + 1].Color.PackedValue =
                        ((uint)(255 * spriteAtIndex.mVertices[0].Color.X)) +
                        (((uint)(255 * spriteAtIndex.mVertices[0].Color.Y)) << 8) +
                        (((uint)(255 * spriteAtIndex.mVertices[0].Color.Z)) << 16) +
                        (((uint)(255 * spriteAtIndex.mVertices[0].Color.W)) << 24);

                    arrayAtIndex[vertNum + 2].Color.PackedValue =
                        ((uint)(255 * spriteAtIndex.mVertices[1].Color.X)) +
                        (((uint)(255 * spriteAtIndex.mVertices[1].Color.Y)) << 8) +
                        (((uint)(255 * spriteAtIndex.mVertices[1].Color.Z)) << 16) +
                        (((uint)(255 * spriteAtIndex.mVertices[1].Color.W)) << 24);

                    arrayAtIndex[vertNum + 5].Color.PackedValue =
                        ((uint)(255 * spriteAtIndex.mVertices[2].Color.X)) +
                        (((uint)(255 * spriteAtIndex.mVertices[2].Color.Y)) << 8) +
                        (((uint)(255 * spriteAtIndex.mVertices[2].Color.Z)) << 16) +
                        (((uint)(255 * spriteAtIndex.mVertices[2].Color.W)) << 24);
#endif
                    #endregion

                    arrayAtIndex[vertNum + 0].Position = spriteAtIndex.mVertices[3].Position;
                    arrayAtIndex[vertNum + 0].TextureCoordinate = spriteAtIndex.mVertices[3].TextureCoordinate;

                    arrayAtIndex[vertNum + 1].Position = spriteAtIndex.mVertices[0].Position;
                    arrayAtIndex[vertNum + 1].TextureCoordinate = spriteAtIndex.mVertices[0].TextureCoordinate;

                    arrayAtIndex[vertNum + 2].Position = spriteAtIndex.mVertices[1].Position;
                    arrayAtIndex[vertNum + 2].TextureCoordinate = spriteAtIndex.mVertices[1].TextureCoordinate;

                    arrayAtIndex[vertNum + 3] = arrayAtIndex[vertNum + 0];
                    arrayAtIndex[vertNum + 4] = arrayAtIndex[vertNum + 2];

                    arrayAtIndex[vertNum + 5].Position = spriteAtIndex.mVertices[2].Position;
                    arrayAtIndex[vertNum + 5].TextureCoordinate = spriteAtIndex.mVertices[2].TextureCoordinate;

                    if (spriteAtIndex.FlipHorizontal)
                    {
                        arrayAtIndex[vertNum + 0].TextureCoordinate = arrayAtIndex[vertNum + 5].TextureCoordinate;
                        arrayAtIndex[vertNum + 5].TextureCoordinate = arrayAtIndex[vertNum + 3].TextureCoordinate;
                        arrayAtIndex[vertNum + 3].TextureCoordinate = arrayAtIndex[vertNum + 0].TextureCoordinate;

                        arrayAtIndex[vertNum + 2].TextureCoordinate = arrayAtIndex[vertNum + 1].TextureCoordinate;
                        arrayAtIndex[vertNum + 1].TextureCoordinate = arrayAtIndex[vertNum + 4].TextureCoordinate;
                        arrayAtIndex[vertNum + 4].TextureCoordinate = arrayAtIndex[vertNum + 2].TextureCoordinate;
                    }

                    if (spriteAtIndex.FlipVertical)
                    {
                        arrayAtIndex[vertNum + 0].TextureCoordinate = arrayAtIndex[vertNum + 1].TextureCoordinate;
                        arrayAtIndex[vertNum + 1].TextureCoordinate = arrayAtIndex[vertNum + 3].TextureCoordinate;
                        arrayAtIndex[vertNum + 3].TextureCoordinate = arrayAtIndex[vertNum + 0].TextureCoordinate;

                        arrayAtIndex[vertNum + 2].TextureCoordinate = arrayAtIndex[vertNum + 5].TextureCoordinate;
                        arrayAtIndex[vertNum + 5].TextureCoordinate = arrayAtIndex[vertNum + 4].TextureCoordinate;
                        arrayAtIndex[vertNum + 4].TextureCoordinate = arrayAtIndex[vertNum + 2].TextureCoordinate;
                    }
                }

                #endregion

                else
                {
                    arrayAtIndex[vertNum + 0] = spriteAtIndex.mVerticesForDrawing[3];
                    arrayAtIndex[vertNum + 1] = spriteAtIndex.mVerticesForDrawing[0];
                    arrayAtIndex[vertNum + 2] = spriteAtIndex.mVerticesForDrawing[1];
                    arrayAtIndex[vertNum + 3] = spriteAtIndex.mVerticesForDrawing[3];
                    arrayAtIndex[vertNum + 4] = spriteAtIndex.mVerticesForDrawing[1];
                    arrayAtIndex[vertNum + 5] = spriteAtIndex.mVerticesForDrawing[2];
                }
            }

            _manualResetEvent.Set();
        }
    }

    #endregion

    /// <summary>
    /// Static class responsible for drawing/rendering content to the cameras on screen.
    /// </summary> 
    /// <remarks>This class is called by <see cref="FlatRedBallServices.Draw()"/></remarks>
    public static partial class Renderer
    {
        #region Fields

        static IGraphicsDeviceService mGraphics;
        public static SpriteBatch mSpriteBatch;
        static List<FillVertexLogic> mFillVertexLogics = new List<FillVertexLogic>();

        #region Render targets and textures

        public static Dictionary<int, SurfaceFormat> RenderModeFormats;

        static RenderMode mCurrentRenderMode = RenderMode.Default;

        #endregion

        #region Vertex fields

        // Vertex buffers
        static List<DynamicVertexBuffer> mVertexBufferList;
        static List<DynamicVertexBuffer> mShapesVertexBufferList;

        static List<VertexPositionColorTexture[]> mSpriteVertices = new List<VertexPositionColorTexture[]>();
        static List<VertexPositionColorTexture[]> mZBufferedSpriteVertices = new List<VertexPositionColorTexture[]>();
        static List<VertexPositionColorTexture[]> mTextVertices = new List<VertexPositionColorTexture[]>();
        static List<VertexPositionColor[]> mShapeVertices = new List<VertexPositionColor[]>();

        // Vertex declarations
        static VertexDeclaration mPositionColorTexture;
        static VertexDeclaration mPositionColor;

        // Vertex arrays
        static VertexPositionColorTexture[] mVertexArray;
        static VertexPositionColor[] mShapeDrawingVertexArray;

        // Render breaks
        static List<RenderBreak> mRenderBreaks = new List<RenderBreak>();
        static List<RenderBreak> mSpriteRenderBreaks = new List<RenderBreak>();
        static List<RenderBreak> mZBufferedSpriteRenderBreaks = new List<RenderBreak>();
        static List<RenderBreak> mTextRenderBreaks = new List<RenderBreak>();

        // Current vertex buffer
        static VertexBuffer mVertexBuffer;
        static IndexBuffer mIndexBuffer;

        // Quad fields
        static VertexPositionTexture[] mQuadVertices;
        static short[] mQuadIndices;
        static VertexDeclaration mQuadVertexDeclaration;

        #endregion

        #region Effects

        static BasicEffect mBasicEffect;
        static BasicEffect mWireframeEffect;
        static Effect mEffect;
        static Effect mExternalEffect;
        static Effect mCurrentEffect; // This stores the current effect in use

        #endregion

        #region Texture fields

        static Texture2D mTexture;
        static BlendOperation mBlendOperation;
        static TextureAddressMode mTextureAddressMode;
        static internal ColorOperation mLastColorOperationSet = ColorOperation.Texture;

        #endregion

        #region Debugging information

        internal static int NumberOfSpritesDrawn;
        static int mFillVBListCallsThisFrame;
        static int mRenderBreaksAllocatedThisFrame;

        #endregion

        #endregion

        #region Properties

        #region Public properties

        public static Texture2D Texture
        {
            set
            {
                if (value != mTexture)
                {
                    ForceSetTexture(value);
                }
            }
        }

        static void ForceSetTexture(Texture2D value)
        {
            mTexture = value;
            mEffectManager.ParameterCurrentTexture.SetValue(mTexture);
        }

        public static Texture2D TextureOnDevice
        {
            set
            {
                if (value != mTexture)
                {
                    mTexture = value;
                    GraphicsDevice.Textures[0] = mTexture;
                }
            }
        }

        public static VertexBuffer VertexBuffer
        {
            set
            {
                if (value != mVertexBuffer && value != null)
                {
                    mVertexBuffer = value;
                    GraphicsDevice.SetVertexBuffer(mVertexBuffer);
                }
                else
                {
                    mVertexBuffer = null;
                }
            }
        }

        public static IndexBuffer IndexBuffer
        {
            set
            {
                if (value != mIndexBuffer && value != null)
                {
                    mIndexBuffer = value;
                    GraphicsDevice.Indices = mIndexBuffer;
                }
                else
                {
                    mIndexBuffer = null;
                }
            }
        }

        /// <summary>
        /// Returns the layer currently being rendered. Can be used in IDrawableBatches and debug code.
        /// </summary>
        public static Layer CurrentLayer { get; private set; }

        internal static string CurrentLayerName
        {
            get
            {
                if (CurrentLayer != null)
                {
                    return CurrentLayer.Name;
                }
                else
                {
                    return "Unlayered";
                }
            }
        }

        public static VertexDeclaration PositionColorVertexDeclaration { get { return mPositionColor; } }
        public static VertexDeclaration PositionColorTextureVertexDeclaration { get { return mPositionColorTexture; } }

        public static bool IsInRendering { get; set; }

        public static RenderMode CurrentRenderMode { get { return mCurrentRenderMode; } }

        public static SwapChain SwapChain { get; set; }

        [Obsolete("Use LastFrameRenderBreakList instead")]
        public static int RenderBreaksAllocatedThisFrame
        {
            get
            {
                if (RecordRenderBreaks == false)
                {
                    throw new InvalidOperationException($"You must set {nameof(RecordRenderBreaks)} to true before getting RenderBreaksAllocatdThisFrame");
                }

                return LastFrameRenderBreakList?.Count ?? 0;
            }
        }

        /// <summary>
        /// Tells the renderer to record and keep track of render breaks so they
        /// can be used when optimizing rendering. This value defaults to false
        /// </summary>
        public static bool RecordRenderBreaks
        {
            get { return mRecordRenderBreaks; }
            set
            {
                mRecordRenderBreaks = value;

                if (mRecordRenderBreaks && LastFrameRenderBreakList == null)
                {
                    LastFrameRenderBreakList = new List<RenderBreak>();
                }

                if (!mRecordRenderBreaks && LastFrameRenderBreakList != null)
                {
                    LastFrameRenderBreakList.Clear();
                }
            }
        }
        static bool mRecordRenderBreaks;

        /// <summary>
        /// Contains the list of render breaks from the previous frame.
        /// This is updated every time FlatRedBall is drawn.
        /// </summary>
        public static List<RenderBreak> LastFrameRenderBreakList
        {
            get
            {
#if DEBUG
                if (RecordRenderBreaks == false)
                {
                    throw new InvalidOperationException($"You must set {nameof(RecordRenderBreaks)} to true before getting LastFrameRenderBreakList");

                }
#endif
                return lastFrameRenderBreakList;
            }
            private set { lastFrameRenderBreakList = value; }
        }
        static List<RenderBreak> lastFrameRenderBreakList;

        /// <summary>
        /// When this is enabled texture colors will be translated to linear space before 
        /// any other shader operations are performed. This is useful for games with 
        /// lighting and other special shader effects. If the colors are left in gamma 
        /// space the shader calculations will crush the colors and not look like natural 
        /// lighting. Delinearization must be done by the developer in the last render 
        /// step when rendering to the screen. This technique is called gamma correction.
        /// Disabled by default.
        /// </summary>
        public static bool LinearizeTextures { get; set; }

        public static List<IPostProcess> GlobalPostProcesses { get; private set; } = new List<IPostProcess>();

        #endregion

        #region Internal properties

        /// <summary>
        /// Sets the color operation on the graphics device if the set value differs from the current value.
        /// This is public so that IDrawableBatches can set the color operation.
        /// </summary>
        public static ColorOperation ColorOperation
        {
            get
            {
                return mLastColorOperationSet;
            }
            set
            {
                if (mLastColorOperationSet != value)
                {
                    ForceSetColorOperation(value);
                }
            }
        }

        /// <summary>
        /// Sets the blend operation on the graphics device if the set value differs from the current value.
        /// If the two values are the same, then the property doesn't do anything.
        /// </summary>
        public static BlendOperation BlendOperation
        {
            get { return mBlendOperation; }
            set
            {
                if (value != mBlendOperation)
                {
                    mBlendOperation = value;
                    ForceSetBlendOperation();
                }
            }
        }

        public static TextureAddressMode TextureAddressMode
        {
            get { return mTextureAddressMode; }
            set
            {
                if (value != mTextureAddressMode)
                {
                    ForceSetTextureAddressMode(value);
                }
            }
        }

        public static void ForceSetTextureAddressMode(TextureAddressMode value)
        {
            mTextureAddressMode = value;
            FlatRedBallServices.GraphicsOptions.ForceRefreshSamplerState(0);
            FlatRedBallServices.GraphicsOptions.ForceRefreshSamplerState(1);
        }

        static internal IGraphicsDeviceService Graphics { get { return mGraphics; } }
        static internal GraphicsDevice GraphicsDevice { get { return mGraphics.GraphicsDevice; } }

        static CustomEffectManager mEffectManager = new CustomEffectManager();
        public static CustomEffectManager ExternalEffectManager { get; } = new CustomEffectManager();

        public static Effect Effect
        {
            get { return mEffect; }
            set
            {
                mEffect = value;
                mEffectManager.Effect = mEffect;
            }
        }

        public static Effect ExternalEffect
        {
            get { return mExternalEffect; }
            set
            {
                mExternalEffect = value;
                ExternalEffectManager.Effect = mExternalEffect;
            }
        }

        #endregion

        #endregion

        #region Methods

        #region Constructor and initialization

        static Renderer()
        {
            // Vertex buffers
            mVertexBufferList = new List<DynamicVertexBuffer>();
            mShapesVertexBufferList = new List<DynamicVertexBuffer>();

            // Vertex arrays
            mVertexArray = new VertexPositionColorTexture[6000];
            mShapeDrawingVertexArray = new VertexPositionColor[6000];

            SetNumberOfThreadsToUse(1);
        }

        internal static void Initialize(IGraphicsDeviceService graphics)
        {
            // Make sure the device isn't null
            if (graphics.GraphicsDevice == null)
            {
                throw new NullReferenceException("The GraphicsDevice is null.  Are you calling FlatRedBallServices.InitializeFlatRedBall from the Game's constructor?  If so, you need to call it in the Initialize or LoadGraphicsContent method.");
            }

            mGraphics = graphics;

            InitializeEffect();

            ForceSetBlendOperation();
        }

        private static void InitializeEffect()
        {
            mPositionColorTexture = VertexPositionColorTexture.VertexDeclaration;
            mPositionColor = VertexPositionColor.VertexDeclaration;

            // Create render mode formats dictionary
            RenderModeFormats = new Dictionary<int, SurfaceFormat>(10);
            RenderModeFormats.Add((int)RenderMode.Color, SurfaceFormat.Color);
            RenderModeFormats.Add((int)RenderMode.Default, SurfaceFormat.Color);

            // Set the initial viewport
            var viewport = mGraphics.GraphicsDevice.Viewport;
            viewport.Width = FlatRedBallServices.ClientWidth;
            viewport.Height = FlatRedBallServices.ClientHeight;
            mGraphics.GraphicsDevice.Viewport = viewport;

            // Sprite batch
            mSpriteBatch = new SpriteBatch(FlatRedBallServices.GraphicsDevice);

            // Basic effect
            mBasicEffect = new BasicEffect(mGraphics.GraphicsDevice);
            mBasicEffect.Alpha = 1.0f;
            mBasicEffect.AmbientLightColor = new Vector3(1f, 1f, 1f);
            mBasicEffect.World = Matrix.Identity;

            mWireframeEffect = new BasicEffect(FlatRedBallServices.GraphicsDevice);

            BlendOperation = BlendOperation.Regular;

            var depthStencilState = new DepthStencilState();
            depthStencilState.DepthBufferEnable = false;
            depthStencilState.DepthBufferWriteEnable = false;

            mGraphics.GraphicsDevice.DepthStencilState = depthStencilState;

            var rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;
        }

        #endregion

        #region Public methods

        #region Main drawing methods

        static void PrepareForDrawScene(Camera camera, RenderMode renderMode)
        {
            mCurrentRenderMode = renderMode;

            // Set the viewport for the current camera
            var viewport = camera.GetViewport();

            mGraphics.GraphicsDevice.Viewport = viewport;

            #region Clear the viewport

            if (renderMode == RenderMode.Default || renderMode == RenderMode.Color)
            {
                // Vic says:  This code used to be:
                // if (!mUseRenderTargets && camera.BackgroundColor.A == 0)
                // Why prevent color clearing only when we aren't using render targets?  Don't know, 
                // so I changed this in June 

                // UPDATE:
                // It seems that removing the !mUseRenderTargets just makes the background purple...no change
                // happens in tems of things being able to be drawn.  Not sure why, but I'll update the docs to
                // indicate that you can't use RenderTargets and have stuff drawn before FRB

                if (camera.BackgroundColor.A == 0)
                {
                    if (camera.ClearsDepthBuffer)
                    {
                        // Clearing to a transparent color, so just clear depth
                        mGraphics.GraphicsDevice.Clear(ClearOptions.DepthBuffer, camera.BackgroundColor, 1, 0);
                    }
                }
                else
                {
                    if (camera.ClearsDepthBuffer)
                    {
                        mGraphics.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, camera.BackgroundColor, 1, 0);
                    }
                    else
                    {
                        mGraphics.GraphicsDevice.Clear(ClearOptions.Target, camera.BackgroundColor, 1, 0);
                    }
                }
            }
            else if (renderMode == RenderMode.Depth)
            {
                if (camera.ClearsDepthBuffer)
                {
                    mGraphics.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.White, 1, 0);
                }
            }
            else
            {
                if (camera.ClearsDepthBuffer)
                {
                    Color colorToClearTo = Color.Transparent;

                    mGraphics.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, colorToClearTo, 1, 0);
                }
            }

            #endregion

            #region Set device settings for rendering

            // Let's force texture address mode just in case someone screwed
            // with it outside of the rendering, like when using render states.
            ForceSetTextureAddressMode(TextureAddressMode.Clamp);

            #endregion

            #region Set camera values on the current effect

            mCurrentEffect = mEffect;
            camera.SetDeviceViewAndProjection(mCurrentEffect, false);

            #endregion
        }

        #endregion

        public static new String ToString()
        {
            return String.Format(
                "Number of RenderBreaks allocated: %d\nNumber of Sprites drawn: %d",
                mRenderBreaksAllocatedThisFrame, NumberOfSpritesDrawn);
        }

        #endregion

        #region Internal methods

        public static void Update()
        {
        }

        internal static void UpdateDependencies()
        {
        }

        public static void Draw()
        {
            Draw(null);
        }

        // This is public for those who want to have more control over how FRB renders.
        public static void Draw(Section section)
        {
            bool hasGlobalPostProcessing = false;

            foreach (var item in GlobalPostProcesses)
            {
                if (item.IsEnabled)
                {
                    hasGlobalPostProcessing = true;
                    break;
                }
            }
#if DEBUG
            if (hasGlobalPostProcessing && SwapChain == null)
            {
                throw new InvalidOperationException("SwapChain must be set prior to rendering the first frame if using any post processing");
            }
#endif
            if (hasGlobalPostProcessing)
            {
                SetRenderTargetForPostProcessing();
            }
            else
            {
                // Just in case we removed all post processing, but are on "B"
                SwapChain?.ResetForFrame();
            }

            DrawInternal(section);

            if (hasGlobalPostProcessing)
            {
                ApplyPostProcessing();
            }
        }

        static void SetRenderTargetForPostProcessing()
        {
            // Post processing 
            ForceSetBlendOperation();
            ForceSetColorOperation(Renderer.ColorOperation);

            SwapChain.ResetForFrame();

            // Set the render target before drawing anything
            GraphicsDevice.SetRenderTarget(Renderer.SwapChain.CurrentRenderTarget);
        }

        static void ApplyPostProcessing()
        {
            foreach (var postProcess in Renderer.GlobalPostProcesses)
            {
                if (postProcess.IsEnabled)
                {
#if DEBUG
                    mRenderBreaks.Add(new RenderBreak() { ObjectCausingBreak = postProcess });
#endif
                    SwapChain.Swap();
                    postProcess.Apply(Renderer.SwapChain.CurrentTexture);
                }
            }

#if DEBUG
            mRenderBreaks.Add(new RenderBreak() { ObjectCausingBreak = SwapChain });
#endif
            SwapChain.RenderToScreen();
        }

        static void DrawInternal(Section section)
        {
            if (section != null)
            {
                Section.GetAndStartContextAndTime("Start of Renderer.Draw");
            }

            IsInRendering = true;

            // Drawing should only occur if the window actually has pixels.
            // Using ClientBounds causes memory to be allocated. We can just
            // use the FlatRedBallService's value which gets updated whenever
            // the resolution changes.
            if (FlatRedBallServices.mClientWidth == 0 ||
                FlatRedBallServices.mClientHeight == 0)
            {
                IsInRendering = false;
                return;
            }

            #region Reset the debugging and profiling information

            mFillVBListCallsThisFrame = 0;
            mRenderBreaksAllocatedThisFrame = 0;
            if (lastFrameRenderBreakList != null)
            {
                lastFrameRenderBreakList.Clear();
            }

            NumberOfSpritesDrawn = 0;

            #endregion

#if DEBUG
            if (SpriteManager.Cameras.Count <= 0)
            {
                NullReferenceException exception = new NullReferenceException(
                    "There are no cameras to render, did you forget to add a camera to the SpriteManager?");
                throw exception;
            }
            if (mGraphics == null || mGraphics.GraphicsDevice == null)
            {
                NullReferenceException exception = new NullReferenceException(
                    "Renderer's GraphicsDeviceManager is null.  Did you forget to call FlatRedBallServices.Initialize?");
                throw exception;
            }
#endif

            if (section != null)
            {
                Section.EndContextAndTime();
                Section.GetAndStartContextAndTime("Render Cameras");
            }

            #region Loop through all cameras (viewports)

            // Note: It may be more efficient to do this loop at each point
            // there is a camera reference to avoid passing geometry multiple times.
            // Addition: The noted idea may not be faster due to render target swapping.

            for (int i = 0; i < SpriteManager.Cameras.Count; i++)
            {
                var camera = SpriteManager.Cameras[i];

                lock (Graphics.GraphicsDevice)
                {
                    #region If the Camera either DrawsWorld or DrawsCameraLayer, then perform drawing

                    if (camera.DrawsWorld || camera.DrawsCameraLayer)
                    {
                        if (section != null)
                        {
                            string cameraName = camera.Name;
                            if (string.IsNullOrEmpty(cameraName))
                            {
                                cameraName = "at index " + i;
                            }
                            Section.GetAndStartContextAndTime("Render camera " + cameraName);
                        }

                        DrawCamera(camera, RenderMode.Default, section);

                        if (section != null)
                        {
                            Section.EndContextAndTime();
                        }
                    }

                    #endregion
                }
            }

            #endregion

            if (section != null)
            {
                Section.EndContextAndTime();
                Section.GetAndStartContextAndTime("End of Render");
            }

            IsInRendering = false;

            Screens.ScreenManager.Draw();

            if (section != null)
            {
                Section.EndContextAndTime();
            }
        }

        public static void ForceSetColorOperation(ColorOperation value)
        {
            mLastColorOperationSet = value;

            var technique = mEffectManager.GetVertexColorTechniqueFromColorOperation(value);

            if (technique == null)
            {
                string errorString =
                    "Could not find a technique for " + value.ToString() +
                    ", filter: " + FlatRedBallServices.GraphicsOptions.TextureFilter +
                    " in the current shader. If using a custom shader verify that" +
                    " this pixel shader technique exists.";
                throw new Exception(errorString);
            }
            else
            {
                if (mCurrentEffect == null)
                {
                    mCurrentEffect = mEffect;
                }

                mCurrentEffect.CurrentTechnique = technique;
            }
        }

        public static BlendState AddBlendState = new BlendState()
        {
            AlphaSourceBlend = Blend.SourceAlpha,
            AlphaDestinationBlend = Blend.One,
            AlphaBlendFunction = BlendFunction.Max,
        };

        public static BlendState RegularBlendState = new BlendState()
        {
            AlphaSourceBlend = Blend.SourceAlpha,
            AlphaDestinationBlend = Blend.InverseSourceAlpha,
            AlphaBlendFunction = BlendFunction.ReverseSubtract,
        };

        public static void ForceSetBlendOperation()
        {
            switch (mBlendOperation)
            {
                case BlendOperation.Add:
                    mGraphics.GraphicsDevice.BlendState = BlendState.Additive;
                    break;
                case BlendOperation.Regular:
                    mGraphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
                    break;
                case BlendOperation.NonPremultipliedAlpha:
                    mGraphics.GraphicsDevice.BlendState = BlendState.NonPremultiplied;
                    break;
                case BlendOperation.Modulate:
                    {
                        BlendState blendState = new BlendState();
                        blendState.AlphaSourceBlend = Blend.DestinationColor;
                        blendState.ColorSourceBlend = Blend.DestinationColor;

                        blendState.AlphaDestinationBlend = Blend.Zero;
                        blendState.ColorDestinationBlend = Blend.InverseSourceAlpha;
                        blendState.ColorBlendFunction = BlendFunction.Add;

                        mGraphics.GraphicsDevice.BlendState = blendState;

                    }
                    break;
                case BlendOperation.SubtractAlpha:
                    {
                        // Vic says 12/19/2020
                        // This took me a while to figure out, so I'll document what I learned.
                        // For alpha, the operation is:
                        // ResultAlpha = (SourceAlpha * Blend.AlphaSourceBlend) {BlendFunc} (DestinationAlpha * Blend.AlphaDestblend)
                        // where:
                        // ResultAlpha is the resulting pixel alpha after the operation occurs
                        // SourceAlpha is the alpha of the pixel on the sprite that is being drawn
                        // DestinationAlpha is the alpha of the pixel on the surface before the pixel is drawn, which is the result alpha from a previous operation
                        // In this case we want to subtract the sprite being drawn.
                        // To subtract the sprite that is being drawn, which is the SourceSprite, we need to do a ReverseSubtract
                        // so that the Source is being subtracted.
                        // We want to use Blend.One on both so that the values being used are the pixel values on source and dest.
                        // Keep in mind that since we're making a texture, we need this texture to be premultiplied, so we
                        // need to multiply the destination color by the inverse source alpha, so that if alpha is 0, we preserve the color, otherwise we
                        // darken it to premult

                        var blendState = new BlendState();

                        blendState.ColorSourceBlend = Blend.Zero;
                        blendState.ColorBlendFunction = BlendFunction.Add;
                        blendState.ColorDestinationBlend = Blend.InverseSourceAlpha;

                        blendState.AlphaSourceBlend = Blend.One;
                        blendState.AlphaBlendFunction = BlendFunction.ReverseSubtract;
                        blendState.AlphaDestinationBlend = Blend.One;

                        mGraphics.GraphicsDevice.BlendState = blendState;
                    }
                    break;

                case BlendOperation.Modulate2X:
                    {
                        var blendState = new BlendState();

                        blendState.AlphaSourceBlend = Blend.DestinationColor;
                        blendState.ColorSourceBlend = Blend.DestinationColor;

                        blendState.AlphaDestinationBlend = Blend.SourceColor;
                        blendState.ColorDestinationBlend = Blend.SourceColor;

                        mGraphics.GraphicsDevice.BlendState = blendState;
                    }
                    break;

                default:
                    throw new NotImplementedException("Blend operation not implemented: " + mBlendOperation);
            }
        }

        #endregion

        #region Private methods

        #region Drawing methods

        /// <summary>
        /// Draws a quad. The effect must already be started.
        /// </summary>
        public static void DrawQuad(Vector3 bottomLeft, Vector3 topRight)
        {
            mQuadVertices = new VertexPositionTexture[] {
                new VertexPositionTexture(new Vector3(1, -1, 1), new Vector2(1, 1)),
                new VertexPositionTexture(new Vector3(-1, -1, 1), new Vector2(0, 1)),
                new VertexPositionTexture(new Vector3(-1, 1, 1), new Vector2(0, 0)),
                new VertexPositionTexture(new Vector3(1, 1, 1), new Vector2(1, 0)) };

            mQuadIndices = new short[] { 0, 1, 2, 2, 3, 0 };

            mQuadVertexDeclaration = VertexPositionTexture.VertexDeclaration;

            mQuadVertices[0].Position = new Vector3(topRight.X, bottomLeft.Y, 1);
            mQuadVertices[1].Position = new Vector3(bottomLeft.X, bottomLeft.Y, 1);
            mQuadVertices[2].Position = new Vector3(bottomLeft.X, topRight.Y, 1);
            mQuadVertices[3].Position = new Vector3(topRight.X, topRight.Y, 1);

            throw new NotImplementedException();
        }

        /// <summary>
        /// Draws a full-screen quad. The effect must already be started.
        /// </summary>
        public static void DrawFullScreenQuad()
        {
            DrawQuad(Vector3.One * -1f, Vector3.One);
        }

        internal static void DrawZBufferedSprites(Camera camera, SpriteList listToRender)
        {
            // A note about how ZBuffered rendering works with alpha.
            // FRB shaders use a clip() function, which prevents a pixel
            // from being processed. In this case the FRB shader clips
            // based off of the Alpha in the Sprite. If the alpha is
            // essentially 0, then the pixel is not rendered and it
            // does not modify the depth buffer.

            if (SpriteManager.ZBufferedSortType == SortType.Texture)
            {
                listToRender.SortTextureInsertion();
            }

            // Set device settings for drawing ZBuffered sprites
            mVisibleSprites.Clear();

            GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            if (camera.ClearsDepthBuffer)
            {
                GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            }

            // Currently ZBuffered Sprites are all drawn. Performance improvement possible here by culling.

            lock (listToRender)
            {
                for (int i = 0; i < listToRender.Count; i++)
                {
                    var s = listToRender[i];

                    if (s.AbsoluteVisible && s.Alpha > .0001f)
                    {
                        mVisibleSprites.Add(s);
                    }
                }
            }

            // Draw
            PrepareSprites(
                mZBufferedSpriteVertices, mZBufferedSpriteRenderBreaks,
                mVisibleSprites, 0, mVisibleSprites.Count);

            DrawSprites(
                mZBufferedSpriteVertices, mZBufferedSpriteRenderBreaks,
                mVisibleSprites, 0,
                mVisibleSprites.Count, camera);
        }

        private static void ClearBackgroundForLayer(Camera camera)
        {
            if (camera.ClearsDepthBuffer)
            {
                Color clearColor = Color.Transparent;
                mGraphics.GraphicsDevice.Clear(ClearOptions.DepthBuffer, clearColor, 1, 0);
            }
        }

        static List<int> vertsPerVertexBuffer = new List<int>(4);

        static void DrawShapes(Camera camera,
            PositionedObjectList<Sphere> spheres,
            PositionedObjectList<AxisAlignedCube> cubes,
            PositionedObjectList<AxisAlignedRectangle> rectangles,
            PositionedObjectList<Circle> circles,
            PositionedObjectList<Polygon> polygons,
            PositionedObjectList<Line> lines,
            PositionedObjectList<Capsule2D> capsule2Ds,
            Layer layer)
        {
            vertsPerVertexBuffer.Clear();

            var oldFilter = FlatRedBallServices.GraphicsOptions.TextureFilter;

            // August 28, 2023 - Why do we use linear filtering for polygons?
            // This won't make the lines anti-aliased. It just blends the
            // textels. This doesn't seem necessary, and is just a waste of performance.
            //FlatRedBallServices.GraphicsOptions.TextureFilter = TextureFilter.Linear;

            if (layer == null)
            {
                // Reset the camera as it may have been set differently by layers
                camera.SetDeviceViewAndProjection(mBasicEffect, false);
                camera.SetDeviceViewAndProjection(mEffect, false);
            }
            else
            {
                camera.SetDeviceViewAndProjection(mBasicEffect, layer.RelativeToCamera);
                camera.SetDeviceViewAndProjection(mEffect, layer.RelativeToCamera);
            }

            ColorOperation = ColorOperation.Color;
            ForceSetColorOperation(ColorOperation.Color);

            #region Count the number of Vertices needed to draw the various shapes

            const int numberOfSphereSlices = 4;
            const int numberOfSphereVertsPerSlice = 17;
            int verticesToDraw = 0;

            // Rectangles require 5 points since the last is repeated
            for (int i = 0; i < rectangles.Count; i++)
            {
                if (rectangles[i].AbsoluteVisible)
                {
                    verticesToDraw += 5;
                }
            }

            // Add all the vertices for circles
            for (int i = 0; i < circles.Count; i++)
            {
                if (circles[i].AbsoluteVisible)
                {
                    verticesToDraw += ShapeManager.NumberOfVerticesForCircles;
                }
            }

            // Add all the vertices for the polygons
            verticesToDraw += ShapeManager.GetTotalPolygonVertexCount(polygons);

            // Add all the vertices for the lines
            verticesToDraw += lines.Count * 2;

            // Add all the vertices for the Capsule2D's
            verticesToDraw += capsule2Ds.Count * ShapeManager.NumberOfVerticesForCapsule2Ds;

            // Add the vertices for AxisAlignedCubes
            verticesToDraw += cubes.Count * 16; // 16 points needed to draw a cube

            verticesToDraw += spheres.Count * numberOfSphereSlices * numberOfSphereVertsPerSlice;

            #endregion

            // If nothing is being drawn, exit the function now
            if (verticesToDraw != 0)
            {
                #region Make sure that there are enough VertexBuffers created to hold how many vertices are needed

                // Each vertex buffer holds 6000 vertices. This is common throughout FRB rendering.
                int numberOfVertexBuffers = 1 + (verticesToDraw / 6000);

                while (mShapeVertices.Count < numberOfVertexBuffers)
                {
                    mShapeVertices.Add(new VertexPositionColor[6000]);
                }

                #endregion

                int vertexBufferNum = 0;
                int vertNum = 0;

                mRenderBreaks.Clear();

                var renderBreak = new RenderBreak(
                    0, null, ColorOperation.Color, BlendOperation.Regular, TextureAddressMode.Clamp);

#if DEBUG
                renderBreak.ObjectCausingBreak = "ShapeManager";
#endif

                mRenderBreaks.Add(renderBreak);

                int renderBreakNumber = 0;

                int verticesLeftToDraw = verticesToDraw;

                #region Fill the vertArray with the Rectangle vertices

                for (int i = 0; i < rectangles.Count; i++)
                {
                    var rectangle = rectangles[i];

                    if (rectangle.AbsoluteVisible)
                    {
                        // Since there are many kinds of shapes, check the vert num before each shape
                        if (vertNum + 5 > 6000)
                        {
                            vertexBufferNum++;
                            verticesLeftToDraw -= (vertNum);
                            vertsPerVertexBuffer.Add(vertNum);
                            vertNum = 0;
                        }

                        var buffer = mShapeVertices[vertexBufferNum];

                        buffer[vertNum + 0].Position = new Vector3(rectangle.Left, rectangle.Top, rectangle.Z);
                        buffer[vertNum + 0].Color.PackedValue = rectangle.Color.PackedValue;

                        buffer[vertNum + 1].Position = new Vector3(rectangle.Right, rectangle.Top, rectangle.Z);
                        buffer[vertNum + 1].Color.PackedValue = rectangle.Color.PackedValue;

                        buffer[vertNum + 2].Position = new Vector3(rectangle.Right, rectangle.Bottom, rectangle.Z);
                        buffer[vertNum + 2].Color.PackedValue = rectangle.Color.PackedValue;

                        buffer[vertNum + 3].Position = new Vector3(rectangle.Left, rectangle.Bottom, rectangle.Z);
                        buffer[vertNum + 3].Color.PackedValue = rectangle.Color.PackedValue;

                        buffer[vertNum + 4] = buffer[vertNum + 0];

                        vertNum += 5;
                        mRenderBreaks.Add(
                                new RenderBreak((6000) * vertexBufferNum + vertNum,
                                null, ColorOperation.Color, BlendOperation.Regular, TextureAddressMode.Clamp));
                        renderBreakNumber++;
                    }
                }
                #endregion

                #region Fill the vert array with the circle vertices

                Circle circle;

                // Loop through all of the circles
                for (int i = 0; i < circles.Count; i++)
                {
                    circle = circles[i];

                    if (circle.AbsoluteVisible)
                    {
                        if (vertNum + ShapeManager.NumberOfVerticesForCircles > 6000)
                        {
                            vertexBufferNum++;
                            verticesLeftToDraw -= (vertNum);
                            vertsPerVertexBuffer.Add(vertNum);
                            vertNum = 0;
                        }

                        for (int pointNumber = 0; pointNumber < ShapeManager.NumberOfVerticesForCircles; pointNumber++)
                        {
                            float angle = pointNumber * 2 * 3.1415928f / (ShapeManager.NumberOfVerticesForCircles - 1);

                            mShapeVertices[vertexBufferNum][vertNum + pointNumber].Position =
                                new Vector3(
                                    circle.Radius * (float)System.Math.Cos(angle) + circle.X,
                                    circle.Radius * (float)System.Math.Sin(angle) + circle.Y,
                                    circle.Z);

                            mShapeVertices[vertexBufferNum][vertNum + pointNumber].Color.PackedValue = circle.mPremultipliedColor.PackedValue;
                        }

                        vertNum += ShapeManager.NumberOfVerticesForCircles;

                        renderBreak =
                            new RenderBreak(6000 * vertexBufferNum + vertNum,
                            null, ColorOperation.Color, BlendOperation.Regular, TextureAddressMode.Clamp);

#if DEBUG
                        renderBreak.ObjectCausingBreak = "Circle";
#endif

                        mRenderBreaks.Add(renderBreak);
                        renderBreakNumber++;
                    }
                }
                #endregion

                #region Fill the vert array with the Capsule2D vertices

                Capsule2D capsule2D;
                int numberOfVerticesPerHalf = ShapeManager.NumberOfVerticesForCapsule2Ds / 2;

                // This is the distance from the center of the Capsule to the center of the endpoint
                float endPointCenterDistanceX = 0;
                float endPointCenterDistanceY = 0;

                // Loop through all of the Capsule2Ds
                for (int i = 0; i < capsule2Ds.Count; i++)
                {
                    if (vertNum + ShapeManager.NumberOfVerticesForCapsule2Ds > 6000)
                    {
                        vertexBufferNum++;
                        verticesLeftToDraw -= (vertNum);
                        vertsPerVertexBuffer.Add(vertNum);
                        vertNum = 0;
                    }

                    capsule2D = capsule2Ds[i];

                    endPointCenterDistanceX = (float)(System.Math.Cos(capsule2D.RotationZ) * (capsule2D.mScale - capsule2D.mEndpointRadius));
                    endPointCenterDistanceY = (float)(System.Math.Sin(capsule2D.RotationZ) * (capsule2D.mScale - capsule2D.mEndpointRadius));

                    // First draw one half, then the draw the other half
                    for (int pointNumber = 0; pointNumber < numberOfVerticesPerHalf; pointNumber++)
                    {
                        float angle = capsule2D.RotationZ + -1.5707963f + pointNumber * 3.1415928f / (numberOfVerticesPerHalf - 1);

                        mShapeVertices[vertexBufferNum][vertNum + pointNumber].Position =
                            new Vector3(
                                endPointCenterDistanceX + capsule2D.mEndpointRadius * (float)System.Math.Cos(angle) + capsule2D.X,
                                endPointCenterDistanceY + capsule2D.mEndpointRadius * (float)System.Math.Sin(angle) + capsule2D.Y,
                                capsule2D.Z);

                        mShapeVertices[vertexBufferNum][vertNum + pointNumber].Color.PackedValue = capsule2D.Color.PackedValue;
                    }

                    vertNum += numberOfVerticesPerHalf;

                    for (int pointNumber = 0; pointNumber < numberOfVerticesPerHalf; pointNumber++)
                    {
                        float angle = capsule2D.RotationZ + 1.5707963f + pointNumber * 3.1415928f / (numberOfVerticesPerHalf - 1);

                        mShapeVertices[vertexBufferNum][vertNum + pointNumber].Position =
                            new Vector3(
                                -endPointCenterDistanceX + capsule2D.mEndpointRadius * (float)System.Math.Cos(angle) + capsule2D.X,
                                -endPointCenterDistanceY + capsule2D.mEndpointRadius * (float)System.Math.Sin(angle) + capsule2D.Y,
                                capsule2D.Z);

                        mShapeVertices[vertexBufferNum][vertNum + pointNumber].Color.PackedValue = capsule2D.Color.PackedValue;
                    }

                    vertNum += numberOfVerticesPerHalf;

                    mShapeVertices[vertexBufferNum][vertNum] =
                        mShapeVertices[vertexBufferNum][vertNum - 2 * numberOfVerticesPerHalf];

                    vertNum++;

                    renderBreak =
                        new RenderBreak((6000) * vertexBufferNum + vertNum,
                        null, ColorOperation.Color, BlendOperation.Regular, TextureAddressMode.Clamp);

#if DEBUG
                    renderBreak.ObjectCausingBreak = "Capsule";
#endif

                    mRenderBreaks.Add(renderBreak);
                    renderBreakNumber++;
                }

                #endregion

                #region Fill the vertArray with the polygon vertices

                for (int i = 0; i < polygons.Count; i++)
                {
                    if (polygons[i].Points != null && polygons[i].Points.Count > 1)
                    {
                        // If this polygon knocks us into the next vertex buffer, then set the data for this one, then move on.
                        if (vertNum + polygons[i].Points.Count > 6000)
                        {
                            vertexBufferNum++;
                            verticesLeftToDraw -= (vertNum);
                            vertsPerVertexBuffer.Add(vertNum);
                            vertNum = 0;
                        }

                        polygons[i].Vertices.CopyTo(mShapeVertices[vertexBufferNum], vertNum);
                        vertNum += polygons[i].Vertices.Length;

                        renderBreak =
                            new RenderBreak((6000) * vertexBufferNum + vertNum,
                            null, ColorOperation.Color, BlendOperation.Regular, TextureAddressMode.Clamp);

#if DEBUG
                        renderBreak.ObjectCausingBreak = "Polygon";
#endif

                        mRenderBreaks.Add(renderBreak);
                        renderBreakNumber++;
                    }
                }

                #endregion

                #region Fill the vertArray with the line vertices

                for (int i = 0; i < lines.Count; i++)
                {
                    // If this line knocks us into the next vertex buffer, then set the data for this one, then move on.
                    if (vertNum + 2 > 6000)
                    {
                        vertexBufferNum++;
                        verticesLeftToDraw -= vertNum;
                        vertsPerVertexBuffer.Add(vertNum);
                        vertNum = 0;
                    }

                    // Add the line points
                    mShapeVertices[vertexBufferNum][vertNum + 0].Position =
                        lines[i].Position +
                        Vector3.Transform(new Vector3(
                            (float)lines[i].RelativePoint1.X,
                            (float)lines[i].RelativePoint1.Y,
                            (float)lines[i].RelativePoint1.Z),
                            lines[i].RotationMatrix);

                    mShapeVertices[vertexBufferNum][vertNum + 0].Color.PackedValue = lines[i].Color.PackedValue;

                    mShapeVertices[vertexBufferNum][vertNum + 1].Position =
                        lines[i].Position +
                        Vector3.Transform(new Vector3(
                            (float)lines[i].RelativePoint2.X,
                            (float)lines[i].RelativePoint2.Y,
                            (float)lines[i].RelativePoint2.Z),
                            lines[i].RotationMatrix);

                    mShapeVertices[vertexBufferNum][vertNum + 1].Color.PackedValue = lines[i].Color.PackedValue;

                    // Increment the vertex number past this line
                    vertNum += 2;

                    // Add a render break
                    renderBreak =
                        new RenderBreak(6000 * vertexBufferNum + vertNum,
                        null, ColorOperation.Color, BlendOperation.Regular, TextureAddressMode.Clamp);

#if DEBUG
                    renderBreak.ObjectCausingBreak = "Line";
#endif

                    mRenderBreaks.Add(renderBreak);
                    renderBreakNumber++;
                }

                #endregion

                #region Fill the vertArray with the AxisAlignedCube pieces

                for (int i = 0; i < cubes.Count; i++)
                {
                    var cube = cubes[i];

                    const int numberOfPoints = 16;

                    if (vertNum + numberOfPoints > 6000)
                    {
                        vertexBufferNum++;
                        verticesLeftToDraw -= vertNum;
                        vertsPerVertexBuffer.Add(vertNum);
                        vertNum = 0;
                    }

                    // We can do the top/bottom all in one pass
                    for (int cubeVertIndex = 0; cubeVertIndex < 16; cubeVertIndex++)
                    {
                        mShapeVertices[vertexBufferNum][vertNum + cubeVertIndex].Position = new Vector3(
                            ShapeManager.UnscaledCubePoints[cubeVertIndex].X * cube.mScaleX + cube.Position.X,
                            ShapeManager.UnscaledCubePoints[cubeVertIndex].Y * cube.mScaleY + cube.Position.Y,
                            ShapeManager.UnscaledCubePoints[cubeVertIndex].Z * cube.mScaleZ + cube.Position.Z);

                        mShapeVertices[vertexBufferNum][vertNum + cubeVertIndex].Color = cube.mColor;

                        if (cubeVertIndex == 9 || cubeVertIndex == 11 || cubeVertIndex == 13 || cubeVertIndex == 15)
                        {
                            renderBreak =
                                    new RenderBreak((6000) * vertexBufferNum + vertNum + cubeVertIndex + 1,
                                    null, ColorOperation.Color, BlendOperation.Regular, TextureAddressMode.Clamp);

#if DEBUG
                            renderBreak.ObjectCausingBreak = "Cube";
#endif

                            mRenderBreaks.Add(renderBreak);

                            renderBreakNumber++;
                        }
                    }

                    vertNum += numberOfPoints;
                }

                #endregion

                #region Fill the vertArray with the Sphere pieces

                for (int sphereIndex = 0; sphereIndex < spheres.Count; sphereIndex++)
                {
                    if (vertNum + numberOfSphereSlices * numberOfSphereVertsPerSlice > 6000)
                    {
                        vertexBufferNum++;
                        verticesLeftToDraw -= vertNum;
                        vertsPerVertexBuffer.Add(vertNum);
                        vertNum = 0;
                    }

                    var sphere = spheres[sphereIndex];

                    for (int sliceNumber = 0; sliceNumber < numberOfSphereSlices; sliceNumber++)
                    {
                        float sliceAngle = sliceNumber * 3.1415928f / numberOfSphereSlices;
                        var rotationMatrix = Matrix.CreateRotationY(sliceAngle);

                        for (int pointNumber = 0; pointNumber < numberOfSphereVertsPerSlice; pointNumber++)
                        {
                            float angle = pointNumber * 2 * 3.1415928f / (numberOfSphereVertsPerSlice - 1);

                            var newPosition = new Vector3(
                                    sphere.Radius * (float)System.Math.Cos(angle),
                                    sphere.Radius * (float)System.Math.Sin(angle), 0);

                            MathFunctions.TransformVector(ref newPosition, ref rotationMatrix);

                            newPosition += sphere.Position;

                            mShapeVertices[vertexBufferNum][vertNum + pointNumber].Position = newPosition;

                            mShapeVertices[vertexBufferNum][vertNum + pointNumber].Color.PackedValue = sphere.Color.PackedValue;
                        }

                        vertNum += numberOfSphereVertsPerSlice;

                        renderBreak = new RenderBreak((6000) * vertexBufferNum + vertNum,
                            null, ColorOperation.Color, BlendOperation.Regular, TextureAddressMode.Clamp);

#if DEBUG
                        renderBreak.ObjectCausingBreak = "Sphere";
#endif

                        mRenderBreaks.Add(renderBreak);
                    }
                }

                #endregion

                int vertexBufferIndex = 0;
                int renderBreakIndexForRendering = 0;

                for (; vertexBufferIndex < vertsPerVertexBuffer.Count; vertexBufferIndex++)
                {
                    DrawVertexList<VertexPositionColor>(camera, mShapeVertices, mRenderBreaks,
                        vertsPerVertexBuffer[vertexBufferIndex], PrimitiveType.LineStrip,
                        6000, vertexBufferIndex, ref renderBreakIndexForRendering);
                }

                DrawVertexList<VertexPositionColor>(camera, mShapeVertices, mRenderBreaks,
                    verticesLeftToDraw, PrimitiveType.LineStrip,
                    6000, vertexBufferIndex, ref renderBreakIndexForRendering);
            }
        }

        #endregion

        #region Helper drawing methods

        static void DrawMixedStart(Camera camera)
        {
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            if (camera.ClearsDepthBuffer)
            {
                GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
            }
        }

        static void PrepareSprites(
            List<VertexPositionColorTexture[]> spriteVertices,
            List<RenderBreak> renderBreaks,
            IList<Sprite> spritesToDraw, int startIndex, int numberOfVisible)
        {
            // Make sure there are enough vertex arrays
            int numberOfVertexArrays = 1 + (numberOfVisible / 1000);
            while (spriteVertices.Count < numberOfVertexArrays)
            {
                spriteVertices.Add(new VertexPositionColorTexture[6000]);
            }

            // Clear the render breaks
            renderBreaks.Clear();

            FillVertexList(spritesToDraw, spriteVertices,
                renderBreaks, startIndex, numberOfVisible);

            mRenderBreaksAllocatedThisFrame += renderBreaks.Count;

            if (RecordRenderBreaks)
            {
                LastFrameRenderBreakList.AddRange(renderBreaks);
            }
        }

        static int DrawSprites(
            List<VertexPositionColorTexture[]> spriteVertices,
            List<RenderBreak> renderBreaks,
            IList<Sprite> spritesToDraw, int startIndex, int numberOfVisibleSprites, Camera camera)
        {
            // Prepare device settings

            // TODO:  Turn off cull mode
            var oldTextureAddressMode = TextureAddressMode;
            if (spritesToDraw.Count > 0)
                TextureAddressMode = spritesToDraw[0].TextureAddressMode;

            // numberToRender * 2 represents how many triangles. Therefore we only want to use the number of visible Sprites
            DrawVertexList<VertexPositionColorTexture>(camera, spriteVertices, renderBreaks,
                numberOfVisibleSprites * 2, PrimitiveType.TriangleList, 6000);

            TextureAddressMode = oldTextureAddressMode;

            return numberOfVisibleSprites;
        }

        static void DrawTexts(List<Text> texts, int startIndex, int numToDraw, Camera camera, Section section)
        {
            if (TextManager.UseNativeTextRendering)
            {
                TextManager.DrawTexts(texts, startIndex, numToDraw, camera);
            }
            else
            {
                #region Bitmap font rendering

                int totalVertices = 0;

                for (int i = startIndex; i < startIndex + numToDraw; i++)
                {
                    if (!texts[i].RenderOnTexture)
                        totalVertices += texts[i].VertexCount;
                }

                if (totalVertices == 0)
                {
                    return;
                }

                var oldTextureFilter = FlatRedBallServices.GraphicsOptions.TextureFilter;

                if (TextManager.FilterTexts == false)
                {
                    FlatRedBallServices.GraphicsOptions.TextureFilter = TextureFilter.Point;
                }

                int numberOfVertexBuffers = 1 + (totalVertices / 6000);

                mTextRenderBreaks.Clear();

                // If there are not enough vertex buffers to hold all of the vertices for drawing the texts, add more.
                while (mTextVertices.Count < numberOfVertexBuffers)
                {
                    mTextVertices.Add(new VertexPositionColorTexture[6000]);
                }

                int numberToRender = FillVertexList(texts, mTextVertices, camera, mTextRenderBreaks, startIndex, numToDraw, totalVertices);
                mRenderBreaksAllocatedThisFrame += mTextRenderBreaks.Count;

                if (RecordRenderBreaks)
                {
                    LastFrameRenderBreakList.AddRange(mTextRenderBreaks);
                }

                DrawVertexList<VertexPositionColorTexture>(camera, mTextVertices, mTextRenderBreaks,
                    totalVertices / 3, PrimitiveType.TriangleList, 6000);

                FlatRedBallServices.GraphicsOptions.TextureFilter = oldTextureFilter;

                #endregion
            }
        }

        public static void DrawVertexList<T>(Camera camera, List<T[]> vertexList, List<RenderBreak> renderBreaks,
            int numberOfPrimitives, PrimitiveType primitiveType, int verticesPerVertexBuffer) where T : struct, IVertexType
        {
            int throwAway = 0;
            DrawVertexList(camera, vertexList, renderBreaks, numberOfPrimitives, primitiveType, verticesPerVertexBuffer, 0, ref throwAway);
        }

        public static void DrawVertexList<T>(Camera camera, List<T[]> vertexList, List<RenderBreak> renderBreaks,
            int numberOfPrimitives, PrimitiveType primitiveType, int verticesPerVertexBuffer, int vbIndex, ref int renderBreakIndex) where T : struct, IVertexType
        {
            bool startedOnNonZeroVBIndex = vbIndex != 0;

            if (numberOfPrimitives == 0) return;

            var oldTextureAddressMode = TextureAddressMode;

            #region Get the verticesPerPrimitive and extraVertices according to the PrimitiveType

            int verticesPerPrimitive = 1;

            // Some primitive types, like LineStrip, require 1 extra vertex for the initial point.
            // that is, to draw 3 lines, 4 points are needed.  This variable is used for that
            int extraVertices = 0;

            switch (primitiveType)
            {
                case PrimitiveType.TriangleList:
                    verticesPerPrimitive = 3;
                    break;
                case PrimitiveType.LineStrip:
                    verticesPerPrimitive = 1;
                    extraVertices = 1;
                    break;
            }

            #endregion

            int drawToOnThisVB = 0;
            int drawnOnThisVB = 0;
            int totalDrawn = 0;
            int VBOn = vbIndex;
            int numPasses = 0;
            int numberOfPrimitivesPerVertexBuffer = verticesPerVertexBuffer / verticesPerPrimitive;
            var effectToUse = mCurrentEffect;

            if (primitiveType == PrimitiveType.LineStrip)
            {
                mBasicEffect.LightingEnabled = false;
                mBasicEffect.VertexColorEnabled = true;
                effectToUse = mBasicEffect;
            }

            if (renderBreaks.Count != 0)
            {
                renderBreaks[renderBreakIndex].SetStates();
                renderBreakIndex++;
            }

            #region Move through all of the VBs and draw them

            while (totalDrawn < numberOfPrimitives)
            {
                numPasses++;

                if (startedOnNonZeroVBIndex)
                {
                    drawToOnThisVB = System.Math.Min(numberOfPrimitivesPerVertexBuffer, (numberOfPrimitives));
                }
                else
                {
                    drawToOnThisVB = System.Math.Min(numberOfPrimitivesPerVertexBuffer, (numberOfPrimitives - (numberOfPrimitivesPerVertexBuffer * VBOn)));
                }

                if (drawToOnThisVB < 0)
                {
                    drawToOnThisVB = numberOfPrimitives;
                }

                if (renderBreakIndex < renderBreaks.Count && renderBreaks[renderBreakIndex].ItemNumber < numberOfPrimitivesPerVertexBuffer * VBOn + drawToOnThisVB)
                {
                    drawToOnThisVB = renderBreaks[renderBreakIndex].ItemNumber - (numberOfPrimitivesPerVertexBuffer * VBOn);
                    var currentTechnique = effectToUse.CurrentTechnique;

                    foreach (EffectPass pass in currentTechnique.Passes)
                    {
                        pass.Apply();

                        if (drawToOnThisVB != drawnOnThisVB)
                        {
                            mGraphics.GraphicsDevice.DrawUserPrimitives<T>(
                                primitiveType,
                                vertexList[VBOn],
                                verticesPerPrimitive * drawnOnThisVB,
                                drawToOnThisVB - drawnOnThisVB - extraVertices,
                                vertexList[VBOn][0].VertexDeclaration);
                        }
                    }

                    renderBreaks[renderBreakIndex - 1].Cleanup();
                    renderBreaks[renderBreakIndex].SetStates();

                    renderBreakIndex++;
                }
                else
                {
                    var currentTechnique = effectToUse.CurrentTechnique;

                    foreach (EffectPass pass in currentTechnique.Passes)
                    {
                        pass.Apply();

                        if (drawToOnThisVB - extraVertices != drawnOnThisVB)
                        {
                            try
                            {
                                mGraphics.GraphicsDevice.DrawUserPrimitives<T>(
                                    primitiveType,
                                    vertexList[VBOn],
                                    verticesPerPrimitive * drawnOnThisVB,
                                    drawToOnThisVB - drawnOnThisVB - extraVertices);

                            }
                            catch (Exception e)
                            {
                                int m = 3;
                            }
                        }
                    }
                    renderBreaks[renderBreakIndex - 1].Cleanup();
                }

                totalDrawn += drawToOnThisVB - drawnOnThisVB;
                drawnOnThisVB = drawToOnThisVB;

                if (drawToOnThisVB == numberOfPrimitivesPerVertexBuffer && totalDrawn < numberOfPrimitives)
                {
                    VBOn++;
                    drawnOnThisVB = 0;
                }
            }

            if (TextureAddressMode != oldTextureAddressMode)
                TextureAddressMode = oldTextureAddressMode;

            #endregion
        }

        #endregion

        #region Vertex buffer helpers

        internal static void SetNumberOfThreadsToUse(int numberOfUpdaters)
        {
            mFillVertexLogics.Clear();

            for (int i = 0; i < numberOfUpdaters; i++)
            {
                var logic = new FillVertexLogic();
                mFillVertexLogics.Add(logic);
            }
        }

        static void FillVertexList(IList<Sprite> sa,
            List<VertexPositionColorTexture[]> vertexLists,
            List<RenderBreak> renderBreaks, int firstSprite,
            int numberToDraw)
        {
            mFillVBListCallsThisFrame++;

            // If the array is empty, then we just exit.
            if (sa.Count == 0) return;

            // Clear the places where batching breaks occur.
            renderBreaks.Clear();

            int vertexBufferNum = 0;
            int vertNumForRenderBreaks = 0;
            int vertexBufferNumForRenderBreaks = 0;
            var arrayAtIndex = vertexLists[vertexBufferNum];
            var renderBreak = new RenderBreak(firstSprite, sa[firstSprite]);

            renderBreaks.Add(renderBreak);

            int renderBreakNumber = 0;
            bool addedNewVertexBuffer = false;

            for (int i = firstSprite; i < firstSprite + numberToDraw; i++)
            {
                var spriteAtIndex = sa[i];

                #region If the Sprite is different from the last RenderBreak, break the batch

                if (renderBreaks[renderBreakNumber].DiffersFrom(spriteAtIndex) || addedNewVertexBuffer)
                {
                    renderBreak = new RenderBreak(2000 * vertexBufferNumForRenderBreaks + vertNumForRenderBreaks / 3, spriteAtIndex);

                    // Mark where the break occurred
                    renderBreaks.Add(renderBreak);
                    renderBreakNumber++;
                }

                #endregion

                vertNumForRenderBreaks += 6;

                if (vertNumForRenderBreaks == 6000)
                {
                    vertexBufferNumForRenderBreaks++;
                    vertNumForRenderBreaks = 0;
                    addedNewVertexBuffer = true;
                }
                else
                {
                    addedNewVertexBuffer = false;
                }
            }

            float ratio = 1 / ((float)mFillVertexLogics.Count);
            int spriteCount = (int)(ratio * numberToDraw);

            for (int i = 0; i < mFillVertexLogics.Count; i++)
            {
                mFillVertexLogics[i].SpriteList = sa;
                mFillVertexLogics[i].VertexLists = vertexLists;
                mFillVertexLogics[i].StartIndex = firstSprite + spriteCount * i;
                mFillVertexLogics[i].FirstSpriteInAllSimultaneousLogics = firstSprite;

                if (i == mFillVertexLogics.Count - 1)
                {
                    mFillVertexLogics[i].Count = (firstSprite + numberToDraw) - mFillVertexLogics[i].StartIndex;
                }
                else
                {
                    mFillVertexLogics[i].Count = spriteCount;
                }
            }

            if (mFillVertexLogics.Count == 1)
            {
                // If there's only 1 vertex logic, no need to make it async, that causes memory allocations.
                // Maybe look at multithread fixes for memory allocations too but...at least let's make the default
                // case not allocate:
                mFillVertexLogics[0].FillVertexListSync(null);
            }
            else
            {
                for (int i = 0; i < mFillVertexLogics.Count; i++)
                {
                    mFillVertexLogics[i].FillVertexList();
                }

                for (int i = 0; i < mFillVertexLogics.Count; i++)
                {
                    mFillVertexLogics[i].Wait();
                }
            }
        }

        static int FillVBList(List<Text> texts,
            List<DynamicVertexBuffer> vertexBufferList, Camera camera,
            List<RenderBreak> renderBreaks, int firstText,
            int numToDraw, int numberOfVertices)
        {
            #region If the array is empty or if all texts are empty, just exit

            bool toExit = true;

            if (texts.Count != 0)
            {
                for (int i = 0; i < texts.Count; i++)
                {
                    Text t = texts[i];
                    if (t.VertexCount != 0)
                    {
                        toExit = false;
                        break;
                    }
                }
            }

            if (toExit)
                return 0;

            #endregion

            renderBreaks.Clear();
            int vertexBufferNum = 0;

            // Grab the first vertex buffer
            var vertexBuffer = vertexBufferList[vertexBufferNum];
            int vertNum = 0;

            var renderBreak = new RenderBreak(firstText, texts[firstText].Font.Texture, texts[firstText].ColorOperation,
                BlendOperation.Regular, TextureAddressMode.Clamp);

#if DEBUG
            renderBreak.ObjectCausingBreak = texts[firstText];
#endif

            renderBreaks.Add(renderBreak);

            int renderBreakNumber = 0;

            #region Calculate verticesLeftToRender

            int verticesLeftToRender = 0;

            for (int i = firstText; i < firstText + numToDraw; i++)
            {
                if (texts[i].AbsoluteVisible)
                    verticesLeftToRender += texts[i].VertexCount;
            }

            #endregion

            for (int i = firstText; i < firstText + numToDraw; i++)
            {
                #region If the Text is different from the last RenderBreak, break the batch

                if (renderBreaks[renderBreakNumber].DiffersFrom(texts[i]))
                {
                    renderBreak = new RenderBreak(2000 * vertexBufferNum + vertNum / 3,
                        texts[i].Font.Texture, texts[i].ColorOperation, texts[i].BlendOperation, TextureAddressMode.Clamp);
#if DEBUG
                    renderBreak.ObjectCausingBreak = texts[i];
#endif
                    renderBreaks.Add(renderBreak);
                    renderBreakNumber++;
                }

                #endregion

                int verticesLeftOnThisText = texts[i].VertexCount;

                #region If this text will fit on the current vertex buffer, copy over the info

                if (vertNum + verticesLeftOnThisText < 6000)
                {
                    for (int textVertex = 0; textVertex < texts[i].VertexCount; textVertex++)
                    {
                        mVertexArray[vertNum] = texts[i].VertexArray[textVertex];
                        vertNum++;
                    }

                    verticesLeftToRender -= texts[i].VertexCount;

                    if (vertNum == 6000 &&
                        ((i + 1 < firstText + numToDraw) ||
                         verticesLeftToRender != 0))
                    {
#if MONODROID
                        vertexBufferList[vertexBufferNum].SetData<VertexPositionColorTexture>(mVertexArray);
#else
                        vertexBufferList[vertexBufferNum].SetData<VertexPositionColorTexture>(mVertexArray, 0, vertNum, SetDataOptions.Discard);
#endif
                        vertexBufferNum++;
                        vertNum = 0;
                    }
                }

                #endregion

                #region The text won't fit on this vertex buffer, so copy some over so break the text up

                else
                {
                    int textVertexIndexOn = 0;

                    while (verticesLeftOnThisText > 0)
                    {
                        int numberToCopy = System.Math.Min(verticesLeftToRender, 6000 - vertNum);

                        for (int numberCopied = 0; numberCopied < numberToCopy; numberCopied++)
                        {
                            mVertexArray[vertNum] = texts[i].VertexArray[textVertexIndexOn];
                            vertNum++;
                            verticesLeftToRender--;
                            textVertexIndexOn++;
                        }

                        // Now write the verts if there is more to draw after this
                        if (vertNum == 6000 &&
                            ((i + 1 < firstText + numToDraw) ||
                             verticesLeftToRender != 0))
                        {
#if MONODROID
                            vertexBufferList[vertexBufferNum].SetData<VertexPositionColorTexture>(mVertexArray);
#else
                            vertexBufferList[vertexBufferNum].SetData<VertexPositionColorTexture>(mVertexArray, 0, vertNum, SetDataOptions.Discard);
#endif
                            vertexBufferNum++;
                            vertNum = 0;
                            verticesLeftOnThisText -= 6000;
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                #endregion
            }

            GraphicsDevice.SetVertexBuffer(null);

#if MONODROID
            vertexBufferList[vertexBufferNum].SetData<VertexPositionColorTexture>(mVertexArray);
#else
            vertexBufferList[vertexBufferNum].SetData<VertexPositionColorTexture>(mVertexArray, 0, vertNum, SetDataOptions.Discard);
#endif

            return vertNum / 3 + vertexBufferNum * 2000;
        }

        static int FillVertexList(IList<Text> texts,
            List<VertexPositionColorTexture[]> vertexLists, Camera camera,
            List<RenderBreak> renderBreaks, int firstText,
            int numToDraw, int numberOfVertices)
        {
            #region If the array is empty or if all texts are empty, just exit

            bool toExit = true;

            if (texts.Count != 0)
            {
                for (int i = 0; i < texts.Count; i++)
                {
                    Text t = texts[i];
                    if (t.AbsoluteVisible && t.VertexCount != 0)
                    {
                        toExit = false;
                        break;
                    }
                }
            }

            if (toExit)
                return 0;

            #endregion

            renderBreaks.Clear();
            int vertexBufferNum = 0;
            int vertNum = 0;

            var renderBreak = new RenderBreak(firstText, texts[firstText], 0);

            renderBreaks.Add(renderBreak);

            int renderBreakNumber = 0;

            #region Calculate verticesLeftToRender

            int verticesLeftToRender = 0;

            for (int i = firstText; i < firstText + numToDraw; i++)
            {
                if (texts[i].AbsoluteVisible)
                    verticesLeftToRender += texts[i].VertexCount;
            }

            #endregion

            for (int i = firstText; i < firstText + numToDraw; i++)
            {
                var textAtIndex = texts[i];

                if (textAtIndex.AbsoluteVisible == false || textAtIndex.VertexCount == 0)
                    continue;

                #region If the Text is different from the last RenderBreak, break the batch

                if (renderBreaks[renderBreakNumber].DiffersFrom(textAtIndex))
                {
                    renderBreak = new RenderBreak((2000 * vertexBufferNum + vertNum / 3),
                        textAtIndex, 0);

                    renderBreaks.Add(renderBreak);
                    renderBreakNumber++;
                }

                #endregion

                int verticesLeftOnThisText = textAtIndex.VertexCount;

                #region If this text will fit on the current vertex buffer, copy over the info

                if (vertNum + verticesLeftOnThisText < 6000)
                {
                    Array.Copy(textAtIndex.VertexArray, 0, vertexLists[vertexBufferNum], vertNum, textAtIndex.VertexCount);

                    AddRenderBreaksForTextureSwitches(textAtIndex, renderBreaks, ref renderBreakNumber, vertNum / 3, 0, int.MaxValue);

                    vertNum += textAtIndex.VertexCount;

                    verticesLeftToRender -= textAtIndex.VertexCount;

                    if (vertNum == 6000 &&
                        ((i + 1 < firstText + numToDraw) ||
                         verticesLeftToRender != 0))
                    {
                        vertexBufferNum++;
                        vertNum = 0;
                    }
                }

                #endregion

                #region The text won't fit on this vertex buffer, so copy some over so break the text up

                else
                {
                    int textVertexIndexOn = 0;

                    while (verticesLeftOnThisText > 0)
                    {
                        int numberToCopy = System.Math.Min(verticesLeftOnThisText, 6000 - vertNum);

                        int relativeIndexToStartAt = textAtIndex.VertexCount - verticesLeftOnThisText;

                        AddRenderBreaksForTextureSwitches(textAtIndex, renderBreaks, ref renderBreakNumber, vertNum / 3,
                            relativeIndexToStartAt / 3, numberToCopy / 3 + relativeIndexToStartAt);

                        // This can be sped up using Array.Copy. Do this sometime.
                        for (int numberCopied = 0; numberCopied < numberToCopy; numberCopied++)
                        {
                            vertexLists[vertexBufferNum][vertNum] = texts[i].VertexArray[textVertexIndexOn];
                            vertNum++;
                            verticesLeftToRender--;
                            textVertexIndexOn++;
                        }

                        // Now write the verts if there is more to draw after this
                        if (vertNum == 6000 &&
                            ((i + 1 < firstText + numToDraw) ||
                             verticesLeftToRender != 0))
                        {
                            vertexBufferNum++;
                            vertNum = 0;
                            verticesLeftOnThisText -= textVertexIndexOn;
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                #endregion
            }

            return vertNum / 3 + vertexBufferNum * 2000;
        }

        static void AddRenderBreaksForTextureSwitches(Text textAtIndex,
            List<RenderBreak> renderBreaks, ref int renderBreakNumber, int startIndex, int minimumTriangleIndex, int maximumTriangleIndex)
        {
            for (int i = 0; i < textAtIndex.mInternalTextureSwitches.Count; i++)
            {
                Microsoft.Xna.Framework.Point point = textAtIndex.mInternalTextureSwitches[i];

                if (point.X >= minimumTriangleIndex && point.X < maximumTriangleIndex)
                {
                    var renderBreakToAdd = new RenderBreak(
                        startIndex + point.X,
                        textAtIndex, point.Y);

                    renderBreaks.Add(renderBreakToAdd);
                    renderBreakNumber++;
                }
            }
        }

        #endregion

        static void SortBatchesZInsertionAscending(IList<IDrawableBatch> batches)
        {
            if (batches.Count == 1 || batches.Count == 0)
                return;

            int whereBatchBelongs;

            for (int i = 1; i < batches.Count; i++)
            {
                if (batches[i].Z < batches[i - 1].Z)
                {
                    if (i == 1)
                    {
                        batches.Insert(0, batches[i]);
                        batches.RemoveAt(i + 1);
                        continue;
                    }

                    for (whereBatchBelongs = i - 2; whereBatchBelongs > -1; whereBatchBelongs--)
                    {
                        if (batches[i].Z >= batches[whereBatchBelongs].Z)
                        {
                            batches.Insert(whereBatchBelongs + 1, batches[i]);
                            batches.RemoveAt(i + 1);
                            break;
                        }
                        else if (whereBatchBelongs == 0 && batches[i].Z < batches[0].Z)
                        {
                            batches.Insert(0, batches[i]);
                            batches.RemoveAt(i + 1);
                            break;
                        }
                    }
                }
            }
        }

        #endregion

        #endregion
    }
}
