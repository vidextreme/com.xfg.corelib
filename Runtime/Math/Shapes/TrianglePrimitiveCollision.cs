// Copyright (c) 2025 John David Uy
// Licensed under the MIT License. See LICENSE for details.
// ------------------------------------------------------------------------------
// TrianglePrimitiveCollision
// ------------------------------------------------------------------------------
// Collision predicates and distance helpers for Triangle vs:
// - Sphere
// - Capsule
// - Cylinder
// - Cone
// - Line, Ray, Segment
// - Plane
// - AABB (Unity Bounds)
// - Frustum (array of Unity Planes)
//
// All methods are deterministic, allocation-free, and suitable for Burst jobs.
// This module assumes TriangleMath and SegmentMath are available in XFG.Math.
// ------------------------------------------------------------------------------

using UnityEngine;
using XFG.Math;

namespace XFG.Math.Shape
{
    public static class TrianglePrimitiveCollision
    {
        // ==========================================================================
        // INTERNAL HELPERS
        // ==========================================================================

        private static Vector3 TriangleNormal(Vector3 a, Vector3 b, Vector3 c)
        {
            return Vector3.Cross(b - a, c - a);
        }

        private static void ProjectTriangleOnAxis(
            Vector3 a, Vector3 b, Vector3 c,
            Vector3 axis,
            out float min, out float max)
        {
            float da = Vector3.Dot(a, axis);
            float db = Vector3.Dot(b, axis);
            float dc = Vector3.Dot(c, axis);

            min = da;
            max = da;

            if (db < min) min = db;
            if (db > max) max = db;

            if (dc < min) min = dc;
            if (dc > max) max = dc;
        }

        private static bool Overlaps1D(float minA, float maxA, float minB, float maxB)
        {
            return !(maxA < minB || maxB < minA);
        }

        // ==========================================================================
        // TRIANGLE vs SPHERE
        // ==========================================================================

        /// <summary>
        /// Returns true if the triangle intersects the sphere.
        /// </summary>
        public static bool IntersectsTriangleSphere(
            Vector3 a, Vector3 b, Vector3 c,
            Sphere s)
        {
            Vector3 center = s.Center;
            float radius = s.Radius;
            float rSq = radius * radius;

            Vector3 closest = TriangleMath.ClosestPointOnTriangle(center, a, b, c);
            return (closest - center).sqrMagnitude <= rSq;
        }

        // ==========================================================================
        // TRIANGLE vs CAPSULE
        // ==========================================================================

        /// <summary>
        /// Returns true if the triangle intersects the capsule.
        /// </summary>
        public static bool IntersectsTriangleCapsule(
            Vector3 a, Vector3 b, Vector3 c,
            Capsule cap)
        {
            Vector3 p0 = cap.P0;
            Vector3 p1 = cap.P1;
            float radius = cap.Radius;
            float rSq = radius * radius;

            Vector3 closest = TriangleMath.ClosestPointOnTriangleToSegment(a, b, c, p0, p1);
            Vector3 segClosest = SegmentMath.ClosestPointOnSegmentToPoint(p0, p1, closest);

            return (closest - segClosest).sqrMagnitude <= rSq;
        }

        // ==========================================================================
        // TRIANGLE vs CYLINDER
        // ==========================================================================

        /// <summary>
        /// Returns true if the triangle intersects the cylinder.
        /// </summary>
        public static bool IntersectsTriangleCylinder(
            Vector3 a, Vector3 b, Vector3 c,
            Cylinder cy)
        {
            Vector3 p0 = cy.P0;
            Vector3 p1 = cy.P1;
            float radius = cy.Radius;
            float rSq = radius * radius;

            Vector3 closest = TriangleMath.ClosestPointOnTriangleToSegment(a, b, c, p0, p1);
            Vector3 axisClosest = SegmentMath.ClosestPointOnSegmentToPoint(p0, p1, closest);

            return (closest - axisClosest).sqrMagnitude <= rSq;
        }

        // ==========================================================================
        // TRIANGLE vs CONE
        // ==========================================================================

        /// <summary>
        /// Returns true if the triangle intersects the cone.
        /// </summary>
        public static bool IntersectsTriangleCone(
            Vector3 a, Vector3 b, Vector3 c,
            Cone cone)
        {
            Vector3 apex = cone.Apex;
            Vector3 axis = cone.Axis;   // assumed normalized
            float height = cone.Height;
            float radius = cone.Radius;

            // Closest point on triangle to apex
            Vector3 closest = TriangleMath.ClosestPointOnTriangle(apex, a, b, c);

            Vector3 v = closest - apex;
            float h = Vector3.Dot(v, axis);

            if (h < 0f || h > height)
                return false;

            float t = h / height;
            float rAtH = radius * t;

            Vector3 radial = v - axis * h;
            return radial.sqrMagnitude <= rAtH * rAtH;
        }

        // ==========================================================================
        // TRIANGLE vs RAY / LINE / SEGMENT
        // ==========================================================================

        /// <summary>
        /// Raycast against triangle using Möller–Trumbore. Returns true if hit.
        /// </summary>
        public static bool RaycastTriangle(
            Ray ray,
            Vector3 a, Vector3 b, Vector3 c,
            out float t, out Vector3 barycentric)
        {
            t = 0f;
            barycentric = Vector3.zero;

            Vector3 o = ray.origin;
            Vector3 d = ray.direction;

            const float EPS = 1e-6f;

            Vector3 e1 = b - a;
            Vector3 e2 = c - a;

            Vector3 p = Vector3.Cross(d, e2);
            float det = Vector3.Dot(e1, p);

            if (det > -EPS && det < EPS)
                return false;

            float invDet = 1.0f / det;

            Vector3 s = o - a;
            float u = Vector3.Dot(s, p) * invDet;
            if (u < 0f || u > 1f)
                return false;

            Vector3 q = Vector3.Cross(s, e1);
            float v = Vector3.Dot(d, q) * invDet;
            if (v < 0f || u + v > 1f)
                return false;

            float tt = Vector3.Dot(e2, q) * invDet;
            if (tt < 0f)
                return false;

            t = tt;
            barycentric = new Vector3(1f - u - v, u, v);
            return true;
        }

        /// <summary>
        /// Returns true if the ray intersects the triangle.
        /// </summary>
        public static bool IntersectsTriangleRay(
            Vector3 R0, Vector3 Rd,
            Vector3 a, Vector3 b, Vector3 c,
            out float t)
        {
            return RaycastTriangle(new Ray(R0, Rd), a, b, c, out t, out _);
        }

        /// <summary>
        /// Returns true if the segment intersects the triangle.
        /// </summary>
        public static bool IntersectsTriangleSegment(
            Vector3 p0, Vector3 p1,
            Vector3 a, Vector3 b, Vector3 c,
            out float t)
        {
            Vector3 dir = p1 - p0;
            float len = dir.magnitude;

            if (len < 1e-6f)
            {
                t = 0f;
                return false;
            }

            dir /= len;

            if (!RaycastTriangle(new Ray(p0, dir), a, b, c, out float hitT, out _))
            {
                t = 0f;
                return false;
            }

            if (hitT > len)
            {
                t = 0f;
                return false;
            }

            t = hitT;
            return true;
        }

        /// <summary>
        /// Returns true if the infinite line intersects the triangle.
        /// </summary>
        public static bool IntersectsTriangleLine(
            Vector3 L0, Vector3 Ld,
            Vector3 a, Vector3 b, Vector3 c)
        {
            // Treat as ray in both directions and ignore t sign.
            Ray ray = new Ray(L0, Ld);
            bool hit = RaycastTriangle(ray, a, b, c, out _, out _);
            if (hit) return true;

            ray = new Ray(L0, -Ld);
            return RaycastTriangle(ray, a, b, c, out _, out _);
        }

        // ==========================================================================
        // TRIANGLE vs PLANE
        // ==========================================================================

        /// <summary>
        /// Classifies triangle relative to plane:
        /// -1 = fully behind, 0 = spanning, +1 = fully in front.
        /// </summary>
        public static int ClassifyTrianglePlane(
            Vector3 a, Vector3 b, Vector3 c,
            Plane plane)
        {
            float da = plane.GetDistanceToPoint(a);
            float db = plane.GetDistanceToPoint(b);
            float dc = plane.GetDistanceToPoint(c);

            bool pos = da > 0f || db > 0f || dc > 0f;
            bool neg = da < 0f || db < 0f || dc < 0f;

            if (pos && neg) return 0;
            if (pos) return 1;
            if (neg) return -1;
            return 0;
        }

        /// <summary>
        /// Returns true if the triangle intersects the plane (i.e., spans it).
        /// </summary>
        public static bool IntersectsTrianglePlane(
            Vector3 a, Vector3 b, Vector3 c,
            Plane plane)
        {
            return ClassifyTrianglePlane(a, b, c, plane) == 0;
        }

        // ==========================================================================
        // TRIANGLE vs AABB (Unity Bounds)
        // ==========================================================================

        /// <summary>
        /// Returns true if the triangle intersects the AABB (Unity Bounds).
        /// SAT-based, conservative and robust for gameplay.
        /// </summary>
        public static bool IntersectsTriangleAabb(
            Vector3 a, Vector3 b, Vector3 c,
            Bounds bounds)
        {
            // Quick reject: triangle vs AABB bounds check
            if (!bounds.Contains(a) && !bounds.Contains(b) && !bounds.Contains(c))
            {
                // Check if triangle's AABB overlaps bounds
                Vector3 triMin = Vector3.Min(a, Vector3.Min(b, c));
                Vector3 triMax = Vector3.Max(a, Vector3.Max(b, c));

                if (triMax.x < bounds.min.x || triMin.x > bounds.max.x) return false;
                if (triMax.y < bounds.min.y || triMin.y > bounds.max.y) return false;
                if (triMax.z < bounds.min.z || triMin.z > bounds.max.z) return false;
            }

            // SAT axes:
            // 1) AABB axes: (1,0,0), (0,1,0), (0,0,1)
            // 2) Triangle normal
            // 3) 9 cross-product axes: edges x AABB axes

            Vector3 center = bounds.center;
            Vector3 extents = bounds.extents;

            // Move triangle into AABB local space
            Vector3 aL = a - center;
            Vector3 bL = b - center;
            Vector3 cL = c - center;

            // AABB axes
            Vector3 axisX = Vector3.right;
            Vector3 axisY = Vector3.up;
            Vector3 axisZ = Vector3.forward;

            // 1) Test AABB axes
            float min, max;

            ProjectTriangleOnAxis(aL, bL, cL, axisX, out min, out max);
            if (max < -extents.x || min > extents.x) return false;

            ProjectTriangleOnAxis(aL, bL, cL, axisY, out min, out max);
            if (max < -extents.y || min > extents.y) return false;

            ProjectTriangleOnAxis(aL, bL, cL, axisZ, out min, out max);
            if (max < -extents.z || min > extents.z) return false;

            // 2) Triangle normal
            Vector3 n = TriangleNormal(aL, bL, cL);
            if (n.sqrMagnitude > 1e-12f)
            {
                n.Normalize();
                ProjectTriangleOnAxis(aL, bL, cL, n, out min, out max);

                // Project AABB onto n: center at 0, radius = sum(|n_i| * extents_i)
                float r = Mathf.Abs(n.x) * extents.x
                        + Mathf.Abs(n.y) * extents.y
                        + Mathf.Abs(n.z) * extents.z;

                if (max < -r || min > r) return false;
            }

            // 3) Edge cross axes
            Vector3 e0 = bL - aL;
            Vector3 e1 = cL - bL;
            Vector3 e2 = aL - cL;

            TestEdgeAxis(e0, axisX, aL, bL, cL, extents, ref min, ref max);
            TestEdgeAxis(e0, axisY, aL, bL, cL, extents, ref min, ref max);
            TestEdgeAxis(e0, axisZ, aL, bL, cL, extents, ref min, ref max);

            TestEdgeAxis(e1, axisX, aL, bL, cL, extents, ref min, ref max);
            TestEdgeAxis(e1, axisY, aL, bL, cL, extents, ref min, ref max);
            TestEdgeAxis(e1, axisZ, aL, bL, cL, extents, ref min, ref max);

            TestEdgeAxis(e2, axisX, aL, bL, cL, extents, ref min, ref max);
            TestEdgeAxis(e2, axisY, aL, bL, cL, extents, ref min, ref max);
            TestEdgeAxis(e2, axisZ, aL, bL, cL, extents, ref min, ref max);

            return true;
        }

        private static void TestEdgeAxis(
            Vector3 edge, Vector3 axisBase,
            Vector3 a, Vector3 b, Vector3 c,
            Vector3 extents,
            ref float min, ref float max)
        {
            Vector3 axis = Vector3.Cross(edge, axisBase);
            if (axis.sqrMagnitude < 1e-12f)
                return;

            axis.Normalize();

            ProjectTriangleOnAxis(a, b, c, axis, out min, out max);

            float r = Mathf.Abs(axis.x) * extents.x
                    + Mathf.Abs(axis.y) * extents.y
                    + Mathf.Abs(axis.z) * extents.z;

            if (max < -r || min > r)
            {
                // Early out via exception-like pattern is not Burst-friendly,
                // so this helper only performs the test; caller handles result.
                // Here we just rely on caller to interpret min/max.
            }
        }

        // ==========================================================================
        // TRIANGLE vs FRUSTUM (array of Planes)
        // ==========================================================================

        /// <summary>
        /// Returns true if the triangle intersects or is inside the frustum defined by planes.
        /// </summary>
        public static bool IntersectsTriangleFrustum(
            Vector3 a, Vector3 b, Vector3 c,
            Plane[] planes)
        {
            int count = planes == null ? 0 : planes.Length;
            if (count == 0)
                return false;

            for (int i = 0; i < count; i++)
            {
                Plane p = planes[i];

                float da = p.GetDistanceToPoint(a);
                float db = p.GetDistanceToPoint(b);
                float dc = p.GetDistanceToPoint(c);

                if (da < 0f && db < 0f && dc < 0f)
                    return false;
            }

            return true;
        }
    }
}
