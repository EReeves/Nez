using Microsoft.Xna.Framework;
using Nez.Graphics;
using Nez.Graphics.Batcher;
using Nez.Maths;
using Nez.Utils.Extensions;

namespace Nez.UI.Base
{
    public class Element : ILayout
    {
        protected bool Debug;
        protected bool layoutEnabled = true;

        protected bool needsLayout = true;
        protected bool Visible = true;
        internal Color Color = Color.White;

        protected float OriginX, OriginY;
        internal Group Parent;
        protected float Rotation;
        protected float ScaleX = 1, ScaleY = 1;
        protected Stage Stage;
        protected Touchable Touchable = Touchable.Enabled;
        internal float Width, Height;

        internal float X, Y;
        
        //C# style properties.
        public Vector2 Position
        {
            get => new Vector2(X, Y);
            set => SetPosition(value.X, value.Y);
        }

        public Vector2 Size
        {
            get => new Vector2(Width, Height);
            set => SetSize(value.X,value.Y);
        }

        public Color Colour
        {
            get => GetColor();
            set => SetColor(value);
        }

        /// <summary>
	    ///     true if the widget's layout has been {@link #invalidate() invalidated}.
	    /// </summary>
	    /// <value><c>true</c> if needs layout; otherwise, <c>false</c>.</value>
	    public bool NeedsLayout => needsLayout;


	    /// <summary>
	    ///     If this method is overridden, the super method or {@link #validate()} should be called to ensure the widget is laid
	    ///     out.
	    /// </summary>
	    /// <param name="graphics">Graphics.</param>
	    /// <param name="parentAlpha">Parent alpha.</param>
	    public virtual void Draw(Graphics.Graphics graphics, float parentAlpha)
        {
            Validate();
        }


        protected virtual void SizeChanged()
        {
            Invalidate();
        }


        protected virtual void PositionChanged()
        {
        }


        protected virtual void RotationChanged()
        {
        }


	    /// <summary>
	    ///     returns the distance from point to the bounds of element in the largest dimension or a negative number if the point
	    ///     is inside the bounds.
	    ///     Note that point should be in the element's coordinate system already.
	    /// </summary>
	    /// <returns>The outside bounds to point.</returns>
	    /// <param name="Point">Point.</param>
	    protected float DistanceOutsideBoundsToPoint(Vector2 point)
        {
            var offsetX = System.Math.Max(-point.X, point.X - Width);
            var offsetY = System.Math.Max(-point.Y, point.Y - Height);

            return System.Math.Max(offsetX, offsetY);
        }


	    /// <summary>
	    ///     Draws this element's debug lines
	    /// </summary>
	    /// <param name="graphics">Graphics.</param>
	    public virtual void DebugRender(Graphics.Graphics graphics)
        {
            if (Debug)
                graphics.Batcher.DrawHollowRect(X, Y, Width, Height, Color.Red);
        }


	    /// <summary>
	    ///     returns true if this Element and all parent Elements are visible
	    /// </summary>
	    /// <returns><c>true</c>, if parents visible was ared, <c>false</c> otherwise.</returns>
	    private bool AreParentsVisible()
        {
            if (!Visible)
                return false;

            if (Parent != null)
                return Parent.AreParentsVisible();

            return Visible;
        }


        public virtual Element Hit(Vector2 point)
        {
            // if we are not Touchable or us or any parent is not visible bail out
            if (Touchable != Touchable.Enabled || !AreParentsVisible())
                return null;

            if (point.X >= 0 && point.X < Width && point.Y >= 0 && point.Y < Height)
                return this;
            return null;
        }


	    /// <summary>
	    ///     Removes this element from its parent, if it has a parent
	    /// </summary>
	    public bool Remove()
        {
            if (Parent != null)
                return Parent.RemoveElement(this);
            return false;
        }


        #region Getters/Setters

	    /// <summary>
	    ///     Returns the stage that this element is currently in, or null if not in a stage.
	    /// </summary>
	    /// <returns>The stage.</returns>
	    public Stage GetStage()
        {
            return Stage;
        }


	    /// <summary>
	    ///     Called by the framework when this element or any parent is added to a group that is in the stage.
	    ///     stage May be null if the element or any parent is no longer in a stage
	    /// </summary>
	    /// <param name="stage">Stage.</param>
	    internal virtual void SetStage(Stage stage)
        {
            this.Stage = stage;
        }


	    /// <summary>
	    ///     Returns true if the element's parent is not null
	    /// </summary>
	    /// <returns><c>true</c>, if parent was hased, <c>false</c> otherwise.</returns>
	    public bool HasParent()
        {
            return Parent != null;
        }


	    /// <summary>
	    ///     Returns the parent element, or null if not in a group
	    /// </summary>
	    /// <returns>The parent.</returns>
	    public Group GetParent()
        {
            return Parent;
        }


	    /// <summary>
	    ///     Called by the framework when an element is added to or removed from a group.
	    /// </summary>
	    /// <param name="parent">parent May be null if the element has been removed from the parent</param>
	    internal void SetParent(Group parent)
        {
            this.Parent = parent;
        }


	    /// <summary>
	    ///     Returns true if input events are processed by this element.
	    /// </summary>
	    /// <returns>The touchable.</returns>
	    public bool IsTouchable()
        {
            return Touchable == Touchable.Enabled;
        }


        public Touchable GetTouchable()
        {
            return Touchable;
        }


	    /// <summary>
	    ///     Determines how touch events are distributed to this element. Default is {@link Touchable#enabled}.
	    /// </summary>
	    /// <param name="touchable">Touchable.</param>
	    public void SetTouchable(Touchable touchable)
        {
            this.Touchable = touchable;
        }


        public void SetIsVisible(bool visible)
        {
            Visible = visible;
        }


        public bool IsVisible()
        {
            return Visible;
        }


	    /// <summary>
	    ///     If false, the element will not be drawn and will not receive touch events. Default is true.
	    /// </summary>
	    /// <param name="visible">Visible.</param>
	    public void SetVisible(bool visible)
        {
            Visible = visible;
        }


	    /// <summary>
	    ///     Returns the X position of the element's left edge
	    /// </summary>
	    /// <returns>The x.</returns>
	    public float GetX()
        {
            return X;
        }


	    /// <summary>
	    ///     Returns the X position of the specified {@link Align alignment}.
	    /// </summary>
	    /// <returns>The x.</returns>
	    /// <param name="alignment">Alignment.</param>
	    public float GetX(int alignment)
        {
            var x = this.X;
            if ((alignment & AlignInternal.Right) != 0)
                x += Width;
            else if ((alignment & AlignInternal.Left) == 0)
                x += Width / 2;
            return x;
        }


        public Element SetX(float x)
        {
            if (this.X != x)
            {
                this.X = x;
                PositionChanged();
            }
            return this;
        }


	    /// <summary>
	    ///     Returns the Y position of the element's bottom edge
	    /// </summary>
	    /// <returns>The y.</returns>
	    public float GetY()
        {
            return Y;
        }


	    /// <summary>
	    ///     Returns the Y position of the specified {@link Align alignment}
	    /// </summary>
	    /// <returns>The y.</returns>
	    /// <param name="alignment">Alignment.</param>
	    public float GetY(int alignment)
        {
            var y = this.Y;
            if ((alignment & AlignInternal.Bottom) != 0)
                y += Height;
            else if ((alignment & AlignInternal.Top) == 0)
                y += Height / 2;
            return y;
        }


        public Element SetY(float y)
        {
            if (this.Y != y)
            {
                this.Y = y;
                PositionChanged();
            }
            return this;
        }


	    /// <summary>
	    ///     Sets the position of the element's bottom left corner
	    /// </summary>
	    /// <param name="x">The x coordinate.</param>
	    /// <param name="y">The y coordinate.</param>
	    public Element SetPosition(float x, float y)
        {
            if (this.X != x || this.Y != y)
            {
                this.X = x;
                this.Y = y;
                PositionChanged();
            }
            return this;
        }


	    /// <summary>
	    ///     Sets the position using the specified {@link Align alignment}. Note this may set the position to non-integer
	    ///     coordinates
	    /// </summary>
	    /// <param name="x">The x coordinate.</param>
	    /// <param name="y">The y coordinate.</param>
	    /// <param name="alignment">Alignment.</param>
	    public void SetPosition(float x, float y, int alignment)
        {
            if ((alignment & AlignInternal.Right) != 0)
                x -= Width;
            else if ((alignment & AlignInternal.Left) == 0) //
                x -= Width / 2;

            if ((alignment & AlignInternal.Top) != 0)
                y -= Height;
            else if ((alignment & AlignInternal.Bottom) == 0) //
                y -= Height / 2;

            if (this.X != x || this.Y != y)
            {
                this.X = x;
                this.Y = y;
                PositionChanged();
            }
        }


	    /// <summary>
	    ///     Add x and y to current position
	    /// </summary>
	    /// <param name="x">The x coordinate.</param>
	    /// <param name="y">The y coordinate.</param>
	    public void MoveBy(float x, float y)
        {
            if (x != 0 || y != 0)
            {
                this.X += x;
                this.Y += y;
                PositionChanged();
            }
        }


        public float GetWidth()
        {
            return Width;
        }


        public void SetWidth(float width)
        {
            if (this.Width != width)
            {
                this.Width = width;
                SizeChanged();
            }
        }


        public float GetHeight()
        {
            return Height;
        }


        public void SetHeight(float height)
        {
            if (this.Height != height)
            {
                this.Height = height;
                SizeChanged();
            }
        }


        public void SetSize(float width, float height)
        {
            if (this.Width == width && this.Height == height)
                return;

            this.Width = width;
            this.Height = height;
            SizeChanged();
        }


	    /// <summary>
	    ///     Returns y plus height
	    /// </summary>
	    /// <returns>The top.</returns>
	    public float GetBottom()
        {
            return Y + Height;
        }


	    /// <summary>
	    ///     Returns x plus width
	    /// </summary>
	    /// <returns>The right.</returns>
	    public float GetRight()
        {
            return X + Width;
        }


	    /// <summary>
	    ///     Sets the x, y, width, and height.
	    /// </summary>
	    /// <param name="x">The x coordinate.</param>
	    /// <param name="y">The y coordinate.</param>
	    /// <param name="width">Width.</param>
	    /// <param name="height">Height.</param>
	    public void SetBounds(float x, float y, float width, float height)
        {
            if (this.X != x || this.Y != y)
            {
                this.X = x;
                this.Y = y;
                PositionChanged();
            }

            if (this.Width != width || this.Height != height)
            {
                this.Width = width;
                this.Height = height;
                SizeChanged();
            }
        }


        public float GetOriginX()
        {
            return OriginX;
        }


        public void SetOriginX(float originX)
        {
            this.OriginX = originX;
        }


        public float GetOriginY()
        {
            return OriginY;
        }


        public void SetOriginY(float originY)
        {
            this.OriginY = originY;
        }


	    /// <summary>
	    ///     Sets the origin position which is relative to the element's bottom left corner
	    /// </summary>
	    /// <param name="originX">Origin x.</param>
	    /// <param name="originY">Origin y.</param>
	    public void SetOrigin(float originX, float originY)
        {
            this.OriginX = originX;
            this.OriginY = originY;
        }


	    /// <summary>
	    ///     Sets the origin position to the specified {@link Align alignment}.
	    /// </summary>
	    /// <param name="alignment">Alignment.</param>
	    public void SetOrigin(int alignment)
        {
            if ((alignment & AlignInternal.Left) != 0)
                OriginX = 0;
            else if ((alignment & AlignInternal.Right) != 0)
                OriginX = Width;
            else
                OriginX = Width / 2;

            if ((alignment & AlignInternal.Top) != 0)
                OriginY = 0;
            else if ((alignment & AlignInternal.Bottom) != 0)
                OriginY = Height;
            else
                OriginY = Height / 2;
        }


        public float GetScaleX()
        {
            return ScaleX;
        }


        public void SetScaleX(float scaleX)
        {
            this.ScaleX = scaleX;
        }


        public float GetScaleY()
        {
            return ScaleY;
        }


        public void SetScaleY(float scaleY)
        {
            this.ScaleY = scaleY;
        }


	    /// <summary>
	    ///     Sets the scale for both X and Y
	    /// </summary>
	    /// <param name="scaleXy">Scale X.</param>
	    public void SetScale(float scaleXy)
        {
            ScaleX = scaleXy;
            ScaleY = scaleXy;
        }


	    /// <summary>
	    ///     Sets the scale X and scale Y
	    /// </summary>
	    /// <param name="scaleX">Scale x.</param>
	    /// <param name="scaleY">Scale y.</param>
	    public void SetScale(float scaleX, float scaleY)
        {
            this.ScaleX = scaleX;
            this.ScaleY = scaleY;
        }


	    /// <summary>
	    ///     Adds the specified scale to the current scale
	    /// </summary>
	    /// <param name="scale">Scale.</param>
	    public void ScaleBy(float scale)
        {
            ScaleX += scale;
            ScaleY += scale;
        }


	    /// <summary>
	    ///     Adds the specified scale to the current scale
	    /// </summary>
	    /// <param name="scaleX">Scale x.</param>
	    /// <param name="scaleY">Scale y.</param>
	    public void ScaleBy(float scaleX, float scaleY)
        {
            this.ScaleX += scaleX;
            this.ScaleY += scaleY;
        }


        public float GetRotation()
        {
            return Rotation;
        }


        public void SetRotation(float degrees)
        {
            if (Rotation != degrees)
            {
                Rotation = degrees;
                RotationChanged();
            }
        }


	    /// <summary>
	    ///     Adds the specified rotation to the current rotation
	    /// </summary>
	    /// <param name="amountInDegrees">Amount in degrees.</param>
	    public void RotateBy(float amountInDegrees)
        {
            if (amountInDegrees != 0)
            {
                Rotation += amountInDegrees;
                RotationChanged();
            }
        }


        public void SetColor(Color color)
        {
            this.Color = color;
        }


	    /// <summary>
	    ///     Returns the color the element will be tinted when drawn
	    /// </summary>
	    /// <returns>The color.</returns>
	    public Color GetColor()
        {
            return Color;
        }


	    /// <summary>
	    ///     Changes the z-order for this element so it is in front of all siblings
	    /// </summary>
	    public void ToFront()
        {
            SetZIndex(int.MaxValue);
        }


	    /// <summary>
	    ///     Changes the z-order for this element so it is in back of all siblings
	    /// </summary>
	    public void ToBack()
        {
            SetZIndex(0);
        }


	    /// <summary>
	    ///     Sets the z-index of this element. The z-index is the index into the parent's {@link Group#getChildren() children},
	    ///     where a
	    ///     lower index is below a higher index. Setting a z-index higher than the number of children will move the child to
	    ///     the front.
	    ///     Setting a z-index less than zero is invalid.
	    /// </summary>
	    /// <param name="index">Index.</param>
	    public void SetZIndex(int index)
        {
            var parent = this.Parent;
            if (parent == null)
                return;

            var children = parent.Children;
            if (children.Count == 1)
                return;

            index = System.Math.Min(index, children.Count - 1);
            if (index == children.IndexOf(this))
                return;

            if (!children.Remove(this))
                return;

            children.Insert(index, this);
        }


	    /// <summary>
	    ///     Calls clipBegin(Batcher, float, float, float, float) to clip this actor's bounds
	    /// </summary>
	    /// <returns>The begin.</returns>
	    public bool ClipBegin(Batcher batcher)
        {
            return ClipBegin(batcher, X, Y, Width, Height);
        }


	    /// <summary>
	    ///     Clips the specified screen aligned rectangle, specified relative to the transform matrix of the stage's Batch. The
	    ///     transform matrix and the stage's camera must not have rotational components. Calling this method must be followed
	    ///     by a call
	    ///     to clipEnd() if true is returned.
	    /// </summary>
	    public bool ClipBegin(Batcher batcher, float x, float y, float width, float height)
        {
            if (width <= 0 || height <= 0)
                return false;

            var tableBounds = RectangleExt.FromFloats(x, y, width, height);
            var scissorBounds =
                ScissorStack.CalculateScissors(Stage?.Entity?.Scene?.Camera, batcher.TransformMatrix, tableBounds);
            if (ScissorStack.PushScissors(scissorBounds))
            {
                batcher.EnableScissorTest(true);
                return true;
            }

            return false;
        }


	    /// <summary>
	    ///     Ends clipping begun by clipBegin(Batcher, float, float, float, float)
	    /// </summary>
	    /// <returns>The end.</returns>
	    public void ClipEnd(Batcher batcher)
        {
            batcher.EnableScissorTest(false);
            ScissorStack.PopScissors();
        }


	    /// <summary>
	    ///     If true, {@link #debugDraw} will be called for this element
	    /// </summary>
	    /// <param name="enabled">Enabled.</param>
	    public virtual void SetDebug(bool enabled)
        {
            Debug = enabled;
            if (enabled)
                Stage.Debug = true;
        }


        public bool GetDebug()
        {
            return Debug;
        }

        #endregion


        #region Coordinate conversion

	    /// <summary>
	    ///     Transforms the specified point in screen coordinates to the element's local coordinate system
	    /// </summary>
	    /// <returns>The to local coordinates.</returns>
	    /// <param name="screenCoords">Screen coords.</param>
	    public Vector2 ScreenToLocalCoordinates(Vector2 screenCoords)
        {
            if (Stage == null)
                return screenCoords;
            return StageToLocalCoordinates(Stage.ScreenToStageCoordinates(screenCoords));
        }


	    /// <summary>
	    ///     Transforms the specified point in the stage's coordinates to the element's local coordinate system.
	    /// </summary>
	    /// <returns>The to local coordinates.</returns>
	    /// <param name="stageCoords">Stage coords.</param>
	    public Vector2 StageToLocalCoordinates(Vector2 stageCoords)
        {
            if (Parent != null)
                stageCoords = Parent.StageToLocalCoordinates(stageCoords);

            stageCoords = ParentToLocalCoordinates(stageCoords);
            return stageCoords;
        }


	    /// <summary>
	    ///     Transforms the specified point in the element's coordinates to be in the stage's coordinates
	    /// </summary>
	    /// <returns>The to stage coordinates.</returns>
	    /// <param name="localCoords">Local coords.</param>
	    public Vector2 LocalToStageCoordinates(Vector2 localCoords)
        {
            return LocalToAscendantCoordinates(null, localCoords);
        }


	    /// <summary>
	    ///     Converts coordinates for this element to those of a parent element. The ascendant does not need to be a direct
	    ///     parent
	    /// </summary>
	    /// <returns>The to ascendant coordinates.</returns>
	    /// <param name="ascendant">Ascendant.</param>
	    /// <param name="localCoords">Local coords.</param>
	    public Vector2 LocalToAscendantCoordinates(Element ascendant, Vector2 localCoords)
        {
            var element = this;
            while (element != null)
            {
                localCoords = element.LocalToParentCoordinates(localCoords);
                element = element.Parent;
                if (element == ascendant)
                    break;
            }
            return localCoords;
        }


	    /// <summary>
	    ///     Converts the coordinates given in the parent's coordinate system to this element's coordinate system.
	    /// </summary>
	    /// <returns>The to local coordinates.</returns>
	    /// <param name="parentCoords">Parent coords.</param>
	    public Vector2 ParentToLocalCoordinates(Vector2 parentCoords)
        {
            if (Rotation == 0)
            {
                if (ScaleX == 1 && ScaleY == 1)
                {
                    parentCoords.X -= X;
                    parentCoords.Y -= Y;
                }
                else
                {
                    parentCoords.X = (parentCoords.X - X - OriginX) / ScaleX + OriginX;
                    parentCoords.Y = (parentCoords.Y - Y - OriginY) / ScaleY + OriginY;
                }
            }
            else
            {
                var cos = Mathf.Cos(MathHelper.ToRadians(Rotation));
                var sin = Mathf.Sin(MathHelper.ToRadians(Rotation));
                var tox = parentCoords.X - X - OriginX;
                var toy = parentCoords.Y - Y - OriginY;
                parentCoords.X = (tox * cos + toy * sin) / ScaleX + OriginX;
                parentCoords.Y = (tox * -sin + toy * cos) / ScaleY + OriginY;
            }

            return parentCoords;
        }


	    /// <summary>
	    ///     Transforms the specified point in the element's coordinates to be in the parent's coordinates.
	    /// </summary>
	    /// <returns>The to parent coordinates.</returns>
	    /// <param name="localCoords">Local coords.</param>
	    public Vector2 LocalToParentCoordinates(Vector2 localCoords)
        {
            var rotation = -this.Rotation;

            if (rotation == 0)
            {
                if (ScaleX == 1 && ScaleY == 1)
                {
                    localCoords.X += X;
                    localCoords.Y += Y;
                }
                else
                {
                    localCoords.X = (localCoords.X - OriginX) * ScaleX + OriginX + X;
                    localCoords.Y = (localCoords.Y - OriginY) * ScaleY + OriginY + Y;
                }
            }
            else
            {
                var cos = Mathf.Cos(MathHelper.ToRadians(rotation));
                var sin = Mathf.Sin(MathHelper.ToRadians(rotation));

                var tox = (localCoords.X - OriginX) * ScaleX;
                var toy = (localCoords.Y - OriginY) * ScaleY;
                localCoords.X = tox * cos + toy * sin + OriginX + X;
                localCoords.Y = tox * -sin + toy * cos + OriginY + Y;
            }

            return localCoords;
        }

        #endregion


        #region ILayout

        public bool FillParent { get; set; }

        public virtual bool LayoutEnabled
        {
            get => layoutEnabled;
            set
            {
                if (layoutEnabled != value)
                {
                    layoutEnabled = value;

                    if (layoutEnabled)
                        InvalidateHierarchy();
                }
            }
        }

        public virtual float MinWidth => PreferredWidth;

        public virtual float MinHeight => PreferredHeight;

        public virtual float PreferredWidth => 0;

        public virtual float PreferredHeight => 0;

        public virtual float MaxWidth => 0;

        public virtual float MaxHeight => 0;


        public virtual void Layout()
        {
        }


        public virtual void Invalidate()
        {
            needsLayout = true;
        }


        public virtual void InvalidateHierarchy()
        {
            if (!layoutEnabled)
                return;

            Invalidate();

            if (Parent is ILayout)
                ((ILayout) Parent).InvalidateHierarchy();
        }


        public void Validate()
        {
            if (!layoutEnabled)
                return;

            if (FillParent && Parent != null)
            {
                var stage = GetStage();
                float parentWidth, parentHeight;

                if (stage != null && Parent == stage.GetRoot())
                {
                    parentWidth = stage.GetWidth();
                    parentHeight = stage.GetHeight();
                }
                else
                {
                    parentWidth = Parent.GetWidth();
                    parentHeight = Parent.GetHeight();
                }

                if (Width != parentWidth || Height != parentHeight)
                {
                    SetSize(parentWidth, parentHeight);
                    Invalidate();
                }
            }

            if (!needsLayout)
                return;

            needsLayout = false;
            Layout();
        }


        public virtual void Pack()
        {
            SetSize(PreferredWidth, PreferredHeight);
            Validate();
        }

        #endregion
    }
}