using Microsoft.Xna.Framework;
using Nez.ECS.Components.Renderables;
using Nez.ECS.Components.Renderables.Sprites;
using Nez.Maths;
using Nez.Utils;
using Nez.Utils.Fonts;

namespace Nez.ECS.Components.Text
{
    public class Text : Sprite
    {
        protected IFont Font;


        protected HorizontalAlign HorizontalAlign;
        private Vector2 _size;
        protected string text;
        protected VerticalAlign VerticalAlign;


        public Text(IFont font, string text, Vector2 localOffset, Color color)
        {
            Font = font;
            this.text = text;
            this.localOffset = localOffset;
            this.Color = color;
            HorizontalAlign = HorizontalAlign.Left;
            VerticalAlign = VerticalAlign.Top;

            UpdateSize();
        }

        public override RectangleF Bounds
        {
            get
            {
                if (AreBoundsDirty)
                {
                    Bounds.CalculateBounds(Entity.Transform.Position, localOffset, Origin, Entity.Transform.Scale,
                        Entity.Transform.Rotation, _size.X, _size.Y);
                    AreBoundsDirty = false;
                }

                return Bounds;
            }
        }

	    /// <summary>
	    ///     text to draw
	    /// </summary>
	    /// <value>The text.</value>
	    public string Value
        {
            get => text;
            set => SetText(value);
        }

	    /// <summary>
	    ///     horizontal alignment of the text
	    /// </summary>
	    /// <value>The horizontal origin.</value>
	    public HorizontalAlign HorizontalOrigin
        {
            get => HorizontalAlign;
            set => SetHorizontalAlign(value);
        }

	    /// <summary>
	    ///     vertical alignment of the text
	    /// </summary>
	    /// <value>The vertical origin.</value>
	    public VerticalAlign VerticalOrigin
        {
            get => VerticalAlign;
            set => SetVerticalAlign(value);
        }


        private void UpdateSize()
        {
            _size = Font.MeasureString(text);
            UpdateCentering();
        }


        private void UpdateCentering()
        {
            var oldOrigin = Origin;

            if (HorizontalAlign == HorizontalAlign.Left)
                oldOrigin.X = 0;
            else if (HorizontalAlign == HorizontalAlign.Center)
                oldOrigin.X = _size.X / 2;
            else
                oldOrigin.X = _size.X;

            if (VerticalAlign == VerticalAlign.Top)
                oldOrigin.Y = 0;
            else if (VerticalAlign == VerticalAlign.Center)
                oldOrigin.Y = _size.Y / 2;
            else
                oldOrigin.Y = _size.Y;

            origin = new Vector2((int) oldOrigin.X, (int) oldOrigin.Y);
        }


        public override void Render(Graphics.Graphics graphics, Camera camera)
        {
            BatcherIFontExt.DrawString(graphics.Batcher, Font, (string) text, Entity.Transform.Position + localOffset, Color,
                Entity.Transform.Rotation, origin, Entity.Transform.Scale, SpriteEffects, ((RenderableComponent) this).LayerDepth);
        }


        #region Fluent setters

        public Text SetFont(IFont font)
        {
            Font = font;
            UpdateSize();

            return this;
        }


        public Text SetText(string text)
        {
            this.text = text;
            UpdateSize();
            UpdateCentering();

            return this;
        }


        public Text SetHorizontalAlign(HorizontalAlign hAlign)
        {
            HorizontalAlign = hAlign;
            UpdateCentering();

            return this;
        }


        public Text SetVerticalAlign(VerticalAlign vAlign)
        {
            VerticalAlign = vAlign;
            UpdateCentering();

            return this;
        }

        #endregion
    }
}