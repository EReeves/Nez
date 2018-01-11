using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Nez.PipelineRuntime.Tiled
{
    public class TiledObject
    {
        public enum TiledObjectType
        {
            None,
            Ellipse,
            Image,
            Polygon,
            Polyline
        }

        public int Gid;
        public int Height;
        public string Name;
        public string ObjectType;
        public Vector2[] PolyPoints;
        public readonly Dictionary<string, string> Properties = new Dictionary<string, string>();
        public int Rotation;
        public TiledObjectType ObjType;
        public string Type;
        public bool Visible;
        public int Width;
        public int X;
        public int Y;

	    /// <summary>
	    ///     wraps the x/y fields in a Vector
	    /// </summary>
	    public Vector2 Position
        {
            get => new Vector2(X, Y);
            set
            {
                X = (int) value.X;
                Y = (int) value.Y;
            }
        }
    }
}