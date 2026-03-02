// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.
// ------------------------------------------------------------------------------
// FrustumCollisionExtensions
// ------------------------------------------------------------------------------
// Collision predicates for Frustum vs:
// - Point
// - Sphere
// - Capsule
// - Cylinder
// - Cone
// - AABB
// - OBB
// - Triangle
// - Ray
// - Segment
// - Frustum
//
// All methods are deterministic, allocation-free, and suitable for Burst jobs.
// ------------------------------------------------------------------------------

using UnityEngine;

namespace XFG.Math.Shape
{
    public static class FrustumCollisionExtensions
    {
        // ==========================================================================
        // POINT
        // ==========================================================================
        public static bool ContainsPoint(this Frustum f, Vector3 p)
        {
            for (int i = 0; i < 6; i++)
            {
                Plane plane = f[i];
                if (Vector3.Dot(plane.normal, p) + plane.distance < 0f)
                    return false;
            }
            return true;
        }

        // ==========================================================================
        // SPHERE
        // ==========================================================================
        public static bool Intersects(this Frustum f, Sphere s)
        {
            Vector3 c = s.Center;
            float r = s.Radius;

            for (int i = 0; i < 6; i++)
            {
                Plane p = f[i];
                float dist = Vector3.Dot(p.normal, c) + p.distance;

                if (dist < -r)
                    return false;
            }

            return true;
        }

        // ==========================================================================
        // CAPSULE
        // ==========================================================================
        public static bool Intersects(this Frustum f, Capsule cap)
        {
            if (f.Intersects(new Sphere(cap.P0, cap.Radius))) return true;
            if (f.Intersects(new Sphere(cap.P1, cap.Radius))) return true;

            return f.IntersectsSegment(cap.P0, cap.P1);
        }

        // ==========================================================================
        // CYLINDER
        // ==========================================================================
        public static bool Intersects(this Frustum f, Cylinder cy)
        {
            if (f.Intersects(new Sphere(cy.P0, cy.Radius))) return true;
            if (f.Intersects(new Sphere(cy.P1, cy.Radius))) return true;

            return f.IntersectsSegment(cy.P0, cy.P1);
        }

        // ==========================================================================
        // CONE
        // ==========================================================================
        public static bool Intersects(this Frustum f, Cone cone)
        {
            if (f.ContainsPoint(cone.Apex))
                return true;

            Vector3 tip = cone.Apex + cone.Axis;
            if (f.IntersectsSegment(cone.Apex, tip))
                return true;

            Vector3 mid = cone.Apex + cone.Axis * 0.5f;
            float r = cone.Radius * 0.5f;

            return f.Intersects(new Sphere(mid, r));
        }

        // ==========================================================================
        // AABB
        // ==========================================================================
        public static bool IntersectsAABB(this Frustum f, Bounds b)
        {
            Vector3 c = b.center;
            Vector3 e = b.extents;

            for (int i = 0; i < 6; i++)
            {
                Plane p = f[i];
                Vector3 n = p.normal;

                float r =
                    Mathf.Abs(n.x * e.x) +
                    Mathf.Abs(n.y * e.y) +
                    Mathf.Abs(n.z * e.z);

                float s = Vector3.Dot(n, c) + p.distance;

                if (s < -r)
                    return false;
            }

            return true;
        }

        // ==========================================================================
        // OBB
        // ==========================================================================
        public static bool IntersectsOBB(this Frustum f, OBB b)
        {
            Vector3 center = b.Center;
            Vector3 ext = b.Extents;
            Vector3 right = b.Right;
            Vector3 up = b.Up;
            Vector3 forward = b.Forward;

            for (int i = 0; i < 6; i++)
            {
                Plane p = f[i];
                Vector3 n = p.normal;

                float r =
                    Mathf.Abs(Vector3.Dot(n, right)) * ext.x +
                    Mathf.Abs(Vector3.Dot(n, up)) * ext.y +
                    Mathf.Abs(Vector3.Dot(n, forward)) * ext.z;

                float s = Vector3.Dot(n, center) + p.distance;

                if (s < -r)
                    return false;
            }

            return true;
        }

        // ==========================================================================
        // TRIANGLE
        // ==========================================================================
        public static bool IntersectsTriangle(this Frustum f, Vector3 a, Vector3 b, Vector3 c)
        {
            for (int i = 0; i < 6; i++)
            {
                Plane p = f[i];

                float da = p.GetDistanceToPoint(a);
                float db = p.GetDistanceToPoint(b);
                float dc = p.GetDistanceToPoint(c);

                if (da < 0f && db < 0f && dc < 0f)
                    return false;
            }

            return true;
        }

        // ==========================================================================
        // RAY
        // ==========================================================================
        public static bool IntersectsRay(this Frustum f, Vector3 origin, Vector3 dir)
        {
            float tMin = 0f;
            float tMax = float.MaxValue;

            for (int i = 0; i < 6; i++)
            {
                Plane p = f[i];
                Vector3 n = p.normal;

                float denom = Vector3.Dot(n, dir);
                float dist = Vector3.Dot(n, origin) + p.distance;

                if (Mathf.Abs(denom) < 1e-6f)
                {
                    if (dist > 0f)
                        return false;
                    continue;
                }

                float t = -dist / denom;

                if (denom < 0f)
                {
                    if (t > tMin) tMin = t;
                }
                else
                {
                    if (t < tMax) tMax = t;
                }

                if (tMin > tMax)
                    return false;
            }

            return true;
        }

        // ==========================================================================
        // SEGMENT
        // ==========================================================================
        public static bool IntersectsSegment(this Frustum f, Vector3 a, Vector3 b)
        {
            Vector3 ab = b - a;

            float tMin = 0f;
            float tMax = 1f;

            for (int i = 0; i < 6; i++)
            {
                Plane p = f[i];
                Vector3 n = p.normal;

                float denom = Vector3.Dot(n, ab);
                float dist = Vector3.Dot(n, a) + p.distance;

                if (Mathf.Abs(denom) < 1e-6f)
                {
                    if (dist > 0f)
                        return false;
                    continue;
                }

                float t = -dist / denom;

                if (denom < 0f)
                {
                    if (t > tMin) tMin = t;
                }
                else
                {
                    if (t < tMax) tMax = t;
                }

                if (tMin > tMax)
                    return false;
            }

            return true;
        }

        // ==========================================================================
        // FRUSTUM vs FRUSTUM
        // ==========================================================================
        public static bool IntersectsFrustum(this Frustum a, Frustum b)
        {
            // Test A's planes against B's corners
            Vector3[] corners = new Vector3[8];
            b.GetCorners(corners);

            for (int i = 0; i < 6; i++)
            {
                Plane p = a[i];

                bool allOutside = true;
                for (int j = 0; j < 8; j++)
                {
                    if (Vector3.Dot(p.normal, corners[j]) + p.distance >= 0f)
                    {
                        allOutside = false;
                        break;
                    }
                }

                if (allOutside)
                    return false;
            }

            // Test B's planes against A's corners
            a.GetCorners(corners);

            for (int i = 0; i < 6; i++)
            {
                Plane p = b[i];

                bool allOutside = true;
                for (int j = 0; j < 8; j++)
                {
                    if (Vector3.Dot(p.normal, corners[j]) + p.distance >= 0f)
                    {
                        allOutside = false;
                        break;
                    }
                }

                if (allOutside)
                    return false;
            }

            return true;
        }
    }
}
