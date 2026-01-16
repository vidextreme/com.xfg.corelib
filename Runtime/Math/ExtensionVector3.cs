using System;
using UnityEngine;

namespace XFG.Math
{
    public static class Vector3Math
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
        /// If the two points coincide, the result is Vector3.zero to avoid division by zero.
        /// </summary>
        public static Vector3 DirectionTo(this Vector3 from, Vector3 to)
        {
            Vector3 delta = to - from;
            return delta.sqrMagnitude > 0f
                ? delta / Mathf.Sqrt(delta.sqrMagnitude)
                : Vector3.zero;
        }

        /// <summary>
        /// Attempts to compute a normalized direction from "from" to "to".
        /// Returns true if the direction is well-defined; otherwise returns false and outputs
        /// Vector3.zero. This avoids unstable normalization when the points coincide.
        /// </summary>
        public static bool TryDirectionTo(this Vector3 from, Vector3 to, out Vector3 direction)
        {
            Vector3 delta = to - from;
            float sqr = delta.sqrMagnitude;

            if (sqr > EPSILON_SQR_MAG)
            {
                direction = delta / Mathf.Sqrt(sqr);
                return true;
            }

            direction = Vector3.zero;
            return false;
        }

        #endregion


        #region Distance Utilities

        /// <summary>
        /// Computes the Euclidean distance between two points.
        /// This is equivalent to (a - b).magnitude and involves a square root.
        /// </summary>
        public static float DistanceTo(this Vector3 a, Vector3 b)
            => (a - b).magnitude;

        /// <summary>
        /// Computes the squared Euclidean distance between two points.
        /// This avoids the cost of a square root and is preferred for comparisons.
        /// </summary>
        public static float DistanceSqrTo(this Vector3 a, Vector3 b)
            => (a - b).sqrMagnitude;

        #endregion


        #region Projection Utilities

        /// <summary>
        /// Projects vector v onto direction n, assuming n is already normalized.
        /// No safety checks are performed.
        /// </summary>
        public static Vector3 ProjectOnto(this Vector3 v, Vector3 n)
            => Vector3.Dot(v, n) * n;

        /// <summary>
        /// Projects vector v onto an arbitrary vector n.
        /// If n is too small to normalize safely, the result is Vector3.zero.
        /// This avoids division by extremely small magnitudes.
        /// </summary>
        public static Vector3 ProjectOntoSafe(this Vector3 v, Vector3 n)
        {
            float sqr = n.sqrMagnitude;
            return sqr > EPSILON_SQR_MAG
                ? Vector3.Dot(v, n) / sqr * n
                : Vector3.zero;
        }

        #endregion


        #region Normalization Utilities

        /// <summary>
        /// Returns a normalized version of v. If the vector is too small to
        /// normalize safely, Vector3.zero is returned to avoid unstable division.
        /// </summary>
        public static Vector3 NormalizeSafe(this Vector3 v)
        {
            float sqr = v.sqrMagnitude;
            return sqr > EPSILON_SQR_MAG
                ? v / Mathf.Sqrt(sqr)
                : Vector3.zero;
        }

        /// <summary>
        /// Attempts to normalize v. Returns true if the vector has
        /// sufficient magnitude to normalize; otherwise returns false and outputs
        /// Vector3.zero. This prevents unstable results from tiny vectors.
        /// </summary>
        public static bool TryNormalize(this Vector3 v, out Vector3 normalized)
        {
            float sqr = v.sqrMagnitude;

            if (sqr > EPSILON_SQR_MAG)
            {
                normalized = v / Mathf.Sqrt(sqr);
                return true;
            }

            normalized = Vector3.zero;
            return false;
        }

        #endregion


        #region Angle Utilities

        /// <summary>
        /// Computes the angle in degrees between vectors a and b.
        /// If either vector is degenerate, the angle is defined as 0 degrees.
        /// The dot product is clamped to [-1, 1] to protect against floating point drift.
        /// </summary>
        public static float AngleTo(this Vector3 a, Vector3 b)
        {
            float denom = Mathf.Sqrt(a.sqrMagnitude * b.sqrMagnitude);
            if (denom < EPSILON_SQR_MAG)
                return 0f;

            float dot = Mathf.Clamp(Vector3.Dot(a, b) / denom, -1f, 1f);
            return Mathf.Acos(dot) * Mathf.Rad2Deg;
        }

        #endregion


        #region Movement Utilities

        /// <summary>
        /// Moves "current" toward "target" by at most maxDelta.
        /// If the remaining distance is less than maxDelta, the target is returned directly.
        /// This method avoids overshooting and behaves consistently even when the points coincide.
        /// </summary>
        public static Vector3 MoveTowards(this Vector3 current, Vector3 target, float maxDelta)
        {
            Vector3 delta = target - current;
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
        /// the method returns Vector3.zero to avoid unstable division.
        /// Reflection is computed as: r = v - 2 * projection_of_v_onto_n.
        /// </summary>
        public static Vector3 Reflect(this Vector3 v, Vector3 n)
        {
            float sqr = n.sqrMagnitude;
            if (sqr < EPSILON_SQR_MAG)
                return Vector3.zero;

            float dot = Vector3.Dot(v, n);
            Vector3 projection = (dot / sqr) * n;

            return v - 2f * projection;
        }

        /// <summary>
        /// Attempts to compute the reflection of this vector across the plane defined by normal n.
        /// The normal does not need to be normalized. If n is too small to use safely,
        /// the method returns false and outputs Vector3.zero to avoid unstable division.
        /// Reflection is computed as: r = v - 2 * projection_of_v_onto_n.
        /// </summary>
        public static bool TryReflect(this Vector3 v, Vector3 n, out Vector3 reflected)
        {
            float sqr = n.sqrMagnitude;
            if (sqr < EPSILON_SQR_MAG)
            {
                reflected = Vector3.zero;
                return false;
            }

            float dot = Vector3.Dot(v, n);
            Vector3 projection = (dot / sqr) * n;

            reflected = v - 2f * projection;
            return true;
        }

        /// <summary>
        /// Computes the reflection of this vector across the plane defined by normal n.
        /// The normal is normalized internally. If n is too small to normalize safely,
        /// the method returns Vector3.zero to avoid unstable division.
        /// </summary>
        public static Vector3 ReflectSafe(this Vector3 v, Vector3 n)
        {
            float sqr = n.sqrMagnitude;
            if (sqr < EPSILON_SQR_MAG)
                return Vector3.zero;

            Vector3 nn = n / Mathf.Sqrt(sqr);
            float dot = Vector3.Dot(v, nn);

            return v - 2f * dot * nn;
        }

        /// <summary>
        /// Computes the reflection of this vector across the plane defined by normal n.
        /// Assumes n is already normalized. No safety checks are performed.
        /// </summary>
        public static Vector3 ReflectNormalized(this Vector3 v, Vector3 n)
        {
            float dot = Vector3.Dot(v, n);
            return v - 2f * dot * n;
        }

        #endregion


        #region Other Utilities

        /// <summary>
        /// Computes the centroid (average position) of the provided points.
        /// If the span is empty, Vector3.zero is returned.
        /// This method performs a straightforward summation followed by division and does not
        /// apply numerical stabilization, which is sufficient for typical gameplay coordinate ranges.
        /// </summary>
        public static Vector3 Midpoint(ReadOnlySpan<Vector3> points)
        {
            if (points.Length == 0)
                return Vector3.zero;

            Vector3 sum = Vector3.zero;

            for (int i = 0; i < points.Length; i++)
                sum += points[i];

            return sum / points.Length;
        }

        #endregion
    }
}
