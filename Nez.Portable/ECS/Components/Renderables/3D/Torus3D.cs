using Microsoft.Xna.Framework;

namespace Nez.ECS.Components.Renderables._3D
{
    public class Torus3D : GeometricPrimitive3D
    {
        public Torus3D(float thickness, int tessellation, Color color)
        {
            var diameter = 1f;

            // First we loop around the main ring of the torus. 
            for (var i = 0; i < tessellation; i++)
            {
                var outerAngle = i * MathHelper.TwoPi / tessellation;

                // Create a transform matrix that will align geometry to slice perpendicularly though the current ring position. 
                var vertTransform = Matrix.CreateTranslation(diameter / 2, 0, 0) * Matrix.CreateRotationY(outerAngle);

                // Now we loop along the other axis, around the side of the tube. 
                for (var j = 0; j < tessellation; j++)
                {
                    var innerAngle = j * MathHelper.TwoPi / tessellation;

                    var dx = (float) System.Math.Cos(innerAngle);
                    var dy = (float) System.Math.Sin(innerAngle);

                    // Create a vertex. 
                    var normal = new Vector3(dx, dy, 0);
                    var pos = normal * thickness / 2;

                    pos = Vector3.Transform(pos, vertTransform);
                    normal = Vector3.TransformNormal(normal, vertTransform);

                    AddVertex(pos, color, normal);

                    // and create indices for two triangles. 
                    var nextI = (i + 1) % tessellation;
                    var nextJ = (j + 1) % tessellation;

                    AddIndex(i * tessellation + j);
                    AddIndex(i * tessellation + nextJ);
                    AddIndex(nextI * tessellation + j);

                    AddIndex(i * tessellation + nextJ);
                    AddIndex(nextI * tessellation + nextJ);
                    AddIndex(nextI * tessellation + j);
                }
            }

            InitializePrimitive();
        }
    }
}