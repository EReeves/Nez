using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.Graphics.Batcher;
using Nez.Maths;
using Nez.Utils;

namespace Nez.PipelineRuntime.Tiled
{
    public class TiledIsometricTiledLayer : TiledLayer
    {
        public readonly TiledMap TiledMap;
        public readonly TiledTile[] Tiles;
        public Color Color = Color.White;
        public int Height;
        public int Width;


        public TiledIsometricTiledLayer(TiledMap map, string name, int width, int height, TiledTile[] tiles) :
            base(name)
        {
            this.Width = width;
            this.Height = height;
            this.Tiles = tiles;

            TiledMap = map;
            tiles = PopulateTilePositions();
        }


        public TiledIsometricTiledLayer(TiledMap map, string name, int width, int height) : this(map, name, width,
            height, new TiledTile[width * height])
        {
        }


        public int TileWidth => TiledMap.TileWidth;

        public int TileHeight => TiledMap.TileHeight;

	    /// <summary>
	    ///     loops through the tiles and sets each tiles x/y value
	    /// </summary>
	    /// <returns>The tile positions.</returns>
	    private TiledTile[] PopulateTilePositions()
        {
            for (var y = 0; y < Height; y++)
            for (var x = 0; x < Width; x++)
                if (Tiles[x + y * Width] != null)
                {
                    Tiles[x + y * Width].X = x;
                    Tiles[x + y * Width].Y = y;
                }

            return Tiles;
        }


        public override void Draw(Batcher batcher, Vector2 position, float layerDepth, RectangleF cameraClipBounds)
        {
            // offset render position by the layer offset, used for isometric layer depth
            position += Offset;

            // offset it by the entity position since the tilemap will always expect positions in its own coordinate space
            cameraClipBounds.Location -= position;
            if (TiledMap.RequiresLargeTileCulling)
            {
                // we expand our cameraClipBounds by the excess tile width/height of the largest tiles to ensure we include tiles whose
                // origin might be outside of the cameraClipBounds
                cameraClipBounds.Location -= new Vector2(TiledMap.LargestTileWidth,
                    TiledMap.LargestTileHeight - TiledMap.TileHeight);
                cameraClipBounds.Size += new Vector2(TiledMap.LargestTileWidth,
                    TiledMap.LargestTileHeight - TiledMap.TileHeight);
            }

            Point min, max;
            min = new Point(0, 0);
            max = new Point(TiledMap.Width - 1, TiledMap.Height - 1);

            // loop through and draw all the non-culled tiles
            for (var y = min.Y; y <= max.Y; y++)
            for (var x = min.X; x <= max.X; x++)
            {
                var tile = GetTile(x, y);
                if (tile == null)
                    continue;

                var tileworldpos = TiledMap.IsometricTileToWorldPosition(x, y);
                if (tileworldpos.X < cameraClipBounds.Left || tileworldpos.Y < cameraClipBounds.Top ||
                    tileworldpos.X > cameraClipBounds.Right || tileworldpos.Y > cameraClipBounds.Bottom)
                    continue;

                var tileRegion = tile.TextureRegion;

                // culling for arbitrary size tiles if necessary
                if (TiledMap.RequiresLargeTileCulling)
                    if (tileworldpos.X + tileRegion.SourceRect.Width < cameraClipBounds.Left ||
                        tileworldpos.Y - tileRegion.SourceRect.Height > cameraClipBounds.Bottom)
                        continue;

                DrawTile(batcher, position, layerDepth, x, y, 1);
            }
        }


        public void DrawTile(Batcher batcher, Vector2 position, float layerDepth, int x, int y, float scale)
        {
            var tile = GetTile(x, y);
            if (tile == null)
                return;

            var t = TiledMap.IsometricTileToWorldPosition(x, y);
            var tileRegion = tile.TextureRegion;

            // for the y position, we need to take into account if the tile is larger than the tileHeight and shift. Tiled uses
            // a bottom-left coordinate system and MonoGame a top-left
            var rotation = 0f;

            var spriteEffects = SpriteEffects.None;
            if (tile.FlippedHorizonally)
                spriteEffects |= SpriteEffects.FlipHorizontally;
            if (tile.FlippedVertically)
                spriteEffects |= SpriteEffects.FlipVertically;
            if (tile.FlippedDiagonally)
                if (tile.FlippedHorizonally && tile.FlippedVertically)
                {
                    spriteEffects ^= SpriteEffects.FlipVertically;
                    rotation = MathHelper.PiOver2;
                    t.X += TiledMap.TileHeight + (tileRegion.SourceRect.Height - TiledMap.TileHeight);
                    t.Y -= tileRegion.SourceRect.Width - TiledMap.TileWidth;
                }
                else if (tile.FlippedHorizonally)
                {
                    spriteEffects ^= SpriteEffects.FlipVertically;
                    rotation = -MathHelper.PiOver2;
                    t.Y += TiledMap.TileHeight;
                }
                else if (tile.FlippedVertically)
                {
                    spriteEffects ^= SpriteEffects.FlipHorizontally;
                    rotation = MathHelper.PiOver2;
                    t.X += TiledMap.TileWidth + (tileRegion.SourceRect.Height - TiledMap.TileHeight);
                    t.Y += TiledMap.TileWidth - tileRegion.SourceRect.Width;
                }
                else
                {
                    spriteEffects ^= SpriteEffects.FlipHorizontally;
                    rotation = -MathHelper.PiOver2;
                    t.Y += TiledMap.TileHeight;
                }

            // if we had no rotations (diagonal flipping) shift our y-coord to account for any non-tileSized tiles to account for
            // Tiled being bottom-left origin
            if (rotation == 0)
                t.Y += TiledMap.TileHeight - tileRegion.SourceRect.Height;

            // Scale the tile's relative position, but not the origin
            t = t * new Vector2(scale) + position;

            batcher.Draw(tileRegion.Texture2D, t, tileRegion.SourceRect, Color, rotation, Vector2.Zero, scale,
                spriteEffects, layerDepth);
        }


	    /// <summary>
	    ///     returns the bounds Rectangle of the passed in tile
	    /// </summary>
	    /// <returns>The bounds for tile.</returns>
	    /// <param name="tile">Tile.</param>
	    /// <param name="tilemap">Tilemap.</param>
	    public static Rectangle GetBoundsForTile(TiledTile tile, TiledMap tilemap)
        {
            return new Rectangle(tile.X * tilemap.TileWidth, tile.Y * tilemap.TileHeight, tilemap.TileWidth,
                tilemap.TileHeight);
        }


	    /// <summary>
	    ///     note that world position assumes that the Vector2 was normalized to be in the tilemaps coordinates. i.e. if the
	    ///     tilemap
	    ///     is not at 0,0 then the world position should be moved so that it takes into consideration the tilemap offset from
	    ///     0,0.
	    ///     Example: if the tilemap is at 300,300 then the passed in value should be worldPos - (300,300)
	    /// </summary>
	    /// <returns>The tile at world position.</returns>
	    /// <param name="pos">Position.</param>
	    public TiledTile GetTileAtWorldPosition(Vector2 pos)
        {
            var tilePos = TiledMap.IsometricWorldToTilePosition(pos);
            return GetTile(tilePos.X, tilePos.Y);
        }


	    /// <summary>
	    ///     Returns a list of rectangles in tile space, where any non-null tile is combined into bounding regions
	    /// </summary>
	    /// <param name="layer">Layer.</param>
	    public List<Rectangle> GetCollisionRectangles()
        {
            var checkedIndexes = new bool?[Tiles.Length];
            var rectangles = new List<Rectangle>();
            var startCol = -1;
            var index = -1;

            for (var y = 0; y < TiledMap.Height; y++)
            {
                for (var x = 0; x < TiledMap.Width; x++)
                {
                    index = y * TiledMap.Width + x;
                    var tile = GetTile(x, y);

                    if (tile != null && (checkedIndexes[index] == false || checkedIndexes[index] == null))
                    {
                        if (startCol < 0)
                            startCol = x;

                        checkedIndexes[index] = true;
                    }
                    else if (tile == null || checkedIndexes[index] == true)
                    {
                        if (startCol >= 0)
                        {
                            rectangles.Add(FindBoundsRect(startCol, x, y, checkedIndexes));
                            startCol = -1;
                        }
                    }
                } // end for x

                if (startCol >= 0)
                {
                    rectangles.Add(FindBoundsRect(startCol, TiledMap.Width, y, checkedIndexes));
                    startCol = -1;
                }
            }

            return rectangles;
        }


	    /// <summary>
	    ///     Finds the largest bounding rect around tiles between startX and endX, starting at startY and going
	    ///     down as far as possible
	    /// </summary>
	    /// <returns>The bounds rect.</returns>
	    /// <param name="startX">Start x.</param>
	    /// <param name="endX">End x.</param>
	    /// <param name="startY">Start y.</param>
	    /// <param name="checkedIndexes">Checked indexes.</param>
	    public Rectangle FindBoundsRect(int startX, int endX, int startY, bool?[] checkedIndexes)
        {
            var index = -1;

            for (var y = startY + 1; y < TiledMap.Height; y++)
            for (var x = startX; x < endX; x++)
            {
                index = y * TiledMap.Width + x;
                var tile = GetTile(x, y);

                if (tile == null || checkedIndexes[index] == true)
                {
                    // Set everything we've visited so far in this row to false again because it won't be included in the rectangle and should be checked again
                    for (var x2 = startX; endX < x; x++)
                    {
                        index = y * TiledMap.Width + x;
                        checkedIndexes[index] = false;
                    }

                    return new Rectangle(startX * TiledMap.TileWidth, startY * TiledMap.TileHeight,
                        (endX - startX) * TiledMap.TileWidth, (y - startY) * TiledMap.TileHeight);
                }

                checkedIndexes[index] = true;
            }

            return new Rectangle(startX * TiledMap.TileWidth, startY * TiledMap.TileHeight,
                (endX - startX) * TiledMap.TileWidth, (TiledMap.Height - startY) * TiledMap.TileHeight);
        }


	    /// <summary>
	    ///     gets a List of all the TiledTiles that intersect the passed in Rectangle. The returned List can be put back in the
	    ///     pool via ListPool.free.
	    /// </summary>
	    /// <returns>The tiles intersecting bounds.</returns>
	    /// <param name="bounds">Bounds.</param>
	    public List<TiledTile> GetTilesIntersectingBounds(Rectangle bounds)
        {
            // Note that after transforming the bounds to isometric, top left is the left most tile, bottom right is the right most, top right is top most, bottom left is bottom most
            var topLeft = TiledMap.IsometricWorldToTilePosition(bounds.X, bounds.Y);
            var bottomLeft = TiledMap.IsometricWorldToTilePosition(bounds.X, bounds.Bottom);
            var topRight = TiledMap.IsometricWorldToTilePosition(bounds.Right, bounds.Y);
            var bottomRight = TiledMap.IsometricWorldToTilePosition(bounds.Right, bounds.Bottom);

            var tilelist = ListPool<TiledTile>.Obtain();
            // Iterate in larger isometric rectangle that definitely includes our smaller bounding rectangle
            for (var x = topLeft.X; x <= bottomRight.X; x++)
            for (var y = topRight.Y; y <= bottomLeft.Y; y++)
            {
                var tile = GetTile(x, y);
                // Check if tile collides with our bounds
                if (tile != null && bounds.Contains(TiledMap.IsometricTileToWorldPosition(x, y)))
                    tilelist.Add(tile);
            }

            return tilelist;
        }


	    /// <summary>
	    ///     casts a line from start to end returning the first solid tile it intersects. Note that start and end and clamped to
	    ///     the tilemap
	    ///     bounds so make sure you pass in valid positions else you may get odd results!
	    /// </summary>
	    /// <param name="start">Start.</param>
	    /// <param name="end">End.</param>
	    public TiledTile Linecast(Vector2 start, Vector2 end)
        {
            var direction = end - start;

            // worldToTilePosition clamps to the tilemaps bounds so no need to worry about overlow
            var startCell = TiledMap.IsometricWorldToTilePosition(start);
            var endCell = TiledMap.IsometricWorldToTilePosition(end);

            start.X /= TiledMap.TileWidth;
            start.Y /= TiledMap.TileHeight;

            // what tile are we on
            var intX = startCell.X;
            var intY = startCell.Y;

            // ensure our start cell exists
            if (intX < 0 || intX >= TiledMap.Width || intY < 0 || intY >= TiledMap.Height)
                return null;

            // which way we go
            var stepX = System.Math.Sign(direction.X);
            var stepY = System.Math.Sign(direction.Y);

            // Calculate cell boundaries. when the step is positive, the next cell is after this one meaning we add 1.
            // If negative, cell is before this one in which case dont add to boundary
            var boundaryX = intX + (stepX > 0 ? 1 : 0);
            var boundaryY = intY + (stepY > 0 ? 1 : 0);

            // determine the value of t at which the ray crosses the first vertical tile boundary. same for y/horizontal.
            // The minimum of these two values will indicate how much we can travel along the ray and still remain in the current tile
            // may be infinite for near vertical/horizontal rays
            var tMaxX = (boundaryX - start.X) / direction.X;
            var tMaxY = (boundaryY - start.Y) / direction.Y;
            if (direction.X == 0f)
                tMaxX = float.PositiveInfinity;
            if (direction.Y == 0f)
                tMaxY = float.PositiveInfinity;

            // how far do we have to walk before crossing a cell from a cell boundary. may be infinite for near vertical/horizontal rays
            var tDeltaX = stepX / direction.X;
            var tDeltaY = stepY / direction.Y;

            // start walking and returning the intersecting tiles
            var tile = Tiles[intX + intY * Width];
            if (tile != null)
                return tile;

            while (intX != endCell.X || intY != endCell.Y)
            {
                if (tMaxX < tMaxY)
                {
                    intX += stepX;
                    tMaxX += tDeltaX;
                }
                else
                {
                    intY += stepY;
                    tMaxY += tDeltaY;
                }

                tile = Tiles[intX + intY * Width];
                if (tile != null)
                    return tile;
            }

            return null;
        }


        #region Tile management

	    /// <summary>
	    ///     gets the TiledTile at the x/y coordinates. Note that these are tile coordinates not world coordinates!
	    /// </summary>
	    /// <returns>The tile.</returns>
	    /// <param name="x">The x coordinate.</param>
	    /// <param name="y">The y coordinate.</param>
	    public TiledTile GetTile(int x, int y)
        {
            return Tiles[x + y * Width];
        }


	    /// <summary>
	    ///     gets the TiledTile at the x/y coordinates. Note that these are tile coordinates not world coordinates!
	    /// </summary>
	    /// <returns>The tile.</returns>
	    /// <param name="x">The x coordinate.</param>
	    /// <param name="y">The y coordinate.</param>
	    /// <typeparam name="T">The 1st type parameter.</typeparam>
	    public T GetTile<T>(int x, int y) where T : TiledTile
        {
            return Tiles[x + y * Width] as T;
        }


	    /// <summary>
	    ///     sets the tile and updates its tileset
	    /// </summary>
	    /// <returns>The tile.</returns>
	    /// <param name="tile">Tile.</param>
	    public TiledTile SetTile(TiledTile tile)
        {
            Tiles[tile.X + tile.Y * Width] = tile;
            tile.Tileset = TiledMap.GetTilesetForTileId(tile.Id);

            return tile;
        }


	    /// <summary>
	    ///     nulls out the tile at the x/y coordinates
	    /// </summary>
	    /// <param name="x">The x coordinate.</param>
	    /// <param name="y">The y coordinate.</param>
	    public void RemoveTile(int x, int y)
        {
            Tiles[x + y * Width] = null;
        }

        #endregion
    }
}