using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Graphics.Batcher;
using Nez.Utils;
using Nez.Utils.Fonts;

namespace Nez.Debug
{
    internal class DebugDrawItem
    {
        public BitmapFont BitmapFont;

        // shared by multiple items
        public Color Color;

        internal DebugDrawType DrawType;
        public float Duration;
        public Vector2 End;
        public Vector2 Position;
        public Rectangle Rectangle;
        public float Scale;
        public int Size;
        public NezSpriteFont SpriteFont;

        // used for Line items
        public Vector2 Start;

        // used for Text items
        public string Text;

        // used for Pixel items
        public float X, Y;


        public DebugDrawItem(Vector2 start, Vector2 end, Color color, float duration)
        {
            this.Start = start;
            this.End = end;
            this.Color = color;
            this.Duration = duration;
            DrawType = DebugDrawType.Line;
        }


        public DebugDrawItem(Rectangle rectangle, Color color, float duration)
        {
            this.Rectangle = rectangle;
            this.Color = color;
            this.Duration = duration;
            DrawType = DebugDrawType.HollowRectangle;
        }


        public DebugDrawItem(float x, float y, int size, Color color, float duration)
        {
            this.X = x;
            this.Y = y;
            this.Size = size;
            this.Color = color;
            this.Duration = duration;
            DrawType = DebugDrawType.Pixel;
        }


        public DebugDrawItem(BitmapFont bitmapFont, string text, Vector2 position, Color color, float duration,
            float scale)
        {
            this.BitmapFont = bitmapFont;
            this.Text = text;
            this.Position = position;
            this.Color = color;
            this.Scale = scale;
            this.Duration = duration;
            DrawType = DebugDrawType.BitmapFontText;
        }


        public DebugDrawItem(NezSpriteFont spriteFont, string text, Vector2 position, Color color, float duration,
            float scale)
        {
            this.SpriteFont = spriteFont;
            this.Text = text;
            this.Position = position;
            this.Color = color;
            this.Scale = scale;
            this.Duration = duration;
            DrawType = DebugDrawType.SpriteFontText;
        }


        public DebugDrawItem(string text, Color color, float duration, float scale)
        {
            BitmapFont = Graphics.Graphics.Instance.BitmapFont;
            this.Text = text;
            this.Color = color;
            this.Scale = scale;
            this.Duration = duration;
            DrawType = DebugDrawType.ConsoleText;
        }


	    /// <summary>
	    ///     returns true if we are done with this debug draw item
	    /// </summary>
	    public bool Draw(Graphics.Graphics graphics)
        {
            switch (DrawType)
            {
                case DebugDrawType.Line:
                    graphics.Batcher.DrawLine(Start, End, Color);
                    break;
                case DebugDrawType.HollowRectangle:
                    graphics.Batcher.DrawHollowRect(Rectangle, Color);
                    break;
                case DebugDrawType.Pixel:
                    graphics.Batcher.DrawPixel(X, Y, Color, Size);
                    break;
                case DebugDrawType.BitmapFontText:
                    graphics.Batcher.DrawString(BitmapFont, Text, Position, Color, 0f, Vector2.Zero, Scale,
                        SpriteEffects.None, 0f);
                    break;
                case DebugDrawType.SpriteFontText:
                    graphics.Batcher.DrawString(SpriteFont, Text, Position, Color, 0f, Vector2.Zero, new Vector2(Scale),
                        SpriteEffects.None, 0f);
                    break;
                case DebugDrawType.ConsoleText:
                    graphics.Batcher.DrawString(BitmapFont, Text, Position, Color, 0f, Vector2.Zero, Scale,
                        SpriteEffects.None, 0f);
                    break;
            }

            Duration -= Time.DeltaTime;

            return Duration < 0f;
        }


        public float GetHeight()
        {
            switch (DrawType)
            {
                case DebugDrawType.Line:
                    return (End - Start).Y;
                case DebugDrawType.HollowRectangle:
                    return Rectangle.Height;
                case DebugDrawType.Pixel:
                    return Size;
                case DebugDrawType.BitmapFontText:
                case DebugDrawType.ConsoleText:
                    return BitmapFont.MeasureString(Text).Y * Scale;
                case DebugDrawType.SpriteFontText:
                    return SpriteFont.MeasureString(Text).Y * Scale;
            }

            return 0;
        }

        internal enum DebugDrawType
        {
            Line,
            HollowRectangle,
            Pixel,
            BitmapFontText,
            SpriteFontText,
            ConsoleText
        }
    }
}