using Microsoft.Xna.Framework;
using Nez.Utils.Tweens.Easing;
using Nez.Utils.Tweens.Interfaces;


// concrete implementations of all tweenable types
namespace Nez.Utils.Tweens
{
    public class IntTween : Tween<int>
    {
        public IntTween()
        {
        }


        public IntTween(ITweenTarget<int> target, int to, float duration)
        {
            Initialize(target, to, duration);
        }

        public static IntTween Create()
        {
            return TweenManager.CacheIntTweens ? Pool<IntTween>.Obtain() : new IntTween();
        }


        public override ITween<int> SetIsRelative()
        {
            IsRelative = true;
            ToValue += FromValue;
            return this;
        }


        protected override void UpdateValue()
        {
            Target.SetTweenedValue((int) Lerps.Ease(EaseType, FromValue, ToValue, ElapsedTime, Duration));
        }


        public override void RecycleSelf()
        {
            base.RecycleSelf();

            if (ShouldRecycleTween && TweenManager.CacheIntTweens)
                Pool<IntTween>.Free(this);
        }
    }


    public class FloatTween : Tween<float>
    {
        public FloatTween()
        {
        }


        public FloatTween(ITweenTarget<float> target, float to, float duration)
        {
            Initialize(target, to, duration);
        }

        public static FloatTween Create()
        {
            return TweenManager.CacheFloatTweens ? Pool<FloatTween>.Obtain() : new FloatTween();
        }


        public override ITween<float> SetIsRelative()
        {
            IsRelative = true;
            ToValue += FromValue;
            return this;
        }


        protected override void UpdateValue()
        {
            Target.SetTweenedValue(Lerps.Ease(EaseType, FromValue, ToValue, ElapsedTime, Duration));
        }


        public override void RecycleSelf()
        {
            base.RecycleSelf();

            if (ShouldRecycleTween && TweenManager.CacheFloatTweens)
                Pool<FloatTween>.Free(this);
        }
    }


    public class Vector2Tween : Tween<Vector2>
    {
        public Vector2Tween()
        {
        }


        public Vector2Tween(ITweenTarget<Vector2> target, Vector2 to, float duration)
        {
            Initialize(target, to, duration);
        }

        public static Vector2Tween Create()
        {
            return TweenManager.CacheVector2Tweens ? Pool<Vector2Tween>.Obtain() : new Vector2Tween();
        }


        public override ITween<Vector2> SetIsRelative()
        {
            IsRelative = true;
            ToValue += FromValue;
            return this;
        }


        protected override void UpdateValue()
        {
            Target.SetTweenedValue(Lerps.Ease(EaseType, FromValue, ToValue, ElapsedTime, Duration));
        }


        public override void RecycleSelf()
        {
            base.RecycleSelf();

            if (ShouldRecycleTween && TweenManager.CacheVector2Tweens)
                Pool<Vector2Tween>.Free(this);
        }
    }


    public class Vector3Tween : Tween<Vector3>
    {
        public Vector3Tween()
        {
        }


        public Vector3Tween(ITweenTarget<Vector3> target, Vector3 to, float duration)
        {
            Initialize(target, to, duration);
        }

        public static Vector3Tween Create()
        {
            return TweenManager.CacheVector3Tweens ? Pool<Vector3Tween>.Obtain() : new Vector3Tween();
        }


        public override ITween<Vector3> SetIsRelative()
        {
            IsRelative = true;
            ToValue += FromValue;
            return this;
        }


        protected override void UpdateValue()
        {
            Target.SetTweenedValue(Lerps.Ease(EaseType, FromValue, ToValue, ElapsedTime, Duration));
        }


        public override void RecycleSelf()
        {
            base.RecycleSelf();

            if (ShouldRecycleTween && TweenManager.CacheVector3Tweens)
                Pool<Vector3Tween>.Free(this);
        }
    }


    public class Vector4Tween : Tween<Vector4>
    {
        public Vector4Tween()
        {
        }


        public Vector4Tween(ITweenTarget<Vector4> target, Vector4 to, float duration)
        {
            Initialize(target, to, duration);
        }

        public static Vector4Tween Create()
        {
            return TweenManager.CacheVector4Tweens ? Pool<Vector4Tween>.Obtain() : new Vector4Tween();
        }


        public override ITween<Vector4> SetIsRelative()
        {
            IsRelative = true;
            ToValue += FromValue;
            return this;
        }


        protected override void UpdateValue()
        {
            Target.SetTweenedValue(Lerps.Ease(EaseType, FromValue, ToValue, ElapsedTime, Duration));
        }


        public override void RecycleSelf()
        {
            base.RecycleSelf();

            if (ShouldRecycleTween && TweenManager.CacheVector4Tweens)
                Pool<Vector4Tween>.Free(this);
        }
    }


    public class QuaternionTween : Tween<Quaternion>
    {
        public QuaternionTween()
        {
        }


        public QuaternionTween(ITweenTarget<Quaternion> target, Quaternion to, float duration)
        {
            Initialize(target, to, duration);
        }

        public static QuaternionTween Create()
        {
            return TweenManager.CacheQuaternionTweens ? Pool<QuaternionTween>.Obtain() : new QuaternionTween();
        }


        public override ITween<Quaternion> SetIsRelative()
        {
            IsRelative = true;
            ToValue *= FromValue;
            return this;
        }


        protected override void UpdateValue()
        {
            Target.SetTweenedValue(Lerps.Ease(EaseType, FromValue, ToValue, ElapsedTime, Duration));
        }


        public override void RecycleSelf()
        {
            base.RecycleSelf();

            if (ShouldRecycleTween && TweenManager.CacheQuaternionTweens)
                Pool<QuaternionTween>.Free(this);
        }
    }


    public class ColorTween : Tween<Color>
    {
        public ColorTween()
        {
        }


        public ColorTween(ITweenTarget<Color> target, Color to, float duration)
        {
            Initialize(target, to, duration);
        }

        public static ColorTween Create()
        {
            return TweenManager.CacheColorTweens ? Pool<ColorTween>.Obtain() : new ColorTween();
        }


        public override ITween<Color> SetIsRelative()
        {
            IsRelative = true;
            ToValue.R += FromValue.R;
            ToValue.G += FromValue.G;
            ToValue.B += FromValue.B;
            ToValue.A += FromValue.A;
            return this;
        }


        protected override void UpdateValue()
        {
            Target.SetTweenedValue(Lerps.Ease(EaseType, FromValue, ToValue, ElapsedTime, Duration));
        }


        public override void RecycleSelf()
        {
            base.RecycleSelf();

            if (ShouldRecycleTween && TweenManager.CacheColorTweens)
                Pool<ColorTween>.Free(this);
        }
    }


    public class RectangleTween : Tween<Rectangle>
    {
        public RectangleTween()
        {
        }


        public RectangleTween(ITweenTarget<Rectangle> target, Rectangle to, float duration)
        {
            Initialize(target, to, duration);
        }

        public static RectangleTween Create()
        {
            return TweenManager.CacheRectTweens ? Pool<RectangleTween>.Obtain() : new RectangleTween();
        }


        public override ITween<Rectangle> SetIsRelative()
        {
            IsRelative = true;
            ToValue = new Rectangle
            (
                ToValue.X + FromValue.X,
                ToValue.Y + FromValue.Y,
                ToValue.Width + FromValue.Width,
                ToValue.Height + FromValue.Height
            );

            return this;
        }


        protected override void UpdateValue()
        {
            Target.SetTweenedValue(Lerps.Ease(EaseType, FromValue, ToValue, ElapsedTime, Duration));
        }


        public override void RecycleSelf()
        {
            base.RecycleSelf();

            if (ShouldRecycleTween && TweenManager.CacheRectTweens)
                Pool<RectangleTween>.Free(this);
        }
    }
}