using System.Collections;
using System.Collections.Generic;
using Nez.Debug;

namespace Nez.Utils.Extensions
{
    public static class EnumerableExt
    {
	    /// <summary>
	    ///     Jon Skeet's excellent reimplementation of LINQ Count.
	    /// </summary>
	    /// <typeparam name="TSource">The source type.</typeparam>
	    /// <param name="source">The source IEnumerable.</param>
	    /// <returns>The number of items in the source.</returns>
	    public static int Count<TSource>(this IEnumerable<TSource> source)
        {
            Assert.IsNotNull(source, "source cannot be null");

            // Optimization for ICollection<T> 
            var genericCollection = source as ICollection<TSource>;
            if (genericCollection != null)
                return genericCollection.Count;

            // Optimization for ICollection 
            var nonGenericCollection = source as ICollection;
            if (nonGenericCollection != null)
                return nonGenericCollection.Count;

            // Do it the slow way - and make sure we overflow appropriately 
            checked
            {
                var count = 0;
                using (var iterator = source.GetEnumerator())
                {
                    while (iterator.MoveNext())
                        count++;
                }
                return count;
            }
        }
    }
}