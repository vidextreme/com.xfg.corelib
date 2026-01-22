// Copyright (c) 2025 Extreme Focus Games
// Licensed under the MIT License. See LICENSE for details.

using UnityEngine;

namespace XFG.Math.Shape
{
    // ------------------------------------------------------------------------------
    // ConeCollisionExtensions
    // ------------------------------------------------------------------------------
    // Collision routines for Cone interactions.
    //
    // Provides:
    // - Cone vs Sphere
    // - Cone vs Capsule
    // - Cone vs Cylinder
    // - Cone vs Line
    // - Cone vs Ray
    // - Cone vs Triangle
    //
    // All functions are deterministic, allocation-free, and Burst-friendly.
    // ------------------------------------------------------------------------------

    public static class ConeCollisionExtensions
    {
        // ============================================================
        // INTERNAL HELPERS
        // ============================================================
        #region Helpers

        private static Vector3 ClosestPointOnConeAxis(Cone cone, Vector3 p)
        {
            Vector3 dir = cone.Axis.normalized;
            float t = Vector3.Dot(p - cone.Apex, dir);
            t = Mathf.Clamp01(t);
            return cone.Apex + dir * t;
        }

        private static float LocalConeRadius(Cone cone, Vector3 axisPoint)
        {
            float t = Vector3.Dot(axisPoint - cone.Apex, cone.Axis.normalized);
            t = Mathf.Clamp01(t);
            return (t / cone.Height) * cone.Radius;
        }

        #endregion


        // ============================================================
        // CONE vs SPHERE
        // ============================================================
        #region ConeSphere

        /// <summary>
        /// Returns true if a cone intersects a sphere.
        /// </summary>
        public static bool Intersects(this Cone cone, Sphere s)
        {
            if (cone.Contains(s) || s.Contains(cone))
                return true;

            Vector3 axisPoint = ClosestPointOnConeAxis(cone, s.Center);
            float localRadius = LocalConeRadius(cone, axisPoint);

            float distSq = (s.Center - axisPoint).sqrMagnitude;
            float r = localRadius + s.Radius;

            return distSq <= r * r;
        }

        #endregion


        // ============================================================
        // CONE vs CAPSULE
        // ============================================================
        #region ConeCapsule

        /// <summary>
        /// Returns true if a cone intersects a capsule.
        /// </summary>
        public static bool Intersects(this Cone cone, Capsule c)
        {
            if (cone.Contains(c) || c.Contains(cone))
                return true;

            Vector3 p0 = ClosestPointOnConeAxis(cone, c.P0);
            Vector3 p1 = ClosestPointOnConeAxis(cone, c.P1);
            Vector3 mid = ClosestPointOnConeAxis(cone, (c.P0 + c.P1) * 0.5f);

            float r0 = c.Radius + LocalConeRadius(cone, p0);
            float r1 = c.Radius + LocalConeRadius(cone, p1);
            float rm = c.Radius + LocalConeRadius(cone, mid);

            if ((c.P0 - p0).sqrMagnitude <= r0 * r0) return true;
            if ((c.P1 - p1).sqrMagnitude <= r1 * r1) return true;
            if (((c.P0 + c.P1) * 0.5f - mid).sqrMagnitude <= rm * rm) return true;

            return false;
        }

        #endregion


        // ============================================================
        // CONE vs CYLINDER
        // ============================================================
        #region ConeCylinder

        /// <summary>
        /// Returns true if a cone intersects a finite cylinder.
        /// </summary>
        public static bool Intersects(this Cone cone, Cylinder cy)
        {
            if (cone.Contains(cy) || cy.Contains(cone))
                return true;

            Vector3 mid = (cy.P0 + cy.P1) * 0.5f;
            Vector3 axisPoint = ClosestPointOnConeAxis(cone, mid);

            float localRadius = LocalConeRadius(cone, axisPoint);
            float r = localRadius + cy.Radius;

            return (mid - axisPoint).sqrMagnitude <= r * r;
        }

        #endregion


        // ============================================================
        // CONE vs LINE
        // ============================================================
        #region ConeLine

        /// <summary>
        /// Returns true if an infinite line intersects a cone.
        /// Approximate test using closest point on axis.
        /// </summary>
        public static bool Intersects(this Cone cone, Vector3 L0, Vector3 Ld)
        {
            Vector3 axisPoint = ClosestPointOnConeAxis(cone, L0);
            float localRadius = LocalConeRadius(cone, axisPoint);

            float distSq = LineMath.LineSegmentDistanceSq(
                L0, Ld,
                axisPoint, axisPoint
            );

            return distSq <= localRadius * localRadius;
        }

        #endregion


        // ============================================================
        // CONE vs RAY
        // ============================================================
        #region ConeRay

        /// <summary>
        /// Returns true if a ray intersects a cone. Approximate test.
        /// </summary>
        public static bool IntersectsRay(this Cone cone, Vector3 R0, Vector3 Rd, out float t)
        {
            t = 0f;

            if (cone.Contains(R0))
            {
                t = 0f;
                return true;
            }

            Vector3 axisPoint = ClosestPointOnConeAxis(cone, R0);
            float localRadius = LocalConeRadius(cone, axisPoint);

            float distSq = SegmentMath.SegmentSegmentDistanceSq(
                R0, R0 + Rd * 1e6f,
                axisPoint, axisPoint
            );

            if (distSq > localRadius * localRadius)
                return false;

            Vector3 toAxis = axisPoint - R0;
            float proj = Vector3.Dot(toAxis, Rd) / Vector3.Dot(Rd, Rd);
            if (proj < 0f)
                return false;

            t = proj;
            return true;
        }

        #endregion


        // ============================================================
        // CONE vs TRIANGLE
        // ============================================================
        #region ConeTriangle

        /// <summary>
        /// Returns true if a cone intersects a triangle.
        /// </summary>
        public static bool IntersectsTriangle(this Cone cone, Vector3 a, Vector3 b, Vector3 c)
        {
            if (cone.Contains(a) || cone.Contains(b) || cone.Contains(c))
                return true;

            Vector3 triClosest = TriangleMath.ClosestPointOnTriangle(cone.Apex, a, b, c);
            Vector3 axisPoint = ClosestPointOnConeAxis(cone, triClosest);

            float localRadius = LocalConeRadius(cone, axisPoint);
            return (triClosest - axisPoint).sqrMagnitude <= localRadius * localRadius;
        }

        #endregion
    }
}
