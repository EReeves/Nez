namespace Nez.UI.Base
{
	/// <summary>
	///     Value placeholder, allowing the value to be computed on request. Values are provided an element for context which
	///     reduces the
	///     number of value instances that need to be created and reduces verbosity in code that specifies values
	/// </summary>
	public abstract class Value
    {
	    /// <summary>
	    ///     A value that is always zero.
	    /// </summary>
	    public static Fixed Zero = new Fixed(0);


        public static Value MinWidth = new MinWidthValue();


        public static Value MinHeight = new MinHeightValue();


        public static Value PrefWidth = new PrefWidthValue();


        public static Value PrefHeight = new PrefHeightValue();


        public static Value MaxWidth = new MaxWidthValue();


        public static Value MaxHeight = new MaxHeightValue();

	    /// <summary>
	    ///     context May be null
	    /// </summary>
	    /// <param name="context">Context.</param>
	    public abstract float Get(Element context);


	    /// <summary>
	    ///     Value that is the maxHeight of the element in the cell.
	    /// </summary>
	    public static Value PercentWidth(float percent)
        {
            return new PercentWidthValue
            {
                Percent = percent
            };
        }


	    /// <summary>
	    ///     Returns a value that is a percentage of the specified elements's width. The context element is ignored.
	    /// </summary>
	    public static Value PercentWidth(float percent, Element delegateElement)
        {
            return new PercentWidthDelegateValue
            {
                DelegateElement = delegateElement,
                Percent = percent
            };
        }


	    /// <summary>
	    ///     Returns a value that is a percentage of the element's height.
	    /// </summary>
	    public static Value PercentHeight(float percent)
        {
            return new PercentageHeightValue
            {
                Percent = percent
            };
        }


	    /// <summary>
	    ///     Returns a value that is a percentage of the specified elements's height. The context element is ignored.
	    /// </summary>
	    public static Value PercentHeight(float percent, Element delegateElement)
        {
            return new PercentHeightDelegateValue
            {
                DelegateElement = delegateElement,
                Percent = percent
            };
        }


	    /// <summary>
	    ///     A fixed value that is not computed each time it is used.
	    /// </summary>
	    public class Fixed : Value
        {
            private readonly float _value;

            public Fixed(float value)
            {
                this._value = value;
            }

            public override float Get(Element context)
            {
                return _value;
            }
        }

	    /// <summary>
	    ///     Value that is the minWidth of the element in the cell.
	    /// </summary>
	    public class MinWidthValue : Value
        {
            public override float Get(Element context)
            {
                if (context is ILayout)
                    return ((ILayout) context).MinWidth;
                return context == null ? 0 : context.Width;
            }
        }

	    /// <summary>
	    ///     Value that is the minHeight of the element in the cell.
	    /// </summary>
	    public class MinHeightValue : Value
        {
            public override float Get(Element context)
            {
                if (context is ILayout)
                    return ((ILayout) context).MinHeight;
                return context == null ? 0 : context.Height;
            }
        }

	    /// <summary>
	    ///     Value that is the prefWidth of the element in the cell.
	    /// </summary>
	    public class PrefWidthValue : Value
        {
            public override float Get(Element context)
            {
                if (context is ILayout)
                    return ((ILayout) context).PreferredWidth;
                return context == null ? 0 : context.Width;
            }
        }

	    /// <summary>
	    ///     Value that is the prefHeight of the element in the cell.
	    /// </summary>
	    public class PrefHeightValue : Value
        {
            public override float Get(Element context)
            {
                if (context is ILayout)
                    return ((ILayout) context).PreferredHeight;
                return context == null ? 0 : context.Height;
            }
        }

	    /// <summary>
	    ///     Value that is the maxWidth of the element in the cell.
	    /// </summary>
	    public class MaxWidthValue : Value
        {
            public override float Get(Element context)
            {
                if (context is ILayout)
                    return ((ILayout) context).MaxWidth;
                return context == null ? 0 : context.Width;
            }
        }

	    /// <summary>
	    ///     Value that is the maxHeight of the element in the cell.
	    /// </summary>
	    public class MaxHeightValue : Value
        {
            public override float Get(Element context)
            {
                if (context is ILayout)
                    return ((ILayout) context).MaxHeight;
                return context == null ? 0 : context.Height;
            }
        }

	    /// <summary>
	    ///     Returns a value that is a percentage of the element's width.
	    /// </summary>
	    public class PercentWidthValue : Value
        {
            public float Percent;

            public override float Get(Element element)
            {
                return element.Width * Percent;
            }
        }

	    /// <summary>
	    ///     Returns a value that is a percentage of the specified elements's width. The context element is ignored.
	    /// </summary>
	    public class PercentWidthDelegateValue : Value
        {
            public Element DelegateElement;
            public float Percent;

            public override float Get(Element element)
            {
                return DelegateElement.Width * Percent;
            }
        }

	    /// <summary>
	    ///     Returns a value that is a percentage of the element's height.
	    /// </summary>
	    public class PercentageHeightValue : Value
        {
            public float Percent;

            public override float Get(Element element)
            {
                return element.Height * Percent;
            }
        }

	    /// <summary>
	    ///     Returns a value that is a percentage of the specified elements's height. The context element is ignored.
	    /// </summary>
	    public class PercentHeightDelegateValue : Value
        {
            public Element DelegateElement;
            public float Percent;

            public override float Get(Element element)
            {
                return DelegateElement.Height * Percent;
            }
        }
    }
}