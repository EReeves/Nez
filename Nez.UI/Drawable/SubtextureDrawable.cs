using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.Graphics.Textures;
using Nez.Utils.Extensions;

namespace Nez.UI.Drawable
{
	/// <summary>
	///     Drawable for a {@link Subtexture}
	/// </summary>
	public class SubtextureDrawable : IDrawable
    {
        protected Subtexture subtexture;

        public SpriteEffects SpriteEffects = SpriteEffects.None;
        public Color? TintColor;


        public SubtextureDrawable(Subtexture subtexture)
        {
            this.subtexture = subtexture;
        }


        public SubtextureDrawable(Texture2D texture) : this(new Subtexture(texture))
        {
        }

	    /// <summary>
	    ///     determines if the sprite should be rendered normally or flipped horizontally
	    /// </summary>
	    /// <value><c>true</c> if flip x; otherwise, <c>false</c>.</value>
	    public bool FlipX
        {
            get => (SpriteEffects & SpriteEffects.FlipHorizontally) == SpriteEffects.FlipHorizontally;
            set => SpriteEffects = value
                ? SpriteEffects | SpriteEffects.FlipHorizontally
                : SpriteEffects & ~SpriteEffects.FlipHorizontally;
        }

	    /// <summary>
	    ///     determines if the sprite should be rendered normally or flipped vertically
	    /// </summary>
	    /// <value><c>true</c> if flip y; otherwise, <c>false</c>.</value>
	    public bool FlipY
        {
            get => (SpriteEffects & SpriteEffects.FlipVertically) == SpriteEffects.FlipVertically;
            set => SpriteEffects = value
                ? SpriteEffects | SpriteEffects.FlipVertically
                : SpriteEffects & ~SpriteEffects.FlipVertically;
        }

        public Subtexture Subtexture
        {
            get => subtexture;
            set
            {
                subtexture = value;
                MinWidth = subtexture.SourceRect.Width;
                MinHeight = subtexture.SourceRect.Height;
            }
        }


        public virtual void Draw(Graphics.Graphics graphics, float x, float y, float width, float height, Color color)
        {
            if (TintColor.HasValue)
                color = color.Multiply(TintColor.Value);

            graphics.Batcher.Draw(Subtexture, new Rectangle((int) x, (int) y, (int) width, (int) height),
                Subtexture.SourceRect, color, SpriteEffects);
        }


	    /// <summary>
	    ///     returns a new drawable with the tint color specified
	    /// </summary>
	    /// <returns>The tinted drawable.</returns>
	    /// <param name="tint">Tint.</param>
	    public SubtextureDrawable NewTintedDrawable(Color tint)
        {
            return new SubtextureDrawable(Subtexture)
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