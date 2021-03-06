﻿using Nez.Maths;
using Nez.Utils.Extensions;

namespace Nez.ECS.Components.Renderables.Sprites
{
	/// <summary>
	///     this component will draw the same frame of spriteToMime every frame. The only difference in rendering is that
	///     SpriteMime uses its own
	///     localOffset and color. This allows you to use it for the purpose of shadows (by offsetting via localPosition) or
	///     silhouettes (with a
	///     Material that has a stencil read).
	/// </summary>
	public class SpriteMime : RenderableComponent
    {
        private Sprite _spriteToMime;


        public SpriteMime()
        {
        }


        public SpriteMime(Sprite spriteToMime)
        {
            _spriteToMime = spriteToMime;
        }

        public override float Width => _spriteToMime.Width;
        public override float Height => _spriteToMime.Height;
        public override RectangleF Bounds => _spriteToMime.Bounds;


        public override void OnAddedToEntity()
        {
            if (_spriteToMime == null)
                _spriteToMime = this.GetComponent<Sprite>();
        }


        public override void Render(Graphics.Graphics graphics, Camera camera)
        {
            graphics.Batcher.Draw(_spriteToMime.Subtexture, Entity.Transform.Position + localOffset, Color,
                Entity.Transform.Rotation, _spriteToMime.Origin, Entity.Transform.Scale, _spriteToMime.SpriteEffects,
                LayerDepth);
        }
    }
}