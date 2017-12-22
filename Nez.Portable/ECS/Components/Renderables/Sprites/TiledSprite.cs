using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.Sprites;
using Nez.Textures;

namespace Nez
{
	/// <summary>
	///     Tiled sprite. Note that TiledSprite overrides the Material so that it can wrap the UVs. This class requires the
	///     texture
	///     to not be part of an atlas so that wrapping can work.
	/// </summary>
	public class TiledSprite : Sprite
    {
        private Vector2 _inverseTexScale = Vector2.One;

	    /// <summary>
	    ///     we keep a copy of the sourceRect so that we dont change the Subtexture in case it is used elsewhere
	    /// </summary>
	    protected Rectangle SourceRect;

        private Vector2 _textureScale = Vector2.One;


        public TiledSprite(Subtexture subtexture) : base(subtexture)
        {
            SourceRect = subtexture.SourceRect;
            Material = new Material
            {
                SamplerState = Core.DefaultWrappedSamplerState
            };
        }


        public TiledSprite(Texture2D texture) : this(new Subtexture(texture))
        {
        }

	    /// <summary>
	    ///     x value of the texture scroll
	    /// </summary>
	    /// <value>The scroll x.</value>
	    public int ScrollX
        {
            get => SourceRect.X;
            set => SourceRect.X = value;
        }

	    /// <summary>
	    ///     y value of the texture scroll
	    /// </summary>
	    /// <value>The scroll y.</value>
	    public int ScrollY
        {
            get => SourceRect.Y;
            set => SourceRect.Y = value;
        }

	    /// <summary>
	    ///     scale of the texture
	    /// </summary>
	    /// <value>The texture scale.</value>
	    public Vector2 TextureScale
        {
            get => _textureScale;
            set
            {
                _textureScale = value;

                // recalulcate our inverseTextureScale and the source rect size
                _inverseTexScale = new Vector2(1f / _textureScale.X, 1f / _textureScale.Y);
                SourceRect.Width = (int) (subtexture.SourceRect.Width * _inverseTexScale.X);
                SourceRect.Height = (int) (subtexture.SourceRect.Height * _inverseTexScale.Y);
            }
        }

	    /// <summary>
	    ///     overridden width value so that the TiledSprite can have an independent width than its texture
	    /// </summary>
	    /// <value>The width.</value>
	    public new int Width
        {
            get => SourceRect.Width;
            set => SourceRect.Width = value;
        }

	    /// <summary>
	    ///     overridden height value so that the TiledSprite can have an independent height than its texture
	    /// </summary>
	    /// <value>The height.</value>
	    public new int Height
        {
            get => SourceRect.Height;
            set => SourceRect.Height = value;
        }


        public override void Render(Graphics graphics, Camera camera)
        {
            var topLeft = Entity.Transform.Position + localOffset;
            var destinationRect = RectangleExt.FromFloats(topLeft.X, topLeft.Y,
                SourceRect.Width * Entity.Transform.Scale.X * TextureScale.X,
                SourceRect.Height * Entity.Transform.Scale.Y * TextureScale.Y);

            graphics.Batcher.Draw(subtexture, destinationRect, SourceRect, Color, Entity.Transform.Rotation,
                origin * _inverseTexScale, SpriteEffects, LayerDepth);
        }
    }
}