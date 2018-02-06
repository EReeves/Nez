using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.Graphics;
using Nez.Graphics.Textures;
using Nez.Maths;

namespace Nez.Utils.Extensions
{
    public static class TextureExt
    {
        /// <summary>
        /// Returns the local middle point for the texture. width/2, height/2
        /// </summary>
        /// <param name="tex"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 MiddlePoint(this Texture2D tex)
        {
            return new Vector2(tex.Width / 2f, tex.Height / 2f);
        }
        
    }
}