﻿using System;
using zCode.zCore;

/*
 * Notes
 */

namespace zCode.zField
{
    /// <summary>
    ///
    /// </summary>
    [Serializable]
    internal class GridField2dVec2d : GridField2d<Vec2d>
    {
        #region Nested Types

        /// <summary>
        /// 
        /// </summary>
        internal class Factory : GridField2dFactory<Vec2d>
        {
            /// <inheritdoc />
            public override GridField2d<Vec2d> Create(int countX, int countY)
            {
                return new GridField2dVec2d(countX, countY);
            }


            /// <inheritdoc />
            public override GridField2d<Vec2d> Create(Grid2d grid)
            {
                return new GridField2dVec2d(grid);
            }
        }

        #endregion


        /// <summary>
        /// 
        /// </summary>
        /// <param name="countX"></param>
        /// <param name="countY"></param>
        public GridField2dVec2d(int countX, int countY)
           : base(countX, countY)
        {
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <param name="sampleMode"></param>
        public GridField2dVec2d(Grid2d other)
            : base(other)
        {
        }


        /// <inheritdoc />
        public sealed override GridField2d<Vec2d> Duplicate(bool setValues)
        {
            var result = GridField2d.Vec2d.Create(this);
            if (setValues) result.Set(this);
            return result;
        }

        
        /// <inheritdoc />
        protected sealed override Vec2d ValueAtLinear(Vec2d point)
        {
            point = ToGridSpace(point);
            double u = zMath.Fract(point.X, out int i0);
            double v = zMath.Fract(point.Y, out int j0);

            int i1 = WrapX(i0 + 1);
            int j1 = WrapY(j0 + 1) * CountX;

            i0 = WrapX(i0);
            j0 = WrapY(j0) * CountX;

            var vals = Values;
            return Vec2d.Lerp(
                Vec2d.Lerp(vals[i0 + j0], vals[i1 + j0], u),
                Vec2d.Lerp(vals[i0 + j1], vals[i1 + j1], u),
                v);
        }


        /// <inheritdoc />
        protected sealed override Vec2d ValueAtLinearUnsafe(Vec2d point)
        {
            point = ToGridSpace(point);
            double u = zMath.Fract(point.X, out int i0);
            double v = zMath.Fract(point.Y, out int j0);

            j0 *= CountX;
            int i1 = i0 + 1;
            int j1 = j0 + CountX;

            var vals = Values;
            return Vec2d.Lerp(
                Vec2d.Lerp(vals[i0 + j0], vals[i1 + j0], u),
                Vec2d.Lerp(vals[i0 + j1], vals[i1 + j1], u),
                v);
        }


        /// <inheritdoc />
        public sealed override Vec2d ValueAt(GridPoint2d point)
        {
            return FieldUtil.ValueAt(Values, point.Corners, point.Weights);
        }
    }
}
