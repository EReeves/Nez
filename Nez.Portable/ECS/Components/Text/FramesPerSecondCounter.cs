using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Nez;
using Nez.ECS.Components.Renderables;
using Nez.ECS.Components.Renderables.Sprites;
using Nez.Graphics.Batcher;
using Nez.Utils;
using Nez.Utils.Fonts;

namespace Nez.ECS.Components.Text
{
    public class FramesPerSecondCounter : Text, IUpdatable
    {
        public enum FpsDockPosition
        {
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight
        }

        private readonly Queue<float> _sampleBuffer = new Queue<float>();
        private Vector2 _dockOffset;

        private FpsDockPosition _dockPosition;
        public float AverageFramesPerSecond;
        public float CurrentFramesPerSecond;

	    /// <summary>
	    ///     total number of samples that should be stored and averaged for calculating the FPS
	    /// </summary>
	    public int MaximumSamples;

        public long TotalFrames;


        public FramesPerSecondCounter(BitmapFont font, Color color,
            FpsDockPosition dockPosition = FpsDockPosition.TopRight, int maximumSamples = 100) : base(font,
            string.Empty, Vector2.Zero, color)
        {
            this.MaximumSamples = maximumSamples;
            this.DockPosition = dockPosition;
            Init();
        }


        public FramesPerSecondCounter(NezSpriteFont font, Color color,
            FpsDockPosition dockPosition = FpsDockPosition.TopRight, int maximumSamples = 100) : base(font,
            string.Empty, Vector2.Zero, color)
        {
            this.MaximumSamples = maximumSamples;
            this.DockPosition = dockPosition;
            Init();
        }


	    /// <summary>
	    ///     position the FPS counter should be docked
	    /// </summary>
	    /// <value>The dock position.</value>
	    public FpsDockPosition DockPosition
        {
            get => _dockPosition;
            set
            {
                _dockPosition = value;
                UpdateTextPosition();
            }
        }

	    /// <summary>
	    ///     offset from dockPosition the FPS counter should be drawn
	    /// </summary>
	    /// <value>The dock offset.</value>
	    public Vector2 DockOffset
        {
            get => _dockOffset;
            set
            {
                _dockOffset = value;
                UpdateTextPosition();
            }
        }


        void IUpdatable.Update()
        {
            CurrentFramesPerSecond = 1.0f / Time.UnscaledDeltaTime;
            _sampleBuffer.Enqueue(CurrentFramesPerSecond);

            if (_sampleBuffer.Count > MaximumSamples)
            {
                _sampleBuffer.Dequeue();
                AverageFramesPerSecond = _sampleBuffer.Average(i => i);
            }
            else
            {
                AverageFramesPerSecond = CurrentFramesPerSecond;
            }

            TotalFrames++;

            text = $"FPS: {AverageFramesPerSecond:0.00}";
        }


        private void Init()
        {
            UpdateTextPosition();
        }


        private void UpdateTextPosition()
        {
            switch (DockPosition)
            {
                case FpsDockPosition.TopLeft:
                    HorizontalAlign = HorizontalAlign.Left;
                    VerticalAlign = VerticalAlign.Top;
                    ((RenderableComponent) this).LocalOffset = DockOffset;
                    break;
                case FpsDockPosition.TopRight:
                    HorizontalAlign = HorizontalAlign.Right;
                    VerticalAlign = VerticalAlign.Top;
                    ((RenderableComponent) this).LocalOffset = new Vector2(Core.GraphicsDevice.Viewport.Width - DockOffset.X, DockOffset.Y);
                    break;
                case FpsDockPosition.BottomLeft:
                    HorizontalAlign = HorizontalAlign.Left;
                    VerticalAlign = VerticalAlign.Bottom;
                    ((RenderableComponent) this).LocalOffset = new Vector2(DockOffset.X, Core.GraphicsDevice.Viewport.Height - DockOffset.Y);
                    break;
                case FpsDockPosition.BottomRight:
                    HorizontalAlign = HorizontalAlign.Right;
                    VerticalAlign = VerticalAlign.Bottom;
                    ((RenderableComponent) this).LocalOffset = new Vector2(Core.GraphicsDevice.Viewport.Width - DockOffset.X,
                        Core.GraphicsDevice.Viewport.Height - DockOffset.Y);
                    break;
            }
        }


        public void Reset()
        {
            TotalFrames = 0;
            _sampleBuffer.Clear();
        }


        public override void Render(Graphics.Graphics graphics, Camera camera)
        {
            // we override render and use position instead of entityPosition. this keeps the text in place even if the entity moves
            BatcherIFontExt.DrawString(graphics.Batcher, Font, (string) text, ((RenderableComponent) this).LocalOffset, Color, Entity.Transform.Rotation, ((Sprite) this).Origin,
                Entity.Transform.Scale, SpriteEffects, ((RenderableComponent) this).LayerDepth);
        }


        public override void DebugRender(Graphics.Graphics graphics)
        {
            // due to the override of position in render we have to do the same here
            var rect = Bounds;
            rect.Location = ((RenderableComponent) this).LocalOffset;
            graphics.Batcher.DrawHollowRect(rect, Color.Yellow);
        }


        #region Fluent setters

	    /// <summary>
	    ///     Sets how far the fps text will appear from the edges of the screen.
	    /// </summary>
	    /// <param name="dockOffset">Offset from screen edges</param>
	    public FramesPerSecondCounter SetDockOffset(Vector2 dockOffset)
        {
            this.DockOffset = dockOffset;
            return this;
        }


	    /// <summary>
	    ///     Sets which corner of the screen the fps text will show.
	    /// </summary>
	    /// <param name="dockPosition">Corner of the screen</param>
	    public FramesPerSecondCounter SetDockPosition(FpsDockPosition dockPosition)
        {
            this.DockPosition = dockPosition;
            return this;
        }

        #endregion
    }
}