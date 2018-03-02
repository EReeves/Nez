using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.Graphics.Textures;
using Nez.Utils.Extensions;

namespace Nez.UI.Drawable
{
	/// <summary>
	///     The drawable sizes are set when the ninepatch is set, but they are separate values. Eg, {@link
	///     Drawable#getLeftWidth()} could
	///     be set to more than {@link NinePatch#getLeftWidth()} in order to provide more space on the left than actually
	///     exists in the
	///     ninepatch.
	///     The min size is set to the ninepatch total size by default. It could be set to the left+right and top+bottom,
	///     excluding the
	///     middle size, to allow the drawable to be sized down as small as possible.
	/// </summary>
	public class NinePatchDrawable : IDrawable
    {
        private const int TopLeft = 0;
        private const int TopCenter = 1;
        private const int TopRight = 2;
        private const int MiddleLeft = 3;
        private const int MiddleCenter = 4;
        private const int MiddleRight = 5;
        private const int BottomLeft = 6;
        private const int BottomCenter = 7;
        private const int BottomRight = 8;
        private readonly Rectangle[] _destRects = new Rectangle[9];

	    /// <summary>
	    ///     full area in which we will be rendering
	    /// </summary>
	    private Rectangle _finalRenderRect;

        private readonly NinePatchSubtexture _subtexture;

        public Color? TintColor;


        public NinePatchDrawable(NinePatchSubtexture subtexture)
        {
            _subtexture = subtexture;
            MinWidth = _subtexture.NinePatchRects[MiddleLeft].Width + _subtexture.NinePatchRects[MiddleCenter].Width +
                       _subtexture.NinePatchRects[MiddleRight].Width;
            MinHeight = _subtexture.NinePatchRects[TopCenter].Height +
                        _subtexture.NinePatchRects[MiddleCenter].Height +
                        _subtexture.NinePatchRects[BottomCenter].Height;

            // by default, if padding isn't given, we will pad the content by the nine patch margins
            if (_subtexture.HasPadding)
            {
                LeftWidth = _subtexture.PadLeft;
                RightWidth = _subtexture.PadRight;
                TopHeight = _subtexture.PadTop;
                BottomHeight = _subtexture.PadBottom;
            }
            else
            {
                LeftWidth = _subtexture.Left;
                RightWidth = _subtexture.Right;
                TopHeight = _subtexture.Top;
                BottomHeight = _subtexture.Bottom;
            }
        }


	    /// <summary>
	    ///     creates a NinePatchDrawable using the full texture
	    /// </summary>
	    /// <param name="texture">Texture.</param>
	    /// <param name="left">Left.</param>
	    /// <param name="right">Right.</param>
	    /// <param name="top">Top.</param>
	    /// <param name="bottom">Bottom.</param>
	    public NinePatchDrawable(Texture2D texture, int left, int right, int top, int bottom) : this(
            new NinePatchSubtexture(texture, left, right, top, bottom))
        {
        }


        public NinePatchDrawable(Subtexture subtexture, int left, int right, int top, int bottom) : this(
            new NinePatchSubtexture(subtexture.Texture2D, subtexture.SourceRect, left, right, top, bottom))
        {
        }


        public void Draw(Graphics.Graphics graphics, float x, float y, float width, float height, Color color)
        {
            if (TintColor.HasValue)
                color = color.Multiply(TintColor.Value);

            if (_finalRenderRect.Height != height || _finalRenderRect.Width != width)
            {
                _finalRenderRect.Height = (int) height;
                _finalRenderRect.Width = (int) width;
                _subtexture.GenerateNinePatchRects(_finalRenderRect, _destRects, _subtexture.Left, _subtexture.Right,
                    _subtexture.Top, _subtexture.Bottom);
            }

            for (var i = 0; i < 9; i++)
            {
                // only draw if we have width/height to draw.
                if (_destRects[i].Width == 0 || _destRects[i].Height == 0)
                    continue;

                // shift our destination rect over to our position
                var dest = _destRects[i];
                dest.X += (int) x;
                dest.Y += (int) y;
                graphics.Batcher.Draw(_subtexture, dest, _subtexture.NinePatchRects[i], color);
            }
        }


	    /// <summary>
	    ///     sets the padding on the NinePatchSubtexture
	    /// </summary>
	    /// <param name="left">Left.</param>
	    /// <param name="right">Right.</param>
	    /// <param name="top">Top.</param>
	    /// <param name="bottom">Bottom.</param>
	    public void SetPadding(int left, int right, int top, int bottom)
        {
            _subtexture.Left = left;
            _subtexture.Right = right;
            _subtexture.Top = top;
            _subtexture.Bottom = bottom;
        }


	    /// <summary>
	    ///     returns a new drawable with the tint color specified
	    /// </summary>
	    /// <returns>The tinted drawable.</returns>
	    /// <param name="tint">Tint.</param>
	    public NinePatchDrawable NewTintedDrawable(Color tint)
        {
            return new NinePatchDrawable(_subtexture)
            {
                LeftWidth = LeftWidth,
                RightWidth = RightWidth,
                TopHeight = TopHeight,
                BottomHeight = BottomHeight,
                MinWidth = MinWidth,
                MinHeight = MinHeight,
                TintColor = tint
            };
        }

        #region IDrawable implementation

        public float LeftWidth { get; set; }
        public float RightWidth { get; set; }
        public float TopHeight { get; set; }
        public float BottomHeight { get; set; }
        public float MinWidth { get; set; }
        public float MinHeight { get; set; }


        public void SetPadding(float top, float bottom, float left, float right)
        {
            TopHeight = top;
            BottomHeight = bottom;
            LeftWidth = left;
            RightWidth = right;
        }

        #endregion
    }
}