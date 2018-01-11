using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Nez.Maths;
using Nez.Utils.Extensions;

namespace Nez.ECS
{
    public class Transform
    {
        public enum Component
        {
            Position,
            Scale,
            Rotation
        }


        public Transform(Entity entity)
        {
            this.Entity = entity;
            _scale = _localScale = Vector2.One;
        }


	    /// <summary>
	    ///     returns the Transform child at index
	    /// </summary>
	    /// <returns>The child.</returns>
	    /// <param name="index">Index.</param>
	    public Transform GetChild(int index)
        {
            return _children[index];
        }


	    /// <summary>
	    ///     rounds the position of the Transform
	    /// </summary>
	    public void RoundPosition()
        {
            Position = _position.Round();
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateTransform()
        {
            if (_hierarchyDirty != DirtyType.Clean)
            {
                if (Parent != null)
                    Parent.UpdateTransform();

                if (_localDirty)
                {
                    if (_localPositionDirty)
                    {
                        Matrix2D.CreateTranslation(_localPosition.X, _localPosition.Y, out _translationMatrix);
                        _localPositionDirty = false;
                    }

                    if (_localRotationDirty)
                    {
                        Matrix2D.CreateRotation(_localRotation, out _rotationMatrix);
                        _localRotationDirty = false;
                    }

                    if (_localScaleDirty)
                    {
                        Matrix2D.CreateScale(_localScale.X, _localScale.Y, out _scaleMatrix);
                        _localScaleDirty = false;
                    }

                    Matrix2D.Multiply(ref _scaleMatrix, ref _rotationMatrix, out _localTransform);
                    Matrix2D.Multiply(ref _localTransform, ref _translationMatrix, out _localTransform);

                    if (Parent == null)
                    {
                        _worldTransform = _localTransform;
                        _rotation = _localRotation;
                        _scale = _localScale;
                        _worldInverseDirty = true;
                    }
                    _localDirty = false;
                }

                if (Parent != null)
                {
                    Matrix2D.Multiply(ref _localTransform, ref Parent._worldTransform, out _worldTransform);

                    _rotation = _localRotation + Parent._rotation;
                    _scale = Parent._scale * _localScale;
                    _worldInverseDirty = true;
                }

                _worldToLocalDirty = true;
                _positionDirty = true;
                _hierarchyDirty = DirtyType.Clean;
            }
        }


	    /// <summary>
	    ///     sets the dirty flag on the enum and passes it down to our children
	    /// </summary>
	    /// <param name="dirtyFlagType">Dirty flag type.</param>
	    [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetDirty(DirtyType dirtyFlagType)
        {
            if ((_hierarchyDirty & dirtyFlagType) == 0)
            {
                _hierarchyDirty |= dirtyFlagType;

                switch (dirtyFlagType)
                {
                    case DirtyType.PositionDirty:
                        Entity.OnTransformChanged(Component.Position);
                        break;
                    case DirtyType.RotationDirty:
                        Entity.OnTransformChanged(Component.Rotation);
                        break;
                    case DirtyType.ScaleDirty:
                        Entity.OnTransformChanged(Component.Scale);
                        break;
                }

                // dirty our children as well so they know of the changes
                for (var i = 0; i < _children.Count; i++)
                    _children[i].SetDirty(dirtyFlagType);
            }
        }


        public void CopyFrom(Transform transform)
        {
            _position = transform.Position;
            _localPosition = transform._localPosition;
            _rotation = transform._rotation;
            _localRotation = transform._localRotation;
            _scale = transform._scale;
            _localScale = transform._localScale;

            SetDirty(DirtyType.PositionDirty);
            SetDirty(DirtyType.RotationDirty);
            SetDirty(DirtyType.ScaleDirty);
        }


        public override string ToString()
        {
            return string.Format(
                "[Transform: parent: {0}, position: {1}, rotation: {2}, scale: {3}, localPosition: {4}, localRotation: {5}, localScale: {6}]",
                Parent != null, Position, Rotation, Scale, LocalPosition, LocalRotation, LocalScale);
        }

        [Flags]
        private enum DirtyType
        {
            Clean,
            PositionDirty,
            ScaleDirty,
            RotationDirty
        }


        #region properties and fields

	    /// <summary>
	    ///     if true, everytime position is set it will be rounded automatically
	    /// </summary>
	    public static bool ShouldRoundPosition = true;

	    /// <summary>
	    ///     the Entity associated with this transform
	    /// </summary>
	    public readonly Entity Entity;

	    /// <summary>
	    ///     the parent Transform of this Transform
	    /// </summary>
	    /// <value>The parent.</value>
	    public Transform Parent
        {
            get => _parent;
            set => SetParent(value);
        }


	    /// <summary>
	    ///     total children of this Transform
	    /// </summary>
	    /// <value>The child count.</value>
	    public int ChildCount => _children.Count;


	    /// <summary>
	    ///     position of the transform in world space
	    /// </summary>
	    /// <value>The position.</value>
	    public Vector2 Position
        {
            get
            {
                UpdateTransform();
                if (_positionDirty)
                {
                    if (Parent == null)
                    {
                        _position = _localPosition;
                    }
                    else
                    {
                        Parent.UpdateTransform();
                        Vector2Ext.Transform(ref _localPosition, ref Parent._worldTransform, out _position);
                    }

                    _positionDirty = false;
                }
                return _position;
            }
            set => SetPosition(value);
        }


	    /// <summary>
	    ///     position of the transform relative to the parent transform. If the transform has no parent, it is the same as
	    ///     Transform.position
	    /// </summary>
	    /// <value>The local position.</value>
	    public Vector2 LocalPosition
        {
            get
            {
                UpdateTransform();
                return _localPosition;
            }
            set => SetLocalPosition(value);
        }


	    /// <summary>
	    ///     rotation of the transform in world space in radians
	    /// </summary>
	    /// <value>The rotation.</value>
	    public float Rotation
        {
            get
            {
                UpdateTransform();
                return _rotation;
            }
            set => SetRotation(value);
        }


	    /// <summary>
	    ///     rotation of the transform in world space in degrees
	    /// </summary>
	    /// <value>The rotation degrees.</value>
	    public float RotationDegrees
        {
            get => MathHelper.ToDegrees(_rotation);
            set => SetRotation(MathHelper.ToRadians(value));
        }


	    /// <summary>
	    ///     the rotation of the transform relative to the parent transform's rotation. If the transform has no parent, it is
	    ///     the same as Transform.rotation
	    /// </summary>
	    /// <value>The local rotation.</value>
	    public float LocalRotation
        {
            get
            {
                UpdateTransform();
                return _localRotation;
            }
            set => SetLocalRotation(value);
        }


	    /// <summary>
	    ///     rotation of the transform relative to the parent transform's rotation in degrees
	    /// </summary>
	    /// <value>The rotation degrees.</value>
	    public float LocalRotationDegrees
        {
            get => MathHelper.ToDegrees(_localRotation);
            set => LocalRotation = MathHelper.ToRadians(value);
        }


	    /// <summary>
	    ///     global scale of the transform
	    /// </summary>
	    /// <value>The scale.</value>
	    public Vector2 Scale
        {
            get
            {
                UpdateTransform();
                return _scale;
            }
            set => SetScale(value);
        }


	    /// <summary>
	    ///     the scale of the transform relative to the parent. If the transform has no parent, it is the same as
	    ///     Transform.scale
	    /// </summary>
	    /// <value>The local scale.</value>
	    public Vector2 LocalScale
        {
            get
            {
                UpdateTransform();
                return _localScale;
            }
            set => SetLocalScale(value);
        }


        public Matrix2D WorldInverseTransform
        {
            get
            {
                UpdateTransform();
                if (_worldInverseDirty)
                {
                    Matrix2D.Invert(ref _worldTransform, out _worldInverseTransform);
                    _worldInverseDirty = false;
                }
                return _worldInverseTransform;
            }
        }


        public Matrix2D LocalToWorldTransform
        {
            get
            {
                UpdateTransform();
                return _worldTransform;
            }
        }


        public Matrix2D WorldToLocalTransform
        {
            get
            {
                if (_worldToLocalDirty)
                {
                    if (Parent == null)
                    {
                        _worldToLocalTransform = Matrix2D.Identity;
                    }
                    else
                    {
                        Parent.UpdateTransform();
                        Matrix2D.Invert(ref Parent._worldTransform, out _worldToLocalTransform);
                    }

                    _worldToLocalDirty = false;
                }
                return _worldToLocalTransform;
            }
        }


        private Transform _parent;
        private DirtyType _hierarchyDirty;

        private bool _localDirty;
        private bool _localPositionDirty;
        private bool _localScaleDirty;
        private bool _localRotationDirty;
        private bool _positionDirty;
        private bool _worldToLocalDirty;
        private bool _worldInverseDirty;

        // value is automatically recomputed from the position, rotation and scale
        private Matrix2D _localTransform;

        // value is automatically recomputed from the local and the parent matrices.
        private Matrix2D _worldTransform = Matrix2D.Identity;

        private Matrix2D _worldToLocalTransform = Matrix2D.Identity;
        private Matrix2D _worldInverseTransform = Matrix2D.Identity;

        private Matrix2D _rotationMatrix;
        private Matrix2D _translationMatrix;
        private Matrix2D _scaleMatrix;

        private Vector2 _position;
        private Vector2 _scale;
        private float _rotation;

        private Vector2 _localPosition;
        private Vector2 _localScale;
        private float _localRotation;

        private readonly List<Transform> _children = new List<Transform>();

        #endregion


        #region Fluent setters

	    /// <summary>
	    ///     sets the parent Transform of this Transform
	    /// </summary>
	    /// <returns>The parent.</returns>
	    /// <param name="parent">Parent.</param>
	    public Transform SetParent(Transform parent)
        {
            if (_parent == parent)
                return this;

            if (_parent != null)
                _parent._children.Remove(this);

            if (parent != null)
                parent._children.Add(this);

            _parent = parent;
            SetDirty(DirtyType.PositionDirty);

            return this;
        }


	    /// <summary>
	    ///     sets the position of the transform in world space
	    /// </summary>
	    /// <returns>The position.</returns>
	    /// <param name="position">Position.</param>
	    [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Transform SetPosition(Vector2 position)
        {
            if (ShouldRoundPosition)
                Vector2Ext.Round(ref position);

            if (position == _position)
                return this;

            _position = position;
            if (Parent != null)
                LocalPosition = Vector2.Transform(_position, WorldToLocalTransform);
            else
                LocalPosition = position;

            _positionDirty = false;

            return this;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Transform SetPosition(float x, float y)
        {
            return SetPosition(new Vector2(x, y));
        }


	    /// <summary>
	    ///     sets the position of the transform relative to the parent transform. If the transform has no parent, it is the same
	    ///     as Transform.position
	    /// </summary>
	    /// <returns>The local position.</returns>
	    /// <param name="localPosition">Local position.</param>
	    [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Transform SetLocalPosition(Vector2 localPosition)
        {
            if (ShouldRoundPosition)
                Vector2Ext.Round(ref localPosition);

            if (localPosition == _localPosition)
                return this;

            _localPosition = localPosition;
            _localDirty = _positionDirty = _localPositionDirty = _localRotationDirty = _localScaleDirty = true;
            SetDirty(DirtyType.PositionDirty);

            return this;
        }


	    /// <summary>
	    ///     sets the rotation of the transform in world space in radians
	    /// </summary>
	    /// <returns>The rotation.</returns>
	    /// <param name="radians">Radians.</param>
	    [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Transform SetRotation(float radians)
        {
            _rotation = radians;
            if (Parent != null)
                LocalRotation = Parent.Rotation + radians;
            else
                LocalRotation = radians;

            return this;
        }


	    /// <summary>
	    ///     sets the rotation of the transform in world space in degrees
	    /// </summary>
	    /// <returns>The rotation.</returns>
	    /// <param name="radians">Radians.</param>
	    [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Transform SetRotationDegrees(float degrees)
        {
            return SetRotation(MathHelper.ToRadians(degrees));
        }


	    /// <summary>
	    ///     sets the the rotation of the transform relative to the parent transform's rotation. If the transform has no parent,
	    ///     it is the
	    ///     same as Transform.rotation
	    /// </summary>
	    /// <returns>The local rotation.</returns>
	    /// <param name="radians">Radians.</param>
	    [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Transform SetLocalRotation(float radians)
        {
            _localRotation = radians;
            _localDirty = _positionDirty = _localPositionDirty = _localRotationDirty = _localScaleDirty = true;
            SetDirty(DirtyType.RotationDirty);

            return this;
        }


	    /// <summary>
	    ///     sets the the rotation of the transform relative to the parent transform's rotation. If the transform has no parent,
	    ///     it is the
	    ///     same as Transform.rotation
	    /// </summary>
	    /// <returns>The local rotation.</returns>
	    /// <param name="radians">Radians.</param>
	    [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Transform SetLocalRotationDegrees(float degrees)
        {
            return SetLocalRotation(MathHelper.ToRadians(degrees));
        }


	    /// <summary>
	    ///     sets the global scale of the transform
	    /// </summary>
	    /// <returns>The scale.</returns>
	    /// <param name="scale">Scale.</param>
	    [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Transform SetScale(Vector2 scale)
        {
            _scale = scale;
            if (Parent != null)
                LocalScale = scale / Parent._scale;
            else
                LocalScale = scale;

            return this;
        }


	    /// <summary>
	    ///     sets the global scale of the transform
	    /// </summary>
	    /// <returns>The scale.</returns>
	    /// <param name="scale">Scale.</param>
	    [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Transform SetScale(float scale)
        {
            return SetScale(new Vector2(scale));
        }


	    /// <summary>
	    ///     sets the scale of the transform relative to the parent. If the transform has no parent, it is the same as
	    ///     Transform.scale
	    /// </summary>
	    /// <returns>The local scale.</returns>
	    /// <param name="scale">Scale.</param>
	    [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Transform SetLocalScale(Vector2 scale)
        {
            _localScale = scale;
            _localDirty = _positionDirty = _localScaleDirty = true;
            SetDirty(DirtyType.ScaleDirty);

            return this;
        }


	    /// <summary>
	    ///     sets the scale of the transform relative to the parent. If the transform has no parent, it is the same as
	    ///     Transform.scale
	    /// </summary>
	    /// <returns>The local scale.</returns>
	    /// <param name="scale">Scale.</param>
	    [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Transform SetLocalScale(float scale)
        {
            return SetLocalScale(new Vector2(scale));
        }

        #endregion
    }
}