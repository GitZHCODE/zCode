﻿/*
 * Notes
 */

namespace zCode.zCore
{
    /// <summary>
    /// Represents an angle-preserving affine transformation in 2 dimensions.
    /// </summary>
    public struct Transform2d
    {
        #region Static

        /// <summary></summary>
        public static readonly Transform2d Identity = new Transform2d(new Vec2d(1.0), OrthoBasis2d.Identity, Vec2d.Zero);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="rotation"></param>
        public static implicit operator Transform2d(OrthoBasis2d rotation)
        {
            return new Transform2d(new Vec2d(1.0), rotation, Vec2d.Zero);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="orient"></param>
        public static implicit operator Transform2d(Orient2d orient)
        {
            return new Transform2d(new Vec2d(1.0), orient.Rotation, orient.Translation);
        }


        /// <summary>
        /// Applies the given transformation to the given point.
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Vec2d operator *(Transform2d transform, Vec2d point)
        {
            return transform.Apply(point);
        }


        /// <summary>
        /// Concatenates the given transformations by applying the first to the second.
        /// </summary>
        /// <param name="t0"></param>
        /// <param name="t1"></param>
        /// <returns></returns>
        public static Transform2d operator *(Transform2d t0, Transform2d t1)
        {
            t0.Apply(ref t1, ref t1);
            return t1;
        }


        /// <summary>
        /// Creates a relative transformation from t0 to t1.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static Transform2d CreateFromTo(Transform2d from, Transform2d to)
        {
            return to.Apply(from.Inverse);
        }

        #endregion


        /// <summary></summary>
        public OrthoBasis2d Rotation;
        /// <summary></summary>
        public Vec2d Translation;
        /// <summary></summary>
        public Vec2d Scale;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="scale"></param>
        /// <param name="rotation"></param>
        /// <param name="translation"></param>
        public Transform2d(Vec2d scale, OrthoBasis2d rotation, Vec2d translation)
        {
            Scale = scale;
            Rotation = rotation;
            Translation = translation;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="scale"></param>
        /// <param name="orientation"></param>
        public Transform2d(Vec2d scale, Orient2d orientation)
        {
            Scale = scale;
            Rotation = orientation.Rotation;
            Translation = orientation.Translation;
        }


        /// <summary>
        /// 
        /// </summary>
        public Transform2d Inverse
        {
            get
            {
                var result = this;
                result.Invert();
                return result;
            }
        }


        /// <summary>
        /// Return false if the rotation is undefined.
        /// </summary>
        bool IsValid
        {
            get { return Rotation.IsValid; }
        }


        /// <summary>
        /// Inverts this transformation in place.
        /// </summary>
        public void Invert()
        {
            Scale = 1.0 / Scale;
            Rotation.Invert();
            Translation = Rotation.Apply(-Translation) * Scale;
        }


        /// <summary>
        /// Applies this transformation to the given point.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public Vec2d Apply(Vec2d point)
        {
            return Rotation.Apply(point * Scale) + Translation;
        }


        /// <summary>
        /// Applies this transformation to the given transformation.
        /// </summary>
        /// <param name="other"></param>
        public Transform2d Apply(Transform2d other)
        {
            Apply(ref other, ref other);
            return other;
        }


        /// <summary>
        /// Applies this transformation to the given transformation in place.
        /// </summary>
        /// <param name="other"></param>
        public void Apply(ref Transform2d other)
        {
            Apply(ref other, ref other);
        }


        /// <summary>
        /// Applies this transformation to the given transformation.
        /// </summary>
        /// <param name="other"></param>
        public void Apply(ref Transform2d other, ref Transform2d result)
        {
            result.Rotation = Rotation.Apply(other.Rotation);
            result.Translation = Apply(other.Translation);
            result.Scale = other.Scale * Scale;
        }


        /// <summary>
        /// Applies the inverse of this transformation to the given point.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public Vec2d ApplyInverse(Vec2d point)
        {
            return Rotation.ApplyInverse(point - Translation) / Scale;
        }


        /// <summary>
        /// Applies the inverse of this transformation to the given transformation.
        /// </summary>
        /// <param name="other"></param>
        public Transform2d ApplyInverse(Transform2d other)
        {
            ApplyInverse(ref other, ref other);
            return other;
        }


        /// <summary>
        /// Applies the inverse of this transformation to the given transformation in place.
        /// </summary>
        /// <param name="other"></param>
        public void ApplyInverse(ref Transform2d other)
        {
            ApplyInverse(ref other, ref other);
        }


        /// <summary>
        /// Applies the inverse of this transformation to the given transformation.
        /// </summary>
        /// <param name="other"></param>
        public void ApplyInverse(ref Transform2d other, ref Transform2d result)
        {
            result.Rotation = Rotation.ApplyInverse(other.Rotation);
            result.Translation = ApplyInverse(other.Translation);
            result.Scale = other.Scale / Scale;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public bool ApproxEquals(ref Transform2d other, double tolerance = zMath.ZeroTolerance)
        {
            return
                Translation.ApproxEquals(other.Translation, tolerance) &&
                Rotation.ApproxEquals(other.Rotation, tolerance) &&
                Scale.ApproxEquals(other.Scale, tolerance);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Matrix3d ToMatrix()
        {
            var rx = Rotation.X * Scale.X;
            var ry = Rotation.Y * Scale.Y;

            return new Matrix3d(
                rx.X, ry.X, Translation.X,
                rx.Y, ry.Y, Translation.Y,
                0.0, 0.0, 1.0
                );
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void Deconstruct(out Vec2d scale, out OrthoBasis2d rotation, out Vec2d translation)
        {
            scale = Scale;
            rotation = Rotation;
            translation = Translation;
        }
    }
}
