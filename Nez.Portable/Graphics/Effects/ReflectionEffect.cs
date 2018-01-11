using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Nez.Graphics.Effects
{
    public class ReflectionEffect : Effect
    {
        private const float _reflectionIntensity = 0.4f;
	    protected const float _normalMagnitude = 0.05f;
        private readonly EffectParameter _matrixTransformParam;
        private readonly EffectParameter _normalMagnitudeParam;
        private readonly EffectParameter _normalMapParam;

        private readonly EffectParameter _reflectionIntensityParam;
        private readonly EffectParameter _renderTextureParam;


        public ReflectionEffect() : base(Core.GraphicsDevice, EffectResource.ReflectionBytes)
        {
            _reflectionIntensityParam = Parameters["_reflectionIntensity"];
            _renderTextureParam = Parameters["_renderTexture"];
            _normalMapParam = Parameters["_normalMap"];
            _matrixTransformParam = Parameters["_matrixTransform"];
            _normalMagnitudeParam = Parameters["_normalMagnitude"];

            _reflectionIntensityParam.SetValue((float) _reflectionIntensity);
            _normalMagnitudeParam.SetValue((float) _normalMagnitude);
        }

	    /// <summary>
	    ///     0 - 1 range. Intensity of the reflection where 0 is none and 1 is full reflected
	    /// </summary>
	    /// <value>The reflection intensity.</value>
	    public float ReflectionIntensity
        {
            set => _reflectionIntensityParam.SetValue(value);
        }

	    /// <summary>
	    ///     magnitude of the normal map contribution to the UV offset of the sampled RenderTarget. Default is 0.05. Very small
	    ///     numbers work best.
	    /// </summary>
	    /// <value>The normal magnitude.</value>
	    public float NormalMagnitude
        {
            set => _normalMagnitudeParam.SetValue(value);
        }

	    /// <summary>
	    ///     optional normal map used to displace/refract the UV of the sampled RenderTarget.
	    /// </summary>
	    /// <value>The normal map.</value>
	    public Texture2D NormalMap
        {
            set => _normalMapParam.SetValue(value);
        }

	    /// <summary>
	    ///     the render textured used for the reflections
	    /// </summary>
	    /// <value>The render texture.</value>
	    internal RenderTarget2D RenderTexture
        {
            set => _renderTextureParam.SetValue(value);
        }

        internal Matrix MatrixTransform
        {
            set => _matrixTransformParam.SetValue(value);
        }
    }
}