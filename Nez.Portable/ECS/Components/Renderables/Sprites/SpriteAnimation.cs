﻿using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Nez.Graphics.Textures;

namespace Nez.ECS.Components.Renderables.Sprites
{
    public enum AnimationCompletionBehavior
    {
        RemainOnFinalFrame,
        RevertToFirstFrame,
        HideSprite
    }


	/// <summary>
	///     houses the information that a SpriteT requires for animation
	/// </summary>
	public class SpriteAnimation
    {
        private float _fps = 10;
        private bool _isDirty = true;
        private bool _loop = true;
        private bool _pingPong;
        public AnimationCompletionBehavior CompletionBehavior;

        public float Delay = 0f;
        public List<Subtexture> Frames = new List<Subtexture>();
        public float IterationDuration;

        // calculated values used by SpriteT
        public float SecondsPerFrame;

        public float TotalDuration;


        public SpriteAnimation()
        {
        }


        public SpriteAnimation(Subtexture frame)
        {
            AddFrame(frame);
        }


        public SpriteAnimation(List<Subtexture> frames)
        {
            AddFrames(frames);
        }

	    /// <summary>
	    ///     frames per second for the animations
	    /// </summary>
	    /// <value>The fps.</value>
	    public float Fps
        {
            get => _fps;
            set => SetFps(value);
        }

	    /// <summary>
	    ///     controls whether the animation should loop
	    /// </summary>
	    /// <value>The loop.</value>
	    public bool Loop
        {
            get => _loop;
            set => SetLoop(value);
        }

	    /// <summary>
	    ///     if loop is true, this controls if an animation loops sequentially or back and forth
	    /// </summary>
	    /// <value>The ping pong.</value>
	    public bool PingPong
        {
            get => _pingPong;
            set => SetPingPong(value);
        }


	    /// <summary>
	    ///     called by SpriteT to calculate the secondsPerFrame and totalDuration based on the loop details and frame count
	    /// </summary>
	    /// <returns>The for use.</returns>
	    public void PrepareForUse()
        {
            if (!_isDirty)
                return;

            SecondsPerFrame = 1f / Fps;
            IterationDuration = SecondsPerFrame * Frames.Count;

            if (Loop)
                TotalDuration = float.PositiveInfinity;
            else if (PingPong)
                TotalDuration = IterationDuration * 2f;
            else
                TotalDuration = IterationDuration;

            _isDirty = false;
        }


	    /// <summary>
	    ///     sets the origin for all frames in this animation
	    /// </summary>
	    /// <param name="origin"></param>
	    public SpriteAnimation SetOrigin(Vector2 origin)
        {
            for (var i = 0; i < Frames.Count; i++)
                Frames[i].Origin = origin;
            return this;
        }


        public SpriteAnimation SetFps(float fps)
        {
            _fps = fps;
            _isDirty = true;
            return this;
        }


        public SpriteAnimation SetLoop(bool loop)
        {
            _loop = loop;
            _isDirty = true;
            return this;
        }


        public SpriteAnimation SetPingPong(bool pingPong)
        {
            _pingPong = pingPong;
            _isDirty = true;
            return this;
        }


	    /// <summary>
	    ///     adds a frame to this animation
	    /// </summary>
	    /// <param name="subtexture">Subtexture.</param>
	    public SpriteAnimation AddFrame(Subtexture subtexture)
        {
            Frames.Add(subtexture);
            return this;
        }


	    /// <summary>
	    ///     adds multiple frames to this animation
	    /// </summary>
	    /// <returns>The frames.</returns>
	    /// <param name="subtextures">Subtextures.</param>
	    public SpriteAnimation AddFrames(List<Subtexture> subtextures)
        {
            for (var i = 0; i < subtextures.Count; i++)
                AddFrame(subtextures[i]);
            return this;
        }
    }
}