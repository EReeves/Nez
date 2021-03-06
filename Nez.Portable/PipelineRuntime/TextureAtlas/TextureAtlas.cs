using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Nez.ECS.Components.Renderables.Sprites;
using Nez.Graphics.Textures;
using Nez.Utils.Extensions;

namespace Nez.PipelineRuntime.TextureAtlas
{
	/// <summary>
	///     runtime component of the TextureAtlasGenerator. Stores the main Texture2D and all relevant metadata
	/// </summary>
	public class TextureAtlas
    {
        private readonly int _animationFps = 15;

	    /// <summary>
	    ///     stores a map of the name of the sprite animation (derived from the folder name) to a Point. The Point x/y values
	    ///     are the
	    ///     start/end indexes of the subtextures for the animation frames.
	    /// </summary>
	    private readonly Dictionary<string, Point> _spriteAnimationDetails;

	    /// <summary>
	    ///     image names for the subtextures. maps directly to the subtextures array
	    /// </summary>
	    public readonly string[] RegionNames;

	    /// <summary>
	    ///     array of all subtextures from the atlas
	    /// </summary>
	    public readonly Subtexture[] Subtextures;

        private Dictionary<string, SpriteAnimation> _spriteAnimations;


        public TextureAtlas(string[] regionNames, Subtexture[] subtextures,
            Dictionary<string, Point> spriteAnimationDetails, int animationFps = 15)
        {
            this.RegionNames = regionNames;
            this.Subtextures = subtextures;
            _spriteAnimationDetails = spriteAnimationDetails;
            _animationFps = animationFps;
        }


        public TextureAtlas(string[] regionNames, Subtexture[] subtextures) : this(regionNames, subtextures, null)
        {
        }


        public Subtexture this[string name] => GetSubtexture(name);


	    /// <summary>
	    ///     gets the Subtexture for the passed in image name
	    /// </summary>
	    /// <returns>The subtexture.</returns>
	    /// <param name="name">Name.</param>
	    public Subtexture GetSubtexture(string name)
        {
            return Subtextures[Array.IndexOf(RegionNames, name)];
        }


	    /// <summary>
	    ///     checks whether the subtexture is contained in this atlas.
	    /// </summary>
	    /// <returns><c>true</c>, if subtexture is containsed, <c>false</c> otherwise.</returns>
	    /// <param name="name">the image name</param>
	    public bool ContainsSubtexture(string name)
        {
            return RegionNames.Contains(name);
        }


	    /// <summary>
	    ///     returns a SpriteAnimation given an animationName where the animationName is the folder that contains the images
	    /// </summary>
	    /// <returns>The sprite animation.</returns>
	    /// <param name="animationName">Animation name.</param>
	    public SpriteAnimation GetSpriteAnimation(string animationName)
        {
            // create the cache Dictionary if necessary. Return the animation direction if already cached.
            if (_spriteAnimations == null)
                _spriteAnimations = new Dictionary<string, SpriteAnimation>();
            else if (_spriteAnimations.ContainsKey(animationName))
                return _spriteAnimations[animationName];

            Point point;
            if (_spriteAnimationDetails.TryGetValue(animationName, out point))
            {
                var animation = new SpriteAnimation
                {
                    Fps = _animationFps
                };

                for (var i = point.X; i <= point.Y; i++)
                    animation.AddFrame(Subtextures[i]);

                _spriteAnimations[animationName] = animation;

                return animation;
            }

            throw new KeyNotFoundException(animationName);
        }
    }
}