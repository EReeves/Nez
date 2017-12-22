using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.Textures;

namespace Nez.Sprites
{
	/// <summary>
	///     renders and fades a series of copies of the Sprite on the same Entity. minDistanceBetweenInstances determines how
	///     often a trail
	///     sprite is added.
	/// </summary>
	public class SpriteTrail : RenderableComponent, IUpdatable
    {
        private readonly Stack<SpriteTrailInstance> _availableSpriteTrailInstances = new Stack<SpriteTrailInstance>();

	    /// <summary>
	    ///     if awaitingDisable all instances are allowed to fade out before the component is disabled
	    /// </summary>
	    private bool _awaitingDisable;

	    /// <summary>
	    ///     flag when true it will always add a new instance regardless of the distance check
	    /// </summary>
	    private bool _isFirstInstance;

        private Vector2 _lastPosition;
        private readonly List<SpriteTrailInstance> _liveSpriteTrailInstances = new List<SpriteTrailInstance>(5);

        private int _maxSpriteInstances = 15;
        private Sprite _sprite;

	    /// <summary>
	    ///     delay before starting the color fade
	    /// </summary>
	    public float FadeDelay = 0.1f;

	    /// <summary>
	    ///     total duration of the fade from initialColor to fadeToColor
	    /// </summary>
	    public float FadeDuration = 0.8f;

	    /// <summary>
	    ///     final color that will be lerped to over the course of fadeDuration
	    /// </summary>
	    public Color FadeToColor = Color.Transparent;

	    /// <summary>
	    ///     initial color of the trail instances
	    /// </summary>
	    public Color InitialColor = Color.White;

	    /// <summary>
	    ///     how far does the Sprite have to move before a new instance is spawned
	    /// </summary>
	    public float MinDistanceBetweenInstances = 30f;


        public SpriteTrail()
        {
        }


        public SpriteTrail(Sprite sprite)
        {
            _sprite = sprite;
        }

        protected RectangleF bounds;

        public override RectangleF Bounds => bounds;

        public int MaxSpriteInstances
        {
            get => _maxSpriteInstances;
            set => SetMaxSpriteInstances(value);
        }


        void IUpdatable.Update()
        {
            if (_isFirstInstance)
            {
                _isFirstInstance = false;
                SpawnInstance();
            }
            else
            {
                var distanceMoved = Math.Abs(Vector2.Distance(Entity.Transform.Position + localOffset, _lastPosition));
                if (distanceMoved >= MinDistanceBetweenInstances)
                    SpawnInstance();
            }

            var min = new Vector2(float.MaxValue, float.MaxValue);
            var max = new Vector2(float.MinValue, float.MinValue);

            // update any live instances
            for (var i = _liveSpriteTrailInstances.Count - 1; i >= 0; i--)
                if (_liveSpriteTrailInstances[i].Update())
                {
                    _availableSpriteTrailInstances.Push(_liveSpriteTrailInstances[i]);
                    _liveSpriteTrailInstances.RemoveAt(i);
                }
                else
                {
                    // calculate our min/max for the bounds
                    Vector2.Min(ref min, ref _liveSpriteTrailInstances[i].Position, out min);
                    Vector2.Max(ref max, ref _liveSpriteTrailInstances[i].Position, out max);
                }

            bounds.Location = min;
            bounds.Width = max.X - min.X;
            bounds.Height = max.Y - min.Y;
            bounds.Inflate(_sprite.Width, _sprite.Height);

            // nothing left to render. disable ourself
            if (_awaitingDisable && _liveSpriteTrailInstances.Count == 0)
                Enabled = false;
        }


	    /// <summary>
	    ///     enables the SpriteTrail
	    /// </summary>
	    /// <returns>The sprite trail.</returns>
	    public SpriteTrail EnableSpriteTrail()
        {
            _awaitingDisable = false;
            _isFirstInstance = true;
            Enabled = true;
            return this;
        }


	    /// <summary>
	    ///     disables the SpriteTrail optionally waiting for the current trail to fade out first
	    /// </summary>
	    /// <param name="completeCurrentTrail">If set to <c>true</c> complete current trail.</param>
	    public void DisableSpriteTrail(bool completeCurrentTrail = true)
        {
            if (completeCurrentTrail)
            {
                _awaitingDisable = true;
            }
            else
            {
                Enabled = false;

                for (var i = 0; i < _liveSpriteTrailInstances.Count; i++)
                    _availableSpriteTrailInstances.Push(_liveSpriteTrailInstances[i]);
                _liveSpriteTrailInstances.Clear();
            }
        }


        public override void OnAddedToEntity()
        {
            if (_sprite == null)
                _sprite = this.GetComponent<Sprite>();

            Assert.IsNotNull(_sprite, "There is no Sprite on this Entity for the SpriteTrail to use");

            // move the trail behind the Sprite
            layerDepth = _sprite.LayerDepth + 0.001f;

            // if setMaxSpriteInstances is called it will handle initializing the SpriteTrailInstances so make sure we dont do it twice
            if (_availableSpriteTrailInstances.Count == 0)
                for (var i = 0; i < _maxSpriteInstances; i++)
                    _availableSpriteTrailInstances.Push(new SpriteTrailInstance());
        }


	    /// <summary>
	    ///     stores the last position for distance calculations and spawns a new trail instance if there is one available in the
	    ///     stack
	    /// </summary>
	    private void SpawnInstance()
        {
            _lastPosition = _sprite.Entity.Transform.Position + _sprite.LocalOffset;

            if (_awaitingDisable || _availableSpriteTrailInstances.Count == 0)
                return;

            var instance = _availableSpriteTrailInstances.Pop();
            instance.Spawn(_lastPosition, _sprite.Subtexture, FadeDuration, FadeDelay, InitialColor, FadeToColor);
            instance.SetSpriteRenderOptions(_sprite.Entity.Transform.Rotation, _sprite.Origin,
                _sprite.Entity.Transform.Scale, _sprite.SpriteEffects, layerDepth);
            _liveSpriteTrailInstances.Add(instance);
        }


        public override void Render(Graphics graphics, Camera camera)
        {
            for (var i = 0; i < _liveSpriteTrailInstances.Count; i++)
                _liveSpriteTrailInstances[i].Render(graphics, camera);
        }

	    /// <summary>
	    ///     helper class that houses the data required for the individual trail instances
	    /// </summary>
	    private class SpriteTrailInstance
        {
            private float _elapsedTime;
            private float _fadeDelay;
            private float _fadeDuration;
            private Color _initialColor;
            private float _layerDepth;
            private Vector2 _origin;
            private Color _renderColor;

            private float _rotation;
            private Vector2 _scale;
            private SpriteEffects _spriteEffects;
            private Subtexture _subtexture;
            private Color _targetColor;
            public Vector2 Position;


            public void Spawn(Vector2 position, Subtexture subtexture, float fadeDuration, float fadeDelay,
                Color initialColor, Color targetColor)
            {
                this.Position = position;
                _subtexture = subtexture;

                _initialColor = initialColor;
                _elapsedTime = 0f;

                _fadeDuration = fadeDuration;
                _fadeDelay = fadeDelay;
                _initialColor = initialColor;
                _targetColor = targetColor;
            }


            public void SetSpriteRenderOptions(float rotation, Vector2 origin, Vector2 scale,
                SpriteEffects spriteEffects, float layerDepth)
            {
                _rotation = rotation;
                _origin = origin;
                _scale = scale;
                _spriteEffects = spriteEffects;
                _layerDepth = layerDepth;
            }


	        /// <summary>
	        ///     returns true when the fade out is complete
	        /// </summary>
	        [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Update()
            {
                _elapsedTime += Time.DeltaTime;
                // fading block
                if (_elapsedTime > _fadeDelay && _elapsedTime < _fadeDuration + _fadeDelay)
                {
                    var t = Mathf.Map01(_elapsedTime, 0f, _fadeDelay + _fadeDuration);
                    ColorExt.Lerp(ref _initialColor, ref _targetColor, out _renderColor, t);
                }
                else if (_elapsedTime >= _fadeDuration + _fadeDelay)
                {
                    return true;
                }

                return false;
            }


            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Render(Graphics graphics, Camera camera)
            {
                graphics.Batcher.Draw(_subtexture, Position, _renderColor, _rotation, _origin, _scale, _spriteEffects,
                    _layerDepth);
            }
        }


        #region Fluent setters

        public SpriteTrail SetMaxSpriteInstances(int maxSpriteInstances)
        {
            // if our new value is greater than our previous count instantiate the required SpriteTrailInstances
            if (_availableSpriteTrailInstances.Count < maxSpriteInstances)
            {
                var newInstances = _availableSpriteTrailInstances.Count - maxSpriteInstances;
                for (var i = 0; i < newInstances; i++)
                    _availableSpriteTrailInstances.Push(new SpriteTrailInstance());
            }

            // if our new value is less than our previous count trim the List
            if (_availableSpriteTrailInstances.Count > maxSpriteInstances)
            {
                var excessInstances = maxSpriteInstances - _availableSpriteTrailInstances.Count;
                for (var i = 0; i < excessInstances; i++)
                    _availableSpriteTrailInstances.Pop();
            }

            _maxSpriteInstances = maxSpriteInstances;


            return this;
        }


        public SpriteTrail SetMinDistanceBetweenInstances(float minDistanceBetweenInstances)
        {
            this.MinDistanceBetweenInstances = minDistanceBetweenInstances;
            return this;
        }


        public SpriteTrail SetFadeDuration(float fadeDuration)
        {
            this.FadeDuration = fadeDuration;
            return this;
        }


        public SpriteTrail SetFadeDelay(float fadeDelay)
        {
            this.FadeDelay = fadeDelay;
            return this;
        }


        public SpriteTrail SetInitialColor(Color initialColor)
        {
            this.InitialColor = initialColor;
            return this;
        }


        public SpriteTrail SetFadeToColor(Color fadeToColor)
        {
            this.FadeToColor = fadeToColor;
            return this;
        }

        #endregion
    }
}