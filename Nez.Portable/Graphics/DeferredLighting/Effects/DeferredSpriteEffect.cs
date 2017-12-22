using Microsoft.Xna.Framework.Graphics;

namespace Nez.DeferredLighting
{
	/// <summary>
	///     effect used to render sprites that take part in deferred lighting. A normal map is required. The normal map can
	///     optionally use the alpha
	///     channel for self illumination by setitng useNormalAlphaChannelForSelfIllumination to true. Note that you need to
	///     turn off premultiplied
	///     alpha in the Pipeline tool when using the alpha for self illumination!
	/// </summary>
	public class DeferredSpriteEffect : Effect
    {
        private readonly EffectParameter _alphaAsSelfIlluminationParam;


        private readonly EffectParameter _alphaCutoffParam;

        private readonly EffectParameter _normalMapParam;
        private readonly EffectParameter _selfIlluminationPowerParam;


        public DeferredSpriteEffect() : base(Core.GraphicsDevice, EffectResource.DeferredSpriteBytes)
        {
            _normalMapParam = Parameters["_normalMap"];
            _alphaCutoffParam = Parameters["_alphaCutoff"];
            _alphaAsSelfIlluminationParam = Parameters["_alphaAsSelfIllumination"];
            _selfIlluminationPowerParam = Parameters["_selfIlluminationPower"];

            SetAlphaCutoff(0.3f);
            SetSelfIlluminationPower(1);
        }

	    /// <summary>
	    ///     alpha cutoff for the alpha test. defaults to 0.3
	    /// </summary>
	    /// <value>The alpha cutoff.</value>
	    public float AlphaCutoff { get; private set; }

	    /// <summary>
	    ///     if true, the normal map alpha channel will be used for self illumination. Note that you need to turn off
	    ///     premultiplied
	    ///     alpha in the Pipeline tool when using the alpha for self illumination!
	    /// </summary>
	    /// <value><c>true</c> if use normal alpha channel for self illumination; otherwise, <c>false</c>.</value>
	    public bool UseNormalAlphaChannelForSelfIllumination { get; private set; }

	    /// <summary>
	    ///     controls the power of the self illumination where 0 is no contribution and 1 is fully self illuminated
	    /// </summary>
	    /// <value>The self illumination power parameter.</value>
	    public float SelfIlluminationPower { get; private set; }


        #region Configuration

        public DeferredSpriteEffect SetNormalMap(Texture2D normalMap)
        {
            _normalMapParam.SetValue(normalMap);
            return this;
        }


	    /// <summary>
	    ///     alpha cutoff for the alpha test. defaults to 0.3
	    /// </summary>
	    /// <returns>The alpha cutoff.</returns>
	    /// <param name="alphaCutoff">Alpha cutoff.</param>
	    public DeferredSpriteEffect SetAlphaCutoff(float alphaCutoff)
        {
            if (this.AlphaCutoff != alphaCutoff)
            {
                this.AlphaCutoff = alphaCutoff;
                _alphaCutoffParam.SetValue(alphaCutoff);
            }
            return this;
        }


	    /// <summary>
	    ///     if true, the normal map alpha channel will be used for self illumination. Note that you need to turn off
	    ///     premultiplied
	    ///     alpha in the Pipeline tool when using the alpha for self illumination!
	    /// </summary>
	    /// <returns>The use normal alpha channel for self illumination.</returns>
	    /// <param name="useNormalAlphaChannelForSelfIllumination">
	    ///     If set to <c>true</c> use normal alpha channel for self
	    ///     illumination.
	    /// </param>
	    public DeferredSpriteEffect SetUseNormalAlphaChannelForSelfIllumination(
            bool useNormalAlphaChannelForSelfIllumination)
        {
            if (this.UseNormalAlphaChannelForSelfIllumination != useNormalAlphaChannelForSelfIllumination)
            {
                this.UseNormalAlphaChannelForSelfIllumination = useNormalAlphaChannelForSelfIllumination;
                _alphaAsSelfIlluminationParam.SetValue(useNormalAlphaChannelForSelfIllumination ? 1f : 0f);
            }
            return this;
        }


	    /// <summary>
	    ///     controls the power of the self illumination where 0 is no contribution and 1 is fully self illuminated
	    /// </summary>
	    /// <returns>The self illumination power.</returns>
	    /// <param name="selfIlluminationPower">Self illumination power.</param>
	    public DeferredSpriteEffect SetSelfIlluminationPower(float selfIlluminationPower)
        {
            if (this.SelfIlluminationPower != selfIlluminationPower)
            {
                this.SelfIlluminationPower = selfIlluminationPower;
                _selfIlluminationPowerParam.SetValue(selfIlluminationPower);
            }
            return this;
        }

        #endregion
    }
}