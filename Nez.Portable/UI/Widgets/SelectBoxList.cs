using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nez.UI.Base;
using Nez.UI.Containers;
using Nez.Utils;

namespace Nez.UI.Widgets
{
    public class SelectBoxList<T> : ScrollPane where T : class
    {
        private bool _isListBelowSelectBox;
        private Element _previousScrollFocus;
        private Vector2 _screenPosition;

        private readonly SelectBox<T> _selectBox;
        public ListBox<T> ListBox;
        public int MaxListCount;


        public SelectBoxList(SelectBox<T> selectBox) : base(null, selectBox.GetStyle().ScrollStyle)
        {
            _selectBox = selectBox;

            SetOverscroll(false, false);
            SetFadeScrollBars(false);
            SetScrollingDisabled(true, false);

            ListBox = new ListBox<T>(selectBox.GetStyle().ListStyle);
            ListBox.SetTouchable(Touchable.Disabled);
            SetWidget(ListBox);

            ListBox.OnChanged += item =>
            {
                selectBox.GetSelection().Choose(item);
                if (selectBox.OnChanged != null)
                    selectBox.OnChanged(item);
                Hide();
            };
        }


        public void Show(Stage stage)
        {
            if (ListBox.IsTouchable())
                return;

            stage.AddElement(this);

            _screenPosition = _selectBox.LocalToStageCoordinates(Vector2.Zero);

            // show the list above or below the select box, limited to a number of items and the available height in the stage.
            var itemHeight = ListBox.GetItemHeight();
            var height = itemHeight * (MaxListCount <= 0
                             ? _selectBox.GetItems().Count
                             : System.Math.Min(MaxListCount, _selectBox.GetItems().Count));
            var scrollPaneBackground = GetStyle().Background;
            if (scrollPaneBackground != null)
                height += scrollPaneBackground.TopHeight + scrollPaneBackground.BottomHeight;
            var listBackground = ListBox.GetStyle().Background;
            if (listBackground != null)
                height += listBackground.TopHeight + listBackground.BottomHeight;

            var heightAbove = _screenPosition.Y;
            var heightBelow = Screen.Height /*camera.viewportHeight */ - _screenPosition.Y - _selectBox.GetHeight();
            _isListBelowSelectBox = true;
            if (height > heightBelow)
                if (heightAbove > heightBelow)
                {
                    _isListBelowSelectBox = false;
                    height = System.Math.Min(height, heightAbove);
                }
                else
                {
                    height = heightBelow;
                }

            if (!_isListBelowSelectBox)
                SetY(_screenPosition.Y - height);
            else
                SetY(_screenPosition.Y + _selectBox.GetHeight());
            SetX(_screenPosition.X);
            SetHeight(height);
            Validate();

            var width = System.Math.Max(PreferredWidth, _selectBox.GetWidth());
            if (PreferredHeight > height && !DisableY)
                width += GetScrollBarWidth();
            SetWidth(width);

            Validate();
            ScrollTo(0, ListBox.GetHeight() - _selectBox.GetSelectedIndex() * itemHeight - itemHeight / 2, 0, 0, true,
                true);
            UpdateVisualScroll();

            _previousScrollFocus = null;

            ListBox.GetSelection().Set(_selectBox.GetSelected());
            ListBox.SetTouchable(Touchable.Enabled);
            _selectBox.OnShow(this, _isListBelowSelectBox);
        }


        public void Hide()
        {
            if (!ListBox.IsTouchable() || !HasParent())
                return;

            ListBox.SetTouchable(Touchable.Disabled);

            if (Stage != null)
                if (_previousScrollFocus != null && _previousScrollFocus.GetStage() == null)
                    _previousScrollFocus = null;

            _selectBox.OnHide(this);
        }


        public override void Draw(Graphics.Graphics graphics, float parentAlpha)
        {
            var temp = _selectBox.LocalToStageCoordinates(Vector2.Zero);
            if (temp != _screenPosition)
                Core.Schedule(0f, false, this, t => ((SelectBoxList<T>) t.Context).Hide());

            base.Draw(graphics, parentAlpha);
        }


        protected override void Update()
        {
            if (Input.Input.IsKeyPressed(Keys.Escape))
            {
                Core.Schedule(0f, false, this, t => ((SelectBoxList<T>) t.Context).Hide());
                return;
            }

            if (Input.Input.LeftMouseButtonPressed)
            {
                var point = Stage.GetMousePosition();
                point = ScreenToLocalCoordinates(point);

                float yMin = 0, yMax = Height;

                // we need to include the list and the select box for our click checker. if the list is above the select box we expand the
                // height to include it. If the list is below we check for positions up to -_selectBox.height
                if (_isListBelowSelectBox)
                    yMin -= _selectBox.Height;
                else
                    yMax += _selectBox.Height;

                if (point.X < 0 || point.X > Width || point.Y > yMax || point.Y < yMin)
                    Core.Schedule(0f, false, this, t => ((SelectBoxList<T>) t.Context).Hide());
            }

            base.Update();
            ToFront();
        }
    }
}