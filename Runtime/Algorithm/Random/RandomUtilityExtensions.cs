// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

using System;
using UnityEngine;

namespace XFG.Algorithm
{
    /// <summary>
    /// Extension methods providing Unity-like random functions for any IRandom
    /// implementation. Allows deterministic behavior and pluggable RNG algorithms.
    /// </summary>
    public static partial class RandomUtilityExtensions
    {
        /// <summary>
        /// Returns a float in the range [0, 1).
        /// Equivalent to UnityEngine.Random.value.
        /// </summary>
        public static float Value(this IRandom rng)
        {
            return rng.NextFloat01();
        }

        /// <summary>
        /// Returns a float in the range [min, max).
        /// Equivalent to UnityEngine.Random.Range(float, float).
        /// </summary>
        public static float Range(this IRandom rng, float min, float max)
        {
            return rng.NextFloat(min, max);
        }

        /// <summary>
        /// Returns an integer in the range [minInclusive, maxExclusive).
        /// Equivalent to UnityEngine.Random.Range(int, int).
        /// </summary>
        public static int Range(this IRandom rng, int minInclusive, int maxExclusive)
        {
            return rng.NextInt(minInclusive, maxExclusive);
        }

        /// <summary>
        /// Returns true with the given probability in the range [0, 1].
        /// Equivalent to UnityEngine.Random.value < probability.
        /// </summary>
        public static bool Chance(this IRandom rng, float probability)
        {
            if (probability <= 0f)
                return false;

            if (probability >= 1f)
                return true;

            return rng.NextFloat01() < probability;
        }

        /// <summary>
        /// Returns -1 or +1 with equal probability.
        /// Equivalent to UnityEngine.Random.Range(0, 2) * 2 - 1.
        /// </summary>
        public static int Sign(this IRandom rng)
        {
            return rng.NextInt(2) == 0 ? -1 : 1;
        }

        /// <summary>
        /// Returns a random element from the given span.
        /// Equivalent to selecting a random index.
        /// </summary>
        public static T Element<T>(this IRandom rng, ReadOnlySpan<T> items)
        {
            if (items.Length == 0)
                throw new InvalidOperationException("Cannot select from an empty span.");

            int index = rng.NextInt(items.Length);
            return items[index];
        }

        /// <summary>
        /// Returns a random point inside the unit circle.
        /// Equivalent to UnityEngine.Random.insideUnitCircle.
        /// </summary>
        public static Vector2 InsideUnitCircle(this IRandom rng)
        {
            while (true)
            {
                float x = rng.NextFloat(-1f, 1f);
                float y = rng.NextFloat(-1f, 1f);
                float s = x * x + y * y;

                if (s > 0f && s < 1f)
                    return new Vector2(x, y);
            }
        }

        /// <summary>
        /// Returns a random point on the unit circle.
        /// Equivalent to UnityEngine.Random.insideUnitCircle.normalized.
        /// </summary>
        public static Vector2 OnUnitCircle(this IRandom rng)
        {
            Vector2 p = rng.InsideUnitCircle();
            return p.normalized;
        }

        /// <summary>
        /// Returns a random point inside the unit sphere.
        /// Equivalent to UnityEngine.Random.insideUnitSphere.
        /// </summary>
        public static Vector3 InsideUnitSphere(this IRandom rng)
        {
            while (true)
            {
                float x = rng.NextFloat(-1f, 1f);
                float y = rng.NextFloat(-1f, 1f);
                float z = rng.NextFloat(-1f, 1f);
                float s = x * x + y * y + z * z;

                if (s > 0f && s < 1f)
                    return new Vector3(x, y, z);
            }
        }

        /// <summary>
        /// Returns a random point on the unit sphere.
        /// Equivalent to UnityEngine.Random.onUnitSphere.
        /// </summary>
        public static Vector3 OnUnitSphere(this IRandom rng)
        {
            return rng.InsideUnitSphere().normalized;
        }

        /// <summary>
        /// Returns a uniformly distributed random rotation.
        /// Equivalent to UnityEngine.Random.rotation.
        /// Uses Shoemake's method for unbiased quaternion sampling.
        /// </summary>
        public static Quaternion Rotation(this IRandom rng)
        {
            float u1 = rng.NextFloat01();
            float u2 = rng.NextFloat01();
            float u3 = rng.NextFloat01();

            float sqrt1 = Mathf.Sqrt(1f - u1);
            float sqrt2 = Mathf.Sqrt(u1);

            float theta1 = 2f * Mathf.PI * u2;
            float theta2 = 2f * Mathf.PI * u3;

            float x = sqrt1 * Mathf.Sin(theta1);
            float y = sqrt1 * Mathf.Cos(theta1);
            float z = sqrt2 * Mathf.Sin(theta2);
            float w = sqrt2 * Mathf.Cos(theta2);

            return new Quaternion(x, y, z, w);
        }

        /// <summary>
        /// Returns a random rotation around a given axis.
        /// Equivalent to Quaternion.AngleAxis(randomAngle, axis).
        /// </summary>
        public static Quaternion RotationAroundAxis(this IRandom rng, Vector3 axis)
        {
            if (axis == Vector3.zero)
                throw new InvalidOperationException("Axis must be non-zero.");

            Vector3 n = axis.normalized;
            float angle = rng.NextFloat(0f, 360f);
            float rad = angle * Mathf.Deg2Rad;
            float half = rad * 0.5f;

            float s = Mathf.Sin(half);
            float c = Mathf.Cos(half);

            return new Quaternion(n.x * s, n.y * s, n.z * s, c);
        }

        /// <summary>
        /// Returns a quaternion that rotates from Vector3.forward to a random direction.
        /// Equivalent to Quaternion.LookRotation(randomDirection).
        /// </summary>
        public static Quaternion LookRotation(this IRandom rng)
        {
            Vector3 dir = rng.OnUnitSphere();
            return Quaternion.LookRotation(dir);
        }

        /// <summary>
        /// Returns a quaternion built from random Euler angles in degrees.
        /// Equivalent to Quaternion.Euler(x, y, z) with random components.
        /// </summary>
        public static Quaternion Euler(
            this IRandom rng,
            float minX, float maxX,
            float minY, float maxY,
            float minZ, float maxZ)
        {
            float ex = rng.NextFloat(minX, maxX);
            float ey = rng.NextFloat(minY, maxY);
            float ez = rng.NextFloat(minZ, maxZ);

            return Quaternion.Euler(ex, ey, ez);
        }
    }
}
