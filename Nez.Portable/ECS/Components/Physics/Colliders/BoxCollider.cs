using Microsoft.Xna.Framework;
using Nez.Graphics.Batcher;
using Nez.Physics.Shapes;

namespace Nez.ECS.Components.Physics.Colliders
{
    public class BoxCollider : Collider
    {
	    /// <summary>
	    ///     zero param constructor requires that a RenderableComponent be on the entity so that the collider can size itself
	    ///     when the
	    ///     entity is added to the scene.
	    /// </summary>
	    public BoxCollider()
        {
            // we stick a 1x1 box in here as a placeholder until the next frame when the Collider is added to the Entity and can get more
            // accurate auto-sizing data
            Shape = new Box(1, 1);
            ColliderRequiresAutoSizing = true;
        }


	    /// <summary>
	    ///     creates a BoxCollider and uses the x/y components as the localOffset
	    /// </summary>
	    /// <param name="x">The x coordinate.</param>
	    /// <param name="y">The y coordinate.</param>
	    /// <param name="width">Width.</param>
	    /// <param name="height">Height.</param>
	    public BoxCollider(float x, float y, float width, float height)
        {
            LocalOffset = new Vector2(x + width / 2, y + height / 2);
            Shape = new Box(width, height);
        }


        public BoxCollider(float width, float height) : this(-width / 2, -height / 2, width, height)
        {
        }


	    /// <summary>
	    ///     creates a BoxCollider and uses the x/y components of the Rect as the localOffset
	    /// </summary>
	    /// <param name="rect">Rect.</param>
	    public BoxCollider(Rectangle rect) : this(rect.X, rect.Y, rect.Width, rect.Height)
        {
        }

        public float Width
        {
            get => ((Box) Shape).Width;
            set => SetWidth(value);
        }

        public float Height
        {
            get => ((Box) Shape).Height;
            set => SetHeight(value);
        }


        public override void DebugRender(Graphics.Graphics graphics)
        {
            var poly = Shape as Polygon;
            graphics.Batcher.DrawHollowRect(Bounds, Debug.Debug.Colors.ColliderBounds, Debug.Debug.Size.LineSizeMultiplier);
            graphics.Batcher.DrawPolygon(Shape.Position, poly.Points, Debug.Debug.Colors.ColliderEdge, true,
                Debug.Debug.Size.LineSizeMultiplier);
            graphics.Batcher.DrawPixel(Entity.Transform.Position, Debug.Debug.Colors.ColliderPosition,
                4 * Debug.Debug.Size.LineSizeMultiplier);
            graphics.Batcher.DrawPixel(Entity.Transform.Position + Shape.Center, Debug.Debug.Colors.ColliderCenter,
                2 * Debug.Debug.Size.LineSizeMultiplier);
        }


        public override string ToString()
        {
            return string.Format("[BoxCollider: bounds: {0}", Bounds);
        }


        #region Fluent setters

	    /// <summary>
	    ///     sets the size of the BoxCollider
	    /// </summary>
	    /// <returns>The size.</returns>
	    /// <param name="width">Width.</param>
	    /// <param name="height">Height.</param>
	    public BoxCollider SetSize(float width, float height)
        {
            ColliderRequiresAutoSizing = false;
            var box = Shape as Box;
            if (width != box.Width || height != box.Height)
            {
                // update the box, dirty our bounds and if we need to update our bounds in the Physics system
                box.UpdateBox(width, height);
                IsPositionDirty = true;
                if (Entity != null && IsParentEntityAddedToScene)
                    Nez.Physics.Physics.UpdateCollider(this);
            }

            return this;
        }


	    /// <summary>
	    ///     sets the width of the BoxCollider
	    /// </summary>
	    /// <returns>The width.</returns>
	    /// <param name="width">Width.</param>
	    public BoxCollider SetWidth(float width)
        {
            ColliderRequiresAutoSizing = false;
            var box = Shape as Box;
            if (width != box.Width)
            {
                // update the box, dirty our bounds and if we need to update our bounds in the Physics system
                box.UpdateBox(width, box.Height);
                IsPositionDirty = true;
                if (Entity != null && IsParentEntityAddedToScene)
                    Nez.Physics.Physics.UpdateCollider(this);
            }

            return this;
        }


	    /// <summary>
	    ///     sets the height of the BoxCollider
	    /// </summary>
	    /// <returns>The height.</returns>
	    /// <param name="height">Height.</param>
	    public BoxCollider SetHeight(float height)
        {
            ColliderRequiresAutoSizing = false;
            var box = Shape as Box;
            if (height != box.Height)
            {
                // update the box, dirty our bounds and if we need to update our bounds in the Physics system
                box.UpdateBox(box.Width, height);
                IsPositionDirty = true;
                if (Entity != null && IsParentEntityAddedToScene)
                    Nez.Physics.Physics.UpdateCollider(this);
            }

            return this;
        }

        #endregion
    }
}