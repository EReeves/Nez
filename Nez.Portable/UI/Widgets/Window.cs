using Microsoft.Xna.Framework;
using Nez;
using Nez.Debug;
using Nez.Maths;
using Nez.UI.Base;
using Nez.UI.Containers;
using IDrawable = Nez.UI.Drawable.IDrawable;

namespace Nez.UI.Widgets
{
	/// <summary>
	///     A table that can be dragged and resized. The top padding is used as the window's title height.
	///     The preferred size of a window is the preferred size of the title text and the children as laid out by the table.
	///     After adding
	///     children to the window, it can be convenient to call {@link #pack()} to size the window to the size of the
	///     children.
	/// </summary>
	public class Window : Table, IInputListener
    {
        private static readonly int Move = 1 << 5;
        private bool _dragging;
        private bool _isMovable = true, _isResizable;
        private bool _keepWithinStage = true;
        private int _resizeBorderSize = 8;

        private WindowStyle _style;
        private readonly Label _titleLabel;
        private readonly Table _titleTable;


        public Window(string title, WindowStyle style)
        {
            Assert.IsNotNull(title, "title cannot be null");

            Touchable = Touchable.Enabled;
            Clip = true;

            _titleLabel = new Label(title, new LabelStyle(style.TitleFont, style.TitleFontColor));
            _titleLabel.SetEllipsis(true);

            _titleTable = new Table();
            _titleTable.Add(_titleLabel).SetExpandX().SetFillX().SetMinWidth(0);
            AddElement(_titleTable);

            SetStyle(style);
            Width = 150;
            Height = 150;
        }


        public Window(string title, Skin skin, string styleName = null) : this(title, skin.Get<WindowStyle>(styleName))
        {
        }


        public Window SetStyle(WindowStyle style)
        {
            this._style = style;
            SetBackground(style.Background);

            var labelStyle = _titleLabel.GetStyle();
            labelStyle.Font = style.TitleFont ?? labelStyle.Font;
            labelStyle.FontColor = style.TitleFontColor;
            _titleLabel.SetStyle(labelStyle);

            InvalidateHierarchy();
            return this;
        }


	    /// <summary>
	    ///     Returns the window's style. Modifying the returned style may not have an effect until {@link
	    ///     #setStyle(WindowStyle)} is called
	    /// </summary>
	    /// <returns>The style.</returns>
	    public WindowStyle GetStyle()
        {
            return _style;
        }


        public void KeepWithinStage()
        {
            if (!_keepWithinStage)
                return;

            var stage = GetStage();
            var parentWidth = stage.GetWidth();
            var parentHeight = stage.GetHeight();

            if (X < 0)
                X = 0;
            if (Y < 0)
                Y = 0;
            if (GetY(AlignInternal.Bottom) > parentHeight)
                Y = parentHeight - Height;
            if (GetX(AlignInternal.Right) > parentWidth)
                X = parentWidth - Width;
        }


        public override void Draw(Graphics.Graphics graphics, float parentAlpha)
        {
            KeepWithinStage();

            if (_style.StageBackground != null)
            {
                var stagePos = StageToLocalCoordinates(Vector2.Zero);
                var stageSize = StageToLocalCoordinates(new Vector2(Stage.GetWidth(), Stage.GetHeight()));
                DrawStageBackground(graphics, parentAlpha, GetX() + stagePos.X, GetY() + stagePos.Y,
                    GetX() + stageSize.X, GetY() + stageSize.Y);
            }

            base.Draw(graphics, parentAlpha);
        }


        protected void DrawStageBackground(Graphics.Graphics graphics, float parentAlpha, float x, float y, float width,
            float height)
        {
            _style.StageBackground.Draw(graphics, x, y, width, height, new Color(Color, (int) (Color.A * parentAlpha)));
        }


        protected override void DrawBackground(Graphics.Graphics graphics, float parentAlpha, float x, float y)
        {
            base.DrawBackground(graphics, parentAlpha, x, y);

            // Manually draw the title table before clipping is done.
            _titleTable.Color.A = Color.A;
            float padTop = GetPadTop(), padLeft = GetPadLeft();
            _titleTable.SetSize(GetWidth() - padLeft - GetPadRight(), padTop);
            _titleTable.SetPosition(padLeft, 0);
        }


        public override Element Hit(Vector2 point)
        {
            // TODO: is this correct? should we be transforming the point here?
            if (!HasParent())
                point = StageToLocalCoordinates(point);

            var hit = base.Hit(point);
            if (hit == null || hit == this)
                return hit;

            var height = GetHeight();
            if (Y <= height && Y >= height - GetPadTop() && X >= 0 && X <= GetWidth())
            {
                // Hit the title bar, don't use the hit child if it is in the Window's table.
                var current = hit;
                while (current.GetParent() != this)
                    current = current.GetParent();

                if (GetCell(current) != null)
                    return this;
            }
            return hit;
        }


        public bool IsMovable()
        {
            return _isMovable;
        }


        public Window SetMovable(bool isMovable)
        {
            _isMovable = isMovable;
            return this;
        }


        public Window SetKeepWithinStage(bool keepWithinStage)
        {
            _keepWithinStage = keepWithinStage;
            return this;
        }


        public bool IsResizable()
        {
            return _isResizable;
        }


        public Window SetResizable(bool isResizable)
        {
            _isResizable = isResizable;
            return this;
        }


        public Window SetResizeBorderSize(int resizeBorderSize)
        {
            this._resizeBorderSize = resizeBorderSize;
            return this;
        }


        public bool IsDragging()
        {
            return _dragging;
        }


        public float GetPrefWidth()
        {
            return System.Math.Max(PreferredWidth, _titleLabel.PreferredWidth + GetPadLeft() + GetPadRight());
        }


        public Table GetTitleTable()
        {
            return _titleTable;
        }


        public Label GetTitleLabel()
        {
            return _titleLabel;
        }


        #region IInputListener

        private int _edge;
        private float _startX, _startY, _lastX, _lastY;

        void IInputListener.OnMouseEnter()
        {
        }


        void IInputListener.OnMouseExit()
        {
        }


        bool IInputListener.OnMousePressed(Vector2 mousePos)
        {
            float width = GetWidth(), height = GetHeight();
            _edge = 0;
            if (_isResizable && mousePos.X >= 0 && mousePos.X < width && mousePos.Y >= 0 && mousePos.Y < height)
            {
                if (mousePos.X < _resizeBorderSize)
                    _edge |= AlignInternal.Left;
                if (mousePos.X > width - _resizeBorderSize)
                    _edge |= AlignInternal.Right;
                if (mousePos.Y < _resizeBorderSize)
                    _edge |= AlignInternal.Top;
                if (mousePos.Y > height - _resizeBorderSize)
                    _edge |= AlignInternal.Bottom;

                var tempResizeBorderSize = _resizeBorderSize;
                if (_edge != 0)
                    tempResizeBorderSize += 25;
                if (mousePos.X < tempResizeBorderSize)
                    _edge |= AlignInternal.Left;
                if (mousePos.X > width - tempResizeBorderSize)
                    _edge |= AlignInternal.Right;
                if (mousePos.Y < tempResizeBorderSize)
                    _edge |= AlignInternal.Top;
                if (mousePos.Y > height - tempResizeBorderSize)
                    _edge |= AlignInternal.Bottom;
            }

            if (_isMovable && _edge == 0 && mousePos.Y >= 0 && mousePos.Y <= GetPadTop() && mousePos.X >= 0 &&
                mousePos.X <= width)
                _edge = Move;

            _dragging = _edge != 0;

            _startX = mousePos.X;
            _startY = mousePos.Y;
            _lastX = mousePos.X;
            _lastY = mousePos.Y;

            return true;
        }


        void IInputListener.OnMouseMoved(Vector2 mousePos)
        {
            if (!_dragging)
                return;

            float width = GetWidth(), height = GetHeight();
            float windowX = GetX(), windowY = GetY();

            var stage = GetStage();
            var parentWidth = stage.GetWidth();
            var parentHeight = stage.GetHeight();

            var clampPosition = _keepWithinStage && GetParent() == stage.GetRoot();

            if ((_edge & Move) != 0)
            {
                float amountX = mousePos.X - _startX, amountY = mousePos.Y - _startY;
                windowX += amountX;
                windowY += amountY;
            }
            if ((_edge & AlignInternal.Left) != 0)
            {
                var amountX = mousePos.X - _startX;
                if (width - amountX < MinWidth)
                    amountX = -(MinWidth - width);
                if (clampPosition && windowX + amountX < 0)
                    amountX = -windowX;
                width -= amountX;
                windowX += amountX;
            }
            if ((_edge & AlignInternal.Top) != 0)
            {
                var amountY = mousePos.Y - _startY;
                if (height - amountY < MinHeight)
                    amountY = -(MinHeight - height);
                if (clampPosition && windowY + amountY < 0)
                    amountY = -windowY;
                height -= amountY;
                windowY += amountY;
            }
            if ((_edge & AlignInternal.Right) != 0)
            {
                var amountX = mousePos.X - _lastX;
                if (width + amountX < MinWidth)
                    amountX = MinWidth - width;
                if (clampPosition && windowX + width + amountX > parentWidth)
                    amountX = parentWidth - windowX - width;
                width += amountX;
            }
            if ((_edge & AlignInternal.Bottom) != 0)
            {
                var amountY = mousePos.Y - _lastY;
                if (height + amountY < MinHeight)
                    amountY = MinHeight - height;
                if (clampPosition && windowY + height + amountY > parentHeight)
                    amountY = parentHeight - windowY - height;
                height += amountY;
            }

            _lastX = mousePos.X;
            _lastY = mousePos.Y;
            SetBounds(Mathf.Round(windowX), Mathf.Round(windowY), Mathf.Round(width), Mathf.Round(height));
        }


        void IInputListener.OnMouseUp(Vector2 mousePos)
        {
            _dragging = false;
        }


        bool IInputListener.OnMouseScrolled(int mouseWheelDelta)
        {
            return false;
        }

        #endregion
    }


    public class WindowStyle
    {
        /** Optional. */
        public IDrawable Background;

        /** Optional. */
        public IDrawable StageBackground;

        public BitmapFont TitleFont;

        /** Optional. */
        public Color TitleFontColor = Color.White;


        public WindowStyle()
        {
            TitleFont = Graphics.Graphics.Instance.BitmapFont;
        }


        public WindowStyle(BitmapFont titleFont, Color titleFontColor, IDrawable background)
        {
            this.TitleFont = titleFont ?? Graphics.Graphics.Instance.BitmapFont;
            this.Background = background;
            this.TitleFontColor = titleFontColor;
        }


        public WindowStyle Clone()
        {
            return new WindowStyle
            {
                Background = Background,
                TitleFont = TitleFont,
                TitleFontColor = TitleFontColor,
                StageBackground = StageBackground
            };
        }
    }
}