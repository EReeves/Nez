using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.Textures;

namespace Nez
{
	/// <summary>
	///     this PostProcessor expects that the layerRenderTarget is the top-most layer and that it contains
	/// </summary>
	public class PixelBloomPostProcessor : BloomPostProcessor
    {
        private readonly RenderTexture _layerRt;
        private readonly RenderTexture _tempRt;


        public PixelBloomPostProcessor(RenderTexture layerRenderTexture, int executionOrder) : base(executionOrder)
        {
            _layerRt = layerRenderTexture;
            _tempRt = new RenderTexture(_layerRt.RenderTarget.Width, _layerRt.RenderTarget.Height, DepthFormat.None);
        }


        public override void OnSceneBackBufferSizeChanged(int newWidth, int newHeight)
        {
            base.OnSceneBackBufferSizeChanged(newWidth, newHeight);

            _tempRt.Resize(newWidth, newHeight);
        }


        public override void Process(RenderTarget2D source, RenderTarget2D destination)
        {
            // first we process the rendered layer with the bloom effect
            base.Process(_layerRt, _tempRt);

            // we need to be careful here and ensure we use AlphaBlending since the layer we rendered is mostly transparent
            GraphicsDeviceExt.SetRenderTarget(Core.GraphicsDevice, destination);
            Graphics.Instance.Batcher.Begin(BlendState.AlphaBlend, SamplerState, DepthStencilState.None,
                RasterizerState.CullNone);

            // now we first draw the full scene (source), then draw our bloomed layer (tempRT) then draw the un-bloomed layer (layerRT)
            Graphics.Instance.Batcher.Draw(source, new Rectangle(0, 0, destination.Width, destination.Height),
                Color.White);
            Graphics.Instance.Batcher.Draw(_tempRt, new Rectangle(0, 0, destination.Width, destination.Height),
                Color.White);
            Graphics.Instance.Batcher.Draw(_layerRt, new Rectangle(0, 0, destination.Width, destination.Height),
                Color.White);

            Graphics.Instance.Batcher.End();
        }


        public override void Unload()
        {
            base.Unload();

            _tempRt.Dispose();
        }
    }
}