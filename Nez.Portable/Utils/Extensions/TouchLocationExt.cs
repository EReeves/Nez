#if !FNA
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;

namespace Nez.Utils.Extensions
{
    public static class TouchLocationExt
    {
        public static Vector2 ScaledPosition(this TouchLocation touchLocation)
        {
            return Input.Input.ScaledPosition(touchLocation.Position);
        }
    }
}
#endif