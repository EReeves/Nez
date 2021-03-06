﻿using Microsoft.Xna.Framework.Graphics;
using Nez.Debug.Inspector.Attributes;

namespace Nez.Graphics.Effects
{
    public class ScanlinesEffect : Effect
    {
        private float _attenuation = 0.04f;

        private readonly EffectParameter _attenuationParam;
        private float _linesFactor = 800f;
        private readonly EffectParameter _linesFactorParam;


        public ScanlinesEffect() : base(Core.GraphicsDevice, EffectResource.ScanlinesBytes)
        {
            _attenuationParam = Parameters["_attenuation"];
            _linesFactorParam = Parameters["_linesFactor"];

            _attenuationParam.SetValue(_attenuation);
            _linesFactorParam.SetValue(_linesFactor);
        }

        [Range(0.001f, 1f, 0.001f)]
        public float Attenuation
        {
            get => _attenuation;
            set
            {
                if (_attenuation != value)
                {
                    _attenuation = value;
                    _attenuationParam.SetValue(_attenuation);
                }
            }
        }

        [Range(10, 1000, 1)]
        public float LinesFactor
        {
            get => _linesFactor;
            set
            {
                if (_linesFactor != value)
                {
                    _linesFactor = value;
                    _linesFactorParam.SetValue(_linesFactor);
                }
            }
        }
    }
}