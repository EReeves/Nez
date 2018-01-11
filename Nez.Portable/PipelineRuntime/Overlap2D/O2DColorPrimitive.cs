using Microsoft.Xna.Framework;

namespace Nez.PipelineRuntime.Overlap2D
{
    public class O2DColorPrimitive : O2DMainItem
    {
        public Vector2[] Polygon;


        public Vector3[] GetPolygon3D()
        {
            var poly3D = new Vector3[Polygon.Length + 1];

            for (var i = 0; i < Polygon.Length; i++)
                poly3D[i] = new Vector3(Polygon[i], 0);

            poly3D[Polygon.Length] = new Vector3(Polygon[0], 0);

            return poly3D;
        }
    }
}