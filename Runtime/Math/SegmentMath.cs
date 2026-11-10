// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

using UnityEngine;

namespace XFG.Math
{
    /// <summary>
    /// Math utilities for line segments and infinite/half-infinite lines.
    /// </summary>
    /// <remarks>
    /// Provides:
    /// <list type="bullet">
    /// <item>Closest point on infinite line</item>
    /// <item>Closest point on ray (Ray + origin/direction overloads)</item>
    /// <item>Closest point on segment (core + aliases)</item>
    /// <item>Closest points between segment and infinite line</item>
    /// <item>Closest points between segment and ray</item>
    /// <item>Segment vs segment closest points</item>
    /// <item>Segment vs segment squared distance</item>
    /// <item>Closest point on segment to AABB</item>
    /// <item>Distance to point</item>
    /// <item>Segment length and direction</item>
    /// </list>
    /// All repeated property getters are cached.
    /// </remarks>
    public static class SegmentMath
    {
        // ---------------------------------------------------------------------
        // Closest Point on Infinite Line
        // ---------------------------------------------------------------------
        /// <summary>
        /// Returns the point on the infinite line through a and b closest to the given point.
        /// </summary>
        /// <param name="a">A point on the line.</param>
        /// <param name="b">A second point on the line.</param>
        /// <param name="point">The query point.</param>
        /// <returns>The closest point on the line, or <paramref name="a"/> if a and b coincide.</returns>
        public static Vector3 ClosestPointOnLineToPoint(Vector3 a, Vector3 b, Vector3 point)
        {
            Vector3 ab = b - a;
            float lenSq = ab.sqrMagnitude;

            if (lenSq < 1e-6f)
                return a;

            float t = Vector3.Dot(point - a, ab) / lenSq;
            return a + ab * t;
        }

        // ---------------------------------------------------------------------
        // Closest Point on Ray
        // ---------------------------------------------------------------------
        /// <summary>
        /// Returns the point on the ray closest to the given point.
        /// </summary>
        /// <param name="ray">The ray.</param>
        /// <param name="point">The query point.</param>
        /// <returns>The closest point on the ray.</returns>
        public static Vector3 ClosestPointOnRayToPoint(Ray ray, Vector3 point)
        {
            Vector3 o = ray.origin;
            Vector3 d = ray.direction;

            float t = Vector3.Dot(point - o, d);
            if (t < 0f) t = 0f;

            return o + d * t;
        }

        /// <summary>
        /// Returns the point on the ray closest to the given point.
        /// </summary>
        /// <param name="origin">The ray origin.</param>
        /// <param name="direction">The ray direction.</param>
        /// <param name="point">The query point.</param>
        /// <returns>The closest point on the ray.</returns>
        public static Vector3 ClosestPointOnRayToPoint(Vector3 origin, Vector3 direction, Vector3 point)
        {
            return ClosestPointOnRayToPoint(new Ray(origin, direction), point);
        }

        // ---------------------------------------------------------------------
        // Core: Closest Point on Segment
        // ---------------------------------------------------------------------
        /// <summary>
        /// Returns the point on segment ab closest to the given point.
        /// </summary>
        /// <param name="a">The segment start point.</param>
        /// <param name="b">The segment end point.</param>
        /// <param name="point">The query point.</param>
        /// <returns>The closest point on the segment.</returns>
        public static Vector3 ClosestPoint(Vector3 a, Vector3 b, Vector3 point)
        {
            Vector3 ab = b - a;
            float lenSq = ab.sqrMagnitude;

            if (lenSq < 1e-6f)
                return a;

            float t = Vector3.Dot(point - a, ab) / lenSq;
            t = Mathf.Clamp01(t);

            return a + ab * t;
        }

        /// <summary>
        /// Alias of <see cref="ClosestPoint(Vector3, Vector3, Vector3)"/>.
        /// </summary>
        /// <param name="a">The segment start point.</param>
        /// <param name="b">The segment end point.</param>
        /// <param name="point">The query point.</param>
        /// <returns>The closest point on the segment.</returns>
        public static Vector3 ClosestPointOnSegmentToPoint(Vector3 a, Vector3 b, Vector3 point)
        {
            return ClosestPoint(a, b, point);
        }

        /// <summary>
        /// Alias of <see cref="ClosestPoint(Vector3, Vector3, Vector3)"/>.
        /// </summary>
        /// <param name="a">The segment start point.</param>
        /// <param name="b">The segment end point.</param>
        /// <param name="point">The query point.</param>
        /// <returns>The closest point on the segment.</returns>
        public static Vector3 ClosestPointOnSegment(Vector3 a, Vector3 b, Vector3 point)
        {
            return ClosestPoint(a, b, point);
        }

        // ---------------------------------------------------------------------
        // Closest Point on Segment to AABB
        // ---------------------------------------------------------------------
        /// <summary>
        /// Returns the closest point on a line segment to an axis-aligned bounding box.
        /// Uses the slab method to compute the earliest valid intersection parameter.
        /// </summary>
        /// <param name="p0">The segment start point.</param>
        /// <param name="p1">The segment end point.</param>
        /// <param name="aabbMin">The minimum corner of the box.</param>
        /// <param name="aabbMax">The maximum corner of the box.</param>
        /// <returns>A point on the segment.</returns>
        public static Vector3 ClosestPointOnSegmentToAABB(
            Vector3 p0,
            Vector3 p1,
            Vector3 aabbMin,
            Vector3 aabbMax)
        {
            Vector3 dir = p1 - p0;
            float lenSq = dir.sqrMagnitude;

            // Degenerate segment: treat as point
            if (lenSq < 1e-12f)
                return ClosestPointOnAABB(p0, aabbMin, aabbMax);

            float tMin = 0f;
            float tMax = 1f;

            // Slab intersection on each axis
            for (int axis = 0; axis < 3; axis++)
            {
                float origin = p0[axis];
                float direction = dir[axis];

                // Parallel to slab
                if (Mathf.Abs(direction) < 1e-12f)
                {
                    if (origin < aabbMin[axis] || origin > aabbMax[axis])
                        return ClosestPointOnAABB(p0, aabbMin, aabbMax);
                }
                else
                {
                    float inv = 1f / direction;
                    float t1 = (aabbMin[axis] - origin) * inv;
                    float t2 = (aabbMax[axis] - origin) * inv;

                    if (t1 > t2)
                    {
                        float tmp = t1;
                        t1 = t2;
                        t2 = tmp;
                    }

                    if (t1 > tMin) tMin = t1;
                    if (t2 < tMax) tMax = t2;

                    if (tMin > tMax)
                        break;
                }
            }

            float t = Mathf.Clamp01(tMin);
            return p0 + dir * t;
        }

        /// <summary>
        /// Returns the closest point on an AABB to a given point.
        /// </summary>
        /// <param name="point">The query point.</param>
        /// <param name="min">The minimum corner of the box.</param>
        /// <param name="max">The maximum corner of the box.</param>
        /// <returns>The point clamped to the box.</returns>
        public static Vector3 ClosestPointOnAABB(Vector3 point, Vector3 min, Vector3 max)
        {
            float x = Mathf.Clamp(point.x, min.x, max.x);
            float y = Mathf.Clamp(point.y, min.y, max.y);
            float z = Mathf.Clamp(point.z, min.z, max.z);
            return new Vector3(x, y, z);
        }

        // ---------------------------------------------------------------------
        // Segment vs Line Closest Points
        // ---------------------------------------------------------------------
        /// <summary>
        /// Computes the closest pair of points between a segment and an infinite line.
        /// </summary>
        /// <param name="segA">The segment start point.</param>
        /// <param name="segB">The segment end point.</param>
        /// <param name="lineA">A point on the line.</param>
        /// <param name="lineB">A second point on the line.</param>
        /// <param name="segPoint">The closest point on the segment.</param>
        /// <param name="linePoint">The closest point on the line.</param>
        public static void ClosestPointOnSegmentToLine(
            Vector3 segA, Vector3 segB,
            Vector3 lineA, Vector3 lineB,
            out Vector3 segPoint, out Vector3 linePoint)
        {
            Vector3 d1 = segB - segA;
            Vector3 d2 = lineB - lineA;
            Vector3 r = segA - lineA;

            float a = Vector3.Dot(d1, d1);
            float c = Vector3.Dot(d2, d2);
            float b = Vector3.Dot(d1, d2);
            float d = Vector3.Dot(d1, r);
            float e = Vector3.Dot(d2, r);

            float denom = a * c - b * b;

            float s, t;

            if (denom < 1e-6f)
            {
                s = 0f;
                t = e / c;
            }
            else
            {
                s = (b * e - c * d) / denom;
                s = Mathf.Clamp01(s);
                t = (a * e - b * d) / denom;
            }

            segPoint = segA + d1 * s;
            linePoint = lineA + d2 * t;
        }

        /// <summary>
        /// Returns the point on a segment closest to an infinite line.
        /// </summary>
        /// <param name="segA">The segment start point.</param>
        /// <param name="segB">The segment end point.</param>
        /// <param name="lineA">A point on the line.</param>
        /// <param name="lineB">A second point on the line.</param>
        /// <returns>The closest point on the segment.</returns>
        public static Vector3 ClosestPointOnSegmentToLine(
            Vector3 segA, Vector3 segB,
            Vector3 lineA, Vector3 lineB)
        {
            ClosestPointOnSegmentToLine(segA, segB, lineA, lineB, out Vector3 segPoint, out _);
            return segPoint;
        }

        // ---------------------------------------------------------------------
        // Segment vs Ray Closest Points
        // ---------------------------------------------------------------------
        /// <summary>
        /// Computes the closest pair of points between a segment and a ray.
        /// </summary>
        /// <param name="segA">The segment start point.</param>
        /// <param name="segB">The segment end point.</param>
        /// <param name="ray">The ray.</param>
        /// <param name="segPoint">The closest point on the segment.</param>
        /// <param name="rayPoint">The closest point on the ray.</param>
        public static void ClosestPointOnSegmentToRay(
            Vector3 segA, Vector3 segB,
            Ray ray,
            out Vector3 segPoint, out Vector3 rayPoint)
        {
            Vector3 d1 = segB - segA;
            Vector3 d2 = ray.direction;
            Vector3 r = segA - ray.origin;

            float a = Vector3.Dot(d1, d1);
            float c = Vector3.Dot(d2, d2);
            float b = Vector3.Dot(d1, d2);
            float d = Vector3.Dot(d1, r);
            float e = Vector3.Dot(d2, r);

            float denom = a * c - b * b;

            float s, t;

            if (denom < 1e-6f)
            {
                s = 0f;
                t = e / c;
                if (t < 0f) t = 0f;
            }
            else
            {
                s = (b * e - c * d) / denom;
                s = Mathf.Clamp01(s);
                t = (a * e - b * d) / denom;

                if (t < 0f)
                {
                    t = 0f;

                    float a2 = Vector3.Dot(d1, d1);
                    float c2 = Vector3.Dot(d1, ray.origin - segA);
                    s = a2 < 1e-6f ? 0f : Mathf.Clamp01(-c2 / a2);
                }
            }

            segPoint = segA + d1 * s;
            rayPoint = ray.origin + d2 * t;
        }

        /// <summary>
        /// Returns the point on a segment closest to a ray.
        /// </summary>
        /// <param name="segA">The segment start point.</param>
        /// <param name="segB">The segment end point.</param>
        /// <param name="ray">The ray.</param>
        /// <returns>The closest point on the segment.</returns>
        public static Vector3 ClosestPointOnSegmentToRay(
            Vector3 segA, Vector3 segB,
            Ray ray)
        {
            ClosestPointOnSegmentToRay(segA, segB, ray, out Vector3 segPoint, out _);
            return segPoint;
        }

        // ---------------------------------------------------------------------
        // Segment vs Segment Closest Points
        // ---------------------------------------------------------------------
        /// <summary>
        /// Computes the closest pair of points between two segments.
        /// </summary>
        /// <param name="p1">The start point of the first segment.</param>
        /// <param name="q1">The end point of the first segment.</param>
        /// <param name="p2">The start point of the second segment.</param>
        /// <param name="q2">The end point of the second segment.</param>
        /// <param name="c1">The closest point on the first segment.</param>
        /// <param name="c2">The closest point on the second segment.</param>
        public static void ClosestPoints(
            Vector3 p1, Vector3 q1,
            Vector3 p2, Vector3 q2,
            out Vector3 c1, out Vector3 c2)
        {
            Vector3 d1 = q1 - p1;
            Vector3 d2 = q2 - p2;
            Vector3 r = p1 - p2;

            float a = Vector3.Dot(d1, d1);
            float e = Vector3.Dot(d2, d2);
            float f = Vector3.Dot(d2, r);

            float s, t;

            if (a < 1e-6f && e < 1e-6f)
            {
                c1 = p1;
                c2 = p2;
                return;
            }

            if (a < 1e-6f)
            {
                s = 0f;
                t = Mathf.Clamp01(f / e);
            }
            else
            {
                float c = Vector3.Dot(d1, r);

                if (e < 1e-6f)
                {
                    t = 0f;
                    s = Mathf.Clamp01(-c / a);
                }
                else
                {
                    float b = Vector3.Dot(d1, d2);
                    float denom = a * e - b * b;

                    if (denom < 1e-6f)
                    {
                        s = 0f;
                        t = Mathf.Clamp01(f / e);
                    }
                    else
                    {
                        s = Mathf.Clamp01((b * f - c * e) / denom);
                        t = (b * s + f) / e;

                        if (t < 0f)
                        {
                            t = 0f;
                            s = Mathf.Clamp01(-c / a);
                        }
                        else if (t > 1f)
                        {
                            t = 1f;
                            s = Mathf.Clamp01((b - c) / a);
                        }
                    }
                }
            }

            c1 = p1 + d1 * s;
            c2 = p2 + d2 * t;
        }

        // ---------------------------------------------------------------------
        // Segment vs Segment Squared Distance
        // ---------------------------------------------------------------------
        /// <summary>
        /// Returns the squared distance between two segments.
        /// </summary>
        /// <param name="p1">The start point of the first segment.</param>
        /// <param name="q1">The end point of the first segment.</param>
        /// <param name="p2">The start point of the second segment.</param>
        /// <param name="q2">The end point of the second segment.</param>
        /// <returns>The squared distance between the closest points of the segments.</returns>
        public static float SegmentSegmentDistanceSq(
            Vector3 p1, Vector3 q1,
            Vector3 p2, Vector3 q2)
        {
            ClosestPoints(p1, q1, p2, q2, out Vector3 c1, out Vector3 c2);
            return (c1 - c2).sqrMagnitude;
        }

        // ---------------------------------------------------------------------
        // Length and Direction
        // ---------------------------------------------------------------------
        /// <summary>
        /// Returns the length of segment ab.
        /// </summary>
        /// <param name="a">The segment start point.</param>
        /// <param name="b">The segment end point.</param>
        /// <returns>The segment length.</returns>
        public static float Length(Vector3 a, Vector3 b)
        {
            return (b - a).magnitude;
        }

        /// <summary>
        /// Returns the normalized direction from a to b.
        /// </summary>
        /// <param name="a">The segment start point.</param>
        /// <param name="b">The segment end point.</param>
        /// <returns>The unit direction, or <see cref="Vector3.zero"/> for a degenerate segment.</returns>
        public static Vector3 Direction(Vector3 a, Vector3 b)
        {
            Vector3 d = b - a;
            float m = d.magnitude;
            return m > 1e-6f ? d / m : Vector3.zero;
        }
    }
}
