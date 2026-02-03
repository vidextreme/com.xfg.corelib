// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.
// ------------------------------------------------------------------------------
// CapsuleCollisionExtensions
// ------------------------------------------------------------------------------
// Collision predicates for Capsule vs:
// - Sphere
// - Capsule
// - Cylinder
// - Line, Ray
// - Triangle
//
// All methods are deterministic, allocation-free, and suitable for Burst jobs.
// ------------------------------------------------------------------------------

using UnityEngine;
using XFG.Math; // Needed for SegmentMath, TriangleMath

namespace XFG.Math.Shape
{
    public static class CapsuleCollisionExtensions
    {
        // ==============================================================================
        // CAPSULE vs SPHERE
        // ==============================================================================
        #region CapsuleSphere

        /// <summary>
        /// Returns true if the capsule intersects the sphere.
        /// </summary>
        public static bool Intersects(this Capsule c, Sphere s)
        {
            return s.Intersects(c);
        }

        #endregion

        // ==============================================================================
        // CAPSULE vs CAPSULE
        // ==============================================================================
        #region CapsuleCapsule

        /// <summary>
        /// Returns true if the capsules intersect.
        /// </summary>
        public static bool Intersects(this Capsule a, Capsule b)
        {
            if (a.Contains(b) || b.Contains(a))
                return true;

            Vector3 aP0 = a.P0;
            Vector3 aP1 = a.P1;
            Vector3 bP0 = b.P0;
            Vector3 bP1 = b.P1;

            float distSq = SegmentMath.SegmentSegmentDistanceSq(aP0, aP1, bP0, bP1);
            float r = a.Radius + b.Radius;

            return distSq <= r * r;
        }

        #endregion

        // ==============================================================================
        // CAPSULE vs CYLINDER
        // ==============================================================================
        #region CapsuleCylinder

        /// <summary>
        /// Returns true if the capsule intersects the cylinder.
        /// </summary>
        public static bool Intersects(this Capsule c, Cylinder cy)
        {
            if (c.Contains(cy) || cy.Contains(c))
                return true;

            Vector3 cP0 = c.P0;
            Vector3 cP1 = c.P1;
            Vector3 cyP0 = cy.P0;
            Vector3 cyP1 = cy.P1;

            float distSq = SegmentMath.SegmentSegmentDistanceSq(cP0, cP1, cyP0, cyP1);
            float r = c.Radius + cy.Radius;

            return distSq <= r * r;
        }

        #endregion

        // ==============================================================================
        // CAPSULE vs LINE
        // ==============================================================================
        #region CapsuleLine

        /// <summary>
        /// Returns true if the infinite line intersects the capsule.
        /// </summary>
        public static bool Intersects(this Capsule c, Vector3 L0, Vector3 Ld)
        {
            Vector3 p0 = c.P0;
            Vector3 p1 = c.P1;
            float radius = c.Radius;

            // Closest point on segment to infinite line
            Vector3 closest = SegmentMath.ClosestPointOnSegmentToLine(p0, p1, L0, L0 + Ld);

            float distSq = (closest - L0).sqrMagnitude;
            return distSq <= radius * radius;
        }

        #endregion

        // ==============================================================================
        // CAPSULE vs RAY
        // ==============================================================================
        #region CapsuleRay

        /// <summary>
        /// Returns true if the ray intersects the capsule.
        /// </summary>
        public static bool IntersectsRay(this Capsule c, Vector3 R0, Vector3 Rd, out float t)
        {
            t = 0f;

            Vector3 p0 = c.P0;
            Vector3 p1 = c.P1;
            float radius = c.Radius;

            // Closest point on segment to ray
            Vector3 closest = SegmentMath.ClosestPointOnSegmentToRay(p0, p1, new Ray(R0, Rd));
            Vector3 diff = closest - R0;

            float proj = Vector3.Dot(diff, Rd);
            if (proj < 0f)
                return false;

            float distSq = diff.sqrMagnitude - proj * proj;
            if (distSq > radius * radius)
                return false;

            t = proj - Mathf.Sqrt(radius * radius - distSq);
            return t >= 0f;
        }

        #endregion

        // ==============================================================================
        // CAPSULE vs TRIANGLE
        // ==============================================================================
        #region CapsuleTriangle

        /// <summary>
        /// Returns true if the capsule intersects the triangle.
        /// </summary>
        public static bool IntersectsTriangle(this Capsule c, Vector3 a, Vector3 b, Vector3 d)
        {
            Vector3 p0 = c.P0;
            Vector3 p1 = c.P1;
            float radius = c.Radius;
            float rSq = radius * radius;

            // Closest point on triangle to segment
            Vector3 closest = TriangleMath.ClosestPointOnTriangleToSegment(a, b, d, p0, p1);

            return (closest - p0).sqrMagnitude <= rSq
                || (closest - p1).sqrMagnitude <= rSq;
        }

        #endregion
    }
}
