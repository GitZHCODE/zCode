﻿using System.Collections.Generic;

using zCode.zCore;

/*
 * Notes
 */

namespace zCode.zField
{
    /// <summary>
    /// 
    /// </summary>
    public static class IField2dExtension
    {
        #region IField2d<Vec2d>

        /// <summary>
        /// 
        /// </summary>
        /// <param name="field"></param>
        /// <param name="point"></param>
        /// <param name="stepSize"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public static IEnumerable<Vec2d> IntegrateFrom(this IField2d<Vec2d> field, Vec2d point, double stepSize, IntegrationMode mode = IntegrationMode.Euler)
        {
            return SimulationUtil.IntegrateFrom(field, point, stepSize, mode);
        }

        #endregion
    }
}
