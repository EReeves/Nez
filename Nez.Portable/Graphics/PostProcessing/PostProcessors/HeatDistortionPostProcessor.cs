using Microsoft.Xna.Framework.Graphics;
using Nez.Graphics.Effects;
using Nez.Utils;

namespace Nez.Graphics.PostProcessing.PostProcessors
{
    public class HeatDistortionPostProcessor : PostProcessor
    {
        private float _distortionFactor = 0.005f;
        private EffectParameter _distortionFactorParam;
        private float _riseFactor = 0.15f;
        private EffectParameter _riseFactorParam;
        private EffectParameter _timeParam;


        public HeatDistortionPostProcessor(int executionOrder) : base(executionOrder)
        {
        }

        public float DistortionFactor
        {
            get => _distortionFactor;
            set
            {
                if (_distortionFactor != value)
                {
                    _distortionFactor = value;

                    if (Effect != null)
                        _distortionFactorParam.SetValue(_distortionFactor);
                }
            }
        }

        public float RiseFactor
        {
            get => _riseFactor;
            set
            {
                if (_riseFactor != value)
                {
                    _riseFactor = value;

                    if (Effect != null)
                        _riseFactorParam.SetValue(_riseFactor);
                }
            }
        }

        public Texture2D DistortionTexture
        {
            set => Effect.Parameters["_distortionTexture"].SetValue(value);
        }


        public override void OnAddedToScene()
        {
            Effect = Scene.Content.LoadEffect<Effect>("heatDistortion", EffectResource.HeatDistortionBytes);

            _timeParam = Effect.Parameters["_time"];
            _distortionFactorParam = Effect.Parameters["_distortionFactor"];
            _riseFactorParam = Effect.Parameters["_riseFactor"];

            _distortionFactorParam.SetValue(_distortionFactor);
            _riseFactorParam.SetValue(_riseFactor);

            DistortionTexture = Scene.Content.Load<Texture2D>("nez/textures/heatDistortionNoise");
        }


        public override void Process(RenderTarget2D source, RenderTarget2D destination)
        {
            _timeParam.SetValue(Time.TotalTime);
            base.Process(source, destination);
        }
    }
}