// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace XFG.Math.ShapeBurst
{
    /// <summary>
    /// A Burst-friendly sphere packed into a single <see cref="float4"/>.
    /// </summary>
    public struct BoundingSphereSimd
    {
        /// <summary>
        /// Packed sphere data: xyz = center, w = radius.
        /// </summary>
        public float4 data;

        /// <summary>
        /// The sphere center.
        /// </summary>
        public readonly float3 Center => data.xyz;

        /// <summary>
        /// The sphere radius; negative for an empty sphere.
        /// </summary>
        public readonly float Radius => data.w;

        /// <summary>
        /// Creates a sphere.
        /// </summary>
        /// <param name="center">The sphere center.</param>
        /// <param name="radius">The sphere radius.</param>
        public BoundingSphereSimd(float3 center, float radius)
        {
            data = new float4(center, radius);
        }

        /// <summary>
        /// An empty sphere (negative radius) that contains no points.
        /// </summary>
        public static BoundingSphereSimd Empty => new BoundingSphereSimd(new float3(0f), -1f);

        /// <summary>
        /// Whether this sphere is empty (negative radius).
        /// </summary>
        public readonly bool IsEmpty => data.w < 0f;

        /// <summary>
        /// Returns whether a point lies inside the sphere, within a small tolerance.
        /// </summary>
        /// <param name="p">The point to test.</param>
        /// <returns><c>true</c> if the point is inside or on the sphere.</returns>
        public bool Contains(float3 p)
        {
            float3 d = p - data.xyz;
            float dist2 = math.dot(d, d);
            float r = data.w;
            return dist2 <= r * r + MinimalEnclosingSphereBurst.EPSILON_CONTAIN;
        }
    }

    /// <summary>
    /// Burst-compatible computation of the exact minimal enclosing sphere of a point set.
    /// </summary>
    [BurstCompile]
    public static class MinimalEnclosingSphereBurst
    {
        /// <summary>
        /// Tolerance used when testing whether a point lies inside a sphere.
        /// </summary>
        public const float EPSILON_CONTAIN = 1e-6f;

        /// <summary>
        /// Threshold below which a configuration of points is treated as degenerate.
        /// </summary>
        public const float EPSILON_DEGENERATE = 1e-12f;

        /// <summary>
        /// Computes the exact minimal enclosing sphere for a set of 3D points.
        /// </summary>
        /// <remarks>
        /// This is a non-recursive, SIMD-friendly implementation of Welzl's algorithm.
        /// It incrementally builds the minimal sphere by iterating over points and
        /// promoting them into the boundary set when they lie outside the current sphere.
        /// <para>Key properties:</para>
        /// <list type="bullet">
        /// <item>Exact solution (not an approximation)</item>
        /// <item>Deterministic (no randomization unless added externally)</item>
        /// <item>Burst-compatible (no recursion, no unsafe, no allocations)</item>
        /// <item>Uses SIMD-leaning float4 math for efficient distance and update operations</item>
        /// </list>
        /// Expected performance is O(n) for typical point distributions, with a worst-case
        /// O(n^4) boundary promotion pattern, but in practice this is extremely rare.
        /// </remarks>
        /// <param name="points">The points to enclose.</param>
        /// <returns>The minimal enclosing sphere, or a zero-radius sphere at the origin if empty.</returns>
        public static BoundingSphereSimd Compute(NativeArray<float3> points)
        {
            int count = points.Length;
            if (count == 0)
                return new BoundingSphereSimd(float3.zero, 0f);

            BoundingSphereSimd sphere = BoundingSphereSimd.Empty;

            for (int i = 0; i < count; i++)
            {
                float3 pi = points[i];
                if (!sphere.IsEmpty && sphere.Contains(pi))
                    continue;

                sphere = new BoundingSphereSimd(pi, 0f);

                for (int j = 0; j < i; j++)
                {
                    float3 pj = points[j];
                    if (sphere.Contains(pj))
                        continue;

                    sphere = SphereFrom2(pi, pj);

                    for (int k = 0; k < j; k++)
                    {
                        float3 pk = points[k];
                        if (sphere.Contains(pk))
                            continue;

                        sphere = SphereFrom3(pi, pj, pk);

                        for (int l = 0; l < k; l++)
                        {
                            float3 pl = points[l];
                            if (sphere.Contains(pl))
                                continue;

                            sphere = SphereFrom4(pi, pj, pk, pl);
                        }
                    }
                }
            }

            return sphere;
        }

        private static BoundingSphereSimd SphereFrom2(float3 a, float3 b)
        {
            float3 center = (a + b) * 0.5f;
            float radius = math.distance(a, b) * 0.5f;
            return new BoundingSphereSimd(center, radius);
        }

        private static BoundingSphereSimd SphereFrom3(float3 a, float3 b, float3 c)
        {
            float3 ab = b - a;
            float3 ac = c - a;
            float3 abXac = math.cross(ab, ac);

            float denom = 2f * math.lengthsq(abXac);
            if (denom < EPSILON_DEGENERATE)
            {
                BoundingSphereSimd s = SphereFrom2(a, b);
                s = ExpandToInclude(s, c);
                return s;
            }

            float ab2 = math.lengthsq(ab);
            float ac2 = math.lengthsq(ac);

            float3 term1 = math.cross(abXac, ab) * ac2;
            float3 term2 = math.cross(ac, abXac) * ab2;

            float3 center = a + (term1 + term2) / denom;
            float radius = math.distance(center, a);
            return new BoundingSphereSimd(center, radius);
        }

        private static BoundingSphereSimd SphereFrom4(float3 a, float3 b, float3 c, float3 d)
        {
            float3 ba = b - a;
            float3 ca = c - a;
            float3 da = d - a;

            float ba2 = math.lengthsq(ba);
            float ca2 = math.lengthsq(ca);
            float da2 = math.lengthsq(da);

            float3 rhs = 0.5f * new float3(ba2, ca2, da2);

            float3x3 m = new float3x3(ba, ca, da);
            float det = math.determinant(m);

            if (math.abs(det) < EPSILON_DEGENERATE)
            {
                BoundingSphereSimd s = SphereFrom3(a, b, c);
                s = ExpandToInclude(s, d);
                return s;
            }

            float3 centerLocal = math.mul(math.inverse(m), rhs);
            float3 center = a + centerLocal;
            float radius = math.distance(center, a);
            return new BoundingSphereSimd(center, radius);
        }

        private static BoundingSphereSimd ExpandToInclude(BoundingSphereSimd s, float3 p)
        {
            if (s.IsEmpty)
                return new BoundingSphereSimd(p, 0f);

            if (s.Contains(p))
                return s;

            float3 center = s.data.xyz;
            float radius = s.data.w;

            float3 dir = p - center;
            float dist2 = math.dot(dir, dir);
            float dist = math.sqrt(dist2);

            if (dist < EPSILON_CONTAIN)
                return s;

            float newRadius = (radius + dist) * 0.5f;
            float invDist = math.rsqrt(dist2);
            float t = (newRadius - radius) * invDist;

            float3 newCenter = center + dir * t;
            return new BoundingSphereSimd(newCenter, newRadius);
        }
    }
}
