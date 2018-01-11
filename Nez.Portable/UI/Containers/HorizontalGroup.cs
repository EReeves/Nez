using Nez.Maths;
using Nez.UI.Base;

namespace Nez.UI.Containers
{
    public class HorizontalGroup : Group
    {
        public int Align;
        public float Fill;
        public float PadTop, PadLeft, PadBottom, PadRight;
        private float _prefWidth, _prefHeight;
        public bool Reverse;


        public bool Round = true;

        private bool _sizeInvalid = true;

        public float Spacing;


        public HorizontalGroup()
        {
            Touchable = Touchable.ChildrenOnly;
        }


        public HorizontalGroup(float spacing) : this()
        {
            SetSpacing(spacing);
        }

        public override float PreferredWidth
        {
            get
            {
                if (_sizeInvalid)
                    ComputeSize();
                return _prefWidth;
            }
        }

        public override float PreferredHeight
        {
            get
            {
                if (_sizeInvalid)
                    ComputeSize();
                return _prefHeight;
            }
        }


        public override void Invalidate()
        {
            _sizeInvalid = true;
            base.Invalidate();
        }


        private void ComputeSize()
        {
            _sizeInvalid = false;
            _prefWidth = PadLeft + PadRight + Spacing * (Children.Count - 1);
            _prefHeight = 0;
            for (var i = 0; i < Children.Count; i++)
            {
                var child = Children[i];
                if (child is ILayout)
                {
                    var layout = (ILayout) child;
                    _prefWidth += layout.PreferredWidth;
                    _prefHeight = System.Math.Max(_prefHeight, layout.PreferredHeight);
                }
                else
                {
                    _prefWidth += child.Width;
                    _prefHeight += System.Math.Max(_prefHeight, child.Height);
                    ;
                }
            }

            _prefHeight += PadTop + PadBottom;
            if (Round)
            {
                _prefWidth = Mathf.Round(_prefWidth);
                _prefHeight = Mathf.Round(_prefHeight);
            }
        }


        public override void Layout()
        {
            var groupHeight = Height - PadTop - PadBottom;
            var x = !Reverse ? PadLeft : Width - PadRight + Spacing;

            for (var i = 0; i < Children.Count; i++)
            {
                var child = Children[i];
                float width, height;
                ILayout layout = null;

                if (child is ILayout)
                {
                    layout = child;
                    if (Fill > 0)
                        height = groupHeight * Fill;
                    else
                        height = System.Math.Min(layout.PreferredHeight, groupHeight);
                    height = System.Math.Max(height, layout.MinHeight);

                    var maxheight = layout.MaxHeight;
                    if (maxheight > 0 && height > MaxHeight)
                        height = maxheight;
                    width = layout.PreferredWidth;
                }
                else
                {
                    width = child.Width;
                    height = child.Height;

                    if (Fill > 0)
                        height *= Fill;
                }

                var y = PadTop;
                if ((Align & AlignInternal.Bottom) != 0)
                    y += groupHeight - height;
                else if ((Align & AlignInternal.Top) == 0) // center
                    y += (groupHeight - height) / 2;

                if (Reverse)
                    x -= width + Spacing;

                if (Round)
                    child.SetBounds(Mathf.Round(x), Mathf.Round(y), Mathf.Round(width), Mathf.Round(height));
                else
                    child.SetBounds(x, y, width, height);

                if (!Reverse)
                    x += width + Spacing;

                if (layout != null)
                    layout.Validate();
            }
        }


        #region Configuration

	    /// <summary>
	    ///     Sets the alignment of widgets within the vertical group. Set to {@link Align#center}, {@link Align#top},
	    ///     {@link Align#bottom}, {@link Align#left}, {@link Align#right}, or any combination of those
	    /// </summary>
	    /// <param name="align">Align.</param>
	    public HorizontalGroup SetAlignment(Align align)
        {
            Align = (int) align;
            return this;
        }


	    /// <summary>
	    ///     If true, the children will be ordered from bottom to top rather than the default top to bottom.
	    /// </summary>
	    /// <param name="reverse">If set to <c>true</c> reverse.</param>
	    public HorizontalGroup SetReverse(bool reverse)
        {
            Reverse = reverse;
            return this;
        }


	    /// <summary>
	    ///     Sets the space between children
	    /// </summary>
	    /// <param name="spacing">Spacing.</param>
	    public HorizontalGroup SetSpacing(float spacing)
        {
            Spacing = spacing;
            return this;
        }


	    /// <summary>
	    ///     Sets the padTop, padLeft, padBottom, and padRight to the specified value
	    /// </summary>
	    /// <param name="pad">Pad.</param>
	    public HorizontalGroup SetPad(float pad)
        {
            PadTop = pad;
            PadLeft = pad;
            PadBottom = pad;
            PadRight = pad;
            return this;
        }


        public HorizontalGroup SetPad(float top, float left, float bottom, float right)
        {
            PadTop = top;
            PadLeft = left;
            PadBottom = bottom;
            PadRight = right;
            return this;
        }


        public HorizontalGroup SetPadTop(float padTop)
        {
            PadTop = padTop;
            return this;
        }


        public HorizontalGroup SetPadLeft(float padLeft)
        {
            PadLeft = padLeft;
            return this;
        }


        public HorizontalGroup SetPadBottom(float padBottom)
        {
            PadBottom = padBottom;
            return this;
        }


        public HorizontalGroup SetPadRight(float padRight)
        {
            PadRight = padRight;
            return this;
        }


	    /// <summary>
	    ///     If true (the default), positions and sizes are rounded to integers.
	    /// </summary>
	    /// <param name="round">If set to <c>true</c> round.</param>
	    public HorizontalGroup SetRound(bool round)
        {
            Round = round;
            return this;
        }


	    /// <summary>
	    ///     fill 0 will use pref width
	    /// </summary>
	    /// <param name="fill">Fill.</param>
	    public HorizontalGroup SetFill(float fill)
        {
            Fill = fill;
            return this;
        }

        #endregion
    }
}