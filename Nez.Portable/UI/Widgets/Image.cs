using Microsoft.Xna.Framework.Graphics;
using Nez.Textures;

namespace Nez.UI
{
    public class Image : Element
    {
        private int _align;

        private IDrawable _drawable;
        private Scaling _scaling;
        private float _imageX, _imageY, _imageWidth, _imageHeight;


        public Image(IDrawable drawable, Scaling scaling = Scaling.Stretch, int align = AlignInternal.Center)
        {
            SetDrawable(drawable);
            _scaling = scaling;
            _align = align;
            SetSize(PreferredWidth, PreferredHeight);
            Touchable = Touchable.Disabled;
        }


        public Image() : this((IDrawable) null)
        {
        }


        public Image(Subtexture subtexture, Scaling scaling = Scaling.Stretch, int align = AlignInternal.Center) : this(
            new SubtextureDrawable(subtexture), scaling, align)
        {
        }


        public Image(Texture2D texture, Scaling scaling = Scaling.Stretch, int align = AlignInternal.Center) : this(
            new Subtexture(texture), scaling, align)
        {
        }


        public override void Draw(Graphics graphics, float parentAlpha)
        {
            Validate();

            var col = Color * (Color.A / 255f);

            //			if( drawable instanceof TransformDrawable )
            //			{
            //				float rotation = getRotation();
            //				if (scaleX != 1 || scaleY != 1 || rotation != 0)
            //				{
            //					((TransformDrawable)drawable).draw(batch, x + imageX, y + imageY, getOriginX() - imageX, getOriginY() - imageY,
            //						imageWidth, imageHeight, scaleX, scaleY, rotation);
            //					return;
            //				}
            //			}

            if (_drawable != null)
                _drawable.Draw(graphics, X + _imageX, Y + _imageY, _imageWidth * ScaleX, _imageHeight * ScaleY, col);
        }


        public override void Layout()
        {
            if (_drawable == null)
                return;

            var regionWidth = _drawable.MinWidth;
            var regionHeight = _drawable.MinHeight;

            var size = _scaling.Apply(regionWidth, regionHeight, Width, Height);
            _imageWidth = size.X;
            _imageHeight = size.Y;

            if ((_align & AlignInternal.Left) != 0)
                _imageX = 0;
            else if ((_align & AlignInternal.Right) != 0)
                _imageX = (int) (Width - _imageWidth);
            else
                _imageX = (int) (Width / 2 - _imageWidth / 2);

            if ((_align & AlignInternal.Top) != 0)
                _imageY = (int) (Height - _imageHeight);
            else if ((_align & AlignInternal.Bottom) != 0)
                _imageY = 0;
            else
                _imageY = (int) (Height / 2 - _imageHeight / 2);
        }


        #region Configuration

        public Image SetDrawable(IDrawable drawable)
        {
            if (_drawable != drawable)
            {
                if (_drawable != null)
                {
                    if (PreferredWidth != drawable.MinWidth || PreferredHeight != drawable.MinHeight)
                        InvalidateHierarchy();
                }
                else
                {
                    InvalidateHierarchy();
                }
                _drawable = drawable;
            }

            return this;
        }


	    /// <summary>
	    /// </summary>
	    /// <param name="alignment">Alignment.</param>
	    public Image SetAlignment(Align alignment)
        {
            _align = (int) alignment;
            return this;
        }


        public Image SetScaling(Scaling scaling)
        {
            _scaling = scaling;
            return this;
        }

        #endregion


        #region ILayout

        public override float PreferredWidth => _drawable != null ? _drawable.MinWidth : 0;

        public override float PreferredHeight => _drawable != null ? _drawable.MinHeight : 0;

        #endregion
    }
}