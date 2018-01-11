using Nez.UI.Base;
using Nez.UI.Drawable;

namespace Nez.UI.Widgets
{
    public class TextTooltip : Tooltip
    {
        public TextTooltip(string text, Element targetElement, Skin skin, string styleName = null) : this(text,
            targetElement, skin.Get<TextTooltipStyle>(styleName))
        {
        }


        public TextTooltip(string text, Element targetElement, TextTooltipStyle style) : base(null, targetElement)
        {
            var label = new Label(text, style.LabelStyle);
            Container.SetElement(label);
            SetStyle(style);
        }


        public TextTooltip SetStyle(TextTooltipStyle style)
        {
            Container.GetElement<Label>().SetStyle(style.LabelStyle);
            Container.SetBackground(style.Background);
            return this;
        }
    }


    public class TextTooltipStyle
    {
        /** Optional. */
        public IDrawable Background;

        public LabelStyle LabelStyle;


        public TextTooltipStyle()
        {
        }


        public TextTooltipStyle(LabelStyle label, IDrawable background)
        {
            LabelStyle = label;
            this.Background = background;
        }
    }
}