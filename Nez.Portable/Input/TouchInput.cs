using System.Collections.Generic;
#if !FNA
using Microsoft.Xna.Framework.Input.Touch;

#endif


namespace Nez
{
	/// <summary>
	///     to enable touch input you must first call enableTouchSupport()
	/// </summary>
	public class TouchInput
    {
#pragma warning disable 0649
#pragma warning restore 0649


        private void OnGraphicsDeviceReset()
        {
#if !FNA
            TouchPanel.DisplayWidth = Core.GraphicsDevice.Viewport.Width;
            TouchPanel.DisplayHeight = Core.GraphicsDevice.Viewport.Height;
            TouchPanel.DisplayOrientation = Core.GraphicsDevice.PresentationParameters.DisplayOrientation;
#endif
        }


        internal void Update()
        {
            if (!IsConnected)
                return;

#if !FNA
            PreviousTouches = CurrentTouches;
            CurrentTouches = TouchPanel.GetState();

            PreviousGestures = CurrentGestures;
            CurrentGestures.Clear();
            while (TouchPanel.IsGestureAvailable)
                CurrentGestures.Add(TouchPanel.ReadGesture());
#endif
        }


        public void EnableTouchSupport()
        {
#if !FNA
            IsConnected = TouchPanel.GetCapabilities().IsConnected;
#endif

            if (IsConnected)
            {
                Core.Emitter.AddObserver(CoreEvents.GraphicsDeviceReset, OnGraphicsDeviceReset);
                Core.Emitter.AddObserver(CoreEvents.OrientationChanged, OnGraphicsDeviceReset);
                OnGraphicsDeviceReset();
            }
        }
#if !FNA
        public bool IsConnected { get; private set; }

        public TouchCollection CurrentTouches { get; private set; }

        public TouchCollection PreviousTouches { get; private set; }

        public List<GestureSample> PreviousGestures { get; private set; } = new List<GestureSample>();

        public List<GestureSample> CurrentGestures { get; } = new List<GestureSample>();

#endif
    }
}