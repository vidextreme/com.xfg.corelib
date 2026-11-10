// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

using UnityEngine;
using XFG.Math; // Needed for SegmentMath, TriangleMath

namespace XFG.Math.Shape
{
    /// <summary>
    /// Collision predicates for <see cref="Capsule"/> against other shapes.
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
    public static class CapsuleCollisionExtensions
    {
        // ---------------------------------------------------------------------
        // Capsule vs Sphere
        // ---------------------------------------------------------------------
        #region CapsuleSphere

        /// <summary>
        /// Returns true if the capsule intersects the sphere.
        /// </summary>
        /// <param name="c">The capsule.</param>
        /// <param name="s">The sphere.</param>
        /// <returns><c>true</c> if the shapes intersect.</returns>
        public static bool Intersects(this Capsule c, Sphere s)
        {
            return s.Intersects(c);
        }

        #endregion

        // ---------------------------------------------------------------------
        // Capsule vs Capsule
        // ---------------------------------------------------------------------
        #region CapsuleCapsule

        /// <summary>
        /// Returns true if the capsules intersect.
        /// </summary>
        /// <param name="a">The first capsule.</param>
        /// <param name="b">The second capsule.</param>
        /// <returns><c>true</c> if the shapes intersect.</returns>
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

        // ---------------------------------------------------------------------
        // Capsule vs Cylinder
        // ---------------------------------------------------------------------
        #region CapsuleCylinder

        /// <summary>
        /// Returns true if the capsule intersects the cylinder.
        /// </summary>
        /// <param name="c">The capsule.</param>
        /// <param name="cy">The cylinder.</param>
        /// <returns><c>true</c> if the shapes intersect.</returns>
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

        // ---------------------------------------------------------------------
        // Capsule vs Line
        // ---------------------------------------------------------------------
        #region CapsuleLine

        /// <summary>
        /// Returns true if the infinite line intersects the capsule.
        /// </summary>
        /// <param name="c">The capsule.</param>
        /// <param name="L0">A point on the line.</param>
        /// <param name="Ld">The line direction.</param>
        /// <returns><c>true</c> if the line intersects the capsule.</returns>
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

        // ---------------------------------------------------------------------
        // Capsule vs Ray
        // ---------------------------------------------------------------------
        #region CapsuleRay

        /// <summary>
        /// Returns true if the ray intersects the capsule.
        /// </summary>
        /// <param name="c">The capsule.</param>
        /// <param name="R0">The ray origin.</param>
        /// <param name="Rd">The ray direction (normalized).</param>
        /// <param name="t">The distance along the ray to the hit, if any.</param>
        /// <returns><c>true</c> if the ray hits the capsule in front of its origin.</returns>
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

        // ---------------------------------------------------------------------
        // Capsule vs Triangle
        // ---------------------------------------------------------------------
        #region CapsuleTriangle

        /// <summary>
        /// Returns true if the capsule intersects the triangle.
        /// </summary>
        /// <param name="c">The capsule.</param>
        /// <param name="a">Triangle vertex A.</param>
        /// <param name="b">Triangle vertex B.</param>
        /// <param name="d">Triangle vertex C.</param>
        /// <returns><c>true</c> if the shapes intersect.</returns>
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
