// Copyright (c) 2025 Extreme Focus Games
// Licensed under the MIT License. See LICENSE for details.

using UnityEngine;

namespace XFG.Math.Shape
{
    // ------------------------------------------------------------------------------
    // CapsuleCollisionExtensions
    // ------------------------------------------------------------------------------
    // Collision routines for Capsule interactions.
    //
    // Provides:
    // - Capsule vs Capsule
    // - Capsule vs Sphere
    // - Capsule vs Cylinder
    // - Capsule vs Line
    // - Capsule vs Ray
    // - Capsule vs Triangle
    //
    // All functions are deterministic, allocation-free, and Burst-friendly.
    // ------------------------------------------------------------------------------

    public static class CapsuleCollisionExtensions
    {
        // ============================================================
        // CAPSULE vs CAPSULE
        // ============================================================
        #region CapsuleCapsule

        /// <summary>
        /// Returns true if two capsules intersect.
        /// </summary>
        public static bool Intersects(this Capsule a, Capsule b)
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
        // CAPSULE vs SPHERE
        // ============================================================
        #region CapsuleSphere

        /// <summary>
        /// Returns true if a capsule intersects a sphere.
        /// </summary>
        public static bool Intersects(this Capsule c, Sphere s)
        {
            if (c.Contains(s) || s.Contains(c))
                return true;

            Vector3 closest = SegmentMath.ClosestPointOnSegment(c.P0, c.P1, s.Center);
            float r = c.Radius + s.Radius;
            return (s.Center - closest).sqrMagnitude <= r * r;
        }

        #endregion


        // ============================================================
        // CAPSULE vs CYLINDER
        // ============================================================
        #region CapsuleCylinder

        /// <summary>
        /// Returns true if a capsule intersects a finite cylinder.
        /// </summary>
        public static bool Intersects(this Capsule c, Cylinder cy)
        {
            if (c.Contains(cy) || cy.Contains(c))
                return true;

            float distSq = SegmentMath.SegmentSegmentDistanceSq(
                c.P0, c.P1,
                cy.P0, cy.P1
            );

            float r = c.Radius + cy.Radius;
            return distSq <= r * r;
        }

        #endregion


        // ============================================================
        // CAPSULE vs LINE
        // ============================================================
        #region CapsuleLine

        /// <summary>
        /// Returns true if an infinite line intersects a capsule.
        /// </summary>
        public static bool Intersects(this Capsule c, Vector3 L0, Vector3 Ld)
        {
            Vector3 L1 = L0 + Ld * 1e6f;

            float distSq = SegmentMath.SegmentSegmentDistanceSq(
                L0, L1,
                c.P0, c.P1
            );

            return distSq <= c.Radius * c.Radius;
        }

        #endregion


        // ============================================================
        // CAPSULE vs RAY
        // ============================================================
        #region CapsuleRay

        /// <summary>
        /// Returns true if a ray intersects a capsule. Outputs approximate t.
        /// </summary>
        public static bool IntersectsRay(this Capsule c, Vector3 R0, Vector3 Rd, out float t)
        {
            t = 0f;

            if (c.Contains(R0))
            {
                t = 0f;
                return true;
            }

            Vector3 R1 = R0 + Rd * 1e6f;

            float distSq = SegmentMath.SegmentSegmentDistanceSq(
                R0, R1,
                c.P0, c.P1
            );

            if (distSq > c.Radius * c.Radius)
                return false;

            Vector3 mid = (c.P0 + c.P1) * 0.5f;
            Vector3 toMid = mid - R0;

            float tProj = Vector3.Dot(toMid, Rd) / Vector3.Dot(Rd, Rd);
            if (tProj < 0f)
                return false;

            t = tProj;
            return true;
        }

        #endregion


        // ============================================================
        // CAPSULE vs TRIANGLE
        // ============================================================
        #region CapsuleTriangle

        /// <summary>
        /// Returns true if a capsule intersects a triangle.
        /// Uses closest-point sampling along the capsule segment.
        /// </summary>
        public static bool IntersectsTriangle(this Capsule c, Vector3 a, Vector3 b, Vector3 cTri)
        {
            if (c.Contains(a) || c.Contains(b) || c.Contains(cTri))
                return true;

            Vector3 p0 = TriangleMath.ClosestPointOnTriangle(c.P0, a, b, cTri);
            if ((p0 - c.P0).sqrMagnitude <= c.Radius * c.Radius)
                return true;

            Vector3 p1 = TriangleMath.ClosestPointOnTriangle(c.P1, a, b, cTri);
            if ((p1 - c.P1).sqrMagnitude <= c.Radius * c.Radius)
                return true;

            Vector3 mid = (c.P0 + c.P1) * 0.5f;
            Vector3 pm = TriangleMath.ClosestPointOnTriangle(mid, a, b, cTri);
            if ((pm - mid).sqrMagnitude <= c.Radius * c.Radius)
                return true;

            return false;
        }

        #endregion
    }
}
