﻿using zCode.zCore;

/*
 * Notes
 */ 

namespace zCode.zField
{
    /// <summary>
    /// Interface for a spatially varying function in 3 dimensions.
    /// </summary>
    public interface IField3d<T>
    {
        /// <summary>
        /// Returns the value at the given point.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        T ValueAt(Vec3d point);
    }
}
