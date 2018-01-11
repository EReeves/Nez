using System;
using Microsoft.Xna.Framework;
using Nez.UI.Base;
using Nez.UI.Containers;
using Nez.UI.Drawable;
using IDrawable = Nez.UI.Drawable.IDrawable;

namespace Nez.UI.Widgets
{
    public class Button : Table, IInputListener, IGamepadFocusable
    {
        internal ButtonGroup ButtonGroup;
        protected bool isChecked;
        protected bool isDisabled;
        protected bool MouseOver, MouseDown;

	    /// <summary>
	    ///     the maximum distance outside the button the mouse can move when pressing it to cause it to be unfocused
	    /// </summary>
	    public float ButtonBoundaryThreshold = 50f;

        public bool ProgrammaticChangeEvents;
        private ButtonStyle _style;

        public override float PreferredWidth
        {
            get
            {
                var width = base.PreferredWidth;
                if (_style.Up != null)
                    width = System.Math.Max(width, _style.Up.MinWidth);
                if (_style.Down != null)
                    width = System.Math.Max(width, _style.Down.MinWidth);
                if (_style.Checkked != null)
                    width = System.Math.Max(width, _style.Checkked.MinWidth);
                return width;
            }
        }

        public override float PreferredHeight
        {
            get
            {
                var height = base.PreferredHeight;
                if (_style.Up != null)
                    height = System.Math.Max(height, _style.Up.MinHeight);
                if (_style.Down != null)
                    height = System.Math.Max(height, _style.Down.MinHeight);
                if (_style.Checkked != null)
                    height = System.Math.Max(height, _style.Checkked.MinHeight);
                return height;
            }
        }

        public override float MinWidth => PreferredWidth;

        public override float MinHeight => PreferredHeight;

        public bool IsChecked
        {
            get => isChecked;
            set => SetChecked(value, ProgrammaticChangeEvents);
        }

        public event Action<bool> OnChanged;
        public event Action<Button> OnClicked;


        public virtual void SetStyle(ButtonStyle style)
        {
            this._style = style;

            if (MouseDown && !isDisabled)
            {
                Background = style.Down == null ? style.Up : style.Down;
            }
            else
            {
                if (isDisabled && style.Disabled != null)
                    Background = style.Disabled;
                else if (isChecked && style.Checkked != null)
                    Background = MouseOver && style.CheckedOver != null ? style.CheckedOver : style.Checkked;
                else if (MouseOver && style.Over != null)
                    Background = style.Over;
                else
                    Background = style.Up;
            }

            SetBackground(Background);
        }


        private void SetChecked(bool isChecked, bool fireEvent)
        {
            if (this.isChecked == isChecked)
                return;

            if (ButtonGroup != null && !ButtonGroup.CanCheck(this, isChecked))
                return;
            this.isChecked = isChecked;

            if (fireEvent && OnChanged != null)
                OnChanged(isChecked);
        }


	    /// <summary>
	    ///     Toggles the checked state. This method changes the checked state, which fires a {@link onChangedEvent} (if
	    ///     programmatic change
	    ///     events are enabled), so can be used to simulate a button click.
	    /// </summary>
	    public void Toggle()
        {
            isChecked = !isChecked;
        }


	    /// <summary>
	    ///     Returns the button's style. Modifying the returned style may not have an effect until {@link
	    ///     #setStyle(ButtonStyle)} is called.
	    /// </summary>
	    /// <returns>The style.</returns>
	    public virtual ButtonStyle GetStyle()
        {
            return _style;
        }


	    /// <summary>
	    ///     May be null
	    /// </summary>
	    /// <returns>The button group.</returns>
	    public ButtonGroup GetButtonGroup()
        {
            return ButtonGroup;
        }


        public void SetDisabled(bool disabled)
        {
            isDisabled = disabled;
        }


        public bool GetDisabled()
        {
            return isDisabled;
        }


        public override void Draw(Graphics.Graphics graphics, float parentAlpha)
        {
            Validate();

            if (isDisabled && _style.Disabled != null)
                Background = _style.Disabled;
            else if (MouseDown && _style.Down != null)
                Background = _style.Down;
            else if (isChecked && _style.Checkked != null)
                Background = _style.CheckedOver != null && MouseOver ? _style.CheckedOver : _style.Checkked;
            else if (MouseOver && _style.Over != null)
                Background = _style.Over;
            else if (_style.Up != null) //
                Background = _style.Up;
            SetBackground(Background);

            float offsetX = 0, offsetY = 0;
            if (MouseDown && !isDisabled)
            {
                offsetX = _style.PressedOffsetX;
                offsetY = _style.PressedOffsetY;
            }
            else if (isChecked && !isDisabled)
            {
                offsetX = _style.CheckedOffsetX;
                offsetY = _style.CheckedOffsetY;
            }
            else
            {
                offsetX = _style.UnpressedOffsetX;
                offsetY = _style.UnpressedOffsetY;
            }

            for (var i = 0; i < Children.Count; i++)
                Children[i].MoveBy(offsetX, offsetY);

            base.Draw(graphics, parentAlpha);

            for (var i = 0; i < Children.Count; i++)
                Children[i].MoveBy(-offsetX, -offsetY);
        }


        public override string ToString()
        {
            return "[Button]";
        }


        #region Constructors

        public Button(ButtonStyle style)
        {
            SetTouchable(Touchable.Enabled);
            SetStyle(style);
            SetSize(PreferredWidth, PreferredHeight);
        }


        public Button(Skin skin, string styleName = null) : this(skin.Get<ButtonStyle>(styleName))
        {
        }


        public Button(IDrawable up) : this(new ButtonStyle(up, null, null))
        {
        }


        public Button(IDrawable up, IDrawable down) : this(new ButtonStyle(up, down, null))
        {
        }


        public Button(IDrawable up, IDrawable down, IDrawable checked_) : this(new ButtonStyle(up, down, checked_))
        {
        }

        #endregion


        #region IInputListener

        void IInputListener.OnMouseEnter()
        {
            MouseOver = true;
        }


        void IInputListener.OnMouseExit()
        {
            MouseOver = MouseDown = false;
        }


        bool IInputListener.OnMousePressed(Vector2 mousePos)
        {
            if (isDisabled)
                return false;

            MouseDown = true;
            return true;
        }


        void IInputListener.OnMouseMoved(Vector2 mousePos)
        {
            // if we get too far outside the button cancel future events
            if (DistanceOutsideBoundsToPoint(mousePos) > ButtonBoundaryThreshold)
            {
                MouseDown = MouseOver = false;
                GetStage().RemoveInputFocusListener(this);
            }
        }


        void IInputListener.OnMouseUp(Vector2 mousePos)
        {
           // if (MouseDown == false) return;
            MouseDown = false;

            SetChecked(!isChecked, true);

            if (OnClicked != null)
                OnClicked(this);
        }


        bool IInputListener.OnMouseScrolled(int mouseWheelDelta)
        {
            return false;
        }

        #endregion


        #region IGamepadFocusable

        public bool ShouldUseExplicitFocusableControl { get; set; }
        public IGamepadFocusable GamepadUpElement { get; set; }
        public IGamepadFocusable GamepadDownElement { get; set; }
        public IGamepadFocusable GamepadLeftElement { get; set; }
        public IGamepadFocusable GamepadRightElement { get; set; }


        public void EnableExplicitFocusableControl(IGamepadFocusable upEle, IGamepadFocusable downEle,
            IGamepadFocusable leftEle, IGamepadFocusable rightEle)
        {
            ShouldUseExplicitFocusableControl = true;
            GamepadUpElement = upEle;
            GamepadDownElement = downEle;
            GamepadLeftElement = leftEle;
            GamepadRightElement = rightEle;
        }


        void IGamepadFocusable.OnUnhandledDirectionPressed(Direction direction)
        {
        }


        void IGamepadFocusable.OnFocused()
        {
            OnFocused();
        }


        void IGamepadFocusable.OnUnfocused()
        {
            OnUnfocused();
        }


        void IGamepadFocusable.OnActionButtonPressed()
        {
            OnActionButtonPressed();
        }


        void IGamepadFocusable.OnActionButtonReleased()
        {
            OnActionButtonReleased();
        }

        #endregion


        #region overrideable focus handlers

        protected virtual void OnFocused()
        {
            MouseOver = true;
        }


        protected virtual void OnUnfocused()
        {
            MouseOver = MouseDown = false;
        }


        protected virtual void OnActionButtonPressed()
        {
            MouseDown = true;
        }


        protected virtual void OnActionButtonReleased()
        {
            MouseDown = false;

            SetChecked(!isChecked, true);

            if (OnClicked != null)
                OnClicked(this);
        }

        #endregion
    }


	/// <summary>
	///     The style for a button
	/// </summary>
	public class ButtonStyle
    {
        /** Optional. offsets children (labels for example). */
        public float PressedOffsetX, PressedOffsetY, UnpressedOffsetX, UnpressedOffsetY, CheckedOffsetX, CheckedOffsetY;

        /** Optional. */
        public IDrawable Up, Down, Over, Checkked, CheckedOver, Disabled;


        public ButtonStyle()
        {
        }


        public ButtonStyle(IDrawable up, IDrawable down, IDrawable over)
        {
            this.Up = up;
            this.Down = down;
            this.Over = over;
        }


        public static ButtonStyle Create(Color upColor, Color downColor, Color overColor)
        {
            return new ButtonStyle
            {
                Up = new PrimitiveDrawable(upColor),
                Down = new PrimitiveDrawable(downColor),
                Over = new PrimitiveDrawable(overColor)
            };
        }


        public ButtonStyle Clone()
        {
            return new ButtonStyle
            {
                Up = Up,
                Down = Down,
                Over = Over,
                Checkked = Checkked,
                CheckedOver = CheckedOver,
                Disabled = Disabled
            };
        }
    }
}