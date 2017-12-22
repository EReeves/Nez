using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.Textures;

namespace Nez.Tiled
{
    public class TiledImageCollectionTileset : TiledTileset
    {
        public TiledImageCollectionTileset(Texture2D texture, int firstId) : base(texture, firstId)
        {
        }


        public void SetTileTextureRegion(int tileId, Rectangle sourceRect)
        {
            Regions[tileId] = new Subtexture(Texture, sourceRect);
        }
    }
}