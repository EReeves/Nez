using System;

namespace Nez.UI
{
    public class Cell : IPoolable
    {
        private static readonly int Centeri = AlignInternal.Center;

        private static readonly int Topi = AlignInternal.Top;

        private static readonly int Bottomi = AlignInternal.Bottom;

        private static readonly int Lefti = AlignInternal.Left;

        private static readonly int Righti = AlignInternal.Right;

        private static bool _files;
        private static Cell _defaults;
        internal int? Align;
        internal int CellAboveIndex;
        internal int? Colspan;
        internal int Column, Row;
        internal float ComputedPadTop, ComputedPadLeft, ComputedPadBottom, ComputedPadRight;

        internal Element Element;
        internal float ElementWidth, ElementHeight;
        internal float ElementX, ElementY;
        internal bool EndRow;
        internal int? ExpandX, ExpandY;
        internal float? FillX, FillY;
        internal Value MaxWidth, MaxHeight;

        internal Value MinWidth, MinHeight;
        internal Value PadTop, PadLeft, PadBottom, PadRight;
        internal Value PrefWidth, PrefHeight;
        internal Value SpaceTop, SpaceLeft, SpaceBottom, SpaceRight;

        private Table _table;
        internal bool? UniformX, UniformY;


        public Cell()
        {
            Reset();
        }


	    /// <summary>
	    ///     Reset state so the cell can be reused, setting all constraints to their {@link #defaults() default} values.
	    /// </summary>
	    public void Reset()
        {
            Element = null;
            _table = null;
            EndRow = false;
            CellAboveIndex = -1;

            var defaults = GetDefaults();
            if (defaults != null)
                Set(defaults);
        }


        internal void SetLayout(Table table)
        {
            this._table = table;
        }


	    /// <summary>
	    ///     Returns the element for this cell casted to T, or null.
	    /// </summary>
	    /// <returns>The element.</returns>
	    public T GetElement<T>() where T : Element
        {
            return Element as T;
        }


	    /// <summary>
	    ///     Returns true if the cell's element is not null.
	    /// </summary>
	    /// <returns><c>true</c>, if element was hased, <c>false</c> otherwise.</returns>
	    public bool HasElement()
        {
            return Element != null;
        }


        public void SetElementBounds(float x, float y, float width, float height)
        {
            ElementX = x;
            ElementY = y;
            ElementWidth = width;
            ElementHeight = height;
        }


        public float GetElementX()
        {
            return ElementX;
        }


        public void SetElementX(float elementX)
        {
            this.ElementX = elementX;
        }


        public float GetElementY()
        {
            return ElementY;
        }


        public void SetElementY(float elementY)
        {
            this.ElementY = elementY;
        }


        public float GetElementWidth()
        {
            return ElementWidth;
        }


        public void SetElementWidth(float elementWidth)
        {
            this.ElementWidth = elementWidth;
        }


        public float GetElementHeight()
        {
            return ElementHeight;
        }


        public void SetElementHeight(float elementHeight)
        {
            this.ElementHeight = elementHeight;
        }


        public int GetColumn()
        {
            return Column;
        }


        public int GetRow()
        {
            return Row;
        }


	    /// <summary>
	    ///     May be null if this cell is row defaults.
	    /// </summary>
	    /// <returns>The minimum width value.</returns>
	    public Value GetMinWidthValue()
        {
            return MinWidth;
        }


        public float GetMinWidth()
        {
            return MinWidth.Get(Element);
        }


	    /// <summary>
	    ///     May be null if this cell is row defaults
	    /// </summary>
	    /// <returns>The minimum height value.</returns>
	    public Value GetMinHeightValue()
        {
            return MinHeight;
        }


        public float GetMinHeight()
        {
            return MinHeight.Get(Element);
        }


	    /// <summary>
	    ///     May be null if this cell is row defaults.
	    /// </summary>
	    /// <returns>The preference width value.</returns>
	    public Value GetPrefWidthValue()
        {
            return PrefWidth;
        }


        public float GetPrefWidth()
        {
            return PrefWidth.Get(Element);
        }


	    /// <summary>
	    ///     May be null if this cell is row defaults.
	    /// </summary>
	    /// <returns>The preference height value.</returns>
	    public Value GetPrefHeightValue()
        {
            return PrefHeight;
        }


        public float GetPrefHeight()
        {
            return PrefHeight.Get(Element);
        }


	    /// <summary>
	    ///     May be null if this cell is row defaults
	    /// </summary>
	    /// <returns>The max width value.</returns>
	    public Value GetMaxWidthValue()
        {
            return MaxWidth;
        }


        public float GetMaxWidth()
        {
            return MaxWidth.Get(Element);
        }


	    /// <summary>
	    ///     May be null if this cell is row defaults
	    /// </summary>
	    /// <returns>The max height value.</returns>
	    public Value GetMaxHeightValue()
        {
            return MaxHeight;
        }


        public float GetMaxHeight()
        {
            return MaxHeight.Get(Element);
        }


	    /// <summary>
	    ///     May be null if this value is not set
	    /// </summary>
	    /// <returns>The space top value.</returns>
	    public Value GetSpaceTopValue()
        {
            return SpaceTop;
        }


        public float GetSpaceTop()
        {
            return SpaceTop.Get(Element);
        }


	    /// <summary>
	    ///     May be null if this value is not set.
	    /// </summary>
	    /// <returns>The space left value.</returns>
	    public Value GetSpaceLeftValue()
        {
            return SpaceLeft;
        }


        public float GetSpaceLeft()
        {
            return SpaceLeft.Get(Element);
        }


	    /// <summary>
	    ///     May be null if this value is not set
	    /// </summary>
	    /// <returns>The space bottom value.</returns>
	    public Value GetSpaceBottomValue()
        {
            return SpaceBottom;
        }


        public float GetSpaceBottom()
        {
            return SpaceBottom.Get(Element);
        }


	    /// <summary>
	    ///     May be null if this value is not set
	    /// </summary>
	    /// <returns>The space right value.</returns>
	    public Value GetSpaceRightValue()
        {
            return SpaceRight;
        }


        public float GetSpaceRight()
        {
            return SpaceRight.Get(Element);
        }


	    /// <summary>
	    ///     May be null if this value is not set
	    /// </summary>
	    /// <returns>The pad top value.</returns>
	    public Value GetPadTopValue()
        {
            return PadTop;
        }


        public float GetPadTop()
        {
            return PadTop.Get(Element);
        }


	    /// <summary>
	    ///     May be null if this value is not set
	    /// </summary>
	    /// <returns>The pad left value.</returns>
	    public Value GetPadLeftValue()
        {
            return PadLeft;
        }


        public float GetPadLeft()
        {
            return PadLeft.Get(Element);
        }


	    /// <summary>
	    ///     May be null if this value is not set
	    /// </summary>
	    /// <returns>The pad bottom value.</returns>
	    public Value GetPadBottomValue()
        {
            return PadBottom;
        }


        public float GetPadBottom()
        {
            return PadBottom.Get(Element);
        }


	    /// <summary>
	    ///     May be null if this value is not set
	    /// </summary>
	    /// <returns>The pad right value.</returns>
	    public Value GetPadRightValue()
        {
            return PadRight;
        }


        public float GetPadRight()
        {
            return PadRight.Get(Element);
        }


	    /// <summary>
	    ///     Returns {@link #getPadLeft()} plus {@link #getPadRight()}
	    /// </summary>
	    /// <returns>The pad x.</returns>
	    public float GetPadX()
        {
            return PadLeft.Get(Element) + PadRight.Get(Element);
        }


	    /// <summary>
	    ///     Returns {@link #getPadTop()} plus {@link #getPadBottom()}
	    /// </summary>
	    /// <returns>The pad y.</returns>
	    public float GetPadY()
        {
            return PadTop.Get(Element) + PadBottom.Get(Element);
        }


	    /// <summary>
	    ///     May be null if this value is not set
	    /// </summary>
	    /// <returns>The fill x.</returns>
	    public float? GetFillX()
        {
            return FillX;
        }


	    /// <summary>
	    ///     May be null
	    /// </summary>
	    /// <returns>The fill y.</returns>
	    public float? GetFillY()
        {
            return FillY;
        }


	    /// <summary>
	    ///     May be null
	    /// </summary>
	    /// <returns>The align.</returns>
	    public int? GetAlign()
        {
            return Align;
        }


	    /// <summary>
	    ///     May be null
	    /// </summary>
	    /// <returns>The expand x.</returns>
	    public int? GetExpandX()
        {
            return ExpandX;
        }


	    /// <summary>
	    ///     May be null
	    /// </summary>
	    /// <returns>The expand y.</returns>
	    public int? GetExpandY()
        {
            return ExpandY;
        }


	    /// <summary>
	    ///     May be null
	    /// </summary>
	    /// <returns>The colspan.</returns>
	    public int? GetColspan()
        {
            return Colspan;
        }


	    /// <summary>
	    ///     May be null
	    /// </summary>
	    /// <returns>The uniform x.</returns>
	    public bool? GetUniformX()
        {
            return UniformX;
        }


	    /// <summary>
	    ///     May be null
	    /// </summary>
	    /// <returns>The uniform y.</returns>
	    public bool? GetUniformY()
        {
            return UniformY;
        }


	    /// <summary>
	    ///     May be null
	    /// </summary>
	    /// <returns><c>true</c>, if end row was ised, <c>false</c> otherwise.</returns>
	    public bool IsEndRow()
        {
            return EndRow;
        }


	    /// <summary>
	    ///     The actual amount of combined padding and spacing from the last layout.
	    /// </summary>
	    /// <returns>The computed pad top.</returns>
	    public float GetComputedPadTop()
        {
            return ComputedPadTop;
        }


	    /// <summary>
	    ///     The actual amount of combined padding and spacing from the last layout.
	    /// </summary>
	    /// <returns>The computed pad left.</returns>
	    public float GetComputedPadLeft()
        {
            return ComputedPadLeft;
        }


	    /// <summary>
	    ///     The actual amount of combined padding and spacing from the last layout
	    /// </summary>
	    /// <returns>The computed pad bottom.</returns>
	    public float GetComputedPadBottom()
        {
            return ComputedPadBottom;
        }


	    /// <summary>
	    ///     The actual amount of combined padding and spacing from the last layout
	    /// </summary>
	    /// <returns>The computed pad right.</returns>
	    public float GetComputedPadRight()
        {
            return ComputedPadRight;
        }


        public void SetRow()
        {
            _table.Row();
        }


        public Table GetTable()
        {
            return _table;
        }


	    /// <summary>
	    ///     Returns the defaults to use for all cells. This can be used to avoid needing to set the same defaults for every
	    ///     table (eg,
	    ///     for spacing).
	    /// </summary>
	    /// <returns>The defaults.</returns>
	    public static Cell GetDefaults()
        {
            if (!_files)
            {
                _files = true;
                _defaults = new Cell();
                _defaults.MinWidth = Value.MinWidth;
                _defaults.MinHeight = Value.MinHeight;
                _defaults.PrefWidth = Value.PrefWidth;
                _defaults.PrefHeight = Value.PrefHeight;
                _defaults.MaxWidth = Value.MaxWidth;
                _defaults.MaxHeight = Value.MaxHeight;
                _defaults.SpaceTop = Value.Zero;
                _defaults.SpaceLeft = Value.Zero;
                _defaults.SpaceBottom = Value.Zero;
                _defaults.SpaceRight = Value.Zero;
                _defaults.PadTop = Value.Zero;
                _defaults.PadLeft = Value.Zero;
                _defaults.PadBottom = Value.Zero;
                _defaults.PadRight = Value.Zero;
                _defaults.FillX = 0f;
                _defaults.FillY = 0f;
                _defaults.Align = Centeri;
                _defaults.ExpandX = 0;
                _defaults.ExpandY = 0;
                _defaults.Colspan = 1;
                _defaults.UniformX = null;
                _defaults.UniformY = null;
            }
            return _defaults;
        }


	    /// <summary>
	    ///     Sets all constraint fields to null
	    /// </summary>
	    public void Clear()
        {
            MinWidth = null;
            MinHeight = null;
            PrefWidth = null;
            PrefHeight = null;
            MaxWidth = null;
            MaxHeight = null;
            SpaceTop = null;
            SpaceLeft = null;
            SpaceBottom = null;
            SpaceRight = null;
            PadTop = null;
            PadLeft = null;
            PadBottom = null;
            PadRight = null;
            FillX = null;
            FillY = null;
            Align = null;
            ExpandX = null;
            ExpandY = null;
            Colspan = null;
            UniformX = null;
            UniformY = null;
        }


        public void Set(Cell cell)
        {
            MinWidth = cell.MinWidth;
            MinHeight = cell.MinHeight;
            PrefWidth = cell.PrefWidth;
            PrefHeight = cell.PrefHeight;
            MaxWidth = cell.MaxWidth;
            MaxHeight = cell.MaxHeight;
            SpaceTop = cell.SpaceTop;
            SpaceLeft = cell.SpaceLeft;
            SpaceBottom = cell.SpaceBottom;
            SpaceRight = cell.SpaceRight;
            PadTop = cell.PadTop;
            PadLeft = cell.PadLeft;
            PadBottom = cell.PadBottom;
            PadRight = cell.PadRight;
            FillX = cell.FillX;
            FillY = cell.FillY;
            Align = cell.Align;
            ExpandX = cell.ExpandX;
            ExpandY = cell.ExpandY;
            Colspan = cell.Colspan;
            UniformX = cell.UniformX;
            UniformY = cell.UniformY;
        }


	    /// <summary>
	    ///     cell may be null
	    /// </summary>
	    /// <param name="cell">Cell.</param>
	    public void Merge(Cell cell)
        {
            if (cell == null)
                return;

            if (cell.MinWidth != null)
                MinWidth = cell.MinWidth;
            if (cell.MinHeight != null)
                MinHeight = cell.MinHeight;
            if (cell.PrefWidth != null)
                PrefWidth = cell.PrefWidth;
            if (cell.PrefHeight != null)
                PrefHeight = cell.PrefHeight;
            if (cell.MaxWidth != null)
                MaxWidth = cell.MaxWidth;
            if (cell.MaxHeight != null)
                MaxHeight = cell.MaxHeight;
            if (cell.SpaceTop != null)
                SpaceTop = cell.SpaceTop;
            if (cell.SpaceLeft != null)
                SpaceLeft = cell.SpaceLeft;
            if (cell.SpaceBottom != null)
                SpaceBottom = cell.SpaceBottom;
            if (cell.SpaceRight != null)
                SpaceRight = cell.SpaceRight;
            if (cell.PadTop != null)
                PadTop = cell.PadTop;
            if (cell.PadLeft != null)
                PadLeft = cell.PadLeft;
            if (cell.PadBottom != null)
                PadBottom = cell.PadBottom;
            if (cell.PadRight != null)
                PadRight = cell.PadRight;
            if (cell.FillX != null)
                FillX = cell.FillX;
            if (cell.FillY != null)
                FillY = cell.FillY;
            if (cell.Align != null)
                Align = cell.Align;
            if (cell.ExpandX != null)
                ExpandX = cell.ExpandX;
            if (cell.ExpandY != null)
                ExpandY = cell.ExpandY;
            if (cell.Colspan != null)
                Colspan = cell.Colspan;
            if (cell.UniformX != null)
                UniformX = cell.UniformX;
            if (cell.UniformY != null)
                UniformY = cell.UniformY;
        }


        public override string ToString()
        {
            return Element != null ? Element.ToString() : base.ToString();
        }


        #region Chainable configuration

	    /// <summary>
	    ///     Sets the element in this cell and adds the element to the cell's table. If null, removes any current element.
	    /// </summary>
	    /// <returns>The element.</returns>
	    /// <param name="newelement">New element.</param>
	    public Cell SetElement(Element newElement)
        {
            if (Element != newElement)
            {
                if (Element != null)
                    Element.Remove();
                Element = newElement;
                if (newElement != null)
                    _table.AddElement(newElement);
            }
            return this;
        }


	    /// <summary>
	    ///     Removes the current element for the cell, if any.
	    /// </summary>
	    /// <returns>The element.</returns>
	    public Cell ClearElement()
        {
            SetElement(null);
            return this;
        }


	    /// <summary>
	    ///     Sets the minWidth, prefWidth, maxWidth, minHeight, prefHeight, and maxHeight to the specified value.
	    /// </summary>
	    /// <param name="size">Size.</param>
	    public Cell Size(Value size)
        {
            Assert.IsNotNull(size, "size cannot be null.");

            MinWidth = size;
            MinHeight = size;
            PrefWidth = size;
            PrefHeight = size;
            MaxWidth = size;
            MaxHeight = size;
            return this;
        }


	    /// <summary>
	    ///     Sets the minWidth, prefWidth, maxWidth, minHeight, prefHeight, and maxHeight to the specified values.
	    /// </summary>
	    /// <param name="width">Width.</param>
	    /// <param name="height">Height.</param>
	    public Cell Size(Value width, Value height)
        {
            Assert.IsNotNull(width, "width cannot be null.");
            Assert.IsNotNull(height, "height cannot be null.");

            MinWidth = width;
            MinHeight = height;
            PrefWidth = width;
            PrefHeight = height;
            MaxWidth = width;
            MaxHeight = height;
            return this;
        }


	    /// <summary>
	    ///     Sets the minWidth, prefWidth, maxWidth, minHeight, prefHeight, and maxHeight to the specified value.
	    /// </summary>
	    /// <param name="size">Size.</param>
	    public Cell Size(float size)
        {
            return this.Size(new Value.Fixed(size));
        }


	    /// <summary>
	    ///     Sets the minWidth, prefWidth, maxWidth, minHeight, prefHeight, and maxHeight to the specified values.
	    /// </summary>
	    /// <param name="width">Width.</param>
	    /// <param name="height">Height.</param>
	    public Cell Size(float width, float height)
        {
            return Size(new Value.Fixed(width), new Value.Fixed(height));
        }


	    /// <summary>
	    ///     Sets the minWidth, prefWidth, and maxWidth to the specified value.
	    /// </summary>
	    /// <param name="width">Width.</param>
	    public Cell Width(Value width)
        {
            if (width == null)
                throw new Exception("width cannot be null.");
            MinWidth = width;
            PrefWidth = width;
            MaxWidth = width;
            return this;
        }


	    /// <summary>
	    ///     Sets the minWidth, prefWidth, and maxWidth to the specified value.
	    /// </summary>
	    /// <param name="width">Width.</param>
	    public Cell Width(float width)
        {
            return this.Width(new Value.Fixed(width));
        }


	    /// <summary>
	    ///     Sets the minHeight, prefHeight, and maxHeight to the specified value.
	    /// </summary>
	    /// <param name="height">Height.</param>
	    public Cell Height(Value height)
        {
            if (height == null)
                throw new Exception("height cannot be null.");
            MinHeight = height;
            PrefHeight = height;
            MaxHeight = height;

            return this;
        }


	    /// <summary>
	    ///     Sets the minHeight, prefHeight, and maxHeight to the specified value.
	    /// </summary>
	    /// <param name="height">Height.</param>
	    public Cell Height(float height)
        {
            return this.Height(new Value.Fixed(height));
        }


	    /// <summary>
	    ///     Sets the minWidth and minHeight to the specified value.
	    /// </summary>
	    /// <returns>The size.</returns>
	    /// <param name="size">Size.</param>
	    public Cell MinSize(Value size)
        {
            if (size == null)
                throw new Exception("size cannot be null.");
            MinWidth = size;
            MinHeight = size;
            return this;
        }


	    /// <summary>
	    ///     Sets the minWidth and minHeight to the specified values.
	    /// </summary>
	    /// <returns>The size.</returns>
	    /// <param name="width">Width.</param>
	    /// <param name="height">Height.</param>
	    public Cell MinSize(Value width, Value height)
        {
            if (width == null)
                throw new Exception("width cannot be null.");
            if (height == null)
                throw new Exception("height cannot be null.");
            MinWidth = width;
            MinHeight = height;
            return this;
        }


        public Cell SetMinWidth(Value minWidth)
        {
            if (minWidth == null)
                throw new Exception("minWidth cannot be null.");
            this.MinWidth = minWidth;
            return this;
        }


        public Cell SetMinHeight(Value minHeight)
        {
            if (minHeight == null)
                throw new Exception("minHeight cannot be null.");
            this.MinHeight = minHeight;
            return this;
        }


	    /// <summary>
	    ///     Sets the minWidth and minHeight to the specified value.
	    /// </summary>
	    /// <returns>The size.</returns>
	    /// <param name="size">Size.</param>
	    public Cell MinSize(float size)
        {
            MinSize(new Value.Fixed(size));
            return this;
        }


	    /// <summary>
	    ///     Sets the minWidth and minHeight to the specified values.
	    /// </summary>
	    /// <returns>The size.</returns>
	    /// <param name="width">Width.</param>
	    /// <param name="height">Height.</param>
	    public Cell MinSize(float width, float height)
        {
            MinSize(new Value.Fixed(width), new Value.Fixed(height));
            return this;
        }


        public Cell SetMinWidth(float minWidth)
        {
            this.MinWidth = new Value.Fixed(minWidth);
            return this;
        }


        public Cell SetMinHeight(float minHeight)
        {
            this.MinHeight = new Value.Fixed(minHeight);
            return this;
        }


	    /// <summary>
	    ///     Sets the prefWidth and prefHeight to the specified value.
	    /// </summary>
	    /// <returns>The size.</returns>
	    /// <param name="size">Size.</param>
	    public Cell PrefSize(Value size)
        {
            if (size == null)
                throw new Exception("size cannot be null.");
            PrefWidth = size;
            PrefHeight = size;
            return this;
        }


	    /// <summary>
	    ///     Sets the prefWidth and prefHeight to the specified values.
	    /// </summary>
	    /// <returns>The size.</returns>
	    /// <param name="width">Width.</param>
	    /// <param name="height">Height.</param>
	    public Cell PrefSize(Value width, Value height)
        {
            if (width == null)
                throw new Exception("width cannot be null.");
            if (height == null)
                throw new Exception("height cannot be null.");
            PrefWidth = width;
            PrefHeight = height;
            return this;
        }


        public Cell SetPrefWidth(Value prefWidth)
        {
            if (prefWidth == null)
                throw new Exception("prefWidth cannot be null.");
            this.PrefWidth = prefWidth;
            return this;
        }


        public Cell SetPrefHeight(Value prefHeight)
        {
            if (prefHeight == null)
                throw new Exception("prefHeight cannot be null.");
            this.PrefHeight = prefHeight;
            return this;
        }


	    /// <summary>
	    ///     Sets the prefWidth and prefHeight to the specified value.
	    /// </summary>
	    /// <returns>The size.</returns>
	    /// <param name="width">Width.</param>
	    /// <param name="height">Height.</param>
	    public Cell PrefSize(float width, float height)
        {
            PrefSize(new Value.Fixed(width), new Value.Fixed(height));
            return this;
        }


	    /// <summary>
	    ///     Sets the prefWidth and prefHeight to the specified values.
	    /// </summary>
	    /// <returns>The size.</returns>
	    /// <param name="size">Size.</param>
	    public Cell PrefSize(float size)
        {
            PrefSize(new Value.Fixed(size));
            return this;
        }


        public Cell SetPrefWidth(float prefWidth)
        {
            this.PrefWidth = new Value.Fixed(prefWidth);
            return this;
        }


        public Cell SetPrefHeight(float prefHeight)
        {
            this.PrefHeight = new Value.Fixed(prefHeight);
            return this;
        }


	    /// <summary>
	    ///     Sets the maxWidth and maxHeight to the specified value.
	    /// </summary>
	    /// <returns>The size.</returns>
	    /// <param name="size">Size.</param>
	    public Cell MaxSize(Value size)
        {
            if (size == null)
                throw new Exception("size cannot be null.");
            MaxWidth = size;
            MaxHeight = size;
            return this;
        }


	    /// <summary>
	    ///     Sets the maxWidth and maxHeight to the specified values.
	    /// </summary>
	    /// <returns>The size.</returns>
	    /// <param name="width">Width.</param>
	    /// <param name="height">Height.</param>
	    public Cell MaxSize(Value width, Value height)
        {
            if (width == null)
                throw new Exception("width cannot be null.");
            if (height == null)
                throw new Exception("height cannot be null.");
            MaxWidth = width;
            MaxHeight = height;
            return this;
        }


        public Cell SetMaxWidth(Value maxWidth)
        {
            if (maxWidth == null)
                throw new Exception("maxWidth cannot be null.");
            this.MaxWidth = maxWidth;
            return this;
        }


        public Cell SetMaxHeight(Value maxHeight)
        {
            if (maxHeight == null)
                throw new Exception("maxHeight cannot be null.");
            this.MaxHeight = maxHeight;
            return this;
        }


	    /// <summary>
	    ///     Sets the maxWidth and maxHeight to the specified value.
	    /// </summary>
	    /// <returns>The size.</returns>
	    /// <param name="size">Size.</param>
	    public Cell MaxSize(float size)
        {
            MaxSize(new Value.Fixed(size));
            return this;
        }


	    /// <summary>
	    ///     Sets the maxWidth and maxHeight to the specified values.
	    /// </summary>
	    /// <returns>The size.</returns>
	    /// <param name="width">Width.</param>
	    /// <param name="height">Height.</param>
	    public Cell MaxSize(float width, float height)
        {
            MaxSize(new Value.Fixed(width), new Value.Fixed(height));
            return this;
        }


        public Cell SetMaxWidth(float maxWidth)
        {
            this.MaxWidth = new Value.Fixed(maxWidth);
            return this;
        }


        public Cell SetMaxHeight(float maxHeight)
        {
            this.MaxHeight = new Value.Fixed(maxHeight);
            return this;
        }


	    /// <summary>
	    ///     Sets the spaceTop, spaceLeft, spaceBottom, and spaceRight to the specified value.
	    /// </summary>
	    /// <param name="space">Space.</param>
	    public Cell Space(Value space)
        {
            if (space == null)
                throw new Exception("space cannot be null.");
            SpaceTop = space;
            SpaceLeft = space;
            SpaceBottom = space;
            SpaceRight = space;
            return this;
        }


        public Cell Space(Value top, Value left, Value bottom, Value right)
        {
            if (top == null)
                throw new Exception("top cannot be null.");
            if (left == null)
                throw new Exception("left cannot be null.");
            if (bottom == null)
                throw new Exception("bottom cannot be null.");
            if (right == null)
                throw new Exception("right cannot be null.");
            SpaceTop = top;
            SpaceLeft = left;
            SpaceBottom = bottom;
            SpaceRight = right;
            return this;
        }


        public Cell SetSpaceTop(Value spaceTop)
        {
            if (spaceTop == null)
                throw new Exception("spaceTop cannot be null.");
            this.SpaceTop = spaceTop;
            return this;
        }


        public Cell SetSpaceLeft(Value spaceLeft)
        {
            if (spaceLeft == null)
                throw new Exception("spaceLeft cannot be null.");
            this.SpaceLeft = spaceLeft;
            return this;
        }


        public Cell SetSpaceBottom(Value spaceBottom)
        {
            if (spaceBottom == null)
                throw new Exception("spaceBottom cannot be null.");
            this.SpaceBottom = spaceBottom;
            return this;
        }


        public Cell SetSpaceRight(Value spaceRight)
        {
            if (spaceRight == null)
                throw new Exception("spaceRight cannot be null.");
            this.SpaceRight = spaceRight;
            return this;
        }


	    /// <summary>
	    ///     Sets the spaceTop, spaceLeft, spaceBottom, and spaceRight to the specified value.
	    /// </summary>
	    /// <param name="space">Space.</param>
	    public Cell Space(float space)
        {
            if (space < 0)
                throw new Exception("space cannot be < 0.");
            return this.Space(new Value.Fixed(space));
        }


        public Cell Space(float top, float left, float bottom, float right)
        {
            if (top < 0)
                throw new Exception("top cannot be < 0.");
            if (left < 0)
                throw new Exception("left cannot be < 0.");
            if (bottom < 0)
                throw new Exception("bottom cannot be < 0.");
            if (right < 0)
                throw new Exception("right cannot be < 0.");
            Space(new Value.Fixed(top), new Value.Fixed(left), new Value.Fixed(bottom), new Value.Fixed(right));
            return this;
        }


        public Cell SetSpaceTop(float spaceTop)
        {
            if (spaceTop < 0)
                throw new Exception("spaceTop cannot be < 0.");
            this.SpaceTop = new Value.Fixed(spaceTop);
            return this;
        }


        public Cell SetSpaceLeft(float spaceLeft)
        {
            if (spaceLeft < 0)
                throw new Exception("spaceLeft cannot be < 0.");
            this.SpaceLeft = new Value.Fixed(spaceLeft);
            return this;
        }


        public Cell SetSpaceBottom(float spaceBottom)
        {
            if (spaceBottom < 0)
                throw new Exception("spaceBottom cannot be < 0.");
            this.SpaceBottom = new Value.Fixed(spaceBottom);
            return this;
        }


        public Cell SetSpaceRight(float spaceRight)
        {
            if (spaceRight < 0)
                throw new Exception("spaceRight cannot be < 0.");
            this.SpaceRight = new Value.Fixed(spaceRight);
            return this;
        }


	    /// <summary>
	    ///     Sets the padTop, padLeft, padBottom, and padRight to the specified value.
	    /// </summary>
	    /// <param name="pad">Pad.</param>
	    public Cell Pad(Value pad)
        {
            if (pad == null)
                throw new Exception("pad cannot be null.");
            PadTop = pad;
            PadLeft = pad;
            PadBottom = pad;
            PadRight = pad;
            return this;
        }


        public Cell Pad(Value top, Value left, Value bottom, Value right)
        {
            if (top == null)
                throw new Exception("top cannot be null.");
            if (left == null)
                throw new Exception("left cannot be null.");
            if (bottom == null)
                throw new Exception("bottom cannot be null.");
            if (right == null)
                throw new Exception("right cannot be null.");
            PadTop = top;
            PadLeft = left;
            PadBottom = bottom;
            PadRight = right;
            return this;
        }


        public Cell SetPadTop(Value padTop)
        {
            if (padTop == null)
                throw new Exception("padTop cannot be null.");
            this.PadTop = padTop;
            return this;
        }


        public Cell SetPadLeft(Value padLeft)
        {
            if (padLeft == null)
                throw new Exception("padLeft cannot be null.");
            this.PadLeft = padLeft;
            return this;
        }


        public Cell SetPadBottom(Value padBottom)
        {
            if (padBottom == null)
                throw new Exception("padBottom cannot be null.");
            this.PadBottom = padBottom;
            return this;
        }


        public Cell SetPadRight(Value padRight)
        {
            if (padRight == null)
                throw new Exception("padRight cannot be null.");
            this.PadRight = padRight;
            return this;
        }


	    /// <summary>
	    ///     Sets the padTop, padLeft, padBottom, and padRight to the specified value.
	    /// </summary>
	    /// <param name="pad">Pad.</param>
	    public Cell Pad(float pad)
        {
            return this.Pad(new Value.Fixed(pad));
        }


        public Cell Pad(float top, float left, float bottom, float right)
        {
            Pad(new Value.Fixed(top), new Value.Fixed(left), new Value.Fixed(bottom), new Value.Fixed(right));
            return this;
        }


        public Cell SetPadTop(float padTop)
        {
            this.PadTop = new Value.Fixed(padTop);
            return this;
        }


        public Cell SetPadLeft(float padLeft)
        {
            this.PadLeft = new Value.Fixed(padLeft);
            return this;
        }


        public Cell SetPadBottom(float padBottom)
        {
            this.PadBottom = new Value.Fixed(padBottom);
            return this;
        }


        public Cell SetPadRight(float padRight)
        {
            this.PadRight = new Value.Fixed(padRight);
            return this;
        }


	    /// <summary>
	    ///     Sets fillX and fillY to 1
	    /// </summary>
	    public Cell Fill()
        {
            FillX = 1f;
            FillY = 1f;
            return this;
        }


	    /// <summary>
	    ///     Sets fillX to 1
	    /// </summary>
	    /// <returns>The fill x.</returns>
	    public Cell SetFillX()
        {
            FillX = 1f;
            return this;
        }


	    /// <summary>
	    ///     Sets fillY to 1
	    /// </summary>
	    /// <returns>The fill y.</returns>
	    public Cell SetFillY()
        {
            FillY = 1f;
            return this;
        }


        public Cell Fill(float x, float y)
        {
            FillX = x;
            FillY = y;
            return this;
        }


	    /// <summary>
	    ///     Sets fillX and fillY to 1 if true, 0 if false.
	    /// </summary>
	    /// <param name="x">If set to <c>true</c> x.</param>
	    /// <param name="y">If set to <c>true</c> y.</param>
	    public Cell Fill(bool x, bool y)
        {
            FillX = x ? 1f : 0f;
            FillY = y ? 1f : 0f;
            return this;
        }


	    /// <summary>
	    ///     Sets fillX and fillY to 1 if true, 0 if false.
	    /// </summary>
	    /// <param name="fill">If set to <c>true</c> fill.</param>
	    public Cell Fill(bool fill)
        {
            FillX = fill ? 1f : 0f;
            FillY = fill ? 1f : 0f;
            return this;
        }


	    /// <summary>
	    ///     Sets the alignment of the element within the cell. Set to {@link Align#center}, {@link Align#top}, {@link
	    ///     Align#bottom},
	    ///     {@link Align#left}, {@link Align#right}, or any combination of those.
	    /// </summary>
	    /// <returns>The align.</returns>
	    /// <param name="align">Align.</param>
	    public Cell SetAlign(Align align)
        {
            this.Align = (int) align;
            return this;
        }


	    /// <summary>
	    ///     Sets the alignment of the element within the cell to {@link Align#center}. This clears any other alignment.
	    /// </summary>
	    public Cell Center()
        {
            Align = Centeri;
            return this;
        }


	    /// <summary>
	    ///     Adds {@link Align#top} and clears {@link Align#bottom} for the alignment of the element within the cell.
	    /// </summary>
	    public Cell Top()
        {
            if (Align == null)
                Align = Topi;
            else
                Align = (Align | AlignInternal.Top) & ~AlignInternal.Bottom;
            return this;
        }


	    /// <summary>
	    ///     Adds {@link Align#left} and clears {@link Align#right} for the alignment of the element within the cell
	    /// </summary>
	    public Cell Left()
        {
            if (Align == null)
                Align = Lefti;
            else
                Align = (Align | AlignInternal.Left) & ~AlignInternal.Right;
            return this;
        }


	    /// <summary>
	    ///     Adds {@link Align#bottom} and clears {@link Align#top} for the alignment of the element within the cell
	    /// </summary>
	    public Cell Bottom()
        {
            if (Align == null)
                Align = Bottomi;
            else
                Align = (Align | AlignInternal.Bottom) & ~AlignInternal.Top;
            return this;
        }


	    /// <summary>
	    ///     Adds {@link Align#right} and clears {@link Align#left} for the alignment of the element within the cell
	    /// </summary>
	    public Cell Right()
        {
            if (Align == null)
                Align = Righti;
            else
                Align = (Align | AlignInternal.Right) & ~AlignInternal.Left;
            return this;
        }


	    /// <summary>
	    ///     Sets expandX, expandY, fillX, and fillY to 1
	    /// </summary>
	    public Cell Grow()
        {
            ExpandX = 1;
            ExpandY = 1;
            FillX = 1f;
            FillY = 1f;
            return this;
        }


	    /// <summary>
	    ///     Sets expandX and fillX to 1
	    /// </summary>
	    /// <returns>The x.</returns>
	    public Cell GrowX()
        {
            ExpandX = 1;
            FillX = 1f;
            return this;
        }


	    /// <summary>
	    ///     Sets expandY and fillY to 1
	    /// </summary>
	    /// <returns>The y.</returns>
	    public Cell GrowY()
        {
            ExpandY = 1;
            FillY = 1f;
            return this;
        }


	    /// <summary>
	    ///     Sets expandX and expandY to 1
	    /// </summary>
	    public Cell Expand()
        {
            ExpandX = 1;
            ExpandY = 1;
            return this;
        }


	    /// <summary>
	    ///     Sets expandX to 1
	    /// </summary>
	    /// <returns>The expand x.</returns>
	    public Cell SetExpandX()
        {
            ExpandX = 1;
            return this;
        }


	    /// <summary>
	    ///     Sets expandY to 1
	    /// </summary>
	    /// <returns>The expand y.</returns>
	    public Cell SetExpandY()
        {
            ExpandY = 1;
            return this;
        }


        public Cell Expand(int x, int y)
        {
            ExpandX = x;
            ExpandY = y;
            return this;
        }


	    /// <summary>
	    ///     Sets expandX and expandY to 1 if true, 0 if false
	    /// </summary>
	    /// <param name="x">If set to <c>true</c> x.</param>
	    /// <param name="y">If set to <c>true</c> y.</param>
	    public Cell Expand(bool x, bool y)
        {
            ExpandX = x ? 1 : 0;
            ExpandY = y ? 1 : 0;
            return this;
        }


        public Cell SetColspan(int colspan)
        {
            this.Colspan = colspan;
            return this;
        }


	    /// <summary>
	    ///     Sets uniformX and uniformY to true
	    /// </summary>
	    public Cell Uniform()
        {
            UniformX = true;
            UniformY = true;
            return this;
        }


	    /// <summary>
	    ///     Sets uniformX to true
	    /// </summary>
	    public Cell SetUniformX()
        {
            UniformX = true;
            return this;
        }


	    /// <summary>
	    ///     Sets uniformY to true
	    /// </summary>
	    public Cell SetUniformY()
        {
            UniformY = true;
            return this;
        }


        public Cell Uniform(bool x, bool y)
        {
            UniformX = x;
            UniformY = y;
            return this;
        }

        #endregion
    }
}