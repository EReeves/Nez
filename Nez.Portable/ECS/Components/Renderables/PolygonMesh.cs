using Microsoft.Xna.Framework;
using Nez.Utils;

namespace Nez.ECS.Components.Renderables
{
	/// <summary>
	///     renders a basic, CCW, convex polygon
	/// </summary>
	public class PolygonMesh : Mesh
    {
        public PolygonMesh(Vector2[] points, bool arePointsCcw = true)
        {
            var triangulator = new Triangulator();
            triangulator.Triangulate(points, arePointsCcw);

            SetVertPositions(points);
            SetTriangles(triangulator.TriangleIndices.ToArray());
            RecalculateBounds(true);
        }
    }
}