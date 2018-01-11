using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.Debug.Inspector.Attributes;
using Nez.Graphics;
using Nez.UI;
using Nez.UI.Containers;
using Nez.UI.Widgets;
using Nez.Utils;
using Nez.Utils.Extensions;

#if DEBUG
namespace Nez.Debug.Inspector.Inspectors
{
    public class EffectInspector : Inspector
    {
        private readonly List<Inspector> _inspectors = new List<Inspector>();


        public override void Initialize(Table table, Skin skin)
        {
            // we either have a getter that gets a Material or an Effedt
            var effect = ValueType == typeof(Material) ? GetValue<Material>().Effect : GetValue<Effect>();
            if (effect == null)
                return;

            // add a header and indent our cells
            table.Add(effect.GetType().Name).SetColspan(2).GetElement<Label>().SetFontColor(new Color(228, 228, 76));
            table.Row().SetPadLeft(15);

            // figure out which properties are useful to add to the inspector
            var effectProps = ReflectionUtils.GetProperties(effect.GetType());
            foreach (var prop in effectProps)
            {
                if (prop.DeclaringType == typeof(Effect))
                    continue;

                if (!prop.CanRead || !prop.CanWrite || prop.Name == "Name")
                    continue;

                if ((!prop.GetMethod.IsPublic || !prop.SetMethod.IsPublic) &&
                    prop.GetCustomAttributes<InspectableAttribute>().Count() == 0)
                    continue;

                var inspector = GetInspectorForType(prop.PropertyType, effect, prop);
                if (inspector != null)
                {
                    inspector.SetTarget(effect, prop);
                    inspector.Initialize(table, skin);
                    _inspectors.Add(inspector);

                    table.Row().SetPadLeft(15);
                }
            }

            table.Row();
        }


        public override void Update()
        {
            foreach (var i in _inspectors)
                i.Update();
        }
    }
}
#endif