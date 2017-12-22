using Microsoft.Xna.Framework;
using Nez.BitmapFonts;

namespace Nez.UI
{
    public class TextButton : Button
    {
        private readonly Label _label;
        private TextButtonStyle _style;


        public TextButton(string text, TextButtonStyle style) : base(style)
        {
            SetStyle(style);
            _label = new Label(text, style.Font, style.FontColor);
            _label.SetAlignment(UI.Align.Center);

            Add(_label).Expand().Fill();
            SetSize(PreferredWidth, PreferredHeight);
        }


        public TextButton(string text, Skin skin, string styleName = null) : this(text,
            skin.Get<TextButtonStyle>(styleName))
        {
        }


        public override void SetStyle(ButtonStyle style)
        {
            Assert.IsTrue(style is TextButtonStyle, "style must be a TextButtonStyle");

            base.SetStyle(style);
            this._style = (TextButtonStyle) style;

            if (_label != null)
            {
                var textButtonStyle = (TextButtonStyle) style;
                var labelStyle = _label.GetStyle();
                labelStyle.Font = textButtonStyle.Font;
                labelStyle.FontColor = textButtonStyle.FontColor;
                _label.SetStyle(labelStyle);
            }
        }


        public new TextButtonStyle GetStyle()
        {
            return _style;
        }


        public override void Draw(Graphics graphics, float parentAlpha)
        {
            Color? fontColor = null;
            if (isDisabled && _style.DisabledFontColor.HasValue)
                fontColor = _style.DisabledFontColor;
            else if (MouseDown && _style.DownFontColor.HasValue)
                fontColor = _style.DownFontColor;
            else if (isChecked &&
                     (!MouseOver && _style.CheckedFontColor.HasValue ||
                      MouseOver && _style.CheckedOverFontColor.HasValue))
                fontColor = MouseOver && _style.CheckedOverFontColor.HasValue
                    ? _style.CheckedOverFontColor
                    : _style.CheckedFontColor;
            else if (MouseOver && _style.OverFontColor.HasValue)
                fontColor = _style.OverFontColor;
            else
                fontColor = _style.FontColor;

            if (fontColor != null)
                _label.GetStyle().FontColor = fontColor.Value;

            base.Draw(graphics, parentAlpha);
        }


        public Label GetLabel()
        {
            return _label;
        }


        public Cell GetLabelCell()
        {
            return GetCell(_label);
        }


        public TextButton SetText(string text)
        {
            _label.SetText(text);
            return this;
        }


        public string GetText()
        {
            return _label.GetText();
        }


        public override string ToString()
        {
            return string.Format("[TextButton] text: {0}", GetText());
        }
    }


	/// <summary>
	///     The style for a text button
	/// </summary>
	public class TextButtonStyle : ButtonStyle
    {
        public Color? DownFontColor, OverFontColor, CheckedFontColor, CheckedOverFontColor, DisabledFontColor;
        public BitmapFont Font;

        /** Optional. */
        public Color FontColor = Color.White;


        public TextButtonStyle()
        {
            Font = Graphics.Instance.BitmapFont;
        }


        public TextButtonStyle(IDrawable up, IDrawable down, IDrawable over, BitmapFont font) : base(up, down, over)
        {
            this.Font = font ?? Graphics.Instance.BitmapFont;
        }


        public TextButtonStyle(IDrawable up, IDrawable down, IDrawable over) : this(up, down, over,
            Graphics.Instance.BitmapFont)
        {
        }


        public new static TextButtonStyle Create(Color upColor, Color downColor, Color overColor)
        {
            return new TextButtonStyle
            {
                Up = new PrimitiveDrawable(upColor),
                Down = new PrimitiveDrawable(downColor),
                Over = new PrimitiveDrawable(overColor)
            };
        }


        public new TextButtonStyle Clone()
        {
            return new TextButtonStyle
            {
                Up = Up,
                Down = Down,
                Over = Over,
                Checkked = Checkked,
                CheckedOver = CheckedOver,
                Disabled = Disabled,

                Font = Font,
                FontColor = FontColor,
                DownFontColor = DownFontColor,
                OverFontColor = OverFontColor,
                CheckedFontColor = CheckedFontColor,
                CheckedOverFontColor = CheckedOverFontColor,
                DisabledFontColor = DisabledFontColor
            };
        }
    }
}