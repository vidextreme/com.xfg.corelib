// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

using UnityEngine;
using XFG.Math; // Needed for SegmentMath, TriangleMath

namespace XFG.Math.Shape
{
    /// <summary>
    /// Collision predicates for <see cref="Cone"/> against other shapes.
    /// </summary>
    /// <remarks>
    /// Supported shapes:
    /// <list type="bullet">
    /// <item>Sphere</item>
    /// <item>Capsule</item>
    /// <item>Cylinder</item>
    /// <item>Cone</item>
    /// <item>Line, Ray</item>
    /// <item>Triangle</item>
    /// </list>
    /// All methods are deterministic, allocation-free, and suitable for Burst jobs.
    /// </remarks>
    public static class ConeCollisionExtensions
    {
        // ---------------------------------------------------------------------
        // Internal Helpers
        // ---------------------------------------------------------------------
        #region Helpers

        private static Vector3 ClosestPointOnConeAxis(this Cone cone, Vector3 p)
        {
            Vector3 apex = cone.Apex;
            Vector3 axis = cone.Axis.normalized; // Axis has length Height; normalize for projections
            float height = cone.Height;

            float t = Vector3.Dot(p - apex, axis);
            t = Mathf.Clamp(t, 0f, height);

            return apex + axis * t;
        }

        private static float LocalConeRadius(this Cone cone, Vector3 axisPoint)
        {
            Vector3 apex = cone.Apex;
            Vector3 axis = cone.Axis.normalized;
            float height = cone.Height;
            float radius = cone.Radius;

            float h = Vector3.Dot(axisPoint - apex, axis);
            h = Mathf.Clamp(h, 0f, height);

            return (h / height) * radius;
        }

        #endregion

        // ---------------------------------------------------------------------
        // Cone vs Sphere
        // ---------------------------------------------------------------------
        #region ConeSphere

        /// <summary>
        /// Returns true if the cone intersects the sphere.
        /// </summary>
        /// <param name="cone">The cone.</param>
        /// <param name="s">The sphere.</param>
        /// <returns><c>true</c> if the shapes intersect.</returns>
        public static bool Intersects(this Cone cone, Sphere s)
        {
            if (cone.Contains(s) || s.Contains(cone))
                return true;

            Vector3 center = s.Center;
            float sRadius = s.Radius;

            Vector3 axisPoint = cone.ClosestPointOnConeAxis(center);
            float localRadius = cone.LocalConeRadius(axisPoint);
            float r = localRadius + sRadius;

            return (center - axisPoint).sqrMagnitude <= r * r;
        }

        #endregion

        // ---------------------------------------------------------------------
        // Cone vs Capsule
        // ---------------------------------------------------------------------
        #region ConeCapsule

        /// <summary>
        /// Returns true if the cone intersects the capsule.
        /// </summary>
        /// <param name="cone">The cone.</param>
        /// <param name="c">The capsule.</param>
        /// <returns><c>true</c> if the shapes intersect.</returns>
        public static bool Intersects(this Cone cone, Capsule c)
        {
            if (cone.Contains(c) || c.Contains(cone))
                return true;

            Vector3 p0 = c.P0;
            Vector3 p1 = c.P1;
            float cr = c.Radius;

            Vector3 p0Axis = cone.ClosestPointOnConeAxis(p0);
            Vector3 p1Axis = cone.ClosestPointOnConeAxis(p1);

            float r0 = cone.LocalConeRadius(p0Axis) + cr;
            float r1 = cone.LocalConeRadius(p1Axis) + cr;

            return (p0 - p0Axis).sqrMagnitude <= r0 * r0
                || (p1 - p1Axis).sqrMagnitude <= r1 * r1;
        }

        #endregion

        // ---------------------------------------------------------------------
        // Cone vs Cylinder
        // ---------------------------------------------------------------------
        #region ConeCylinder

        /// <summary>
        /// Returns true if the cone intersects the cylinder.
        /// </summary>
        /// <param name="cone">The cone.</param>
        /// <param name="cy">The cylinder.</param>
        /// <returns><c>true</c> if the shapes intersect.</returns>
        public static bool Intersects(this Cone cone, Cylinder cy)
        {
            if (cone.Contains(cy) || cy.Contains(cone))
                return true;

            Vector3 cP0 = cy.P0;
            Vector3 cP1 = cy.P1;
            float cr = cy.Radius;

            Vector3 p0Axis = cone.ClosestPointOnConeAxis(cP0);
            Vector3 p1Axis = cone.ClosestPointOnConeAxis(cP1);

            float r0 = cone.LocalConeRadius(p0Axis) + cr;
            float r1 = cone.LocalConeRadius(p1Axis) + cr;

            return (cP0 - p0Axis).sqrMagnitude <= r0 * r0
                || (cP1 - p1Axis).sqrMagnitude <= r1 * r1;
        }

        #endregion

        // ---------------------------------------------------------------------
        // Cone vs Cone
        // ---------------------------------------------------------------------
        #region ConeCone

        /// <summary>
        /// Returns true if the cones intersect.
        /// </summary>
        /// <param name="a">The first cone.</param>
        /// <param name="b">The second cone.</param>
        /// <returns><c>true</c> if the shapes intersect.</returns>
        public static bool Intersects(this Cone a, Cone b)
        {
            if (a.Contains(b) || b.Contains(a))
                return true;

            Vector3 aApex = a.Apex;
            Vector3 bApex = b.Apex;

            Vector3 aApexAxis = a.ClosestPointOnConeAxis(bApex);
            Vector3 bApexAxis = b.ClosestPointOnConeAxis(aApex);

            float aLocal = a.LocalConeRadius(aApexAxis);
            float bLocal = b.LocalConeRadius(bApexAxis);

            bool aHit = (bApex - aApexAxis).sqrMagnitude <= aLocal * aLocal;
            bool bHit = (aApex - bApexAxis).sqrMagnitude <= bLocal * bLocal;

            return aHit || bHit;
        }

        #endregion

        // ---------------------------------------------------------------------
        // Cone vs Line
        // ---------------------------------------------------------------------
        #region ConeLine

        /// <summary>
        /// Returns true if the infinite line intersects the cone.
        /// </summary>
        /// <param name="cone">The cone.</param>
        /// <param name="L0">A point on the line.</param>
        /// <param name="Ld">The line direction.</param>
        /// <returns><c>true</c> if the line intersects the cone.</returns>
        public static bool Intersects(this Cone cone, Vector3 L0, Vector3 Ld)
        {
            Vector3 apex = cone.Apex;

            // Closest point on infinite line defined by (L0, L0 + Ld)
            Vector3 closest = SegmentMath.ClosestPointOnLineToPoint(L0, L0 + Ld, apex);
            Vector3 axisPoint = cone.ClosestPointOnConeAxis(closest);

            float localRadius = cone.LocalConeRadius(axisPoint);
            return (closest - axisPoint).sqrMagnitude <= localRadius * localRadius;
        }

        #endregion

        // ---------------------------------------------------------------------
        // Cone vs Ray
        // ---------------------------------------------------------------------
        #region ConeRay

        /// <summary>
        /// Returns true if the ray intersects the cone.
        /// </summary>
        /// <param name="cone">The cone.</param>
        /// <param name="R0">The ray origin.</param>
        /// <param name="Rd">The ray direction (normalized).</param>
        /// <param name="t">The distance along the ray to the closest approach, if hit.</param>
        /// <returns><c>true</c> if the ray hits the cone in front of its origin.</returns>
        public static bool IntersectsRay(this Cone cone, Vector3 R0, Vector3 Rd, out float t)
        {
            t = 0f;

            Vector3 apex = cone.Apex;

            Vector3 closest = SegmentMath.ClosestPointOnRayToPoint(R0, Rd, apex);
            Vector3 axisPoint = cone.ClosestPointOnConeAxis(closest);

            float localRadius = cone.LocalConeRadius(axisPoint);

            if ((closest - axisPoint).sqrMagnitude > localRadius * localRadius)
                return false;

            t = Vector3.Dot(closest - R0, Rd);
            return t >= 0f;
        }

        #endregion

        // ---------------------------------------------------------------------
        // Cone vs Triangle
        // ---------------------------------------------------------------------
        #region ConeTriangle

        /// <summary>
        /// Returns true if the cone intersects the triangle.
        /// </summary>
        /// <param name="cone">The cone.</param>
        /// <param name="a">Triangle vertex A.</param>
        /// <param name="b">Triangle vertex B.</param>
        /// <param name="c">Triangle vertex C.</param>
        /// <returns><c>true</c> if the shapes intersect.</returns>
        public static bool IntersectsTriangle(this Cone cone, Vector3 a, Vector3 b, Vector3 c)
        {
            Vector3 apex = cone.Apex;

            Vector3 closest = TriangleMath.ClosestPointOnTriangle(apex, a, b, c);
            Vector3 axisPoint = cone.ClosestPointOnConeAxis(closest);

            float localRadius = cone.LocalConeRadius(axisPoint);
            return (closest - axisPoint).sqrMagnitude <= localRadius * localRadius;
        }

        #endregion
    }
}
