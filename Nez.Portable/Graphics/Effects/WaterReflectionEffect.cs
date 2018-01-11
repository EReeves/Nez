using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.Maths;
using Nez.Utils;

namespace Nez.Graphics.Effects
{
    public class WaterReflectionEffect : ReflectionEffect
    {
	    /// <summary>
	    ///     defaults to 0.015. Waves are calculated by sampling the normal map twice. Any values generated that are
	    ///     sparkleIntensity greater
	    ///     than the actual uv value at the place of sampling will be colored sparkleColor.
	    /// </summary>
	    /// <value>The sparkle intensity.</value>
	    public float SparkleIntensity
        {
            set => _sparkleIntensityParam.SetValue(value);
        }

	    /// <summary>
	    ///     the color for the sparkly wave peaks
	    /// </summary>
	    /// <value>The color of the sparkle.</value>
	    public Vector3 SparkleColor
        {
            set => _sparkleColorParam.SetValue(value);
        }

	    /// <summary>
	    ///     position in screen space of the top of the water plane
	    /// </summary>
	    /// <value>The screen space vertical offset.</value>
	    public float ScreenSpaceVerticalOffset
        {
            set => _screenSpaceVerticalOffsetParam.SetValue(Mathf.Map(value, 0, 1, -1, 1));
        }

	    /// <summary>
	    ///     defaults to 0.3. intensity of the perspective correction
	    /// </summary>
	    /// <value>The perspective correction intensity.</value>
	    public float PerspectiveCorrectionIntensity
        {
            set => _perspectiveCorrectionIntensityParam.SetValue(value);
        }

	    /// <summary>
	    ///     defaults to 2. speed that the first displacment/normal uv is scrolled
	    /// </summary>
	    /// <value>The first displacement speed.</value>
	    public float FirstDisplacementSpeed
        {
            set => _firstDisplacementSpeedParam.SetValue(value / 100);
        }

	    /// <summary>
	    ///     defaults to 6. speed that the second displacment/normal uv is scrolled
	    /// </summary>
	    /// <value>The second displacement speed.</value>
	    public float SecondDisplacementSpeed
        {
            set => _secondDisplacementSpeedParam.SetValue(value / 100);
        }

	    /// <summary>
	    ///     defaults to 3. the normal map is sampled twice then combined. The 2nd sampling is scaled by this value.
	    /// </summary>
	    /// <value>The second displacement scale.</value>
	    public float SecondDisplacementScale
        {
            set => _secondDisplacementScaleParam.SetValue(value);
        }

        private const float _sparkleIntensity = 0.015f;
        private const float _perspectiveCorrectionIntensity = 0.3f;
        private const float _reflectionIntensity = 0.85f;
        private const float _NormalMagnitude = 0.03f;
        private const float _firstDisplacementSpeed = 6f;
        private const float _secondDisplacementSpeed = 2f;
        private const float _secondDisplacementScale = 3f;

        private readonly EffectParameter _timeParam;
        private readonly EffectParameter _sparkleIntensityParam;
        private readonly EffectParameter _sparkleColorParam;
        private readonly EffectParameter _screenSpaceVerticalOffsetParam;
        private readonly EffectParameter _perspectiveCorrectionIntensityParam;
        private readonly EffectParameter _firstDisplacementSpeedParam;
        private readonly EffectParameter _secondDisplacementSpeedParam;
        private readonly EffectParameter _secondDisplacementScaleParam;


        public WaterReflectionEffect()
        {
            CurrentTechnique = Techniques["WaterReflectionTechnique"];

            _timeParam = Parameters["_time"];
            _sparkleIntensityParam = Parameters["_sparkleIntensity"];
            _sparkleColorParam = Parameters["_sparkleColor"];
            _screenSpaceVerticalOffsetParam = Parameters["_screenSpaceVerticalOffset"];
            _perspectiveCorrectionIntensityParam = Parameters["_perspectiveCorrectionIntensity"];
            _firstDisplacementSpeedParam = Parameters["_firstDisplacementSpeed"];
            _secondDisplacementSpeedParam = Parameters["_secondDisplacementSpeed"];
            _secondDisplacementScaleParam = Parameters["_secondDisplacementScale"];

            _sparkleIntensityParam.SetValue((float) _sparkleIntensity);
            _sparkleColorParam.SetValue(Vector3.One);
            _perspectiveCorrectionIntensityParam.SetValue((float) _perspectiveCorrectionIntensity);
            FirstDisplacementSpeed = _firstDisplacementSpeed;
            SecondDisplacementSpeed = _secondDisplacementSpeed;
            _secondDisplacementScaleParam.SetValue((float) _secondDisplacementScale);

            // override some defaults from the ReflectionEffect
            ReflectionIntensity = _reflectionIntensity;
            NormalMagnitude = _normalMagnitude;
        }


#if !FNA
        protected override void OnApply()
        {
            _timeParam.SetValue(Time.TotalTime);
        }
#else
		protected override void OnApply()
		{
			_timeParam.SetValue( Time.time );
		}
		#endif
    }
}