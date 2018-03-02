using System.Globalization;
using Microsoft.Xna.Framework;
using Nez.UI;
using Nez.UI.Containers;
using Nez.UI.Widgets;

#if DEBUG
namespace Nez.Debug.Inspector.Inspectors
{
    public class Vector2Inspector : Inspector
    {
        private TextField _textFieldX, _textFieldY;


        public override void Initialize(Table table, Skin skin)
        {
            var value = GetValue<Vector2>();
            var label = CreateNameLabel(table, skin);

            var labelX = new Label("x", skin);
            _textFieldX = new TextField(value.X.ToString(CultureInfo.InvariantCulture), skin);
            _textFieldX.SetTextFieldFilter(new FloatFilter()).SetPreferredWidth(50);
            _textFieldX.OnTextChanged += (field, str) =>
            {
                if (float.TryParse(str,NumberStyles.Float,CultureInfo.InvariantCulture, out var newX))
                {
                    var newValue = GetValue<Vector2>();
                    newValue.X = newX;
                    SetValue(newValue);
                }
            };

            var labelY = new Label("y", skin);
            _textFieldY = new TextField(value.Y.ToString(CultureInfo.InvariantCulture), skin);
            _textFieldY.SetTextFieldFilter(new FloatFilter()).SetPreferredWidth(50);
            _textFieldY.OnTextChanged += (field, str) =>
            {
                float newY;
                if (float.TryParse(str, out newY))
                {
                    var newValue = GetValue<Vector2>();
                    newValue.Y = newY;
                    SetValue(newValue);
                }
            };

            var hBox = new HorizontalGroup(5);
            hBox.AddElement(labelX);
            hBox.AddElement(_textFieldX);
            hBox.AddElement(labelY);
            hBox.AddElement(_textFieldY);

            table.Add(label);
            table.Add(hBox);
        }


        public override void Update()
        {
            var value = GetValue<Vector2>();
            _textFieldX.SetText(value.X.ToString(CultureInfo.InvariantCulture));
            _textFieldY.SetText(value.Y.ToString(CultureInfo.InvariantCulture));
        }
    }
}
#endif