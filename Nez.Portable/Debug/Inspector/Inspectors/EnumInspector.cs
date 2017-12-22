using System;
using System.Collections.Generic;
using Nez.UI;

#if DEBUG
namespace Nez
{
    public class EnumInspector : Inspector
    {
        private SelectBox<string> _selectBox;


        public override void Initialize(Table table, Skin skin)
        {
            var label = CreateNameLabel(table, skin);

            // gotta get ugly here
            _selectBox = new SelectBox<string>(skin);

            var enumValues = Enum.GetValues(ValueType);
            var enumStringValues = new List<string>();
            foreach (var e in enumValues)
                enumStringValues.Add(e.ToString());
            _selectBox.SetItems(enumStringValues);

            _selectBox.OnChanged += selectedItem => { SetValue(Enum.Parse(ValueType, selectedItem)); };

            table.Add(label);
            table.Add(_selectBox).SetFillX();
        }


        public override void Update()
        {
            _selectBox.SetSelected(GetValue<object>().ToString());
        }
    }
}
#endif