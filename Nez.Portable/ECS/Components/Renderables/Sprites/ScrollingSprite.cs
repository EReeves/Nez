using Microsoft.Xna.Framework.Graphics;
using Nez.Textures;

namespace Nez.Sprites
{
	/// <summary>
	///     Scrolling sprite. Note that ScrollingSprite overrides the Material so that it can wrap the UVs. This class requires
	///     the texture
	///     to not be part of an atlas so that wrapping can work.
	/// </summary>
	public class ScrollingSprite : TiledSprite, IUpdatable
    {
        // accumulate scroll in a separate float so that we can round it without losing precision for small scroll speeds
        private float _scrollX, _scrollY;

	    /// <summary>
	    ///     x speed of automatic scrolling
	    /// </summary>
	    public float ScrollSpeedX = 0;

	    /// <summary>
	    ///     y speed of automatic scrolling
	    /// </summary>
	    public float ScrollSpeedY = 0;


        public ScrollingSprite(Subtexture subtexture) : base(subtexture)
        {
        }


        public ScrollingSprite(Texture2D texture) : this(new Subtexture(texture))
        {
        }


        void IUpdatable.Update()
        {
            _scrollX += ScrollSpeedX * Time.DeltaTime;
            _scrollY += ScrollSpeedY * Time.DeltaTime;
            SourceRect.X = (int) _scrollX;
            SourceRect.Y = (int) _scrollY;
        }
    }
}