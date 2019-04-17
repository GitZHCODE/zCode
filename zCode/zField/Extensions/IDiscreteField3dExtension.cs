﻿using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

/*
 * Notes
 */

namespace zCode.zField
{
    /// <summary>
    /// 
    /// </summary>
    public static class IDiscreteField3dExtension
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field"></param>
        /// <returns></returns>
        public static IDiscreteField3d<T> Duplicate<T>(this IDiscreteField3d<T> field)
            where T : struct
        {
            return field.Duplicate(true);
        }
        

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field"></param>
        /// <param name="other"></param>
        /// <param name="parallel"></param>
        public static void Sample<T>(this IDiscreteField3d<T> field, IField3d<T> other, bool parallel = false)
        {
            if (parallel)
                Parallel.ForEach(Partitioner.Create(0, field.Count), range => Body(range.Item1, range.Item2));
            else
                Body(0, field.Count);

            void Body(int from, int to)
            {
                var vals = field.Values;

                for (int i = from; i < to; i++)
                    vals[i] = other.ValueAt(field.CoordinateAt(i));
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="field"></param>
        /// <param name="other"></param>
        /// <param name="converter"></param>
        /// <param name="parallel"></param>
        public static void Sample<T, U>(this IDiscreteField3d<T> field, IField3d<U> other, Func<U, T> converter, bool parallel = false)
        {
            if (parallel)
                Parallel.ForEach(Partitioner.Create(0, field.Count), range => Body(range.Item1, range.Item2));
            else
                Body(0, field.Count);

            void Body(int from, int to)
            {
                var vals = field.Values;

                for (int i = from; i < to; i++)
                    vals[i] = converter(other.ValueAt(field.CoordinateAt(i)));
            }
        }
    }
}
