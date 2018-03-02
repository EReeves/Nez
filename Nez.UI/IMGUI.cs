using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Utils.Extensions;

namespace Nez.UI
{
	/// <summary>
	///     IMGUI is a very simple class with only static methods designed to make sticking buttons, checkboxes, sliders and
	///     progress bars on screen
	///     in quick and dirty fashion. It is not designed to be a full and proper UI system.
	/// </summary>
	public class Imgui
    {
        private const float FontLineHeight = 10;
        private const float ElementHeight = 20;
        private const float ShortElementHeight = 15;
        private const float ElementPadding = 10;
        private static readonly SpriteBatch SpriteBatch;
        private static readonly BitmapFont Font;
        private static Vector2 _fontScale;
        private static readonly Color FontColor = new Color(255, 255, 255);
        private static readonly Color WindowColor = new Color(17, 17, 17);
        private static readonly Color ButtonColor = new Color(78, 91, 98);
        private static readonly Color ButtonColorActive = new Color(168, 207, 115);
        private static readonly Color ButtonColorDown = new Color(244, 23, 135);
        private static readonly Color ToggleBg = new Color(63, 63, 63);
        private static readonly Color ToggleBgActive = new Color(130, 130, 130);
        private static readonly Color ToggleOn = new Color(168, 207, 115);
        private static readonly Color ToggleOnActive = new Color(244, 23, 135);
        private static readonly Color SliderBg = new Color(78, 91, 98);
        private static readonly Color SliderThumbBg = new Color(25, 144, 188);
        private static readonly Color SliderThumbBgActive = new Color(168, 207, 115);
        private static readonly Color SliderThumbBgDown = new Color(244, 23, 135);
        private static readonly Color HeaderBg = new Color(40, 46, 50);
        private static float _lastY;
        private static float _elementX;
        private static float _windowWidth;

        private enum TextAlign
        {
            Left,
            Center,
            Right
        }

        // constants

        // colors

        // state
#pragma warning disable 0414
        private static float _windowHeight;
        private static float _elementWidth;
        private static Point _mouseInWorldCoords;


        static Imgui()
        {
            SpriteBatch = new SpriteBatch(Core.GraphicsDevice);
            Font = Graphics.Graphics.Instance.BitmapFont;

            var scale = FontLineHeight / Font.LineHeight;
            _fontScale = new Vector2(scale, scale);
        }


        #region Helpers

        private static void DrawString(string text, Color color, TextAlign align = TextAlign.Center,
            float elementHeight = ElementHeight)
        {
            // center align the text
            var textSize = Font.MeasureString(text) * _fontScale.Y;
            var x = _elementX;
            switch (align)
            {
                case TextAlign.Center:
                    x += (_elementWidth - textSize.X) * 0.5f;
                    break;
                case TextAlign.Right:
                    x = _elementX + _elementWidth - textSize.X;
                    break;
            }

            var y = _lastY + ElementPadding + (elementHeight - FontLineHeight) * 0.5f;

            SpriteBatch.DrawString(Font, text, new Vector2(x, y), color, 0, Vector2.Zero, _fontScale,
                SpriteEffects.None, 0);
        }


        private static bool IsMouseOverElement()
        {
            var rect = new Rectangle((int) _elementX, (int) _lastY + (int) ElementPadding, (int) _elementWidth,
                (int) ElementHeight);
            return rect.Contains(_mouseInWorldCoords);
        }


        private static bool IsMouseBetween(float left, float right)
        {
            var rect = new Rectangle((int) left, (int) _lastY + (int) ElementPadding, (int) right - (int) left,
                (int) ElementHeight);
            return rect.Contains(_mouseInWorldCoords);
        }


        private static void EndElement(float elementHeight = ElementHeight)
        {
            _lastY += elementHeight + ElementPadding;
        }

        #endregion


	    /// <summary>
	    ///     begins an IMGUI window specifying where and how large it should be. If you are not using IMGUI in world space (for
	    ///     example, inside
	    ///     a Scene with a scaled resolution policy) passing false for useRawMousePosition will use the
	    ///     Input.scaledMousePosition.
	    /// </summary>
	    /// <param name="x">The x coordinate.</param>
	    /// <param name="y">The y coordinate.</param>
	    /// <param name="width">Width.</param>
	    /// <param name="height">Height.</param>
	    /// <param name="useRawMousePosition">If set to <c>true</c> use raw mouse position.</param>
	    public static void BeginWindow(float x, float y, float width, float height, bool useRawMousePosition = true)
        {
            SpriteBatch.Begin();

            SpriteBatch.DrawRect(x, y, width, height, WindowColor);

            _elementX = x + ElementPadding;
            _lastY = y;
            _windowWidth = width;
            _windowHeight = height;
            _elementWidth = _windowWidth - 2f * ElementPadding;

            var mousePos = useRawMousePosition ? Input.Input.RawMousePosition : Input.Input.ScaledMousePosition.ToPoint();
            _mouseInWorldCoords = mousePos - new Point(Core.GraphicsDevice.Viewport.X, Core.GraphicsDevice.Viewport.Y);
        }


        public static void EndWindow()
        {
            SpriteBatch.End();
        }


        public static bool Button(string text)
        {
            var ret = false;

            var color = ButtonColor;
            if (IsMouseOverElement())
            {
                ret = Input.Input.LeftMouseButtonReleased;
                color = Input.Input.LeftMouseButtonDown ? ButtonColorDown : ButtonColorActive;
            }

            SpriteBatch.DrawRect(_elementX, _lastY + ElementPadding, _elementWidth, ElementHeight, color);
            DrawString(text, FontColor);
            EndElement();

            return ret;
        }


	    /// <summary>
	    ///     creates a checkbox/toggle
	    /// </summary>
	    /// <param name="text">Text.</param>
	    /// <param name="isChecked">If set to <c>true</c> is checked.</param>
	    public static bool Toggle(string text, bool isChecked)
        {
            var toggleX = _elementX + _elementWidth - ElementHeight;
            var color = ToggleBg;
            var toggleCheckColor = ToggleOn;
            var isToggleActive = false;

            if (IsMouseBetween(toggleX, toggleX + ElementHeight))
            {
                color = ToggleBgActive;
                if (Input.Input.LeftMouseButtonDown)
                {
                    isToggleActive = true;
                    toggleCheckColor = ToggleOnActive;
                }

                if (Input.Input.LeftMouseButtonReleased)
                    isChecked = !isChecked;
            }

            DrawString(text, FontColor, TextAlign.Left);
            SpriteBatch.DrawRect(toggleX, _lastY + ElementPadding, ElementHeight, ElementHeight, color);

            if (isChecked || isToggleActive)
                SpriteBatch.DrawRect(toggleX + 3, _lastY + ElementPadding + 3, ElementHeight - 6, ElementHeight - 6,
                    toggleCheckColor);

            EndElement();

            return isChecked;
        }


	    /// <summary>
	    ///     value should be between 0 and 1
	    /// </summary>
	    /// <param name="value">Value.</param>
	    public static float Slider(float value, string name = "")
        {
            var workingWidth = _elementWidth - ShortElementHeight;
            var thumbPos = workingWidth * value;
            var color = SliderThumbBg;

            if (IsMouseOverElement())
                if (Input.Input.LeftMouseButtonDown)
                {
                    var localMouseX = _mouseInWorldCoords.X - _elementX - ShortElementHeight * 0.5f;
                    value = MathHelper.Clamp(localMouseX / workingWidth, 0, 1);
                    thumbPos = workingWidth * value;
                    color = SliderThumbBgDown;
                }
                else
                {
                    color = SliderThumbBgActive;
                }

            SpriteBatch.DrawRect(_elementX, _lastY + ElementPadding, _elementWidth, ShortElementHeight, SliderBg);
            SpriteBatch.DrawRect(_elementX + thumbPos, _lastY + ElementPadding, ShortElementHeight,
                ShortElementHeight, color);
            DrawString(name + value.ToString("F"), FontColor, TextAlign.Center, ShortElementHeight);
            EndElement();

            return value;
        }


	    /// <summary>
	    ///     value should be between 0 and 1
	    /// </summary>
	    /// <returns>The bar.</returns>
	    /// <param name="value">Value.</param>
	    public static float ProgressBar(float value)
        {
            var thumbPos = _elementWidth * value;
            var color = SliderThumbBg;

            if (IsMouseOverElement())
                if (Input.Input.LeftMouseButtonDown)
                {
                    var localMouseX = _mouseInWorldCoords.X - _elementX;
                    value = MathHelper.Clamp(localMouseX / _elementWidth, 0, 1);
                    thumbPos = _elementWidth * value;
                    color = SliderThumbBgDown;
                }
                else
                {
                    color = SliderThumbBgActive;
                }

            SpriteBatch.DrawRect(_elementX, _lastY + ElementPadding, _elementWidth, ElementHeight, SliderBg);
            SpriteBatch.DrawRect(_elementX, _lastY + ElementPadding, thumbPos, ElementHeight, color);
            DrawString(value.ToString("F"), FontColor);
            EndElement();

            return value;
        }


	    /// <summary>
	    ///     creates a full width header with text
	    /// </summary>
	    /// <param name="text">Text.</param>
	    public static void Header(string text)
        {
            // expand the header to full width and use a shorter element height
            SpriteBatch.DrawRect(_elementX - ElementPadding, _lastY + ElementPadding,
                _elementWidth + ElementPadding * 2, ShortElementHeight, HeaderBg);
            DrawString(text, FontColor, TextAlign.Center, ShortElementHeight);
            EndElement(ShortElementHeight);
        }


	    /// <summary>
	    ///     adds some vertical space
	    /// </summary>
	    /// <param name="verticalSpace">Vertical space.</param>
	    public static void Space(float verticalSpace)
        {
            _lastY += verticalSpace;
        }
    }
}