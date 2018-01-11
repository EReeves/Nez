﻿using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.ECS;
using Nez.Utils;
using Nez.Utils.Tweens.Easing;

namespace Nez.Graphics.Transitions
{
	/// <summary>
	///     fades from the current Scene to the new Scene
	/// </summary>
	public class CrossFadeTransition : SceneTransition
    {
        private Color _color = Color.White;

        private Color _fromColor = Color.White;
        private Color _toColor = Color.Transparent;

	    /// <summary>
	    ///     duration for the fade
	    /// </summary>
	    public float FadeDuration = 1f;

	    /// <summary>
	    ///     ease equation to use for the cross fade
	    /// </summary>
	    public EaseType FadeEaseType = EaseType.QuartIn;


        public CrossFadeTransition(Func<Scene> sceneLoadAction) : base(sceneLoadAction, true)
        {
        }


        public CrossFadeTransition() : this(null)
        {
        }


        public override IEnumerator OnBeginTransition()
        {
            yield return null;

            // load up the new Scene
            yield return Core.StartCoroutine(LoadNextScene());

            var elapsed = 0f;
            while (elapsed < FadeDuration)
            {
                elapsed += Time.DeltaTime;
                _color = Lerps.Ease(FadeEaseType, ref _fromColor, ref _toColor, elapsed, FadeDuration);

                yield return null;
            }

            TransitionComplete();
        }


        public override void Render(Graphics graphics)
        {
            GraphicsDeviceExt.SetRenderTarget(Core.GraphicsDevice, null);
            graphics.Batcher.Begin(BlendState.NonPremultiplied, Core.DefaultSamplerState, DepthStencilState.None, null);
            graphics.Batcher.Draw(PreviousSceneRender, Vector2.Zero, _color);
            graphics.Batcher.End();
        }
    }
}