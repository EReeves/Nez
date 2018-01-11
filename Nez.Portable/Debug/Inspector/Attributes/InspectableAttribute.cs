using System;

namespace Nez.Debug.Inspector.Attributes
{
	/// <summary>
	///     Attribute that is used to indicate that the field/property should be present in the inspector
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class InspectableAttribute : Attribute
    {
    }
}