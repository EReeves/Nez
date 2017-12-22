using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nez.Systems;
#if !FNA

#endif


namespace Nez
{
    public static class Input
    {
        public const float DefaultDeadzone = 0.1f;
        public static Emitter<InputEventType, InputEvent> Emitter;

        public static GamePadData[] GamePads;

	    /// <summary>
	    ///     set by the Scene and used to scale mouse input
	    /// </summary>
	    internal static Vector2 ResolutionScale;

	    /// <summary>
	    ///     set by the Scene and used to scale input
	    /// </summary>
	    internal static Point ResolutionOffset;

        private static KeyboardState _previousKbState;
        private static KeyboardState _currentKbState;
        private static MouseState _currentMouseState;
        internal static FastList<VirtualInput> VirtualInputs = new FastList<VirtualInput>();
        private static int _maxSupportedGamePads;

        public static TouchInput Touch;


        static Input()
        {
            Emitter = new Emitter<InputEventType, InputEvent>();
            Touch = new TouchInput();

            _previousKbState = new KeyboardState();
            _currentKbState = Keyboard.GetState();

            PreviousMouseState = new MouseState();
            _currentMouseState = Mouse.GetState();

            MaxSupportedGamePads = 1;
        }


        public static int MaxSupportedGamePads
        {
            get => _maxSupportedGamePads;
            set
            {
                _maxSupportedGamePads = Mathf.Clamp(value, 1, 4);
                GamePads = new GamePadData[_maxSupportedGamePads];
                for (var i = 0; i < _maxSupportedGamePads; i++)
                    GamePads[i] = new GamePadData((PlayerIndex) i);
            }
        }


        public static void Update()
        {
            Touch.Update();

            _previousKbState = _currentKbState;
            _currentKbState = Keyboard.GetState();

            PreviousMouseState = _currentMouseState;
            _currentMouseState = Mouse.GetState();

            for (var i = 0; i < _maxSupportedGamePads; i++)
                GamePads[i].Update();

            for (var i = 0; i < VirtualInputs.Length; i++)
                VirtualInputs.Buffer[i].Update();
        }

	    /// <summary>
	    ///     this takes into account the SceneResolutionPolicy and returns the value scaled to the RenderTargets coordinates
	    /// </summary>
	    /// <value>The scaled position.</value>
	    [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 ScaledPosition(Vector2 position)
        {
            var scaledPos = new Vector2(position.X - ResolutionOffset.X, position.Y - ResolutionOffset.Y);
            return scaledPos * ResolutionScale;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 ScaledPosition(Point position)
        {
            return ScaledPosition(new Vector2(position.X, position.Y));
        }

        #region Keyboard

        public static KeyboardState PreviousKeyboardState => _previousKbState;

        public static KeyboardState CurrentKeyboardState => _currentKbState;


	    /// <summary>
	    ///     only true if down this frame
	    /// </summary>
	    /// <returns><c>true</c>, if key pressed was gotten, <c>false</c> otherwise.</returns>
	    public static bool IsKeyPressed(Keys key)
        {
            return _currentKbState.IsKeyDown(key) && !_previousKbState.IsKeyDown(key);
        }


	    /// <summary>
	    ///     true the entire time the key is down
	    /// </summary>
	    /// <returns><c>true</c>, if key down was gotten, <c>false</c> otherwise.</returns>
	    public static bool IsKeyDown(Keys key)
        {
            return _currentKbState.IsKeyDown(key);
        }


	    /// <summary>
	    ///     true only the frame the key is released
	    /// </summary>
	    /// <returns><c>true</c>, if key up was gotten, <c>false</c> otherwise.</returns>
	    public static bool IsKeyReleased(Keys key)
        {
            return !_currentKbState.IsKeyDown(key) && _previousKbState.IsKeyDown(key);
        }


	    /// <summary>
	    ///     only true if one of the keys is down this frame
	    /// </summary>
	    /// <returns><c>true</c>, if key pressed was gotten, <c>false</c> otherwise.</returns>
	    public static bool IsKeyPressed(Keys keyA, Keys keyB)
        {
            return IsKeyPressed(keyA) || IsKeyPressed(keyB);
        }


	    /// <summary>
	    ///     true while either of the keys are down
	    /// </summary>
	    /// <returns><c>true</c>, if key down was gotten, <c>false</c> otherwise.</returns>
	    public static bool IsKeyDown(Keys keyA, Keys keyB)
        {
            return IsKeyDown(keyA) || IsKeyDown(keyB);
        }


	    /// <summary>
	    ///     true only the frame one of the keys are released
	    /// </summary>
	    /// <returns><c>true</c>, if key up was gotten, <c>false</c> otherwise.</returns>
	    public static bool IsKeyReleased(Keys keyA, Keys keyB)
        {
            return IsKeyReleased(keyA) || IsKeyReleased(keyB);
        }

        #endregion


        #region Mouse

	    /// <summary>
	    ///     returns the previous mouse state. Use with caution as it only contains raw data and does not take camera scaling
	    ///     into affect like
	    ///     Input.mousePosition does.
	    /// </summary>
	    /// <value>The state of the previous mouse.</value>
	    public static MouseState PreviousMouseState { get; private set; }

	    /// <summary>
	    ///     only true if down this frame
	    /// </summary>
	    public static bool LeftMouseButtonPressed => _currentMouseState.LeftButton == ButtonState.Pressed &&
                                                     PreviousMouseState.LeftButton == ButtonState.Released;

	    /// <summary>
	    ///     true while the button is down
	    /// </summary>
	    public static bool LeftMouseButtonDown => _currentMouseState.LeftButton == ButtonState.Pressed;

	    /// <summary>
	    ///     true only the frame the button is released
	    /// </summary>
	    public static bool LeftMouseButtonReleased => _currentMouseState.LeftButton == ButtonState.Released &&
                                                      PreviousMouseState.LeftButton == ButtonState.Pressed;

	    /// <summary>
	    ///     only true if pressed this frame
	    /// </summary>
	    public static bool RightMouseButtonPressed => _currentMouseState.RightButton == ButtonState.Pressed &&
                                                      PreviousMouseState.RightButton == ButtonState.Released;

	    /// <summary>
	    ///     true while the button is down
	    /// </summary>
	    public static bool RightMouseButtonDown => _currentMouseState.RightButton == ButtonState.Pressed;

	    /// <summary>
	    ///     true only the frame the button is released
	    /// </summary>
	    public static bool RightMouseButtonReleased => _currentMouseState.RightButton == ButtonState.Released &&
                                                       PreviousMouseState.RightButton == ButtonState.Pressed;

	    /// <summary>
	    ///     only true if down this frame
	    /// </summary>
	    public static bool MiddleMouseButtonPressed => _currentMouseState.MiddleButton == ButtonState.Pressed &&
                                                       PreviousMouseState.MiddleButton == ButtonState.Released;

	    /// <summary>
	    ///     true while the button is down
	    /// </summary>
	    public static bool MiddleMouseButtonDown => _currentMouseState.MiddleButton == ButtonState.Pressed;

	    /// <summary>
	    ///     true only the frame the button is released
	    /// </summary>
	    public static bool MiddleMouseButtonReleased => _currentMouseState.MiddleButton == ButtonState.Released &&
                                                        PreviousMouseState.MiddleButton == ButtonState.Pressed;

	    /// <summary>
	    ///     gets the raw ScrollWheelValue
	    /// </summary>
	    /// <value>The mouse wheel.</value>
	    public static int MouseWheel => _currentMouseState.ScrollWheelValue;

	    /// <summary>
	    ///     gets the delta ScrollWheelValue
	    /// </summary>
	    /// <value>The mouse wheel delta.</value>
	    public static int MouseWheelDelta => _currentMouseState.ScrollWheelValue - PreviousMouseState.ScrollWheelValue;

	    /// <summary>
	    ///     unscaled mouse position. This is the actual screen space value
	    /// </summary>
	    /// <value>The raw mouse position.</value>
	    public static Point RawMousePosition => new Point(_currentMouseState.X, _currentMouseState.Y);

	    /// <summary>
	    ///     alias for scaledMousePosition
	    /// </summary>
	    /// <value>The mouse position.</value>
	    public static Vector2 MousePosition => ScaledMousePosition;

	    /// <summary>
	    ///     this takes into account the SceneResolutionPolicy and returns the value scaled to the RenderTargets coordinates
	    /// </summary>
	    /// <value>The scaled mouse position.</value>
	    public static Vector2 ScaledMousePosition =>
            ScaledPosition(new Vector2(_currentMouseState.X, _currentMouseState.Y));

        public static Point MousePositionDelta => new Point(_currentMouseState.X, _currentMouseState.Y) -
                                                  new Point(PreviousMouseState.X, PreviousMouseState.Y);

        public static Vector2 ScaledMousePositionDelta
        {
            get
            {
                var pastPos = new Vector2(PreviousMouseState.X - ResolutionOffset.X,
                    PreviousMouseState.Y - ResolutionOffset.Y);
                pastPos *= ResolutionScale;
                return ScaledMousePosition - pastPos;
            }
        }

        #endregion
    }


    public enum InputEventType
    {
        GamePadConnected,
        GamePadDisconnected
    }


    public struct InputEvent
    {
        public int GamePadIndex;
    }


	/// <summary>
	///     comparer that should be passed to a dictionary constructor to avoid boxing/unboxing when using an enum as a key
	///     on Mono
	/// </summary>
	public struct InputEventTypeComparer : IEqualityComparer<InputEventType>
    {
        public bool Equals(InputEventType x, InputEventType y)
        {
            return x == y;
        }


        public int GetHashCode(InputEventType obj)
        {
            return (int) obj;
        }
    }
}