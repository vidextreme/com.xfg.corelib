// Copyright (c) 2025 John David Uy
// Licensed under the MIT License. See LICENSE for details.
// ------------------------------------------------------------------------------
// RayMath
// ------------------------------------------------------------------------------
// Math utilities for rays.
//
// Provides:
// - Closest point on ray
// - Distance to point
// - Ray vs segment closest points
// - Ray vs ray closest points
//
// All repeated property getters are cached.
// ------------------------------------------------------------------------------

using UnityEngine;

namespace XFG.Math
{
    public static class RayMath
    {
        // ==========================================================================
        // CLOSEST POINT ON RAY
        // ==========================================================================
        public static Vector3 ClosestPoint(Ray ray, Vector3 point)
        {
            Vector3 o = ray.origin;
            Vector3 d = ray.direction;

            float t = Vector3.Dot(point - o, d);
            if (t < 0f) t = 0f;

            return o + d * t;
        }

        // ==========================================================================
        // DISTANCE TO POINT
        // ==========================================================================
        public static float DistanceToPoint(Ray ray, Vector3 point)
        {
            Vector3 cp = ClosestPoint(ray, point);
            return (cp - point).magnitude;
        }

        // ==========================================================================
        // RAY vs SEGMENT CLOSEST POINTS
        // ==========================================================================
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

        // ==========================================================================
        // RAY vs RAY CLOSEST POINTS
        // ==========================================================================
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
