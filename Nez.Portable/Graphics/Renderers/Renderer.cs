﻿using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Nez.ECS;
using Nez.ECS.Components.Renderables;
using Nez.Graphics.Textures;

namespace Nez.Graphics.Renderers
{
	/// <summary>
	///     Renderers are added to a Scene and handle all of the actual calls to RenderableComponent.render and
	///     Entity.debugRender.
	///     A simple Renderer could just start the Graphics.instanceGraphics.batcher or it could create its own local Graphics
	///     instance
	///     if it needs it for some kind of custom rendering.
	///     Note that it is a best practice to ensure all Renderers that render to a RenderTarget have lower renderOrders to
	///     avoid issues
	///     with clearing the back buffer
	///     (http://gamedev.stackexchange.com/questions/90396/monogame-setrendertarget-is-wiping-the-backbuffer).
	///     Giving them a negative renderOrder is a good strategy to deal with this.
	/// </summary>
	public abstract class Renderer : IComparable<Renderer>
    {
	    /// <summary>
	    ///     specifies the order in which the Renderers will be called by the scene
	    /// </summary>
	    public readonly int RenderOrder;

	    /// <summary>
	    ///     holds the current Material of the last rendered Renderable (or the Renderer.material if no changes were made)
	    /// </summary>
	    protected Material CurrentMaterial;

	    /// <summary>
	    ///     the Camera this renderer uses for rendering (really its transformMatrix and bounds for culling). This is a
	    ///     convenience field and isnt
	    ///     required. Renderer subclasses can pick the camera used when calling beginRender.
	    /// </summary>
	    public Camera Camera;

	    /// <summary>
	    ///     Material used by the Batcher. Any RenderableComponent can override this.
	    /// </summary>
	    public Material Material = Material.DefaultMaterial;

	    /// <summary>
	    ///     if renderTarget is not null this Color will be used to clear the screen
	    /// </summary>
	    public Color RenderTargetClearColor = Color.Transparent;

	    /// <summary>
	    ///     if renderTarget is not null this renderer will render into the RenderTarget instead of to the screen
	    /// </summary>
	    public RenderTexture RenderTexture;

	    /// <summary>
	    ///     flag for this renderer that decides if it should debug render or not. The render method receives a bool
	    ///     (debugRenderEnabled)
	    ///     letting the renderer know if the global debug rendering is on/off. The renderer then uses the local bool to decide
	    ///     if it
	    ///     should debug render or not.
	    /// </summary>
	    public bool ShouldDebugRender = true;

	    /// <summary>
	    ///     if true, the Scene will call the render method AFTER all PostProcessors have finished. This must be set to true
	    ///     BEFORE calling
	    ///     Scene.addRenderer to take effect and the Renderer should NOT have a renderTexture. The main reason for this type of
	    ///     Renderer
	    ///     is so that you can render your UI without post processing on top of the rest of your Scene. The ScreenSpaceRenderer
	    ///     is an
	    ///     example Renderer that sets this to true;
	    /// </summary>
	    public bool WantsToRenderAfterPostProcessors;


        protected Renderer(int renderOrder) : this(renderOrder, null)
        {
        }


        protected Renderer(int renderOrder, Camera camera)
        {
            this.Camera = camera;
            this.RenderOrder = renderOrder;
        }

	    /// <summary>
	    ///     if true, the Scene will call SetRenderTarget with the scene RenderTarget. The default implementaiton returns true
	    ///     if the Renderer
	    ///     has a renderTexture
	    /// </summary>
	    /// <value><c>true</c> if wants to render to scene render target; otherwise, <c>false</c>.</value>
	    public virtual bool WantsToRenderToSceneRenderTarget => RenderTexture == null;


        public int CompareTo(Renderer other)
        {
            return RenderOrder.CompareTo(other.RenderOrder);
        }


	    /// <summary>
	    ///     if a RenderTarget is used this will set it up. The Batcher is also started. The passed in Camera will be used to
	    ///     set the ViewPort
	    ///     (if a ViewportAdapter is present) and for the Batcher transform Matrix.
	    /// </summary>
	    /// <param name="cam">Cam.</param>
	    protected virtual void BeginRender(Camera cam)
        {
            // if we have a renderTarget render into it
            if (RenderTexture != null)
            {
                GraphicsDeviceExt.SetRenderTarget(Core.GraphicsDevice, RenderTexture);
                Core.GraphicsDevice.Clear(RenderTargetClearColor);
            }

            CurrentMaterial = Material;
            Graphics.Instance.Batcher.Begin(CurrentMaterial, cam.TransformMatrix);
        }


        public abstract void Render(Scene scene);


	    /// <summary>
	    ///     renders the RenderableComponent flushing the Batcher and resetting current material if necessary
	    /// </summary>
	    /// <param name="renderable">Renderable.</param>
	    /// <param name="cam">Cam.</param>
	    [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void RenderAfterStateCheck(IRenderable renderable, Camera cam)
        {
            // check for Material changes
            if (renderable.Material != null && renderable.Material != CurrentMaterial)
            {
                CurrentMaterial = renderable.Material;
                if (CurrentMaterial.Effect != null)
                    CurrentMaterial.OnPreRender(cam);
                FlushBatch(cam);
            }
            else if (renderable.Material == null && CurrentMaterial != Material)
            {
                CurrentMaterial = Material;
                FlushBatch(cam);
            }

            renderable.Render(Graphics.Instance, cam);
        }


	    /// <summary>
	    ///     force flushes the Batcher by calling End then Begin on it.
	    /// </summary>
	    private void FlushBatch(Camera cam)
        {
            Graphics.Instance.Batcher.End();
            Graphics.Instance.Batcher.Begin(CurrentMaterial, cam.TransformMatrix);
        }


	    /// <summary>
	    ///     ends the Batcher and clears the RenderTarget if it had a RenderTarget
	    /// </summary>
	    protected virtual void EndRender()
        {
            Graphics.Instance.Batcher.End();
        }


	    /// <summary>
	    ///     default debugRender method just loops through all entities and calls entity.debugRender
	    /// </summary>
	    /// <param name="scene">Scene.</param>
	    protected virtual void DebugRender(Scene scene, Camera cam)
        {
            Graphics.Instance.Batcher.End();
            Graphics.Instance.Batcher.Begin(Core.Scene.Camera.TransformMatrix);

            for (var i = 0; i < scene.Entities.Count; i++)
            {
                var entity = scene.Entities[i];
                if (entity.Enabled)
                    entity.DebugRender(Graphics.Instance);
            }
        }


	    /// <summary>
	    ///     called when the default scene RenderTarget is resized and when adding a Renderer if the scene has already began.
	    ///     default implementation
	    ///     calls through to RenderTexture.onSceneBackBufferSizeChanged
	    ///     so that it can size itself appropriately if necessary.
	    /// </summary>
	    /// <param name="newWidth">New width.</param>
	    /// <param name="newHeight">New height.</param>
	    public virtual void OnSceneBackBufferSizeChanged(int newWidth, int newHeight)
        {
            if (RenderTexture != null)
                RenderTexture.OnSceneBackBufferSizeChanged(newWidth, newHeight);
        }


	    /// <summary>
	    ///     called when a scene is ended. use this for cleanup.
	    /// </summary>
	    public virtual void Unload()
        {
            if (RenderTexture != null)
            {
                RenderTexture.Dispose();
                RenderTexture = null;
            }
        }
    }
}