// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

using UnityEngine;

namespace XFG.Math.Shape
{
    /// <summary>
    /// Collision predicates for <see cref="Sphere"/> against other shapes.
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
    public static class SphereCollisionExtensions
    {
        // ---------------------------------------------------------------------
        // Sphere vs Sphere
        // ---------------------------------------------------------------------
        #region SphereSphere

        /// <summary>
        /// Returns true if the spheres intersect or one contains the other.
        /// </summary>
        /// <param name="a">The first sphere.</param>
        /// <param name="b">The second sphere.</param>
        /// <returns><c>true</c> if the shapes intersect.</returns>
        public static bool Intersects(this Sphere a, Sphere b)
        {
            return a.IntersectsOrContains(b);
        }

        #endregion

        // ---------------------------------------------------------------------
        // Sphere vs Capsule
        // ---------------------------------------------------------------------
        #region SphereCapsule

        /// <summary>
        /// Returns true if the sphere intersects the capsule.
        /// </summary>
        /// <param name="s">The sphere.</param>
        /// <param name="c">The capsule.</param>
        /// <returns><c>true</c> if the shapes intersect.</returns>
        public static bool Intersects(this Sphere s, Capsule c)
        {
            if (s.Contains(c) || c.Contains(s))
                return true;

            Vector3 closest = SegmentMath.ClosestPointOnSegment(c.P0, c.P1, s.Center);
            float r = s.Radius + c.Radius;
            return (s.Center - closest).sqrMagnitude <= r * r;
        }

        #endregion

        // ---------------------------------------------------------------------
        // Sphere vs Cylinder
        // ---------------------------------------------------------------------
        #region SphereCylinder

        /// <summary>
        /// Returns true if the sphere intersects the cylinder.
        /// </summary>
        /// <param name="s">The sphere.</param>
        /// <param name="cy">The cylinder.</param>
        /// <returns><c>true</c> if the shapes intersect.</returns>
        public static bool Intersects(this Sphere s, Cylinder cy)
        {
            if (s.Contains(cy) || cy.Contains(s))
                return true;

            Vector3 axis = cy.P1 - cy.P0;
            float axisLenSq = Vector3.Dot(axis, axis);

            float t = Vector3.Dot(s.Center - cy.P0, axis) / axisLenSq;
            t = Mathf.Clamp01(t);

            Vector3 closest = cy.P0 + axis * t;

            float r = s.Radius + cy.Radius;
            return (s.Center - closest).sqrMagnitude <= r * r;
        }

        #endregion

        // ---------------------------------------------------------------------
        // Sphere vs Line
        // ---------------------------------------------------------------------
        #region SphereLine

        /// <summary>
        /// Returns true if the infinite line intersects the sphere.
        /// </summary>
        /// <param name="s">The sphere.</param>
        /// <param name="L0">A point on the line.</param>
        /// <param name="Ld">The line direction (normalized).</param>
        /// <returns><c>true</c> if the line intersects the sphere.</returns>
        public static bool Intersects(this Sphere s, Vector3 L0, Vector3 Ld)
        {
            if (s.Contains(L0))
                return true;

            Vector3 m = L0 - s.Center;

            float b = Vector3.Dot(m, Ld);
            float c = Vector3.Dot(m, m) - s.Radius * s.Radius;

            if (c <= 0f)
                return true;

            if (b > 0f)
                return false;

            float discr = b * b - c;
            return discr >= 0f;
        }

        #endregion

        // ---------------------------------------------------------------------
        // Sphere vs Ray
        // ---------------------------------------------------------------------
        #region SphereRay

        /// <summary>
        /// Returns true if the ray intersects the sphere. Outputs the hit distance.
        /// </summary>
        /// <param name="s">The sphere.</param>
        /// <param name="R0">The ray origin.</param>
        /// <param name="Rd">The ray direction (normalized).</param>
        /// <param name="t">The distance along the ray to the hit; 0 if the origin is inside.</param>
        /// <returns><c>true</c> if the ray hits the sphere.</returns>
        public static bool IntersectsRay(this Sphere s, Vector3 R0, Vector3 Rd, out float t)
        {
            t = 0f;

            if (s.Contains(R0))
                return true;

            Vector3 m = R0 - s.Center;
            float b = Vector3.Dot(m, Rd);
            float c = Vector3.Dot(m, m) - s.Radius * s.Radius;

            if (c > 0f && b > 0f)
                return false;

            float discr = b * b - c;
            if (discr < 0f)
                return false;

            float tHit = -b - Mathf.Sqrt(discr);
            if (tHit < 0f)
                tHit = -b + Mathf.Sqrt(discr);

            if (tHit < 0f)
                return false;

            t = tHit;
            return true;
        }

        #endregion

        // ---------------------------------------------------------------------
        // Sphere vs Triangle
        // ---------------------------------------------------------------------
        #region SphereTriangle

        /// <summary>
        /// Returns true if the sphere intersects the triangle.
        /// </summary>
        /// <param name="s">The sphere.</param>
        /// <param name="a">Triangle vertex A.</param>
        /// <param name="b">Triangle vertex B.</param>
        /// <param name="c">Triangle vertex C.</param>
        /// <returns><c>true</c> if the shapes intersect.</returns>
        public static bool IntersectsTriangle(this Sphere s, Vector3 a, Vector3 b, Vector3 c)
        {
            if (s.Contains(a) || s.Contains(b) || s.Contains(c))
                return true;

            Vector3 closest = TriangleMath.ClosestPointOnTriangle(s.Center, a, b, c);
            return (closest - s.Center).sqrMagnitude <= s.Radius * s.Radius;
        }

        #endregion
    }
}
