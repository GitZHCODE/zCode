﻿
/*
 * Notes
 */

namespace zCode.zMesh
{
    /// <summary>
    /// Enum of boundary types for smoothing.
    /// </summary>
    public enum SmoothBoundaryType
    {
        /// <summary>All boundary vertices are fixed.</summary>
        Fixed,
        /// <summary>Only degree 2 boundary vertices are fixed.</summary>
        CornerFixed,
        /// <summary>Boundary vertices are free.</summary>
        Free
    }
}
