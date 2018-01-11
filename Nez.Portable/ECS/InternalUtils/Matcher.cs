using System;
using System.Text;
using Nez.Utils;

namespace Nez.ECS.InternalUtils
{
    public class Matcher
    {
        protected BitSet AllSet = new BitSet();
        protected BitSet ExclusionSet = new BitSet();
        protected BitSet OneSet = new BitSet();


        public BitSet GetAllSet()
        {
            return AllSet;
        }


        public BitSet GetExclusionSet()
        {
            return ExclusionSet;
        }


        public BitSet GetOneSet()
        {
            return OneSet;
        }


        public bool IsInterested(Entity e)
        {
            return IsInterested(e.ComponentBits);
        }


        public bool IsInterested(BitSet componentBits)
        {
            // Check if the entity possesses ALL of the components defined in the aspect.
            if (!AllSet.IsEmpty())
                for (var i = AllSet.NextSetBit(0); i >= 0; i = AllSet.NextSetBit(i + 1))
                    if (!componentBits.Get(i))
                        return false;

            // If we are STILL interested,
            // Check if the entity possesses ANY of the exclusion components, if it does then the system is not interested.
            if (!ExclusionSet.IsEmpty() && ExclusionSet.Intersects(componentBits))
                return false;

            // If we are STILL interested,
            // Check if the entity possesses ANY of the components in the oneSet. If so, the system is interested.
            if (!OneSet.IsEmpty() && !OneSet.Intersects(componentBits))
                return false;

            return true;
        }


        public Matcher All(params Type[] types)
        {
            foreach (var type in types)
                AllSet.Set(ComponentTypeManager.GetIndexFor(type));

            return this;
        }


        public Matcher Exclude(params Type[] types)
        {
            foreach (var type in types)
                ExclusionSet.Set(ComponentTypeManager.GetIndexFor(type));

            return this;
        }


        public Matcher One(params Type[] types)
        {
            foreach (var type in types)
                OneSet.Set(ComponentTypeManager.GetIndexFor(type));

            return this;
        }


        public static Matcher Empty()
        {
            return new Matcher();
        }


        public override string ToString()
        {
            var builder = new StringBuilder(1024);

            builder.AppendLine("Matcher:");
            AppendTypes(builder, " -  Requires the components: ", AllSet);
            AppendTypes(builder, " -  Has none of the components: ", ExclusionSet);
            AppendTypes(builder, " -  Has at least one of the components: ", OneSet);

            return builder.ToString();
        }


        private static void AppendTypes(StringBuilder builder, string headerMessage, BitSet typeBits)
        {
            var firstType = true;
            builder.Append(headerMessage);
            foreach (var type in ComponentTypeManager.GetTypesFromBits(typeBits))
            {
                if (!firstType)
                    builder.Append(", ");
                builder.Append(type.Name);

                firstType = false;
            }

            builder.AppendLine();
        }
    }
}