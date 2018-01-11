using System;
using Microsoft.Xna.Framework;
using Nez.Debug;
using Nez.Input;
using Nez.Maths;
using Nez.UI.Base;
using Nez.UI.Drawable;
using IDrawable = Nez.UI.Drawable.IDrawable;

namespace Nez.UI.Widgets
{
    public class ProgressBar : Element
    {
        protected float Min, Max, StepSize;

        protected float Value;
        protected bool Vertical;

        public bool Disabled;
        protected float Position;
        public bool ShiftIgnoresSnap;
        public float SnapThreshold;

        public float[] SnapValues;
        private ProgressBarStyle _style;


        public ProgressBar(float min, float max, float stepSize, bool vertical, ProgressBarStyle style)
        {
            Assert.IsTrue(min < max, "min must be less than max");
            Assert.IsTrue(stepSize > 0, "stepSize must be greater than 0");

            SetStyle(style);
            Min = min;
            Max = max;
            StepSize = stepSize;
            Vertical = vertical;
            Value = Min;

            SetSize(PreferredWidth, PreferredHeight);
        }

        public ProgressBar(float min, float max, float stepSize, bool vertical, Skin skin, string styleName = null) :
            this(min, max, stepSize, vertical, skin.Get<ProgressBarStyle>(styleName))
        {
        }

        public ProgressBar(Skin skin, string styleName = null) : this(0, 1, 0.01f, false, skin)
        {
        }

        public override float PreferredWidth
        {
            get
            {
                if (Vertical)
                    return System.Math.Max(_style.Knob == null ? 0 : _style.Knob.MinWidth,
                        _style.Background != null ? _style.Background.MinWidth : 0);
                return 140;
            }
        }

        public override float PreferredHeight
        {
            get
            {
                if (Vertical)
                    return 140;
                return System.Math.Max(_style.Knob == null ? 0 : _style.Knob.MinHeight,
                    _style.Background != null ? _style.Background.MinHeight : 0);
            }
        }

        public event Action<float> OnChanged;


        public virtual void SetStyle(ProgressBarStyle style)
        {
            this._style = style;
            InvalidateHierarchy();
        }


	    /// <summary>
	    ///     Returns the progress bar's style. Modifying the returned style may not have an effect until
	    ///     {@link #setStyle(ProgressBarStyle)} is called.
	    /// </summary>
	    /// <returns>The style.</returns>
	    public ProgressBarStyle GetStyle()
        {
            return _style;
        }


	    /// <summary>
	    ///     Sets the progress bar position, rounded to the nearest step size and clamped to the minimum and maximum values.
	    /// </summary>
	    /// <param name="value">Value.</param>
	    public ProgressBar SetValue(float value)
        {
            if (!ShiftIgnoresSnap || !InputUtils.IsShiftDown())
            {
                value = Mathf.Clamp(Mathf.Round(value / StepSize) * StepSize, Min, Max);
                value = Snap(value);
            }
            else
            {
                Mathf.Clamp(value, Min, Max);
            }

            if (value == Value)
                return this;

            Value = value;

            // fire changed event
            if (OnChanged != null)
                OnChanged(Value);

            return this;
        }


        public ProgressBar SetStepSize(float stepSize)
        {
            StepSize = stepSize;
            return this;
        }


        protected virtual IDrawable GetKnobDrawable()
        {
            return Disabled && _style.DisabledKnob != null ? _style.DisabledKnob : _style.Knob;
        }


        public override void Draw(Graphics.Graphics graphics, float parentAlpha)
        {
            var knob = GetKnobDrawable();
            var bg = Disabled && _style.DisabledBackground != null ? _style.DisabledBackground : _style.Background;
            var knobBefore = Disabled && _style.DisabledKnobBefore != null ? _style.DisabledKnobBefore : _style.KnobBefore;
            var knobAfter = Disabled && _style.DisabledKnobAfter != null ? _style.DisabledKnobAfter : _style.KnobAfter;

            var x = this.X;
            var y = this.Y;
            var width = this.Width;
            var height = this.Height;
            var knobHeight = knob == null ? 0 : knob.MinHeight;
            var knobWidth = knob == null ? 0 : knob.MinWidth;
            var percent = GetVisualPercent();
            var color = new Color(this.Color, (int) (this.Color.A * parentAlpha));

            if (Vertical)
            {
                var positionHeight = height;

                float bgTopHeight = 0;
                if (bg != null)
                {
                    bg.Draw(graphics, x + (int) ((width - bg.MinWidth) * 0.5f), y, bg.MinWidth, height, color);
                    bgTopHeight = bg.TopHeight;
                    positionHeight -= bgTopHeight + bg.BottomHeight;
                }

                float knobHeightHalf = 0;
                if (Min != Max)
                {
                    if (knob == null)
                    {
                        knobHeightHalf = knobBefore == null ? 0 : knobBefore.MinHeight * 0.5f;
                        Position = (positionHeight - knobHeightHalf) * percent;
                        Position = System.Math.Min(positionHeight - knobHeightHalf, Position);
                    }
                    else
                    {
                        var bgBottomHeight = bg != null ? bg.BottomHeight : 0;
                        knobHeightHalf = knobHeight * 0.5f;
                        Position = (positionHeight - knobHeight) * percent;
                        Position = System.Math.Min(positionHeight - knobHeight, Position) + bgBottomHeight;
                    }
                    Position = System.Math.Max(0, Position);
                }

                if (knobBefore != null)
                {
                    float offset = 0;
                    if (bg != null)
                        offset = bgTopHeight;
                    knobBefore.Draw(graphics, x + (width - knobBefore.MinWidth) * 0.5f, y + offset, knobBefore.MinWidth,
                        (int) (Position + knobHeightHalf), color);
                }

                if (knobAfter != null)
                    knobAfter.Draw(graphics, x + (width - knobAfter.MinWidth) * 0.5f, y + Position + knobHeightHalf,
                        knobAfter.MinWidth, height - Position - knobHeightHalf, color);

                if (knob != null)
                    knob.Draw(graphics, x + (int) ((width - knobWidth) * 0.5f), (int) (y + Position), knobWidth,
                        knobHeight, color);
            }
            else
            {
                var positionWidth = width;

                float bgLeftWidth = 0;
                if (bg != null)
                {
                    bg.Draw(graphics, x, y + (int) ((height - bg.MinHeight) * 0.5f), width, bg.MinHeight, color);
                    bgLeftWidth = bg.LeftWidth;
                    positionWidth -= bgLeftWidth + bg.RightWidth;
                }

                float knobWidthHalf = 0;
                if (Min != Max)
                {
                    if (knob == null)
                    {
                        knobWidthHalf = knobBefore == null ? 0 : knobBefore.MinWidth * 0.5f;
                        Position = (positionWidth - knobWidthHalf) * percent;
                        Position = System.Math.Min(positionWidth - knobWidthHalf, Position);
                    }
                    else
                    {
                        knobWidthHalf = knobWidth * 0.5f;
                        Position = (positionWidth - knobWidth) * percent;
                        Position = System.Math.Min(positionWidth - knobWidth, Position) + bgLeftWidth;
                    }
                    Position = System.Math.Max(0, Position);
                }

                if (knobBefore != null)
                {
                    float offset = 0;
                    if (bg != null)
                        offset = bgLeftWidth;
                    knobBefore.Draw(graphics, x + offset, y + (int) ((height - knobBefore.MinHeight) * 0.5f),
                        (int) (Position + knobWidthHalf), knobBefore.MinHeight, color);
                }

                if (knobAfter != null)
                    knobAfter.Draw(graphics, x + (int) (Position + knobWidthHalf),
                        y + (int) ((height - knobAfter.MinHeight) * 0.5f),
                        width - (int) (Position + knobWidthHalf), knobAfter.MinHeight, color);

                if (knob != null)
                    knob.Draw(graphics, (int) (x + Position), (int) (y + (height - knobHeight) * 0.5f), knobWidth,
                        knobHeight, color);
            }
        }


        public float GetVisualPercent()
        {
            return (Value - Min) / (Max - Min);
        }


	    /// <summary>
	    ///     Returns a snapped value
	    /// </summary>
	    /// <param name="value">Value.</param>
	    private float Snap(float value)
        {
            if (SnapValues == null)
                return value;

            for (var i = 0; i < SnapValues.Length; i++)
                if (System.Math.Abs(value - SnapValues[i]) <= SnapThreshold)
                    return SnapValues[i];
            return value;
        }
    }


	/// <summary>
	///     The style for a progress bar
	/// </summary>
	public class ProgressBarStyle
    {
	    /// <summary>
	    ///     The progress bar background, stretched only in one direction. Optional.
	    /// </summary>
	    public IDrawable Background;

	    /// <summary>
	    ///     Optional
	    /// </summary>
	    public IDrawable DisabledBackground;

	    /// <summary>
	    ///     Optional, centered on the background.
	    /// </summary>
	    public IDrawable Knob, DisabledKnob;

	    /// <summary>
	    ///     Optional
	    /// </summary>
	    public IDrawable KnobBefore, KnobAfter, DisabledKnobBefore, DisabledKnobAfter;


        public ProgressBarStyle()
        {
        }


        public ProgressBarStyle(IDrawable background, IDrawable knob)
        {
            this.Background = background;
            this.Knob = knob;
        }


        public static ProgressBarStyle Create(Color knobBeforeColor, Color knobAfterColor)
        {
            var knobBefore = new PrimitiveDrawable(knobBeforeColor);
            knobBefore.MinWidth = knobBefore.MinHeight = 10;

            var knobAfter = new PrimitiveDrawable(knobAfterColor);
            knobAfter.MinWidth = knobAfter.MinHeight = 10;

            return new ProgressBarStyle
            {
                KnobBefore = knobBefore,
                KnobAfter = knobAfter
            };
        }


        public static ProgressBarStyle CreateWithKnob(Color backgroundColor, Color knobColor)
        {
            var background = new PrimitiveDrawable(backgroundColor);
            background.MinWidth = background.MinHeight = 10;

            var knob = new PrimitiveDrawable(knobColor);
            knob.MinWidth = knob.MinHeight = 20;

            return new ProgressBarStyle
            {
                Background = background,
                Knob = knob
            };
        }


        public ProgressBarStyle Clone()
        {
            return new ProgressBarStyle
            {
                Background = Background,
                DisabledBackground = DisabledBackground,
                Knob = Knob,
                DisabledKnob = DisabledKnob,
                KnobBefore = KnobBefore,
                KnobAfter = KnobAfter,
                DisabledKnobBefore = DisabledKnobBefore,
                DisabledKnobAfter = DisabledKnobAfter
            };
        }
    }
}