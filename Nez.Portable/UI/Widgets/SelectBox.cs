using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Nez;
using Nez.Debug;
using Nez.UI.Base;
using Nez.UI.Containers;
using Nez.UI.Utils;
using Nez.Utils.Fonts;
using IDrawable = Nez.UI.Drawable.IDrawable;

namespace Nez.UI.Widgets
{
    public class SelectBox<T> : Element, IInputListener where T : class
    {
        private bool _isDisabled;
        private bool _isMouseOver;
        private readonly List<T> _items = new List<T>();
        private float _prefWidth, _prefHeight;
        private readonly SelectBoxList<T> _selectBoxList;
        private readonly ArraySelection<T> _selection;
        public Action<T> OnChanged;

        private SelectBoxStyle _style;


        public SelectBox(Skin skin) : this(skin.Get<SelectBoxStyle>())
        {
        }


        public SelectBox(Skin skin, string styleName = null) : this(skin.Get<SelectBoxStyle>(styleName))
        {
        }


        public SelectBox(SelectBoxStyle style)
        {
            _selection = new ArraySelection<T>(_items);
            SetStyle(style);
            SetSize(PreferredWidth, PreferredHeight);

            _selection.SetElement(this);
            _selection.SetRequired(true);
            _selectBoxList = new SelectBoxList<T>(this);
        }


        public override void Layout()
        {
            var bg = _style.Background;
            var font = _style.Font;

            if (bg != null)
                _prefHeight = System.Math.Max(bg.TopHeight + bg.BottomHeight + font.LineHeight - font.Descent * 2f,
                    bg.MinHeight);
            else
                _prefHeight = font.LineHeight - font.Descent * 2;

            float maxItemWidth = 0;
            for (var i = 0; i < _items.Count; i++)
                maxItemWidth = (float) System.Math.Max((double) font.MeasureString(_items[i].ToString()).X, maxItemWidth);


            _prefWidth = maxItemWidth;
            if (bg != null)
                _prefWidth += bg.LeftWidth + bg.RightWidth;

            var listStyle = _style.ListStyle;
            var scrollStyle = _style.ScrollStyle;
            var listWidth = maxItemWidth + listStyle.Selection.LeftWidth + listStyle.Selection.RightWidth;
            if (scrollStyle.Background != null)
                listWidth += scrollStyle.Background.LeftWidth + scrollStyle.Background.RightWidth;
            if (_selectBoxList == null || !_selectBoxList.IsScrollingDisabledY())
                listWidth += System.Math.Max(_style.ScrollStyle.VScroll != null ? _style.ScrollStyle.VScroll.MinWidth : 0,
                    _style.ScrollStyle.VScrollKnob != null ? _style.ScrollStyle.VScrollKnob.MinWidth : 0);
            _prefWidth = System.Math.Max(_prefWidth, listWidth);
        }


        public override void Draw(Graphics.Graphics graphics, float parentAlpha)
        {
            Validate();

            IDrawable background;
            if (_isDisabled && _style.BackgroundDisabled != null)
                background = _style.BackgroundDisabled;
            else if (_selectBoxList.HasParent() && _style.BackgroundOpen != null)
                background = _style.BackgroundOpen;
            else if (_isMouseOver && _style.BackgroundOver != null)
                background = _style.BackgroundOver;
            else if (_style.Background != null)
                background = _style.Background;
            else
                background = null;

            var font = _style.Font;
            var fontColor = _isDisabled ? _style.DisabledFontColor : _style.FontColor;

            var color = GetColor();
            color = new Color(color, (int) (color.A * parentAlpha));
            var x = GetX();
            var y = GetY();
            var width = GetWidth();
            var height = GetHeight();

            if (background != null)
                background.Draw(graphics, x, y, width, height, color);

            var selected = _selection.First();
            if (selected != null)
            {
                var str = selected.ToString();
                if (background != null)
                {
                    width -= background.LeftWidth + background.RightWidth;
                    height -= background.BottomHeight + background.TopHeight;
                    x += background.LeftWidth;
                    y += (int) (height / 2 + background.BottomHeight - font.LineHeight / 2);
                }
                else
                {
                    y += (int) (height / 2 + font.LineHeight / 2);
                }

                fontColor = new Color(fontColor, (int) (fontColor.A * parentAlpha));
                graphics.Batcher.DrawString(font, str, new Vector2(x, y), fontColor);
            }
        }


        #region ILayout

        public override float PreferredWidth
        {
            get
            {
                Validate();
                return _prefWidth;
            }
        }

        public override float PreferredHeight
        {
            get
            {
                Validate();
                return _prefHeight;
            }
        }

        #endregion


        #region IInputListener

        void IInputListener.OnMouseEnter()
        {
            _isMouseOver = true;
        }


        void IInputListener.OnMouseExit()
        {
            _isMouseOver = false;
        }


        bool IInputListener.OnMousePressed(Vector2 mousePos)
        {
            if (_isDisabled)
                return false;

            if (_selectBoxList.HasParent())
                HideList();
            else
                ShowList();

            return true;
        }


        void IInputListener.OnMouseMoved(Vector2 mousePos)
        {
        }


        void IInputListener.OnMouseUp(Vector2 mousePos)
        {
        }


        bool IInputListener.OnMouseScrolled(int mouseWheelDelta)
        {
            return false;
        }

        #endregion


        #region config

	    /// <summary>
	    ///     Set the max number of items to display when the select box is opened. Set to 0 (the default) to display as many as
	    ///     fit in
	    ///     the stage height.
	    /// </summary>
	    /// <returns>The max list count.</returns>
	    /// <param name="maxListCount">Max list count.</param>
	    public void SetMaxListCount(int maxListCount)
        {
            _selectBoxList.MaxListCount = maxListCount;
        }


	    /// <summary>
	    ///     returns Max number of items to display when the box is opened, or <= 0 to display them all.
	    /// </summary>
	    /// <returns>The max list count.</returns>
	    public int GetMaxListCount()
        {
            return _selectBoxList.MaxListCount;
        }


        internal override void SetStage(Stage stage)
        {
            if (stage == null)
                _selectBoxList.Hide();
            base.SetStage(stage);
        }


        public void SetStyle(SelectBoxStyle style)
        {
            Assert.IsNotNull(style, "style cannot be null");
            this._style = style;
            InvalidateHierarchy();
        }


	    /// <summary>
	    ///     Returns the select box's style. Modifying the returned style may not have an effect until setStyle(SelectBoxStyle)
	    ///     is called.
	    /// </summary>
	    /// <returns>The style.</returns>
	    public SelectBoxStyle GetStyle()
        {
            return _style;
        }


	    /// <summary>
	    ///     Set the backing Array that makes up the choices available in the SelectBox
	    /// </summary>
	    /// <returns>The items.</returns>
	    /// <param name="newItems">New items.</param>
	    public void SetItems(params T[] newItems)
        {
            SetItems(new List<T>(newItems));
        }


	    /// <summary>
	    ///     Sets the items visible in the select box
	    /// </summary>
	    /// <returns>The items.</returns>
	    /// <param name="newItems">New items.</param>
	    public void SetItems(List<T> newItems)
        {
            Assert.IsNotNull(newItems, "newItems cannot be null");
            var oldPrefWidth = PreferredWidth;

            _items.Clear();
            _items.AddRange(newItems);
            _selection.Validate();
            _selectBoxList.ListBox.SetItems(_items);

            Invalidate();
            Validate();
            if (oldPrefWidth != _prefWidth)
            {
                InvalidateHierarchy();
                SetSize(_prefWidth, _prefHeight);
            }
        }


        public void ClearItems()
        {
            if (_items.Count == 0)
                return;

            _items.Clear();
            _selection.Clear();
            InvalidateHierarchy();
        }


	    /// <summary>
	    ///     Returns the internal items array. If modified, setItems(Array) must be called to reflect the changes.
	    /// </summary>
	    /// <returns>The items.</returns>
	    public List<T> GetItems()
        {
            return _items;
        }


	    /// <summary>
	    ///     Get the set of selected items, useful when multiple items are selected returns a Selection object containing the
	    ///     selected elements
	    /// </summary>
	    /// <returns>The selection.</returns>
	    public ArraySelection<T> GetSelection()
        {
            return _selection;
        }


	    /// <summary>
	    ///     Returns the first selected item, or null. For multiple selections use SelectBox#getSelection()
	    /// </summary>
	    /// <returns>The selected.</returns>
	    public T GetSelected()
        {
            return _selection.First();
        }


	    /// <summary>
	    ///     Sets the selection to only the passed item, if it is a possible choice, else selects the first item.
	    /// </summary>
	    /// <returns>The selected.</returns>
	    /// <param name="item">Item.</param>
	    public void SetSelected(T item)
        {
            if (_items.Contains(item))
                _selection.Set(item);
            else if (_items.Count > 0)
                _selection.Set(_items[0]);
            else
                _selection.Clear();
        }


	    /// <summary>
	    ///     returns The index of the first selected item. The top item has an index of 0. Nothing selected has an index of -1.
	    /// </summary>
	    /// <returns>The selected index.</returns>
	    public int GetSelectedIndex()
        {
            var selected = _selection.Items();
            return selected.Count == 0 ? -1 : _items.IndexOf(selected.First());
        }


	    /// <summary>
	    ///     Sets the selection to only the selected index
	    /// </summary>
	    /// <returns>The selected index.</returns>
	    /// <param name="index">Index.</param>
	    public void SetSelectedIndex(int index)
        {
            _selection.Set(_items[index]);
        }


        public void SetDisabled(bool disabled)
        {
            if (disabled && !_isDisabled)
                HideList();
            _isDisabled = disabled;
        }


        public bool IsDisabled()
        {
            return _isDisabled;
        }


        public void ShowList()
        {
            if (_items.Count == 0)
                return;
            _selectBoxList.Show(GetStage());
        }


        public void HideList()
        {
            _selectBoxList.Hide();
        }


	    /// <summary>
	    ///     Returns the ListBox shown when the select box is open
	    /// </summary>
	    /// <returns>The list.</returns>
	    public ListBox<T> GetListBox()
        {
            return _selectBoxList.ListBox;
        }


	    /// <summary>
	    ///     Disables scrolling of the list shown when the select box is open.
	    /// </summary>
	    /// <returns>The scrolling disabled.</returns>
	    /// <param name="y">The y coordinate.</param>
	    public void SetScrollingDisabled(bool y)
        {
            _selectBoxList.SetScrollingDisabled(true, y);
            InvalidateHierarchy();
        }


	    /// <summary>
	    ///     Returns the scroll pane containing the list that is shown when the select box is open.
	    /// </summary>
	    /// <returns>The scroll pane.</returns>
	    public ScrollPane GetScrollPane()
        {
            return _selectBoxList;
        }


        public void OnShow(Element selectBoxList, bool below)
        {
            //_selectBoxList.getColor().A = 0;
            //_selectBoxList.addAction( fadeIn( 0.3f, Interpolation.fade ) );
        }


        public void OnHide(Element selectBoxList)
        {
            //selectBoxList.getColor().A = 255;
            //selectBoxList.addAction( sequence( fadeOut( 0.15f, Interpolation.fade ), removeActor() ) );
            selectBoxList.Remove();
        }

        #endregion
    }


    public class SelectBoxStyle
    {
        /** Optional */
        public IDrawable Background;

        /** Optional */
        public IDrawable BackgroundOver, BackgroundOpen, BackgroundDisabled;

        /** Optional */
        public Color DisabledFontColor;

        public BitmapFont Font;
        public Color FontColor = Color.White;
        public ListBoxStyle ListStyle;
        public ScrollPaneStyle ScrollStyle;


        public SelectBoxStyle()
        {
            Font = Graphics.Graphics.Instance.BitmapFont;
        }

        public SelectBoxStyle(BitmapFont font, Color fontColor, IDrawable background, ScrollPaneStyle scrollStyle,
            ListBoxStyle listStyle)
        {
            this.Font = font;
            this.FontColor = fontColor;
            this.Background = background;
            this.ScrollStyle = scrollStyle;
            this.ListStyle = listStyle;
        }
    }
}