using System.Globalization;
using Nez.Debug.Inspector.Attributes;
using Nez.UI;
using Nez.UI.Containers;
using Nez.UI.Widgets;

#if DEBUG
namespace Nez.Debug.Inspector.Inspectors
{
    public class FloatInspector : Inspector
    {
        private Slider _slider;
        private TextField _textField;


        public override void Initialize(Table table, Skin skin)
        {
            // if we have a RangeAttribute we need to make a slider
            var rangeAttr = GetFieldOrPropertyAttribute<RangeAttribute>();
            if (rangeAttr != null)
                SetupSlider(table, skin, rangeAttr.MinValue, rangeAttr.MaxValue, rangeAttr.StepSize);
            else
                SetupTextField(table, skin);
        }


        private void SetupTextField(Table table, Skin skin)
        {
            var label = CreateNameLabel(table, skin);
            _textField = new TextField(GetValue<float>().ToString(CultureInfo.InvariantCulture), skin);
            _textField.SetTextFieldFilter(new FloatFilter());
            _textField.OnTextChanged += (field, str) =>
            {
                if (float.TryParse(str, out var newValue))
                if(float.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out newValue))
                    SetValue(newValue);
            };

            table.Add(label);
            table.Add(_textField).SetMaxWidth(70);
        }


        private void SetupSlider(Table table, Skin skin, float minValue, float maxValue, float stepSize)
        {
            var label = CreateNameLabel(table, skin);
            _slider = new Slider(skin, null, minValue, maxValue);
            _slider.SetStepSize(stepSize);
            _slider.SetValue(GetValue<float>());
            _slider.OnChanged += newValue => { Setter.Invoke(newValue); };

            table.Add(label);
            table.Add(_slider);
        }


        public override void Update()
        {
            if (_textField != null)
                _textField.SetText(GetValue<float>().ToString());
            if (_slider != null)
                _slider.SetValue(GetValue<float>());
        }
    }
}
#endif