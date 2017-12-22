using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.BitmapFonts;

namespace Nez
{
	/// <summary>
	///     provides a cached run of text for super fast text drawing. Note that this is only appropriate for text that doesnt
	///     change often
	///     and doesnt move.
	/// </summary>
	public class TextRun
    {
        private static readonly float[] CornerOffsetX = {0.0f, 1.0f, 0.0f, 1.0f};
        private static readonly float[] CornerOffsetY = {0.0f, 0.0f, 1.0f, 1.0f};
        private CharDetails[] _charDetails;
        private readonly Color _color = Color.White;
        private BitmapFont _font;


        private HorizontalAlign _horizontalAlign;
        private Vector2 _scale = Vector2.One;
        private Vector2 _size;
        private string _text;
        private VerticalAlign _verticalAlign;
        public Vector2 Position;
        public float Rotation;


        public TextRun(BitmapFont font)
        {
            _font = font;
            _horizontalAlign = HorizontalAlign.Left;
            _verticalAlign = VerticalAlign.Top;
        }

        public float Width => _size.X;
        public float Height => _size.Y;
        public Vector2 Origin { get; private set; }

	    /// <summary>
	    ///     text to draw
	    /// </summary>
	    /// <value>The text.</value>
	    public string Text
        {
            get => _text;
            set => SetText(value);
        }

	    /// <summary>
	    ///     horizontal alignment of the text
	    /// </summary>
	    /// <value>The horizontal origin.</value>
	    public HorizontalAlign HorizontalOrigin
        {
            get => _horizontalAlign;
            set => SetHorizontalAlign(value);
        }

	    /// <summary>
	    ///     vertical alignment of the text
	    /// </summary>
	    /// <value>The vertical origin.</value>
	    public VerticalAlign VerticalOrigin
        {
            get => _verticalAlign;
            set => SetVerticalAlign(value);
        }


        private void UpdateSize()
        {
            _size = _font.MeasureString(_text) * _scale;
            UpdateCentering();
        }


        private void UpdateCentering()
        {
            var newOrigin = Vector2.Zero;

            if (_horizontalAlign == HorizontalAlign.Left)
                newOrigin.X = 0;
            else if (_horizontalAlign == HorizontalAlign.Center)
                newOrigin.X = _size.X / 2;
            else
                newOrigin.X = _size.X;

            if (_verticalAlign == VerticalAlign.Top)
                newOrigin.Y = 0;
            else if (_verticalAlign == VerticalAlign.Center)
                newOrigin.Y = _size.Y / 2;
            else
                newOrigin.Y = _size.Y;

            Origin = new Vector2((int) (newOrigin.X * _scale.X), (int) (newOrigin.Y * _scale.Y));
        }


	    /// <summary>
	    ///     compiles the text into raw verts/texture coordinates. This method must be called anytime text or any other
	    ///     properties are
	    ///     changed.
	    /// </summary>
	    public void Compile()
        {
            _charDetails = new CharDetails[_text.Length];
            BitmapFontRegion currentFontRegion = null;
            var effects = (byte) SpriteEffects.None;

            var transformationMatrix = Matrix2D.Identity;
            var requiresTransformation = Rotation != 0f || _scale != Vector2.One;
            if (requiresTransformation)
            {
                Matrix2D temp;
                Matrix2D.CreateTranslation(-Origin.X, -Origin.Y, out transformationMatrix);
                Matrix2D.CreateScale(_scale.X, _scale.Y, out temp);
                Matrix2D.Multiply(ref transformationMatrix, ref temp, out transformationMatrix);
                Matrix2D.CreateRotation(Rotation, out temp);
                Matrix2D.Multiply(ref transformationMatrix, ref temp, out transformationMatrix);
                Matrix2D.CreateTranslation(Position.X, Position.Y, out temp);
                Matrix2D.Multiply(ref transformationMatrix, ref temp, out transformationMatrix);
            }

            var offset = requiresTransformation ? Vector2.Zero : Position - Origin;

            for (var i = 0; i < _text.Length; ++i)
            {
                _charDetails[i].Initialize();
                _charDetails[i].Color = _color;

                var c = _text[i];
                if (c == '\n')
                {
                    offset.X = requiresTransformation ? 0f : Position.X - Origin.X;
                    offset.Y += _font.LineHeight;
                    currentFontRegion = null;
                    continue;
                }

                if (currentFontRegion != null)
                    offset.X += _font.Spacing + currentFontRegion.XAdvance;

                currentFontRegion = _font.FontRegionForChar(c, true);
                var p = offset;
                p.X += currentFontRegion.XOffset;
                p.Y += currentFontRegion.YOffset;

                // transform our point if we need to
                if (requiresTransformation)
                    Vector2Ext.Transform(ref p, ref transformationMatrix, out p);

                var destination = new Vector4(p.X, p.Y, currentFontRegion.Width * _scale.X,
                    currentFontRegion.Height * _scale.Y);
                _charDetails[i].Texture = currentFontRegion.Subtexture.Texture2D;


                // Batcher calculations
                var sourceRectangle = currentFontRegion.Subtexture.SourceRect;
                float sourceX, sourceY, sourceW, sourceH;
                var destW = destination.Z;
                var destH = destination.W;

                // calculate uvs
                var inverseTexW = 1.0f / currentFontRegion.Subtexture.Texture2D.Width;
                var inverseTexH = 1.0f / currentFontRegion.Subtexture.Texture2D.Height;

                sourceX = sourceRectangle.X * inverseTexW;
                sourceY = sourceRectangle.Y * inverseTexH;
                sourceW = Math.Max(sourceRectangle.Width, float.Epsilon) * inverseTexW;
                sourceH = Math.Max(sourceRectangle.Height, float.Epsilon) * inverseTexH;

                // Rotation Calculations
                float rotationMatrix1X;
                float rotationMatrix1Y;
                float rotationMatrix2X;
                float rotationMatrix2Y;
                if (!Mathf.WithinEpsilon(Rotation, 0.0f))
                {
                    var sin = Mathf.Sin(Rotation);
                    var cos = Mathf.Cos(Rotation);
                    rotationMatrix1X = cos;
                    rotationMatrix1Y = sin;
                    rotationMatrix2X = -sin;
                    rotationMatrix2Y = cos;
                }
                else
                {
                    rotationMatrix1X = 1.0f;
                    rotationMatrix1Y = 0.0f;
                    rotationMatrix2X = 0.0f;
                    rotationMatrix2Y = 1.0f;
                }

                // Calculate vertices, finally.
                // top-left
                _charDetails[i].Verts[0].X = rotationMatrix2X + rotationMatrix1X + destination.X - 1;
                _charDetails[i].Verts[0].Y = rotationMatrix2Y + rotationMatrix1Y + destination.Y - 1;

                // top-right
                var cornerX = CornerOffsetX[1] * destW;
                var cornerY = CornerOffsetY[1] * destH;
                _charDetails[i].Verts[1].X = rotationMatrix2X * cornerY +
                                             rotationMatrix1X * cornerX +
                                             destination.X;
                _charDetails[i].Verts[1].Y = rotationMatrix2Y * cornerY +
                                             rotationMatrix1Y * cornerX +
                                             destination.Y;

                // bottom-left
                cornerX = CornerOffsetX[2] * destW;
                cornerY = CornerOffsetY[2] * destH;
                _charDetails[i].Verts[2].X = rotationMatrix2X * cornerY +
                                             rotationMatrix1X * cornerX +
                                             destination.X;
                _charDetails[i].Verts[2].Y = rotationMatrix2Y * cornerY +
                                             rotationMatrix1Y * cornerX +
                                             destination.Y;

                // bottom-right
                cornerX = CornerOffsetX[3] * destW;
                cornerY = CornerOffsetY[3] * destH;
                _charDetails[i].Verts[3].X = rotationMatrix2X * cornerY +
                                             rotationMatrix1X * cornerX +
                                             destination.X;
                _charDetails[i].Verts[3].Y = rotationMatrix2Y * cornerY +
                                             rotationMatrix1Y * cornerX +
                                             destination.Y;


                // texture coordintes
                _charDetails[i].TexCoords[0].X = CornerOffsetX[0 ^ effects] * sourceW + sourceX;
                _charDetails[i].TexCoords[0].Y = CornerOffsetY[0 ^ effects] * sourceH + sourceY;
                _charDetails[i].TexCoords[1].X = CornerOffsetX[1 ^ effects] * sourceW + sourceX;
                _charDetails[i].TexCoords[1].Y = CornerOffsetY[1 ^ effects] * sourceH + sourceY;
                _charDetails[i].TexCoords[2].X = CornerOffsetX[2 ^ effects] * sourceW + sourceX;
                _charDetails[i].TexCoords[2].Y = CornerOffsetY[2 ^ effects] * sourceH + sourceY;
                _charDetails[i].TexCoords[3].X = CornerOffsetX[3 ^ effects] * sourceW + sourceX;
                _charDetails[i].TexCoords[3].Y = CornerOffsetY[3 ^ effects] * sourceH + sourceY;
            }
        }


        public void Render(Graphics graphics)
        {
            for (var i = 0; i < _charDetails.Length; i++)
                graphics.Batcher.DrawRaw(_charDetails[i].Texture, _charDetails[i].Verts, _charDetails[i].TexCoords,
                    _charDetails[i].Color);
        }

        private struct CharDetails
        {
            public Texture2D Texture;
            public Vector3[] Verts;
            public Vector2[] TexCoords;
            public Color Color;

            public void Initialize()
            {
                Verts = new Vector3[4];
                TexCoords = new Vector2[4];
            }
        }


        #region Fluent setters

        public TextRun SetFont(BitmapFont font)
        {
            _font = font;
            UpdateSize();
            return this;
        }


        public TextRun SetText(string text)
        {
            _text = text;
            UpdateSize();
            UpdateCentering();
            return this;
        }


        public TextRun SetHorizontalAlign(HorizontalAlign hAlign)
        {
            _horizontalAlign = hAlign;
            UpdateCentering();
            return this;
        }


        public TextRun SetVerticalAlign(VerticalAlign vAlign)
        {
            _verticalAlign = vAlign;
            UpdateCentering();
            return this;
        }

        #endregion
    }
}