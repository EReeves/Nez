using Nez.ECS;
using Nez.Graphics.Effects;

namespace Nez.Graphics.Materials
{
    public class PaletteCyclerMaterial : Material<PaletteCyclerEffect>
    {
        public PaletteCyclerMaterial()
        {
            Effect = new PaletteCyclerEffect();
        }


        public override void OnPreRender(Camera camera)
        {
            Effect.UpdateTime();
        }
    }
}