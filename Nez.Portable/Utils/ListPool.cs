using System.Collections.Generic;

namespace Nez.Utils
{
	/// <summary>
	///     simple static class that can be used to pool Lists
	/// </summary>
	public static class ListPool<T>
    {
        private static readonly Queue<List<T>> ObjectQueue = new Queue<List<T>>();


	    /// <summary>
	    ///     warms up the cache filling it with a max of cacheCount objects
	    /// </summary>
	    /// <param name="cacheCount">new cache count</param>
	    public static void WarmCache(int cacheCount)
        {
            cacheCount -= ObjectQueue.Count;
            if (cacheCount > 0)
                for (var i = 0; i < cacheCount; i++)
                    ObjectQueue.Enqueue(new List<T>());
        }


	    /// <summary>
	    ///     trims the cache down to cacheCount items
	    /// </summary>
	    /// <param name="cacheCount">Cache count.</param>
	    public static void TrimCache(int cacheCount)
        {
            while (cacheCount > ObjectQueue.Count)
                ObjectQueue.Dequeue();
        }


	    /// <summary>
	    ///     clears out the cache
	    /// </summary>
	    public static void ClearCache()
        {
            ObjectQueue.Clear();
        }


	    /// <summary>
	    ///     pops an item off the stack if available creating a new item as necessary
	    /// </summary>
	    public static List<T> Obtain()
        {
            if (ObjectQueue.Count > 0)
                return ObjectQueue.Dequeue();

            return new List<T>();
        }


	    /// <summary>
	    ///     pushes an item back on the stack
	    /// </summary>
	    /// <param name="obj">Object.</param>
	    public static void Free(List<T> obj)
        {
            ObjectQueue.Enqueue(obj);
            obj.Clear();
        }
    }
}