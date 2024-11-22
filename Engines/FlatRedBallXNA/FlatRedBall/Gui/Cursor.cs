#define SUPPORTS_TOUCH_SCREEN

using System;
using System.Collections.Generic;

using FlatRedBall.Gui;
using FlatRedBall.ManagedSpriteGroups;

using FlatRedBall.Math;
using FlatRedBall.Graphics;
using FlatRedBall.Math.Geometry;
using FlatRedBall.Input;


using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.ComponentModel;


namespace FlatRedBall.Gui
{
    #region Enums

    // This is going to be a field so we can use
    // both cursor and touch screen simultaneously.
    public enum InputDevice
    {
        TouchScreen = 1,
        Mouse = 2
    }

    #endregion

    /// <summary>
    /// The cursor is a controllable graphical icon which can interact with FRB elements and
    /// stores information about mouse activity. By default the Cursor is controlled by the mouse
    /// but it can be controlled by gamepads or custom delegates.
    /// </summary>
    public partial class Cursor
    {
        #region Fields

        DelegateBasedPressableInput mPrimaryButton;
        DelegateBasedPressableInput mSecondaryButton;
        DelegateBasedPressableInput mMiddleButton;

        /// <summary>
        /// Gets the main cursor. If multiple Cursors have been added, returns the first one.
        /// </summary>
        public static Cursor Main => GuiManager.Cursor;

        IWindow mWindowPushed;

        /// <summary>
        /// The window that the cursor was over when the mouse button was pressed.
        /// </summary>
        /// <remarks>
        /// When the secondary (right) mouse button is pushed down, this value is set - either to null 
        /// if the mouse is not over any Windows or to the Window that the mouse is over.  
        /// When the secondary mouse button is released (clicked), this value is set to null.  
        /// This value is useful for clicking on Windows.  Specifically, when the cursor is clicked
        /// on a Button, the WindowPushed value is tested to make sure that it is the same as 
        /// the Window clicked on.  This allows for players to push on a Button but move 
        /// the mouse away and click elsewhere without clicking on the Button originally 
        /// pushed and without accidentally clicking on other Buttons.  This is also used
        /// with ToggleButtons to control when they are pressed and unpressed.
        /// </remarks>
        public IWindow mWindowSecondaryPushed;

        /// <summary>
        /// If this value is true, the Cursor will not move in response to the mouse or gamepad.
        /// </summary>
        /// <remarks>
        /// This value can be set to true if the Cursor should not move in response to input.
        /// The staticPosition value is only used by the engine when over the button on an UpDown.  When pushing down
        /// on an UpDown button, the staticPosition is set to true, and set to false when releasing the mouse button.
        /// </remarks>
        public bool StaticPosition;

        private IWindow mLastWindowOver;

        /// <summary>
        /// The window that the cursor has grabbed.
        /// </summary>
        /// <remarks>
        /// When the mouse button is released, the windowGrabbed reference is set to null.  If a Window is grabbed, it will
        /// move as the mouse moves.  This is used commonly for dragging on menu bars and scroll bars.  The cursor does
        /// not recognize which types of Windows can be dragged, so windows must be grabbed through the Cursor.GrabWindow
        /// method.  The windowGrabbed's onDrag event is fired every frame.
        /// </remarks>
        public IWindow WindowGrabbed;

        Sides mSidesGrabbed = Sides.None;

        /// <summary>
        /// Matrix used to apply a transformation to the Cursor's position. This should only be used
        /// to adjust the crusor position relative to the Camera's destination rectangle.
        /// </summary>
        public Matrix TransformationMatrix = Matrix.Identity;

        double mScreenX;
        double mScreenY;

        float mGrabbedWindowRelativeX;
        float mGrabbedWindowRelativeY;

        // these are used to calculate the velocity of the windows Cursor
        double mLastScreenX;
        double mLastScreenY;

        // On Windows RT we need to scale the values.  We will
        // only read values from the hardware (mouse or touch)
        // when either the user touches the screen or when the user
        // moves the mouse.  If neither has happened then we want to 
        // store off the last read value and use those.
        int mLastXFromHardware;
        int mLastYFromHardware;

        bool mHasDraggedAwayFromPushPoint;

        Ray mLastRay;

        /// <summary>
        /// Storage for reference to a grabbed object.
        /// </summary>
        /// <remarks>
        /// This variable has no internal engine functionality.  It merely provies a place to store a reference
        /// to a grabbed Object - useful in graphical applications where the Cursor can grab and move objects such
        /// as Sprites or Text objects.
        /// The ObjectGrabbedRelativeX and ObjectGrabbedRelativeY can also be set to keep the object static after a click rather than
        /// "snapping" its center to the Cursor's tip.
        /// <seealso cref="FlatRedBall.Gui.Cursor.SetObjectRelativePosition"/>
        /// </remarks>
        IStaticPositionable mObjectGrabbed;

        /// <summary>
        /// The relative x position of a grabbed object from the center of the cursor.
        /// </summary>
        /// <remarks>
        /// This value can be set through the SetObjectRelativePosition.  
        /// This value used in the GetCursorPositionForSprite method.
        ///  <seealso cref="FlatRedBall.Gui.Cursor.GetCursorPositionForSprite(ref float, ref float, float)"/>
        ///  <seealso cref="FlatRedBall.Gui.Cursor.SetObjectRelativePosition"/>
        /// </remarks>
        public float ObjectGrabbedRelativeX;

        /// <summary>
        /// The relative y position of a grabbed object from the center of the cursor.
        /// </summary>
        /// <remarks>
        /// This value can be set through the SetObjectRelativePosition.  
        /// This value used in the GetCursorPositionForSprite method.
        ///  <seealso cref="FlatRedBall.Gui.Cursor.GetCursorPositionForSprite(ref float, ref float, float)"/>
        ///  <seealso cref="FlatRedBall.Gui.Cursor.SetObjectRelativePosition"/>
        /// </remarks>
        public float ObjectGrabbedRelativeY;

        /// <summary>
        /// Whether the primary button was pushed this frame. A push is when the primary button is not down last frame but is down this frame.
        /// </summary>
        public bool PrimaryPush;

        public bool PrimaryDoublePush;

        /// <summary>
        /// Determines whether the primary button is down this frame.
        /// </summary>
        public bool PrimaryDown;

        /// <summary>
        /// Whether the primary button was clicked (released) this frame. A release is when the primary button is down last frame but not down this frame.
        /// </summary>
        public bool PrimaryClick;

        /// <summary>
        /// Determines whether the primary button was double clicked this frame.
        /// </summary>
        public bool PrimaryDoubleClick;


        internal bool mLastPrimaryDown;


        double mLastTimePrimaryClick;
        double mLastTimePrimaryPush;

        /// <summary>
        /// Determines whether the secondary button was pushed this frame.
        /// </summary>
        public bool SecondaryPush;

        /// <summary>
        /// Determines whether the secondary button is down this frame.
        /// </summary>
        public bool SecondaryDown;

        /// <summary>
        /// Determines whether the secondary button was clicked (released) this frame.
        /// </summary>
        public bool SecondaryClick;

        /// <summary>
        /// Determines whether the secondary button was double clicked this frame.
        /// </summary>
        public bool SecondaryDoubleClick;


        internal bool mLastSecondaryDown;
        //double mLastTimeSecondaryClick;

        /// <summary>
        /// Whether the middle mouse button is down this frame.
        /// </summary>
        public bool MiddleDown
        {
            get;
            set;
        }

        /// <summary>
        /// Whether the middle mouse button was pushed this frame (not down last frame but down this frame).
        /// </summary>
        public bool MiddlePush
        {
            get;
            set;
        }

        /// <summary>
        /// Whether the middle mouse button was clicked (released) this frame.
        /// </summary>
        public bool MiddleClick
        {
            get;
            set;
        }

        internal bool mLastMiddleDown;

        //bool mUsingMouse;
        //Xbox360GamePad mGamepad;

        List<IInputDevice> devicesControllingCursor = new List<IInputDevice>();

        /// <summary>
        /// The IInputDevices controlling the cursor. By default this is the Mouse.
        /// </summary>
        public IReadOnlyList<IInputDevice> DevicesControllingCursor => devicesControllingCursor;

        /// <summary>
        /// Reference to the camera to which the cursor belongs.
        /// </summary>
		internal protected Camera mCamera;
        //		float distanceFromCamera;

        internal bool mUsingWindowsCursor = true;

        bool ignoreNextFrameInput;
        // When a frame of input is ignored, the following frame
        // may pick up that the cursor is down. We want to ignore pushes
        // for that frame too:
        bool ignoreNextPush;

        IWindow mWindowOver;

        /// <summary>
        /// Returns the Window that this cursor is over. If the cursor is not over any Window, this value will be null.
        /// </summary>
        /// <remarks>
        /// This value is usually used in two situations:
        /// 1. To test if the cursor is over a Window to prevent clicking on objects in the game world. For example, this is checked
        /// first, then tests against entities are performed if this value is null.
        /// 2. To diagnose UI issues. The WindowOver can be printed to the screen to detect if which window is receiving cursor input.
        /// </remarks>
        public IWindow WindowOver
        {
            get
            {
                return mWindowOver;
            }
            set
            {
                mWindowOver = value;
            }
        }
        public IWindow WindowClosing;

        float mPushedX;
        float mPushedY;


        #region members for speeding up method calls
        Vector3 vector3;
        #endregion

        #endregion

        #region Properties

        /// <summary>
        /// The Primary button (usually left mouse button) which can be tested for clicks, pushes, and if it is held down.
        /// </summary>
        /// <remarks>
        /// This object is internally created only once, and that same reference is returned on subsequent calls to keep 
        /// allocation low.
        /// </remarks>
        public IPressableInput PrimaryButton
        {
            get
            {
                if (mPrimaryButton == null)
                {
                    mPrimaryButton = new DelegateBasedPressableInput(
                        () => this.PrimaryDown,
                        () => this.PrimaryPush,
                        () => this.PrimaryClick
                        );
                }
                return mPrimaryButton;
            }
        }

        public IPressableInput SecondaryButton
        {
            get
            {
                if (mSecondaryButton == null)
                {
                    mSecondaryButton = new DelegateBasedPressableInput(
                        () => this.SecondaryDown,
                        () => this.SecondaryPush,
                        () => this.SecondaryClick
                        );
                }
                return mSecondaryButton;
            }
        }

        public IPressableInput MiddleButton
        {
            get
            {

                if (mMiddleButton == null)
                {
                    mMiddleButton = new DelegateBasedPressableInput(
                        () => this.MiddleDown,
                        () => this.MiddlePush,
                        () => this.MiddleClick
                        );
                }
                return mMiddleButton;
            }
        }


        public InputDevice SupportedInputDevices
        {
            get;
            set;
        }

        /// <summary>
        /// The last InputDevice that controlled the cursor. Typically this would be a touch screen or mouse.
        /// </summary>
        public InputDevice LastInputDevice
        {
            get;
            private set;
        }

        bool active = true;
        /// <summary>
        /// Sets whether the cursor is active.
        /// </summary>
        /// <remarks>
        /// An inactive cursor will not move or read any input.
        /// Setting the active property to false will also clear the following
        /// fields:
        /// 
        /// <para>primaryClick</para>
        /// <para>primaryDoubleClick</para>
        /// <para>primaryDown</para>
        /// <para>primaryPush</para>
        /// <para>secondaryClick</para>
        /// <para>secondaryDoubleClick</para>
        /// <para>secondaryDown</para>
        /// <para>secondaryPush</para>
        /// 
        /// </remarks>
        public bool Active
        {
            set
            {
                active = value;
                if (value == false)
                {
                    ClearInputValues();
                }
            }
            get { return active; }
        }


        /// <summary>
        /// Whether the game window must be in focus to perform cursor logic. If false, then
        /// clicks will be processed even if the Window isn't in focus.
        /// </summary>
        public bool RequiresGameWindowInFocus { get; set; } = true;

        public Camera Camera
        {
            get { return mCamera; }
            set { mCamera = value; }
        }


        public bool IgnoreInputThisFrame
        {
            get
            {
                // Users will assign IgnoreInputThisFrame because it makes sense conceptually, but technically
                // when the user assigns this value the engine has already processed input this frame. They probably
                // want it ignored next frame. Internally we'll use the technical terminology.
                return ignoreNextFrameInput;
            }
            set
            {
                ignoreNextFrameInput = value;
                ignoreNextPush = value;
                if (value)
                {
                    PrimaryClick = false;
                    PrimaryPush = false;
                }
            }
        }

        internal IWindow LastWindowOver
        {
            get { return mLastWindowOver; }
            set
            {
                if (value != mLastWindowOver)
                {
                    // there's been a change
                    if (mLastWindowOver != null)
                    {
                        CallRollOffRecursively(mLastWindowOver);

                    }

                    mLastWindowOver = value;

                    if (mLastWindowOver != null)
                        mLastWindowOver.CallRollOn();

                }
            }
        }

        private void CallRollOffRecursively(IWindow window)
        {
            window.CallRollOff();
            if(window.Parent != null && !window.Parent.HasCursorOver(this))
            {
                CallRollOffRecursively(window.Parent);
            }
        }

        public int ClickNoSlideThreshold
        {
            get;
            set;
        }

        public bool PrimaryClickNoSlide
        {
            get
            {
                int axisThreshold = ClickNoSlideThreshold;

                return this.PrimaryClick && !mHasDraggedAwayFromPushPoint;
            }
        }

        public bool PrimaryClickQuick
        {
            get
            {
                const float quickClickTime = .2f;
                return PrimaryClickNoSlide && PrimaryPushTime < quickClickTime;
            }
        }

        public double PrimaryPushTime
        {
            get
            {
                if (PrimaryDown || PrimaryClick)
                {
                    return TimeManager.SecondsSince(mLastTimePrimaryPush);
                }
                else
                {
                    return 0;
                }
            }
        }


        public bool HasMovedLessThanXPixelsSincePush(int x)
        {
            return System.Math.Abs(ScreenX - mPushedX) < x &&
                    System.Math.Abs(ScreenY - mPushedY) < x;
        }

        /// <summary>
        /// The movement rate of the controlling input (usually mouse) on the z axis. For the mouse this refers to the scroll wheel.
        /// </summary>
        public float ZVelocity
        {
            get;
            set;
        }



        /// <summary>
        /// The number of pixels between the left of the screen and the cursor. Note that this does not consider
        /// the camera's position, orientation, or perspective - it will always return 0 at the left-edge of the screen.
        /// </summary>
        /// <remarks>This can be set if the cursor is using an input device which modifies the position incrementally (such as an XboxGamePad).
        /// If using an input device which sets the position in absolute coordiantes (such as a Mouse), this value will be overwritten every frame.</remarks>
        public int ScreenX
        {

            get { return (int)mScreenX; }
            // set made public for custom modification of cursor
            set { mScreenX = value; }
        }

        /// <summary>
        /// The number of pixels between the top of the screen and the cursor. Note that this does not consider
        /// the camera's position, orientation, or perspective - it will always return 0 at the top-edge of the screen. This value
        /// increases downward.
        /// </summary>
        /// <remarks>This can be set if the cursor is using an input device which modifies the position incrementally (such as an XboxGamePad).
        /// If using an input device which sets the position in absolute coordiantes (such as a Mouse), this value will be overwritten every frame.</remarks>
        public int ScreenY
        {
            get { return (int)mScreenY; }
            // set made public for custom modification of cursor
            set { mScreenY = value; }
        }

        public int LastScreenX
        {
            get { return (int)mLastScreenX; }
            // set made public for custom modification of cursor
            set { mLastScreenX = value; }
        }

        public int LastScreenY
        {
            get { return (int)mLastScreenY; }
            // set made public for custom modification of cursor
            set { mLastScreenY = value; }

        }

        /// <summary>
        /// Returns the number of pixels on the X axis
        /// that the cursor has moved since the last
        /// frame. This can be used to move grabbed windows.
        /// </summary>
        public int ScreenXChange
        {
            get
            {
                return ScreenX - LastScreenX;
            }
        }

        // Exists to match the MonoGame cursor so we can unify the code
        // You can use this or ScreenXChange and they mean the same thing.
        // Note - This property does not show up if you link FRB .dlls (or nuget)
        // but it does show up if you link source. Sorry for the confusion, but I
        // blame the stupid behavior of EditorBrowsableState
        [EditorBrowsable(EditorBrowsableState.Never)]
        public int XChange => ScreenXChange;

        /// <summary>
        /// Returns the number of pixels on the Y axis
        /// that the cursor has moved since the last
        /// frame. This can be used to move grabbed windows.
        /// </summary>
        public int ScreenYChange
        {
            get
            {
                return ScreenY - LastScreenY;
            }
        }


        // Exists to match the MonoGame cursor so we can unify the code
        // You can use this or ScreenYChange and they mean the same thing.
        // Note - This property does not show up if you link FRB .dlls (or nuget)
        // but it does show up if you link source. Sorry for the confusion, but I
        // blame the stupid behavior of EditorBrowsableState
        [EditorBrowsable(EditorBrowsableState.Never)]
        public int YChange => ScreenYChange;

        [Obsolete("Use ScreenX instead")]
        public int WindowRelativeX
        {
            get
            {
                return (int)mScreenX;
            }
        }

        [Obsolete("Use ScreenY instead")]
        public int WindowRelativeY
        {
            get
            {
                return (int)mScreenY;
            }
        }

        /// <summary>
        /// A shortcut property for WorldXAt(0) providing the world X coordinate of the cursor.
        ///
        /// Safe for orthogonal 2D games or 3D games where gameplay happens at 0 on
        /// the Z axis. For accurate cursor picking in 3D games, use WorldXAt(z).
        /// </summary>
        public float WorldX => WorldXAt(0);

        /// <summary>
        /// A shortcut property for WorldYAt(0) providing the world Y coordinate of the cursor.
        ///
        /// Safe for orthogonal 2D games or 3D games where gameplay happens at 0 on
        /// the Z axis. For accurate cursor picking in 3D games, use WorldYAt(z).
        /// </summary>
        public float WorldY => WorldYAt(0);

        /// <summary>
        /// Shortcut vector to get the X and Y coordinates of the cursor in world space at
        /// a Z index of 0.
        /// 
        /// Safe for orthogonal 2D games or 3D games where you only need to track the cursor
        /// at a Z coordinate of 0. Use WorldXAt(z) and WorldYAt(z) if 3D picking is needed.
        /// </summary>
        public Vector2 WorldPosition => new Vector2(WorldX, WorldY);

        /// <summary>
        /// The window that the cursor was over when the mouse button was pressed.
        /// </summary>
        /// <remarks>
        /// When the mouse button is pushed down, this value is set - either to null 
        /// if the mouse is not over any Windows or to the Window that the mouse is over.  
        /// When the mouse button is released (clicked), this value is set to null.  
        /// This value is useful for clicking on Windows.  Specifically, when the cursor is clicked
        /// on a Button, the WindowPushed value is tested to make sure that it is the same as 
        /// the Window clicked on.  This allows for players to push on a Button but move 
        /// the mouse away and click elsewhere without clicking on the Button originally 
        /// pushed and without accidentally clicking on other Buttons.  This is also used
        /// with ToggleButtons to control when they are pressed and unpressed.
        /// </remarks>
        public IWindow WindowPushed
        {
            get { return mWindowPushed; }
            set
            {
#if DEBUG
                if (value != mWindowPushed && FlatRedBall.Debugging.Debugger.AutomaticDebuggingBehavior.PrintCursorWindowPushed)
                {
                    string whatToPrint = "<NULL>";
                    if (value != null)
                    {
                        whatToPrint = value.ToString() + " " + value.GetType();
                    }

                    FlatRedBall.Debugging.Debugger.CommandLineWrite("Cursor.WindowPushed set to " + whatToPrint);
                }
#endif
                if(value != mWindowPushed)
                {
                    mWindowPushed?.CallRemovedAsPushedWindow();
                    mWindowPushed = value;
                }

            }
        }

        /// <summary>
        /// Assigns the ObjectGrabbed and calculates the relative position of the
        /// grabbed object.  After this method is called, UpdateObjectGrabbedPosition can be called
        /// every frame to position the grabbed object.
        /// </summary>
        public IStaticPositionable ObjectGrabbed
        {
            get { return mObjectGrabbed; }
            set
            {
                mObjectGrabbed = value;

                SetObjectRelativePosition(mObjectGrabbed);
            }
        }

        #endregion

        public Func<bool> CustomIsActive;

        /// <summary>
        /// Delegate which can be used to apply custom
        /// positioning and button state logic.  The argument
        /// is the Cursor instance, and the bool indicates whether
        /// the internal Cursor logic should apply click and push values.
        /// </summary>
        /// <remarks>
        /// If the CustomUpdate delegate assigns Click, Push, and DoubleClick
        /// values internally, then it should return false, indicating that the default
        /// Cursor logic shouldn't change these values.  If the CustomUpdate only modifies the
        /// down states, then the CustomUpdate delegate should return true so that
        /// the Click, Push, and DoubleClick values are automatically managed based on the custom down states.
        /// </remarks>
        public Func<Cursor, bool> CustomUpdate;

        #region Constructor

        /// <summary>
        /// Creates a new Cursor
        /// </summary>
        /// <remarks>
        /// Usually the cursor does not have to be created explicitly.  Calling
        /// the GuiManager's constructor without a cursor reference (default in the
        /// FrbTemplate) creates a cursor automatically.  New cursors should only
        /// be created if multiple cursors are needed, or if you are inheriting
        /// from the Cursor class.  Usually multiple cursors are only needed
        /// in games with multiple cameras.
        /// </remarks>
        /// <param name="cameraToUse">The camera that the cursor will belong to.</param>
        public Cursor(Camera cameraToUse)
        {
#if ANDROID || IOS
            SupportedInputDevices = Gui.InputDevice.TouchScreen;
#else
            SupportedInputDevices = Gui.InputDevice.Mouse;
#endif

            // This was 6 but I think we
            // were getting too many slide
            // detections when we shouldn't
            // have
            ClickNoSlideThreshold = 9;

            devicesControllingCursor.Add(InputManager.Mouse);

            mCamera = cameraToUse;
            //primaryDown = false;

            if (FlatRedBallServices.IsWindowsCursorVisible)
            {
                if (FlatRedBallServices.Game != null)
                {
                    mLastScreenX = InputManager.Mouse.X + FlatRedBallServices.Game.Window.ClientBounds.X;
                    mLastScreenX = InputManager.Mouse.Y + FlatRedBallServices.Game.Window.ClientBounds.Y;
                }
            }
            else
            {
                mLastScreenX = 0;
                mLastScreenY = 0;
            }
        }
        #endregion

        #region Public Methods


        /// <summary>
        /// Returns the world X Velocity of the Cursor at a particular Z.
        /// </summary>
        /// <remarks>
        /// As the cursor can control objects at various Z values, this method returns the X velocity
        /// of the cursor at a paritular Z.  This method assumes that the Camera is unrotated (looking
        /// down the Z axis.
        /// </remarks>
        /// <param name="z">The z value to measure the velocity at.</param>
        /// <returns>The X velocity at the particular Z.</returns>
        public float ActualXVelocityAt(float z)
        {
            // Update November 14, 2011
            // It seems like we should just
            // use WorldXChangeAt and divide
            // by the TimeManager's SecondDifference
            // so that we aren't duplicating code.
            //if (mCamera.Orthogonal)
            //{
            //    float ratioMoved = XVelocity / (2 * mCamera.XEdge);

            //    return ratioMoved * mCamera.OrthogonalWidth + 
            //        mCamera.XVelocity * TimeManager.SecondDifference;
            //}
            //else
            //{
            //    int pixelChange = ScreenX - LastScreenX;
            //    float worldXChange, throwaway;

            //    MathFunctions.ScreenToAbsoluteDistance(pixelChange, 0, out worldXChange, out throwaway, z, SpriteManager.Camera);

            //    return worldXChange / TimeManager.SecondDifference;
            //}
            if (TimeManager.SecondDifference == 0)
            {
                return 0;
            }
            else
            {
                return WorldXChangeAt(z) / TimeManager.SecondDifference;
            }
        }

        /// <summary>
        /// Returns the world X Velocity of the Cursor at a particular Z on a particular Layer.
        /// </summary>
        /// <param name="z">The Z value to measure the velocity at.</param>
        /// <param name="layer">The Layer to use for velocity calculations</param>
        /// <returns>The X velocity at the particular Z and Layer.</returns>
        public float ActualXVelocityAt(float z, Layer layer)
        {
            if (TimeManager.SecondDifference == 0)
            {
                return 0;
            }
            else
            {
                return WorldXChangeAt(z, layer) / TimeManager.SecondDifference;
            }
        }

        /// <summary>
        /// Returns the yVelocity of the cursor at a particular Z.
        /// </summary>
        /// <remarks>
        /// As the cursor can control objects at various Z values, this method returns the Y velocity
        /// of the cursor at a paritular Z.  This method assumes that the Camera is unrotated (looking
        /// down the Z axis.
        /// </remarks>
        /// <param name="z">The z value to measure the velocity at.</param>
        /// <returns>The Y velocity at the particular Z.</returns>
        public float ActualYVelocityAt(float z)
        {
            // See the comment in ActualXVelocityAt
            // for why the following block of code has
            // been commented out.
            //if (mCamera.Orthogonal)
            //{
            //    float ratioMoved = YVelocity / (2 * mCamera.YEdge);

            //    return ratioMoved * mCamera.OrthogonalHeight +
            //        mCamera.YVelocity * TimeManager.SecondDifference;
            //}
            //else
            //{
            //    int pixelChange = ScreenY - LastScreenY;
            //    float worldYChange, throwaway;

            //    MathFunctions.ScreenToAbsoluteDistance(0, pixelChange, out throwaway, out worldYChange, z, SpriteManager.Camera);

            //    return worldYChange / TimeManager.SecondDifference;
            //}
            if (TimeManager.SecondDifference == 0)
            {
                return 0;
            }
            else
            {
                return WorldYChangeAt(z) / TimeManager.SecondDifference;
            }
        }

        public float ActualYVelocityAt(float z, Layer layer)
        {
            if (TimeManager.SecondDifference == 0)
            {
                return 0;
            }
            else
            {
                return WorldYChangeAt(z, layer) / TimeManager.SecondDifference;
            }
        }


        /// <summary>
        /// Clears all input values for this frame. If this cursor is actively updated (default) these
        /// values may get assigned again next frame.
        /// </summary>
        public void ClearInputValues()
        {
            PrimaryClick = false;
            PrimaryDoubleClick = false;
            PrimaryDown = false;
            PrimaryPush = false;
            PrimaryDoublePush = false;

            SecondaryClick = false;
            SecondaryDoubleClick = false;
            SecondaryDown = false;
            SecondaryPush = false;

            MiddleClick = false;
            MiddleDown = false;
            MiddlePush = false;

            WindowPushed = null;
            WindowOver = null;
            WindowGrabbed = null;
        }

        /// <summary>
        /// Modifies the x and y arguments to show the point of the cursor's tip at at the z value.
        /// </summary>
        /// <remarks>
        /// This method assumes that the Camera is unrotated (looking
        /// down the Z axis).
        /// </remarks>
        /// <param name="x">The x value to change.</param>
        /// <param name="y">The y value to change.</param>
        /// <param name="absoluteZ">The z value to use.</param>
        [Obsolete("Use WorldXAt and WorldYAt instead of this function.  This function will be removed at some point in the future")]
        public void GetCursorPosition(out float x, out float y, float absoluteZ)
        {
            x = WorldXAt(absoluteZ);
            y = WorldYAt(absoluteZ);
        }

        /// <summary>
        /// Modifies the x and y arguments to show the position that the grabbed Sprite should be at.
        /// </summary>
        /// <remarks>
        /// This method assumes that the camera's currentFollowingStyle is Camera.LOOKINGSTYLE.DOWNZ.  The x and y
        /// arguments will represent the location of the cursor's tip and adds the relative Sprite positions.
        /// 
        /// <seealso cref="FlatRedBall.Gui.Cursor.SetObjectRelativePosition"/>
        /// </remarks>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="absolute"></param>
        public void GetCursorPositionForSprite(ref float x, ref float y, float absoluteZ)
        {
            x = WorldXAt(absoluteZ);
            y = WorldYAt(absoluteZ);

            x += ObjectGrabbedRelativeX;
            y += ObjectGrabbedRelativeY;
        }

        public void GetCursorPositionForSprite(ref float x, ref float y, float absoluteZ, Layer layer, bool useRelativePosition = true)
        {
            x = WorldXAt(absoluteZ, layer);
            y = WorldYAt(absoluteZ, layer);

            if (useRelativePosition)
            {
                x += ObjectGrabbedRelativeX;
                y += ObjectGrabbedRelativeY;
            }

        }

        /// <summary>
        /// Gets the cursor's position and sets the argument positionedObject's
        /// x and y values to the cursor's position at the argument Z value.
        /// </summary>
        /// <remarks>
        /// This is a simple way to set the position of an object
        /// to the cursor's position.
        /// </remarks>
        /// <param name="positionedObject">Reference to the positioned object.</param>
        /// <param name="absoluteZ">The Z value at which the position should be set.</param>
        public void GetCursorPosition(PositionedObject positionedObject, float absoluteZ)
        {
            positionedObject.X = WorldXAt(0);
            positionedObject.Y = WorldYAt(0);
        }

        public Ray GetLastRay()
        {
            return mLastRay;
        }

        public Ray GetRay()
        {
#if DEBUG
            if (mCamera == null)
            {
                throw new Exception(
                    "The Cursor's Camera is null.  This is not valid.  If you get this error please report on the FlatRedBall forums and if possible post a project for us to look at");
            }
#endif

            return MathFunctions.GetRay((int)mScreenX, (int)mScreenY, 1, this.mCamera);
        }

        public Ray GetRay(Layer layer)
        {
            if (layer == null)
            {
                return MathFunctions.GetRay((int)mScreenX, (int)mScreenY, 1, this.mCamera, null);
            }
            else
            {
                return MathFunctions.GetRay((int)mScreenX, (int)mScreenY, 1, this.mCamera, layer.LayerCameraSettings);
            }
        }

        /// <summary>
        /// Returns the Sprite the cursor is over giving preference to the closest Sprite to the Camera.
        /// </summary>
        /// <remarks>
        /// This method will not return inactive Sprites (Sprites with the .active variable set to false).  To consider inactives as well
        /// call the GetSpriteOver(SpriteArray, bool) overload.
        /// </remarks>
        /// <param name="spriteArray">The SpriteArray to search through.</param>
        /// <returns>The Sprite that is found, or null if the cursor is not over any Sprites.</returns>
        public T GetSpriteOver<T>(IList<T> spriteArray) where T : ICursorSelectable, IAttachable
        {
            return GetSpriteOver(spriteArray, false);
        }

        public T GetSpriteOver<T>(IList<T> spriteArray, bool considerInactives) where T : ICursorSelectable, IAttachable
        {
            return GetSpriteOver(spriteArray, considerInactives, false);
        }

        /// <summary>
        /// Returns the Sprite the cursor is over giving preference to the closest Sprite to the Camera.
        /// </summary>
        /// <param name="spriteArray">The SpriteArray to search through.</param>
        /// <param name="considerInactives">Whether inactive Sprites (Sprites with the .active variable set to false) are considered.</param>
        /// <param name="skipFirst"></param>
        /// <returns>The Sprite that is found, or null if the cursor is not over any Sprites.</returns>
        public T GetSpriteOver<T>(IList<T> spriteArray, bool considerInactives, bool skipFirst) where T : ICursorSelectable, IAttachable
        {
            double closestDistance = -1;
            T closestSprite = default(T);

            double secondClosestDistance = -1;
            T secondClosest = default(T);

            Vector3 newVector;

            float distance = 0;

            foreach (T s in spriteArray)
            {
                bool isOver = (s.CursorSelectable || considerInactives);

                if (s is Text)
                    isOver &= IsOn3D(s as Text, out newVector);
                else
                    isOver &= IsOn3D(s, out newVector);


                if (isOver)
                {
                    distance = (mCamera.Position - newVector).Length();

                    if (distance < closestDistance || closestDistance < 0)
                    {
                        closestDistance = distance;
                        closestSprite = s;
                    }
                    else if (distance < secondClosestDistance || secondClosestDistance < 0)
                    {
                        secondClosestDistance = distance;
                        secondClosest = s;
                    }
                }
            }

            if (skipFirst)
            {
                return secondClosest;
            }
            else
            {
                return closestSprite;
            }
        }

        /// <summary>
        /// Returns the Sprite the cursor is over giving preference to the closest Sprite to the Camera.
        /// </summary>
        /// <param name="spriteArrayArray">The SpriteArrayArray to search through.</param>
        /// <returns>The Sprite that is found, or null if the cursor is not over any Sprites.</returns>
        public Sprite GetSpriteOver(IList<SpriteList> spriteArrayArray)
        {
            Sprite spriteOver = null;

            Sprite closestSprite = null;

            float closestDistance = float.NaN;
            float distance = 0;
            for (int i = 0; i < spriteArrayArray.Count; i++)
            {
                spriteOver = GetSpriteOver(spriteArrayArray[i]);
                if (spriteOver != null)
                {
                    distance = ((Vector3)(spriteOver.Position - mCamera.Position)).Length();

                    if (float.IsNaN(closestDistance) || distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestSprite = spriteOver;
                    }
                }
            }
            return closestSprite;
        }

        /// <summary>
        /// Returns the Sprite the cursor is over giving preference to the closest Sprite to the Camera.
        /// </summary>
        /// <param name="spriteLayer">The SpriteLayer to search through.</param>
        /// <returns>The Sprite that is found, or null if the Cursor is not over any Sprites.</returns>
        public Sprite GetSpriteOver(Layer spriteLayer)
        {
            return GetSpriteOver(spriteLayer.Sprites);
        }

        /// <summary>
        /// Grabs a Window with the Cursor.
        /// </summary>
        /// <remarks>
        /// The windowGrabbed reference will automatically be set to null when the primary button is 
        /// released (clicked) by the GuiManager.Control method.
        /// <seealso cref="FlatRedBall.Gui.GuiManager.Control"/>
        /// </remarks>
        /// <param name="windowToGrab">The Window to grab.</param>
        public void GrabWindow(IWindow windowToGrab)
        {
            this.WindowGrabbed = windowToGrab;

            float cursorX = WorldXAt(0);
            float cursorY = WorldYAt(0);

            if (WindowGrabbed != null)
            {
                mGrabbedWindowRelativeX = (float)(windowToGrab.WorldUnitX - cursorX);
                mGrabbedWindowRelativeY = (float)(windowToGrab.WorldUnitY - cursorY);

            }
        }

        #region ====IsOn Methods====

        #region XML Docs
        /// <summary>
        /// Determines whether the cursor is on a Sprite, but only considers rotation on the z axis (RotationZ).
        /// </summary>
        /// <remarks>
        /// This method will not consider whether spriteToTest is rotated on the x or y axes, and assumes
        /// that the camera is looking down the Z axis (the Camera is unrotated).  Cursor.IsOn3D
        /// works properly for any rotation..
        /// 
        /// <para>This method will not select Sprites which are closer that the camera's nearClipPlane or
        /// further than the camera's farClipPlane.</para>
        /// </remarks>
        /// <param name="spriteToTest">The Sprite to test if the cursor is over.</param>
        /// <returns>Whether the cursor is on the Sprite.</returns>
        #endregion
        public bool IsOn(Sprite spriteToTest)
        {
            if (spriteToTest == null)
                return false;

            bool relativeToCamera = SpriteManager.IsRelativeToCamera(spriteToTest);


            float cursorX = 0;
            float cursorY = 0;
            float distanceFromCamera;

            if (relativeToCamera)
                distanceFromCamera = spriteToTest.Z;
            else
            {
                distanceFromCamera = mCamera.Z - spriteToTest.Z;
            }

            // check the near and far clipping first
            if (distanceFromCamera < mCamera.NearClipPlane ||
                distanceFromCamera > mCamera.FarClipPlane)
                return false;

            if (relativeToCamera)
            {
                cursorX = WorldXAt(spriteToTest.Z) + mCamera.X;
                cursorY = WorldYAt(spriteToTest.Z) + mCamera.Y;
            }
            else
            {
                cursorX = WorldXAt(spriteToTest.Z);
                cursorY = WorldYAt(spriteToTest.Z);
            }

            if (spriteToTest.RotationZ != 0.0f)
            {
                MathFunctions.RotatePointAroundPoint(spriteToTest.X, spriteToTest.Y, ref cursorX, ref cursorY, -spriteToTest.RotationZ);
            }

            return cursorX > spriteToTest.X - spriteToTest.ScaleX && cursorX < spriteToTest.X + spriteToTest.ScaleX &&
                cursorY > spriteToTest.Y - spriteToTest.ScaleY && cursorY < spriteToTest.Y + spriteToTest.ScaleY;
        }

        /// <summary>
        /// Determines whether the cursor is over a textObject.
        /// </summary>
        /// <remarks>
        /// This method will not consider whether the Text is rotated on the x or y axes, and assumes
        /// that the cursor is looking down the Z axis(the camera.LookStyle is DOWNZ).  
        /// 
        /// <para>This method will not select Texts which are closer that the camera's nearClipPlane or
        /// further than the camera's farClipPlane.</para>
        /// 
        /// Currently, the method treats the text object as one rectangle, so the width of the Text will be equal
        /// to the widest line.  For example:
        /// 
        /// <code>
        /// // if the text were centered, the collidable area would be as follows
        /// +-------------------------+
        /// |       The outline       |
        /// |represents the collidable|
        /// |    area of the Text.    |
        /// |  Notice that the border |
        /// |    extends to include   |
        /// |    the longest line.    |
        /// +-------------------------+
        /// 
        /// // if the text were left aligned, the area would still be the same:
        /// +-------------------------+
        /// |The outline              |
        /// |represents the collidable|
        /// |area of the Text.        |
        /// |Notice that the border   |
        /// |extends to include       |
        /// |the longest line.        |
        /// +-------------------------+
        /// </code>
        /// </remarks>
        /// <param name="textObject">The TextObject to test if the cursor is over.</param>
        /// <returns>Whether the cursor is on the TextObject.</returns>
        public bool IsOn(Text textObject)
        {
            if (textObject == null)
                return false;
            float distanceFromCamera = (mCamera.Z) - textObject.Z;
            if (distanceFromCamera < mCamera.NearClipPlane ||
                distanceFromCamera > mCamera.FarClipPlane)
                return false;

            float cursorX = 0;
            float cursorY = 0;


            cursorX = WorldXAt(textObject.Position.Z);
            cursorY = WorldYAt(textObject.Position.Z);

            if (textObject.RotationZ != 0.0f)
            {
                MathFunctions.RotatePointAroundPoint(textObject.X, textObject.Y, ref cursorX, ref cursorY, -textObject.RotationZ);
            }


            float textScaleX = textObject.ScaleX;
            float textScaleY = textObject.ScaleY;

            float horizontalCenter = textObject.HorizontalCenter;
            float verticalCenter = textObject.VerticalCenter;


            return cursorX > horizontalCenter - textScaleX && cursorX < horizontalCenter + textScaleX &&
                cursorY > verticalCenter - textScaleY && cursorY < verticalCenter + textScaleY;
        }

        public bool IsOn(Polygon polygon)
        {
            if (mUsingWindowsCursor)
            {
                return polygon.IsPointInside(WorldXAt(polygon.Z), WorldYAt(polygon.Z));
            }
            else
            {
                throw new System.NotImplementedException();
            }
        }

        public bool IsOn(Circle circle)
        {
            if (mUsingWindowsCursor)
            {
                return circle.IsPointInside(WorldXAt(circle.Z), WorldYAt(circle.Z));
            }
            else
            {
                throw new System.NotImplementedException();
            }
        }


        public bool IsOn(Layer layer)
        {
            if (layer == null || layer.LayerCameraSettings == null)
            {
                return true;
            }

            else if (layer.LayerCameraSettings.BottomDestination > -1 &&
                layer.LayerCameraSettings.TopDestination > -1 &&
                layer.LayerCameraSettings.LeftDestination > -1 &&
                layer.LayerCameraSettings.RightDestination > -1)
            {
                return this.ScreenX >= layer.LayerCameraSettings.LeftDestination &&
                    this.ScreenX < layer.LayerCameraSettings.RightDestination &&
                    this.ScreenY >= layer.LayerCameraSettings.TopDestination &&
                    this.ScreenY < layer.LayerCameraSettings.BottomDestination;
            }

            return true;



        }

        public bool IsOnFloatingChildren(IWindow window)
        {
            int i;

            for (i = 0; i < window.FloatingChildren.Count; i++)
            {
                IWindow floatingChild = window.FloatingChildren[i];
                if (IsOnWindowOrFloatingChildren(floatingChild))
                    return true;
            }

            for (i = 0; i < window.Children.Count; i++)
            {
                IWindow child = window.Children[i];
                if (IsOnFloatingChildren(child))
                    return true;
            }

            return false;
        }


        public bool IsOnWindowOrFloatingChildren(IWindow windowToTest)
        {
            if (windowToTest.Visible && windowToTest.HasCursorOver(this))
                return true;

            int i;

            if (windowToTest.FloatingChildren != null)
            {
                for (i = 0; i < windowToTest.FloatingChildren.Count; i++)
                {
                    IWindow w = windowToTest.FloatingChildren[i];
                    if (IsOnWindowOrFloatingChildren(w))
                        return true;
                }

                if (IsOnFloatingChildren(windowToTest))
                    return true;
            }
            return false;
        }


        #region XML Docs
        /// <summary>
        /// Returns whether the cursor is over the argument SpriteFrame.
        /// </summary>
        /// <param name="sf">Reference to the SpriteFrame to test.</param>
        /// <returns>Whether the cursor is over the argument SpriteFrame.</returns>
        #endregion
        public bool IsOn<T>(T sf) where T : IPositionable, IRotatable, IScalable
        {
            if (sf == null)
                return false;

            float cursorX = 0;
            float cursorY = 0;

            float distanceFromCamera = (mCamera.Z) - sf.Z;

            if (distanceFromCamera < mCamera.NearClipPlane ||
                distanceFromCamera > mCamera.FarClipPlane)
                return false;

            // I think WorldXAt and WorldYAt will be sufficient instead of usin this complex logic.
            //if (mCamera.Orthogonal == false)
            //{
            //    cursorX = (mCamera.X) + (distanceFromCamera / 100.0f) * (XForUI);
            //    cursorY = (mCamera.Y) + (distanceFromCamera / 100.0f) * (YForUI);

            //}
            //else
            //{
            //    // orthoWidth measures left to right side of the screen, but we want orthoScl
            //    cursorX = mCamera.X + mCamera.OrthogonalWidth * (XForUI) / (2 * mCamera.XEdge);
            //    cursorY = mCamera.Y + mCamera.OrthogonalHeight * (YForUI) / (2 * mCamera.YEdge);
            //}
            cursorX = WorldXAt(sf.Z);
            cursorY = WorldYAt(sf.Z);




            if (sf.RotationZ != 0.0f)
            {
                MathFunctions.RotatePointAroundPoint(sf.X, sf.Y, ref cursorX, ref cursorY, -sf.RotationZ);
            }


            return cursorX < sf.X + sf.ScaleX &&
                        cursorX > sf.X - sf.ScaleX &&
                        cursorY < sf.Y + sf.ScaleY &&
                        cursorY > sf.Y - sf.ScaleY;
        }


        public bool IsOn3D(Scene scene, Layer layer)
        {
            return IsOn3D(scene, layer, false);
        }

        public bool IsOn3D(Scene scene, Layer layer, bool ignoreInvisible)
        {
            for (int i = scene.Sprites.Count - 1; i > -1; i--)
            {
                if ((!ignoreInvisible || scene.Sprites[i].AbsoluteVisible) && IsOn3D(scene.Sprites[i], layer))
                {
                    return true;
                }
            }

            for (int i = scene.Texts.Count - 1; i > -1; i--)
            {
                if ((!ignoreInvisible || scene.Texts[i].AbsoluteVisible) && IsOn3D(scene.Texts[i], layer))
                {
                    return true;
                }
            }

            for (int i = scene.SpriteFrames.Count - 1; i > -1; i--)
            {
                if ((!ignoreInvisible || scene.SpriteFrames[i].AbsoluteVisible) && IsOn3D(scene.SpriteFrames[i], layer))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Determines whether the Cursor is on a Sprite.
        /// </summary>
        /// <remarks>
        /// This is the "full featured" version of the IsOn method, considering rotation on all axes for
        /// both the spriteToTest and the camera.  All lookingStyles are considered and will return accurate results.
        /// This method is slower than the IsOn method, and if Sprites are facing the camera under its
        /// default orientation, the IsOn method should be used.
        /// 
        /// <para>This method will not select Sprites if the cursor is over them on a point
        /// that is closer that the camera's nearClipPlane or further than the camera's 
        /// farClipPlane.</para>
        /// 
        /// </remarks>
        /// <param name="spriteToTest">The Sprite to test if the Cursor is over.</param>
        /// <returns>Whether the Cursor is over the Sprite.</returns>
        public bool IsOn3D(Sprite spriteToTest)
        {
            return IsOn3D(spriteToTest, ref vector3);
        }


        public bool IsOn3D(Text textToTest)
        {
            return IsOn3D(textToTest, null, out vector3);
        }

        public bool IsOn3D(Text textToTest, Layer layer)
        {
            return IsOn3D(textToTest, layer, out vector3);
        }

        public bool IsOn3D(Text textToTest, out Vector3 intersectionPoint)
        {
            return IsOn3D(textToTest, null, out intersectionPoint);
        }

        public bool IsOn3D(Text textToTest, Layer layer, out Vector3 intersectionPoint)
        {
            Vector3 oldPosition = textToTest.Position;

            #region Adjust based off of HorizontalAlignment

            switch (textToTest.HorizontalAlignment)
            {
                case HorizontalAlignment.Center:

                    // do nothing
                    break;

                case HorizontalAlignment.Left:
                    {
                        float amountToMove = -textToTest.ScaleX;

                        textToTest.Position -=
                            new Vector3(textToTest.RotationMatrix.M11,
                                        textToTest.RotationMatrix.M12,
                                        textToTest.RotationMatrix.M13) *
                            amountToMove;

                        break;
                    }
                case HorizontalAlignment.Right:
                    {
                        float amountToMove = textToTest.ScaleX;

                        textToTest.Position -=
                            new Vector3(textToTest.RotationMatrix.M11,
                                        textToTest.RotationMatrix.M12,
                                        textToTest.RotationMatrix.M13) *
                            amountToMove;

                        break;

                    }
            }

            #endregion

            #region Adjust based off of VerticalAlignment

            switch (textToTest.VerticalAlignment)
            {
                case VerticalAlignment.Center:
                    // do nothing
                    break;
                case VerticalAlignment.Top:
                    {
                        float amountToMove = textToTest.ScaleY;

                        textToTest.Position -=
                            new Vector3(textToTest.RotationMatrix.M21,
                                textToTest.RotationMatrix.M22,
                                textToTest.RotationMatrix.M23) * amountToMove;
                        break;
                    }
                case VerticalAlignment.Bottom:
                    {
                        float amountToMove = -textToTest.ScaleY;

                        textToTest.Position -=
                            new Vector3(textToTest.RotationMatrix.M21,
                                textToTest.RotationMatrix.M22,
                                textToTest.RotationMatrix.M23) * amountToMove;
                        break;
                    }

            }

            #endregion

            bool returnValue = IsOn3D(textToTest, false, layer, out intersectionPoint);

            textToTest.Position = oldPosition;

            return returnValue;

        }

        public bool IsOn3D(AxisAlignedCube cubeToTest)
        {
            Ray ray = this.GetRay();
            // This can give improper results if not normalized!
            ray.Direction.Normalize();
            return ray.Intersects(cubeToTest.AsBoundingBox()).HasValue;
        }

        public bool IsOn3D(Sphere sphere)
        {
            return IsOn3D(sphere, (Layer)null);
        }

        public bool IsOn3D(Sphere sphere, Layer layer)
        {
            Ray ray = this.GetRay(layer);

            BoundingSphere boundingSphere = sphere.AsBoundingSphere();
            // This can give improper results if not normalized!
            ray.Direction.Normalize();

            return ray.Intersects(boundingSphere).HasValue;

        }


        public bool IsOn3D<T>(T objectToTest) where T : IPositionable, IRotatable, IReadOnlyScalable
        {
            return IsOn3D(objectToTest, false, null, out vector3);
        }

        public bool IsOn3D<T>(T objectToTest, Layer layer) where T : IPositionable, IRotatable, IReadOnlyScalable
        {
            return IsOn3D(objectToTest, false, layer, out vector3);
        }

        #region XML Docs
        /// <summary>
        /// Determines whether the Cursor is on a Sprite and stores the intersection point in a Vector3.
        /// </summary>
        /// <remarks>		
        /// This is the "full featured" version of the IsOn method, considering rotation on all axes for
        /// both the spriteToTest and the camera.  All lookingStyles are considered and will return accurate results.
        /// This method is slower than the IsOn method, and if Sprites are facing the camera under its
        /// default orientation, the IsOn method should be used.
        /// 
        /// This method also modifies a Vector3 which marks the intersection point on the Sprite.
        /// 
        /// <para>This method will not select Sprites if the cursor is over them on a point
        /// that is closer that the camera's nearClipPlane or further than the camera's 
        /// farClipPlane.</para>
        /// </remarks>
        /// <param name="spriteToTest">The Sprite to test if the Cursor is over.</param>
        /// <param name="intersectionPoint">The point where the cursor touches the Sprite.</param>
        /// <returns>Whether the Cursor is on the Sprite.</returns>
        #endregion
        public bool IsOn3D(Sprite spriteToTest, ref Vector3 intersectionPoint)
        {
            return IsOn3D(spriteToTest, SpriteManager.IsRelativeToCamera(spriteToTest), null, out intersectionPoint);
        }



        public bool IsOn3D<T>(T spriteToTest, out Vector3 intersectionPoint) where T : IPositionable, IRotatable, IReadOnlyScalable
        {
            return IsOn3D(spriteToTest, false, null, out intersectionPoint);
        }


        public bool IsOn3D<T>(T spriteToTest, bool relativeToCamera, Layer layer, out Vector3 intersectionPoint) where T : IPositionable, IRotatable, IReadOnlyScalable
        {
            intersectionPoint = Vector3.Zero;
            if (spriteToTest == null)
                return false;

            return MathFunctions.IsOn3D<T>(
                spriteToTest, relativeToCamera, this.GetRay(layer),
                mCamera, out intersectionPoint);
        }

        public bool IsOn3D(Polygon polygon)
        {
            return IsOn3D(polygon, null);
        }

        public bool IsOn3D(Polygon polygon, Layer layer)
        {
            Ray ray = GetRay(layer);
            Matrix inverseRotation = polygon.RotationMatrix;

            Matrix.Invert(ref inverseRotation, out inverseRotation);

            Plane plane = new Plane(polygon.Position, polygon.Position + polygon.RotationMatrix.Up,
                polygon.Position + polygon.RotationMatrix.Right);

            float? result = ray.Intersects(plane);

            if (!result.HasValue)
            {
                return false;
            }

            Vector3 intersection = ray.Position + ray.Direction * result.Value;


            return polygon.IsPointInside(ref intersection);
        }

        public bool IsOn3D(Circle circle)
        {
            return IsOn3D(circle, null);
        }

        public bool IsOn3D(Circle circle, Layer layer)
        {
            Ray ray = GetRay(layer);
            Matrix inverseRotation = circle.RotationMatrix;

            Matrix.Invert(ref inverseRotation, out inverseRotation);

            Plane plane = new Plane(circle.Position, circle.Position + circle.RotationMatrix.Up,
                circle.Position + circle.RotationMatrix.Right);

            float? result = ray.Intersects(plane);

            if (!result.HasValue)
            {
                return false;
            }

            Vector3 intersection = ray.Position + ray.Direction * result.Value;

            return circle.IsPointInside(ref intersection);
        }

        public bool IsOn3D(ShapeCollection shapeCollection, Layer layer)
        {
            for (int i = 0; i < shapeCollection.AxisAlignedCubes.Count; i++)
            {
                if (IsOn3D(shapeCollection.AxisAlignedCubes[i], layer))
                {
                    return true;
                }
            }

            for (int i = 0; i < shapeCollection.AxisAlignedRectangles.Count; i++)
            {
                if (IsOn3D(shapeCollection.AxisAlignedRectangles[i], layer))
                {
                    return true;
                }
            }

            //for (int i = 0; i < shapeCollection.Capsule2Ds.Count; i++)
            //{
            //    if (IsOn3D(shapeCollection.Capsule2Ds[i], layer))
            //    {
            //        return true;
            //    }
            //}

            for (int i = 0; i < shapeCollection.Circles.Count; i++)
            {
                if (IsOn3D(shapeCollection.Circles[i], layer))
                {
                    return true;
                }
            }

            //for (int i = 0; i < shapeCollection.Lines.Count; i++)
            //{
            //    if (IsOn3D(shapeCollection.Lines[i], layer))
            //    {
            //        return true;
            //    }
            //}

            for (int i = 0; i < shapeCollection.Polygons.Count; i++)
            {
                if (IsOn3D(shapeCollection.Polygons[i], layer))
                {
                    return true;
                }
            }

            for (int i = 0; i < shapeCollection.Spheres.Count; i++)
            {
                if (IsOn3D(shapeCollection.Spheres[i], layer))
                {
                    return true;
                }
            }

            return false;
        }

        #endregion


        /// <summary>
        /// Determines whether the cursor is currently in the active Control which owns the application.
        /// </summary>
        /// <returns>Whether the cursor is currently in the active Control which owns the application.</returns>
        public bool IsInWindow()
        {
            return InputManager.Mouse.IsInGameWindow();
        }




        public void ResetCursor()
        {
            if (FlatRedBallServices.mIsInitialized)
            {
                UpdatePositionToMouse();
            }
        }

        /// <summary>
        /// Sets the cursor to be controlled by the joystick rather than the mouse.
        /// </summary>
        /// <param name="gamePad">Refernce to the Xbox360GamePad that will control the cursor.</param>
        [Obsolete("Use SetControllingGamepad")]
        public void SetJoystickControl(Xbox360GamePad gamePad)
        {
            devicesControllingCursor.Clear();
            devicesControllingCursor.Add(gamePad);
        }

        /// <summary>
        /// Clears all input devices and sets the cursor to be controlled by the gamepad.
        /// </summary>
        /// <param name="gamePad"></param>
        public void SetControllingGamepad(Xbox360GamePad gamePad)
        {
            devicesControllingCursor.Clear();
            devicesControllingCursor.Add(gamePad);
        }

        /// <summary>
        /// Clears all input devices and sets the cursor to be controlled by all argument gamepads. This allows multiple
        /// gamepads to control a single cursor.
        /// </summary>
        /// <param name="gamePads">The gamepads to control this.</param>
        public void SetControllingGamepads(IEnumerable<Xbox360GamePad> gamePads)
        {
            devicesControllingCursor.Clear();
            devicesControllingCursor.AddRange(gamePads);
        }

        public void SetMouseControl()
        {
            devicesControllingCursor.Clear();
            devicesControllingCursor.Add(InputManager.Mouse);
        }

        /// <summary>
        /// Tells the Cursor to store the relative position of the Sprite to the Cursor's tip.
        /// </summary>
        /// <remarks>
        /// The relative Sprite position is used in the GetCursorPositionForSprite method.  Relative values
        /// keep objects from "snapping" to the center of the cursor when grabbed.
        /// </remarks>
        /// <param name="objectSetTo">The PositionedObject that the relative values should be set to.</param>
        public void SetObjectRelativePosition(IStaticPositionable objectSetTo)
        {
            if (objectSetTo == null) return;

            bool relativeToCamera = objectSetTo is Sprite && SpriteManager.IsRelativeToCamera(objectSetTo as Sprite);

            float cursorX = 0;
            float cursorY = 0;

            if (relativeToCamera)
            {
                this.GetCursorPosition(out cursorX, out cursorY, objectSetTo.Z + mCamera.Z);
            }
            else
            {
                this.GetCursorPosition(out cursorX, out cursorY, objectSetTo.Z);
            }
            ObjectGrabbedRelativeX = objectSetTo.X - cursorX;
            ObjectGrabbedRelativeY = objectSetTo.Y - cursorY;

            if (relativeToCamera)
            {
                ObjectGrabbedRelativeX += mCamera.X;
                ObjectGrabbedRelativeY += mCamera.Y;
            }
        }


        public override string ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("X: ").Append(ScreenX).Append("\n");
            sb.Append("Y: ").Append(ScreenY).Append("\n");
            //sb.Append("X Velocity: ").Append(XVelocity).Append("\n");
            //sb.Append("Y Velocity: ").Append(YVelocity).Append("\n");
            sb.Append("PrimaryDown: ").Append(PrimaryDown).Append("\n");
            sb.Append("PrimaryPush: ").Append(PrimaryPush).Append("\n");
            sb.Append("\nWindow Grabbed: ").Append(WindowGrabbed);



            return sb.ToString();
        }


        public void UpdateObjectGrabbedPosition()
        {
            UpdateObjectGrabbedPosition(null);
        }

        public void UpdateObjectGrabbedPosition(int xOffset, int yOffset, bool useCursorRelativePosition)
        {
            UpdateObjectGrabbedPosition(null, xOffset, yOffset, useCursorRelativePosition);
        }

        public void UpdateObjectGrabbedPosition(Layer layer, int xOffset = 0, int yOffset = 0, bool useCursorRelativePosition = true)
        {
            if (mObjectGrabbed != null)
            {
                float outX = 0;
                float outY = 0;

                // November 22, 2011
                // I don't think we support this anymore.
                // Glue has kinda solved this with attachments
                // to the camera.
                //if (SpriteManager.IsRelativeToCamera((Sprite)mObjectGrabbed))
                //{

                //    GetCursorPositionForSprite(
                //        ref outX, ref outY, mObjectGrabbed.Z + mCamera.Z, layer);

                //    mObjectGrabbed.X = outX - mCamera.X;
                //    mObjectGrabbed.Y = outY - mCamera.Y;
                //}
                //else
                {

                    GetCursorPositionForSprite(ref outX, ref outY, mObjectGrabbed.Z, layer, useCursorRelativePosition);
                    mObjectGrabbed.X = outX + xOffset;
                    mObjectGrabbed.Y = outY + yOffset;
                }
            }

        }
        #endregion

        #region Positions (WorldX/Y At)


        /// <summary>
        /// Returns the X world coordiante of the cursor at the argument Z position.
        /// This method requires a Z value to properly work with perspective cameras.
        /// This method assumes an unrotated camera.
        /// 
        /// For 2D games, the Z value is usually always 0.
        /// </summary>
        /// <param name="zPosition">The world Z to check at.</param>
        /// <returns>The world X coordiante.</returns>
        public float WorldXAt(float zPosition)
        {
            var camera = Camera;
            return camera.WorldXAt(this.ScreenX, zPosition, camera.Orthogonal, camera.OrthogonalWidth);
        }

        public float WorldXAt(float zPosition, Layer layer)
        {
            return this.Camera.WorldXAt(this.ScreenX, zPosition, layer);
        }

        public float WorldXAt(float zPosition, Camera camera, bool orthogonal, float orthogonalWidth)
        {

            return camera.WorldXAt(zPosition, orthogonal, orthogonalWidth, ScreenX);
        }

        public static float WorldXAt(float zPosition, Camera camera, bool orthogonal, float orthogonalWidth, float screenRelativeX)
        {
            return camera.WorldXAt(zPosition, orthogonal, orthogonalWidth, screenRelativeX);
        }
        
        /// <summary>
        /// Returns the Y world coordiante of the cursor at the argument Z position.
        /// This method requires a Z value to properly work with perspective cameras.
        /// This method assumes an unrotated camera.
        /// 
        /// For 2D games, the Z value is usually always 0.
        /// </summary>
        /// <param name="zPosition">The world Z to check at.</param>
        /// <returns>The world Y coordiante.</returns>
        public float WorldYAt(float zPosition)
        {
            return mCamera.WorldYAt(ScreenY, zPosition, mCamera.Orthogonal, mCamera.OrthogonalHeight);
        }

        public float WorldYAt(float zPosition, Layer layer)
        {
            return mCamera.WorldYAt(ScreenY, zPosition, layer);
        }

        public float WorldYAt(float zPosition, Camera camera, bool orthogonal, float orthogonalHeight)
        {
            return mCamera.WorldYAt(zPosition, orthogonal, orthogonalHeight, ScreenY);
        }

        public static float WorldYAt(float zPosition, Camera camera, bool orthogonal, float orthogonalHeight, float screenRelativeY)
        {
            return camera.WorldYAt(zPosition, orthogonal, orthogonalHeight, screenRelativeY);
        }


        /// <summary>
        /// Returns the change in X world coordinate of the cursor at the argument Z position since last frame
        /// </summary>
        /// <param name="zPosition">The Z position. For 2D games, pass 0.</param>
        /// <returns>The change in world coordiantes since last frame.</returns>
        public float WorldXChangeAt(float zPosition)
        {
            return WorldXChangeAt(zPosition, this.Camera.Orthogonal, this.Camera.OrthogonalWidth);
        }

        public float WorldXChangeAt(float zPosition, Layer layer)
        {
            if (layer == null || layer.LayerCameraSettings == null)
            {
                return WorldXChangeAt(zPosition, SpriteManager.Camera.Orthogonal, SpriteManager.Camera.OrthogonalWidth);
            }
            else
            {
                LayerCameraSettings lcs = layer.LayerCameraSettings;

                float returnValue = WorldXChangeAt(zPosition, lcs.Orthogonal, lcs.OrthogonalWidth);

				if ((lcs.LeftDestination != -1 && lcs.RightDestination != -1) && 
					(lcs.LeftDestination != lcs.RightDestination) &&
					(lcs.LeftDestination != Camera.LeftDestination || lcs.RightDestination != Camera.RightDestination))
                {
                    float amplificationValue = (Camera.RightDestination - Camera.LeftDestination) / (float)(lcs.RightDestination - lcs.LeftDestination);

                    returnValue *= amplificationValue;
                }


                return returnValue;
            }
        }

        float WorldXChangeAt(float zPosition, bool orthogonal, float orthogonalWidth)
        {
#if ANDROID || IOS
            if(PrimaryPush || InputManager.TouchScreen.NumberOfTouchesChanged)
            {
                return 0;
            }
#endif


            float ratio = (float)(mScreenX - mLastScreenX) /

                // Victor Chelaru
                // February 1, 2014
                // This used to use the
                // resolution width, but what
                // if the entire Camera is only
                // rendering to a portion of the
                // screen.  Then we should use the
                // Camera's DestinationRectangle rather
                // than the full Screen's resolution:
                //(float)FlatRedBallServices.GraphicsOptions.ResolutionWidth;
                (float)Camera.DestinationRectangle.Width;

            if (orthogonal)
            {
                return ratio * orthogonalWidth;
            }
            else
            {
                // This simplifies the code and no longer uses the old XVelocity value which I want to phase out
                return this.Camera.RelativeXEdgeAt(zPosition) * 2 * ratio;
            }
            //if (orthogonal)
            //{
            //    float horizontalPercentage = XVelocity / (GuiManager.Camera.XEdge * 2);// / (float)SpriteManager.Camera.DestinationRectangle.Height;

            //    return horizontalPercentage * orthogonalWidth;
            //}
            //else
            //{
            //    return MathFunctions.ForwardVector3.Z * (zPosition - this.Camera.Z) * this.XVelocity / 100.0f;
            //}
        }


        /// <summary>
        /// Returns the change in Y coordiante of the cursor at the argument Z position since last frame. Since this is in world coordiantes, positive Y points up.
        /// </summary>
        /// <param name="zPosition">The Z position. For 2D games, pass 0.</param>
        /// <returns>The change in world cooriantes since last frame.</returns>
        public float WorldYChangeAt(float zPosition)
        {
            return WorldYChangeAt(zPosition, this.Camera.Orthogonal, this.Camera.OrthogonalHeight);

        }

        public float WorldYChangeAt(float zPosition, Layer layer)
        {
            if (layer == null || layer.LayerCameraSettings == null)
            {
                return WorldYChangeAt(zPosition, SpriteManager.Camera.Orthogonal, SpriteManager.Camera.OrthogonalHeight);
            }
            else
            {
                LayerCameraSettings lcs = layer.LayerCameraSettings;

                float returnValue = WorldYChangeAt(zPosition, lcs.Orthogonal, lcs.OrthogonalHeight);

                if (lcs.TopDestination != Camera.TopDestination ||
                    lcs.BottomDestination != Camera.BottomDestination)
                {
                    float amplificationValue = 1;

                    if (lcs.BottomDestination != -1 && lcs.TopDestination != -1)
                    {
                        amplificationValue = (Camera.BottomDestination - Camera.TopDestination) / (float)(lcs.BottomDestination - lcs.TopDestination);
                    }
                    returnValue *= amplificationValue;
                }

                return returnValue;
            }
        }

        float WorldYChangeAt(float zPosition, bool orthogonal, float orthogonalHeight)
        {
#if ANDROID || IOS
            if (PrimaryPush || InputManager.TouchScreen.NumberOfTouchesChanged)
            {
                return 0;
            }
#endif

            float ratio = (float)(mScreenY - mLastScreenY) /
                // See WorldXChangeAt comment for
                // information about this change:
                //(float)FlatRedBallServices.GraphicsOptions.ResolutionHeight;
                (float)this.Camera.DestinationRectangle.Height;

            if (orthogonal)
            {
                return -ratio * orthogonalHeight;
            }
            else
            {

                // This simplifies the code and no longer uses the old YVelocity value which I want to phase out
                return -this.Camera.RelativeYEdgeAt(zPosition) * 2 * ratio;
            }
        }



        #endregion


        #region Internal Methods

        bool mActiveThisFrame = true;
        internal protected virtual void Update(double currentTime)
        {
            bool lastFrameActive = mActiveThisFrame;

            #region EARLY-OUTS!!!!!

            bool shouldEarlyOut = false;

            shouldEarlyOut = !Active;
#if !UNIT_TESTS
            if(RequiresGameWindowInFocus)
            {
                shouldEarlyOut |= !FlatRedBallServices.Game.IsActive;
            }

            // If the game isn't active we may be debugging.  This can cause clicks to happen every frame, and we don't want that
            PrimaryClick = false;
#endif

            if (CustomIsActive != null)
            {
                shouldEarlyOut = !CustomIsActive();
            }

            mActiveThisFrame = !shouldEarlyOut;

            if(RequiresGameWindowInFocus && !FlatRedBallServices.Game.IsActive)
            {
                // Monogame 3.8.1 seems to have a bug where the cursor may still get activity for one frame
                // after the main window loses focus. If true, then the push/click buttons may remain down:
                ClearInputValues();
            }

            if (shouldEarlyOut)
            {


                return;
                
            }

            #endregion



            #region Record the "last" variables

            mLastRay = GetRay();

			mLastPrimaryDown = PrimaryDown;
			mLastSecondaryDown = SecondaryDown;
            mLastMiddleDown = MiddleDown;

            mLastScreenX = mScreenX;
            mLastScreenY = mScreenY;

            #endregion


            #region Reset everything

            PrimaryDown = false;
            PrimaryPush = false;
            PrimaryDoublePush = false;
            PrimaryClick = false;
            PrimaryDoubleClick = false;

            SecondaryDown = false;
            SecondaryPush = false;
            SecondaryClick = false;
            SecondaryDoubleClick = false;

            MiddleDown = false;
            MiddlePush = false;
            MiddleClick = false;

            #endregion

                
            if(ignoreNextFrameInput == false)
            {
                UpdateValuesFromInputDevices(currentTime);
            }
            ignoreNextFrameInput = false;

			#region Keeping inside the screen

            float cameraXEdge;
            float cameraYEdge;

            if (mCamera == SpriteManager.Camera)
            {
                cameraXEdge = GuiManager.XEdge;
                cameraYEdge = GuiManager.YEdge;
            }
            else
            {
                cameraXEdge = mCamera.XEdge;
                cameraYEdge = mCamera.YEdge;

            }

			#endregion


            if (PrimaryClick)
            {
                mLastTimePrimaryClick = currentTime;
            }

            if (PrimaryPush)
            {
                mLastTimePrimaryPush = currentTime;
            }

            // If we don't do this, then clicking off the window and back
            // causes a huge change
            if (!lastFrameActive)
            {
                mLastScreenX = mScreenX;
                mLastScreenY = mScreenY;
            }

            if (PrimaryPush)
            {
                mPushedX = this.ScreenX;
                mPushedY = this.ScreenY;

                mHasDraggedAwayFromPushPoint = false;
            }

            if (PrimaryDown && !mHasDraggedAwayFromPushPoint)
            {
                mHasDraggedAwayFromPushPoint = !HasMovedLessThanXPixelsSincePush(ClickNoSlideThreshold);
            }

            WindowMovingActivity();

            if(WindowOver != null)
            {
                WindowOver.CallRollOver();
            }
        }

        private void UpdateValuesFromInputDevices(double currentTime)
        {
            bool assignPushAndClickValues = true;

            if (CustomUpdate != null)
            {
                assignPushAndClickValues = CustomUpdate(this);
            }
            else
            {
                PrimaryDown = false;
                SecondaryDown = false;

                ZVelocity = 0;
                for (int i = 0; i < devicesControllingCursor.Count; i++)
                {
                    var device = devicesControllingCursor[i];

                    if(device is Xbox360GamePad gamepad)
                    {
                        UpdateValuesFromJoystick(gamepad);
                    }
                    else if(device is Mouse mouse)
                    {
                        assignPushAndClickValues = UpdateValuesFromMouse();
                    }
#if DEBUG
                    else if(device == null)
                    {
                        throw new NullReferenceException("The cursor is referencing a null device - this not allowed");
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            $"An input device of {device.GetType()} is attempting to control the cursor, but this is not implemented in the engine");
                    }
#endif

                }


            }
            #region Clicking, pushing, and holding down button logic

            if (assignPushAndClickValues)
            {
                // todo - maybe we should break these up into separate bools?
                if(ignoreNextPush && !PrimaryDown && !SecondaryDown && !MiddleDown)
                {
                    ignoreNextPush = false;
                }

                if(ignoreNextPush == false)
                {
                    PrimaryPush = PrimaryDown && mLastPrimaryDown == false;
                    SecondaryPush = SecondaryDown == true && mLastSecondaryDown == false;
                    MiddlePush = MiddleDown == true && mLastMiddleDown == false;
                }

                PrimaryDoublePush = PrimaryPush && (currentTime - mLastTimePrimaryPush) < .25;

                PrimaryClick = PrimaryDown == false && mLastPrimaryDown == true;
                SecondaryClick = !SecondaryDown && mLastSecondaryDown;
                MiddleClick = !MiddleDown && mLastMiddleDown;

                PrimaryDoubleClick = PrimaryClick && (currentTime - mLastTimePrimaryClick) < .25;


            }
            #endregion
        }

        public float GamepadPixelsPerSecondMovementSpeed { get; set; } = 600;

        private void UpdateValuesFromJoystick(Xbox360GamePad gamepad)
        {
            if (gamepad != null)
            {
                var inputXPosition = gamepad.LeftStick.Position.X;
                var inputYPosition = gamepad.LeftStick.Position.Y;

                if(gamepad.ButtonDown(Xbox360GamePad.Button.DPadLeft))
                {
                    inputXPosition = -1;
                }
                if (gamepad.ButtonDown(Xbox360GamePad.Button.DPadRight))
                {
                    inputXPosition = 1;
                }
                if (gamepad.ButtonDown(Xbox360GamePad.Button.DPadUp))
                {
                    inputYPosition = 1;
                }
                if (gamepad.ButtonDown(Xbox360GamePad.Button.DPadDown))
                {
                    inputYPosition = -1;
                }

                var thisFrameMultiplier = GamepadPixelsPerSecondMovementSpeed * TimeManager.SecondDifference;
                mScreenX += inputXPosition * thisFrameMultiplier;
                mScreenY -= inputYPosition * thisFrameMultiplier;

                if(mScreenX < 0)
                {
                    mScreenX = 0;
                }
                if(mScreenX > FlatRedBallServices.GraphicsOptions.ResolutionWidth)
                {
                    mScreenX = FlatRedBallServices.GraphicsOptions.ResolutionWidth;
                }

                if (mScreenY < 0)
                {
                    mScreenY = 0;
                }
                if (mScreenY > FlatRedBallServices.GraphicsOptions.ResolutionHeight)
                {
                    mScreenY = FlatRedBallServices.GraphicsOptions.ResolutionHeight;
                }



                PrimaryDown = PrimaryDown ||gamepad.ButtonDown(Xbox360GamePad.Button.A);
                SecondaryDown = SecondaryDown || gamepad.ButtonDown(Xbox360GamePad.Button.X);

                ZVelocity += gamepad.RightStick.Position.Y;
            }
        }



        // Made public so we can use this in our own events to modify the values.
        public bool UpdateValuesFromMouse()
        {
            bool assignPushAndClickValues = false;

            if (InputManager.Mouse != null)
            {
                assignPushAndClickValues = false;

                #region If using windows cursor
                if (mUsingWindowsCursor)
                {
                    bool handled = false;

#if SUPPORTS_TOUCH_SCREEN
                    handled = TryHandleCursorPositionSetByTouchScreen();

#endif

                    if (!handled && (SupportedInputDevices & Gui.InputDevice.Mouse) == Gui.InputDevice.Mouse)
                    {

                        LastInputDevice = InputDevice.Mouse;

                        // If we are using both mouse and touch screen we only
                        // want the mouse to set its values if the user has actually
                        // moved it
                        // Update January 1, 2015
                        // Not sure why we are checking
                        // if the mouse has move to assign
                        // these values.  The touch screen gets 
                        // priority, but if there are no touches (!handled)
                        // then we should default to the mouse, no?
                        // Update January 1, 2015
                        // Actually, if we do use the mouse always, instead of
                        // just when the mouse moves, then the position of the cursor 
                        // will snap back to the mouse position whenever the user lifts
                        // his finger.  Instead, we should just not change mScreenX and rely
                        // on what it was last frame.
                        bool hasMoved = InputManager.Mouse.XChange != 0 || InputManager.Mouse.YChange != 0;

                        if (hasMoved || ((SupportedInputDevices & Gui.InputDevice.TouchScreen) == Gui.InputDevice.TouchScreen) == false)
                        {
                            mScreenX = InputManager.Mouse.X;
                            mScreenY = InputManager.Mouse.Y;
                            handled = true;
                        }
                    }

                    // First record the values from the hardware...
                    if (handled)
                    {
                        mLastXFromHardware = (int)mScreenX;
                        mLastYFromHardware = (int)mScreenY;
                    }
                    else
                    {
                        mScreenX = mLastXFromHardware;
                        mScreenY = mLastYFromHardware;
                    }
                    // ...then modify the mScreenX and mScreenY according to the Matrix
                    if(TransformationMatrix != Matrix.Identity)
                    {
                        Vector3 screenPosition;
                        screenPosition.X = (float)mScreenX;
                        screenPosition.Y = (float)mScreenY;
                        screenPosition.Z = 0;

                        screenPosition = Vector3.Transform(screenPosition, TransformationMatrix);

                        mScreenX = screenPosition.X;
                        mScreenY = screenPosition.Y;
                    }


                    ZVelocity = InputManager.Mouse.ScrollWheelChange;

                    #region if not static position, read the new position
                    if (this.StaticPosition == false)
                    {
                        UpdatePositionToMouse();

#if ANDROID || IOS
                        float xVelocity;
                        float yVelocity;
                        MathFunctions.ScreenToAbsoluteDistance(
                            InputManager.TouchScreen.AverageTouchPointChange.X,
                            -InputManager.TouchScreen.AverageTouchPointChange.Y,
                            out xVelocity, out yVelocity, mCamera.Z - 100, mCamera);

#endif
                    }
                    #endregion
                }
                #endregion


                #region using an FRB-drawn cursor
                else
                {
                    ZVelocity = InputManager.Mouse.ScrollWheelChange;
                }
                #endregion

                #region Handle all of the "Primary" variables

                if ((SupportedInputDevices & Gui.InputDevice.TouchScreen) == Gui.InputDevice.TouchScreen)
                {
#if SUPPORTS_TOUCH_SCREEN
                    GetPushDownClickFromTouchScreen();
#endif
                }
                if ((SupportedInputDevices & Gui.InputDevice.Mouse) == Gui.InputDevice.Mouse )
                {
                    GetPushDownClickFromMouse();
                }

                #endregion


                if (PrimaryPush)
                {
                    // We want to check an actual ==
                    // and not a .HasFlag because if there
                    // is a moue involved we want to use that position
                    if (SupportedInputDevices == Gui.InputDevice.TouchScreen)
                    {
                        mLastScreenX = mScreenX;
                        mLastScreenY = mScreenY;
                    }
                }

            }

            return assignPushAndClickValues;
        }

        private void WindowResizingActivity()
        {
            if (WindowPushed != null && mSidesGrabbed != Sides.None)
            {
                #region Resizing the Window

                if (mSidesGrabbed == Sides.BottomRight)
                {
                    float oldScaleX = WindowPushed.ScaleX;
                    float oldScaleY = WindowPushed.ScaleY;



                    #region Keep the Window from becoming too small

                    if (WindowPushed.ScaleX < 2)
                    {
                        float difference = 2 - WindowPushed.ScaleX;

                        WindowPushed.ScaleX += difference;
                        WindowPushed.X += difference;
                    }

                    if (WindowPushed.ScaleY < 1)
                    {
                        float difference = 1 - WindowPushed.ScaleY;

                        WindowPushed.ScaleY += difference;

                        WindowPushed.Y += difference;
                    }

                    #endregion


                    // now that we have the change, the attached Windows need to have their relative positions changed
                    float ScaleXDifference = WindowPushed.ScaleX - oldScaleX;
                    float ScaleYDifference = WindowPushed.ScaleY - oldScaleY;


                    if (ScaleXDifference != 0 || ScaleYDifference != 0)
                    {
                        // To improve performance and simplify debugging only call
                        // callOnResize if the scale values have changed.
                        WindowPushed.OnResize();
                    }
                }
                #endregion

            }
        }

        private void WindowMovingActivity()
        {

            if (WindowGrabbed != null)
            {
                if (WindowGrabbed.MovesWhenGrabbed)
                {
                    if (WindowGrabbed.Parent == null)
                    {

                        float outX = WorldXAt(0);
                        float outY = WorldYAt(0);

                        WindowGrabbed.WorldUnitX = outX + mGrabbedWindowRelativeX;
                        WindowGrabbed.WorldUnitY = outY + mGrabbedWindowRelativeY;
                    }
                    else
                    {
                        float cursorX = WorldXAt(0);
                        float cursorY = WorldYAt(0);

                        WindowGrabbed.WorldUnitRelativeX = cursorX + mGrabbedWindowRelativeX - WindowGrabbed.Parent.WorldUnitX;
                        WindowGrabbed.WorldUnitRelativeY = cursorY + mGrabbedWindowRelativeY - WindowGrabbed.Parent.WorldUnitY;
                    }
                }

                WindowGrabbed.OnDragging();
            }
        }
        
        private void GetPushDownClickFromMouse()
        {

            PrimaryDown |= InputManager.Mouse.ButtonDown(FlatRedBall.Input.Mouse.MouseButtons.LeftButton);
            PrimaryPush |= InputManager.Mouse.ButtonPushed(FlatRedBall.Input.Mouse.MouseButtons.LeftButton);
            PrimaryClick |= InputManager.Mouse.ButtonReleased(FlatRedBall.Input.Mouse.MouseButtons.LeftButton) &&
                ignoreNextPush == false;
            PrimaryDoubleClick |= InputManager.Mouse.ButtonDoubleClicked(FlatRedBall.Input.Mouse.MouseButtons.LeftButton);
            PrimaryDoublePush |= InputManager.Mouse.ButtonDoublePushed(FlatRedBall.Input.Mouse.MouseButtons.LeftButton);

            SecondaryDown |= InputManager.Mouse.ButtonDown(FlatRedBall.Input.Mouse.MouseButtons.RightButton);
            SecondaryPush |= InputManager.Mouse.ButtonPushed(FlatRedBall.Input.Mouse.MouseButtons.RightButton);
            SecondaryClick |= InputManager.Mouse.ButtonReleased(FlatRedBall.Input.Mouse.MouseButtons.RightButton) &&
                ignoreNextPush == false;

            SecondaryDoubleClick |= InputManager.Mouse.ButtonDoubleClicked(FlatRedBall.Input.Mouse.MouseButtons.RightButton);

            MiddleDown |= InputManager.Mouse.ButtonDown(FlatRedBall.Input.Mouse.MouseButtons.MiddleButton);
            MiddlePush |= InputManager.Mouse.ButtonPushed(Mouse.MouseButtons.MiddleButton);
            MiddleClick |= InputManager.Mouse.ButtonReleased(Mouse.MouseButtons.MiddleButton) &&
                ignoreNextPush == false;

            if (ignoreNextPush && !PrimaryDown && !SecondaryDown && !MiddleDown)
            {
                ignoreNextPush = false;
            }
        }

        #endregion

        #region Private Methods

        private void UpdatePositionToMouse()
        {
            // Is this method only called when using a windows cursor?
            float outX = 0;
            float outY = 0;

            float xEdge = GuiManager.XEdge;
            float yEdge = GuiManager.YEdge;

            if (float.IsNaN(GuiManager.OverridingFieldOfView) == false)
            {
                yEdge = (float)(100 * System.Math.Tan(GuiManager.OverridingFieldOfView / 2.0));
                xEdge = yEdge * SpriteManager.Camera.AspectRatio;

            }

            FlatRedBall.Math.MathFunctions.WindowToAbsolute(
                InputManager.Mouse.X,
                InputManager.Mouse.Y,
                ref outX,
                ref outY,
                -100,
                SpriteManager.Camera.Position,
                xEdge,
                yEdge,
                SpriteManager.Camera.DestinationRectangle,
                Camera.CoordinateRelativity.RelativeToCamera);

        }

        #endregion

    }

}
