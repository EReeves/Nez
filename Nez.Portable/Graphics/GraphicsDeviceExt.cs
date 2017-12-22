using Microsoft.Xna.Framework.Graphics;

namespace Nez
{
    public static class GraphicsDeviceExt
    {
        private static readonly RenderTargetBinding[] RenderTargetBinding = new RenderTargetBinding[1];


	    /// <summary>
	    ///     sets the RenderTarget without allocating a RenderTargetBinding array.
	    /// </summary>
	    /// <param name="self">Self.</param>
	    /// <param name="renderTarget">Render target.</param>
	    public static void SetRenderTarget(this GraphicsDevice self, RenderTarget2D renderTarget)
        {
            if (renderTarget == null)
            {
                self.SetRenderTargets(null);
            }
            else
            {
                RenderTargetBinding[0] = renderTarget;
                self.SetRenderTargets(RenderTargetBinding);
            }
        }
    }
}