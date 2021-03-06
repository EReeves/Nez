﻿using Nez.UI;
using Nez.UI.Containers;
using Nez.UI.Widgets;

#if DEBUG
namespace Nez.Debug.Inspector.Inspectors
{
    public class BoolInspector : Inspector
    {
        private CheckBox _checkbox;


        public override void Initialize(Table table, Skin skin)
        {
            var label = CreateNameLabel(table, skin);
            _checkbox = new CheckBox(string.Empty, skin);
            _checkbox.ProgrammaticChangeEvents = false;
            _checkbox.IsChecked = GetValue<bool>();
            _checkbox.OnChanged += newValue => { SetValue(newValue); };

            table.Add(label);
            table.Add(_checkbox);
        }


        public override void Update()
        {
            _checkbox.IsChecked = GetValue<bool>();
        }
    }
}
#endif