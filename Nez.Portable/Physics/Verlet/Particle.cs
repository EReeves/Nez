using Microsoft.Xna.Framework;

namespace Nez.Verlet
{
    public class Particle
    {
        internal Vector2 Acceleration;

	    /// <summary>
	    ///     if true, the Particle will collide with standard Nez Colliders
	    /// </summary>
	    public bool CollidesWithColliders = true;

        internal bool IsPinned;

	    /// <summary>
	    ///     the position of the Particle prior to its latest move
	    /// </summary>
	    public Vector2 LastPosition;

	    /// <summary>
	    ///     the mass of the Particle. Taken into account for all forces and constraints
	    /// </summary>
	    public float Mass = 1;

        internal Vector2 PinnedPosition;

	    /// <summary>
	    ///     the current position of the Particle
	    /// </summary>
	    public Vector2 Position;

	    /// <summary>
	    ///     the radius of the Particle
	    /// </summary>
	    public float Radius;


        public Particle(Vector2 position)
        {
            this.Position = position;
            LastPosition = position;
        }


        public Particle(float x, float y) : this(new Vector2(x, y))
        {
        }


	    /// <summary>
	    ///     applies a force taking mass into account to the Particle
	    /// </summary>
	    /// <param name="force">Force.</param>
	    public void ApplyForce(Vector2 force)
        {
            // acceleration = (1 / mass) * force
            Acceleration += force / Mass;
        }


	    /// <summary>
	    ///     pins the Particle to its current position
	    /// </summary>
	    public Particle Pin()
        {
            IsPinned = true;
            PinnedPosition = Position;
            return this;
        }


	    /// <summary>
	    ///     pins the particle to the specified position
	    /// </summary>
	    /// <param name="position">Position.</param>
	    public Particle PinTo(Vector2 position)
        {
            IsPinned = true;
            PinnedPosition = position;
            this.Position = PinnedPosition;
            return this;
        }


	    /// <summary>
	    ///     unpins the particle setting it free like the wind
	    /// </summary>
	    public Particle Unpin()
        {
            IsPinned = false;
            return this;
        }
    }
}