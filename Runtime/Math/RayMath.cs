// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

using UnityEngine;

namespace XFG.Math
{
    /// <summary>
    /// Math utilities for rays.
    /// </summary>
    /// <remarks>
    /// Provides:
    /// <list type="bullet">
    /// <item>Closest point on ray</item>
    /// <item>Distance to point</item>
    /// <item>Ray vs segment closest points</item>
    /// <item>Ray vs ray closest points</item>
    /// </list>
    /// All repeated property getters are cached.
    /// </remarks>
    public static class RayMath
    {
        // ---------------------------------------------------------------------
        // Closest Point on Ray
        // ---------------------------------------------------------------------
        /// <summary>
        /// Returns the point on the ray closest to the given point.
        /// </summary>
        /// <param name="ray">The ray.</param>
        /// <param name="point">The query point.</param>
        /// <returns>The closest point on the ray (the origin if the point lies behind it).</returns>
        public static Vector3 ClosestPoint(Ray ray, Vector3 point)
        {
            Vector3 o = ray.origin;
            Vector3 d = ray.direction;

            float t = Vector3.Dot(point - o, d);
            if (t < 0f) t = 0f;

            return o + d * t;
        }

        // ---------------------------------------------------------------------
        // Distance to Point
        // ---------------------------------------------------------------------
        /// <summary>
        /// Returns the distance from the ray to a point.
        /// </summary>
        /// <param name="ray">The ray.</param>
        /// <param name="point">The query point.</param>
        /// <returns>The distance between the point and its closest point on the ray.</returns>
        public static float DistanceToPoint(Ray ray, Vector3 point)
        {
            Vector3 cp = ClosestPoint(ray, point);
            return (cp - point).magnitude;
        }

        // ---------------------------------------------------------------------
        // Ray vs Segment Closest Points
        // ---------------------------------------------------------------------
        /// <summary>
        /// Computes the closest pair of points between a ray and a segment.
        /// </summary>
        /// <param name="ray">The ray.</param>
        /// <param name="s0">The segment start point.</param>
        /// <param name="s1">The segment end point.</param>
        /// <param name="rayPoint">The closest point on the ray.</param>
        /// <param name="segPoint">The closest point on the segment.</param>
        public static void ClosestPoints(Ray ray, Vector3 s0, Vector3 s1, out Vector3 rayPoint, out Vector3 segPoint)
        {
            Vector3 o = ray.origin;
            Vector3 d = ray.direction;

            Vector3 u = s1 - s0;
            Vector3 w0 = o - s0;

            float a = Vector3.Dot(d, d);
            float b = Vector3.Dot(d, u);
            float c = Vector3.Dot(u, u);
            float d0 = Vector3.Dot(d, w0);
            float e0 = Vector3.Dot(u, w0);

            float denom = a * c - b * b;

            float t, s;

            if (denom < 1e-6f)
            {
                t = 0f;
                s = Mathf.Clamp(e0 / c, 0f, 1f);
            }
            else
            {
                t = (b * e0 - c * d0) / denom;
                if (t < 0f) t = 0f;

                s = (a * e0 - b * d0) / denom;
                s = Mathf.Clamp(s, 0f, 1f);
            }

            rayPoint = o + d * t;
            segPoint = s0 + u * s;
        }

        // ---------------------------------------------------------------------
        // Ray vs Ray Closest Points
        // ---------------------------------------------------------------------
        /// <summary>
        /// Computes the closest pair of points between two rays.
        /// </summary>
        /// <param name="a">The first ray.</param>
        /// <param name="b">The second ray.</param>
        /// <param name="pA">The closest point on ray <paramref name="a"/>.</param>
        /// <param name="pB">The closest point on ray <paramref name="b"/>.</param>
        public static void ClosestPoints(Ray a, Ray b, out Vector3 pA, out Vector3 pB)
        {
            Vector3 o1 = a.origin;
            Vector3 d1 = a.direction;

            Vector3 o2 = b.origin;
            Vector3 d2 = b.direction;

            Vector3 r = o1 - o2;

            float a1 = Vector3.Dot(d1, d1);
            float b1 = Vector3.Dot(d1, d2);
            float c1 = Vector3.Dot(d2, d2);
            float d1r = Vector3.Dot(d1, r);
            float d2r = Vector3.Dot(d2, r);

            float denom = a1 * c1 - b1 * b1;

            float t1, t2;

            if (denom < 1e-6f)
            {
                t1 = 0f;
                t2 = d2r / c1;
            }
            else
            {
                t1 = (b1 * d2r - c1 * d1r) / denom;
                t2 = (a1 * d2r - b1 * d1r) / denom;
            }

            if (t1 < 0f) t1 = 0f;
            if (t2 < 0f) t2 = 0f;

            pA = o1 + d1 * t1;
            pB = o2 + d2 * t2;
        }
    }
}
