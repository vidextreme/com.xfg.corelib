using System;
using UnityEngine;

namespace XFG.Math
{
    public static class Vector2Math
    {
        #region Constants

        /// <summary>
        /// Minimum squared magnitude required to treat a vector as non-degenerate.
        /// This threshold prevents division by zero and reduces numerical instability
        /// when normalizing or projecting extremely small vectors.
        /// </summary>
        private const float EPSILON_SQR_MAG = 1e-12f;

        #endregion


        #region Direction Utilities

        /// <summary>
        /// Computes a normalized direction vector pointing from "from" to "to".
        /// If the two points coincide, the result is Vector2.zero to avoid division by zero.
        /// </summary>
        public static Vector2 DirectionTo(this Vector2 from, Vector2 to)
        {
            Vector2 delta = to - from;
            return delta.sqrMagnitude > 0f
                ? delta / Mathf.Sqrt(delta.sqrMagnitude)
                : Vector2.zero;
        }

        /// <summary>
        /// Attempts to compute a normalized direction from "from" to "to".
        /// Returns true if the direction is well-defined; otherwise returns false and outputs
        /// Vector2.zero. This avoids unstable normalization when the points coincide.
        /// </summary>
        public static bool TryDirectionTo(this Vector2 from, Vector2 to, out Vector2 direction)
        {
            Vector2 delta = to - from;
            float sqr = delta.sqrMagnitude;

            if (sqr > EPSILON_SQR_MAG)
            {
                direction = delta / Mathf.Sqrt(sqr);
                return true;
            }

            direction = Vector2.zero;
            return false;
        }

        #endregion


        #region Distance Utilities

        /// <summary>
        /// Computes the Euclidean distance between two points.
        /// This is equivalent to (a - b).magnitude and involves a square root.
        /// </summary>
        public static float DistanceTo(this Vector2 a, Vector2 b)
            => (a - b).magnitude;

        /// <summary>
        /// Computes the squared Euclidean distance between two points.
        /// This avoids the cost of a square root and is preferred for comparisons.
        /// </summary>
        public static float DistanceSqrTo(this Vector2 a, Vector2 b)
            => (a - b).sqrMagnitude;

        #endregion


        #region Projection Utilities

        /// <summary>
        /// Projects vector v onto direction n, assuming n is already normalized.
        /// No safety checks are performed.
        /// </summary>
        public static Vector2 ProjectOnto(this Vector2 v, Vector2 n)
            => Vector2.Dot(v, n) * n;

        /// <summary>
        /// Projects vector v onto an arbitrary vector n.
        /// If n is too small to normalize safely, the result is Vector2.zero.
        /// This avoids division by extremely small magnitudes.
        /// </summary>
        public static Vector2 ProjectOntoSafe(this Vector2 v, Vector2 n)
        {
            float sqr = n.sqrMagnitude;
            return sqr > EPSILON_SQR_MAG
                ? Vector2.Dot(v, n) / sqr * n
                : Vector2.zero;
        }

        #endregion


        #region Normalization Utilities

        /// <summary>
        /// Returns a normalized version of v. If the vector is too small to
        /// normalize safely, Vector2.zero is returned to avoid unstable division.
        /// </summary>
        public static Vector2 NormalizeSafe(this Vector2 v)
        {
            float sqr = v.sqrMagnitude;
            return sqr > EPSILON_SQR_MAG
                ? v / Mathf.Sqrt(sqr)
                : Vector2.zero;
        }

        /// <summary>
        /// Attempts to normalize v. Returns true if the vector has
        /// sufficient magnitude to normalize; otherwise returns false and outputs
        /// Vector2.zero. This prevents unstable results from tiny vectors.
        /// </summary>
        public static bool TryNormalize(this Vector2 v, out Vector2 normalized)
        {
            float sqr = v.sqrMagnitude;

            if (sqr > EPSILON_SQR_MAG)
            {
                normalized = v / Mathf.Sqrt(sqr);
                return true;
            }

            normalized = Vector2.zero;
            return false;
        }

        #endregion


        #region Angle Utilities

        /// <summary>
        /// Computes the angle in degrees between vectors a and b.
        /// If either vector is degenerate, the angle is defined as 0 degrees.
        /// The dot product is clamped to [-1, 1] to protect against floating point drift.
        /// </summary>
        public static float AngleTo(this Vector2 a, Vector2 b)
        {
            float denom = Mathf.Sqrt(a.sqrMagnitude * b.sqrMagnitude);
            if (denom < EPSILON_SQR_MAG)
                return 0f;

            float dot = Mathf.Clamp(Vector2.Dot(a, b) / denom, -1f, 1f);
            return Mathf.Acos(dot) * Mathf.Rad2Deg;
        }

        #endregion


        #region Movement Utilities

        /// <summary>
        /// Moves "current" toward "target" by at most maxDelta.
        /// If the remaining distance is less than maxDelta, the target is returned directly.
        /// This method avoids overshooting and behaves consistently even when the points coincide.
        /// </summary>
        public static Vector2 MoveTowards(this Vector2 current, Vector2 target, float maxDelta)
        {
            Vector2 delta = target - current;
            float sqr = delta.sqrMagnitude;

            if (sqr <= maxDelta * maxDelta)
                return target;

            return current + delta / Mathf.Sqrt(sqr) * maxDelta;
        }

        #endregion


        #region Reflection Utilities

        /// <summary>
        /// Computes the reflection of this vector across the plane defined by normal n.
        /// The normal does not need to be normalized. If n is too small to use safely,
        /// the method returns Vector2.zero to avoid unstable division.
        /// Reflection is computed as: r = v - 2 * projection_of_v_onto_n.
        /// </summary>
        public static Vector2 Reflect(this Vector2 v, Vector2 n)
        {
            float sqr = n.sqrMagnitude;
            if (sqr < EPSILON_SQR_MAG)
                return Vector2.zero;

            float dot = Vector2.Dot(v, n);
            Vector2 projection = (dot / sqr) * n;

            return v - 2f * projection;
        }

        /// <summary>
        /// Attempts to compute the reflection of this vector across the plane defined by normal n.
        /// The normal does not need to be normalized. If n is too small to use safely,
        /// the method returns false and outputs Vector2.zero to avoid unstable division.
        /// Reflection is computed as: r = v - 2 * projection_of_v_onto_n.
        /// </summary>
        public static bool TryReflect(this Vector2 v, Vector2 n, out Vector2 reflected)
        {
            float sqr = n.sqrMagnitude;
            if (sqr < EPSILON_SQR_MAG)
            {
                reflected = Vector2.zero;
                return false;
            }

            float dot = Vector2.Dot(v, n);
            Vector2 projection = (dot / sqr) * n;

            reflected = v - 2f * projection;
            return true;
        }

        /// <summary>
        /// Computes the reflection of this vector across the plane defined by normal n.
        /// The normal is normalized internally. If n is too small to normalize safely,
        /// the method returns Vector2.zero to avoid unstable division.
        /// </summary>
        public static Vector2 ReflectSafe(this Vector2 v, Vector2 n)
        {
            float sqr = n.sqrMagnitude;
            if (sqr < EPSILON_SQR_MAG)
                return Vector2.zero;

            Vector2 nn = n / Mathf.Sqrt(sqr);
            float dot = Vector2.Dot(v, nn);

            return v - 2f * dot * nn;
        }

        /// <summary>
        /// Computes the reflection of this vector across the plane defined by normal n.
        /// Assumes n is already normalized. No safety checks are performed.
        /// </summary>
        public static Vector2 ReflectNormalized(this Vector2 v, Vector2 n)
        {
            float dot = Vector2.Dot(v, n);
            return v - 2f * dot * n;
        }

        #endregion


        #region Other Utilities

        /// <summary>
        /// Computes the centroid (average position) of the provided points.
        /// If the span is empty, Vector2.zero is returned.
        /// </summary>
        public static Vector2 Midpoint(ReadOnlySpan<Vector2> points)
        {
            if (points.Length == 0)
                return Vector2.zero;

            Vector2 sum = Vector2.zero;

            for (int i = 0; i < points.Length; i++)
                sum += points[i];

            return sum / points.Length;
        }

        #endregion
    }
}
