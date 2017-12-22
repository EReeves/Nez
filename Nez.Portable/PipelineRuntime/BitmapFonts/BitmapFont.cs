using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Nez.BitmapFonts
{
    public class BitmapFont : IFont
    {
        private readonly Dictionary<char, BitmapFontRegion> _characterMap;

	    /// <summary>
	    ///     width of a space
	    /// </summary>
	    public readonly int SpaceWidth;

	    /// <summary>
	    ///     this sucker gets used a lot so we cache it to avoid having to create it every frame
	    /// </summary>
	    private Matrix2D _transformationMatrix = Matrix2D.Identity;

	    /// <summary>
	    ///     populated with ' ' by default and reset whenever defaultCharacter is set
	    /// </summary>
	    public BitmapFontRegion DefaultCharacterRegion;

	    /// <summary>
	    ///     The distance from the bottom of the glyph that extends the lowest to the baseline. This number is negative.
	    /// </summary>
	    public float Descent;

	    /// <summary>
	    ///     Gets or sets the line spacing (the distance from baseline to baseline) of the font.
	    /// </summary>
	    /// <value>The height of the line.</value>
	    public int LineHeight;

	    /// <summary>
	    ///     these are currently read in from the .fnt file but not used
	    /// </summary>
	    public float PadTop, PadBottom, PadLeft, PadRight;

	    /// <summary>
	    ///     Gets or sets the spacing (tracking) between characters in the font.
	    /// </summary>
	    public float Spacing;


        internal BitmapFont(BitmapFontRegion[] regions, int lineHeight)
        {
            _characterMap = new Dictionary<char, BitmapFontRegion>(regions.Length, CharComparer.DefaultCharComparer);
            for (var i = 0; i < regions.Length; i++)
                _characterMap[regions[i].Character] = regions[i];

            this.LineHeight = lineHeight;
            DefaultCharacter = ' ';
            SpaceWidth = DefaultCharacterRegion.Width + DefaultCharacterRegion.XAdvance;
        }

	    /// <summary>
	    ///     Gets or sets the character that will be substituted when a given character is not included in the font.
	    /// </summary>
	    public char DefaultCharacter
        {
            set
            {
                if (!_characterMap.TryGetValue(value, out DefaultCharacterRegion))
                    Debug.Error("BitmapFont does not contain a region for the default character being set: {0}", value);
            }
        }

        float IFont.LineSpacing => LineHeight;


	    /// <summary>
	    ///     Returns the size of the contents of a string when rendered in this font.
	    /// </summary>
	    /// <returns>The string.</returns>
	    /// <param name="text">Text.</param>
	    public Vector2 MeasureString(string text)
        {
            var source = new FontCharacterSource(text);
            Vector2 size;
            MeasureString(ref source, out size);
            return size;
        }


	    /// <summary>
	    ///     Returns the size of the contents of a StringBuilder when rendered in this font.
	    /// </summary>
	    /// <returns>The string.</returns>
	    /// <param name="text">Text.</param>
	    public Vector2 MeasureString(StringBuilder text)
        {
            var source = new FontCharacterSource(text);
            Vector2 size;
            MeasureString(ref source, out size);
            return size;
        }


	    /// <summary>
	    ///     checks to see if a BitmapFontRegion exists for the char
	    /// </summary>
	    /// <returns><c>true</c>, if region exists for char was fonted, <c>false</c> otherwise.</returns>
	    /// <param name="c">C.</param>
	    public bool HasCharacter(char c)
        {
            BitmapFontRegion fontRegion;
            return TryGetFontRegionForChar(c, out fontRegion);
        }


        public string WrapText(string text, float maxLineWidth)
        {
            var words = text.Split(' ');
            var sb = new StringBuilder();
            var lineWidth = 0f;

            if (maxLineWidth < SpaceWidth)
                return string.Empty;

            foreach (var word in words)
            {
                var size = MeasureString(word);

                if (lineWidth + size.X < maxLineWidth)
                {
                    sb.Append(word + " ");
                    lineWidth += size.X + SpaceWidth;
                }
                else
                {
                    if (size.X > maxLineWidth)
                    {
                        if (sb.ToString() == "")
                            sb.Append(WrapText(word.Insert(word.Length / 2, " ") + " ", maxLineWidth));
                        else
                            sb.Append("\n" + WrapText(word.Insert(word.Length / 2, " ") + " ", maxLineWidth));
                    }
                    else
                    {
                        sb.Append("\n" + word + " ");
                        lineWidth = size.X + SpaceWidth;
                    }
                }
            }

            return sb.ToString();
        }


	    /// <summary>
	    ///     gets the BitmapFontRegion for the given char optionally substituting the default region if it isnt present.
	    /// </summary>
	    /// <returns><c>true</c>, if get font region for char was tryed, <c>false</c> otherwise.</returns>
	    /// <param name="c">C.</param>
	    /// <param name="fontRegion">Font region.</param>
	    /// <param name="useDefaultRegionIfNotPresent">If set to <c>true</c> use default region if not present.</param>
	    public bool TryGetFontRegionForChar(char c, out BitmapFontRegion fontRegion,
            bool useDefaultRegionIfNotPresent = false)
        {
            if (!_characterMap.TryGetValue(c, out fontRegion))
            {
                if (useDefaultRegionIfNotPresent)
                {
                    fontRegion = DefaultCharacterRegion;
                    return true;
                }
                return false;
            }

            return true;
        }


	    /// <summary>
	    ///     gets the BitmapFontRegion for char. Returns null if it doesnt exist and useDefaultRegionIfNotPresent is false.
	    /// </summary>
	    /// <returns>The region for char.</returns>
	    /// <param name="c">C.</param>
	    /// <param name="useDefaultRegionIfNotPresent">If set to <c>true</c> use default region if not present.</param>
	    public BitmapFontRegion FontRegionForChar(char c, bool useDefaultRegionIfNotPresent = false)
        {
            BitmapFontRegion fontRegion;
            TryGetFontRegionForChar(c, out fontRegion, useDefaultRegionIfNotPresent);
            return fontRegion;
        }


        private void MeasureString(ref FontCharacterSource text, out Vector2 size)
        {
            if (text.Length == 0)
            {
                size = Vector2.Zero;
                return;
            }

            var width = 0.0f;
            var finalLineHeight = (float) LineHeight;
            var fullLineCount = 0;
            BitmapFontRegion currentFontRegion = null;
            var offset = Vector2.Zero;

            for (var i = 0; i < text.Length; i++)
            {
                var c = text[i];

                if (c == '\r')
                    continue;

                if (c == '\n')
                {
                    fullLineCount++;
                    finalLineHeight = LineHeight;

                    offset.X = 0;
                    offset.Y = LineHeight * fullLineCount;
                    currentFontRegion = null;
                    continue;
                }

                if (currentFontRegion != null)
                    offset.X += Spacing + currentFontRegion.XAdvance;

                if (!_characterMap.TryGetValue(c, out currentFontRegion))
                    currentFontRegion = DefaultCharacterRegion;

                var proposedWidth = offset.X + currentFontRegion.XAdvance + Spacing;
                if (proposedWidth > width)
                    width = proposedWidth;

                if (currentFontRegion.Height + currentFontRegion.YOffset > finalLineHeight)
                    finalLineHeight = currentFontRegion.Height + currentFontRegion.YOffset;
            }

            size.X = width;
            size.Y = fullLineCount * LineHeight + finalLineHeight;
        }


        private class CharComparer : IEqualityComparer<char>
        {
            public static readonly CharComparer DefaultCharComparer = new CharComparer();

            public bool Equals(char x, char y)
            {
                return x == y;
            }

            public int GetHashCode(char b)
            {
                return b | (b << 16);
            }
        }


        #region drawing

        void IFont.DrawInto(Batcher batcher, string text, Vector2 position, Color color,
            float rotation, Vector2 origin, Vector2 scale, SpriteEffects effect, float depth)
        {
            var source = new FontCharacterSource(text);
            DrawInto(batcher, ref source, position, color, rotation, origin, scale, effect, depth);
        }


        void IFont.DrawInto(Batcher batcher, StringBuilder text, Vector2 position, Color color,
            float rotation, Vector2 origin, Vector2 scale, SpriteEffects effect, float depth)
        {
            var source = new FontCharacterSource(text);
            DrawInto(batcher, ref source, position, color, rotation, origin, scale, effect, depth);
        }


        internal void DrawInto(Batcher batcher, ref FontCharacterSource text, Vector2 position, Color color,
            float rotation, Vector2 origin, Vector2 scale, SpriteEffects effect, float depth)
        {
            var flipAdjustment = Vector2.Zero;

            var flippedVert = (effect & SpriteEffects.FlipVertically) == SpriteEffects.FlipVertically;
            var flippedHorz = (effect & SpriteEffects.FlipHorizontally) == SpriteEffects.FlipHorizontally;

            if (flippedVert || flippedHorz)
            {
                Vector2 size;
                MeasureString(ref text, out size);

                if (flippedHorz)
                {
                    origin.X *= -1;
                    flipAdjustment.X = -size.X;
                }

                if (flippedVert)
                {
                    origin.Y *= -1;
                    flipAdjustment.Y = LineHeight - size.Y;
                }
            }


            var requiresTransformation = flippedHorz || flippedVert || rotation != 0f || scale != Vector2.One;
            if (requiresTransformation)
            {
                Matrix2D temp;
                Matrix2D.CreateTranslation(-origin.X, -origin.Y, out _transformationMatrix);
                Matrix2D.CreateScale(flippedHorz ? -scale.X : scale.X, flippedVert ? -scale.Y : scale.Y, out temp);
                Matrix2D.Multiply(ref _transformationMatrix, ref temp, out _transformationMatrix);
                Matrix2D.CreateTranslation(flipAdjustment.X, flipAdjustment.Y, out temp);
                Matrix2D.Multiply(ref temp, ref _transformationMatrix, out _transformationMatrix);
                Matrix2D.CreateRotation(rotation, out temp);
                Matrix2D.Multiply(ref _transformationMatrix, ref temp, out _transformationMatrix);
                Matrix2D.CreateTranslation(position.X, position.Y, out temp);
                Matrix2D.Multiply(ref _transformationMatrix, ref temp, out _transformationMatrix);
            }

            BitmapFontRegion currentFontRegion = null;
            var offset = requiresTransformation ? Vector2.Zero : position - origin;

            for (var i = 0; i < text.Length; ++i)
            {
                var c = text[i];
                if (c == '\r')
                    continue;

                if (c == '\n')
                {
                    offset.X = requiresTransformation ? 0f : position.X - origin.X;
                    offset.Y += LineHeight;
                    currentFontRegion = null;
                    continue;
                }

                if (currentFontRegion != null)
                    offset.X += Spacing + currentFontRegion.XAdvance;

                if (!_characterMap.TryGetValue(c, out currentFontRegion))
                    currentFontRegion = DefaultCharacterRegion;


                var p = offset;

                if (flippedHorz)
                    p.X += currentFontRegion.Width;
                p.X += currentFontRegion.XOffset;

                if (flippedVert)
                    p.Y += currentFontRegion.Height - LineHeight;
                p.Y += currentFontRegion.YOffset;

                // transform our point if we need to
                if (requiresTransformation)
                    Vector2Ext.Transform(ref p, ref _transformationMatrix, out p);

                var destRect = RectangleExt.FromFloats
                (
                    p.X, p.Y,
                    currentFontRegion.Width * scale.X,
                    currentFontRegion.Height * scale.Y
                );

                batcher.Draw(currentFontRegion.Subtexture, destRect, currentFontRegion.Subtexture.SourceRect, color,
                    rotation, Vector2.Zero, effect, depth);
            }
        }


	    /// <summary>
	    ///     old SpriteBatch drawing method. This should probably be removed since SpriteBatch was replaced by Batcher
	    /// </summary>
	    /// <param name="spriteBatch">Sprite batch.</param>
	    /// <param name="text">Text.</param>
	    /// <param name="position">Position.</param>
	    /// <param name="color">Color.</param>
	    /// <param name="rotation">Rotation.</param>
	    /// <param name="origin">Origin.</param>
	    /// <param name="scale">Scale.</param>
	    /// <param name="effect">Effect.</param>
	    /// <param name="depth">Depth.</param>
	    internal void DrawInto(SpriteBatch spriteBatch, ref FontCharacterSource text, Vector2 position, Color color,
            float rotation, Vector2 origin, Vector2 scale, SpriteEffects effect, float depth)
        {
            var flipAdjustment = Vector2.Zero;

            var flippedVert = (effect & SpriteEffects.FlipVertically) == SpriteEffects.FlipVertically;
            var flippedHorz = (effect & SpriteEffects.FlipHorizontally) == SpriteEffects.FlipHorizontally;

            if (flippedVert || flippedHorz)
            {
                Vector2 size;
                MeasureString(ref text, out size);

                if (flippedHorz)
                {
                    origin.X *= -1;
                    flipAdjustment.X = -size.X;
                }

                if (flippedVert)
                {
                    origin.Y *= -1;
                    flipAdjustment.Y = LineHeight - size.Y;
                }
            }


            var requiresTransformation = flippedHorz || flippedVert || rotation != 0f || scale != Vector2.One;
            if (requiresTransformation)
            {
                Matrix2D temp;
                Matrix2D.CreateTranslation(-origin.X, -origin.Y, out _transformationMatrix);
                Matrix2D.CreateScale(flippedHorz ? -scale.X : scale.X, flippedVert ? -scale.Y : scale.Y, out temp);
                Matrix2D.Multiply(ref _transformationMatrix, ref temp, out _transformationMatrix);
                Matrix2D.CreateTranslation(flipAdjustment.X, flipAdjustment.Y, out temp);
                Matrix2D.Multiply(ref temp, ref _transformationMatrix, out _transformationMatrix);
                Matrix2D.CreateRotation(rotation, out temp);
                Matrix2D.Multiply(ref _transformationMatrix, ref temp, out _transformationMatrix);
                Matrix2D.CreateTranslation(position.X, position.Y, out temp);
                Matrix2D.Multiply(ref _transformationMatrix, ref temp, out _transformationMatrix);
            }

            BitmapFontRegion currentFontRegion = null;
            var offset = requiresTransformation ? Vector2.Zero : position - origin;

            for (var i = 0; i < text.Length; ++i)
            {
                var c = text[i];
                if (c == '\r')
                    continue;

                if (c == '\n')
                {
                    offset.X = requiresTransformation ? 0f : position.X - origin.X;
                    offset.Y += LineHeight;
                    currentFontRegion = null;
                    continue;
                }

                if (currentFontRegion != null)
                    offset.X += Spacing + currentFontRegion.XAdvance;

                if (!_characterMap.TryGetValue(c, out currentFontRegion))
                    currentFontRegion = DefaultCharacterRegion;


                var p = offset;

                if (flippedHorz)
                    p.X += currentFontRegion.Width;
                p.X += currentFontRegion.XOffset;

                if (flippedVert)
                    p.Y += currentFontRegion.Height - LineHeight;
                p.Y += currentFontRegion.YOffset;

                // transform our point if we need to
                if (requiresTransformation)
                    Vector2Ext.Transform(ref p, ref _transformationMatrix, out p);

                var destRect = RectangleExt.FromFloats
                (
                    p.X, p.Y,
                    currentFontRegion.Width * scale.X,
                    currentFontRegion.Height * scale.Y
                );

                spriteBatch.Draw(currentFontRegion.Subtexture, destRect, currentFontRegion.Subtexture.SourceRect, color,
                    rotation, Vector2.Zero, effect, depth);
            }
        }

        #endregion
    }
}