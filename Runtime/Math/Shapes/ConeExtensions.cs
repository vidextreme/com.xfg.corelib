// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

using UnityEngine;

namespace XFG.Math.Shape
{
    /// <summary>
    /// Containment extension methods for <see cref="Cone"/>.
    /// </summary>
    /// <remarks>
    /// Provides:
    /// <list type="bullet">
    /// <item>Point containment</item>
    /// <item>Shape containment (Sphere, Capsule, Cylinder, Cone)</item>
    /// <item>Deterministic, allocation-free geometric helpers</item>
    /// </list>
    /// All methods are suitable for Burst jobs and runtime gameplay systems.
    /// </remarks>
    public static class ConeExtensions
    {
        // ---------------------------------------------------------------------
        // Internal Helpers
        // ---------------------------------------------------------------------
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

        // ---------------------------------------------------------------------
        // Contains Point
        // ---------------------------------------------------------------------
        #region ContainsPoint

        /// <summary>
        /// Returns true if the cone contains the point.
        /// </summary>
        /// <param name="cone">The cone.</param>
        /// <param name="p">The point to test.</param>
        /// <returns><c>true</c> if the point is inside or on the cone.</returns>
        public static bool Contains(this Cone cone, Vector3 p)
        {
            Vector3 axisPoint = cone.ClosestPointOnConeAxis(p);
            float localRadius = cone.LocalConeRadius(axisPoint);

            return (p - axisPoint).sqrMagnitude <= localRadius * localRadius;
        }

        #endregion

        // ---------------------------------------------------------------------
        // Contains Sphere
        // ---------------------------------------------------------------------
        #region ContainsSphere

        /// <summary>
        /// Returns true if the cone fully contains the sphere.
        /// </summary>
        /// <param name="cone">The containing cone.</param>
        /// <param name="s">The sphere to test.</param>
        /// <returns><c>true</c> if the sphere is fully inside.</returns>
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

        // ---------------------------------------------------------------------
        // Contains Capsule
        // ---------------------------------------------------------------------
        #region ContainsCapsule

        /// <summary>
        /// Returns true if the cone fully contains the capsule.
        /// </summary>
        /// <param name="cone">The containing cone.</param>
        /// <param name="c">The capsule to test.</param>
        /// <returns><c>true</c> if the capsule is fully inside.</returns>
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

        // ---------------------------------------------------------------------
        // Contains Cylinder
        // ---------------------------------------------------------------------
        #region ContainsCylinder

        /// <summary>
        /// Returns true if the cone fully contains the cylinder.
        /// </summary>
        /// <param name="cone">The containing cone.</param>
        /// <param name="cy">The cylinder to test.</param>
        /// <returns><c>true</c> if the cylinder is fully inside (cap centers tested only).</returns>
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

        // ---------------------------------------------------------------------
        // Contains Cone
        // ---------------------------------------------------------------------
        #region ContainsCone

        /// <summary>
        /// Returns true if this cone fully contains another cone.
        /// </summary>
        /// <param name="cone">The containing cone.</param>
        /// <param name="other">The cone to test.</param>
        /// <returns><c>true</c> if <paramref name="other"/> is fully inside (apex and base tested only).</returns>
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
