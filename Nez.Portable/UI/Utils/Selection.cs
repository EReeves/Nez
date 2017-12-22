using System.Collections.Generic;
using System.Linq;

namespace Nez.UI
{
    public class Selection<T> where T : class
    {
        protected bool isDisabled;
        private Element _element;
        private T _lastSelected;
        protected bool Multiple;
        private readonly List<T> _old = new List<T>();
        private bool _programmaticChangeEvents = true;
        protected bool Required;
        protected List<T> Selected = new List<T>();
        private bool _toggle;


	    /// <summary>
	    ///     An Element to fire ChangeEvent on when the selection changes, or null
	    /// </summary>
	    /// <returns>The element.</returns>
	    /// <param name="element">element.</param>
	    public Element SetElement(Element element)
        {
            this._element = element;
            return element;
        }


	    /// <summary>
	    ///     Selects or deselects the specified item based on how the selection is configured, whether ctrl is currently
	    ///     pressed, etc.
	    ///     This is typically invoked by user interaction.
	    /// </summary>
	    /// <param name="item">Item.</param>
	    public virtual void Choose(T item)
        {
            Assert.IsNotNull(item, "item cannot be null");
            if (isDisabled)
                return;
            Snapshot();

            try
            {
                if ((_toggle || !Required && Selected.Count == 1 || InputUtils.IsControlDown()) &&
                    Selected.Contains(item))
                {
                    if (Required && Selected.Count == 1)
                        return;
                    Selected.Remove(item);
                    _lastSelected = null;
                }
                else
                {
                    var modified = false;
                    if (!Multiple || !_toggle && !InputUtils.IsControlDown())
                    {
                        if (Selected.Count == 1 && Selected.Contains(item))
                            return;
                        modified = Selected.Count > 0;
                        Selected.Clear();
                    }

                    if (!Selected.AddIfNotPresent(item) && !modified)
                        return;
                    _lastSelected = item;
                }

                if (FireChangeEvent())
                    Revert();
                else
                    Changed();
            }
            finally
            {
                Cleanup();
            }
        }


        public bool HasItems()
        {
            return Selected.Count > 0;
        }


        public bool IsEmpty()
        {
            return Selected.Count == 0;
        }


        public int Size()
        {
            return Selected.Count;
        }


        public List<T> Items()
        {
            return Selected;
        }


	    /// <summary>
	    ///     Returns the first selected item, or null
	    /// </summary>
	    public T First()
        {
            return Selected.FirstOrDefault();
        }


        protected void Snapshot()
        {
            _old.Clear();
            _old.AddRange(Selected);
        }


        protected void Revert()
        {
            Selected.Clear();
            Selected.AddRange(_old);
        }


        protected void Cleanup()
        {
            _old.Clear();
        }


	    /// <summary>
	    ///     Sets the selection to only the specified item
	    /// </summary>
	    /// <param name="item">Item.</param>
	    public Selection<T> Set(T item)
        {
            Assert.IsNotNull(item, "item cannot be null.");

            if (Selected.Count == 1 && Selected.First() == item)
                return this;

            Snapshot();
            Selected.Clear();
            Selected.Add(item);

            if (_programmaticChangeEvents && FireChangeEvent())
            {
                Revert();
            }
            else
            {
                _lastSelected = item;
                Changed();
            }
            Cleanup();
            return this;
        }


        public Selection<T> SetAll(List<T> items)
        {
            var added = false;
            Snapshot();
            _lastSelected = null;
            Selected.Clear();
            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                Assert.IsNotNull(item, "item cannot be null");
                added = Selected.AddIfNotPresent(item);
            }

            if (added)
                if (_programmaticChangeEvents && FireChangeEvent())
                {
                    Revert();
                }
                else if (items.Count > 0)
                {
                    _lastSelected = items.Last();
                    Changed();
                }
            Cleanup();
            return this;
        }


	    /// <summary>
	    ///     Adds the item to the selection
	    /// </summary>
	    /// <param name="item">Item.</param>
	    public void Add(T item)
        {
            Assert.IsNotNull(item, "item cannot be null");
            if (!Selected.AddIfNotPresent(item))
                return;

            if (_programmaticChangeEvents && FireChangeEvent())
            {
                Selected.Remove(item);
            }
            else
            {
                _lastSelected = item;
                Changed();
            }
        }


        public void AddAll(List<T> items)
        {
            var added = false;
            Snapshot();
            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                Assert.IsNotNull(item, "item cannot be null");
                added = Selected.AddIfNotPresent(item);
            }
            if (added)
                if (_programmaticChangeEvents && FireChangeEvent())
                {
                    Revert();
                }
                else
                {
                    _lastSelected = items.LastOrDefault();
                    Changed();
                }
            Cleanup();
        }


        public void Remove(T item)
        {
            Assert.IsNotNull(item, "item cannot be null");
            if (!Selected.Remove(item))
                return;

            if (_programmaticChangeEvents && FireChangeEvent())
            {
                Selected.Add(item);
            }
            else
            {
                _lastSelected = null;
                Changed();
            }
        }


        public void RemoveAll(List<T> items)
        {
            var removed = false;
            Snapshot();
            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                Assert.IsNotNull(item, "item cannot be null");
                removed = Selected.Remove(item);
            }

            if (removed)
                if (_programmaticChangeEvents && FireChangeEvent())
                {
                    Revert();
                }
                else
                {
                    _lastSelected = null;
                    Changed();
                }
            Cleanup();
        }


        public void Clear()
        {
            if (Selected.Count == 0)
                return;

            Snapshot();
            Selected.Clear();
            if (_programmaticChangeEvents && FireChangeEvent())
            {
                Revert();
            }
            else
            {
                _lastSelected = null;
                Changed();
            }
            Cleanup();
        }


	    /// <summary>
	    ///     Called after the selection changes. The default implementation does nothing.
	    /// </summary>
	    protected virtual void Changed()
        {
        }


	    /// <summary>
	    ///     Fires a change event on the selection's Element, if any. Called internally when the selection changes, depending on
	    ///     setProgrammaticChangeEvents(bool)
	    /// </summary>
	    public bool FireChangeEvent()
        {
            if (_element == null)
                return false;

            // TODO: if actual events are ever used switch over to this
            //var changeEvent = Pools.obtain( ChangeEvent.class);
            //try {
            //	return actor.fire(changeEvent);
            //} finally {
            //	Pools.free(changeEvent);
            //}

            return false;
        }


        public bool Contains(T item)
        {
            if (item == null)
                return false;
            return Selected.Contains(item);
        }


	    /// <summary>
	    ///     Makes a best effort to return the last item selected, else returns an arbitrary item or null if the selection is
	    ///     empty.
	    /// </summary>
	    /// <returns>The last selected.</returns>
	    public T GetLastSelected()
        {
            if (_lastSelected != null)
                return _lastSelected;

            return Selected.FirstOrDefault();
        }


	    /// <summary>
	    ///     If true, prevents choose(Object) from changing the selection. Default is false.
	    /// </summary>
	    /// <param name="isDisabled">Is disabled.</param>
	    public Selection<T> SetDisabled(bool isDisabled)
        {
            this.isDisabled = isDisabled;
            return this;
        }


        public bool IsDisabled()
        {
            return isDisabled;
        }


        public bool GetToggle()
        {
            return _toggle;
        }


	    /// <summary>
	    ///     If true, prevents choose(Object) from clearing the selection. Default is false.
	    /// </summary>
	    /// <param name="toggle">Toggle.</param>
	    public Selection<T> SetToggle(bool toggle)
        {
            this._toggle = toggle;
            return this;
        }


        public bool GetMultiple()
        {
            return Multiple;
        }


	    /// <summary>
	    ///     If true, allows choose(Object) to select multiple items. Default is false.
	    /// </summary>
	    /// <param name="multiple">Multiple.</param>
	    public Selection<T> SetMultiple(bool multiple)
        {
            this.Multiple = multiple;
            return this;
        }


        public bool GetRequired()
        {
            return Required;
        }


	    /// <summary>
	    ///     If true, prevents choose(Object) from selecting none. Default is false.
	    /// </summary>
	    /// <param name="required">Required.</param>
	    public Selection<T> SetRequired(bool required)
        {
            this.Required = required;
            return this;
        }


	    /// <summary>
	    ///     If false, only choose(Object) will fire a change event. Default is true.
	    /// </summary>
	    /// <param name="programmaticChangeEvents">Programmatic change events.</param>
	    public Selection<T> SetProgrammaticChangeEvents(bool programmaticChangeEvents)
        {
            this._programmaticChangeEvents = programmaticChangeEvents;
            return this;
        }


        public override string ToString()
        {
            return Selected.ToString();
        }
    }
}