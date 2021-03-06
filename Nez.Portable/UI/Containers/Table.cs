﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Nez.Graphics.Batcher;
using Nez.Maths;
using Nez.UI.Base;
using Nez.UI.Widgets;
using Nez.Utils;
using IDrawable = Nez.UI.Drawable.IDrawable;

namespace Nez.UI.Containers
{
	/// <summary>
	///     A group that sizes and positions children using table constraints. By default, {@link #getTouchable()} is
	///     {@link Touchable#childrenOnly}.
	///     The preferred and minimum sizes are that of the chdebugn when laid out in columns and rows.
	/// </summary>
	public class Table : Group
    {
        public enum TableDebug
        {
            None,
            All,
            Table,
            Cell,
            Element
        }

        public static Color DebugTableColor = new Color(0, 0, 255, 255);
        public static Color DebugCellColor = new Color(255, 0, 0, 255);
        public static Color DebugElementColor = new Color(0, 255, 0, 255);
        private static float[] _columnWeightedWidth, _rowWeightedHeight;
        private int _align = AlignInternal.Center;

        protected IDrawable Background;
        private readonly Cell _cellDefaults;

        private readonly List<Cell> _cells = new List<Cell>(4);
        private readonly List<Cell> _columnDefaults = new List<Cell>(2);
        private float[] _columnMinWidth, _rowMinHeight;
        private float[] _columnPrefWidth, _rowPrefHeight;


        private int _columns, _rows;
        private float[] _columnWidth, _rowHeight;
        private List<DebugRectangleF> _debugRects;
        private float[] _expandWidth, _expandHeight;
        private bool _implicitEndRow;

        private Value _padTop = BackgroundTop,
            _padLeft = BackgroundLeft,
            _padBottom = BackgroundBottom,
            _padRight = BackgroundRight;

        private bool _round = true;
        private Cell _rowDefaults;

        private bool _sizeInvalid = true;

        private TableDebug _tableDebug = TableDebug.None;
        private float _tableMinWidth, _tableMinHeight;
        private float _tablePrefWidth, _tablePrefHeight;
        public bool Clip;


        public Table()
        {
            _cellDefaults = ObtainCell();

            Transform = false;
            Touchable = Touchable.ChildrenOnly;
        }
        
        public IDrawable BackgroundDrawable
        {
            get => GetBackground();
            set => SetBackground(value);
        }

        public override float MinWidth
        {
            get
            {
                if (_sizeInvalid)
                    ComputeSize();
                return _tableMinWidth;
            }
        }

        public override float MinHeight
        {
            get
            {
                if (_sizeInvalid)
                    ComputeSize();
                return _tableMinHeight;
            }
        }

        public override float PreferredWidth
        {
            get
            {
                if (_sizeInvalid)
                    ComputeSize();
                var width = _tablePrefWidth;
                if (Background != null)
                    return System.Math.Max(width, Background.MinWidth);
                return width;
            }
        }

        public override float PreferredHeight
        {
            get
            {
                if (_sizeInvalid)
                    ComputeSize();
                var height = _tablePrefHeight;
                if (Background != null)
                    return System.Math.Max(height, Background.MinHeight);
                return height;
            }
        }


        private Cell ObtainCell()
        {
            var cell = Pool<Cell>.Obtain();
            cell.SetLayout(this);
            return cell;
        }


        public override void Draw(Graphics.Graphics graphics, float parentAlpha)
        {
            Validate();
            if (Transform)
            {
                ApplyTransform(graphics, ComputeTransform());
                DrawBackground(graphics, parentAlpha, 0, 0);

                if (Clip)
                {
                    graphics.Batcher.FlushBatch();
                    float padLeft = _padLeft.Get(this), padBottom = _padBottom.Get(this);
                    if (ClipBegin(graphics.Batcher, padLeft, padBottom, GetWidth() - padLeft - _padRight.Get(this),
                        GetHeight() - padBottom - _padTop.Get(this)))
                    {
                        DrawChildren(graphics, parentAlpha);
                        ClipEnd(graphics.Batcher);
                    }
                }
                else
                {
                    DrawChildren(graphics, parentAlpha);
                }
                ResetTransform(graphics);
            }
            else
            {
                DrawBackground(graphics, parentAlpha, X, Y);
                base.Draw(graphics, parentAlpha);
            }
        }


	    /// <summary>
	    ///     Called to draw the background, before clipping is applied (if enabled). Default implementation draws the background
	    ///     drawable.
	    /// </summary>
	    /// <param name="graphics">Graphics.</param>
	    /// <param name="parentAlpha">Parent alpha.</param>
	    /// <param name="x">The x coordinate.</param>
	    /// <param name="y">The y coordinate.</param>
	    protected virtual void DrawBackground(Graphics.Graphics graphics, float parentAlpha, float x, float y)
        {
            if (Background == null)
                return;

            Background.Draw(graphics, x, y, Width, Height, new Color(Color, (int) (Color.A * parentAlpha)));
        }


        public IDrawable GetBackground()
        {
            return Background;
        }


        public override Element Hit(Vector2 point)
        {
            if (Clip)
            {
                if (Touchable == Touchable.Disabled)
                    return null;
                if (point.X < 0 || point.X >= Width || point.Y < 0 || point.Y >= Height)
                    return null;
            }

            return base.Hit(point);
        }


        public override void Invalidate()
        {
            _sizeInvalid = true;
            base.Invalidate();
        }


	    /// <summary>
	    ///     Adds a new cell to the table with the specified element.
	    /// </summary>
	    /// <param name="element">element.</param>
	    public Cell Add(Element element)
        {
            var cell = ObtainCell();
            cell.Element = element;

            // the row was ended for layout, not by the user, so revert it.
            if (_implicitEndRow)
            {
                _implicitEndRow = false;
                _rows--;
                _cells.Last().EndRow = false;
            }

            var cellCount = _cells.Count;
            if (cellCount > 0)
            {
                // Set cell column and row.
                var lastCell = _cells.Last();
                if (!lastCell.EndRow)
                {
                    cell.Column = lastCell.Column + lastCell.Colspan.Value;
                    cell.Row = lastCell.Row;
                }
                else
                {
                    cell.Column = 0;
                    cell.Row = lastCell.Row + 1;
                }

                // set the index of the cell above.
                if (cell.Row > 0)
                {
                    for (var i = cellCount - 1; i >= 0; i--)
                    {
                        var other = _cells[i];
                        for (int column = other.Column, nn = column + other.Colspan.Value; column < nn; column++)
                            if (column == cell.Column)
                            {
                                cell.CellAboveIndex = i;
                                goto outer;
                            }
                    }
                    outer:
                    {
                    }
                }
            }
            else
            {
                cell.Column = 0;
                cell.Row = 0;
            }
            _cells.Add(cell);

            cell.Set(_cellDefaults);
            if (cell.Column < _columnDefaults.Count)
            {
                var columnCell = _columnDefaults[cell.Column];
                if (columnCell != null)
                    cell.Merge(columnCell);
            }
            cell.Merge(_rowDefaults);

            if (element != null)
                AddElement(element);

            return cell;
        }


	    /// <summary>
	    ///     Adds a new cell with a label
	    /// </summary>
	    /// <param name="text">Text.</param>
	    public Cell Add(string text)
        {
            return Add(new Label(text));
        }


	    /// <summary>
	    ///     Adds a cell without an element
	    /// </summary>
	    public Cell Add()
        {
            return Add((Element) null);
        }


	    /// <summary>
	    ///     Adds a new cell to the table with the specified elements in a {@link Stack}.
	    /// </summary>
	    /// <param name="elements">Elements.</param>
	    public Cell Stack(params Element[] elements)
        {
            var stack = new Stack();

            foreach (var element in elements)
                stack.Add(element);

            return Add(stack);
        }


        public override bool RemoveElement(Element element)
        {
            if (!base.RemoveElement(element))
                return false;

            var cell = GetCell(element);
            if (cell != null)
                cell.Element = null;
            return true;
        }


	    /// <summary>
	    ///     Removes all elements and cells from the table
	    /// </summary>
	    public override void ClearChildren()
        {
            for (var i = _cells.Count - 1; i >= 0; i--)
            {
                var cell = _cells[i];
                var element = cell.Element;
                if (element != null)
                    element.Remove();

                Pool<Cell>.Free(cell);
            }

            _cells.Clear();
            _rows = 0;
            _columns = 0;

            if (_rowDefaults != null)
                Pool<Cell>.Free(_rowDefaults);

            _rowDefaults = null;
            _implicitEndRow = false;

            base.ClearChildren();
        }


	    /// <summary>
	    ///     Removes all elements and cells from the table (same as {@link #clear()}) and additionally resets all table
	    ///     properties and
	    ///     cell, column, and row defaults.
	    /// </summary>
	    public void Reset()
        {
            Clear();
            _padTop = BackgroundTop;
            _padLeft = BackgroundLeft;
            _padBottom = BackgroundBottom;
            _padRight = BackgroundRight;
            _align = AlignInternal.Center;
            _tableDebug = TableDebug.None;

            _cellDefaults.Reset();

            for (int i = 0, n = _columnDefaults.Count; i < n; i++)
            {
                var columnCell = _columnDefaults[i];
                if (columnCell != null)
                    Pool<Cell>.Free(columnCell);
            }
            _columnDefaults.Clear();
        }


	    /// <summary>
	    ///     Indicates that subsequent cells should be added to a new row and returns the cell values that will be used as the
	    ///     defaults
	    ///     for all cells in the new row.
	    /// </summary>
	    public Cell Row()
        {
            if (_cells.Count > 0)
            {
                EndRow();
                Invalidate();
            }

            if (_rowDefaults != null)
                Pool<Cell>.Free(_rowDefaults);

            _rowDefaults = Pool<Cell>.Obtain();
            _rowDefaults.Clear();
            return _rowDefaults;
        }


        private void EndRow()
        {
            var rowColumns = 0;
            for (var i = _cells.Count - 1; i >= 0; i--)
            {
                var cell = _cells[i];
                if (cell.EndRow)
                    break;
                rowColumns += cell.Colspan.Value;
            }

            _columns = System.Math.Max(_columns, rowColumns);
            _rows++;
            _cells.Last().EndRow = true;
        }


	    /// <summary>
	    ///     Gets the cell values that will be used as the defaults for all cells in the specified column. Columns are indexed
	    ///     starting at 0
	    /// </summary>
	    /// <returns>The column defaults.</returns>
	    /// <param name="column">Column.</param>
	    public Cell GetColumnDefaults(int column)
        {
            var cell = _columnDefaults.Count > column ? _columnDefaults[column] : null;
            if (cell == null)
            {
                cell = ObtainCell();
                cell.Clear();
                if (column >= _columnDefaults.Count)
                {
                    for (var i = _columnDefaults.Count; i < column; i++)
                        _columnDefaults.Add(null);
                    _columnDefaults.Add(cell);
                }
                else
                {
                    _columnDefaults[column] = cell;
                }
            }
            return cell;
        }


        private float[] EnsureSize(float[] array, int size)
        {
            if (array == null || array.Length < size)
                return new float[size];

            for (int i = 0, n = array.Length; i < n; i++)
                array[i] = 0;

            return array;
        }


        public override void Layout()
        {
            Layout(0, 0, Width, Height);

            if (_round)
                for (int i = 0, n = _cells.Count; i < n; i++)
                {
                    var c = _cells[i];
                    var elementWidth = Mathf.Round(c.ElementWidth);
                    var elementHeight = Mathf.Round(c.ElementHeight);
                    var elementX = Mathf.Round(c.ElementX);
                    var elementY = Mathf.Round(c.ElementY);
                    c.SetElementBounds(elementX, elementY, elementWidth, elementHeight);

                    if (c.Element != null)
                        c.Element.SetBounds(elementX, elementY, elementWidth, elementHeight);
                }
            else
                for (int i = 0, n = _cells.Count; i < n; i++)
                {
                    var c = _cells[i];
                    var elementY = c.ElementY;
                    c.SetElementY(elementY);

                    if (c.Element != null)
                        c.Element.SetBounds(c.ElementX, elementY, c.ElementWidth, c.ElementHeight);
                }

            // Validate children separately from sizing elements to ensure elements without a cell are validated.
            for (int i = 0, n = Children.Count; i < n; i++)
            {
                var child = Children[i];
                if (child is ILayout)
                    ((ILayout) child).Validate();
            }
        }


	    /// <summary>
	    ///     Positions and sizes children of the table using the cell associated with each child. The values given are the
	    ///     position
	    ///     within the parent and size of the table.
	    /// </summary>
	    /// <param name="layoutX">Layout x.</param>
	    /// <param name="layoutY">Layout y.</param>
	    /// <param name="layoutWidth">Layout width.</param>
	    /// <param name="layoutHeight">Layout height.</param>
	    private void Layout(float layoutX, float layoutY, float layoutWidth, float layoutHeight)
        {
            if (_sizeInvalid)
                ComputeSize();

            var cellCount = _cells.Count;
            var padLeft = _padLeft.Get(this);
            var hpadding = padLeft + _padRight.Get(this);
            var padTop = _padTop.Get(this);
            var vpadding = padTop + _padBottom.Get(this);

            int columns = _columns, rows = _rows;
            float[] expandWidth = _expandWidth, expandHeight = _expandHeight;
            float[] columnWidth = _columnWidth, rowHeight = _rowHeight;

            float totalExpandWidth = 0, totalExpandHeight = 0;
            for (var i = 0; i < columns; i++)
                totalExpandWidth += expandWidth[i];
            for (var i = 0; i < rows; i++)
                totalExpandHeight += expandHeight[i];

            // Size columns and rows between min and pref size using (preferred - min) size to weight distribution of extra space.
            float[] columnWeightedWidth;
            var totalGrowWidth = _tablePrefWidth - _tableMinWidth;
            if (totalGrowWidth == 0)
            {
                columnWeightedWidth = _columnMinWidth;
            }
            else
            {
                var extraWidth = System.Math.Min(totalGrowWidth, System.Math.Max(0, layoutWidth - _tableMinWidth));
                columnWeightedWidth = _columnWeightedWidth = EnsureSize(_columnWeightedWidth, columns);
                float[] columnMinWidth = _columnMinWidth, columnPrefWidth = _columnPrefWidth;
                for (var i = 0; i < columns; i++)
                {
                    var growWidth = columnPrefWidth[i] - columnMinWidth[i];
                    var growRatio = growWidth / totalGrowWidth;
                    columnWeightedWidth[i] = columnMinWidth[i] + extraWidth * growRatio;
                }
            }

            float[] rowWeightedHeight;
            var totalGrowHeight = _tablePrefHeight - _tableMinHeight;
            if (totalGrowHeight == 0)
            {
                rowWeightedHeight = _rowMinHeight;
            }
            else
            {
                rowWeightedHeight = _rowWeightedHeight = EnsureSize(_rowWeightedHeight, rows);
                var extraHeight = System.Math.Min(totalGrowHeight, System.Math.Max(0, layoutHeight - _tableMinHeight));
                float[] rowMinHeight = _rowMinHeight, rowPrefHeight = _rowPrefHeight;
                for (var i = 0; i < rows; i++)
                {
                    var growHeight = rowPrefHeight[i] - rowMinHeight[i];
                    var growRatio = growHeight / totalGrowHeight;
                    rowWeightedHeight[i] = rowMinHeight[i] + extraHeight * growRatio;
                }
            }

            // Determine element and cell sizes (before expand or fill).
            for (var i = 0; i < cellCount; i++)
            {
                var cell = _cells[i];
                int column = cell.Column, row = cell.Row;

                var spannedWeightedWidth = 0f;
                var colspan = cell.Colspan.Value;
                for (int ii = column, nn = ii + colspan; ii < nn; ii++)
                    spannedWeightedWidth += columnWeightedWidth[ii];
                var weightedHeight = rowWeightedHeight[row];

                var prefWidth = cell.PrefWidth.Get(cell.Element);
                var prefHeight = cell.PrefHeight.Get(cell.Element);
                var minWidth = cell.MinWidth.Get(cell.Element);
                var minHeight = cell.MinHeight.Get(cell.Element);
                var maxWidth = cell.MaxWidth.Get(cell.Element);
                var maxHeight = cell.MaxHeight.Get(cell.Element);

                if (prefWidth < minWidth)
                    prefWidth = minWidth;
                if (prefHeight < minHeight)
                    prefHeight = minHeight;
                if (maxWidth > 0 && prefWidth > maxWidth)
                    prefWidth = maxWidth;
                if (maxHeight > 0 && prefHeight > maxHeight)
                    prefHeight = maxHeight;

                cell.ElementWidth = System.Math.Min(spannedWeightedWidth - cell.ComputedPadLeft - cell.ComputedPadRight,
                    prefWidth);
                cell.ElementHeight =
                    System.Math.Min(weightedHeight - cell.ComputedPadTop - cell.ComputedPadBottom, prefHeight);

                if (colspan == 1)
                    columnWidth[column] = System.Math.Max(columnWidth[column], spannedWeightedWidth);
                rowHeight[row] = System.Math.Max(rowHeight[row], weightedHeight);
            }

            // distribute remaining space to any expanding columns/rows.
            if (totalExpandWidth > 0)
            {
                var extra = layoutWidth - hpadding;
                for (var i = 0; i < columns; i++)
                    extra -= columnWidth[i];

                var used = 0f;
                var lastIndex = 0;
                for (var i = 0; i < columns; i++)
                {
                    if (expandWidth[i] == 0)
                        continue;
                    var amount = extra * expandWidth[i] / totalExpandWidth;
                    columnWidth[i] += amount;
                    used += amount;
                    lastIndex = i;
                }
                columnWidth[lastIndex] += extra - used;
            }

            if (totalExpandHeight > 0)
            {
                var extra = layoutHeight - vpadding;
                for (var i = 0; i < rows; i++)
                    extra -= rowHeight[i];

                var used = 0f;
                var lastIndex = 0;
                for (var i = 0; i < rows; i++)
                {
                    if (expandHeight[i] == 0)
                        continue;

                    var amount = extra * expandHeight[i] / totalExpandHeight;
                    rowHeight[i] += amount;
                    used += amount;
                    lastIndex = i;
                }
                rowHeight[lastIndex] += extra - used;
            }

            // distribute any additional width added by colspanned cells to the columns spanned.
            for (var i = 0; i < cellCount; i++)
            {
                var c = _cells[i];
                var colspan = c.Colspan.Value;
                if (colspan == 1)
                    continue;

                var extraWidth = 0f;
                for (int column = c.Column, nn = column + colspan; column < nn; column++)
                    extraWidth += columnWeightedWidth[column] - columnWidth[column];
                extraWidth -= System.Math.Max(0, c.ComputedPadLeft + c.ComputedPadRight);

                extraWidth /= colspan;
                if (extraWidth > 0)
                    for (int column = c.Column, nn = column + colspan; column < nn; column++)
                        columnWidth[column] += extraWidth;
            }

            // Determine table size.
            float tableWidth = hpadding, tableHeight = vpadding;
            for (var i = 0; i < columns; i++)
                tableWidth += columnWidth[i];
            for (var i = 0; i < rows; i++)
                tableHeight += rowHeight[i];

            // Position table within the container.
            var x = layoutX + padLeft;
            if ((_align & AlignInternal.Right) != 0)
                x += layoutWidth - tableWidth;
            else if ((_align & AlignInternal.Left) == 0) // Center
                x += (layoutWidth - tableWidth) / 2;

            var y = layoutY + padTop; // bottom
            if ((_align & AlignInternal.Bottom) != 0)
                y += layoutHeight - tableHeight;
            else if ((_align & AlignInternal.Top) == 0) // Center
                y += (layoutHeight - tableHeight) / 2;

            // position elements within cells.
            float currentX = x, currentY = y;
            for (var i = 0; i < cellCount; i++)
            {
                var c = _cells[i];

                var spannedCellWidth = 0f;
                for (int column = c.Column, nn = column + c.Colspan.Value; column < nn; column++)
                    spannedCellWidth += columnWidth[column];
                spannedCellWidth -= c.ComputedPadLeft + c.ComputedPadRight;

                currentX += c.ComputedPadLeft;

                float fillX = c.FillX.Value, fillY = c.FillY.Value;
                if (fillX > 0)
                {
                    c.ElementWidth = System.Math.Max(spannedCellWidth * fillX, c.MinWidth.Get(c.Element));
                    var maxWidth = c.MaxWidth.Get(c.Element);
                    if (maxWidth > 0)
                        c.ElementWidth = System.Math.Min(c.ElementWidth, maxWidth);
                }
                if (fillY > 0)
                {
                    c.ElementHeight = System.Math.Max(rowHeight[c.Row] * fillY - c.ComputedPadTop - c.ComputedPadBottom,
                        c.MinHeight.Get(c.Element));
                    var maxHeight = c.MaxHeight.Get(c.Element);
                    if (maxHeight > 0)
                        c.ElementHeight = System.Math.Min(c.ElementHeight, maxHeight);
                }

                var cellAlign = c.Align.Value;
                if ((cellAlign & AlignInternal.Left) != 0)
                    c.ElementX = currentX;
                else if ((cellAlign & AlignInternal.Right) != 0)
                    c.ElementX = currentX + spannedCellWidth - c.ElementWidth;
                else
                    c.ElementX = currentX + (spannedCellWidth - c.ElementWidth) / 2;

                if ((cellAlign & AlignInternal.Top) != 0)
                    c.ElementY = currentY + c.ComputedPadTop;
                else if ((cellAlign & AlignInternal.Bottom) != 0)
                    c.ElementY = currentY + rowHeight[c.Row] - c.ElementHeight - c.ComputedPadBottom;
                else
                    c.ElementY =
                        currentY + (rowHeight[c.Row] - c.ElementHeight + c.ComputedPadTop - c.ComputedPadBottom) / 2;

                if (c.EndRow)
                {
                    currentX = x;
                    currentY += rowHeight[c.Row];
                }
                else
                {
                    currentX += spannedCellWidth + c.ComputedPadRight;
                }
            }

            if (_tableDebug != TableDebug.None)
                ComputeDebugRects(x, y, layoutX, layoutY, layoutWidth, layoutHeight, tableWidth, tableHeight, hpadding,
                    vpadding);
        }


        private void ComputeSize()
        {
            _sizeInvalid = false;

            var cellCount = _cells.Count;

            // Implicitly End the row for layout purposes.
            if (cellCount > 0 && !_cells.Last().EndRow)
            {
                EndRow();
                _implicitEndRow = true;
            }
            else
            {
                _implicitEndRow = false;
            }

            int columns = _columns, rows = _rows;
            _columnWidth = EnsureSize(_columnWidth, columns);
            _rowHeight = EnsureSize(_rowHeight, rows);
            _columnMinWidth = EnsureSize(_columnMinWidth, columns);
            _rowMinHeight = EnsureSize(_rowMinHeight, rows);
            _columnPrefWidth = EnsureSize(_columnPrefWidth, columns);
            _rowPrefHeight = EnsureSize(_rowPrefHeight, rows);
            _expandWidth = EnsureSize(_expandWidth, columns);
            _expandHeight = EnsureSize(_expandHeight, rows);

            var spaceRightLast = 0f;
            for (var i = 0; i < cellCount; i++)
            {
                var cell = _cells[i];
                int column = cell.Column, row = cell.Row, colspan = cell.Colspan.Value;

                // Collect rows that expand and colspan=1 columns that expand.
                if (cell.ExpandY != 0 && _expandHeight[row] == 0)
                    _expandHeight[row] = cell.ExpandY.Value;
                if (colspan == 1 && cell.ExpandX != 0 && _expandWidth[column] == 0)
                    _expandWidth[column] = cell.ExpandX.Value;

                // Compute combined padding/spacing for cells.
                // Spacing between elements isn't additive, the larger is used. Also, no spacing around edges.
                cell.ComputedPadLeft = cell.PadLeft.Get(cell.Element) +
                                       (column == 0
                                           ? 0
                                           : System.Math.Max(0, cell.SpaceLeft.Get(cell.Element) - spaceRightLast));
                cell.ComputedPadTop = cell.PadTop.Get(cell.Element);
                if (cell.CellAboveIndex != -1)
                {
                    var above = _cells[cell.CellAboveIndex];
                    cell.ComputedPadTop += System.Math.Max(0,
                        cell.SpaceTop.Get(cell.Element) - above.SpaceBottom.Get(cell.Element));
                }

                var spaceRight = cell.SpaceRight.Get(cell.Element);
                cell.ComputedPadRight =
                    cell.PadRight.Get(cell.Element) + (column + colspan == columns ? 0 : spaceRight);
                cell.ComputedPadBottom = cell.PadBottom.Get(cell.Element) +
                                         (row == rows - 1 ? 0 : cell.SpaceBottom.Get(cell.Element));
                spaceRightLast = spaceRight;

                // Determine minimum and preferred cell sizes.
                var prefWidth = cell.PrefWidth.Get(cell.Element);
                var prefHeight = cell.PrefHeight.Get(cell.Element);
                var minWidth = cell.MinWidth.Get(cell.Element);
                var minHeight = cell.MinHeight.Get(cell.Element);
                var maxWidth = cell.MaxWidth.Get(cell.Element);
                var maxHeight = cell.MaxHeight.Get(cell.Element);

                if (prefWidth < minWidth)
                    prefWidth = minWidth;
                if (prefHeight < minHeight)
                    prefHeight = minHeight;
                if (maxWidth > 0 && prefWidth > maxWidth)
                    prefWidth = maxWidth;
                if (maxHeight > 0 && prefHeight > maxHeight)
                    prefHeight = maxHeight;

                if (colspan == 1)
                {
                    // Spanned column min and pref width is added later.
                    var Hpadding = cell.ComputedPadLeft + cell.ComputedPadRight;
                    _columnPrefWidth[column] = System.Math.Max(_columnPrefWidth[column], prefWidth + Hpadding);
                    _columnMinWidth[column] = System.Math.Max(_columnMinWidth[column], minWidth + Hpadding);
                }
                var Vpadding = cell.ComputedPadTop + cell.ComputedPadBottom;
                _rowPrefHeight[row] = System.Math.Max(_rowPrefHeight[row], prefHeight + Vpadding);
                _rowMinHeight[row] = System.Math.Max(_rowMinHeight[row], minHeight + Vpadding);
            }

            float uniformMinWidth = 0, uniformMinHeight = 0;
            float uniformPrefWidth = 0, uniformPrefHeight = 0;
            for (var i = 0; i < cellCount; i++)
            {
                var c = _cells[i];

                // Colspan with expand will expand all spanned columns if none of the spanned columns have expand.
                var expandX = c.ExpandX.Value;
                if (expandX != 0)
                {
                    var nn = c.Column + c.Colspan.Value;
                    for (var ii = c.Column; ii < nn; ii++)
                        if (_expandWidth[ii] != 0)
                            goto outer;
                    for (var ii = c.Column; ii < nn; ii++)
                        _expandWidth[ii] = expandX;
                }
                outer:
                {
                }

                // Collect uniform sizes.
                if (c.UniformX.HasValue && c.UniformX.Value && c.Colspan == 1)
                {
                    var Hpadding = c.ComputedPadLeft + c.ComputedPadRight;
                    uniformMinWidth = System.Math.Max(uniformMinWidth, _columnMinWidth[c.Column] - Hpadding);
                    uniformPrefWidth = System.Math.Max(uniformPrefWidth, _columnPrefWidth[c.Column] - Hpadding);
                }

                if (c.UniformY.HasValue && c.UniformY.Value)
                {
                    var Vpadding = c.ComputedPadTop + c.ComputedPadBottom;
                    uniformMinHeight = System.Math.Max(uniformMinHeight, _rowMinHeight[c.Row] - Vpadding);
                    uniformPrefHeight = System.Math.Max(uniformPrefHeight, _rowPrefHeight[c.Row] - Vpadding);
                }
            }

            // Size uniform cells to the same width/height.
            if (uniformPrefWidth > 0 || uniformPrefHeight > 0)
                for (var i = 0; i < cellCount; i++)
                {
                    var c = _cells[i];
                    if (uniformPrefWidth > 0 && c.UniformX.HasValue && c.UniformX.Value && c.Colspan == 1)
                    {
                        var Hpadding = c.ComputedPadLeft + c.ComputedPadRight;
                        _columnMinWidth[c.Column] = uniformMinWidth + Hpadding;
                        _columnPrefWidth[c.Column] = uniformPrefWidth + Hpadding;
                    }

                    if (uniformPrefHeight > 0 && c.UniformY.HasValue && c.UniformY.Value)
                    {
                        var Vpadding = c.ComputedPadTop + c.ComputedPadBottom;
                        _rowMinHeight[c.Row] = uniformMinHeight + Vpadding;
                        _rowPrefHeight[c.Row] = uniformPrefHeight + Vpadding;
                    }
                }

            // Distribute any additional min and pref width added by colspanned cells to the columns spanned.
            for (var i = 0; i < cellCount; i++)
            {
                var c = _cells[i];
                var colspan = c.Colspan.Value;
                if (colspan == 1)
                    continue;

                var a = c.Element;
                var minWidth = c.MinWidth.Get(a);
                var prefWidth = c.PrefWidth.Get(a);
                var maxWidth = c.MaxWidth.Get(a);
                if (prefWidth < minWidth)
                    prefWidth = minWidth;
                if (maxWidth > 0 && prefWidth > maxWidth)
                    prefWidth = maxWidth;

                float spannedMinWidth = -(c.ComputedPadLeft + c.ComputedPadRight), spannedPrefWidth = spannedMinWidth;
                var totalExpandWidth = 0f;
                for (int ii = c.Column, nn = ii + colspan; ii < nn; ii++)
                {
                    spannedMinWidth += _columnMinWidth[ii];
                    spannedPrefWidth += _columnPrefWidth[ii];
                    totalExpandWidth +=
                        _expandWidth[ii]; // Distribute extra space using expand, if any columns have expand.
                }

                var extraMinWidth = System.Math.Max(0, minWidth - spannedMinWidth);
                var extraPrefWidth = System.Math.Max(0, prefWidth - spannedPrefWidth);
                for (int ii = c.Column, nn = ii + colspan; ii < nn; ii++)
                {
                    var ratio = totalExpandWidth == 0 ? 1f / colspan : _expandWidth[ii] / totalExpandWidth;
                    _columnMinWidth[ii] += extraMinWidth * ratio;
                    _columnPrefWidth[ii] += extraPrefWidth * ratio;
                }
            }

            // Determine table min and pref size.
            _tableMinWidth = 0;
            _tableMinHeight = 0;
            _tablePrefWidth = 0;
            _tablePrefHeight = 0;
            for (var i = 0; i < columns; i++)
            {
                _tableMinWidth += _columnMinWidth[i];
                _tablePrefWidth += _columnPrefWidth[i];
            }

            for (var i = 0; i < rows; i++)
            {
                _tableMinHeight += _rowMinHeight[i];
                _tablePrefHeight += System.Math.Max(_rowMinHeight[i], _rowPrefHeight[i]);
            }

            var hpadding = _padLeft.Get(this) + _padRight.Get(this);
            var vpadding = _padTop.Get(this) + _padBottom.Get(this);
            _tableMinWidth = _tableMinWidth + hpadding;
            _tableMinHeight = _tableMinHeight + vpadding;
            _tablePrefWidth = System.Math.Max(_tablePrefWidth + hpadding, _tableMinWidth);
            _tablePrefHeight = System.Math.Max(_tablePrefHeight + vpadding, _tableMinHeight);
        }


        #region Chainable Configuration

	    /// <summary>
	    ///     The cell values that will be used as the defaults for all cells.
	    /// </summary>
	    public Cell Defaults()
        {
            return _cellDefaults;
        }


        public Table SetFillParent(bool fillParent)
        {
            this.FillParent = fillParent;
            return this;
        }


	    /// <summary>
	    ///     background may be null to clear the background.
	    /// </summary>
	    /// <param name="background">Background.</param>
	    public Table SetBackground(IDrawable background)
        {
            if (Background == background)
                return this;

            float padTopOld = GetPadTop(),
                padLeftOld = GetPadLeft(),
                padBottomOld = GetPadBottom(),
                padRightOld = GetPadRight();
            Background = background;
            float padTopNew = GetPadTop(),
                padLeftNew = GetPadLeft(),
                padBottomNew = GetPadBottom(),
                padRightNew = GetPadRight();
            if (padTopOld + padBottomOld != padTopNew + padBottomNew ||
                padLeftOld + padRightOld != padLeftNew + padRightNew)
                InvalidateHierarchy();
            else if (padTopOld != padTopNew || padLeftOld != padLeftNew || padBottomOld != padBottomNew ||
                     padRightOld != padRightNew)
                Invalidate();

            return this;
        }


	    /// <summary>
	    ///     If true (the default), positions and sizes are rounded to integers.
	    /// </summary>
	    /// <param name="round">If set to <c>true</c> round.</param>
	    public Table Round(bool round)
        {
            _round = round;
            return this;
        }


	    /// <summary>
	    ///     Sets the padTop, padLeft, padBottom, and padRight around the table to the specified value.
	    /// </summary>
	    /// <param name="pad">Pad.</param>
	    public Table Pad(Value pad)
        {
            if (pad == null)
                throw new Exception("pad cannot be null.");

            _padTop = pad;
            _padLeft = pad;
            _padBottom = pad;
            _padRight = pad;
            _sizeInvalid = true;

            return this;
        }


        public Table Pad(Value top, Value left, Value bottom, Value right)
        {
            if (top == null)
                throw new Exception("top cannot be null.");
            if (left == null)
                throw new Exception("left cannot be null.");
            if (bottom == null)
                throw new Exception("bottom cannot be null.");
            if (right == null)
                throw new Exception("right cannot be null.");

            _padTop = top;
            _padLeft = left;
            _padBottom = bottom;
            _padRight = right;
            _sizeInvalid = true;

            return this;
        }


	    /// <summary>
	    ///     Padding at the top edge of the table.
	    /// </summary>
	    /// <param name="padTop">Pad top.</param>
	    public Table PadTop(Value padTop)
        {
            if (padTop == null)
                throw new Exception("padTop cannot be null.");
            _padTop = padTop;
            _sizeInvalid = true;
            return this;
        }


	    /// <summary>
	    ///     Padding at the left edge of the table.
	    /// </summary>
	    /// <param name="padLeft">Pad left.</param>
	    public Table PadLeft(Value padLeft)
        {
            if (padLeft == null)
                throw new Exception("padLeft cannot be null.");
            _padLeft = padLeft;
            _sizeInvalid = true;

            return this;
        }


	    /// <summary>
	    ///     Padding at the bottom edge of the table.
	    /// </summary>
	    /// <param name="padBottom">Pad bottom.</param>
	    public Table PadBottom(Value padBottom)
        {
            if (padBottom == null)
                throw new Exception("padBottom cannot be null.");
            _padBottom = padBottom;
            _sizeInvalid = true;
            return this;
        }


	    /// <summary>
	    ///     Padding at the right edge of the table.
	    /// </summary>
	    /// <param name="padRight">Pad right.</param>
	    public Table PadRight(Value padRight)
        {
            if (padRight == null)
                throw new Exception("padRight cannot be null.");
            _padRight = padRight;
            _sizeInvalid = true;
            return this;
        }


	    /// <summary>
	    ///     Sets the padTop, padLeft, padBottom, and padRight around the table to the specified value.
	    /// </summary>
	    /// <param name="pad">Pad.</param>
	    public Table Pad(float pad)
        {
            this.Pad(new Value.Fixed(pad));
            return this;
        }


        public Table Pad(float top, float left, float bottom, float right)
        {
            _padTop = new Value.Fixed(top);
            _padLeft = new Value.Fixed(left);
            _padBottom = new Value.Fixed(bottom);
            _padRight = new Value.Fixed(right);
            _sizeInvalid = true;
            return this;
        }


	    /// <summary>
	    ///     Padding at the top edge of the table.
	    /// </summary>
	    /// <param name="padTop">Pad top.</param>
	    public Table PadTop(float padTop)
        {
            _padTop = new Value.Fixed(padTop);
            _sizeInvalid = true;
            return this;
        }


	    /// <summary>
	    ///     Padding at the left edge of the table.
	    /// </summary>
	    /// <param name="padLeft">Pad left.</param>
	    public Table PadLeft(float padLeft)
        {
            _padLeft = new Value.Fixed(padLeft);
            _sizeInvalid = true;
            return this;
        }


	    /// <summary>
	    ///     Padding at the bottom edge of the table.
	    /// </summary>
	    /// <param name="padBottom">Pad bottom.</param>
	    public Table PadBottom(float padBottom)
        {
            _padBottom = new Value.Fixed(padBottom);
            _sizeInvalid = true;
            return this;
        }


	    /// <summary>
	    ///     Padding at the right edge of the table.
	    /// </summary>
	    /// <param name="padRight">Pad right.</param>
	    public Table PadRight(float padRight)
        {
            _padRight = new Value.Fixed(padRight);
            _sizeInvalid = true;
            return this;
        }


	    /// <summary>
	    ///     Alignment of the logical table within the table element. Set to {@link Align#center}, {@link Align#top}, {@link
	    ///     Align#bottom}
	    ///     {@link Align#left}, {@link Align#right}, or any combination of those.
	    /// </summary>
	    /// <param name="align">Align.</param>
	    public Table Align(int align)
        {
            _align = align;
            return this;
        }


	    /// <summary>
	    ///     Sets the alignment of the logical table within the table element to {@link Align#center}. This clears any other
	    ///     alignment.
	    /// </summary>
	    public Table Center()
        {
            _align = AlignInternal.Center;
            return this;
        }


	    /// <summary>
	    ///     Adds {@link Align#top} and clears {@link Align#bottom} for the alignment of the logical table within the table
	    ///     element.
	    /// </summary>
	    public Table Top()
        {
            _align |= AlignInternal.Top;
            _align &= ~AlignInternal.Bottom;
            return this;
        }


	    /// <summary>
	    ///     Adds {@link Align#left} and clears {@link Align#right} for the alignment of the logical table within the table
	    ///     element.
	    /// </summary>
	    public Table Left()
        {
            _align |= AlignInternal.Left;
            _align &= ~AlignInternal.Right;
            return this;
        }


	    /// <summary>
	    ///     Adds {@link Align#bottom} and clears {@link Align#top} for the alignment of the logical table within the table
	    ///     element.
	    /// </summary>
	    public Table Bottom()
        {
            _align |= AlignInternal.Bottom;
            _align &= ~AlignInternal.Top;
            return this;
        }


	    /// <summary>
	    ///     Adds {@link Align#right} and clears {@link Align#left} for the alignment of the logical table within the table
	    ///     element.
	    /// </summary>
	    public Table Right()
        {
            _align |= AlignInternal.Right;
            _align &= ~AlignInternal.Left;
            return this;
        }


	    /// <summary>
	    ///     enables/disables all debug lines (table, cell, and widget)
	    /// </summary>
	    /// <param name="enabled">If set to <c>true</c> enabled.</param>
	    public override void SetDebug(bool enabled)
        {
            DebugTable(enabled ? TableDebug.All : TableDebug.None);
            Debug = enabled;
        }


	    /// <summary>
	    ///     Turn on all debug lines (table, cell, and element)
	    /// </summary>
	    /// <returns>The all.</returns>
	    public new Table DebugAll()
        {
            SetDebug(true);
            base.DebugAll();
            return this;
        }


	    /// <summary>
	    ///     Turns on table debug lines.
	    /// </summary>
	    /// <returns>The table.</returns>
	    public Table DebugTable()
        {
            base.SetDebug(true);
            if (_tableDebug != TableDebug.Table)
            {
                _tableDebug = TableDebug.Table;
                Invalidate();
            }
            return this;
        }


	    /// <summary>
	    ///     Turns on cell debug lines.
	    /// </summary>
	    public Table DebugCell()
        {
            base.SetDebug(true);
            if (_tableDebug != TableDebug.Cell)
            {
                _tableDebug = TableDebug.Cell;
                Invalidate();
            }
            return this;
        }


	    /// <summary>
	    ///     Turns on element debug lines.
	    /// </summary>
	    public Table DebugElement()
        {
            base.SetDebug(true);
            if (_tableDebug != TableDebug.Element)
            {
                _tableDebug = TableDebug.Element;
                Invalidate();
            }
            return this;
        }


	    /// <summary>
	    ///     Turns debug lines on or off.
	    /// </summary>
	    /// <param name="tableDebug">Table debug.</param>
	    public Table DebugTable(TableDebug tableDebug)
        {
            base.SetDebug(tableDebug != TableDebug.None);
            if (_tableDebug != tableDebug)
            {
                _tableDebug = tableDebug;
                if (_tableDebug == TableDebug.None)
                {
                    if (_debugRects != null)
                        _debugRects.Clear();
                }
                else
                {
                    Invalidate();
                }
            }
            return this;
        }

        #endregion


        #region Getters

	    /// <summary>
	    ///     gets the current Cell defaults. This is the same value returned by a call to row()
	    /// </summary>
	    /// <returns>The row defaults.</returns>
	    public Cell GetRowDefaults()
        {
            if (_rowDefaults == null)
            {
                _rowDefaults = Pool<Cell>.Obtain();
                _rowDefaults.Clear();
            }

            return _rowDefaults;
        }


	    /// <summary>
	    ///     Returns the cell for the specified element in this table, or null.
	    /// </summary>
	    /// <returns>The cell.</returns>
	    /// <param name="element">element.</param>
	    public Cell GetCell(Element element)
        {
            for (int i = 0, n = _cells.Count; i < n; i++)
            {
                var c = _cells[i];
                if (c.Element == element)
                    return c;
            }
            return null;
        }


	    /// <summary>
	    ///     returns all the Cells in the table
	    /// </summary>
	    /// <returns>The cells.</returns>
	    public List<Cell> GetCells()
        {
            return _cells;
        }


        public Value GetPadTopValue()
        {
            return _padTop;
        }


        public float GetPadTop()
        {
            return _padTop.Get(this);
        }


        public Value GetPadLeftValue()
        {
            return _padLeft;
        }


        public float GetPadLeft()
        {
            return _padLeft.Get(this);
        }


        public Value GetPadBottomValue()
        {
            return _padBottom;
        }


        public float GetPadBottom()
        {
            return _padBottom.Get(this);
        }


        public Value GetPadRightValue()
        {
            return _padRight;
        }


        public float GetPadRight()
        {
            return _padRight.Get(this);
        }


	    /// <summary>
	    ///     Returns {@link #getPadLeft()} plus {@link #getPadRight()}.
	    /// </summary>
	    /// <returns>The pad x.</returns>
	    public float GetPadX()
        {
            return _padLeft.Get(this) + _padRight.Get(this);
        }


	    /// <summary>
	    ///     Returns {@link #getPadTop()} plus {@link #getPadBottom()}.
	    /// </summary>
	    /// <returns>The pad y.</returns>
	    public float GetPadY()
        {
            return _padTop.Get(this) + _padBottom.Get(this);
        }


        public int GetAlign()
        {
            return _align;
        }


        public int GetRows()
        {
            return _rows;
        }


        public int GetColumns()
        {
            return _columns;
        }


	    /// <summary>
	    ///     Returns the height of the specified row.
	    /// </summary>
	    /// <returns>The row height.</returns>
	    /// <param name="rowIndex">Row index.</param>
	    public float GetRowHeight(int rowIndex)
        {
            return _rowHeight[rowIndex];
        }


	    /// <summary>
	    ///     Returns the width of the specified column.
	    /// </summary>
	    /// <returns>The column width.</returns>
	    /// <param name="columnIndex">Column index.</param>
	    public float GetColumnWidth(int columnIndex)
        {
            return _columnWidth[columnIndex];
        }

        #endregion


        #region Debug

        public override void DebugRender(Graphics.Graphics graphics)
        {
            if (_debugRects != null)
                foreach (var d in _debugRects)
                    graphics.Batcher.DrawHollowRect(X + d.Rect.X, Y + d.Rect.Y, d.Rect.Width, d.Rect.Height, d.Color);

            base.DebugRender(graphics);
        }


        private void ComputeDebugRects(float x, float y, float layoutX, float layoutY, float layoutWidth,
            float layoutHeight, float tableWidth, float tableHeight, float hpadding, float vpadding)
        {
            if (_debugRects != null)
                _debugRects.Clear();

            var currentX = x;
            var currentY = y;
            if (_tableDebug == TableDebug.Table || _tableDebug == TableDebug.All)
            {
                AddDebugRect(layoutX, layoutY, layoutWidth, layoutHeight, DebugTableColor);
                AddDebugRect(x, y, tableWidth - hpadding, tableHeight - vpadding, DebugTableColor);
            }

            for (var i = 0; i < _cells.Count; i++)
            {
                var cell = _cells[i];

                // element bounds.
                if (_tableDebug == TableDebug.Element || _tableDebug == TableDebug.All)
                    AddDebugRect(cell.ElementX, cell.ElementY, cell.ElementWidth, cell.ElementHeight,
                        DebugElementColor);

                // Cell bounds.
                float spannedCellWidth = 0;
                for (int column = cell.Column, nn = column + cell.Colspan.Value; column < nn; column++)
                    spannedCellWidth += _columnWidth[column];
                spannedCellWidth -= cell.ComputedPadLeft + cell.ComputedPadRight;
                currentX += cell.ComputedPadLeft;

                if (_tableDebug == TableDebug.Cell || _tableDebug == TableDebug.All)
                    AddDebugRect(currentX, currentY + cell.ComputedPadTop, spannedCellWidth,
                        _rowHeight[cell.Row] - cell.ComputedPadTop - cell.ComputedPadBottom, DebugCellColor);

                if (cell.EndRow)
                {
                    currentX = x;
                    currentY += _rowHeight[cell.Row];
                }
                else
                {
                    currentX += spannedCellWidth + cell.ComputedPadRight;
                }
            }
        }


        private void AddDebugRect(float x, float y, float w, float h, Color color)
        {
            if (_debugRects == null)
                _debugRects = new List<DebugRectangleF>();

            var rect = new DebugRectangleF(x, y, w, h, color);
            _debugRects.Add(rect);
        }

        #endregion


        #region Value types

        public static Value BackgroundTop = new BackgroundTopValue();

	    /// <summary>
	    ///     Value that is the top padding of the table's background
	    /// </summary>
	    public class BackgroundTopValue : Value
        {
            public override float Get(Element context)
            {
                var background = ((Table) context).Background;
                return background == null ? 0 : background.TopHeight;
            }
        }


        public static Value BackgroundLeft = new BackgroundLeftValue();

	    /// <summary>
	    ///     Value that is the left padding of the table's background
	    /// </summary>
	    public class BackgroundLeftValue : Value
        {
            public override float Get(Element context)
            {
                var background = ((Table) context).Background;
                return background == null ? 0 : background.LeftWidth;
            }
        }


        public static Value BackgroundBottom = new BackgroundBottomValue();

	    /// <summary>
	    ///     Value that is the bottom padding of the table's background
	    /// </summary>
	    public class BackgroundBottomValue : Value
        {
            public override float Get(Element context)
            {
                var background = ((Table) context).Background;
                return background == null ? 0 : background.BottomHeight;
            }
        }


        public static Value BackgroundRight = new BackgroundRightValue();

	    /// <summary>
	    ///     Value that is the right padding of the table's background
	    /// </summary>
	    public class BackgroundRightValue : Value
        {
            public override float Get(Element context)
            {
                var background = ((Table) context).Background;
                return background == null ? 0 : background.RightWidth;
            }
        }

        #endregion
    }
}