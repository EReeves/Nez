using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Nez.ECS.Components.Renderables._3D
{
    public abstract class GeometricPrimitive3D : Renderable3D, IDisposable
    {
        private BasicEffect _basicEffect;
        private IndexBuffer _indexBuffer;
        protected List<ushort> Indices = new List<ushort>();

        private VertexBuffer _vertexBuffer;
        protected List<VertexPositionColorNormal> Vertices = new List<VertexPositionColorNormal>();


        public override void Render(Graphics.Graphics graphics, Camera camera)
        {
            // flush the 2D batch so we render appropriately depth-wise
            graphics.Batcher.FlushBatch();

            Core.GraphicsDevice.BlendState = BlendState.Opaque;
            Core.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            // Set BasicEffect parameters.
            _basicEffect.World = WorldMatrix;
            _basicEffect.View = camera.ViewMatrix3D;
            _basicEffect.Projection = camera.ProjectionMatrix3D;
            _basicEffect.DiffuseColor = Color.ToVector3();

            // Set our vertex declaration, vertex buffer, and index buffer.
            Core.GraphicsDevice.SetVertexBuffer(_vertexBuffer);
            Core.GraphicsDevice.Indices = _indexBuffer;

            _basicEffect.CurrentTechnique.Passes[0].Apply();
            var primitiveCount = Indices.Count / 3;
            Core.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, primitiveCount);
        }


        #region Initialization and configuration

        protected void AddVertex(Vector3 position, Color color, Vector3 normal)
        {
            Vertices.Add(new VertexPositionColorNormal(position, color, normal));
        }


        protected void AddIndex(int index)
        {
            Indices.Add((ushort) index);
        }


	    /// <summary>
	    ///     Once all the geometry has been specified by calling AddVertex and AddIndex, this method copies the vertex and index
	    ///     data into
	    ///     GPU format buffers, ready for efficient rendering.
	    protected void InitializePrimitive()
        {
            // create a vertex buffer, and copy our vertex data into it.
            _vertexBuffer = new VertexBuffer(Core.GraphicsDevice, typeof(VertexPositionColorNormal), Vertices.Count,
                BufferUsage.None);
            _vertexBuffer.SetData(Vertices.ToArray());

            // create an index buffer, and copy our index data into it.
            _indexBuffer = new IndexBuffer(Core.GraphicsDevice, typeof(ushort), Indices.Count, BufferUsage.None);
            _indexBuffer.SetData(Indices.ToArray());
        }


        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();

            _basicEffect = Entity.Scene.Content.LoadMonoGameEffect<BasicEffect>();
            _basicEffect.VertexColorEnabled = true;
            _basicEffect.EnableDefaultLighting();
        }

        #endregion


        #region IDisposable

        ~GeometricPrimitive3D()
        {
            Dispose(false);
        }


	    /// <summary>
	    ///     frees resources used by this object.
	    /// </summary>
	    public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


	    /// <summary>
	    ///     frees resources used by this object.
	    /// </summary>
	    protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_vertexBuffer != null)
                    _vertexBuffer.Dispose();

                if (_indexBuffer != null)
                    _indexBuffer.Dispose();

                if (_basicEffect != null)
                    _basicEffect.Dispose();
            }
        }

        #endregion
    }
}