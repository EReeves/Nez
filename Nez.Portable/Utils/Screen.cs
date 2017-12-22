using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Nez
{
    public static class Screen
    {
        internal static GraphicsDeviceManager GraphicsManager;


	    /// <summary>
	    ///     width of the GraphicsDevice back buffer
	    /// </summary>
	    /// <value>The width.</value>
	    public static int Width
        {
            get => GraphicsManager.GraphicsDevice.PresentationParameters.BackBufferWidth;
            set => GraphicsManager.GraphicsDevice.PresentationParameters.BackBufferWidth = value;
        }


	    /// <summary>
	    ///     height of the GraphicsDevice back buffer
	    /// </summary>
	    /// <value>The height.</value>
	    public static int Height
        {
            get => GraphicsManager.GraphicsDevice.PresentationParameters.BackBufferHeight;
            set => GraphicsManager.GraphicsDevice.PresentationParameters.BackBufferHeight = value;
        }


	    /// <summary>
	    ///     gets the Screen's center
	    /// </summary>
	    /// <value>The center.</value>
	    public static Vector2 Center => new Vector2(Width / 2, Height / 2);


        public static int PreferredBackBufferWidth
        {
            get => GraphicsManager.PreferredBackBufferWidth;
            set => GraphicsManager.PreferredBackBufferWidth = value;
        }


        public static int PreferredBackBufferHeight
        {
            get => GraphicsManager.PreferredBackBufferHeight;
            set => GraphicsManager.PreferredBackBufferHeight = value;
        }


        public static int MonitorWidth => GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;


        public static int MonitorHeight => GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;


        public static SurfaceFormat BackBufferFormat =>
            GraphicsManager.GraphicsDevice.PresentationParameters.BackBufferFormat;


        public static SurfaceFormat PreferredBackBufferFormat
        {
            get => GraphicsManager.PreferredBackBufferFormat;
            set => GraphicsManager.PreferredBackBufferFormat = value;
        }


        public static bool SynchronizeWithVerticalRetrace
        {
            get => GraphicsManager.SynchronizeWithVerticalRetrace;
            set => GraphicsManager.SynchronizeWithVerticalRetrace = value;
        }


        // defaults to Depth24Stencil8
        public static DepthFormat PreferredDepthStencilFormat
        {
            get => GraphicsManager.PreferredDepthStencilFormat;
            set => GraphicsManager.PreferredDepthStencilFormat = value;
        }


        public static bool IsFullscreen
        {
            get => GraphicsManager.IsFullScreen;
            set => GraphicsManager.IsFullScreen = value;
        }


        public static DisplayOrientation SupportedOrientations
        {
            get => GraphicsManager.SupportedOrientations;
            set => GraphicsManager.SupportedOrientations = value;
        }


        internal static void Initialize(GraphicsDeviceManager graphicsManager)
        {
            GraphicsManager = graphicsManager;
        }


        public static void ApplyChanges()
        {
            GraphicsManager.ApplyChanges();
        }


	    /// <summary>
	    ///     sets the preferredBackBuffer then applies the changes
	    /// </summary>
	    /// <param name="width">Width.</param>
	    /// <param name="height">Height.</param>
	    public static void SetSize(int width, int height)
        {
            PreferredBackBufferWidth = width;
            PreferredBackBufferHeight = height;
            ApplyChanges();
        }
    }
}