using Microsoft.Xna.Framework;
using Nez;
using Nez.Debug;
using Nez.UI.Base;
using IDrawable = Nez.UI.Drawable.IDrawable;

namespace Nez.UI.Widgets
{
	/// <summary>
	///     A checkbox is a button that contains an image indicating the checked or unchecked state and a label
	/// </summary>
	public class CheckBox : TextButton
    {
        private readonly Image _image;
        private readonly Cell _imageCell;
        private CheckBoxStyle _style;


        public CheckBox(string text, CheckBoxStyle style) : base(text, style)
        {
            ClearChildren();
            var label = GetLabel();
            _imageCell = Add(_image = new Image(style.CheckboxOff));
            Add(label);
            label.SetAlignment(Base.Align.Left);
            GetLabelCell().SetPadLeft(10);
            SetSize(PreferredWidth, PreferredHeight);
        }


        public CheckBox(string text, Skin skin, string styleName = null) : this(text,
            skin.Get<CheckBoxStyle>(styleName))
        {
        }


        public override void SetStyle(ButtonStyle style)
        {
            Assert.IsTrue(style is CheckBoxStyle, "style must be a CheckBoxStyle");
            base.SetStyle(style);
            this._style = (CheckBoxStyle) style;
        }


	    /// <summary>
	    ///     Returns the checkbox's style. Modifying the returned style may not have an effect until {@link
	    ///     #setStyle(ButtonStyle)} is called
	    /// </summary>
	    /// <returns>The style.</returns>
	    public new CheckBoxStyle GetStyle()
        {
            return _style;
        }


        public override void Draw(Graphics.Graphics graphics, float parentAlpha)
        {
            IDrawable checkbox = null;
            if (isDisabled)
                if (((Button) this).IsChecked && _style.CheckboxOnDisabled != null)
                    checkbox = _style.CheckboxOnDisabled;
                else
                    checkbox = _style.CheckboxOffDisabled;

            if (checkbox == null)
                if (((Button) this).IsChecked && _style.CheckboxOn != null)
                    checkbox = _style.CheckboxOn;
                else if (MouseOver && _style.CheckboxOver != null && !isDisabled)
                    checkbox = _style.CheckboxOver;
                else
                    checkbox = _style.CheckboxOff;

            _image.SetDrawable(checkbox);
            base.Draw(graphics, parentAlpha);
        }


        public Image GetImage()
        {
            return _image;
        }


        public Cell GetImageCell()
        {
            return _imageCell;
        }
    }


	/// <summary>
	///     The style for a select box
	/// </summary>
	public class CheckBoxStyle : TextButtonStyle
    {
        public IDrawable CheckboxOn, CheckboxOff;

        /** Optional. */
        public IDrawable CheckboxOver, CheckboxOnDisabled, CheckboxOffDisabled;


        public CheckBoxStyle()
        {
            Font = Graphics.Graphics.Instance.BitmapFont;
        }


        public CheckBoxStyle(IDrawable checkboxOff, IDrawable checkboxOn, BitmapFont font, Color fontColor)
        {
            this.CheckboxOff = checkboxOff;
            this.CheckboxOn = checkboxOn;
            this.Font = font ?? Graphics.Graphics.Instance.BitmapFont;
            this.FontColor = fontColor;
        }
    }
}