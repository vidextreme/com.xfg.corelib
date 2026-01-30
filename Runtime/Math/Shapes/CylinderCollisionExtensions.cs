// Copyright (c) 2025 John David Uy
// Licensed under the MIT License. See LICENSE for details.
// ------------------------------------------------------------------------------
// CylinderCollisionExtensions
// ------------------------------------------------------------------------------
// Collision predicates for Cylinder vs:
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
    public static class CylinderCollisionExtensions
    {
        // ==============================================================================
        // CYLINDER vs SPHERE
        // ==============================================================================
        #region CylinderSphere

        /// <summary>
        /// Returns true if the cylinder intersects the sphere.
        /// </summary>
        public static bool Intersects(this Cylinder cy, Sphere s)
        {
            return s.Intersects(cy);
        }

        #endregion

        // ==============================================================================
        // CYLINDER vs CAPSULE
        // ==============================================================================
        #region CylinderCapsule

        /// <summary>
        /// Returns true if the cylinder intersects the capsule.
        /// </summary>
        public static bool Intersects(this Cylinder cy, Capsule c)
        {
            return c.Intersects(cy);
        }

        #endregion

        // ==============================================================================
        // CYLINDER vs CYLINDER
        // ==============================================================================
        #region CylinderCylinder

        /// <summary>
        /// Returns true if the cylinders intersect.
        /// </summary>
        public static bool Intersects(this Cylinder a, Cylinder b)
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
        // CYLINDER vs LINE
        // ==============================================================================
        #region CylinderLine

        /// <summary>
        /// Returns true if the infinite line intersects the cylinder.
        /// </summary>
        public static bool Intersects(this Cylinder cy, Vector3 L0, Vector3 Ld)
        {
            Vector3 p0 = cy.P0;
            Vector3 p1 = cy.P1;
            float radius = cy.Radius;

            // Closest point on segment to infinite line
            Vector3 closest = SegmentMath.ClosestPointOnSegmentToLine(p0, p1, L0, L0 + Ld);

            // Distance from line to segment
            float distSq = (closest - L0).sqrMagnitude;

            return distSq <= radius * radius;
        }

        #endregion

        // ==============================================================================
        // CYLINDER vs RAY
        // ==============================================================================
        #region CylinderRay

        /// <summary>
        /// Returns true if the ray intersects the cylinder.
        /// </summary>
        public static bool IntersectsRay(this Cylinder cy, Vector3 R0, Vector3 Rd, out float t)
        {
            t = 0f;

            Vector3 p0 = cy.P0;
            Vector3 p1 = cy.P1;
            float radius = cy.Radius;

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
        // CYLINDER vs TRIANGLE
        // ==============================================================================
        #region CylinderTriangle

        /// <summary>
        /// Returns true if the cylinder intersects the triangle.
        /// </summary>
        public static bool IntersectsTriangle(this Cylinder cy, Vector3 a, Vector3 b, Vector3 c)
        {
            Vector3 p0 = cy.P0;
            Vector3 p1 = cy.P1;
            float radius = cy.Radius;
            float rSq = radius * radius;

            // Closest point on triangle to segment
            Vector3 closest = TriangleMath.ClosestPointOnTriangleToSegment(a, b, c, p0, p1);

            // Check against both endpoints
            return (closest - p0).sqrMagnitude <= rSq
                || (closest - p1).sqrMagnitude <= rSq;
        }

        #endregion
    }
}
