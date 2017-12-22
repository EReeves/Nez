using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Nez.Spatial
{
	/// <summary>
	///     A QuadTree Object that provides fast and efficient storage of objects in a world space.
	/// </summary>
	/// <typeparam name="T">Any object implementing IQuadStorable.</typeparam>
	public class QuadTreeNode<T> where T : IQuadTreeStorable
    {
        // How many objects can exist in a QuadTree before it sub divides itself
        private const int MaxObjectsPerNode = 2;

        // Bottom Left Child

        // Bottom Right Child

        // Top Left Child

        // Top Right Child


        // The objects in this QuadTree
        private List<QuadTreeObject<T>> _objects;

        // The parent of this quad

        // The area this QuadTree represents
        private Rectangle _rect;


	    /// <summary>
	    ///     Creates a QuadTree for the specified area.
	    /// </summary>
	    /// <param name="rect">The area this QuadTree object will encompass.</param>
	    public QuadTreeNode(Rectangle rect)
        {
            _rect = rect;
        }


	    /// <summary>
	    ///     Creates a QuadTree for the specified area.
	    /// </summary>
	    /// <param name="x">The top-left position of the area rectangle.</param>
	    /// <param name="y">The top-right position of the area rectangle.</param>
	    /// <param name="width">The width of the area rectangle.</param>
	    /// <param name="height">The height of the area rectangle.</param>
	    public QuadTreeNode(int x, int y, int width, int height)
        {
            _rect = new Rectangle(x, y, width, height);
        }


        private QuadTreeNode(QuadTreeNode<T> parent, Rectangle rect) : this(rect)
        {
            this.Parent = parent;
        }


	    /// <summary>
	    ///     The area this QuadTree represents.
	    /// </summary>
	    public Rectangle QuadRect => _rect;

	    /// <summary>
	    ///     The top left child for this QuadTree
	    /// </summary>
	    public QuadTreeNode<T> TopLeftChild { get; private set; }

	    /// <summary>
	    ///     The top right child for this QuadTree
	    /// </summary>
	    public QuadTreeNode<T> TopRightChild { get; private set; }

	    /// <summary>
	    ///     The bottom left child for this QuadTree
	    /// </summary>
	    public QuadTreeNode<T> BottomLeftChild { get; private set; }

	    /// <summary>
	    ///     The bottom right child for this QuadTree
	    /// </summary>
	    public QuadTreeNode<T> BottomRightChild { get; private set; }

	    /// <summary>
	    ///     This QuadTree's parent
	    /// </summary>
	    public QuadTreeNode<T> Parent { get; }

	    /// <summary>
	    ///     How many total objects are contained within this QuadTree (ie, includes children)
	    /// </summary>
	    public int Count => ObjectCount();

	    /// <summary>
	    ///     Returns true if this is a empty leaf node
	    /// </summary>
	    public bool IsEmptyLeaf => Count == 0 && TopLeftChild == null;


	    /// <summary>
	    ///     Add an item to the object list.
	    /// </summary>
	    /// <param name="item">The item to add.</param>
	    private void Add(QuadTreeObject<T> item)
        {
            if (_objects == null)
                _objects = new List<QuadTreeObject<T>>();

            item.Owner = this;
            _objects.Add(item);
        }


	    /// <summary>
	    ///     Remove an item from the object list.
	    /// </summary>
	    /// <param name="item">The object to remove.</param>
	    private void Remove(QuadTreeObject<T> item)
        {
            if (_objects != null)
            {
                var removeIndex = _objects.IndexOf(item);
                if (removeIndex >= 0)
                {
                    _objects[removeIndex] = _objects[_objects.Count - 1];
                    _objects.RemoveAt(_objects.Count - 1);
                }
            }
        }


	    /// <summary>
	    ///     Get the total for all objects in this QuadTree, including children.
	    /// </summary>
	    /// <returns>The number of objects contained within this QuadTree and its children.</returns>
	    private int ObjectCount()
        {
            var count = 0;

            // Add the objects at this level
            if (_objects != null)
                count += _objects.Count;

            // Add the objects that are contained in the children
            if (TopLeftChild != null)
            {
                count += TopLeftChild.ObjectCount();
                count += TopRightChild.ObjectCount();
                count += BottomLeftChild.ObjectCount();
                count += BottomRightChild.ObjectCount();
            }

            return count;
        }


	    /// <summary>
	    ///     Subdivide this QuadTree and move it's children into the appropriate Quads where applicable.
	    /// </summary>
	    private void Subdivide()
        {
            // We've reached capacity, subdivide...
            var size = new Point(_rect.Width / 2, _rect.Height / 2);
            var mid = new Point(_rect.X + size.X, _rect.Y + size.Y);

            TopLeftChild = new QuadTreeNode<T>(this, new Rectangle(_rect.Left, _rect.Top, size.X, size.Y));
            TopRightChild = new QuadTreeNode<T>(this, new Rectangle(mid.X, _rect.Top, size.X, size.Y));
            BottomLeftChild = new QuadTreeNode<T>(this, new Rectangle(_rect.Left, mid.Y, size.X, size.Y));
            BottomRightChild = new QuadTreeNode<T>(this, new Rectangle(mid.X, mid.Y, size.X, size.Y));

            // If they're completely contained by the quad, bump objects down
            for (var i = 0; i < _objects.Count; i++)
            {
                var destTree = GetDestinationTree(_objects[i]);

                if (destTree != this)
                {
                    // Insert to the appropriate tree, remove the object, and back up one in the loop
                    destTree.Insert(_objects[i]);
                    Remove(_objects[i]);
                    i--;
                }
            }
        }


	    /// <summary>
	    ///     Get the child Quad that would contain an object.
	    /// </summary>
	    /// <param name="item">The object to get a child for.</param>
	    /// <returns></returns>
	    private QuadTreeNode<T> GetDestinationTree(QuadTreeObject<T> item)
        {
            // If a child can't contain an object, it will live in this Quad
            var destTree = this;

            if (TopLeftChild.QuadRect.Contains(item.Data.Bounds))
                destTree = TopLeftChild;
            else if (TopRightChild.QuadRect.Contains(item.Data.Bounds))
                destTree = TopRightChild;
            else if (BottomLeftChild.QuadRect.Contains(item.Data.Bounds))
                destTree = BottomLeftChild;
            else if (BottomRightChild.QuadRect.Contains(item.Data.Bounds))
                destTree = BottomRightChild;

            return destTree;
        }


        private void Relocate(QuadTreeObject<T> item)
        {
            // Are we still inside our parent?
            if (QuadRect.Contains(item.Data.Bounds))
            {
                // Good, have we moved inside any of our children?
                if (TopLeftChild != null)
                {
                    var dest = GetDestinationTree(item);
                    if (item.Owner != dest)
                    {
                        // Delete the item from this quad and add it to our child
                        // Note: Do NOT clean during this call, it can potentially delete our destination quad
                        var formerOwner = item.Owner;
                        Delete(item, false);
                        dest.Insert(item);

                        // Clean up ourselves
                        formerOwner.CleanUpwards();
                    }
                }
            }
            else
            {
                // We don't fit here anymore, move up, if we can
                if (Parent != null)
                    Parent.Relocate(item);
            }
        }


        private void CleanUpwards()
        {
            if (TopLeftChild != null)
            {
                // If all the children are empty leaves, delete all the children
                if (TopLeftChild.IsEmptyLeaf && TopRightChild.IsEmptyLeaf && BottomLeftChild.IsEmptyLeaf &&
                    BottomRightChild.IsEmptyLeaf)
                {
                    TopLeftChild = null;
                    TopRightChild = null;
                    BottomLeftChild = null;
                    BottomRightChild = null;

                    if (Parent != null && Count == 0)
                        Parent.CleanUpwards();
                }
            }
            else
            {
                // I could be one of 4 empty leaves, tell my parent to clean up
                if (Parent != null && Count == 0)
                    Parent.CleanUpwards();
            }
        }


	    /// <summary>
	    ///     Clears the QuadTree of all objects, including any objects living in its children.
	    /// </summary>
	    internal void Clear()
        {
            // Clear out the children, if we have any
            if (TopLeftChild != null)
            {
                TopLeftChild.Clear();
                TopRightChild.Clear();
                BottomLeftChild.Clear();
                BottomRightChild.Clear();
            }

            // Clear any objects at this level
            if (_objects != null)
            {
                _objects.Clear();
                _objects = null;
            }

            // Set the children to null
            TopLeftChild = null;
            TopRightChild = null;
            BottomLeftChild = null;
            BottomRightChild = null;
        }


	    /// <summary>
	    ///     Deletes an item from this QuadTree. If the object is removed causes this Quad to have no objects in its children,
	    ///     it's children will be removed as well.
	    /// </summary>
	    /// <param name="item">The item to remove.</param>
	    /// <param name="clean">Whether or not to clean the tree</param>
	    internal void Delete(QuadTreeObject<T> item, bool clean)
        {
            if (item.Owner != null)
                if (item.Owner == this)
                {
                    Remove(item);
                    if (clean)
                        CleanUpwards();
                }
                else
                {
                    item.Owner.Delete(item, clean);
                }
        }


	    /// <summary>
	    ///     Insert an item into this QuadTree object.
	    /// </summary>
	    /// <param name="item">The item to insert.</param>
	    internal void Insert(QuadTreeObject<T> item)
        {
            // If this quad doesn't contain the items rectangle, do nothing, unless we are the root
            if (!_rect.Contains(item.Data.Bounds))
            {
                Assert.IsNull(Parent, "We are not the root, and this object doesn't fit here. How did we get here?");
                if (Parent == null)
                    Add(item);
                else
                    return;
            }

            if (_objects == null || TopLeftChild == null && _objects.Count + 1 <= MaxObjectsPerNode)
            {
                // If there's room to add the object, just add it
                Add(item);
            }
            else
            {
                // No quads, create them and bump objects down where appropriate
                if (TopLeftChild == null)
                    Subdivide();

                // Find out which tree this object should go in and add it there
                var destTree = GetDestinationTree(item);
                if (destTree == this)
                    Add(item);
                else
                    destTree.Insert(item);
            }
        }


	    /// <summary>
	    ///     Get the objects in this tree that intersect with the specified rectangle.
	    /// </summary>
	    /// <param name="searchRect">The rectangle to find objects in.</param>
	    internal List<T> GetObjects(Rectangle searchRect)
        {
            var results = new List<T>();
            GetObjects(searchRect, ref results);
            return results;
        }


	    /// <summary>
	    ///     Get the objects in this tree that intersect with the specified rectangle.
	    /// </summary>
	    /// <param name="searchRect">The rectangle to find objects in.</param>
	    /// <param name="results">A reference to a list that will be populated with the results.</param>
	    internal void GetObjects(Rectangle searchRect, ref List<T> results)
        {
            // We can't do anything if the results list doesn't exist
            if (results != null)
                if (searchRect.Contains(_rect))
                {
                    // If the search area completely contains this quad, just get every object this quad and all it's children have
                    GetAllObjects(ref results);
                }
                else if (searchRect.Intersects(_rect))
                {
                    // Otherwise, if the quad isn't fully contained, only add objects that intersect with the search rectangle
                    if (_objects != null)
                        for (var i = 0; i < _objects.Count; i++)
                            if (searchRect.Intersects(_objects[i].Data.Bounds))
                                results.Add(_objects[i].Data);

                    // Get the objects for the search rectangle from the children
                    if (TopLeftChild != null)
                    {
                        TopLeftChild.GetObjects(searchRect, ref results);
                        TopRightChild.GetObjects(searchRect, ref results);
                        BottomLeftChild.GetObjects(searchRect, ref results);
                        BottomRightChild.GetObjects(searchRect, ref results);
                    }
                }
        }


	    /// <summary>
	    ///     Get all objects in this Quad, and it's children.
	    /// </summary>
	    /// <param name="results">A reference to a list in which to store the objects.</param>
	    internal void GetAllObjects(ref List<T> results)
        {
            // If this Quad has objects, add them
            if (_objects != null)
                for (var i = 0; i < _objects.Count; i++)
                    results.Add(_objects[i].Data);

            // If we have children, get their objects too
            if (TopLeftChild != null)
            {
                TopLeftChild.GetAllObjects(ref results);
                TopRightChild.GetAllObjects(ref results);
                BottomLeftChild.GetAllObjects(ref results);
                BottomRightChild.GetAllObjects(ref results);
            }
        }


	    /// <summary>
	    ///     Moves the QuadTree object in the tree
	    /// </summary>
	    /// <param name="item">The item that has moved</param>
	    internal void Move(QuadTreeObject<T> item)
        {
            if (item.Owner != null)
                item.Owner.Relocate(item);
            else
                Relocate(item);
        }
    }
}