﻿
/*
 * Notes
 */

#if USING_RHINO

using Rhino.Geometry;

namespace zCode.zCore
{
    /// <summary>
    /// Represents a double precision interval.
    /// </summary>
    public partial struct Intervald
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static implicit operator Intervald(Interval interval)
        {
            return new Intervald(interval.T0, interval.T1);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static implicit operator Interval(Intervald interval)
        {
            return new Interval(interval.A, interval.B);
        }
    }


    /// <summary>
    /// 
    /// </summary>
    public partial struct Vec2d
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static implicit operator Vec2d(Vector2d vector)
        {
            return new Vec2d(vector.X, vector.Y);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static implicit operator Vector2d(Vec2d vector)
        {
            return new Vector2d(vector.X, vector.Y);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static implicit operator Vec2d(Vector2f vector)
        {
            return new Vec2d(vector.X, vector.Y);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static explicit operator Vector2f(Vec2d vector)
        {
            return new Vector2f()
            {
                X = (float)vector.X,
                Y = (float)vector.Y
            };
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static implicit operator Vec2d(Point2d point)
        {
            return new Vec2d(point.X, point.Y);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static implicit operator Point2d(Vec2d vector)
        {
            return new Point2d(vector.X, vector.Y);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static implicit operator Vec2d(Point2f point)
        {
            return new Vec2d(point.X, point.Y);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static explicit operator Point2f(Vec2d vector)
        {
            return new Point2f((float)vector.X, (float)vector.Y);
        }
    }


    /// <summary>
    /// 
    /// </summary>
    public partial struct Vec3d
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static implicit operator Vec3d(Vector3d vector)
        {
            return new Vec3d(vector.X, vector.Y, vector.Z);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static implicit operator Vector3d(Vec3d vector)
        {
            return new Vector3d(vector.X, vector.Y, vector.Z);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static implicit operator Vec3d(Vector3f vector)
        {
            return new Vec3d(vector.X, vector.Y, vector.Z);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static explicit operator Vector3f(Vec3d vector)
        {
            return new Vector3f((float)vector.X, (float)vector.Y, (float)vector.Z);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static implicit operator Vec3d(Point3d point)
        {
            return new Vec3d(point.X, point.Y, point.Z);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static implicit operator Point3d(Vec3d vector)
        {
            return new Point3d(vector.X, vector.Y, vector.Z);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static implicit operator Vec3d(Point3f point)
        {
            return new Vec3d(point.X, point.Y, point.Z);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static explicit operator Point3f(Vec3d vector)
        {
            return new Point3f((float)vector.X, (float)vector.Y, (float)vector.Z);
        }
    }


    /// <summary>
    /// 
    /// </summary>
    public partial struct Vec4d
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static implicit operator Vec4d(Point4d point)
        {
            return new Vec4d(point.X, point.Y, point.Z, point.W);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static implicit operator Point4d(Vec4d vector)
        {
            return new Point4d(vector.X, vector.Y, vector.Z, vector.W);
        }
    }


    /// <summary>
    /// 
    /// </summary>
    public partial struct Quaterniond
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rotation"></param>
        public static implicit operator Quaterniond(Quaternion rotation)
        {
            return new Quaterniond(rotation.B, rotation.C, rotation.D, rotation.A);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="rotation"></param>
        public static implicit operator Quaternion(Quaterniond rotation)
        {
            return new Quaternion(rotation.W, rotation.X, rotation.Y, rotation.Z);
        }
    }


    /// <summary>
    /// 
    /// </summary>
    public partial struct Matrix4d
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="transform"></param>
        public static implicit operator Matrix4d(Transform transform)
        {
            return new Matrix4d(
                transform.M00, transform.M01, transform.M02, transform.M03,
                transform.M10, transform.M11, transform.M12, transform.M13,
                transform.M20, transform.M21, transform.M22, transform.M23,
                transform.M30, transform.M31, transform.M32, transform.M33
                );
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="transform"></param>
        public static implicit operator Transform(Matrix4d transform)
        {
            var m = Transform.Identity;

            m.M00 = transform.M00;
            m.M01 = transform.M01;
            m.M02 = transform.M02;
            m.M03 = transform.M03;

            m.M10 = transform.M10;
            m.M11 = transform.M11;
            m.M12 = transform.M12;
            m.M13 = transform.M13;

            m.M20 = transform.M20;
            m.M21 = transform.M21;
            m.M22 = transform.M22;
            m.M23 = transform.M23;

            m.M30 = transform.M30;
            m.M31 = transform.M31;
            m.M32 = transform.M32;
            m.M33 = transform.M33;

            return m;
        }
    }


    /// <summary>
    /// 
    /// </summary>
    public partial struct Interval3d
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="interval"></param>
        public static implicit operator Interval3d(BoundingBox bounds)
        {
            return new Interval3d(bounds.Min, bounds.Max);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="interval"></param>
        public static implicit operator BoundingBox(Interval3d interval)
        {
            return new BoundingBox(interval.Min, interval.Max);
        }
    }
}

#endif