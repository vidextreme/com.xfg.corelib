// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

using UnityEngine;
using XFG.Math; // Needed for SegmentMath, TriangleMath

namespace XFG.Math.Shape
{
    /// <summary>
    /// Collision predicates for <see cref="Cylinder"/> against other shapes.
    /// </summary>
    /// <remarks>
    /// Supported shapes:
    /// <list type="bullet">
    /// <item>Sphere</item>
    /// <item>Capsule</item>
    /// <item>Cylinder</item>
    /// <item>Line, Ray</item>
    /// <item>Triangle</item>
    /// </list>
    /// All methods are deterministic, allocation-free, and suitable for Burst jobs.
    /// </remarks>
    public static class CylinderCollisionExtensions
    {
        // ---------------------------------------------------------------------
        // Cylinder vs Sphere
        // ---------------------------------------------------------------------
        #region CylinderSphere

        /// <summary>
        /// Returns true if the cylinder intersects the sphere.
        /// </summary>
        /// <param name="cy">The cylinder.</param>
        /// <param name="s">The sphere.</param>
        /// <returns><c>true</c> if the shapes intersect.</returns>
        public static bool Intersects(this Cylinder cy, Sphere s)
        {
            return s.Intersects(cy);
        }

        #endregion

        // ---------------------------------------------------------------------
        // Cylinder vs Capsule
        // ---------------------------------------------------------------------
        #region CylinderCapsule

        /// <summary>
        /// Returns true if the cylinder intersects the capsule.
        /// </summary>
        /// <param name="cy">The cylinder.</param>
        /// <param name="c">The capsule.</param>
        /// <returns><c>true</c> if the shapes intersect.</returns>
        public static bool Intersects(this Cylinder cy, Capsule c)
        {
            return c.Intersects(cy);
        }

        #endregion

        // ---------------------------------------------------------------------
        // Cylinder vs Cylinder
        // ---------------------------------------------------------------------
        #region CylinderCylinder

        /// <summary>
        /// Returns true if the cylinders intersect.
        /// </summary>
        /// <param name="a">The first cylinder.</param>
        /// <param name="b">The second cylinder.</param>
        /// <returns><c>true</c> if the shapes intersect.</returns>
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

        // ---------------------------------------------------------------------
        // Cylinder vs Line
        // ---------------------------------------------------------------------
        #region CylinderLine

        /// <summary>
        /// Returns true if the infinite line intersects the cylinder.
        /// </summary>
        /// <param name="cy">The cylinder.</param>
        /// <param name="L0">A point on the line.</param>
        /// <param name="Ld">The line direction.</param>
        /// <returns><c>true</c> if the line intersects the cylinder.</returns>
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

        // ---------------------------------------------------------------------
        // Cylinder vs Ray
        // ---------------------------------------------------------------------
        #region CylinderRay

        /// <summary>
        /// Returns true if the ray intersects the cylinder.
        /// </summary>
        /// <param name="cy">The cylinder.</param>
        /// <param name="R0">The ray origin.</param>
        /// <param name="Rd">The ray direction (normalized).</param>
        /// <param name="t">The distance along the ray to the hit, if any.</param>
        /// <returns><c>true</c> if the ray hits the cylinder in front of its origin.</returns>
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

        // ---------------------------------------------------------------------
        // Cylinder vs Triangle
        // ---------------------------------------------------------------------
        #region CylinderTriangle

        /// <summary>
        /// Returns true if the cylinder intersects the triangle.
        /// </summary>
        /// <param name="cy">The cylinder.</param>
        /// <param name="a">Triangle vertex A.</param>
        /// <param name="b">Triangle vertex B.</param>
        /// <param name="c">Triangle vertex C.</param>
        /// <returns><c>true</c> if the shapes intersect.</returns>
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
