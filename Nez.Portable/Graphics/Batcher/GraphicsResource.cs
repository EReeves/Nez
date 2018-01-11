using System;
using Microsoft.Xna.Framework.Graphics;
using Nez.Debug;
using Nez.Utils;

namespace Nez.Graphics.Batcher
{
	/// <summary>
	///     this class exists only so that we can sneak the Batcher through and have it work just like SpriteBatch with regard
	///     to resource handling.
	/// </summary>
	public abstract class GraphicsResource : IDisposable
    {
        // The GraphicsDevice property should only be accessed in Dispose(bool) if the disposing
        // parameter is true. If disposing is false, the GraphicsDevice may or may not be disposed yet.
        private GraphicsDevice _graphicsDevice;

        private WeakReference _selfReference;


        internal GraphicsResource()
        {
        }

        public GraphicsDevice GraphicsDevice
        {
            get => _graphicsDevice;
            internal set
            {
                Assert.IsTrue(value != null);

                if (_graphicsDevice == value)
                    return;

                // VertexDeclaration objects can be bound to multiple GraphicsDevice objects
                // during their lifetime. But only one GraphicsDevice should retain ownership.
                if (_graphicsDevice != null)
                {
                    UpdateResourceReference(false);
                    _selfReference = null;
                }

                _graphicsDevice = value;

                _selfReference = new WeakReference(this);
                UpdateResourceReference(true);
            }
        }

        public bool IsDisposed { get; private set; }


        public void Dispose()
        {
            // Dispose of managed objects as well
            Dispose(true);
            // Since we have been manually disposed, do not call the finalizer on this object
            GC.SuppressFinalize(this);
        }


        ~GraphicsResource()
        {
            // Pass false so the managed objects are not released
            Dispose(false);
        }


	    /// <summary>
	    ///     The method that derived classes should override to implement disposing of managed and native resources.
	    /// </summary>
	    /// <param name="disposing">True if managed objects should be disposed.</param>
	    /// <remarks>Native resources should always be released regardless of the value of the disposing parameter.</remarks>
	    protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    // Release managed objects
                }

                // Remove from the global list of graphics resources
                if (GraphicsDevice != null)
                    UpdateResourceReference(false);

                _selfReference = null;
                _graphicsDevice = null;
                IsDisposed = true;
            }
        }


        private void UpdateResourceReference(bool shouldAdd)
        {
            var method = shouldAdd ? "AddResourceReference" : "RemoveResourceReference";
            var methodInfo = ReflectionUtils.GetMethodInfo(GraphicsDevice, method);
            methodInfo.Invoke(GraphicsDevice, new object[] {_selfReference});
        }
    }
}