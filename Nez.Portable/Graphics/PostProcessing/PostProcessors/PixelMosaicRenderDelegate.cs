using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.ECS;
using Nez.Graphics.Effects;
using Nez.Graphics.Textures;

namespace Nez.Graphics.PostProcessing.PostProcessors
{
	/// <summary>
	///     overlays a mosaic on top of the final render. Useful only for pixel perfect pixel art.
	/// </summary>
	public class PixelMosaicRenderDelegate : IFinalRenderDelegate
    {
        private int _lastMosaicScale = -1;
        private RenderTarget2D _mosaicRenderTex;
        private Texture2D _mosaicTexture;

        private Effect _effect;
        public Scene Scene { get; set; }


        public void OnAddedToScene()
        {
            _effect = Scene.Content.LoadEffect<Effect>("multiTextureOverlay", EffectResource.MultiTextureOverlayBytes);
        }


        public void OnSceneBackBufferSizeChanged(int newWidth, int newHeight)
        {
            // dont recreate the mosaic unless we really need to
            if (_lastMosaicScale != Scene.PixelPerfectScale)
            {
                CreateMosaicTexture(Scene.PixelPerfectScale);
                _lastMosaicScale = Scene.PixelPerfectScale;
            }

            if (_mosaicRenderTex != null)
            {
                _mosaicRenderTex.Dispose();
                _mosaicRenderTex = RenderTarget.Create(newWidth * Scene.PixelPerfectScale,
                    newHeight * Scene.PixelPerfectScale, DepthFormat.None);
            }
            else
            {
                _mosaicRenderTex = RenderTarget.Create(newWidth * Scene.PixelPerfectScale,
                    newHeight * Scene.PixelPerfectScale, DepthFormat.None);
            }

            // based on the look of games by: http://deepnight.net/games/strike-of-rage/
            // use the mosaic to render to a full sized RenderTarget repeating the mosaic
            GraphicsDeviceExt.SetRenderTarget(Core.GraphicsDevice, _mosaicRenderTex);
            Graphics.Instance.Batcher.Begin(BlendState.Opaque, SamplerState.PointWrap, DepthStencilState.None,
                RasterizerState.CullNone);
            Graphics.Instance.Batcher.Draw(_mosaicTexture, Vector2.Zero,
                new Rectangle(0, 0, _mosaicRenderTex.Width, _mosaicRenderTex.Height), Color.White);
            Graphics.Instance.Batcher.End();

            // let our Effect know about our rendered, full screen mosaic
            _effect.Parameters["_secondTexture"].SetValue(_mosaicRenderTex);
        }


        public void HandleFinalRender(Color letterboxColor, RenderTarget2D source, Rectangle finalRenderDestinationRect,
            SamplerState samplerState)
        {
            // we can just draw directly to the screen here with our effect
            GraphicsDeviceExt.SetRenderTarget(Core.GraphicsDevice, null);
            Core.GraphicsDevice.Clear(letterboxColor);
            Graphics.Instance.Batcher.Begin(BlendState.Opaque, samplerState, DepthStencilState.None,
                RasterizerState.CullNone, _effect);
            Graphics.Instance.Batcher.Draw(source, finalRenderDestinationRect, Color.White);
            Graphics.Instance.Batcher.End();
        }


        public void Unload()
        {
            _mosaicTexture.Dispose();
            _mosaicRenderTex.Dispose();
        }


        private void CreateMosaicTexture(int size)
        {
            if (_mosaicTexture != null)
                _mosaicTexture.Dispose();

            _mosaicTexture = new Texture2D(Core.GraphicsDevice, size, size);
            var colors = new uint[size * size];

            for (var i = 0; i < colors.Length; i++)
                colors[i] = 0x808080;

            colors[0] = 0xffffffff;
            colors[size * size - 1] = 0xff000000;

            for (var x = 1; x < size - 1; x++)
            {
                colors[x * size] = 0xffE0E0E0;
                colors[x * size + 1] = 0xffffffff;
                colors[x * size + size - 1] = 0xff000000;
            }

            for (var y = 1; y < size - 1; y++)
            {
                colors[y] = 0xffffffff;
                colors[(size - 1) * size + y] = 0xff000000;
            }

            _mosaicTexture.SetData(colors);
            _effect.Parameters["_secondTexture"].SetValue(_mosaicTexture);
        }
    }
}