using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Nez.Tiled
{
    public class TiledImageLayer : TiledLayer
    {
        public readonly Texture2D Texture;

        private RectangleF _bounds;


        public TiledImageLayer(string name, Texture2D texture) : base(name)
        {
            this.Texture = texture;
            _bounds.Width = texture.Width;
            _bounds.Height = texture.Height;
        }


        public void Draw(Batcher batcher)
        {
            batcher.Draw(Texture, Offset, Color.White);
        }


        public override void Draw(Batcher batcher, Vector2 parentPosition, float layerDepth,
            RectangleF cameraClipBounds)
        {
            batcher.Draw(Texture, parentPosition + Offset, null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None,
                layerDepth);
        }
    }
}