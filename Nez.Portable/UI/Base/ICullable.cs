using Microsoft.Xna.Framework;

namespace Nez.UI.Base
{
    public interface ICullable
    {
        void SetCullingArea(Rectangle cullingArea);
    }
}