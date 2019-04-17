﻿using System.Collections.Generic;

/*
 * Notes
 */

namespace zCode.zCore
{
    /// <summary>
    /// 
    /// </summary>
    public static class GenericExtension
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        public static IEnumerable<T> Yield<T>(this T item)
        {
            yield return item;
        }
    }
}
