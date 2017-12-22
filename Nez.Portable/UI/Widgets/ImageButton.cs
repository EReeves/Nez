namespace Nez.UI
{
	/// <summary>
	///     A button with a child {@link Image} to display an image. This is useful when the button must be larger than the
	///     image and the
	///     image centered on the button. If the image is the size of the button, a {@link Button} without any children can be
	///     used, where
	///     the {@link Button.ButtonStyle#up}, {@link Button.ButtonStyle#down}, and {@link Button.ButtonStyle#checked} nine
	///     patches define
	///     the image.
	/// </summary>
	public class ImageButton : Button
    {
        private readonly Image _image;
        private ImageButtonStyle _style;


        public ImageButton(ImageButtonStyle style) : base(style)
        {
            _image = new Image();
            _image.SetScaling(Scaling.Fit);
            Add(_image);
            SetStyle(style);
            SetSize(PreferredWidth, PreferredHeight);
        }

        public ImageButton(Skin skin, string styleName = null) : this(skin.Get<ImageButtonStyle>(styleName))
        {
        }


        public ImageButton(IDrawable imageUp) : this(new ImageButtonStyle(null, null, null, imageUp, null, null))
        {
        }


        public ImageButton(IDrawable imageUp, IDrawable imageDown) : this(new ImageButtonStyle(null, null, null,
            imageUp, imageDown, null))
        {
        }


        public ImageButton(IDrawable imageUp, IDrawable imageDown, IDrawable imageOver) : this(
            new ImageButtonStyle(null, null, null, imageUp, imageDown, imageOver))
        {
        }


        public override void SetStyle(ButtonStyle style)
        {
            Assert.IsTrue(style is ImageButtonStyle, "style must be a ImageButtonStyle");

            base.SetStyle(style);
            this._style = (ImageButtonStyle) style;
            if (_image != null)
                UpdateImage();
        }


        public new ImageButtonStyle GetStyle()
        {
            return _style;
        }


        public Image GetImage()
        {
            return _image;
        }


        public Cell GetImageCell()
        {
            return GetCell(_image);
        }


        private void UpdateImage()
        {
            IDrawable drawable = null;
            if (isDisabled && _style.ImageDisabled != null)
                drawable = _style.ImageDisabled;
            else if (MouseDown && _style.ImageDown != null)
                drawable = _style.ImageDown;
            else if (isChecked && _style.ImageChecked != null)
                drawable = _style.ImageCheckedOver != null && MouseOver ? _style.ImageCheckedOver : _style.ImageChecked;
            else if (MouseOver && _style.ImageOver != null)
                drawable = _style.ImageOver;
            else if (_style.ImageUp != null) //
                drawable = _style.ImageUp;

            _image.SetDrawable(drawable);
        }


        public override void Draw(Graphics graphics, float parentAlpha)
        {
            UpdateImage();
            base.Draw(graphics, parentAlpha);
        }
    }


    public class ImageButtonStyle : ButtonStyle
    {
        /** Optional. */
        public IDrawable ImageUp, ImageDown, ImageOver, ImageChecked, ImageCheckedOver, ImageDisabled;


        public ImageButtonStyle()
        {
        }


        public ImageButtonStyle(IDrawable up, IDrawable down, IDrawable checkked, IDrawable imageUp,
            IDrawable imageDown, IDrawable imageChecked) : base(up, down, checkked)
        {
            this.ImageUp = imageUp;
            this.ImageDown = imageDown;
            this.ImageChecked = imageChecked;
        }


        public new ImageButtonStyle Clone()
        {
            return new ImageButtonStyle
            {
                Up = Up,
                Down = Down,
                Over = Over,
                Checkked = Checkked,
                CheckedOver = CheckedOver,
                Disabled = Disabled,

                ImageUp = ImageUp,
                ImageDown = ImageDown,
                ImageOver = ImageOver,
                ImageChecked = ImageChecked,
                ImageCheckedOver = ImageCheckedOver,
                ImageDisabled = ImageDisabled
            };
        }
    }
}