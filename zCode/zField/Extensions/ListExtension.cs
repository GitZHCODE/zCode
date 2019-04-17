﻿
/*
 * Notes
 */
 
using System.Collections.Generic;
using zCode.zField;

namespace zCode.zCore
{
    /// <summary>
    /// 
    /// </summary>
    public static partial class ListExtension
    {
        #region List<IDWObject3d<T>>

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objects"></param>
        /// <param name="point"></param>
        /// <param name="value"></param>
        /// <param name="influence"></param>
        public static void Add<T>(this List<IDWObject3d<T>> objects, Vec3d point, T value, double influence = 1.0)
        {
            objects.Add(IDWPoint3d.Create(point, value, influence));
        }

        #endregion
    }
}
