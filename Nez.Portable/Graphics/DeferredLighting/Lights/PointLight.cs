using Microsoft.Xna.Framework;

namespace Nez.DeferredLighting
{
	/// <summary>
	///     PointLights radiate light in a circle. Note that PointLights are affected by Transform.scale. The Transform.scale.X
	///     value is multiplied
	///     by the lights radius when sent to the GPU. It is expected that scale will be linear.
	/// </summary>
	public class PointLight : DeferredLight
    {
        protected float radius;

	    /// <summary>
	    ///     brightness of the light
	    /// </summary>
	    public float Intensity = 3f;

	    /// <summary>
	    ///     "height" above the scene in the z direction
	    /// </summary>
	    public float ZPosition = 150f;


        public PointLight()
        {
            SetRadius(400f);
        }


        public PointLight(Color color) : this()
        {
            this.Color = color;
        }

        public override RectangleF Bounds
        {
            get
            {
                if (AreBoundsDirty)
                {
                    // the size of the light only uses the x scale value
                    var size = radius * Entity.Transform.Scale.X * 2;
                    Bounds.CalculateBounds(Entity.Transform.Position, localOffset, Radius * Entity.Transform.Scale,
                        Vector2.One, 0, size, size);
                    AreBoundsDirty = false;
                }

                return Bounds;
            }
        }

	    /// <summary>
	    ///     how far does this light reaches
	    /// </summary>
	    public float Radius => radius;


        public PointLight SetZPosition(float z)
        {
            ZPosition = z;
            return this;
        }


	    /// <summary>
	    ///     how far does this light reach
	    /// </summary>
	    /// <returns>The radius.</returns>
	    /// <param name="radius">Radius.</param>
	    public PointLight SetRadius(float radius)
        {
            this.radius = radius;
            AreBoundsDirty = true;
            return this;
        }


	    /// <summary>
	    ///     brightness of the light
	    /// </summary>
	    /// <returns>The intensity.</returns>
	    /// <param name="intensity">Intensity.</param>
	    public PointLight SetIntensity(float intensity)
        {
            this.Intensity = intensity;
            return this;
        }


	    /// <summary>
	    ///     renders the bounds only if there is no collider. Always renders a square on the origin.
	    /// </summary>
	    /// <param name="graphics">Graphics.</param>
	    public override void DebugRender(Graphics graphics)
        {
            graphics.Batcher.DrawCircle(Entity.Transform.Position + localOffset, radius * Entity.Transform.Scale.X,
                Color.DarkOrchid, 2);
        }
    }
}