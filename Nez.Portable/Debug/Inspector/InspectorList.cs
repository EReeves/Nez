using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Nez.ECS;
using Nez.Graphics.PostProcessing;
using Nez.UI;
using Nez.UI.Containers;
using Nez.UI.Widgets;

#if DEBUG
namespace Nez.Debug.Inspector
{
	/// <summary>
	///     container for a Component/PostProcessor/Transform and all of its inspectable properties
	/// </summary>
	public class InspectorList
    {
        private CheckBox _enabledCheckbox;

        private readonly List<Inspectors.Inspector> _inspectors;
        public string Name;
        public object Target;


        public InspectorList(object target)
        {
            this.Target = target;
            Name = target.GetType().Name;
            _inspectors = Inspectors.Inspector.GetInspectableProperties(target);
        }


        public InspectorList(Transform transform)
        {
            Name = "Transform";
            _inspectors = Inspectors.Inspector.GetTransformProperties(transform);
        }


        public void Initialize(Table table, Skin skin)
        {
            table.GetRowDefaults().SetPadTop(10);
            table.Add(Name.Replace("PostProcessor", string.Empty)).GetElement<Label>().SetFontScale(1f)
                .SetFontColor(new Color(241, 156, 0));

            // if we have a component, stick a bool for enabled here
            if (Target != null)
            {
                _enabledCheckbox = new CheckBox(string.Empty, skin);
                _enabledCheckbox.ProgrammaticChangeEvents = false;

                if (Target is Component)
                    _enabledCheckbox.IsChecked = ((Component) Target).Enabled;
                else if (Target is PostProcessor)
                    _enabledCheckbox.IsChecked = ((PostProcessor) Target).Enabled;

                _enabledCheckbox.OnChanged += newValue =>
                {
                    if (Target is Component)
                        ((Component) Target).Enabled = newValue;
                    else if (Target is PostProcessor)
                        ((PostProcessor) Target).Enabled = newValue;
                };

                table.Add(_enabledCheckbox).Right();
            }
            table.Row();

            foreach (var i in _inspectors)
            {
                i.Initialize(table, skin);
                table.Row();
            }
        }


        public void Update()
        {
            foreach (var i in _inspectors)
                i.Update();
        }
    }
}
#endif