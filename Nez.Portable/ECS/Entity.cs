using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Nez.ECS.Components.Physics.Colliders;
using Nez.ECS.InternalUtils;
using Nez.Maths;
using Nez.Utils;

namespace Nez.ECS
{
    public class Entity : IComparable<Entity>
    {
        public Entity()
        {
            Components = new ComponentList(this);
            Transform = new Transform(this);

            if (Core.EntitySystemsEnabled)
                ComponentBits = new BitSet();
        }


        public Entity(string name) : this()
        {
            this.Name = name;
        }


        public int CompareTo(Entity other)
        {
            return UpdateOrder.CompareTo(other.UpdateOrder);
        }


        internal void OnTransformChanged(Transform.Component comp)
        {
            // notify our children of our changed position
            Components.OnEntityTransformChanged(comp);
        }


	    /// <summary>
	    ///     removes the Entity from the scene and destroys all children
	    /// </summary>
	    public void Destroy()
        {
            IsDestroyed = true;
            Scene.Entities.Remove(this);
            Transform.Parent = null;

            // destroy any children we have
            for (var i = Transform.ChildCount - 1; i >= 0; i--)
            {
                var child = Transform.GetChild(i);
                child.Entity.Destroy();
            }
        }


	    /// <summary>
	    ///     detaches the Entity from the scene.
	    ///     the following lifecycle method will be called on the Entity: onRemovedFromScene
	    ///     the following lifecycle method will be called on the Components: onRemovedFromEntity
	    /// </summary>
	    public void DetachFromScene()
        {
            Scene.Entities.Remove(this);
            Components.DeregisterAllComponents();

            for (var i = 0; i < Transform.ChildCount; i++)
                Transform.GetChild(i).Entity.DetachFromScene();
        }


	    /// <summary>
	    ///     attaches an Entity that was previously detached to a new scene
	    /// </summary>
	    /// <param name="newScene">New scene.</param>
	    public void AttachToScene(Scene newScene)
        {
            Scene = newScene;
            newScene.Entities.Add(this);
            Components.RegisterAllComponents();

            for (var i = 0; i < Transform.ChildCount; i++)
                Transform.GetChild(i).Entity.AttachToScene(newScene);
        }


	    /// <summary>
	    ///     creates a deep clone of this Entity. Subclasses can override this method to copy any custom fields. When
	    ///     overriding,
	    ///     the copyFrom method should be called which will clone all Components, Colliders and Transform children for you.
	    ///     Note that cloned
	    ///     objects will not be added to any Scene! You must add them yourself!
	    /// </summary>
	    public virtual Entity Clone(Vector2 position = default(Vector2))
        {
            var entity = Activator.CreateInstance(GetType()) as Entity;
            entity.Name = Name + "(clone)";
            entity.CopyFrom(this);
            entity.Transform.Position = position;

            return entity;
        }


	    /// <summary>
	    ///     copies the properties, components and colliders of Entity to this instance
	    /// </summary>
	    /// <param name="entity">Entity.</param>
	    protected void CopyFrom(Entity entity)
        {
            // Entity fields
            Tag = entity.Tag;
            UpdateInterval = entity.UpdateInterval;
            UpdateOrder = entity.UpdateOrder;
            Enabled = entity.Enabled;

            Transform.Scale = entity.Transform.Scale;
            Transform.Rotation = entity.Transform.Rotation;

            // clone Components
            for (var i = 0; i < entity.Components.Count; i++)
                AddComponent(entity.Components[i].Clone());
            for (var i = 0; i < entity.Components.ComponentsToAdd.Count; i++)
                AddComponent(entity.Components.ComponentsToAdd[i].Clone());

            // clone any children of the Entity.transform
            for (var i = 0; i < entity.Transform.ChildCount; i++)
            {
                var child = entity.Transform.GetChild(i).Entity;

                var childClone = child.Clone();
                childClone.Transform.CopyFrom(child.Transform);
                childClone.Transform.Parent = Transform;
            }
        }


        public override string ToString()
        {
            return $"[Entity: name: {Name}, tag: {Tag}, enabled: {Enabled}, depth: {UpdateOrder}]";
        }

        #region properties and fields

	    /// <summary>
	    ///     the scene this entity belongs to
	    /// </summary>
	    public Scene Scene;

	    /// <summary>
	    ///     entity name. useful for doing scene-wide searches for an entity
	    /// </summary>
	    public string Name;

	    /// <summary>
	    ///     encapsulates the Entity's position/rotation/scale and allows setting up a hieararchy
	    /// </summary>
	    public readonly Transform Transform;

	    /// <summary>
	    ///     list of all the components currently attached to this entity
	    /// </summary>
	    public readonly ComponentList Components;

	    /// <summary>
	    ///     use this however you want to. It can later be used to query the scene for all Entities with a specific tag
	    /// </summary>
	    public int Tag
        {
            get => _tag;
            set => SetTag(value);
        }

	    /// <summary>
	    ///     specifies how often this entitys update method should be called. 1 means every frame, 2 is every other, etc
	    /// </summary>
	    public uint UpdateInterval = 1;

	    /// <summary>
	    ///     enables/disables the Entity. When disabled colliders are removed from the Physics system and components methods
	    ///     will not be called
	    /// </summary>
	    /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
	    public bool Enabled
        {
            get => _enabled;
            set => SetEnabled(value);
        }

	    /// <summary>
	    ///     update order of this Entity. updateOrder is also used to sort tag lists on scene.entities
	    /// </summary>
	    /// <value>The order.</value>
	    public int UpdateOrder
	    {
		    get => _updateOrder;
		    set => SetUpdateOrder(value);
	    }

	    internal readonly BitSet ComponentBits;

	    /// <summary>
	    ///     flag indicating if destroy was called on this Entity
	    /// </summary>
	    internal bool IsDestroyed;
        private int _tag;
        private bool _enabled = true;
	    private int _updateOrder;

	    #endregion


        #region Transform passthroughs

        public Transform Parent
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)] get { return Transform.Parent; }
            [MethodImpl(MethodImplOptions.AggressiveInlining)] set { Transform.SetParent(value); }
        }

        public int ChildCount
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)] get { return Transform.ChildCount; }
        }

        public Vector2 Position
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)] get { return Transform.Position; }
            [MethodImpl(MethodImplOptions.AggressiveInlining)] set { Transform.SetPosition(value); }
        }

        public Vector2 LocalPosition
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)] get { return Transform.LocalPosition; }
            [MethodImpl(MethodImplOptions.AggressiveInlining)] set { Transform.SetLocalPosition(value); }
        }

        public float Rotation
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)] get { return Transform.Rotation; }
            [MethodImpl(MethodImplOptions.AggressiveInlining)] set { Transform.SetRotation(value); }
        }

        public float RotationDegrees
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)] get { return Transform.RotationDegrees; }
            [MethodImpl(MethodImplOptions.AggressiveInlining)] set { Transform.SetRotationDegrees(value); }
        }

        public float LocalRotation
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)] get { return Transform.LocalRotation; }
            [MethodImpl(MethodImplOptions.AggressiveInlining)] set { Transform.SetLocalRotation(value); }
        }

        public float LocalRotationDegrees
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)] get { return Transform.LocalRotationDegrees; }
            [MethodImpl(MethodImplOptions.AggressiveInlining)] set { Transform.SetLocalRotationDegrees(value); }
        }

        public Vector2 Scale
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)] get { return Transform.Scale; }
            [MethodImpl(MethodImplOptions.AggressiveInlining)] set { Transform.SetScale(value); }
        }

        public Vector2 LocalScale
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)] get { return Transform.LocalScale; }
            [MethodImpl(MethodImplOptions.AggressiveInlining)] set { Transform.SetLocalScale(value); }
        }


        public Matrix2D WorldInverseTransform
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)] get { return Transform.WorldInverseTransform; }
        }


        public Matrix2D LocalToWorldTransform
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)] get { return Transform.LocalToWorldTransform; }
        }


        public Matrix2D WorldToLocalTransform
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)] get { return Transform.WorldToLocalTransform; }
        }

        #endregion


        #region Fluent setters

	    /// <summary>
	    ///     sets the tag for the Entity
	    /// </summary>
	    /// <returns>The tag.</returns>
	    /// <param name="tag">Tag.</param>
	    public Entity SetTag(int tag)
        {
            if (this._tag != tag)
            {
                // we only call through to the entityTagList if we already have a scene. if we dont have a scene yet we will be
                // added to the entityTagList when we do
	            Scene?.Entities.RemoveFromTagList(this);
	            this._tag = tag;
	            Scene?.Entities.AddToTagList(this);
            }

            return this;
        }


	    /// <summary>
	    ///     sets the enabled state of the Entity. When disabled colliders are removed from the Physics system and components
	    ///     methods will not be called
	    /// </summary>
	    /// <returns>The enabled.</returns>
	    /// <param name="isEnabled">If set to <c>true</c> is enabled.</param>
	    public Entity SetEnabled(bool isEnabled)
        {
            if (_enabled != isEnabled)
            {
                _enabled = isEnabled;

                if (_enabled)
                    Components.OnEntityEnabled();
                else
                    Components.OnEntityDisabled();
            }

            return this;
        }


	    /// <summary>
	    ///     sets the update order of this Entity. updateOrder is also used to sort tag lists on scene.entities
	    /// </summary>
	    /// <returns>The update order.</returns>
	    /// <param name="updateOrder">Update order.</param>
	    public Entity SetUpdateOrder(int updateOrder)
        {
            if (this._updateOrder != updateOrder)
            {
                this._updateOrder = updateOrder;
                if (Scene != null)
                {
                    Scene.Entities.MarkEntityListUnsorted();
                    Scene.Entities.MarkTagUnsorted(Tag);
                }
            }

            return this;
        }

        #endregion


        #region Entity lifecycle methods

	    /// <summary>
	    ///     Called when this entity is added to a scene after all pending entity changes are committed
	    /// </summary>
	    public virtual void OnAddedToScene()
        {
        }


	    /// <summary>
	    ///     Called when this entity is removed from a scene
	    /// </summary>
	    public virtual void OnRemovedFromScene()
        {
            // if we were destroyed, remove our components. If we were just detached we need to keep our components on the Entity.
            if (IsDestroyed)
                Components.RemoveAllComponents();
        }


	    /// <summary>
	    ///     called each frame as long as the Entity is enabled
	    /// </summary>
	    public virtual void Update()
        {
            Components.Update();
        }


	    /// <summary>
	    ///     called if Core.debugRenderEnabled is true by the default renderers. Custom renderers can choose to call it or not.
	    /// </summary>
	    /// <param name="graphics">Graphics.</param>
	    public virtual void DebugRender(Graphics.Graphics graphics)
        {
            Components.DebugRender(graphics);
        }

        #endregion


        #region Component Management

	    /// <summary>
	    ///     Adds a Component to the components list. Returns the Component.
	    /// </summary>
	    /// <returns>Scene.</returns>
	    /// <param name="component">Component.</param>
	    /// <typeparam name="T">The 1st type parameter.</typeparam>
	    public T AddComponent<T>(T component) where T : Component
        {
            component.Entity = this;
            Components.Add(component);
            component.Initialize();
            return component;
        }


	    /// <summary>
	    ///     Adds a Component to the components list. Returns the Component.
	    /// </summary>
	    /// <returns>Scene.</returns>
	    /// <typeparam name="T">The 1st type parameter.</typeparam>
	    public T AddComponent<T>() where T : Component, new()
        {
	        var component = new T {Entity = this};
	        Components.Add(component);
            component.Initialize();
            return component;
        }


	    /// <summary>
	    ///     Gets the first component of type T and returns it. If no components are found returns null.
	    /// </summary>
	    /// <returns>The component.</returns>
	    /// <typeparam name="T">The 1st type parameter.</typeparam>
	    public T GetComponent<T>() where T : Component
        {
            return Components.GetComponent<T>(false);
        }


	    /// <summary>
	    ///     Gets the first Component of type T and returns it. If no Component is found the Component will be created.
	    /// </summary>
	    /// <returns>The component.</returns>
	    /// <typeparam name="T">The 1st type parameter.</typeparam>
	    public T GetOrCreateComponent<T>() where T : Component, new()
        {
            var comp = Components.GetComponent<T>(true) ?? AddComponent<T>();

	        return comp;
        }


	    /// <summary>
	    ///     Gets the first component of type T and returns it optionally skips checking un-initialized Components (Components
	    ///     who have not yet had their
	    ///     onAddedToEntity method called). If no components are found returns null.
	    /// </summary>
	    /// <returns>The component.</returns>
	    /// <param name="onlyReturnInitializedComponents">If set to <c>true</c> only return initialized components.</param>
	    /// <typeparam name="T">The 1st type parameter.</typeparam>
	    public T GetComponent<T>(bool onlyReturnInitializedComponents) where T : Component
        {
            return Components.GetComponent<T>(onlyReturnInitializedComponents);
        }


	    /// <summary>
	    ///     Gets all the components of type T without a List allocation
	    /// </summary>
	    /// <param name="componentList">Component list.</param>
	    /// <typeparam name="T">The 1st type parameter.</typeparam>
	    public void GetComponents<T>(List<T> componentList) where T : class
        {
            Components.GetComponents(componentList);
        }


	    /// <summary>
	    ///     Gets all the components of type T. The returned List can be put back in the pool via ListPool.free.
	    /// </summary>
	    /// <returns>The component.</returns>
	    /// <typeparam name="T">The 1st type parameter.</typeparam>
	    public List<T> GetComponents<T>() where T : Component
        {
            return Components.GetComponents<T>();
        }


	    /// <summary>
	    ///     removes the first Component of type T from the components list
	    /// </summary>
	    /// <param name="component">The Component to remove</param>
	    public bool RemoveComponent<T>() where T : Component
        {
            var comp = GetComponent<T>();
            if (comp != null)
            {
                RemoveComponent(comp);
                return true;
            }

            return false;
        }


	    /// <summary>
	    ///     removes a Component from the components list
	    /// </summary>
	    /// <param name="component">The Component to remove</param>
	    public void RemoveComponent(Component component)
        {
            Components.Remove(component);
        }


	    /// <summary>
	    ///     removes all Components from the Entity
	    /// </summary>
	    public void RemoveAllComponents()
        {
            for (var i = 0; i < Components.Count; i++)
                RemoveComponent(Components[i]);
        }

        #endregion


        #region Collider management

        [Obsolete("Colliders are now Components. Use addComponent instead.")]
        public T AddCollider<T>(T collider) where T : Collider
        {
            return AddComponent(collider);
        }


        [Obsolete("Colliders are now Components. Use removeComponent instead.")]
        public void RemoveCollider(Collider collider)
        {
            RemoveComponent(collider);
        }


        [Obsolete("Colliders are now Components. Use the normal Component methods to manage Colliders.")]
        public void RemoveAllColliders()
        {
            throw new NotImplementedException();
        }


        [Obsolete("Colliders are now Components. Use the normal Component methods to manage Colliders.")]
        public T GetCollider<T>(bool onlyReturnInitializedColliders = false) where T : Collider
        {
            return GetComponent<T>(onlyReturnInitializedColliders);
        }


        [Obsolete("Colliders are now Components. Use the normal Component methods to manage Colliders.")]
        public void GetColliders(List<Collider> colliders)
        {
            GetComponents(colliders);
        }


        [Obsolete("Colliders are now Components. Use the normal Component methods to manage Colliders.")]
        public List<Collider> GetColliders()
        {
            return GetComponents<Collider>();
        }

        #endregion
    }
}