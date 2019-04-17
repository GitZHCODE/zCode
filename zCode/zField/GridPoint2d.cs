﻿using System;

/*
 * Notes
 */

namespace zCode.zField
{
    /// <summary>
    /// Intermediate representation of a 2d grid query which stores the indices of the 4 nearest values along with weights used for interpolation.
    /// </summary>
    [Serializable]
    public class GridPoint2d
    {
        private readonly double[] _weights;
        private readonly int[] _corners;


        /// <summary>
        /// 
        /// </summary>
        public GridPoint2d()
        {
            _weights = new double[4];
            _corners = new int[4];
            Unset();
        }


        /// <summary>
        /// 
        /// </summary>
        public double[] Weights
        {
            get { return _weights; }
        }


        /// <summary>
        /// 
        /// </summary>
        public int[] Corners
        {
            get { return _corners; }
        }


        /// <summary>
        /// 
        /// </summary>
        public bool IsUnset
        {
            get { return _corners[0] == -1; }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="u"></param>
        /// <param name="v"></param>
        public void SetWeights(double u, double v)
        {
            double u1 = 1.0 - u;
            double v1 = 1.0 - v;
        
            _weights[0] = u1 * v1;
            _weights[1] = u * v1;
            _weights[2] = u1 * v;
            _weights[3] = u * v;
        }


        /// <summary>
        /// 
        /// </summary>
        public void Unset()
        {
            _corners[0] = -1;
        }
    }
}
