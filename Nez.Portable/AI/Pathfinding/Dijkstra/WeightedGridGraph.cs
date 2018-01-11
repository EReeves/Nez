using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Nez.PipelineRuntime.Tiled;

namespace Nez.AI.Pathfinding.Dijkstra
{
	/// <summary>
	///     basic grid graph with support for one type of weighted node
	/// </summary>
	public class WeightedGridGraph : IWeightedGraph<Point>
    {
        public static readonly Point[] Dirs =
        {
            new Point(1, 0),
            new Point(0, -1),
            new Point(-1, 0),
            new Point(0, 1)
        };

        private readonly List<Point> _neighbors = new List<Point>(4);

        private readonly int _width;
        private readonly int _height;
        public int DefaultWeight = 1;

        public HashSet<Point> Walls = new HashSet<Point>();
        public HashSet<Point> WeightedNodes = new HashSet<Point>();
        public int WeightedNodeWeight = 5;


        public WeightedGridGraph(int width, int height)
        {
            _width = width;
            _height = height;
        }


	    /// <summary>
	    ///     creates a WeightedGridGraph from a TiledTileLayer. Present tile are walls and empty tiles are passable.
	    /// </summary>
	    /// <param name="tiledLayer">Tiled layer.</param>
	    public WeightedGridGraph(TiledTileLayer tiledLayer)
        {
            _width = tiledLayer.Width;
            _height = tiledLayer.Height;

            for (var y = 0; y < tiledLayer.TiledMap.Height; y++)
            for (var x = 0; x < tiledLayer.TiledMap.Width; x++)
                if (tiledLayer.GetTile(x, y) != null)
                    Walls.Add(new Point(x, y));
        }


	    /// <summary>
	    ///     ensures the node is in the bounds of the grid graph
	    /// </summary>
	    /// <returns><c>true</c>, if node in bounds was ised, <c>false</c> otherwise.</returns>
	    /// <param name="node">Node.</param>
	    private bool IsNodeInBounds(Point node)
        {
            return 0 <= node.X && node.X < _width && 0 <= node.Y && node.Y < _height;
        }


	    /// <summary>
	    ///     checks if the node is passable. Walls are impassable.
	    /// </summary>
	    /// <returns><c>true</c>, if node passable was ised, <c>false</c> otherwise.</returns>
	    /// <param name="node">Node.</param>
	    private bool IsNodePassable(Point node)
        {
            return !Walls.Contains(node);
        }


	    /// <summary>
	    ///     convenience shortcut for calling AStarPathfinder.search
	    /// </summary>
	    /// <param name="start">Start.</param>
	    /// <param name="goal">Goal.</param>
	    public List<Point> Search(Point start, Point goal)
        {
            return WeightedPathfinder.Search(this, start, goal);
        }


        #region IWeightedGraph implementation

        IEnumerable<Point> IWeightedGraph<Point>.GetNeighbors(Point node)
        {
            _neighbors.Clear();

            foreach (var dir in Dirs)
            {
                var next = new Point(node.X + dir.X, node.Y + dir.Y);
                if (IsNodeInBounds(next) && IsNodePassable(next))
                    _neighbors.Add(next);
            }

            return _neighbors;
        }


        int IWeightedGraph<Point>.Cost(Point from, Point to)
        {
            return WeightedNodes.Contains(to) ? WeightedNodeWeight : DefaultWeight;
        }

        #endregion
    }
}