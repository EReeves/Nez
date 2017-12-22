using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.BitmapFonts;

namespace Nez.UI
{
    public class Label : Element
    {
        //int lineAlign = AlignInternal.left;
        private string _ellipsis;

        private float _fontScaleX = 1;
        private float _fontScaleY = 1;
        private float _lastPrefHeight;
        private Vector2 _prefSize;
        private bool _prefSizeInvalid;


        // configuration
        private LabelStyle _style;

        private string _text;
        private Vector2 _textPosition;

        // internal state
        private string _wrappedString;

        private bool _wrapText;
        private int _labelAlign = AlignInternal.Left;


        public Label(string text, LabelStyle style)
        {
            SetStyle(style);
            SetText(text);
            Touchable = Touchable.Disabled;
        }


        public Label(string text, Skin skin, string styleName = null) : this(text, skin.Get<LabelStyle>(styleName))
        {
        }


        public Label(string text, BitmapFont font, Color fontColor) : this(text, new LabelStyle(font, fontColor))
        {
        }


        public Label(string text, BitmapFont font) : this(text, font, Color.White)
        {
        }


        public Label(string text) : this(text, Graphics.Instance.BitmapFont)
        {
        }

        public override float PreferredWidth
        {
            get
            {
                if (_wrapText)
                    return 0;

                if (_prefSizeInvalid)
                    ComputePrefSize();

                var w = _prefSize.X;
                if (_style.Background != null)
                    w += _style.Background.LeftWidth + _style.Background.RightWidth;
                return w;
            }
        }

        public override float PreferredHeight
        {
            get
            {
                if (_prefSizeInvalid)
                    ComputePrefSize();

                var h = _prefSize.Y;
                if (_style.Background != null)
                    h += _style.Background.TopHeight + _style.Background.BottomHeight;
                return h;
            }
        }


        public virtual Label SetStyle(LabelStyle style)
        {
            _style = style;
            InvalidateHierarchy();
            return this;
        }


	    /// <summary>
	    ///     Returns the button's style. Modifying the returned style may not have an effect until {@link
	    ///     #setStyle(ButtonStyle)} is called.
	    /// </summary>
	    /// <returns>The style.</returns>
	    public virtual LabelStyle GetStyle()
        {
            return _style;
        }


        public override void Invalidate()
        {
            base.Invalidate();
            _prefSizeInvalid = true;
        }


        private void ComputePrefSize()
        {
            _prefSizeInvalid = false;

            if (_wrapText && _ellipsis == null && Width > 0)
            {
                var widthCalc = Width;
                if (_style.Background != null)
                    widthCalc -= _style.Background.LeftWidth + _style.Background.RightWidth;

                _wrappedString = _style.Font.WrapText(_text, widthCalc / _fontScaleX);
            }
            else
            {
                _wrappedString = _text;
            }

            _prefSize = _style.Font.MeasureString(_wrappedString) * new Vector2(_fontScaleX, _fontScaleY);
        }


        public override void Layout()
        {
            if (_prefSizeInvalid)
                ComputePrefSize();

            var isWrapped = _wrapText && _ellipsis == null;
            if (isWrapped)
                if (_lastPrefHeight != PreferredHeight)
                {
                    _lastPrefHeight = PreferredHeight;
                    InvalidateHierarchy();
                }

            var width = this.Width;
            var height = this.Height;
            _textPosition.X = 0;
            _textPosition.Y = 0;
            // TODO: explore why descent causes mis-alignment
            //_textPosition.Y =_style.font.descent;
            if (_style.Background != null)
            {
                _textPosition.X = _style.Background.LeftWidth;
                _textPosition.Y = _style.Background.TopHeight;
                width -= _style.Background.LeftWidth + _style.Background.RightWidth;
                height -= _style.Background.TopHeight + _style.Background.BottomHeight;
            }

            float textWidth, textHeight;
            if (isWrapped || _wrappedString.IndexOf('\n') != -1)
            {
                // If the text can span multiple lines, determine the text's actual size so it can be aligned within the label.
                textWidth = _prefSize.X;
                textHeight = _prefSize.Y;

                if ((_labelAlign & AlignInternal.Left) == 0)
                    if ((_labelAlign & AlignInternal.Right) != 0)
                        _textPosition.X += width - textWidth;
                    else
                        _textPosition.X += (width - textWidth) / 2;
            }
            else
            {
                textWidth = width;
                textHeight = _style.Font.LineHeight * _fontScaleY;
            }

            if ((_labelAlign & AlignInternal.Bottom) != 0)
            {
                _textPosition.Y += height - textHeight;
                Y += _style.Font.Descent;
            }
            else if ((_labelAlign & AlignInternal.Top) != 0)
            {
                _textPosition.Y += 0;
                Y -= _style.Font.Descent;
            }
            else
            {
                _textPosition.Y += (height - textHeight) / 2;
            }

            //_textPosition.Y += textHeight;

            // if we have GlyphLayout this code is redundant
            if ((_labelAlign & AlignInternal.Left) != 0)
                _textPosition.X = 0;
            else if (_labelAlign == AlignInternal.Center)
                _textPosition.X = width / 2 - _prefSize.X / 2; // center of width - center of text size
            else
                _textPosition.X = width - _prefSize.X; // full width - our text size
        }


        public override void Draw(Graphics graphics, float parentAlpha)
        {
            Validate();

            var color = new Color(this.Color, (int) (this.Color.A * parentAlpha));
            if (_style.Background != null)
                _style.Background.Draw(graphics, X, Y, Width == 0 ? _prefSize.X : Width, Height, color);

            graphics.Batcher.DrawString(_style.Font, _wrappedString, new Vector2(X, Y) + _textPosition,
                _style.FontColor, 0, Vector2.Zero, new Vector2(_fontScaleX, _fontScaleY), SpriteEffects.None, 0);
        }


        #region Configuration

        public Label SetText(string text)
        {
            if (_text != text)
            {
                _wrappedString = null;
                _text = text;
                _prefSizeInvalid = true;
                InvalidateHierarchy();
            }
            return this;
        }


        public string GetText()
        {
            return _text;
        }


	    /// <summary>
	    ///     background may be null to clear the background.
	    /// </summary>
	    /// <returns>this</returns>
	    /// <param name="background">Background.</param>
	    public Label SetBackground(IDrawable background)
        {
            _style.Background = background;
            Invalidate();
            return this;
        }


	    /// <summary>
	    ///     alignment Aligns all the text within the label (default left center) and each line of text horizontally (default
	    ///     left)
	    /// </summary>
	    /// <param name="alignment">Alignment.</param>
	    public Label SetAlignment(Align alignment)
        {
            return SetAlignment(alignment, alignment);
        }


	    /// <summary>
	    ///     labelAlign Aligns all the text within the label (default left center).
	    ///     lineAlign Aligns each line of text horizontally (default left).
	    /// </summary>
	    /// <param name="labelAlign">Label align.</param>
	    /// <param name="lineAlign">Line align.</param>
	    public Label SetAlignment(Align labelAlign, Align lineAlign)
        {
            this._labelAlign = (int) labelAlign;

            // TODO
//			var tempLineAlign = (int)lineAlign;
//			if( ( tempLineAlign & AlignInternal.left ) != 0 )
//				this.lineAlign = AlignInternal.left;
//			else if( ( tempLineAlign & AlignInternal.right ) != 0 )
//				this.lineAlign = AlignInternal.right;
//			else
//				this.lineAlign = AlignInternal.center;

            Invalidate();
            return this;
        }


        public Label SetFontColor(Color color)
        {
            _style.FontColor = color;
            return this;
        }


        public Label SetFontScale(float fontScale)
        {
            _fontScaleX = fontScale;
            _fontScaleY = fontScale;
            InvalidateHierarchy();
            return this;
        }


        public Label SetFontScale(float fontScaleX, float fontScaleY)
        {
            _fontScaleX = fontScaleX;
            _fontScaleY = fontScaleY;
            InvalidateHierarchy();
            return this;
        }


	    /// <summary>
	    ///     When non-null the text will be truncated "..." if it does not fit within the width of the label. Wrapping will not
	    ///     occur
	    ///     when ellipsis is enabled. Default is false.
	    /// </summary>
	    /// <param name="ellipsis">Ellipsis.</param>
	    public Label SetEllipsis(string ellipsis)
        {
            _ellipsis = ellipsis;
            return this;
        }


	    /// <summary>
	    ///     When true the text will be truncated "..." if it does not fit within the width of the label. Wrapping will not
	    ///     occur when
	    ///     ellipsis is true. Default is false.
	    /// </summary>
	    /// <param name="ellipsis">Ellipsis.</param>
	    public Label SetEllipsis(bool ellipsis)
        {
            if (ellipsis)
                _ellipsis = "...";
            else
                _ellipsis = null;
            return this;
        }


	    /// <summary>
	    ///     should the text be wrapped?
	    /// </summary>
	    /// <param name="shouldWrap">If set to <c>true</c> should wrap.</param>
	    public Label SetWrap(bool shouldWrap)
        {
            _wrapText = shouldWrap;
            InvalidateHierarchy();
            return this;
        }

        #endregion
    }


	/// <summary>
	///     the style for a label
	/// </summary>
	public class LabelStyle
    {
        public IDrawable Background;
        public BitmapFont Font;
        public Color FontColor = Color.White;


        public LabelStyle()
        {
            Font = Graphics.Instance.BitmapFont;
        }


        public LabelStyle(BitmapFont font, Color fontColor)
        {
            this.Font = font ?? Graphics.Instance.BitmapFont;
            this.FontColor = fontColor;
        }


        public LabelStyle(Color fontColor) : this(null, fontColor)
        {
        }


        public LabelStyle Clone()
        {
            return new LabelStyle
            {
                FontColor = FontColor,
                Font = Font,
                Background = Background
            };
        }
    }
}