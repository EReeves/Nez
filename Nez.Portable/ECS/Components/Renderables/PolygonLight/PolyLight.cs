using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Nez.Shadows
{
	/// <summary>
	///     Point light that also casts shadows
	/// </summary>
	public class PolyLight : RenderableComponent
    {
        // shared Collider cache used for querying for nearby geometry. Maxes out at 10 Colliders.
        protected static Collider[] ColliderCache = new Collider[10];

        private readonly FastList<short> _indices = new FastList<short>(50);

        private Effect _lightEffect;

        protected float radius;
        private readonly FastList<VertexPositionTexture> _vertices = new FastList<VertexPositionTexture>(20);
        protected VisibilityComputer Visibility;

	    /// <summary>
	    ///     layer mask of all the layers this light should interact with. defaults to all layers.
	    /// </summary>
	    public int CollidesWithLayers = Physics.AllLayers;

	    /// <summary>
	    ///     Power of the light, from 0 (turned off) to 1 for maximum brightness
	    /// </summary>
	    public float Power;


        public PolyLight(float radius) : this(radius, Color.White)
        {
        }


        public PolyLight(float radius, Color color) : this(radius, color, 1.0f)
        {
        }


        public PolyLight(float radius, Color color, float power)
        {
            this.radius = radius;
            this.Power = power;
            this.Color = color;
            ComputeTriangleIndices();
        }

        public override RectangleF Bounds
        {
            get
            {
                if (AreBoundsDirty)
                {
                    Bounds.CalculateBounds(Entity.Transform.Position, localOffset, new Vector2(Radius, Radius),
                        Vector2.One, 0, Radius * 2f, Radius * 2f);
                    AreBoundsDirty = false;
                }

                return Bounds;
            }
        }

	    /// <summary>
	    ///     Radius of influence of the light
	    /// </summary>
	    public float Radius
        {
            get => Radius;
            set => SetRadius(value);
        }


	    /// <summary>
	    ///     fetches any Colliders that should be considered for occlusion. Subclasses with a shape other than a circle can
	    ///     override this.
	    /// </summary>
	    /// <returns>The overlapped components.</returns>
	    protected virtual int GetOverlappedColliders()
        {
            return Physics.OverlapCircleAll(Entity.Position + localOffset, Radius, ColliderCache,
                CollidesWithLayers);
        }


	    /// <summary>
	    ///     override point for calling through to VisibilityComputer that allows subclasses to setup their visibility
	    ///     boundaries for
	    ///     different shaped lights.
	    /// </summary>
	    protected virtual void LoadVisibilityBoundaries()
        {
            Visibility.LoadRectangleBoundaries();
        }


	    /// <summary>
	    ///     adds a vert to the list
	    /// </summary>
	    /// <param name="position">Position.</param>
	    /// <param name="texCoord">Tex coordinate.</param>
	    [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddVert(Vector2 position)
        {
            var index = _vertices.Length;
            _vertices.EnsureCapacity();
            _vertices.Buffer[index].Position = position.ToVector3();
            _vertices.Buffer[index].TextureCoordinate = position;
            _vertices.Length++;
        }


        private void ComputeTriangleIndices(int totalTris = 20)
        {
            _indices.Reset();

            // compute the indices to form triangles
            for (var i = 0; i < totalTris; i += 2)
            {
                _indices.Add(0);
                _indices.Add((short) (i + 2));
                _indices.Add((short) (i + 1));
            }
        }


        private void GenerateVertsFromEncounters(List<Vector2> encounters)
        {
            _vertices.Reset();

            // add a vertex for the center of the mesh
            AddVert(Entity.Transform.Position);

            // add all the other encounter points as vertices storing their world position as UV coordinates
            for (var i = 0; i < encounters.Count; i++)
                AddVert(encounters[i]);

            // if we dont have enough tri indices add enough for our encounter list
            var triIndices = _indices.Length / 3;
            if (encounters.Count > triIndices)
                ComputeTriangleIndices(encounters.Count);
        }


        #region Fluent setters

        public virtual PolyLight SetRadius(float radius)
        {
            if (radius != Radius)
            {
                Radius = radius;
                AreBoundsDirty = true;

                if (_lightEffect != null)
                    _lightEffect.Parameters["lightRadius"].SetValue(radius);
            }

            return this;
        }


        public PolyLight SetPower(float power)
        {
            this.Power = power;
            return this;
        }

        #endregion


        #region Component and RenderableComponent

        public override void OnAddedToEntity()
        {
            _lightEffect = Entity.Scene.Content.LoadEffect<Effect>("polygonLight", EffectResource.PolygonLightBytes);
            _lightEffect.Parameters["lightRadius"].SetValue((float) radius);
            Visibility = new VisibilityComputer();
        }


        public override void Render(Graphics graphics, Camera camera)
        {
            if (Power > 0 && IsVisibleFromCamera(camera))
            {
                var totalOverlaps = GetOverlappedColliders();

                // compute the visibility mesh
                Visibility.Begin(Entity.Transform.Position + localOffset, Radius);
                LoadVisibilityBoundaries();
                for (var i = 0; i < totalOverlaps; i++)
                    if (!ColliderCache[i].IsTrigger)
                        Visibility.AddColliderOccluder(ColliderCache[i]);
                Array.Clear(ColliderCache, 0, totalOverlaps);

                // generate a triangle list from the encounter points
                var encounters = Visibility.End();
                GenerateVertsFromEncounters(encounters);
                ListPool<Vector2>.Free(encounters);

                var primitiveCount = _vertices.Length / 2;
                if (primitiveCount == 0)
                    return;

                Core.GraphicsDevice.BlendState = BlendState.Additive;
                Core.GraphicsDevice.RasterizerState = RasterizerState.CullNone;

                // wireframe debug
                //var rasterizerState = new RasterizerState();
                //rasterizerState.FillMode = FillMode.WireFrame;
                //rasterizerState.CullMode = CullMode.None;
                //Core.graphicsDevice.RasterizerState = rasterizerState;

                // Apply the effect
                _lightEffect.Parameters["viewProjectionMatrix"].SetValue(Entity.Scene.Camera.ViewProjectionMatrix);
                _lightEffect.Parameters["lightSource"].SetValue(Entity.Transform.Position);
                _lightEffect.Parameters["lightColor"].SetValue(Color.ToVector3() * Power);
                _lightEffect.Techniques[0].Passes[0].Apply();

                Core.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, _vertices.Buffer, 0,
                    _vertices.Length, _indices.Buffer, 0, primitiveCount);
            }
        }


        public override void DebugRender(Graphics graphics)
        {
            // draw a square for our pivot/origin and draw our bounds
            graphics.Batcher.DrawPixel(Entity.Transform.Position + localOffset, Debug.Colors.RenderableCenter, 4);
            graphics.Batcher.DrawHollowRect(Bounds, Debug.Colors.RenderableBounds);
        }

        #endregion
    }
}