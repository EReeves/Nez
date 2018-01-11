using System;

namespace Nez.Debug.Inspector.Attributes
{
	/// <summary>
	///     displays a tooltip when hovering over the label of any inspectable elements
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class TooltipAttribute : InspectableAttribute
    {
        public string Tooltip;

        public TooltipAttribute(string tooltip)
        {
            this.Tooltip = tooltip;
        }
    }
}