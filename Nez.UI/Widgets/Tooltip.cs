using Microsoft.Xna.Framework;
using Nez.UI.Base;
using Nez.UI.Containers;
using Nez.UI.Utils;

namespace Nez.UI.Widgets
{
	/// <summary>
	///     A listener that shows a tooltip Element when another Element is hovered over with the mouse.
	/// </summary>
	public class Tooltip : Element
    {
        protected Container Container;
        private bool _instant, _always;
        private bool _isMouseOver;

        private readonly TooltipManager _manager;
        protected Element TargetElement;


        public Tooltip(Element contents, Element targetElement)
        {
            _manager = TooltipManager.GetInstance();

            Container = new Container(contents);
            Container.SetOrigin(AlignInternal.Center);
            TargetElement = targetElement;
            Container.SetTouchable(Touchable.Disabled);
        }


        public override Element Hit(Vector2 point)
        {
            // we do some rejiggering here by checking for hits on our target and using that
            var local = TargetElement.ScreenToLocalCoordinates(point);
            if (TargetElement.Hit(local) != null)
            {
                if (!_isMouseOver)
                {
                    _isMouseOver = true;
                    _manager.Enter(this);
                }
                SetContainerPosition(local.X, local.Y);
            }
            else if (_isMouseOver)
            {
                _isMouseOver = false;
                _manager.Hide(this);
            }
            return null;
        }


        private void SetContainerPosition(float x, float y)
        {
            var stage = TargetElement.GetStage();
            if (stage == null)
                return;

            Container.Pack();
            float offsetX = _manager.OffsetX, offsetY = _manager.OffsetY, dist = _manager.EdgeDistance;
            var point = TargetElement.LocalToStageCoordinates(new Vector2(x + offsetX - Container.GetWidth() / 2,
                y - offsetY - Container.GetHeight()));
            if (point.Y < dist)
                point = TargetElement.LocalToStageCoordinates(new Vector2(x + offsetX, y + offsetY));
            if (point.X < dist)
                point.X = dist;
            if (point.X + Container.GetWidth() > stage.GetWidth() - dist)
                point.X = stage.GetWidth() - dist - Container.GetWidth();
            if (point.Y + Container.GetHeight() > stage.GetHeight() - dist)
                point.Y = stage.GetHeight() - dist - Container.GetHeight();
            Container.SetPosition(point.X, point.Y);

            point = TargetElement.LocalToStageCoordinates(new Vector2(TargetElement.GetWidth() / 2,
                TargetElement.GetHeight() / 2));
            point -= new Vector2(Container.GetX(), Container.GetY());
            Container.SetOrigin(point.X, point.Y);
        }


        #region config

        public TooltipManager GetManager()
        {
            return _manager;
        }


        public Container GetContainer()
        {
            return Container;
        }


        public Tooltip SetElement(Element contents)
        {
            Container.SetElement(contents);
            return this;
        }


        public Element GetElement()
        {
            return Container.GetElement();
        }


        public T GetElement<T>() where T : Element
        {
            return Container.GetElement<T>();
        }


        public Tooltip SetTargetElement(Element targetElement)
        {
            TargetElement = targetElement;
            return this;
        }


        public Element GetTargetElement()
        {
            return TargetElement;
        }


	    /// <summary>
	    ///     If true, this tooltip is shown without delay when hovered
	    /// </summary>
	    /// <param name="instant">Instant.</param>
	    public Tooltip SetInstant(bool instant)
        {
            _instant = instant;
            return this;
        }


        public bool GetInstant()
        {
            return _instant;
        }


	    /// <summary>
	    ///     If true, this tooltip is shown even when tooltips are not TooltipManager#enabled
	    /// </summary>
	    /// <param name="always">Always.</param>
	    public Tooltip SetAlways(bool always)
        {
            _always = always;
            return this;
        }


        public bool GetAlways()
        {
            return _always;
        }

        #endregion
    }
}