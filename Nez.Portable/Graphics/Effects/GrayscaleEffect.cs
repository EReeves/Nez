using Microsoft.Xna.Framework.Graphics;

namespace Nez.Graphics.Effects
{
    public class GrayscaleEffect : Effect
    {
        public GrayscaleEffect() : base(Core.GraphicsDevice, EffectResource.GrayscaleBytes)
        {
        }
    }
}