// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

using UnityEngine;

namespace XFG.Math.Shape
{
    /// <summary>
    /// Containment extension methods for <see cref="Cylinder"/>.
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
    public static class CylinderExtensions
    {
        // ---------------------------------------------------------------------
        // Contains Point
        // ---------------------------------------------------------------------
        #region ContainsPoint

        /// <summary>
        /// Returns true if the cylinder contains the point.
        /// </summary>
        /// <param name="cy">The cylinder.</param>
        /// <param name="p">The point to test.</param>
        /// <returns><c>true</c> if the point is inside or on the cylinder.</returns>
        public static bool Contains(this Cylinder cy, Vector3 p)
        {
            Vector3 axis = cy.P1 - cy.P0;
            float axisLenSq = Vector3.Dot(axis, axis);

            float t = Vector3.Dot(p - cy.P0, axis) / axisLenSq;
            t = Mathf.Clamp01(t);

            Vector3 axisPoint = cy.P0 + axis * t;
            return (p - axisPoint).sqrMagnitude <= cy.Radius * cy.Radius;
        }

        #endregion

        // ---------------------------------------------------------------------
        // Contains Sphere
        // ---------------------------------------------------------------------
        #region ContainsSphere

        /// <summary>
        /// Returns true if the cylinder fully contains the sphere.
        /// </summary>
        /// <param name="cy">The containing cylinder.</param>
        /// <param name="s">The sphere to test.</param>
        /// <returns><c>true</c> if the sphere is fully inside.</returns>
        public static bool Contains(this Cylinder cy, Sphere s)
        {
            float innerRadius = cy.Radius - s.Radius;
            if (innerRadius < 0f)
                return false;

            Vector3 axis = cy.P1 - cy.P0;
            float axisLenSq = Vector3.Dot(axis, axis);

            float t = Vector3.Dot(s.Center - cy.P0, axis) / axisLenSq;
            t = Mathf.Clamp01(t);

            Vector3 axisPoint = cy.P0 + axis * t;
            return (s.Center - axisPoint).sqrMagnitude <= innerRadius * innerRadius;
        }

        #endregion

        // ---------------------------------------------------------------------
        // Contains Capsule
        // ---------------------------------------------------------------------
        #region ContainsCapsule

        /// <summary>
        /// Returns true if the cylinder fully contains the capsule.
        /// </summary>
        /// <param name="cy">The containing cylinder.</param>
        /// <param name="c">The capsule to test.</param>
        /// <returns><c>true</c> if the capsule is fully inside.</returns>
        public static bool Contains(this Cylinder cy, Capsule c)
        {
            float innerRadius = cy.Radius - c.Radius;
            if (innerRadius < 0f)
                return false;

            float rSq = innerRadius * innerRadius;

            Vector3 axis = cy.P1 - cy.P0;
            float axisLenSq = Vector3.Dot(axis, axis);

            Vector3 p0Axis = cy.P0 + axis * Mathf.Clamp01(Vector3.Dot(c.P0 - cy.P0, axis) / axisLenSq);
            Vector3 p1Axis = cy.P0 + axis * Mathf.Clamp01(Vector3.Dot(c.P1 - cy.P0, axis) / axisLenSq);

            return (c.P0 - p0Axis).sqrMagnitude <= rSq
                && (c.P1 - p1Axis).sqrMagnitude <= rSq;
        }

        #endregion

        // ---------------------------------------------------------------------
        // Contains Cylinder
        // ---------------------------------------------------------------------
        #region ContainsCylinder

        /// <summary>
        /// Returns true if this cylinder fully contains another cylinder.
        /// </summary>
        /// <param name="cy">The containing cylinder.</param>
        /// <param name="other">The cylinder to test.</param>
        /// <returns><c>true</c> if <paramref name="other"/> is fully inside (cap centers tested only).</returns>
        public static bool Contains(this Cylinder cy, Cylinder other)
        {
            float innerRadius = cy.Radius - other.Radius;
            if (innerRadius < 0f)
                return false;

            float rSq = innerRadius * innerRadius;

            Vector3 axis = cy.P1 - cy.P0;
            float axisLenSq = Vector3.Dot(axis, axis);

            Vector3 p0Axis = cy.P0 + axis * Mathf.Clamp01(Vector3.Dot(other.P0 - cy.P0, axis) / axisLenSq);
            Vector3 p1Axis = cy.P0 + axis * Mathf.Clamp01(Vector3.Dot(other.P1 - cy.P0, axis) / axisLenSq);

            return (other.P0 - p0Axis).sqrMagnitude <= rSq
                && (other.P1 - p1Axis).sqrMagnitude <= rSq;
        }

        #endregion

        // ---------------------------------------------------------------------
        // Contains Cone
        // ---------------------------------------------------------------------
        #region ContainsCone

        /// <summary>
        /// Returns true if the cylinder fully contains the cone.
        /// </summary>
        /// <param name="cy">The containing cylinder.</param>
        /// <param name="cone">The cone to test.</param>
        /// <returns><c>true</c> if the cone is fully inside (apex and base center tested only).</returns>
        public static bool Contains(this Cylinder cy, Cone cone)
        {
            float innerRadius = cy.Radius - cone.Radius;
            if (innerRadius < 0f)
                return false;

            float rSq = innerRadius * innerRadius;

            Vector3 axis = cy.P1 - cy.P0;
            float axisLenSq = Vector3.Dot(axis, axis);

            Vector3 baseCenter = cone.BaseCenter;

            Vector3 apexAxis = cy.P0 + axis * Mathf.Clamp01(Vector3.Dot(cone.Apex - cy.P0, axis) / axisLenSq);
            Vector3 baseAxis = cy.P0 + axis * Mathf.Clamp01(Vector3.Dot(baseCenter - cy.P0, axis) / axisLenSq);

            bool apexInside = (cone.Apex - apexAxis).sqrMagnitude <= cy.Radius * cy.Radius;
            bool baseInside = (baseCenter - baseAxis).sqrMagnitude <= rSq;

            return apexInside && baseInside;
        }

        #endregion
    }
}
