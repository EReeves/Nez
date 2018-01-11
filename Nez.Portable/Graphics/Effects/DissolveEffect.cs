using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Nez.Graphics.Effects
{
    public class DissolveEffect : Effect
    {
        private readonly EffectParameter _dissolveTexParam;
        private float _dissolveThreshold = 0.1f;
        private Color _dissolveThresholdColor = Color.Red;
        private readonly EffectParameter _dissolveThresholdColorParam;
        private readonly EffectParameter _dissolveThresholdParam;

        private float _progress;


        private readonly EffectParameter _progressParam;


        public DissolveEffect(Texture2D dissolveTexture) : base(Core.GraphicsDevice, EffectResource.DissolveBytes)
        {
            _progressParam = Parameters["_progress"];
            _dissolveThresholdParam = Parameters["_dissolveThreshold"];
            _dissolveThresholdColorParam = Parameters["_dissolveThresholdColor"];
            _dissolveTexParam = Parameters["_dissolveTex"];

            _progressParam.SetValue(_progress);
            _dissolveThresholdParam.SetValue(_dissolveThreshold);
            _dissolveThresholdColorParam.SetValue(_dissolveThresholdColor.ToVector4());
            _dissolveTexParam.SetValue(dissolveTexture);
        }

	    /// <summary>
	    ///     value from 0 - 1 that determines how much the dissolve effect will encompass
	    /// </summary>
	    /// <value>The progress.</value>
	    public float Progress
        {
            get => _progress;
            set
            {
                _progressParam.SetValue(value);
                _progress = value;
            }
        }

	    /// <summary>
	    ///     determines how much area around the current dissolve threshold will be colored with dissolveThresholdColor
	    /// </summary>
	    /// <value>The dissolve threshold.</value>
	    public float DissolveThreshold
        {
            get => _dissolveThreshold;
            set
            {
                _dissolveThresholdParam.SetValue(value);
                _dissolveThreshold = value;
            }
        }

	    /// <summary>
	    ///     the Color that will appear on the threshold of the dissolve effect
	    /// </summary>
	    /// <value>The color of the dissolve threshold.</value>
	    public Color DissolveThresholdColor
        {
            get => _dissolveThresholdColor;
            set
            {
                _dissolveThresholdColorParam.SetValue(value.ToVector4());
                _dissolveThresholdColor = value;
            }
        }

	    /// <summary>
	    ///     the grayscale texture used to determine what is disolved
	    /// </summary>
	    /// <value>The dissolve texture.</value>
	    public Texture2D DissolveTexture
        {
            set => _dissolveTexParam.SetValue(value);
        }
    }
}