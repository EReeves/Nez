using Microsoft.Xna.Framework.Graphics;

namespace Nez.Graphics.Effects
{
    public class DotsEffect : Effect
    {
        private float _angle = 0.5f;
        private readonly EffectParameter _angleParam;

        private float _scale = 0.5f;

        private readonly EffectParameter _scaleParam;


        public DotsEffect() : base(Core.GraphicsDevice, EffectResource.DotsBytes)
        {
            _scaleParam = Parameters["scale"];
            _angleParam = Parameters["angle"];

            _scaleParam.SetValue(_scale);
            _angleParam.SetValue(_angle);
        }

        public float Scale
        {
            get => _scale;
            set
            {
                if (_scale != value)
                {
                    _scale = value;
                    _scaleParam.SetValue(_scale);
                }
            }
        }

        public float Angle
        {
            get => _angle;
            set
            {
                if (_angle != value)
                {
                    _angle = value;
                    _angleParam.SetValue(_angle);
                }
            }
        }
    }
}