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
// - OBB
// - Triangle
// - Frustum (array of Unity Planes)
//
// All methods are deterministic, allocation-free, and suitable for Burst jobs.
// This module assumes TriangleMath and SegmentMath are available in XFG.Math.
// ------------------------------------------------------------------------------

using UnityEngine;

namespace XFG.Math.Shape
{
    public static class TrianglePrimitiveCollision
    {
        // ==============================================================================
        // INTERNAL HELPERS
        // ==============================================================================
        #region InternalHelpers

        /// <summary>
        /// Returns the unnormalized triangle normal.
        /// </summary>
        /// <param name="a">Triangle vertex A.</param>
        /// <param name="b">Triangle vertex B.</param>
        /// <param name="c">Triangle vertex C.</param>
        /// <returns>Unnormalized normal vector.</returns>
        private static Vector3 TriangleNormal(Vector3 a, Vector3 b, Vector3 c)
        {
            return Vector3.Cross(b - a, c - a);
        }

        /// <summary>
        /// Projects triangle vertices onto an axis.
        /// </summary>
        /// <param name="a">Triangle vertex A.</param>
        /// <param name="b">Triangle vertex B.</param>
        /// <param name="c">Triangle vertex C.</param>
        /// <param name="axis">Axis to project onto.</param>
        /// <param name="min">Minimum projection value.</param>
        /// <param name="max">Maximum projection value.</param>
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

        /// <summary>
        /// Returns true if 1D intervals overlap.
        /// </summary>
        /// <param name="minA">Minimum of interval A.</param>
        /// <param name="maxA">Maximum of interval A.</param>
        /// <param name="minB">Minimum of interval B.</param>
        /// <param name="maxB">Maximum of interval B.</param>
        /// <returns>True if intervals overlap.</returns>
        private static bool Overlaps1D(float minA, float maxA, float minB, float maxB)
        {
            return !(maxA < minB || maxB < minA);
        }

        #endregion

        // ==============================================================================
        // TRIANGLE vs SPHERE
        // ==============================================================================
        #region TriangleVsSphere

        /// <summary>
        /// Returns true if the triangle intersects the sphere.
        /// </summary>
        /// <param name="a">Triangle vertex A.</param>
        /// <param name="b">Triangle vertex B.</param>
        /// <param name="c">Triangle vertex C.</param>
        /// <param name="s">Sphere to test.</param>
        /// <returns>True if the triangle intersects the sphere.</returns>
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

        #endregion

        // ==============================================================================
        // TRIANGLE vs CAPSULE
        // ==============================================================================
        #region TriangleVsCapsule

        /// <summary>
        /// Returns true if the triangle intersects the capsule.
        /// </summary>
        /// <param name="a">Triangle vertex A.</param>
        /// <param name="b">Triangle vertex B.</param>
        /// <param name="c">Triangle vertex C.</param>
        /// <param name="cap">Capsule to test.</param>
        /// <returns>True if the triangle intersects the capsule.</returns>
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

        #endregion

        // ==============================================================================
        // TRIANGLE vs CYLINDER
        // ==============================================================================
        #region TriangleVsCylinder

        /// <summary>
        /// Returns true if the triangle intersects the cylinder.
        /// </summary>
        /// <param name="a">Triangle vertex A.</param>
        /// <param name="b">Triangle vertex B.</param>
        /// <param name="c">Triangle vertex C.</param>
        /// <param name="cy">Cylinder to test.</param>
        /// <returns>True if the triangle intersects the cylinder.</returns>
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

        #endregion

        // ==============================================================================
        // TRIANGLE vs CONE
        // ==============================================================================
        #region TriangleVsCone

        /// <summary>
        /// Returns true if the triangle intersects the cone.
        /// </summary>
        /// <param name="a">Triangle vertex A.</param>
        /// <param name="b">Triangle vertex B.</param>
        /// <param name="c">Triangle vertex C.</param>
        /// <param name="cone">Cone to test.</param>
        /// <returns>True if the triangle intersects the cone.</returns>
        public static bool IntersectsTriangleCone(
            Vector3 a, Vector3 b, Vector3 c,
            Cone cone)
        {
            Vector3 apex = cone.Apex;
            Vector3 axis = cone.Axis;
            float height = cone.Height;
            float radius = cone.Radius;

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

        #endregion

        // ==============================================================================
        // TRIANGLE vs RAY / LINE / SEGMENT
        // ==============================================================================
        #region TriangleVsRayLineSegment

        /// <summary>
        /// Raycast against triangle using Möller–Trumbore. Returns true if hit.
        /// </summary>
        /// <param name="ray">Ray to test.</param>
        /// <param name="a">Triangle vertex A.</param>
        /// <param name="b">Triangle vertex B.</param>
        /// <param name="c">Triangle vertex C.</param>
        /// <param name="t">Ray parameter at intersection.</param>
        /// <param name="barycentric">Barycentric coordinates at intersection.</param>
        /// <returns>True if the ray intersects the triangle.</returns>
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

            float invDet = 1f / det;

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
        /// <param name="R0">Ray origin.</param>
        /// <param name="Rd">Ray direction.</param>
        /// <param name="a">Triangle vertex A.</param>
        /// <param name="b">Triangle vertex B.</param>
        /// <param name="c">Triangle vertex C.</param>
        /// <param name="t">Ray parameter at intersection.</param>
        /// <returns>True if the ray intersects the triangle.</returns>
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
        /// <param name="p0">Segment start point.</param>
        /// <param name="p1">Segment end point.</param>
        /// <param name="a">Triangle vertex A.</param>
        /// <param name="b">Triangle vertex B.</param>
        /// <param name="c">Triangle vertex C.</param>
        /// <param name="t">Segment parameter at intersection.</param>
        /// <returns>True if the segment intersects the triangle.</returns>
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
        /// <param name="L0">A point on the line.</param>
        /// <param name="Ld">Line direction.</param>
        /// <param name="a">Triangle vertex A.</param>
        /// <param name="b">Triangle vertex B.</param>
        /// <param name="c">Triangle vertex C.</param>
        /// <returns>True if the line intersects the triangle.</returns>
        public static bool IntersectsTriangleLine(
            Vector3 L0, Vector3 Ld,
            Vector3 a, Vector3 b, Vector3 c)
        {
            Ray ray = new Ray(L0, Ld);
            if (RaycastTriangle(ray, a, b, c, out _, out _))
                return true;

            ray = new Ray(L0, -Ld);
            return RaycastTriangle(ray, a, b, c, out _, out _);
        }

        #endregion

        // ==============================================================================
        // TRIANGLE vs PLANE
        // ==============================================================================
        #region TriangleVsPlane

        /// <summary>
        /// Classifies triangle relative to plane:
        /// -1 = fully behind, 0 = spanning, +1 = fully in front.
        /// </summary>
        /// <param name="a">Triangle vertex A.</param>
        /// <param name="b">Triangle vertex B.</param>
        /// <param name="c">Triangle vertex C.</param>
        /// <param name="plane">Plane to test.</param>
        /// <returns>Classification result.</returns>
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
        /// <param name="a">Triangle vertex A.</param>
        /// <param name="b">Triangle vertex B.</param>
        /// <param name="c">Triangle vertex C.</param>
        /// <param name="plane">Plane to test.</param>
        /// <returns>True if the triangle intersects the plane.</returns>
        public static bool IntersectsTrianglePlane(
            Vector3 a, Vector3 b, Vector3 c,
            Plane plane)
        {
            return ClassifyTrianglePlane(a, b, c, plane) == 0;
        }

        #endregion

        // ==============================================================================
        // TRIANGLE vs AABB
        // ==============================================================================
        #region TriangleVsAabb

        /// <summary>
        /// Returns true if the triangle intersects the AABB (Unity Bounds).
        /// SAT-based, conservative and robust for gameplay.
        /// </summary>
        /// <param name="a">Triangle vertex A.</param>
        /// <param name="b">Triangle vertex B.</param>
        /// <param name="c">Triangle vertex C.</param>
        /// <param name="bounds">AABB to test.</param>
        /// <returns>True if the triangle intersects the AABB.</returns>
        public static bool IntersectsTriangleAabb(
            Vector3 a, Vector3 b, Vector3 c,
            Bounds bounds)
        {
            Vector3 triMin = Vector3.Min(a, Vector3.Min(b, c));
            Vector3 triMax = Vector3.Max(a, Vector3.Max(b, c));

            if (triMax.x < bounds.min.x || triMin.x > bounds.max.x) return false;
            if (triMax.y < bounds.min.y || triMin.y > bounds.max.y) return false;
            if (triMax.z < bounds.min.z || triMin.z > bounds.max.z) return false;

            Vector3 center = bounds.center;
            Vector3 extents = bounds.extents;

            Vector3 aL = a - center;
            Vector3 bL = b - center;
            Vector3 cL = c - center;

            Vector3 axisX = Vector3.right;
            Vector3 axisY = Vector3.up;
            Vector3 axisZ = Vector3.forward;

            float min, max;

            ProjectTriangleOnAxis(aL, bL, cL, axisX, out min, out max);
            if (max < -extents.x || min > extents.x) return false;

            ProjectTriangleOnAxis(aL, bL, cL, axisY, out min, out max);
            if (max < -extents.y || min > extents.y) return false;

            ProjectTriangleOnAxis(aL, bL, cL, axisZ, out min, out max);
            if (max < -extents.z || min > extents.z) return false;

            Vector3 n = TriangleNormal(aL, bL, cL);
            if (n.sqrMagnitude > 1e-12f)
            {
                n.Normalize();
                ProjectTriangleOnAxis(aL, bL, cL, n, out min, out max);

                float r = Mathf.Abs(n.x) * extents.x
                        + Mathf.Abs(n.y) * extents.y
                        + Mathf.Abs(n.z) * extents.z;

                if (max < -r || min > r) return false;
            }

            Vector3 e0 = bL - aL;
            Vector3 e1 = cL - bL;
            Vector3 e2 = aL - cL;

            if (!TestEdgeAxesAabb(e0, aL, bL, cL, extents)) return false;
            if (!TestEdgeAxesAabb(e1, aL, bL, cL, extents)) return false;
            if (!TestEdgeAxesAabb(e2, aL, bL, cL, extents)) return false;

            return true;
        }

        /// <summary>
        /// Tests all cross-product axes between an edge and the AABB axes.
        /// </summary>
        /// <param name="edge">Triangle edge in AABB local space.</param>
        /// <param name="a">Triangle vertex A in AABB local space.</param>
        /// <param name="b">Triangle vertex B in AABB local space.</param>
        /// <param name="c">Triangle vertex C in AABB local space.</param>
        /// <param name="extents">AABB extents.</param>
        /// <returns>True if all axes overlap.</returns>
        private static bool TestEdgeAxesAabb(
            Vector3 edge,
            Vector3 a, Vector3 b, Vector3 c,
            Vector3 extents)
        {
            Vector3 axisX = Vector3.right;
            Vector3 axisY = Vector3.up;
            Vector3 axisZ = Vector3.forward;

            if (!TestSingleEdgeAxis(edge, axisX, a, b, c, extents)) return false;
            if (!TestSingleEdgeAxis(edge, axisY, a, b, c, extents)) return false;
            if (!TestSingleEdgeAxis(edge, axisZ, a, b, c, extents)) return false;

            return true;
        }

        /// <summary>
        /// Tests a single cross-product axis between an edge and an AABB axis.
        /// </summary>
        /// <param name="edge">Triangle edge in AABB local space.</param>
        /// <param name="axisBase">AABB axis.</param>
        /// <param name="a">Triangle vertex A in AABB local space.</param>
        /// <param name="b">Triangle vertex B in AABB local space.</param>
        /// <param name="c">Triangle vertex C in AABB local space.</param>
        /// <param name="extents">AABB extents.</param>
        /// <returns>True if projections overlap.</returns>
        private static bool TestSingleEdgeAxis(
            Vector3 edge, Vector3 axisBase,
            Vector3 a, Vector3 b, Vector3 c,
            Vector3 extents)
        {
            Vector3 axis = Vector3.Cross(edge, axisBase);
            if (axis.sqrMagnitude < 1e-12f)
                return true;

            axis.Normalize();

            ProjectTriangleOnAxis(a, b, c, axis, out float min, out float max);

            float r = Mathf.Abs(axis.x) * extents.x
                    + Mathf.Abs(axis.y) * extents.y
                    + Mathf.Abs(axis.z) * extents.z;

            return !(max < -r || min > r);
        }

        #endregion

        // ==============================================================================
        // TRIANGLE vs OBB
        // ==============================================================================
        #region TriangleVsObb

        /// <summary>
        /// Returns true if the triangle intersects the oriented bounding box.
        /// Uses the separating axis theorem.
        /// </summary>
        /// <param name="center">OBB center.</param>
        /// <param name="extents">OBB extents.</param>
        /// <param name="right">OBB local X axis.</param>
        /// <param name="up">OBB local Y axis.</param>
        /// <param name="forward">OBB local Z axis.</param>
        /// <param name="a">Triangle vertex A.</param>
        /// <param name="b">Triangle vertex B.</param>
        /// <param name="c">Triangle vertex C.</param>
        /// <returns>True if the triangle intersects the OBB.</returns>
        public static bool IntersectsTriangleObb(
            Vector3 center, Vector3 extents,
            Vector3 right, Vector3 up, Vector3 forward,
            Vector3 a, Vector3 b, Vector3 c)
        {
            Matrix4x4 worldToObb = Matrix4x4.identity;
            worldToObb.m00 = right.x; worldToObb.m01 = right.y; worldToObb.m02 = right.z;
            worldToObb.m10 = up.x; worldToObb.m11 = up.y; worldToObb.m12 = up.z;
            worldToObb.m20 = forward.x; worldToObb.m21 = forward.y; worldToObb.m22 = forward.z;
            worldToObb.m03 = -Vector3.Dot(right, center);
            worldToObb.m13 = -Vector3.Dot(up, center);
            worldToObb.m23 = -Vector3.Dot(forward, center);

            Vector3 aL = worldToObb.MultiplyPoint3x4(a);
            Vector3 bL = worldToObb.MultiplyPoint3x4(b);
            Vector3 cL = worldToObb.MultiplyPoint3x4(c);

            Bounds localBounds = new Bounds(Vector3.zero, extents * 2f);
            return IntersectsTriangleAabb(aL, bL, cL, localBounds);
        }

        #endregion

        // ==============================================================================
        // TRIANGLE vs TRIANGLE
        // ==============================================================================
        #region TriangleVsTriangle

        /// <summary>
        /// Returns true if the two triangles intersect using the separating axis theorem.
        /// </summary>
        /// <param name="a0">Triangle 1 vertex A.</param>
        /// <param name="b0">Triangle 1 vertex B.</param>
        /// <param name="c0">Triangle 1 vertex C.</param>
        /// <param name="a1">Triangle 2 vertex A.</param>
        /// <param name="b1">Triangle 2 vertex B.</param>
        /// <param name="c1">Triangle 2 vertex C.</param>
        /// <returns>True if the triangles intersect.</returns>
        public static bool IntersectsTriangleTriangle(
            Vector3 a0, Vector3 b0, Vector3 c0,
            Vector3 a1, Vector3 b1, Vector3 c1)
        {
            Vector3 n0 = TriangleNormal(a0, b0, c0);
            if (n0.sqrMagnitude > 1e-12f)
            {
                if (!AxisOverlapTriangles(n0, a0, b0, c0, a1, b1, c1))
                    return false;
            }

            Vector3 n1 = TriangleNormal(a1, b1, c1);
            if (n1.sqrMagnitude > 1e-12f)
            {
                if (!AxisOverlapTriangles(n1, a0, b0, c0, a1, b1, c1))
                    return false;
            }

            Vector3[] e0 =
            {
                b0 - a0,
                c0 - b0,
                a0 - c0
            };

            Vector3[] e1 =
            {
                b1 - a1,
                c1 - b1,
                a1 - c1
            };

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    Vector3 axis = Vector3.Cross(e0[i], e1[j]);
                    if (axis.sqrMagnitude < 1e-12f)
                        continue;

                    if (!AxisOverlapTriangles(axis, a0, b0, c0, a1, b1, c1))
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Returns true if projections of both triangles overlap on the axis.
        /// </summary>
        /// <param name="axis">Axis to test.</param>
        /// <param name="a0">Triangle 1 vertex A.</param>
        /// <param name="b0">Triangle 1 vertex B.</param>
        /// <param name="c0">Triangle 1 vertex C.</param>
        /// <param name="a1">Triangle 2 vertex A.</param>
        /// <param name="b1">Triangle 2 vertex B.</param>
        /// <param name="c1">Triangle 2 vertex C.</param>
        /// <returns>True if projections overlap.</returns>
        private static bool AxisOverlapTriangles(
            Vector3 axis,
            Vector3 a0, Vector3 b0, Vector3 c0,
            Vector3 a1, Vector3 b1, Vector3 c1)
        {
            float p0 = Vector3.Dot(a0, axis);
            float p1 = Vector3.Dot(b0, axis);
            float p2 = Vector3.Dot(c0, axis);

            float q0 = Vector3.Dot(a1, axis);
            float q1 = Vector3.Dot(b1, axis);
            float q2 = Vector3.Dot(c1, axis);

            float min0 = Mathf.Min(p0, Mathf.Min(p1, p2));
            float max0 = Mathf.Max(p0, Mathf.Max(p1, p2));

            float min1 = Mathf.Min(q0, Mathf.Min(q1, q2));
            float max1 = Mathf.Max(q0, Mathf.Max(q1, q2));

            return !(max0 < min1 || max1 < min0);
        }

        #endregion

        // ==============================================================================
        // TRIANGLE vs FRUSTUM
        // ==============================================================================
        #region TriangleVsFrustum

        /// <summary>
        /// Returns true if the triangle intersects or is inside the frustum defined by planes.
        /// </summary>
        /// <param name="a">Triangle vertex A.</param>
        /// <param name="b">Triangle vertex B.</param>
        /// <param name="c">Triangle vertex C.</param>
        /// <param name="planes">Array of frustum planes.</param>
        /// <returns>True if the triangle intersects or is inside the frustum.</returns>
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

        #endregion
    }
}
