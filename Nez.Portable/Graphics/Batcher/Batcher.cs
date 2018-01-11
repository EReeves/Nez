// based on the FNA SpriteBatch implementation by Ethan Lee: https://github.com/FNA-XNA/FNA

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.Debug;
using Nez.Graphics.Textures;
using Nez.Maths;
using SpriteEffect = Nez.Graphics.Effects.SpriteEffect;

namespace Nez.Graphics.Batcher
{
    public class Batcher : GraphicsResource
    {
        public Batcher(GraphicsDevice graphicsDevice)
        {
            Assert.IsTrue(graphicsDevice != null);

            this.GraphicsDevice = graphicsDevice;

            _vertexInfo = new VertexPositionColorTexture4[MaxSprites];
            _textureInfo = new Texture2D[MaxSprites];
            _vertexBuffer = new DynamicVertexBuffer(graphicsDevice, typeof(VertexPositionColorTexture), MaxVertices,
                BufferUsage.WriteOnly);
            _indexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.SixteenBits, MaxIndices,
                BufferUsage.WriteOnly);
            _indexBuffer.SetData(IndexData);

            _spriteEffect = new SpriteEffect();
            _spriteEffectPass = _spriteEffect.CurrentTechnique.Passes[0];

            _projectionMatrix = new Matrix(
                0f, //(float)( 2.0 / (double)viewport.Width ) is the actual value we will use
                0.0f,
                0.0f,
                0.0f,
                0.0f,
                0f, //(float)( -2.0 / (double)viewport.Height ) is the actual value we will use
                0.0f,
                0.0f,
                0.0f,
                0.0f,
                1.0f,
                0.0f,
                -1.0f,
                1.0f,
                0.0f,
                1.0f
            );
        }

        public Matrix TransformMatrix => _transformMatrix;


        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed && disposing)
            {
                _spriteEffect.Dispose();
                _indexBuffer.Dispose();
                _vertexBuffer.Dispose();
            }
            base.Dispose(disposing);
        }


        [Obsolete("SpriteFont is too locked down to use directly. Wrap it in a NezSpriteFont")]
        public void DrawString(SpriteFont spriteFont, string text, Vector2 position, Color color, float rotation,
            Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
        {
            throw new NotImplementedException(
                "SpriteFont is too locked down to use directly. Wrap it in a NezSpriteFont");
        }


        private static short[] GenerateIndexArray()
        {
            var result = new short[MaxIndices];
            for (int i = 0, j = 0; i < MaxIndices; i += 6, j += 4)
            {
                result[i] = (short) j;
                result[i + 1] = (short) (j + 1);
                result[i + 2] = (short) (j + 2);
                result[i + 3] = (short) (j + 3);
                result[i + 4] = (short) (j + 2);
                result[i + 5] = (short) (j + 1);
            }
            return result;
        }


        #region Sprite Data Container Class

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct VertexPositionColorTexture4 : IVertexType
        {
            public const int RealStride = 96;

            VertexDeclaration IVertexType.VertexDeclaration => throw new NotImplementedException();

            public Vector3 position0;
            public Color color0;
            public Vector2 textureCoordinate0;
            public Vector3 position1;
            public Color color1;
            public Vector2 textureCoordinate1;
            public Vector3 position2;
            public Color color2;
            public Vector2 textureCoordinate2;
            public Vector3 position3;
            public Color color3;
            public Vector2 textureCoordinate3;
        }

        #endregion

        #region variables

        // Buffer objects used for actual drawing
        private readonly DynamicVertexBuffer _vertexBuffer;

        private readonly IndexBuffer _indexBuffer;

        // Local data stored before buffering to GPU
        private readonly VertexPositionColorTexture4[] _vertexInfo;

        private readonly Texture2D[] _textureInfo;

        // Default SpriteEffect
        private readonly SpriteEffect _spriteEffect;

        private readonly EffectPass _spriteEffectPass;

        // Tracks Begin/End calls
        private bool _beginCalled;

        // Keep render state for non-Immediate modes.
        private BlendState _blendState;

        private SamplerState _samplerState;
        private DepthStencilState _depthStencilState;
        private RasterizerState _rasterizerState;
        private bool _disableBatching;

        // How many sprites are in the current batch?
        private int _numSprites;

        // Matrix to be used when creating the projection matrix
        private Matrix _transformMatrix;

        // Matrix used internally to calculate the cameras projection
        private Matrix _projectionMatrix;

        // this is the calculated MatrixTransform parameter in sprite shaders
        private Matrix _matrixTransformMatrix;

        // User-provided Effect, if applicable
        private Effect _customEffect;

        #endregion


        #region static variables and constants

        private const int MaxSprites = 2048;
        private const int MaxVertices = MaxSprites * 4;
        private const int MaxIndices = MaxSprites * 6;

        // Used to calculate texture coordinates
        private static readonly float[] CornerOffsetX = {0.0f, 1.0f, 0.0f, 1.0f};

        private static readonly float[] CornerOffsetY = {0.0f, 0.0f, 1.0f, 1.0f};
        private static readonly short[] IndexData = GenerateIndexArray();

        #endregion


        #region Public begin/end methods

        public void Begin()
        {
            Begin(BlendState.AlphaBlend, Core.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, Matrix.Identity, false);
        }


        public void Begin(Effect effect)
        {
            Begin(BlendState.AlphaBlend, Core.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, effect, Matrix.Identity, false);
        }


        public void Begin(Material material)
        {
            Begin(material.BlendState, material.SamplerState, material.DepthStencilState,
                RasterizerState.CullCounterClockwise, material.Effect);
        }


        public void Begin(Matrix transformationMatrix)
        {
            Begin(BlendState.AlphaBlend, Core.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, transformationMatrix, false);
        }


        public void Begin(BlendState blendState)
        {
            Begin(blendState, Core.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise,
                null, Matrix.Identity, false);
        }


        public void Begin(Material material, Matrix transformationMatrix)
        {
            Begin(material.BlendState, material.SamplerState, material.DepthStencilState,
                RasterizerState.CullCounterClockwise, material.Effect, transformationMatrix, false);
        }


        public void Begin(BlendState blendState, SamplerState samplerState, DepthStencilState depthStencilState,
            RasterizerState rasterizerState)
        {
            Begin(
                blendState,
                samplerState,
                depthStencilState,
                rasterizerState,
                null,
                Matrix.Identity,
                false
            );
        }


        public void Begin(BlendState blendState, SamplerState samplerState, DepthStencilState depthStencilState,
            RasterizerState rasterizerState, Effect effect)
        {
            Begin(
                blendState,
                samplerState,
                depthStencilState,
                rasterizerState,
                effect,
                Matrix.Identity,
                false
            );
        }


        public void Begin(BlendState blendState, SamplerState samplerState, DepthStencilState depthStencilState,
            RasterizerState rasterizerState,
            Effect effect, Matrix transformationMatrix)
        {
            Begin(
                blendState,
                samplerState,
                depthStencilState,
                rasterizerState,
                effect,
                transformationMatrix,
                false
            );
        }


        public void Begin(BlendState blendState, SamplerState samplerState, DepthStencilState depthStencilState,
            RasterizerState rasterizerState,
            Effect effect, Matrix transformationMatrix, bool disableBatching)
        {
            Assert.IsFalse(_beginCalled,
                "Begin has been called before calling End after the last call to Begin. Begin cannot be called again until End has been successfully called.");
            _beginCalled = true;

            _blendState = blendState ?? BlendState.AlphaBlend;
            _samplerState = samplerState ?? Core.DefaultSamplerState;
            _depthStencilState = depthStencilState ?? DepthStencilState.None;
            _rasterizerState = rasterizerState ?? RasterizerState.CullCounterClockwise;

            _customEffect = effect;
            _transformMatrix = transformationMatrix;
            _disableBatching = disableBatching;

            if (_disableBatching)
                PrepRenderState();
        }


        public void End()
        {
            Assert.IsTrue(_beginCalled,
                "End was called, but Begin has not yet been called. You must call Begin successfully before you can call End.");
            _beginCalled = false;

            if (!_disableBatching)
                FlushBatch();

            _customEffect = null;
        }

        #endregion


        #region Public draw methods

        public void Draw(Texture2D texture, Vector2 position)
        {
            CheckBegin();
            PushSprite(texture, null, position.X, position.Y, 1.0f, 1.0f,
                Color.White, Vector2.Zero, 0.0f, 0.0f, 0, false, 0, 0, 0, 0);
        }


        public void Draw(Texture2D texture, Vector2 position, Color color)
        {
            CheckBegin();
            PushSprite(texture, null, position.X, position.Y, 1.0f, 1.0f,
                color, Vector2.Zero, 0.0f, 0.0f, 0, false, 0, 0, 0, 0);
        }


        public void Draw(Texture2D texture, Rectangle destinationRectangle)
        {
            CheckBegin();
            PushSprite(texture, null, destinationRectangle.X, destinationRectangle.Y, destinationRectangle.Width,
                destinationRectangle.Height,
                Color.White, Vector2.Zero, 0.0f, 0.0f, 0, true, 0, 0, 0, 0);
        }


        public void Draw(Texture2D texture, Rectangle destinationRectangle, Color color)
        {
            CheckBegin();
            PushSprite(texture, null, destinationRectangle.X, destinationRectangle.Y, destinationRectangle.Width,
                destinationRectangle.Height,
                color, Vector2.Zero, 0.0f, 0.0f, 0, true, 0, 0, 0, 0);
        }


        public void Draw(Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color)
        {
            CheckBegin();
            PushSprite(texture, sourceRectangle, destinationRectangle.X, destinationRectangle.Y,
                destinationRectangle.Width, destinationRectangle.Height,
                color, Vector2.Zero, 0.0f, 0.0f, 0, true, 0, 0, 0, 0);
        }

        public void Draw(Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color,
            SpriteEffects effects)
        {
            CheckBegin();
            PushSprite(texture, sourceRectangle, destinationRectangle.X, destinationRectangle.Y,
                destinationRectangle.Width, destinationRectangle.Height,
                color, Vector2.Zero, 0.0f, 0.0f, (byte) (effects & (SpriteEffects) 0x03), true, 0, 0, 0, 0);
        }


        public void Draw(
            Texture2D texture,
            Rectangle destinationRectangle,
            Rectangle? sourceRectangle,
            Color color,
            float rotation,
            SpriteEffects effects,
            float layerDepth,
            float skewTopX, float skewBottomX, float skewLeftY, float skewRightY
        )
        {
            CheckBegin();
            PushSprite(
                texture,
                sourceRectangle,
                destinationRectangle.X,
                destinationRectangle.Y,
                destinationRectangle.Width,
                destinationRectangle.Height,
                color,
                Vector2.Zero,
                rotation,
                layerDepth,
                (byte) (effects & (SpriteEffects) 0x03),
                true,
                skewTopX, skewBottomX, skewLeftY, skewRightY
            );
        }


        public void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color)
        {
            CheckBegin();
            PushSprite(
                texture,
                sourceRectangle,
                position.X,
                position.Y,
                1.0f,
                1.0f,
                color,
                Vector2.Zero,
                0.0f,
                0.0f,
                0,
                false,
                0, 0, 0, 0
            );
        }


        public void Draw(
            Texture2D texture,
            Vector2 position,
            Rectangle? sourceRectangle,
            Color color,
            float rotation,
            Vector2 origin,
            float scale,
            SpriteEffects effects,
            float layerDepth
        )
        {
            CheckBegin();
            PushSprite(
                texture,
                sourceRectangle,
                position.X,
                position.Y,
                scale,
                scale,
                color,
                origin,
                rotation,
                layerDepth,
                (byte) (effects & (SpriteEffects) 0x03),
                false,
                0, 0, 0, 0
            );
        }


        public void Draw(
            Subtexture subtexture,
            Vector2 position,
            Color color,
            float rotation,
            Vector2 origin,
            float scale,
            SpriteEffects effects,
            float layerDepth
        )
        {
            CheckBegin();
            PushSprite(
                subtexture,
                position.X,
                position.Y,
                scale,
                scale,
                color,
                origin,
                rotation,
                layerDepth,
                (byte) (effects & (SpriteEffects) 0x03),
                0, 0, 0, 0
            );
        }


        public void Draw(
            Texture2D texture,
            Vector2 position,
            Rectangle? sourceRectangle,
            Color color,
            float rotation,
            Vector2 origin,
            Vector2 scale,
            SpriteEffects effects,
            float layerDepth
        )
        {
            CheckBegin();
            PushSprite(
                texture,
                sourceRectangle,
                position.X,
                position.Y,
                scale.X,
                scale.Y,
                color,
                origin,
                rotation,
                layerDepth,
                (byte) (effects & (SpriteEffects) 0x03),
                false,
                0, 0, 0, 0
            );
        }


        public void Draw(
            Subtexture subtexture,
            Vector2 position,
            Color color,
            float rotation,
            Vector2 origin,
            Vector2 scale,
            SpriteEffects effects,
            float layerDepth
        )
        {
            CheckBegin();
            PushSprite(
                subtexture,
                position.X,
                position.Y,
                scale.X,
                scale.Y,
                color,
                origin,
                rotation,
                layerDepth,
                (byte) (effects & (SpriteEffects) 0x03),
                0, 0, 0, 0
            );
        }


        public void Draw(
            Texture2D texture,
            Vector2 position,
            Rectangle? sourceRectangle,
            Color color,
            float rotation,
            Vector2 origin,
            Vector2 scale,
            SpriteEffects effects,
            float layerDepth,
            float skewTopX, float skewBottomX, float skewLeftY, float skewRightY
        )
        {
            CheckBegin();
            PushSprite(
                texture,
                sourceRectangle,
                position.X,
                position.Y,
                scale.X,
                scale.Y,
                color,
                origin,
                rotation,
                layerDepth,
                (byte) (effects & (SpriteEffects) 0x03),
                false,
                skewTopX, skewBottomX, skewLeftY, skewRightY
            );
        }


        public void Draw(
            Texture2D texture,
            Rectangle destinationRectangle,
            Rectangle? sourceRectangle,
            Color color,
            float rotation,
            Vector2 origin,
            SpriteEffects effects,
            float layerDepth
        )
        {
            CheckBegin();
            PushSprite(
                texture,
                sourceRectangle,
                destinationRectangle.X,
                destinationRectangle.Y,
                destinationRectangle.Width,
                destinationRectangle.Height,
                color,
                origin,
                rotation,
                layerDepth,
                (byte) (effects & (SpriteEffects) 0x03),
                true,
                0, 0, 0, 0
            );
        }


	    /// <summary>
	    ///     direct access to setting vert positions, UVs and colors. The order of elements is top-left, top-right, bottom-left,
	    ///     bottom-right
	    /// </summary>
	    /// <returns>The raw.</returns>
	    /// <param name="texture">Texture.</param>
	    /// <param name="verts">Verts.</param>
	    /// <param name="textureCoords">Texture coords.</param>
	    /// <param name="colors">Colors.</param>
	    public void DrawRaw(Texture2D texture, Vector3[] verts, Vector2[] textureCoords, Color[] colors)
        {
            Assert.IsTrue(verts.Length == 4, "there must be only 4 verts");
            Assert.IsTrue(textureCoords.Length == 4, "there must be only 4 texture coordinates");
            Assert.IsTrue(colors.Length == 4, "there must be only 4 colors");

            // we're out of space, flush
            if (_numSprites >= MaxSprites)
                FlushBatch();

            _vertexInfo[_numSprites].position0 = verts[0];
            _vertexInfo[_numSprites].position1 = verts[1];
            _vertexInfo[_numSprites].position2 = verts[2];
            _vertexInfo[_numSprites].position3 = verts[3];

            _vertexInfo[_numSprites].textureCoordinate0 = textureCoords[0];
            _vertexInfo[_numSprites].textureCoordinate1 = textureCoords[1];
            _vertexInfo[_numSprites].textureCoordinate2 = textureCoords[2];
            _vertexInfo[_numSprites].textureCoordinate3 = textureCoords[3];

            _vertexInfo[_numSprites].color0 = colors[0];
            _vertexInfo[_numSprites].color1 = colors[1];
            _vertexInfo[_numSprites].color2 = colors[2];
            _vertexInfo[_numSprites].color3 = colors[3];

            if (_disableBatching)
            {
                _vertexBuffer.SetData(0, _vertexInfo, 0, 1, VertexPositionColorTexture4.RealStride,
                    SetDataOptions.None);
                DrawPrimitives(texture, 0, 1);
            }
            else
            {
                _textureInfo[_numSprites] = texture;
                _numSprites += 1;
            }
        }


	    /// <summary>
	    ///     direct access to setting vert positions, UVs and colors. The order of elements is top-left, top-right, bottom-left,
	    ///     bottom-right
	    /// </summary>
	    /// <returns>The raw.</returns>
	    /// <param name="texture">Texture.</param>
	    /// <param name="verts">Verts.</param>
	    /// <param name="textureCoords">Texture coords.</param>
	    /// <param name="color">Color.</param>
	    public void DrawRaw(Texture2D texture, Vector3[] verts, Vector2[] textureCoords, Color color)
        {
            Assert.IsTrue(verts.Length == 4, "there must be only 4 verts");
            Assert.IsTrue(textureCoords.Length == 4, "there must be only 4 texture coordinates");

            // we're out of space, flush
            if (_numSprites >= MaxSprites)
                FlushBatch();

            _vertexInfo[_numSprites].position0 = verts[0];
            _vertexInfo[_numSprites].position1 = verts[1];
            _vertexInfo[_numSprites].position2 = verts[2];
            _vertexInfo[_numSprites].position3 = verts[3];

            _vertexInfo[_numSprites].textureCoordinate0 = textureCoords[0];
            _vertexInfo[_numSprites].textureCoordinate1 = textureCoords[1];
            _vertexInfo[_numSprites].textureCoordinate2 = textureCoords[2];
            _vertexInfo[_numSprites].textureCoordinate3 = textureCoords[3];

            _vertexInfo[_numSprites].color0 = color;
            _vertexInfo[_numSprites].color1 = color;
            _vertexInfo[_numSprites].color2 = color;
            _vertexInfo[_numSprites].color3 = color;

            if (_disableBatching)
            {
                _vertexBuffer.SetData(0, _vertexInfo, 0, 1, VertexPositionColorTexture4.RealStride,
                    SetDataOptions.None);
                DrawPrimitives(texture, 0, 1);
            }
            else
            {
                _textureInfo[_numSprites] = texture;
                _numSprites += 1;
            }
        }

        #endregion


        #region Methods

	    /// <summary>
	    ///     the meat of the Batcher. This is where it all goes down
	    /// </summary>
	    [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void PushSprite(Texture2D texture, Rectangle? sourceRectangle, float destinationX, float destinationY,
            float destinationW, float destinationH, Color color, Vector2 origin,
            float rotation, float depth, byte effects, bool destSizeInPixels, float skewTopX, float skewBottomX,
            float skewLeftY, float skewRightY)
        {
            // out of space, flush
            if (_numSprites >= MaxSprites)
                FlushBatch();

            // Source/Destination/Origin Calculations
            float sourceX, sourceY, sourceW, sourceH;
            float originX, originY;
            if (sourceRectangle.HasValue)
            {
                var inverseTexW = 1.0f / texture.Width;
                var inverseTexH = 1.0f / texture.Height;

                sourceX = sourceRectangle.Value.X * inverseTexW;
                sourceY = sourceRectangle.Value.Y * inverseTexH;
                sourceW = sourceRectangle.Value.Width * inverseTexW;
                sourceH = sourceRectangle.Value.Height * inverseTexH;

                originX = origin.X / sourceW * inverseTexW;
                originY = origin.Y / sourceH * inverseTexH;

                if (!destSizeInPixels)
                {
                    destinationW *= sourceRectangle.Value.Width;
                    destinationH *= sourceRectangle.Value.Height;
                }
            }
            else
            {
                sourceX = 0.0f;
                sourceY = 0.0f;
                sourceW = 1.0f;
                sourceH = 1.0f;

                originX = origin.X * (1.0f / texture.Width);
                originY = origin.Y * (1.0f / texture.Height);

                if (!destSizeInPixels)
                {
                    destinationW *= texture.Width;
                    destinationH *= texture.Height;
                }
            }

            // Rotation Calculations
            float rotationMatrix1X;
            float rotationMatrix1Y;
            float rotationMatrix2X;
            float rotationMatrix2Y;
            if (!Mathf.WithinEpsilon(rotation, 0))
            {
                var sin = Mathf.Sin(rotation);
                var cos = Mathf.Cos(rotation);
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


            // flip our skew values if we have a flipped sprite
            if (effects != 0)
            {
                skewTopX *= -1;
                skewBottomX *= -1;
                skewLeftY *= -1;
                skewRightY *= -1;
            }

            // calculate vertices
            // top-left
            var cornerX = (CornerOffsetX[0] - originX) * destinationW + skewTopX;
            var cornerY = (CornerOffsetY[0] - originY) * destinationH - skewLeftY;
            _vertexInfo[_numSprites].position0.X = rotationMatrix2X * cornerY +
                                                   rotationMatrix1X * cornerX +
                                                   destinationX;
            _vertexInfo[_numSprites].position0.Y = rotationMatrix2Y * cornerY +
                                                   rotationMatrix1Y * cornerX +
                                                   destinationY;

            // top-right
            cornerX = (CornerOffsetX[1] - originX) * destinationW + skewTopX;
            cornerY = (CornerOffsetY[1] - originY) * destinationH - skewRightY;
            _vertexInfo[_numSprites].position1.X = rotationMatrix2X * cornerY +
                                                   rotationMatrix1X * cornerX +
                                                   destinationX;
            _vertexInfo[_numSprites].position1.Y = rotationMatrix2Y * cornerY +
                                                   rotationMatrix1Y * cornerX +
                                                   destinationY;

            // bottom-left
            cornerX = (CornerOffsetX[2] - originX) * destinationW + skewBottomX;
            cornerY = (CornerOffsetY[2] - originY) * destinationH - skewLeftY;
            _vertexInfo[_numSprites].position2.X = rotationMatrix2X * cornerY +
                                                   rotationMatrix1X * cornerX +
                                                   destinationX;
            _vertexInfo[_numSprites].position2.Y = rotationMatrix2Y * cornerY +
                                                   rotationMatrix1Y * cornerX +
                                                   destinationY;

            // bottom-right
            cornerX = (CornerOffsetX[3] - originX) * destinationW + skewBottomX;
            cornerY = (CornerOffsetY[3] - originY) * destinationH - skewRightY;
            _vertexInfo[_numSprites].position3.X = rotationMatrix2X * cornerY +
                                                   rotationMatrix1X * cornerX +
                                                   destinationX;
            _vertexInfo[_numSprites].position3.Y = rotationMatrix2Y * cornerY +
                                                   rotationMatrix1Y * cornerX +
                                                   destinationY;

            _vertexInfo[_numSprites].textureCoordinate0.X = CornerOffsetX[0 ^ effects] * sourceW + sourceX;
            _vertexInfo[_numSprites].textureCoordinate0.Y = CornerOffsetY[0 ^ effects] * sourceH + sourceY;
            _vertexInfo[_numSprites].textureCoordinate1.X = CornerOffsetX[1 ^ effects] * sourceW + sourceX;
            _vertexInfo[_numSprites].textureCoordinate1.Y = CornerOffsetY[1 ^ effects] * sourceH + sourceY;
            _vertexInfo[_numSprites].textureCoordinate2.X = CornerOffsetX[2 ^ effects] * sourceW + sourceX;
            _vertexInfo[_numSprites].textureCoordinate2.Y = CornerOffsetY[2 ^ effects] * sourceH + sourceY;
            _vertexInfo[_numSprites].textureCoordinate3.X = CornerOffsetX[3 ^ effects] * sourceW + sourceX;
            _vertexInfo[_numSprites].textureCoordinate3.Y = CornerOffsetY[3 ^ effects] * sourceH + sourceY;
            _vertexInfo[_numSprites].position0.Z = depth;
            _vertexInfo[_numSprites].position1.Z = depth;
            _vertexInfo[_numSprites].position2.Z = depth;
            _vertexInfo[_numSprites].position3.Z = depth;
            _vertexInfo[_numSprites].color0 = color;
            _vertexInfo[_numSprites].color1 = color;
            _vertexInfo[_numSprites].color2 = color;
            _vertexInfo[_numSprites].color3 = color;

            if (_disableBatching)
            {
                _vertexBuffer.SetData(0, _vertexInfo, 0, 1, VertexPositionColorTexture4.RealStride,
                    SetDataOptions.None);
                DrawPrimitives(texture, 0, 1);
            }
            else
            {
                _textureInfo[_numSprites] = texture;
                _numSprites += 1;
            }
        }


	    /// <summary>
	    ///     Subtexture alternative to the old SpriteBatch pushSprite
	    /// </summary>
	    [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void PushSprite(Subtexture subtexture, float destinationX, float destinationY, float destinationW,
            float destinationH, Color color, Vector2 origin,
            float rotation, float depth, byte effects, float skewTopX, float skewBottomX, float skewLeftY,
            float skewRightY)
        {
            // out of space, flush
            if (_numSprites >= MaxSprites)
                FlushBatch();

            // Source/Destination/Origin Calculations. destinationW/H is the scale value so we multiply by the size of the texture region
            var originX = origin.X / subtexture.Uvs.Width / subtexture.Texture2D.Width;
            var originY = origin.Y / subtexture.Uvs.Height / subtexture.Texture2D.Height;
            destinationW *= subtexture.SourceRect.Width;
            destinationH *= subtexture.SourceRect.Height;

            // Rotation Calculations
            float rotationMatrix1X;
            float rotationMatrix1Y;
            float rotationMatrix2X;
            float rotationMatrix2Y;
            if (!Mathf.WithinEpsilon(rotation, 0))
            {
                var sin = Mathf.Sin(rotation);
                var cos = Mathf.Cos(rotation);
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


            // flip our skew values if we have a flipped sprite
            if (effects != 0)
            {
                skewTopX *= -1;
                skewBottomX *= -1;
                skewLeftY *= -1;
                skewRightY *= -1;
            }

            // calculate vertices
            // top-left
            var cornerX = (CornerOffsetX[0] - originX) * destinationW + skewTopX;
            var cornerY = (CornerOffsetY[0] - originY) * destinationH - skewLeftY;
            _vertexInfo[_numSprites].position0.X = rotationMatrix2X * cornerY +
                                                   rotationMatrix1X * cornerX +
                                                   destinationX;
            _vertexInfo[_numSprites].position0.Y = rotationMatrix2Y * cornerY +
                                                   rotationMatrix1Y * cornerX +
                                                   destinationY;

            // top-right
            cornerX = (CornerOffsetX[1] - originX) * destinationW + skewTopX;
            cornerY = (CornerOffsetY[1] - originY) * destinationH - skewRightY;
            _vertexInfo[_numSprites].position1.X = rotationMatrix2X * cornerY +
                                                   rotationMatrix1X * cornerX +
                                                   destinationX;
            _vertexInfo[_numSprites].position1.Y = rotationMatrix2Y * cornerY +
                                                   rotationMatrix1Y * cornerX +
                                                   destinationY;

            // bottom-left
            cornerX = (CornerOffsetX[2] - originX) * destinationW + skewBottomX;
            cornerY = (CornerOffsetY[2] - originY) * destinationH - skewLeftY;
            _vertexInfo[_numSprites].position2.X = rotationMatrix2X * cornerY +
                                                   rotationMatrix1X * cornerX +
                                                   destinationX;
            _vertexInfo[_numSprites].position2.Y = rotationMatrix2Y * cornerY +
                                                   rotationMatrix1Y * cornerX +
                                                   destinationY;

            // bottom-right
            cornerX = (CornerOffsetX[3] - originX) * destinationW + skewBottomX;
            cornerY = (CornerOffsetY[3] - originY) * destinationH - skewRightY;
            _vertexInfo[_numSprites].position3.X = rotationMatrix2X * cornerY +
                                                   rotationMatrix1X * cornerX +
                                                   destinationX;
            _vertexInfo[_numSprites].position3.Y = rotationMatrix2Y * cornerY +
                                                   rotationMatrix1Y * cornerX +
                                                   destinationY;

            _vertexInfo[_numSprites].textureCoordinate0.X =
                CornerOffsetX[0 ^ effects] * subtexture.Uvs.Width + subtexture.Uvs.X;
            _vertexInfo[_numSprites].textureCoordinate0.Y =
                CornerOffsetY[0 ^ effects] * subtexture.Uvs.Height + subtexture.Uvs.Y;
            _vertexInfo[_numSprites].textureCoordinate1.X =
                CornerOffsetX[1 ^ effects] * subtexture.Uvs.Width + subtexture.Uvs.X;
            _vertexInfo[_numSprites].textureCoordinate1.Y =
                CornerOffsetY[1 ^ effects] * subtexture.Uvs.Height + subtexture.Uvs.Y;
            _vertexInfo[_numSprites].textureCoordinate2.X =
                CornerOffsetX[2 ^ effects] * subtexture.Uvs.Width + subtexture.Uvs.X;
            _vertexInfo[_numSprites].textureCoordinate2.Y =
                CornerOffsetY[2 ^ effects] * subtexture.Uvs.Height + subtexture.Uvs.Y;
            _vertexInfo[_numSprites].textureCoordinate3.X =
                CornerOffsetX[3 ^ effects] * subtexture.Uvs.Width + subtexture.Uvs.X;
            _vertexInfo[_numSprites].textureCoordinate3.Y =
                CornerOffsetY[3 ^ effects] * subtexture.Uvs.Height + subtexture.Uvs.Y;
            _vertexInfo[_numSprites].position0.Z = depth;
            _vertexInfo[_numSprites].position1.Z = depth;
            _vertexInfo[_numSprites].position2.Z = depth;
            _vertexInfo[_numSprites].position3.Z = depth;
            _vertexInfo[_numSprites].color0 = color;
            _vertexInfo[_numSprites].color1 = color;
            _vertexInfo[_numSprites].color2 = color;
            _vertexInfo[_numSprites].color3 = color;

            if (_disableBatching)
            {
                _vertexBuffer.SetData(0, _vertexInfo, 0, 1, VertexPositionColorTexture4.RealStride,
                    SetDataOptions.None);
                DrawPrimitives(subtexture, 0, 1);
            }
            else
            {
                _textureInfo[_numSprites] = subtexture;
                _numSprites += 1;
            }
        }


        public void FlushBatch()
        {
            if (_numSprites == 0)
                return;

            var offset = 0;
            Texture2D curTexture = null;

            PrepRenderState();

            _vertexBuffer.SetData(0, _vertexInfo, 0, _numSprites, VertexPositionColorTexture4.RealStride,
                SetDataOptions.None);

            curTexture = _textureInfo[0];
            for (var i = 1; i < _numSprites; i += 1)
                if (_textureInfo[i] != curTexture)
                {
                    DrawPrimitives(curTexture, offset, i - offset);
                    curTexture = _textureInfo[i];
                    offset = i;
                }
            DrawPrimitives(curTexture, offset, _numSprites - offset);

            _numSprites = 0;
        }


	    /// <summary>
	    ///     enables/disables scissor testing. If the RasterizerState changes it will cause a batch flush.
	    /// </summary>
	    /// <returns>The scissor test.</returns>
	    /// <param name="shouldEnable">Should enable.</param>
	    public void EnableScissorTest(bool shouldEnable)
        {
            var currentValue = _rasterizerState.ScissorTestEnable;
            if (currentValue == shouldEnable)
                return;

            FlushBatch();

            _rasterizerState = new RasterizerState
            {
                CullMode = _rasterizerState.CullMode,
                DepthBias = _rasterizerState.DepthBias,
                FillMode = _rasterizerState.FillMode,
                MultiSampleAntiAlias = _rasterizerState.MultiSampleAntiAlias,
                SlopeScaleDepthBias = _rasterizerState.SlopeScaleDepthBias,
                ScissorTestEnable = shouldEnable
            };
        }


        private void PrepRenderState()
        {
            GraphicsDevice.BlendState = _blendState;
            GraphicsDevice.SamplerStates[0] = _samplerState;
            GraphicsDevice.DepthStencilState = _depthStencilState;
            GraphicsDevice.RasterizerState = _rasterizerState;

            GraphicsDevice.SetVertexBuffer(_vertexBuffer);
            GraphicsDevice.Indices = _indexBuffer;

            var viewport = GraphicsDevice.Viewport;

            // inlined CreateOrthographicOffCenter
#if FNA
			_projectionMatrix.M11 = (float)( 2.0 / (double) ( viewport.Width / 2 * 2 - 1 ) );
			_projectionMatrix.M22 = (float)( -2.0 / (double) ( viewport.Height / 2 * 2 - 1 ) );
#else
            _projectionMatrix.M11 = (float) (2.0 / viewport.Width);
            _projectionMatrix.M22 = (float) (-2.0 / viewport.Height);
#endif

            _projectionMatrix.M41 = -1 - 0.5f * _projectionMatrix.M11;
            _projectionMatrix.M42 = 1 - 0.5f * _projectionMatrix.M22;

            Matrix.Multiply(ref _transformMatrix, ref _projectionMatrix, out _matrixTransformMatrix);
            _spriteEffect.SetMatrixTransform(ref _matrixTransformMatrix);

            // we have to Apply here because custom effects often wont have a vertex shader and we need the default SpriteEffect's
            _spriteEffectPass.Apply();
        }


        private void DrawPrimitives(Texture texture, int baseSprite, int batchSize)
        {
            if (_customEffect != null)
            {
                foreach (var pass in _customEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();

                    // Whatever happens in pass.Apply, make sure the texture being drawn ends up in Textures[0].
                    GraphicsDevice.Textures[0] = texture;
                    GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, baseSprite * 4, 0, batchSize * 2);
                }
            }
            else
            {
                GraphicsDevice.Textures[0] = texture;
                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, baseSprite * 4, 0, batchSize * 2);
            }
        }


        [Conditional("DEBUG")]
        private void CheckBegin()
        {
            if (!_beginCalled)
                throw new InvalidOperationException(
                    "Begin has not been called. Begin must be called before you can draw");
        }

        #endregion
    }
}