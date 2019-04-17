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
    internal class IDWField3dVec3d : IDWField3d<Vec3d>
    {
        #region Nested Types

        /// <summary>
        /// 
        /// </summary>
        internal class Factory : IDWFieldFactory<Vec3d>
        {
            /// <inheritdoc />
            public override IDWField3d<Vec3d> Create(double power, double epsilon = zMath.ZeroTolerance)
            {
                return new IDWField3dVec3d(power, epsilon);
            }
        }

        #endregion


        /// <summary>
        /// 
        /// </summary>
        public IDWField3dVec3d(double power, double epsilon = zMath.ZeroTolerance)
            : base(power, epsilon)
        {
        }


        /// <inheritdoc />
        public sealed override IDWField3d<Vec3d> Duplicate()
        {
            return IDWField3d.Vec3d.CreateCopy(this);
        }


        /// <inheritdoc />
        public sealed override Vec3d ValueAt(Vec3d point)
        {
            Vec3d sum = Vec3d.Zero;
            double wsum = 0.0;

            foreach (var obj in Objects)
            {
                double w = obj.Influence / Math.Pow(obj.DistanceTo(point) + Epsilon, Power);
                sum += obj.Value * w;
                wsum += w;
            }

            return (wsum > 0.0) ? sum / wsum : new Vec3d();
        }


        /// <inheritdoc />
        public sealed override void GradientAt(Vec3d point, out Vec3d gx, out Vec3d gy, out Vec3d gz)
        {
            throw new NotImplementedException();
        }
    }
}
