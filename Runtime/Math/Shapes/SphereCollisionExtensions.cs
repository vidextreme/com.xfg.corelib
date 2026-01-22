// Copyright (c) 2025 Extreme Focus Games
// Licensed under the MIT License. See LICENSE for details.

using UnityEngine;

namespace XFG.Math.Shape
{
    // ------------------------------------------------------------------------------
    // SphereCollisionExtensions
    // ------------------------------------------------------------------------------
    // Collision routines for Sphere interactions.
    // Now uses unified IntersectsOrContains predicates.
    //
    // All functions are deterministic, allocation-free, and Burst-friendly.
    // ------------------------------------------------------------------------------

    public static class SphereCollisionExtensions
    {
        // ============================================================
        // SPHERE vs SPHERE
        // ============================================================
        #region SphereSphere

        /// <summary>
        /// Returns true if two spheres intersect.
        /// </summary>
        public static bool Intersects(this Sphere a, Sphere b)
        {
            return a.IntersectsOrContains(b);
        }

        #endregion


        // ============================================================
        // SPHERE vs CAPSULE
        // ============================================================
        #region SphereCapsule

        /// <summary>
        /// Returns true if a sphere intersects a capsule.
        /// </summary>
        public static bool Intersects(this Sphere s, Capsule c)
        {
            if (s.Contains(c) || c.Contains(s))
                return true;

            Vector3 closest = SegmentMath.ClosestPointOnSegment(c.P0, c.P1, s.Center);
            float r = s.Radius + c.Radius;
            return (s.Center - closest).sqrMagnitude <= r * r;
        }

        #endregion


        // ============================================================
        // SPHERE vs CYLINDER
        // ============================================================
        #region SphereCylinder

        /// <summary>
        /// Returns true if a sphere intersects a finite cylinder.
        /// </summary>
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


        // ============================================================
        // SPHERE vs LINE
        // ============================================================
        #region SphereLine

        /// <summary>
        /// Returns true if an infinite line intersects a sphere.
        /// </summary>
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


        // ============================================================
        // SPHERE vs RAY
        // ============================================================
        #region SphereRay

        /// <summary>
        /// Returns true if a ray intersects a sphere. Outputs hit t.
        /// </summary>
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


        // ============================================================
        // SPHERE vs TRIANGLE
        // ============================================================
        #region SphereTriangle

        /// <summary>
        /// Returns true if a sphere intersects a triangle.
        /// </summary>
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
