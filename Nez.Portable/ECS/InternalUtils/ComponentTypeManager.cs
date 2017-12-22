using System;
using System.Collections.Generic;

namespace Nez
{
    public static class ComponentTypeManager
    {
        private static readonly Dictionary<Type, int> ComponentTypesMask = new Dictionary<Type, int>();


        public static void Add(Type type)
        {
            int v;
            if (!ComponentTypesMask.TryGetValue(type, out v))
                ComponentTypesMask[type] = ComponentTypesMask.Count;
        }


        public static int GetIndexFor(Type type)
        {
            var v = -1;
            if (!ComponentTypesMask.TryGetValue(type, out v))
            {
                Add(type);
                ComponentTypesMask.TryGetValue(type, out v);
            }

            return v;
        }


        public static IEnumerable<Type> GetTypesFromBits(BitSet bits)
        {
            foreach (var keyValuePair in ComponentTypesMask)
                if (bits.Get(keyValuePair.Value))
                    yield return keyValuePair.Key;
        }
    }
}