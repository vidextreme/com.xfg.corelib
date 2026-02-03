// Copyright (c) 2025 John David Uy
// Licensed under the MIT License. See LICENSE for details.

using System;
using UnityEngine;

namespace XFG.Math.Shape
{
    public static class MinimalEnclosingSphere
    {
        public const float EPSILON_CONTAIN = 1e-6f;
        public const float EPSILON_DEGENERATE = 1e-12f;

        private static System.Random _random = new System.Random(9001); // default deterministic seed

        public static void SetRandomSeed(int seed)
        {
            _random = new System.Random(seed);
        }

        /// <summary>
        /// Iterative Welzl (no recursion)
        /// Stable 4-point circumsphere
        /// Deterministic if you seed Shuffle
        /// Zero GC inside the main loop
        /// Exact minimal sphere, not an approximation
        /// For Burst performance use XFG.MathBurst.MinimalEnclosingSphereBurst
        /// </summary>
        public static Sphere Compute(ReadOnlySpan<Vector3> points)
        {
            if (points == null || points.Length == 0)
                return new Sphere(Vector3.zero, 0f);

            // Copy and shuffle for expected linear time
            var pts = points.ToArray();
            Shuffle(pts);

            Sphere sphere = new Sphere(pts[0], 0f);

            // Iterative Welzl (unrolled recursion)
            for (int i = 1; i < pts.Length; i++)
            {
                Vector3 p = pts[i];
                if (!sphere.Contains(p))
                {
                    sphere = new Sphere(p, 0f);

                    for (int j = 0; j < i; j++)
                    {
                        Vector3 q = pts[j];
                        if (!sphere.Contains(q))
                        {
                            sphere = SphereFrom2(p, q);

                            for (int k = 0; k < j; k++)
                            {
                                Vector3 r = pts[k];
                                if (!sphere.Contains(r))
                                {
                                    sphere = SphereFrom3(p, q, r);

                                    for (int m = 0; m < k; m++)
                                    {
                                        Vector3 s = pts[m];
                                        if (!sphere.Contains(s))
                                        {
                                            sphere = SphereFrom4(p, q, r, s);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return sphere;
        }

        private static Sphere SphereFrom2(Vector3 a, Vector3 b)
        {
            Vector3 center = (a + b) * 0.5f;
            float radius = Vector3.Distance(a, b) * 0.5f;
            return new Sphere(center, radius);
        }

        private static Sphere SphereFrom3(Vector3 a, Vector3 b, Vector3 c)
        {
            Vector3 ab = b - a;
            Vector3 ac = c - a;
            Vector3 abXac = Vector3.Cross(ab, ac);

            float denom = 2f * abXac.sqrMagnitude;

            Vector3 center =
                a +
                (Vector3.Cross(abXac, ab) * ac.sqrMagnitude +
                 Vector3.Cross(ac, abXac) * ab.sqrMagnitude) / denom;

            float radius = Vector3.Distance(center, a);
            return new Sphere(center, radius);
        }

        private static Sphere SphereFrom4(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            Vector3 ab = b - a;
            Vector3 ac = c - a;
            Vector3 ad = d - a;

            Vector3 rhs = new Vector3(
                b.sqrMagnitude - a.sqrMagnitude,
                c.sqrMagnitude - a.sqrMagnitude,
                d.sqrMagnitude - a.sqrMagnitude
            ) * 0.5f;

            float A00 = ab.x, A01 = ab.y, A02 = ab.z;
            float A10 = ac.x, A11 = ac.y, A12 = ac.z;
            float A20 = ad.x, A21 = ad.y, A22 = ad.z;

            float det =
                A00 * (A11 * A22 - A12 * A21) -
                A01 * (A10 * A22 - A12 * A20) +
                A02 * (A10 * A21 - A11 * A20);

            float invDet = 1f / det;

            Vector3 center = new Vector3(
                (rhs.x * (A11 * A22 - A12 * A21) -
                 A01 * (rhs.y * A22 - A12 * rhs.z) +
                 A02 * (rhs.y * A21 - A11 * rhs.z)) * invDet,

                (A00 * (rhs.y * A22 - A12 * rhs.z) -
                 rhs.x * (A10 * A22 - A12 * A20) +
                 A02 * (A10 * rhs.z - rhs.y * A20)) * invDet,

                (A00 * (A11 * rhs.z - rhs.y * A21) -
                 A01 * (A10 * rhs.z - rhs.y * A20) +
                 rhs.x * (A10 * A21 - A11 * A20)) * invDet
            );

            float radius = Vector3.Distance(center, a);
            return new Sphere(center, radius);
        }

        private static void Shuffle(Span<Vector3> list)
        {
            for (int i = list.Length - 1; i > 0; i--)
            {
                int j = _random.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
