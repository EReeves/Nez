using Nez.Maths;
using Nez.UI.Base;

namespace Nez.UI.Containers
{
	/// <summary>
	///     A group that lays out its children on top of each other in a single column. This can be easier than using {@link
	///     Table} when
	///     elements need to be inserted in the middle of the group.
	///     The preferred width is the largest preferred width of any child. The preferred height is the sum of the children's
	///     preferred
	///     heights, plus spacing between them if set. The min size is the preferred size and the max size is 0.
	/// </summary>
	public class VerticalGroup : Group
    {
        private int _align;
        private float _fill;
        private float _padTop, _padLeft, _padBottom, _padRight;

        private float _prefWidth, _prefHeight;
        private bool _reverse;

        private bool _round = true;
        private bool _sizeInvalid = true;
        private float _spacing;


        public VerticalGroup()
        {
            Touchable = Touchable.ChildrenOnly;
        }


        public VerticalGroup(float spacing) : this()
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
            _prefWidth = 0;
            _prefHeight = _padTop + _padBottom + _spacing * (Children.Count - 1);
            for (var i = 0; i < Children.Count; i++)
            {
                var child = Children[i];
                if (child is ILayout)
                {
                    var layout = (ILayout) child;
                    _prefWidth = System.Math.Max(_prefWidth, layout.PreferredWidth);
                    _prefHeight += layout.PreferredHeight;
                }
                else
                {
                    _prefWidth = System.Math.Max(_prefWidth, child.Width);
                    _prefHeight += child.Height;
                }
            }

            _prefWidth += _padLeft + _padRight;
            if (_round)
            {
                _prefWidth = Mathf.Round(_prefWidth);
                _prefHeight = Mathf.Round(_prefHeight);
            }
        }


        public override void Layout()
        {
            var groupWidth = Width - _padLeft - _padRight;
            var y = _reverse ? Height - _padBottom + _spacing : _padTop;

            for (var i = 0; i < Children.Count; i++)
            {
                var child = Children[i];
                float width, height;

                ILayout layout = null;
                if (child is ILayout)
                {
                    layout = child;
                    if (_fill > 0)
                        width = groupWidth * _fill;
                    else
                        width = System.Math.Min(layout.PreferredWidth, groupWidth);
                    width = System.Math.Max(width, layout.MinWidth);

                    if (layout.MaxWidth > 0 && width > layout.MaxWidth)
                        width = layout.MaxWidth;
                    height = layout.PreferredHeight;
                }
                else
                {
                    width = child.Width;
                    height = child.Height;

                    if (_fill > 0)
                        width *= _fill;
                }

                var x = _padLeft;
                if ((_align & AlignInternal.Right) != 0)
                    x += groupWidth - width;
                else if ((_align & AlignInternal.Left) == 0) // center
                    x += (groupWidth - width) / 2;

                if (_reverse)
                    y -= height + _spacing;

                if (_round)
                    child.SetBounds(Mathf.Round(x), Mathf.Round(y), Mathf.Round(width), Mathf.Round(height));
                else
                    child.SetBounds(x, y, width, height);

                if (!_reverse)
                    y += height + _spacing;

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
	    public VerticalGroup SetAlignment(Align align)
        {
            _align = (int) align;
            return this;
        }


	    /// <summary>
	    ///     If true, the children will be ordered from bottom to top rather than the default top to bottom.
	    /// </summary>
	    /// <param name="reverse">If set to <c>true</c> reverse.</param>
	    public VerticalGroup SetReverse(bool reverse)
        {
            _reverse = reverse;
            return this;
        }


	    /// <summary>
	    ///     Sets the space between children
	    /// </summary>
	    /// <param name="spacing">Spacing.</param>
	    public VerticalGroup SetSpacing(float spacing)
        {
            _spacing = spacing;
            return this;
        }


	    /// <summary>
	    ///     Sets the padTop, padLeft, padBottom, and padRight to the specified value
	    /// </summary>
	    /// <param name="pad">Pad.</param>
	    public VerticalGroup SetPad(float pad)
        {
            _padTop = pad;
            _padLeft = pad;
            _padBottom = pad;
            _padRight = pad;
            return this;
        }


        public VerticalGroup SetPad(float top, float left, float bottom, float right)
        {
            _padTop = top;
            _padLeft = left;
            _padBottom = bottom;
            _padRight = right;
            return this;
        }


        public VerticalGroup SetPadTop(float padTop)
        {
            _padTop = padTop;
            return this;
        }


        public VerticalGroup SetPadLeft(float padLeft)
        {
            _padLeft = padLeft;
            return this;
        }


        public VerticalGroup SetPadBottom(float padBottom)
        {
            _padBottom = padBottom;
            return this;
        }


        public VerticalGroup SetPadRight(float padRight)
        {
            _padRight = padRight;
            return this;
        }


	    /// <summary>
	    ///     If true (the default), positions and sizes are rounded to integers.
	    /// </summary>
	    /// <param name="round">If set to <c>true</c> round.</param>
	    public VerticalGroup SetRound(bool round)
        {
            _round = round;
            return this;
        }


	    /// <summary>
	    ///     fill 0 will use pref width
	    /// </summary>
	    /// <param name="fill">Fill.</param>
	    public VerticalGroup SetFill(float fill)
        {
            _fill = fill;
            return this;
        }

        #endregion
    }
}