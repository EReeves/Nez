using Microsoft.Xna.Framework;
using Nez.PhysicsShapes;

namespace Nez.Shadows
{
	/// <summary>
	///     WIP: still has some odd rendering bugs that need to get worked out
	///     poly spot light. Works just like a PolyLight except it is limited to a cone shape (spotAngle).
	/// </summary>
	public class PolySpotLight : PolyLight
    {
        private Polygon _polygon;

        private float _spotAngle = 45;


        public PolySpotLight(float radius) : this(radius, Color.White)
        {
        }


        public PolySpotLight(float radius, Color color) : this(radius, color, 1.0f)
        {
        }


        public PolySpotLight(float radius, Color color, float power) : base(radius, color, power)
        {
        }

        public override RectangleF Bounds
        {
            get
            {
                if (AreBoundsDirty)
                {
                    _bounds = RectangleF.RectEncompassingPoints(_polygon.Points);
                    _bounds.Location += Entity.Transform.Position;
                    AreBoundsDirty = false;
                }
                return Bounds;
            }
        }

	    /// <summary>
	    ///     the angle of the light's spotlight cone in degrees. Defaults to 45.
	    /// </summary>
	    /// <value>The spot angle.</value>
	    public float SpotAngle
        {
            get => _spotAngle;
            set => SetSpotAngle(value);
        }


	    /// <summary>
	    ///     calculates the points needed to encompass the spot light. The points generate a polygon which is used for overlap
	    ///     detection.
	    /// </summary>
	    private void RecalculatePolyPoints()
        {
            // no need to recaluc if we dont have an Entity to work with
            if (Entity == null)
                return;

            // we only need a small number of verts for the spot polygon. We base how many off of the spot angle. Because we are approximating
            // an arc with a polygon we bump up the radius a bit so that our poly fully encompasses the spot area.
            var expandedRadius = radius + radius * 0.1f;
            var sides = Mathf.CeilToInt(_spotAngle / 25) + 1;
            var stepSize = _spotAngle * Mathf.Deg2Rad / sides;

            var verts = new Vector2[sides + 2];
            verts[0] = Vector2.Zero;

            for (var i = 0; i <= sides; i++)
                verts[i + 1] = new Vector2(expandedRadius * Mathf.Cos(stepSize * i),
                    expandedRadius * Mathf.Sin(stepSize * i));

            if (_polygon == null)
                _polygon = new Polygon(verts);
            else
                _polygon.OriginalPoints = verts;

            // rotate our verts based on the Entity.rotation and offset half of the spot angle so that the center of the spot points in
            // the direction of rotation
            Polygon.RotatePolygonVerts(Entity.Rotation - _spotAngle * 0.5f * Mathf.Deg2Rad, _polygon.OriginalPoints,
                _polygon.Points);
        }


        #region Fluent setters

        public override PolyLight SetRadius(float radius)
        {
            base.SetRadius(radius);
            RecalculatePolyPoints();
            return this;
        }


        public PolySpotLight SetSpotAngle(float spotAngle)
        {
            _spotAngle = spotAngle;
            RecalculatePolyPoints();
            return this;
        }

        #endregion


        #region Component overrides

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();
            RecalculatePolyPoints();
        }


        public override void DebugRender(Graphics graphics)
        {
            base.DebugRender(graphics);
            graphics.Batcher.DrawPolygon(_polygon.Position, _polygon.Points, Debug.Colors.ColliderEdge, true,
                Debug.Size.LineSizeMultiplier);
        }


        public override void OnEntityTransformChanged(Transform.Component comp)
        {
            base.OnEntityTransformChanged(comp);

            if (comp == Transform.Component.Rotation)
                Polygon.RotatePolygonVerts(Entity.Rotation - _spotAngle * 0.5f * Mathf.Deg2Rad,
                    _polygon.OriginalPoints, _polygon.Points);
        }

        #endregion


        #region PolyLight overrides

        protected override int GetOverlappedColliders()
        {
            CollisionResult result;
            var totalCollisions = 0;
            _polygon.Position = Entity.Transform.Position + localOffset;

            var neighbors = Physics.BoxcastBroadphase(Bounds, CollidesWithLayers);
            foreach (var neighbor in neighbors)
            {
                // skip triggers
                if (neighbor.IsTrigger)
                    continue;

                if (_polygon.CollidesWithShape(neighbor.Shape, out result))
                {
                    ColliderCache[totalCollisions++] = neighbor;
                    if (totalCollisions == ColliderCache.Length)
                        return totalCollisions;
                }
            }

            return totalCollisions;
        }


        protected override void LoadVisibilityBoundaries()
        {
            Visibility.LoadSpotLightBoundaries(_polygon.Points);
        }

        #endregion
    }
}