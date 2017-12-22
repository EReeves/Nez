namespace Nez.Shadows
{
	/// <summary>
	///     Represents an occluding line segment in the visibility mesh
	/// </summary>
	internal class Segment
    {
	    /// <summary>
	    ///     First end-point of the segment
	    /// </summary>
	    internal EndPoint P1;

	    /// <summary>
	    ///     Second end-point of the segment
	    /// </summary>
	    internal EndPoint P2;


        internal Segment()
        {
            P1 = null;
            P2 = null;
        }


        public override bool Equals(object obj)
        {
            if (obj is Segment)
            {
                var other = obj as Segment;
                return P1.Equals(other.P1) && P2.Equals(other.P2);
            }

            return false;
        }


        public override int GetHashCode()
        {
            return P1.GetHashCode() + P2.GetHashCode();
        }


        public override string ToString()
        {
            return "{" + P1.Position + ", " + P2.Position + "}";
        }
    }
}