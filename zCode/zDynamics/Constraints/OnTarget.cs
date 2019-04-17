﻿using System;
using System.Collections.Generic;
using zCode.zCore;

/*
 * Notes
 */

namespace zCode.zDynamics
{
    using H = ParticleHandle;

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class OnTarget<T> : Constraint, IConstraint
        where T : class
    {
        private H _handle = new H();
        private T _target;
        private Func<T, Vec3d, Vec3d> _closestPoint;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="closestPoint"></param>
        /// <param name="parallel"></param>
        /// <param name="capacity"></param>
        /// <param name="weight"></param>
        public OnTarget(int index, T target, Func<T, Vec3d, Vec3d> closestPoint, double weight = 1.0)
        {
            _handle.Index = index;
            Target = target;
            _closestPoint = closestPoint;
            Weight = weight;
        }


        /// <summary>
        /// 
        /// </summary>
        public H Handle
        {
            get { return _handle; }
            set { _handle = value; }
        }


        /// <summary>
        /// 
        /// </summary>
        public T Target
        {
            get { return _target; }
            set { _target = value ?? throw new ArgumentNullException(); }
        }


        /// <inheritdoc />
        public ConstraintType Type
        {
            get { return ConstraintType.Position; }
        }


        /// <inheritdoc />
        public void Calculate(IReadOnlyList<IBody> bodies)
        {
            var p = bodies[_handle].Position;
            _handle.Delta = _closestPoint(_target, p) - p;
        }


        /// <inheritdoc />
        public void Apply(IReadOnlyList<IBody> bodies)
        {
            bodies[_handle].ApplyMove(_handle.Delta, Weight);
        }


        #region Explicit interface implementations

        /// <inheritdoc />
        IEnumerable<IHandle> IConstraint.Handles
        {
            get { yield return _handle; }
        }

        #endregion
    }
}
