// Copyright (c) 2025 Extreme Focus Games
// Licensed under the MIT License. See LICENSE for details.

using UnityEngine;

namespace XFG.Math.Shape
{
    // ------------------------------------------------------------------------------
    // CylinderCollisionExtensions
    // ------------------------------------------------------------------------------
    // Collision routines for Cylinder interactions.
    //
    // Provides:
    // - Cylinder vs Cylinder
    // - Cylinder vs Sphere
    // - Cylinder vs Capsule
    // - Cylinder vs Line
    // - Cylinder vs Ray
    // - Cylinder vs Triangle
    //
    // All functions are deterministic, allocation-free, and Burst-friendly.
    // ------------------------------------------------------------------------------

    public static class CylinderCollisionExtensions
    {
        // ============================================================
        // CYLINDER vs CYLINDER
        // ============================================================
        #region CylinderCylinder

        /// <summary>
        /// Returns true if two finite cylinders intersect.
        /// </summary>
        public static bool Intersects(this Cylinder a, Cylinder b)
        {
            if (a.Contains(b) || b.Contains(a))
                return true;

            float distSq = SegmentMath.SegmentSegmentDistanceSq(
                a.P0, a.P1,
                b.P0, b.P1
            );

            float r = a.Radius + b.Radius;
            return distSq <= r * r;
        }

        #endregion


        // ============================================================
        // CYLINDER vs SPHERE
        // ============================================================
        #region CylinderSphere

        /// <summary>
        /// Returns true if a cylinder intersects a sphere.
        /// </summary>
        public static bool Intersects(this Cylinder cy, Sphere s)
        {
            if (cy.Contains(s) || s.Contains(cy))
                return true;

            Vector3 axis = cy.P1 - cy.P0;
            float axisLenSq = Vector3.Dot(axis, axis);

            float t = Vector3.Dot(s.Center - cy.P0, axis) / axisLenSq;
            t = Mathf.Clamp01(t);

            Vector3 closest = cy.P0 + axis * t;

            float r = cy.Radius + s.Radius;
            return (s.Center - closest).sqrMagnitude <= r * r;
        }

        #endregion


        // ============================================================
        // CYLINDER vs CAPSULE
        // ============================================================
        #region CylinderCapsule

        /// <summary>
        /// Returns true if a cylinder intersects a capsule.
        /// </summary>
        public static bool Intersects(this Cylinder cy, Capsule c)
        {
            if (cy.Contains(c) || c.Contains(cy))
                return true;

            float distSq = SegmentMath.SegmentSegmentDistanceSq(
                cy.P0, cy.P1,
                c.P0, c.P1
            );

            float r = cy.Radius + c.Radius;
            return distSq <= r * r;
        }

        #endregion


        // ============================================================
        // CYLINDER vs LINE
        // ============================================================
        #region CylinderLine

        /// <summary>
        /// Returns true if an infinite line intersects a finite cylinder.
        /// </summary>
        public static bool Intersects(this Cylinder cy, Vector3 L0, Vector3 Ld)
        {
            Vector3 L1 = L0 + Ld * 1e6f;

            float distSq = SegmentMath.SegmentSegmentDistanceSq(
                L0, L1,
                cy.P0, cy.P1
            );

            return distSq <= cy.Radius * cy.Radius;
        }

        #endregion


        // ============================================================
        // CYLINDER vs RAY
        // ============================================================
        #region CylinderRay

        /// <summary>
        /// Returns true if a ray intersects a finite cylinder. Outputs approximate t.
        /// </summary>
        public static bool IntersectsRay(this Cylinder cy, Vector3 R0, Vector3 Rd, out float t)
        {
            t = 0f;

            if (cy.Contains(R0))
            {
                t = 0f;
                return true;
            }

            Vector3 R1 = R0 + Rd * 1e6f;

            float distSq = SegmentMath.SegmentSegmentDistanceSq(
                R0, R1,
                cy.P0, cy.P1
            );

            if (distSq > cy.Radius * cy.Radius)
                return false;

            Vector3 mid = (cy.P0 + cy.P1) * 0.5f;
            Vector3 toMid = mid - R0;

            float tProj = Vector3.Dot(toMid, Rd) / Vector3.Dot(Rd, Rd);
            if (tProj < 0f)
                return false;

            t = tProj;
            return true;
        }

        #endregion


        // ============================================================
        // CYLINDER vs TRIANGLE
        // ============================================================
        #region CylinderTriangle

        /// <summary>
        /// Returns true if a finite cylinder intersects a triangle.
        /// </summary>
        public static bool IntersectsTriangle(this Cylinder cy, Vector3 a, Vector3 b, Vector3 cTri)
        {
            if (cy.Contains(a) || cy.Contains(b) || cy.Contains(cTri))
                return true;

            Vector3 axis = cy.P1 - cy.P0;
            float axisLenSq = Vector3.Dot(axis, axis);

            Vector3 triClosest = TriangleMath.ClosestPointOnTriangle(cy.P0, a, b, cTri);

            float t = Vector3.Dot(triClosest - cy.P0, axis) / axisLenSq;
            t = Mathf.Clamp01(t);

            Vector3 axisPoint = cy.P0 + axis * t;

            float distSq = (triClosest - axisPoint).sqrMagnitude;
            return distSq <= cy.Radius * cy.Radius;
        }

        #endregion
    }
}
