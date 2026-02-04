// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

using System;
using Unity.Mathematics;
using XFG.Algorithm;

namespace XFG.AlgorithmBurst
{
    /// <summary>
    /// Burst-friendly extension methods providing Unity-like random functions
    /// for any IRandom implementation. Uses Unity.Mathematics types for DOTS.
    /// </summary>
    public static partial class RandomUtilityExtensionBurst
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
        public static float2 InsideUnitCircle(this IRandom rng)
        {
            while (true)
            {
                float x = rng.NextFloat(-1f, 1f);
                float y = rng.NextFloat(-1f, 1f);
                float s = x * x + y * y;

                if (s > 0f && s < 1f)
                    return new float2(x, y);
            }
        }

        /// <summary>
        /// Returns a random point on the unit circle.
        /// Equivalent to UnityEngine.Random.insideUnitCircle.normalized.
        /// </summary>
        public static float2 OnUnitCircle(this IRandom rng)
        {
            float2 p = rng.InsideUnitCircle();
            return math.normalize(p);
        }

        /// <summary>
        /// Returns a random point inside the unit sphere.
        /// Equivalent to UnityEngine.Random.insideUnitSphere.
        /// </summary>
        public static float3 InsideUnitSphere(this IRandom rng)
        {
            while (true)
            {
                float x = rng.NextFloat(-1f, 1f);
                float y = rng.NextFloat(-1f, 1f);
                float z = rng.NextFloat(-1f, 1f);
                float s = x * x + y * y + z * z;

                if (s > 0f && s < 1f)
                    return new float3(x, y, z);
            }
        }

        /// <summary>
        /// Returns a random point on the unit sphere.
        /// Equivalent to UnityEngine.Random.onUnitSphere.
        /// </summary>
        public static float3 OnUnitSphere(this IRandom rng)
        {
            return math.normalize(rng.InsideUnitSphere());
        }

        /// <summary>
        /// Returns a uniformly distributed random rotation.
        /// Equivalent to UnityEngine.Random.rotation.
        /// Uses Shoemake's method for unbiased quaternion sampling.
        /// </summary>
        public static quaternion Rotation(this IRandom rng)
        {
            float u1 = rng.NextFloat01();
            float u2 = rng.NextFloat01();
            float u3 = rng.NextFloat01();

            float sqrt1 = math.sqrt(1f - u1);
            float sqrt2 = math.sqrt(u1);

            float theta1 = 2f * math.PI * u2;
            float theta2 = 2f * math.PI * u3;

            float x = sqrt1 * math.sin(theta1);
            float y = sqrt1 * math.cos(theta1);
            float z = sqrt2 * math.sin(theta2);
            float w = sqrt2 * math.cos(theta2);

            return new quaternion(x, y, z, w);
        }

        /// <summary>
        /// Returns a random rotation around a given axis.
        /// Equivalent to quaternion.AxisAngle(axis, randomAngle).
        /// </summary>
        public static quaternion RotationAroundAxis(this IRandom rng, float3 axis)
        {
            if (math.lengthsq(axis) == 0f)
                throw new InvalidOperationException("Axis must be non-zero.");

            float3 n = math.normalize(axis);
            float angle = rng.NextFloat(0f, 2f * math.PI);
            float half = angle * 0.5f;

            float s = math.sin(half);
            float c = math.cos(half);

            return new quaternion(n.x * s, n.y * s, n.z * s, c);
        }

        /// <summary>
        /// Returns a quaternion that rotates from float3(0,0,1) to a random direction.
        /// Equivalent to quaternion.LookRotation(randomDirection).
        /// </summary>
        public static quaternion LookRotation(this IRandom rng)
        {
            float3 dir = rng.OnUnitSphere();
            return quaternion.LookRotationSafe(dir, new float3(0f, 1f, 0f));
        }

        /// <summary>
        /// Returns a quaternion built from random Euler angles in degrees.
        /// Equivalent to quaternion.EulerXYZ(randomAngles).
        /// </summary>
        public static quaternion Euler(
            this IRandom rng,
            float minX, float maxX,
            float minY, float maxY,
            float minZ, float maxZ)
        {
            float ex = rng.NextFloat(minX, maxX);
            float ey = rng.NextFloat(minY, maxY);
            float ez = rng.NextFloat(minZ, maxZ);

            return quaternion.EulerXYZ(math.radians(new float3(ex, ey, ez)));
        }
    }
}
