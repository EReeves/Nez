using Microsoft.Xna.Framework.Graphics;
using Nez.Graphics.Effects;

namespace Nez.Graphics.PostProcessing.PostProcessors
{
    public class VignettePostProcessor : PostProcessor
    {
        private float _power = 1f;
        private EffectParameter _powerParam;
        private float _radius = 1.25f;
        private EffectParameter _radiusParam;


        public VignettePostProcessor(int executionOrder) : base(executionOrder)
        {
        }

        public float Power
        {
            get => _power;
            set
            {
                if (_power != value)
                {
                    _power = value;

                    if (Effect != null)
                        _powerParam.SetValue(_power);
                }
            }
        }

        public float Radius
        {
            get => _radius;
            set
            {
                if (_radius != value)
                {
                    _radius = value;

                    if (Effect != null)
                        _radiusParam.SetValue(_radius);
                }
            }
        }


        public override void OnAddedToScene()
        {
            Effect = Scene.Content.LoadEffect<Effect>("vignette", EffectResource.VignetteBytes);

            _powerParam = Effect.Parameters["_power"];
            _radiusParam = Effect.Parameters["_radius"];
            _powerParam.SetValue(_power);
            _radiusParam.SetValue(_radius);
        }
    }
}