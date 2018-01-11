using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Nez.Maths;
using Nez.UI.Widgets;

namespace Nez.UI.Base
{
    public class Group : Element, ICullable
    {
        private Rectangle? _cullingArea;
        private Matrix _previousBatcherTransform;
        internal List<Element> Children = new List<Element>();
        protected bool Transform;

        public void SetCullingArea(Rectangle cullingArea)
        {
            _cullingArea = cullingArea;
        }

        public T AddElement<T>(T element) where T : Element
        {
            if (element.Parent != null)
                element.Parent.RemoveElement(element);

            Children.Add(element);
            element.SetParent(this);
            element.SetStage(Stage);
            OnChildrenChanged();

            return element;
        }


        public T InsertElement<T>(int index, T element) where T : Element
        {
            if (element.Parent != null)
                element.Parent.RemoveElement(element);

            if (index >= Children.Count)
                return AddElement(element);

            Children.Insert(index, element);
            element.SetParent(this);
            element.SetStage(Stage);
            OnChildrenChanged();

            return element;
        }


        public virtual bool RemoveElement(Element element)
        {
            if (!Children.Contains(element))
                return false;

            element.Parent = null;
            Children.Remove(element);
            OnChildrenChanged();
            return true;
        }


	    /// <summary>
	    ///     Returns an ordered list of child elements in this group
	    /// </summary>
	    /// <returns>The children.</returns>
	    public List<Element> GetChildren()
        {
            return Children;
        }


        public void SetTransform(bool transform)
        {
            this.Transform = transform;
        }


	    /// <summary>
	    ///     sets the stage on all children in case the Group is added to the Stage after it is configured
	    /// </summary>
	    /// <param name="stage">Stage.</param>
	    internal override void SetStage(Stage stage)
        {
            this.Stage = stage;
            for (var i = 0; i < Children.Count; i++)
                Children[i].SetStage(stage);
        }


        private void SetLayoutEnabled(Group parent, bool enabled)
        {
            for (var i = 0; i < parent.Children.Count; i++)
                if (parent.Children[i] is ILayout)
                    ((ILayout) parent.Children[i]).LayoutEnabled = enabled;
                else if (parent.Children[i] is Group)
                    SetLayoutEnabled(parent.Children[i] as Group, enabled);
        }


	    /// <summary>
	    ///     Removes all children
	    /// </summary>
	    public void Clear()
        {
            ClearChildren();
        }


	    /// <summary>
	    ///     Removes all elements from this group
	    /// </summary>
	    public virtual void ClearChildren()
        {
            for (var i = 0; i < Children.Count; i++)
                Children[i].Parent = null;

            Children.Clear();
            OnChildrenChanged();
        }


	    /// <summary>
	    ///     Called when elements are added to or removed from the group.
	    /// </summary>
	    protected virtual void OnChildrenChanged()
        {
            InvalidateHierarchy();
        }


        public override Element Hit(Vector2 point)
        {
            if (Touchable == Touchable.Disabled)
                return null;

            for (var i = Children.Count - 1; i >= 0; i--)
            {
                var child = Children[i];
                if (!child.IsVisible())
                    continue;

                var childLocalPoint = child.ParentToLocalCoordinates(point);
                var hit = child.Hit(childLocalPoint);
                if (hit != null)
                    return hit;
            }

            return base.Hit(point);
        }


        public override void Draw(Graphics.Graphics graphics, float parentAlpha)
        {
            if (!IsVisible())
                return;

            Validate();

            if (Transform)
                ApplyTransform(graphics, ComputeTransform());

            DrawChildren(graphics, parentAlpha);

            if (Transform)
                ResetTransform(graphics);
        }


        public void DrawChildren(Graphics.Graphics graphics, float parentAlpha)
        {
            parentAlpha *= Color.A / 255.0f;

            if (_cullingArea.HasValue)
            {
                float cullLeft = _cullingArea.Value.X;
                var cullRight = cullLeft + _cullingArea.Value.Width;
                float cullBottom = _cullingArea.Value.Y;
                var cullTop = cullBottom + _cullingArea.Value.Height;

                if (Transform)
                {
                    for (int i = 0, n = Children.Count; i < n; i++)
                    {
                        var child = Children[i];
                        if (!child.IsVisible()) continue;
                        float cx = child.X, cy = child.Y;
                        if (cx <= cullRight && cy <= cullTop && cx + child.Width >= cullLeft &&
                            cy + child.Height >= cullBottom)
                            child.Draw(graphics, parentAlpha);
                    }
                }
                else
                {
                    float offsetX = X, offsetY = Y;
                    X = 0;
                    Y = 0;
                    for (int i = 0, n = Children.Count; i < n; i++)
                    {
                        var child = Children[i];
                        if (!child.IsVisible()) continue;
                        float cx = child.X, cy = child.Y;
                        if (cx <= cullRight && cy <= cullTop && cx + child.Width >= cullLeft &&
                            cy + child.Height >= cullBottom)
                        {
                            child.X = cx + offsetX;
                            child.Y = cy + offsetY;
                            child.Draw(graphics, parentAlpha);
                            child.X = cx;
                            child.Y = cy;
                        }
                    }
                    X = offsetX;
                    Y = offsetY;
                }
            }
            else
            {
                // No culling, draw all children.
                if (Transform)
                {
                    for (int i = 0, n = Children.Count; i < n; i++)
                    {
                        var child = Children[i];
                        if (!child.IsVisible()) continue;
                        child.Draw(graphics, parentAlpha);
                    }
                }
                else
                {
                    // No transform for this group, offset each child.
                    float offsetX = X, offsetY = Y;
                    X = 0;
                    Y = 0;
                    for (int i = 0, n = Children.Count; i < n; i++)
                    {
                        var child = Children[i];
                        if (!child.IsVisible()) continue;
                        float cx = child.X, cy = child.Y;
                        child.X = cx + offsetX;
                        child.Y = cy + offsetY;
                        child.Draw(graphics, parentAlpha);
                        child.X = cx;
                        child.Y = cy;
                    }
                    X = offsetX;
                    Y = offsetY;
                }
            }
        }


        public override void DebugRender(Graphics.Graphics graphics)
        {
            if (Transform)
                ApplyTransform(graphics, ComputeTransform());

            DebugRenderChildren(graphics, 1f);

            if (Transform)
                ResetTransform(graphics);

            if (this is Button)
                base.DebugRender(graphics);
        }


        public void DebugRenderChildren(Graphics.Graphics graphics, float parentAlpha)
        {
            parentAlpha *= Color.A / 255.0f;
            if (Transform)
            {
                for (var i = 0; i < Children.Count; i++)
                {
                    if (!Children[i].IsVisible())
                        continue;

                    if (!Children[i].GetDebug() && !(Children[i] is Group))
                        continue;

                    Children[i].DebugRender(graphics);
                }
            }
            else
            {
                // No transform for this group, offset each child.
                float offsetX = X, offsetY = Y;
                X = 0;
                Y = 0;
                for (var i = 0; i < Children.Count; i++)
                {
                    if (!Children[i].IsVisible())
                        continue;

                    if (!Children[i].GetDebug() && !(Children[i] is Group))
                        continue;

                    Children[i].X += offsetX;
                    Children[i].Y += offsetY;
                    Children[i].DebugRender(graphics);
                    Children[i].X -= offsetX;
                    Children[i].Y -= offsetY;
                }
                X = offsetX;
                Y = offsetY;
            }
        }


	    /// <summary>
	    ///     Returns the transform for this group's coordinate system
	    /// </summary>
	    /// <returns>The transform.</returns>
	    protected Matrix2D ComputeTransform()
        {
            var mat = Matrix2D.Identity;

            if (OriginX != 0 || OriginY != 0)
                mat = Matrix2D.Multiply(mat, Matrix2D.CreateTranslation(-OriginX, -OriginY));

            if (Rotation != 0)
                mat = Matrix2D.Multiply(mat, Matrix2D.CreateRotation(MathHelper.ToRadians(Rotation)));

            if (ScaleX != 1 || ScaleY != 1)
                mat = Matrix2D.Multiply(mat, Matrix2D.CreateScale(ScaleX, ScaleY));

            mat = Matrix2D.Multiply(mat, Matrix2D.CreateTranslation(X + OriginX, Y + OriginY));

            // Find the first parent that transforms
            var parentGroup = Parent;
            while (parentGroup != null)
            {
                if (parentGroup.Transform)
                    break;
                parentGroup = parentGroup.Parent;
            }

            if (parentGroup != null)
                mat = Matrix2D.Multiply(mat, parentGroup.ComputeTransform());

            return mat;
        }


	    /// <summary>
	    ///     Set the batch's transformation matrix, often with the result of {@link #computeTransform()}. Note this causes the
	    ///     batch to
	    ///     be flushed. {@link #resetTransform(Batch)} will restore the transform to what it was before this call.
	    /// </summary>
	    /// <param name="graphics">Graphics.</param>
	    /// <param name="transform">Transform.</param>
	    protected void ApplyTransform(Graphics.Graphics graphics, Matrix transform)
        {
            _previousBatcherTransform = graphics.Batcher.TransformMatrix;
            graphics.Batcher.End();
            graphics.Batcher.Begin(transform);
        }


	    /// <summary>
	    ///     Restores the batch transform to what it was before {@link #applyTransform(Batch, Matrix4)}. Note this causes the
	    ///     batch to
	    ///     be flushed
	    /// </summary>
	    /// <param name="batch">Batch.</param>
	    protected void ResetTransform(Graphics.Graphics graphics)
        {
            graphics.Batcher.End();
            graphics.Batcher.Begin(_previousBatcherTransform);
        }


	    /// <summary>
	    ///     If true, drawDebug() will be called for this group and, optionally, all children recursively.
	    /// </summary>
	    /// <param name="enabled">If set to <c>true</c> enabled.</param>
	    /// <param name="recursively">If set to <c>true</c> recursively.</param>
	    public void SetDebug(bool enabled, bool recursively)
        {
            Debug = enabled;
            if (recursively)
                foreach (var child in Children)
                    if (child is Group)
                        ((Group) child).SetDebug(enabled, recursively);
                    else
                        child.SetDebug(enabled);
        }


	    /// <summary>
	    ///     Calls {setDebug(true, true)
	    /// </summary>
	    /// <returns>The all.</returns>
	    public virtual Group DebugAll()
        {
            SetDebug(true, true);
            return this;
        }


        #region ILayout

        public override bool LayoutEnabled
        {
            get => LayoutEnabled;
            set
            {
                if (LayoutEnabled != value)
                {
                    LayoutEnabled = value;

                    SetLayoutEnabled(this, LayoutEnabled);
                    if (LayoutEnabled)
                        InvalidateHierarchy();
                }
            }
        }


        public override void Pack()
        {
            SetSize(PreferredWidth, PreferredHeight);
            Validate();

            // Some situations require another layout. Eg, a wrapped label doesn't know its pref height until it knows its width, so it
            // calls invalidateHierarchy() in layout() if its pref height has changed.
            if (NeedsLayout)
            {
                SetSize(PreferredWidth, PreferredHeight);
                Validate();
            }
        }

        #endregion
    }
}