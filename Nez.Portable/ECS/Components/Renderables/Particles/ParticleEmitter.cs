using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.Graphics;
using Nez.Maths;
using Nez.Utils;

namespace Nez.ECS.Components.Renderables.Particles
{
    public class ParticleEmitter : RenderableComponent, IUpdatable
    {
        private bool _active;

	    /// <summary>
	    ///     tracks the elapsed time of the emitter
	    /// </summary>
	    private float _elapsedTime;

	    /// <summary>
	    ///     keeps track of how many particles should be emitted
	    /// </summary>
	    private float _emitCounter;

        private readonly ParticleEmitterConfig _emitterConfig;

	    /// <summary>
	    ///     if the emitter is emitting this will be true. Note that emitting can be false while particles are still alive.
	    ///     emitting gets set
	    ///     to false and then any live particles are allowed to finish their lifecycle.
	    /// </summary>
	    private bool _emitting;

        private readonly List<Particle> _particles;
        private readonly bool _playOnAwake;

	    /// <summary>
	    ///     config object with various properties to deal with particle collisions
	    /// </summary>
	    public ParticleCollisionConfig CollisionConfig;


        public ParticleEmitter(ParticleEmitterConfig emitterConfig, bool playOnAwake = true)
        {
            _emitterConfig = emitterConfig;
            _playOnAwake = playOnAwake;
            _particles = new List<Particle>((int) _emitterConfig.MaxParticles);
            Pool<Particle>.WarmCache((int) _emitterConfig.MaxParticles);

            // set some sensible defaults
            CollisionConfig.Elasticity = 0.5f;
            CollisionConfig.Friction = 0.5f;
            CollisionConfig.CollidesWithLayers = Nez.Physics.Physics.AllLayers;
            CollisionConfig.Gravity = _emitterConfig.Gravity;
            CollisionConfig.LifetimeLoss = 0f;
            CollisionConfig.MinKillSpeedSquared = float.MinValue;
            CollisionConfig.RadiusScale = 0.8f;

            Init();
        }

        public override RectangleF Bounds => base._bounds;

        public bool IsPaused { get; private set; }

        public bool IsPlaying => _active && !IsPaused;
        public bool IsStopped => !_active && !IsPaused;
        public float ElapsedTime => _elapsedTime;

	    /// <summary>
	    ///     convenience method for setting ParticleEmitterConfig.simulateInWorldSpace. If true, particles will simulate in
	    ///     world space. ie when the
	    ///     parent Transform moves it will have no effect on any already active Particles.
	    /// </summary>
	    public bool SimulateInWorldSpace
        {
            set => _emitterConfig.SimulateInWorldSpace = value;
        }


	    /// <summary>
	    ///     creates the Batcher and loads the texture if it is available
	    /// </summary>
	    private void Init()
        {
            // prep our custom BlendState and set the Material with it
            var blendState = new BlendState();
            blendState.ColorSourceBlend = blendState.AlphaSourceBlend = _emitterConfig.BlendFuncSource;
            blendState.ColorDestinationBlend = blendState.AlphaDestinationBlend = _emitterConfig.BlendFuncDestination;

            Material = new Material(blendState);
        }


	    /// <summary>
	    ///     removes all particles from the particle emitter
	    /// </summary>
	    public void Clear()
        {
            for (var i = 0; i < _particles.Count; i++)
                Pool<Particle>.Free(_particles[i]);
            _particles.Clear();
        }


	    /// <summary>
	    ///     plays the particle emitter
	    /// </summary>
	    public void Play()
        {
            // if we are just unpausing, we only toggle flags and we dont mess with any other parameters
            if (IsPaused)
            {
                _active = true;
                IsPaused = false;
                return;
            }

            _active = true;
            _emitting = true;
            _elapsedTime = 0;
            _emitCounter = 0;
        }


	    /// <summary>
	    ///     stops the particle emitter
	    /// </summary>
	    public void Stop()
        {
            _active = false;
            IsPaused = false;
            _elapsedTime = 0;
            _emitCounter = 0;
            Clear();
        }


	    /// <summary>
	    ///     pauses the particle emitter
	    /// </summary>
	    public void Pause()
        {
            IsPaused = true;
            _active = false;
        }


	    /// <summary>
	    ///     manually emit some particles
	    /// </summary>
	    /// <param name="count">Count.</param>
	    public void Emit(int count)
        {
            var rootPosition = Entity.Transform.Position + localOffset;

            Init();
            _active = true;
            for (var i = 0; i < count; i++)
                AddParticle(rootPosition);
        }


	    /// <summary>
	    ///     adds a Particle to the emitter
	    /// </summary>
	    private void AddParticle(Vector2 position)
        {
            // take the next particle out of the particle pool we have created and initialize it
            var particle = Pool<Particle>.Obtain();
            particle.Initialize(_emitterConfig, position);
            _particles.Add(particle);
        }


        #region Component/RenderableComponent

        public override void OnAddedToEntity()
        {
            if (_playOnAwake)
                Play();
        }


        void IUpdatable.Update()
        {
            if (IsPaused)
                return;

            // prep data for the particle.update method
            var rootPosition = Entity.Transform.Position + localOffset;

            // if the emitter is active and the emission rate is greater than zero then emit particles
            if (_active && _emitterConfig.EmissionRate > 0)
            {
                var rate = 1.0f / _emitterConfig.EmissionRate;

                if (_particles.Count < _emitterConfig.MaxParticles)
                    _emitCounter += Time.DeltaTime;

                while (_emitting && _particles.Count < _emitterConfig.MaxParticles && _emitCounter > rate)
                {
                    AddParticle(rootPosition);
                    _emitCounter -= rate;
                }

                _elapsedTime += Time.DeltaTime;

                if (_emitterConfig.Duration != -1 && _emitterConfig.Duration < _elapsedTime)
                {
                    // when we hit our duration we dont emit any more particles
                    _emitting = false;

                    // once all our particles are done we stop the emitter
                    if (_particles.Count == 0)
                        Stop();
                }
            }

            var min = new Vector2(float.MaxValue, float.MaxValue);
            var max = new Vector2(float.MinValue, float.MinValue);
            var maxParticleSize = float.MinValue;

            // loop through all the particles updating their location and color
            for (var i = _particles.Count - 1; i >= 0; i--)
            {
                // get the current particle and update it
                var currentParticle = _particles[i];

                // if update returns true that means the particle is done
                if (currentParticle.Update(_emitterConfig, ref CollisionConfig, rootPosition))
                {
                    Pool<Particle>.Free(currentParticle);
                    _particles.RemoveAt(i);
                }
                else
                {
                    // particle is good. collect min/max positions for the bounds
                    var pos = _emitterConfig.SimulateInWorldSpace ? currentParticle.SpawnPosition : rootPosition;
                    pos += currentParticle.Position;
                    Vector2.Min(ref min, ref pos, out min);
                    Vector2.Max(ref max, ref pos, out max);
                    maxParticleSize = System.Math.Max(maxParticleSize, currentParticle.ParticleSize);
                }
            }

            _bounds.Location = min;
            _bounds.Width = max.X - min.X;
            _bounds.Height = max.Y - min.Y;

            if (_emitterConfig.Subtexture == null)
            {
                Bounds.Inflate(1 * maxParticleSize, 1 * maxParticleSize);
            }
            else
            {
                maxParticleSize /= _emitterConfig.Subtexture.SourceRect.Width;
                Bounds.Inflate(_emitterConfig.Subtexture.SourceRect.Width * maxParticleSize,
                    _emitterConfig.Subtexture.SourceRect.Height * maxParticleSize);
            }
        }


        public override void Render(Graphics.Graphics graphics, Camera camera)
        {
            // we still render when we are paused
            if (!_active && !IsPaused)
                return;

            var rootPosition = Entity.Transform.Position + localOffset;

            // loop through all the particles updating their location and color
            for (var i = 0; i < _particles.Count; i++)
            {
                var currentParticle = _particles[i];
                var pos = _emitterConfig.SimulateInWorldSpace ? currentParticle.SpawnPosition : rootPosition;

                if (_emitterConfig.Subtexture == null)
                    graphics.Batcher.Draw(graphics.PixelTexture, pos + currentParticle.Position, currentParticle.Color,
                        currentParticle.Rotation, Vector2.One, currentParticle.ParticleSize * 0.5f, SpriteEffects.None,
                        layerDepth);
                else
                    graphics.Batcher.Draw(_emitterConfig.Subtexture, pos + currentParticle.Position,
                        currentParticle.Color, currentParticle.Rotation, _emitterConfig.Subtexture.Center,
                        currentParticle.ParticleSize / _emitterConfig.Subtexture.SourceRect.Width, SpriteEffects.None,
                        layerDepth);
            }
        }

        #endregion
    }
}