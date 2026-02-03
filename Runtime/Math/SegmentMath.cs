// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.
// ------------------------------------------------------------------------------
// SegmentMath
// ------------------------------------------------------------------------------
// Math utilities for line segments and infinite/half-infinite lines.
//
// Provides:
// - Closest point on infinite line
// - Closest point on ray (Ray + origin/direction overloads)
// - Closest point on segment (core + aliases)
// - Closest points between segment and infinite line
// - Closest points between segment and ray
// - Segment vs segment closest points
// - Segment vs segment squared distance
// - Distance to point
// - Segment length and direction
//
// All repeated property getters are cached.
// ------------------------------------------------------------------------------

using UnityEngine;

namespace XFG.Math
{
    public static class SegmentMath
    {
        // ==========================================================================
        // CLOSEST POINT ON INFINITE LINE
        // ==========================================================================
        public static Vector3 ClosestPointOnLineToPoint(Vector3 a, Vector3 b, Vector3 point)
        {
            Vector3 ab = b - a;
            float lenSq = ab.sqrMagnitude;

            if (lenSq < 1e-6f)
                return a;

            float t = Vector3.Dot(point - a, ab) / lenSq;
            return a + ab * t;
        }

        // ==========================================================================
        // CLOSEST POINT ON RAY
        // ==========================================================================
        public static Vector3 ClosestPointOnRayToPoint(Ray ray, Vector3 point)
        {
            Vector3 o = ray.origin;
            Vector3 d = ray.direction;

            float t = Vector3.Dot(point - o, d);
            if (t < 0f) t = 0f;

            return o + d * t;
        }

        // Overload: origin + direction
        public static Vector3 ClosestPointOnRayToPoint(Vector3 origin, Vector3 direction, Vector3 point)
        {
            return ClosestPointOnRayToPoint(new Ray(origin, direction), point);
        }

        // ==========================================================================
        // CORE: CLOSEST POINT ON SEGMENT
        // ==========================================================================
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

        // Aliases
        public static Vector3 ClosestPointOnSegmentToPoint(Vector3 a, Vector3 b, Vector3 point)
        {
            return ClosestPoint(a, b, point);
        }

        public static Vector3 ClosestPointOnSegment(Vector3 a, Vector3 b, Vector3 point)
        {
            return ClosestPoint(a, b, point);
        }

        // ==========================================================================
        // SEGMENT vs LINE CLOSEST POINTS
        // ==========================================================================
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

        public static Vector3 ClosestPointOnSegmentToLine(
            Vector3 segA, Vector3 segB,
            Vector3 lineA, Vector3 lineB)
        {
            ClosestPointOnSegmentToLine(segA, segB, lineA, lineB, out Vector3 segPoint, out _);
            return segPoint;
        }

        // ==========================================================================
        // SEGMENT vs RAY CLOSEST POINTS
        // ==========================================================================
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

        public static Vector3 ClosestPointOnSegmentToRay(
            Vector3 segA, Vector3 segB,
            Ray ray)
        {
            ClosestPointOnSegmentToRay(segA, segB, ray, out Vector3 segPoint, out _);
            return segPoint;
        }

        // ==========================================================================
        // SEGMENT vs SEGMENT CLOSEST POINTS
        // ==========================================================================
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

        // ==========================================================================
        // SEGMENT vs SEGMENT SQUARED DISTANCE
        // ==========================================================================
        public static float SegmentSegmentDistanceSq(
            Vector3 p1, Vector3 q1,
            Vector3 p2, Vector3 q2)
        {
            ClosestPoints(p1, q1, p2, q2, out Vector3 c1, out Vector3 c2);
            return (c1 - c2).sqrMagnitude;
        }

        // ==========================================================================
        // LENGTH AND DIRECTION
        // ==========================================================================
        public static float Length(Vector3 a, Vector3 b)
        {
            return (b - a).magnitude;
        }

        public static Vector3 Direction(Vector3 a, Vector3 b)
        {
            Vector3 d = b - a;
            float m = d.magnitude;
            return m > 1e-6f ? d / m : Vector3.zero;
        }
    }
}
