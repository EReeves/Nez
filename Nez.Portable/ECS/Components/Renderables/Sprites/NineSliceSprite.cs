using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.Graphics.Textures;
using Nez.Maths;

namespace Nez.ECS.Components.Renderables.Sprites
{
    public class NineSliceSprite : Sprite
    {
        private readonly Rectangle[] _destRects = new Rectangle[9];
        private bool _destRectsDirty = true;


	    /// <summary>
	    ///     full area in which we will be rendering
	    /// </summary>
	    private Rectangle _finalRenderRect;

        public new NinePatchSubtexture Subtexture;


        public NineSliceSprite(NinePatchSubtexture subtexture) : base(subtexture)
        {
            this.Subtexture = subtexture;
        }


        public NineSliceSprite(Subtexture subtexture, int top, int bottom, int left, int right) : this(
            new NinePatchSubtexture(subtexture, left, right, top, bottom))
        {
        }


        public NineSliceSprite(Texture2D texture, int top, int bottom, int left, int right) : this(
            new NinePatchSubtexture(texture, left, right, top, bottom))
        {
        }

        public new float Width
        {
            get => _finalRenderRect.Width;
            set
            {
                _finalRenderRect.Width = (int) value;
                _destRectsDirty = true;
            }
        }

        public new float Height
        {
            get => _finalRenderRect.Height;
            set
            {
                _finalRenderRect.Height = (int) value;
                _destRectsDirty = true;
            }
        }

        protected RectangleF bounds;
        public override RectangleF Bounds
        {
            get
            {
                if (AreBoundsDirty)
                {
                    bounds.Location = Entity.Transform.Position + LocalOffset;
                    bounds.Width = Width;
                    bounds.Height = Height;
                    AreBoundsDirty = false;
                }

                return bounds;
            }
        }


        public override void Render(Graphics.Graphics graphics, Camera camera)
        {
            if (_destRectsDirty)
            {
                Subtexture.GenerateNinePatchRects(_finalRenderRect, _destRects, Subtexture.Left, Subtexture.Right,
                    Subtexture.Top, Subtexture.Bottom);
                _destRectsDirty = false;
            }

            var pos = (Entity.Transform.Position + localOffset).ToPoint();

            for (var i = 0; i < 9; i++)
            {
                // shift our destination rect over to our position
                var dest = _destRects[i];
                dest.X += pos.X;
                dest.Y += pos.Y;
                graphics.Batcher.Draw(Subtexture, dest, Subtexture.NinePatchRects[i], Color);
            }
        }
    }
}