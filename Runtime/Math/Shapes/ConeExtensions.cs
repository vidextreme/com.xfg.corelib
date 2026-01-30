// Copyright (c) 2025 John David Uy
// Licensed under the MIT License. See LICENSE for details.
// ------------------------------------------------------------------------------
// ConeExtensions
// ------------------------------------------------------------------------------
// Extension methods for Cone providing:
// - Point containment
// - Shape containment (Sphere, Capsule, Cylinder, Cone)
// - Deterministic, allocation-free geometric helpers
//
// All methods are suitable for Burst jobs and runtime gameplay systems.
// ------------------------------------------------------------------------------

using UnityEngine;

namespace XFG.Math.Shape
{
    public static class ConeExtensions
    {
        // ==============================================================================
        // INTERNAL HELPERS
        // ==============================================================================
        #region Helpers

        /// <summary>
        /// Returns the closest point on the cone's axis to the given point.
        /// </summary>
        private static Vector3 ClosestPointOnConeAxis(this Cone cone, Vector3 p)
        {
            Vector3 dir = cone.Axis.normalized;
            float t = Vector3.Dot(p - cone.Apex, dir);
            t = Mathf.Clamp01(t);
            return cone.Apex + dir * t;
        }

        /// <summary>
        /// Returns the radius of the cone at the given axis point.
        /// </summary>
        private static float LocalConeRadius(this Cone cone, Vector3 axisPoint)
        {
            float t = Vector3.Dot(axisPoint - cone.Apex, cone.Axis.normalized);
            t = Mathf.Clamp01(t);
            return (t / cone.Height) * cone.Radius;
        }

        #endregion

        // ==============================================================================
        // CONTAINS POINT
        // ==============================================================================
        #region ContainsPoint

        /// <summary>
        /// Returns true if the cone contains the point.
        /// </summary>
        public static bool Contains(this Cone cone, Vector3 p)
        {
            Vector3 axisPoint = cone.ClosestPointOnConeAxis(p);
            float localRadius = cone.LocalConeRadius(axisPoint);

            return (p - axisPoint).sqrMagnitude <= localRadius * localRadius;
        }

        #endregion

        // ==============================================================================
        // CONTAINS SPHERE
        // ==============================================================================
        #region ContainsSphere

        /// <summary>
        /// Returns true if the cone fully contains the sphere.
        /// </summary>
        public static bool Contains(this Cone cone, Sphere s)
        {
            Vector3 axisPoint = cone.ClosestPointOnConeAxis(s.Center);
            float localRadius = cone.LocalConeRadius(axisPoint);

            float innerRadius = localRadius - s.Radius;
            if (innerRadius < 0f)
                return false;

            return (s.Center - axisPoint).sqrMagnitude <= innerRadius * innerRadius;
        }

        #endregion

        // ==============================================================================
        // CONTAINS CAPSULE
        // ==============================================================================
        #region ContainsCapsule

        /// <summary>
        /// Returns true if the cone fully contains the capsule.
        /// </summary>
        public static bool Contains(this Cone cone, Capsule c)
        {
            Vector3 p0Axis = cone.ClosestPointOnConeAxis(c.P0);
            Vector3 p1Axis = cone.ClosestPointOnConeAxis(c.P1);

            float r0 = cone.LocalConeRadius(p0Axis) - c.Radius;
            float r1 = cone.LocalConeRadius(p1Axis) - c.Radius;

            if (r0 < 0f || r1 < 0f)
                return false;

            return (c.P0 - p0Axis).sqrMagnitude <= r0 * r0
                && (c.P1 - p1Axis).sqrMagnitude <= r1 * r1;
        }

        #endregion

        // ==============================================================================
        // CONTAINS CYLINDER
        // ==============================================================================
        #region ContainsCylinder

        /// <summary>
        /// Returns true if the cone fully contains the cylinder.
        /// </summary>
        public static bool Contains(this Cone cone, Cylinder cy)
        {
            Vector3 p0Axis = cone.ClosestPointOnConeAxis(cy.P0);
            Vector3 p1Axis = cone.ClosestPointOnConeAxis(cy.P1);

            float r0 = cone.LocalConeRadius(p0Axis) - cy.Radius;
            float r1 = cone.LocalConeRadius(p1Axis) - cy.Radius;

            if (r0 < 0f || r1 < 0f)
                return false;

            return (cy.P0 - p0Axis).sqrMagnitude <= r0 * r0
                && (cy.P1 - p1Axis).sqrMagnitude <= r1 * r1;
        }

        #endregion

        // ==============================================================================
        // CONTAINS CONE
        // ==============================================================================
        #region ContainsCone

        /// <summary>
        /// Returns true if this cone fully contains another cone.
        /// </summary>
        public static bool Contains(this Cone cone, Cone other)
        {
            Vector3 apexAxis = cone.ClosestPointOnConeAxis(other.Apex);
            Vector3 baseCenter = other.BaseCenter;
            Vector3 baseAxis = cone.ClosestPointOnConeAxis(baseCenter);

            float baseLocalRadius = cone.LocalConeRadius(baseAxis) - other.Radius;
            if (baseLocalRadius < 0f)
                return false;

            bool apexInside =
                (other.Apex - apexAxis).sqrMagnitude <=
                cone.LocalConeRadius(apexAxis) * cone.LocalConeRadius(apexAxis);

            bool baseInside =
                (baseCenter - baseAxis).sqrMagnitude <= baseLocalRadius * baseLocalRadius;

            return apexInside && baseInside;
        }

        #endregion
    }
}
