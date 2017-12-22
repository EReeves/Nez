﻿using Microsoft.Xna.Framework;

namespace Nez.Tweens
{
    public class RenderableColorTween : ColorTween, ITweenTarget<Color>
    {
        private RenderableComponent _renderable;


        public void SetTweenedValue(Color value)
        {
            _renderable.Color = value;
        }


        public Color GetTweenedValue()
        {
            return _renderable.Color;
        }


        public new object GetTargetObject()
        {
            return _renderable;
        }


        protected override void UpdateValue()
        {
            SetTweenedValue(Lerps.Ease(EaseType, FromValue, ToValue, ElapsedTime, Duration));
        }


        public void SetTarget(RenderableComponent renderable)
        {
            _renderable = renderable;
        }
    }
}