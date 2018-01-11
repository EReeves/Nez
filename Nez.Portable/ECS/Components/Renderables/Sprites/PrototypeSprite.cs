using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.Debug.Inspector.Attributes;
using Nez.Maths;

namespace Nez.ECS.Components.Renderables.Sprites
{
	/// <summary>
	///     skewable rectangle sprite for prototyping
	/// </summary>
	public class PrototypeSprite : Sprite
    {
        [Inspectable] private float _skewTopX, _skewBottomX, _skewLeftY, _skewRightY;

        private float _width, _height;


        public PrototypeSprite(float width, float height) : base(Graphics.Graphics.Instance.PixelTexture)
        {
            _width = width;
            _height = height;
            OriginNormalized = new Vector2(0.5f, 0.5f);
        }

        public override float Width => _width;
        public override float Height => _height;

        public override RectangleF Bounds
        {
            get
            {
                if (AreBoundsDirty)
                {
                    Bounds.CalculateBounds(Entity.Transform.Position, localOffset, Origin, Entity.Transform.Scale,
                        Entity.Transform.Rotation, _width, _height);
                    AreBoundsDirty = false;
                }

                return Bounds;
            }
        }

        public float SkewTopX => _skewTopX;
        public float SkewBottomX => _skewBottomX;
        public float SkewLeftY => _skewLeftY;
        public float SkewRightY => _skewRightY;


	    /// <summary>
	    ///     sets the width of the sprite
	    /// </summary>
	    /// <returns>The width.</returns>
	    /// <param name="width">Width.</param>
	    public PrototypeSprite SetWidth(float width)
        {
            _width = width;
            return this;
        }


	    /// <summary>
	    ///     sets the height of the sprite
	    /// </summary>
	    /// <returns>The height.</returns>
	    /// <param name="height">Height.</param>
	    public PrototypeSprite SetHeight(float height)
        {
            _height = height;
            return this;
        }


	    /// <summary>
	    ///     sets the skew values for the sprite
	    /// </summary>
	    /// <returns>The skew.</returns>
	    /// <param name="skewTopX">Skew top x.</param>
	    /// <param name="skewBottomX">Skew bottom x.</param>
	    /// <param name="skewLeftY">Skew left y.</param>
	    /// <param name="skewRightY">Skew right y.</param>
	    public PrototypeSprite SetSkew(float skewTopX, float skewBottomX, float skewLeftY, float skewRightY)
        {
            this._skewTopX = skewTopX;
            this._skewBottomX = skewBottomX;
            this._skewLeftY = skewLeftY;
            this._skewRightY = skewRightY;
            return this;
        }


        public override void Render(Graphics.Graphics graphics, Camera camera)
        {
            var pos = Entity.Transform.Position - Origin * Entity.Transform.LocalScale + ((RenderableComponent) this).LocalOffset;
            var size = new Point((int) (_width * Entity.Transform.LocalScale.X),
                (int) (_height * Entity.Transform.LocalScale.Y));
            var destRect = new Rectangle((int) pos.X, (int) pos.Y, size.X, size.Y);
            graphics.Batcher.Draw(this.Subtexture, destRect, this.Subtexture.SourceRect, Color, Entity.Transform.Rotation,
                SpriteEffects.None, ((RenderableComponent) this).LayerDepth, _skewTopX, _skewBottomX, _skewLeftY, _skewRightY);
        }
    }
}