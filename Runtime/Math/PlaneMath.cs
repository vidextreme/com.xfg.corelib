// Copyright (c) 2025 John David Uy
// Licensed under the MIT License. See LICENSE for details.
// ------------------------------------------------------------------------------
// PlaneMath
// ------------------------------------------------------------------------------
// Low-level math utilities for planes.
//
// Provides:
// - Construction from point/normal or 3 points
// - Normalization
// - Signed distance
// - Projection
// - Ray/segment intersection
// - Plane flipping
// - Plane-plane intersection
// - AABB/OBB classification
//
// All methods are deterministic, allocation-free, and suitable for Burst jobs.
// ------------------------------------------------------------------------------

using UnityEngine;

namespace XFG.Math.Shape
{
    public static class PlaneMath
    {
        // ==========================================================================
        // CONSTRUCTION
        // ==========================================================================
        public static Plane FromPointNormal(Vector3 point, Vector3 normal)
        {
            Vector3 n = normal.normalized;
            float d = -Vector3.Dot(n, point);
            return new Plane(n, d);
        }

        public static Plane FromPoints(Vector3 a, Vector3 b, Vector3 c)
        {
            Vector3 n = Vector3.Cross(b - a, c - a).normalized;
            float d = -Vector3.Dot(n, a);
            return new Plane(n, d);
        }

        // ==========================================================================
        // NORMALIZATION
        // ==========================================================================
        public static Plane Normalize(Plane p)
        {
            Vector3 n = p.normal;
            float mag = n.magnitude;

            if (mag < 1e-6f)
                return p;

            float inv = 1f / mag;
            return new Plane(n * inv, p.distance * inv);
        }

        // ==========================================================================
        // SIGNED DISTANCE
        // ==========================================================================
        public static float DistanceToPoint(Plane p, Vector3 point)
        {
            Vector3 n = p.normal;
            float d = p.distance;
            return Vector3.Dot(n, point) + d;
        }

        // ==========================================================================
        // PROJECTION
        // ==========================================================================
        public static Vector3 ProjectPoint(Plane p, Vector3 point)
        {
            Vector3 n = p.normal;
            float d = p.distance;
            float dist = Vector3.Dot(n, point) + d;
            return point - n * dist;
        }

        // ==========================================================================
        // RAY INTERSECTION
        // ==========================================================================
        public static bool Raycast(Plane p, Vector3 origin, Vector3 dir, out float t)
        {
            t = 0f;

            Vector3 n = p.normal;
            float d = p.distance;

            float denom = Vector3.Dot(n, dir);
            if (Mathf.Abs(denom) < 1e-6f)
                return false;

            float num = -(Vector3.Dot(n, origin) + d);
            t = num / denom;

            return t >= 0f;
        }

        // ==========================================================================
        // SEGMENT INTERSECTION
        // ==========================================================================
        public static bool SegmentIntersect(Plane p, Vector3 a, Vector3 b, out Vector3 hit)
        {
            hit = Vector3.zero;

            Vector3 n = p.normal;
            float d = p.distance;

            Vector3 ab = b - a;
            float denom = Vector3.Dot(n, ab);

            if (Mathf.Abs(denom) < 1e-6f)
                return false;

            float t = -(Vector3.Dot(n, a) + d) / denom;
            if (t < 0f || t > 1f)
                return false;

            hit = a + ab * t;
            return true;
        }

        // ==========================================================================
        // FLIP
        // ==========================================================================
        public static Plane Flip(Plane p)
        {
            Vector3 n = p.normal;
            float d = p.distance;
            return new Plane(-n, -d);
        }

        // ==========================================================================
        // PLANE-PLANE INTERSECTION
        // ==========================================================================
        public static bool Intersect(Plane a, Plane b, out Vector3 point, out Vector3 dir)
        {
            point = Vector3.zero;

            Vector3 n1 = a.normal;
            Vector3 n2 = b.normal;

            dir = Vector3.Cross(n1, n2);
            float denom = dir.sqrMagnitude;

            if (denom < 1e-6f)
                return false;

            float d1 = -a.distance;
            float d2 = -b.distance;

            Vector3 c1 = Vector3.Cross(dir, n2) * d1;
            Vector3 c2 = Vector3.Cross(n1, dir) * d2;

            point = (c1 + c2) / denom;
            return true;
        }

        // ==========================================================================
        // AABB CLASSIFICATION
        // ==========================================================================
        public static int ClassifyAABB(Plane p, Bounds b)
        {
            Vector3 n = p.normal;
            float d = p.distance;

            Vector3 c = b.center;
            Vector3 e = b.extents;

            float r =
                Mathf.Abs(n.x * e.x) +
                Mathf.Abs(n.y * e.y) +
                Mathf.Abs(n.z * e.z);

            float s = Vector3.Dot(n, c) + d;

            if (s > r) return +1;
            if (s < -r) return -1;
            return 0;
        }

        // ==========================================================================
        // OBB CLASSIFICATION
        // ==========================================================================
        public static int ClassifyOBB(Plane p, OBB b)
        {
            Vector3 n = p.normal;
            float d = p.distance;

            float r =
                Mathf.Abs(Vector3.Dot(n, b.Right)) * b.Extents.x +
                Mathf.Abs(Vector3.Dot(n, b.Up)) * b.Extents.y +
                Mathf.Abs(Vector3.Dot(n, b.Forward)) * b.Extents.z;

            float s = Vector3.Dot(n, b.Center) + d;

            if (s > r) return +1;
            if (s < -r) return -1;
            return 0;
        }
    }
}
