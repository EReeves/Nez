using Nez.UI;
using Nez.UI.Containers;
using Nez.UI.Widgets;

#if DEBUG
namespace Nez.Debug.Inspector.Inspectors
{
    public class StringInspector : Inspector
    {
        private TextField _textField;


        public override void Initialize(Table table, Skin skin)
        {
            var label = CreateNameLabel(table, skin);
            _textField = new TextField(GetValue<string>(), skin);
            _textField.SetTextFieldFilter(new FloatFilter());
            _textField.OnTextChanged += (field, str) => { SetValue(str); };

            table.Add(label);
            table.Add(_textField).SetMaxWidth(70);
        }


        public override void Update()
        {
            _textField.SetText(GetValue<string>());
        }
    }
}
#endif