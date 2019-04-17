﻿using System;

using zCode.zCore;

/*
 * Notes
 */

namespace zCode.zDynamics
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class BodyHandle : ParticleHandle, IHandle
    {
        private Vec3d _angleDelta;
        

        /// <summary>
        /// 
        /// </summary>
        public BodyHandle()
        {
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        public BodyHandle(int index)
            :base(index)
        {
        }


        /// <inheritdoc />
        public Vec3d AngleDelta
        {
            get => _angleDelta;
            set => _angleDelta = value;
        }
    }
}
