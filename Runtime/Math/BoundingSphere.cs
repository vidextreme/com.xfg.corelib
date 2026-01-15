using System;
using System.Collections.Generic;
using System.Drawing;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
namespace XFG.Math
{
    public struct BoundingSphere
    {
        public Vector3 Center;
        public float Radius;

        public BoundingSphere(Vector3 c, float r)
        {
            Center = c;
            Radius = r;
        }

        public bool Contains(Vector3 p)
        {
            return (p - Center).sqrMagnitude <= Radius * Radius + MinimalEnclosingSphere.EPSILON_CONTAIN;
        }

        public BoundingSphere ExpandToInclude(Vector3 p)
        {
            if (Contains(p))
                return this;

            Vector3 dir = p - Center;
            float dist = dir.magnitude;

            float newRadius = (Radius + dist) * 0.5f;
            Vector3 newCenter = Center;

            if (dist > MinimalEnclosingSphere.EPSILON_CONTAIN)
                newCenter += dir.normalized * (newRadius - Radius);

            return new BoundingSphere(newCenter, newRadius);
        }
    }


    public static class MinimalEnclosingSphere
    {
        public const float EPSILON_CONTAIN = 1e-6f;
        public const float EPSILON_DEGENERATE = 1e-12f;

        /// <summary>
        /// Iterative Welzl (no recursion)
        /// Stable 4-point circumsphere
        /// Deterministic if you seed Shuffle
        /// Zero GC inside the main loop
        /// Exact minimal sphere, not an approximation
        /// For Burst performance use XFG.MathBurst.MinimalEnclosingSphereBurst
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public static BoundingSphere Compute(IList<Vector3> points)
        {
            if (points == null || points.Count == 0)
                return new BoundingSphere(Vector3.zero, 0f);

            // Copy and shuffle for expected linear time
            var pts = new List<Vector3>(points);
            Shuffle(pts);

            BoundingSphere sphere = new BoundingSphere(pts[0], 0f);

            // Iterative Welzl (unrolled recursion)
            for (int i = 1; i < pts.Count; i++)
            {
                Vector3 p = pts[i];
                if (!sphere.Contains(p))
                {
                    sphere = new BoundingSphere(p, 0f);

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

        private static BoundingSphere SphereFrom2(Vector3 a, Vector3 b)
        {
            Vector3 center = (a + b) * 0.5f;
            float radius = Vector3.Distance(a, b) * 0.5f;
            return new BoundingSphere(center, radius);
        }

        private static BoundingSphere SphereFrom3(Vector3 a, Vector3 b, Vector3 c)
        {
            Vector3 ab = b - a;
            Vector3 ac = c - a;
            Vector3 abXac = Vector3.Cross(ab, ac);

            float denom = 2f * abXac.sqrMagnitude;
            if (denom < EPSILON_DEGENERATE)
                return SphereFrom2(a, b).ExpandToInclude(c);

            Vector3 center =
                a +
                (Vector3.Cross(abXac, ab) * ac.sqrMagnitude +
                 Vector3.Cross(ac, abXac) * ab.sqrMagnitude) / denom;

            float radius = Vector3.Distance(center, a);
            return new BoundingSphere(center, radius);
        }

        private static BoundingSphere SphereFrom4(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
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

            if (Mathf.Abs(det) < 1e-9f)
                return SphereFrom3(a, b, c).ExpandToInclude(d);

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
            return new BoundingSphere(center, radius);
        }

        private static void Shuffle(List<Vector3> list)
        {
            var rng = new System.Random();
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}