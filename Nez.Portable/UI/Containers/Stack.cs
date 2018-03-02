using Nez.UI.Base;

namespace Nez.UI.Containers
{
	/// <summary>
	///     A stack is a container that sizes its children to its size and positions them at 0,0 on top of each other.
	///     The preferred and min size of the stack is the largest preferred and min size of any children. The max size of the
	///     stack is the
	///     smallest max size of any children.
	/// </summary>
	public class Stack : Group
    {
        private bool _sizeInvalid = true;


        public Stack()
        {
            Transform = false;
            SetSize(150, 150);
            Touchable = Touchable.ChildrenOnly;
        }


        public override void Invalidate()
        {
            base.Invalidate();
            _sizeInvalid = true;
        }


        private void ComputeSize()
        {
            _sizeInvalid = false;
            _prefWidth = 0;
            _prefHeight = 0;
            _minWidth = 0;
            _minHeight = 0;
            _maxWidth = 0;
            _maxHeight = 0;

            for (int i = 0, n = Children.Count; i < n; i++)
            {
                var child = Children[i];
                float childMaxWidth, childMaxHeight;
                if (child is ILayout)
                {
                    var layout = (ILayout) child;
                    _prefWidth = System.Math.Max(_prefWidth, layout.PreferredWidth);
                    _prefHeight = System.Math.Max(_prefHeight, layout.PreferredHeight);
                    _minWidth = System.Math.Max(_minWidth, layout.MinWidth);
                    _minHeight = System.Math.Max(_minHeight, layout.MinHeight);
                    childMaxWidth = layout.MaxWidth;
                    childMaxHeight = layout.MaxHeight;
                }
                else
                {
                    _prefWidth = System.Math.Max(_prefWidth, child.Width);
                    _prefHeight = System.Math.Max(_prefHeight, child.Height);
                    _minWidth = System.Math.Max(_minWidth, child.Width);
                    _minHeight = System.Math.Max(_minHeight, child.Height);
                    childMaxWidth = 0;
                    childMaxHeight = 0;
                }

                if (childMaxWidth > 0)
                    _maxWidth = _maxWidth == 0 ? childMaxWidth : System.Math.Min(_maxWidth, childMaxWidth);
                if (childMaxHeight > 0)
                    _maxHeight = _maxHeight == 0 ? childMaxHeight : System.Math.Min(_maxHeight, childMaxHeight);
            }
        }


        public T Add<T>(T element) where T : Element
        {
            return AddElement(element);
        }


        public override void Layout()
        {
            if (_sizeInvalid)
                ComputeSize();

            for (int i = 0, n = Children.Count; i < n; i++)
            {
                var child = Children[i];
                child.SetBounds(0, 0, Width, Height);
                if (child is ILayout)
                    ((ILayout) child).Validate();
            }
        }

        #region ILayout

        public override float MinWidth
        {
            get
            {
                if (_sizeInvalid)
                    ComputeSize();
                return _minWidth;
            }
        }

        public override float MinHeight
        {
            get
            {
                if (_sizeInvalid)
                    ComputeSize();
                return _minHeight;
            }
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

        public override float MaxWidth
        {
            get
            {
                if (_sizeInvalid)
                    ComputeSize();
                return _maxWidth;
            }
        }

        public override float MaxHeight
        {
            get
            {
                if (_sizeInvalid)
                    ComputeSize();
                return _maxHeight;
            }
        }

        private float _prefWidth, _prefHeight, _minWidth, _minHeight, _maxWidth, _maxHeight;

        #endregion
    }
}