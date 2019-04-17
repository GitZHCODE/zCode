﻿
/*
 * Notes
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace zCode.zMesh
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    class HeNodeList<T, E> : HeElementList<T>
        where T : HeNode<T, E>
        where E : Halfedge<E>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="capacity"></param>
        public HeNodeList(int capacity)
            :base(capacity)
        {
        }


        /// <inheritdoc/>
        public override int CountUnused()
        {
            var items = Items;
            int result = 0;

            for (int i = 0; i < Count; i++)
                if (items[i].IsUnused) result++;

            return result;
        }


        /// <inheritdoc/>
        public override void Compact()
        {
            var items = Items;
            int marker = 0;

            for (int i = 0; i < Count; i++)
            {
                T element = items[i];
                if (element.IsUnused) continue; // skip unused elements

                element.Index = marker;
                items[marker++] = element;
            }

            AfterCompact(marker);
        }


        /// <inheritdoc/>
        public override void CompactAttributes<A>(List<A> attributes)
        {
            int marker = SwimAttributes(attributes);
            attributes.RemoveRange(marker, attributes.Count - marker);
        }


        /// <inheritdoc/>
        public override int SwimAttributes<A>(IList<A> attributes)
        {
            if(attributes is A[] arr)
                return SwimAttributes(arr);

            var items = Items;
            int marker = 0;

            for (int i = 0; i < Count; i++)
            {
                if (items[i].IsUnused) continue; // skip unused elements
                attributes[marker++] = attributes[i];
            }

            return marker;
        }


        /// <inheritdoc/>
        public override int SwimAttributes<A>(A[] attributes)
        {
            var items = Items;
            int marker = 0;

            for (int i = 0; i < Count; i++)
            {
                if (items[i].IsUnused) continue; // skip unused elements
                attributes[marker++] = attributes[i];
            }

            return marker;
        }
    }
}
