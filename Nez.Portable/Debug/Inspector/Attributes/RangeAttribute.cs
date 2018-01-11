using System;

namespace Nez.Debug.Inspector.Attributes
{
	/// <summary>
	///     Range attribute.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class RangeAttribute : InspectableAttribute
    {
        public float MinValue, MaxValue, StepSize;


        public RangeAttribute(float minValue, float maxValue, float stepSize)
        {
            this.MinValue = minValue;
            this.MaxValue = maxValue;
            this.StepSize = stepSize;
        }


        public RangeAttribute(float minValue, float maxValue) : this(minValue, maxValue, 0.1f)
        {
        }
    }
}