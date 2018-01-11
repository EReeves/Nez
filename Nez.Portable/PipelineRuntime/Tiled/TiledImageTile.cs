namespace Nez.PipelineRuntime.Tiled
{
    public class TiledImageTile : TiledTile
    {
        public string ImageSource;
        public new TiledTilesetTile TilesetTile;


        public TiledImageTile(int id, TiledTilesetTile tilesetTile, string imageSource) : base(id)
        {
            this.TilesetTile = tilesetTile;
            this.ImageSource = imageSource;
        }
    }
}