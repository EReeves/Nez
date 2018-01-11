using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Nez.Debug;
using Nez.Utils;

namespace Nez.Graphics.Textures
{
    public class RenderTarget : IUpdatableManager
    {
        internal static RenderTarget Instance;
        private readonly List<TrackedRenderTarget2D> _renderTargetPool = new List<TrackedRenderTarget2D>();


        public RenderTarget()
        {
            Instance = this;
        }


        void IUpdatableManager.Update()
        {
            // remove any TrackedRenderTarget2Ds that havent been used for 2 frames or more
            for (var i = _renderTargetPool.Count - 1; i >= 0; i--)
                if (_renderTargetPool[i].LastFrameUsed + 2 < Time.FrameCount)
                {
                    _renderTargetPool[i].Dispose();
                    _renderTargetPool.RemoveAt(i);
                }
        }

	    /// <summary>
	    ///     internal class with additional lastFrameUsed field for managing temporary RenderTargets
	    /// </summary>
	    internal class TrackedRenderTarget2D : RenderTarget2D
        {
            public uint LastFrameUsed;


            public TrackedRenderTarget2D(int width, int height, SurfaceFormat preferredFormat,
                DepthFormat preferredDepthFormat) : base(Core.GraphicsDevice, width, height, false, preferredFormat,
                preferredDepthFormat, 0, RenderTargetUsage.PreserveContents)
            {
            }
        }


        #region Temporary RenderTarget management

	    /// <summary>
	    ///     gets a temporary RenderTarget2D from the pool. When done using it call releaseTemporary to put it back in the pool.
	    ///     Note that the
	    ///     contents of the RenderTarget2D could be anything at all so clear it if you need to before using it.
	    /// </summary>
	    /// <returns>The temporary.</returns>
	    /// <param name="width">Width.</param>
	    /// <param name="height">Height.</param>
	    public static RenderTarget2D GetTemporary(int width, int height)
        {
            return GetTemporary(width, height, Screen.PreferredDepthStencilFormat);
        }


	    /// <summary>
	    ///     gets a temporary RenderTarget2D from the pool. When done using it call releaseTemporary to put it back in the pool.
	    ///     Note that the
	    ///     contents of the RenderTarget2D could be anything at all so clear it if you need to before using it.
	    /// </summary>
	    /// <returns>The temporary.</returns>
	    /// <param name="width">Width.</param>
	    /// <param name="height">Height.</param>
	    /// <param name="depthFormat">Depth format.</param>
	    public static RenderTarget2D GetTemporary(int width, int height, DepthFormat depthFormat)
        {
            RenderTarget2D tempRenderTarget = null;
            var tempRenderTargetIndex = -1;
            for (var i = 0; i < Instance._renderTargetPool.Count; i++)
            {
                var renderTarget = Instance._renderTargetPool[i];
                if (renderTarget.Width == width && renderTarget.Height == height &&
                    renderTarget.DepthStencilFormat == depthFormat)
                {
                    tempRenderTarget = renderTarget;
                    tempRenderTargetIndex = i;
                    break;
                }
            }

            if (tempRenderTargetIndex >= 0)
            {
                Instance._renderTargetPool.RemoveAt(tempRenderTargetIndex);
                return tempRenderTarget;
            }

            // if we get here, we need to create a fresh RenderTarget2D
            return new TrackedRenderTarget2D(width, height, SurfaceFormat.Color, depthFormat);
        }


	    /// <summary>
	    ///     puts a temporary RenderTarget2D back in the pool. Do not attempt to put RenderTarget2Ds in the pool that were not
	    ///     acquired via getTemporary.
	    /// </summary>
	    /// <param name="renderTarget">Render target.</param>
	    public static void ReleaseTemporary(RenderTarget2D renderTarget)
        {
            Assert.IsTrue(renderTarget is TrackedRenderTarget2D,
                "Attempted to release a temporary RenderTarget2D that is not managed by the system");

            var trackedRt = renderTarget as TrackedRenderTarget2D;
            trackedRt.LastFrameUsed = Time.FrameCount;
            Instance._renderTargetPool.Add(trackedRt);
        }

        #endregion


        #region RenderTarget2D creation helpers

	    /// <summary>
	    ///     helper for creating a full screen RenderTarget2D
	    /// </summary>
	    public static RenderTarget2D Create()
        {
            return Create(Screen.Width, Screen.Height, Screen.BackBufferFormat, Screen.PreferredDepthStencilFormat);
        }


	    /// <summary>
	    ///     helper for creating a full screen RenderTarget2D with a specific DepthFormat
	    /// </summary>
	    /// <param name="preferredDepthFormat">Preferred depth format.</param>
	    public static RenderTarget2D Create(DepthFormat preferredDepthFormat)
        {
            return Create(Screen.Width, Screen.Height, Screen.BackBufferFormat, preferredDepthFormat);
        }


	    /// <summary>
	    ///     helper for creating a RenderTarget2D
	    /// </summary>
	    /// <param name="width">Width.</param>
	    /// <param name="height">Height.</param>
	    public static RenderTarget2D Create(int width, int height)
        {
            return Create(width, height, Screen.BackBufferFormat, Screen.PreferredDepthStencilFormat);
        }


	    /// <summary>
	    ///     helper for creating a RenderTarget2D
	    /// </summary>
	    /// <param name="width">Width.</param>
	    /// <param name="height">Height.</param>
	    /// <param name="preferredDepthFormat">Preferred depth format.</param>
	    public static RenderTarget2D Create(int width, int height, DepthFormat preferredDepthFormat)
        {
            return Create(width, height, Screen.BackBufferFormat, preferredDepthFormat);
        }


	    /// <summary>
	    ///     helper for creating a RenderTarget2D
	    /// </summary>
	    /// <param name="width">Width.</param>
	    /// <param name="height">Height.</param>
	    /// <param name="preferredFormat">Preferred format.</param>
	    /// <param name="preferredDepthFormat">Preferred depth format.</param>
	    public static RenderTarget2D Create(int width, int height, SurfaceFormat preferredFormat,
            DepthFormat preferredDepthFormat)
        {
            return new RenderTarget2D(Core.GraphicsDevice, width, height, false, preferredFormat, preferredDepthFormat,
                0, RenderTargetUsage.PreserveContents);
        }

        #endregion
    }
}