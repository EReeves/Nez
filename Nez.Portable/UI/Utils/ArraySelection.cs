using System.Collections.Generic;
using System.Linq;
using Nez.Debug;
using Nez.Input;

namespace Nez.UI.Utils
{
    public class ArraySelection<T> : Selection<T> where T : class
    {
        private readonly List<T> _array;
        private bool _rangeSelect = true;
        private int _rangeStart;


        public ArraySelection(List<T> array)
        {
            this._array = array;
        }


        public override void Choose(T item)
        {
            Assert.IsNotNull(item, "item cannot be null");
            if (isDisabled)
                return;

            var index = _array.IndexOf(item);
            if (Selected.Count > 0 && _rangeSelect && Multiple && InputUtils.IsShiftDown())
            {
                var oldRangeState = _rangeStart;
                Snapshot();
                // Select new range.
                int start = _rangeStart, end = index;
                if (start > end)
                {
                    var temp = end;
                    end = start;
                    start = temp;
                }

                if (!InputUtils.IsControlDown())
                    Selected.Clear();
                for (var i = start; i <= end; i++)
                    Selected.Add(_array[i]);

                if (FireChangeEvent())
                {
                    _rangeStart = oldRangeState;
                    Revert();
                }
                Cleanup();
                return;
            }
            _rangeStart = index;

            base.Choose(item);
        }


        public bool GetRangeSelect()
        {
            return _rangeSelect;
        }


        public void SetRangeSelect(bool rangeSelect)
        {
            this._rangeSelect = rangeSelect;
        }


	    /// <summary>
	    ///     Removes objects from the selection that are no longer in the items array. If getRequired() is true and there is
	    ///     no selected item, the first item is selected.
	    /// </summary>
	    public void Validate()
        {
            if (_array.Count == 0)
            {
                Clear();
                return;
            }

            for (var i = Selected.Count - 1; i >= 0; i--)
            {
                var item = Selected[i];
                if (!_array.Contains(item))
                    Selected.Remove(item);
            }

            if (Required && Selected.Count == 0)
                Set(_array.First());
        }
    }
}