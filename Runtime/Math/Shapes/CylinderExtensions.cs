// Copyright (c) 2025 John David Uy
// Licensed under the MIT License. See LICENSE for details.
// ------------------------------------------------------------------------------
// CylinderExtensions
// ------------------------------------------------------------------------------
// Extension methods for Cylinder providing:
// - Point containment
// - Shape containment (Sphere, Capsule, Cylinder, Cone)
// - Deterministic, allocation-free geometric helpers
//
// All methods are suitable for Burst jobs and runtime gameplay systems.
// ------------------------------------------------------------------------------

using UnityEngine;

namespace XFG.Math.Shape
{
    public static class CylinderExtensions
    {
        // ==============================================================================
        // CONTAINS POINT
        // ==============================================================================
        #region ContainsPoint

        /// <summary>
        /// Returns true if the cylinder contains the point.
        /// </summary>
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

        // ==============================================================================
        // CONTAINS SPHERE
        // ==============================================================================
        #region ContainsSphere

        /// <summary>
        /// Returns true if the cylinder fully contains the sphere.
        /// </summary>
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

        // ==============================================================================
        // CONTAINS CAPSULE
        // ==============================================================================
        #region ContainsCapsule

        /// <summary>
        /// Returns true if the cylinder fully contains the capsule.
        /// </summary>
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

        // ==============================================================================
        // CONTAINS CYLINDER
        // ==============================================================================
        #region ContainsCylinder

        /// <summary>
        /// Returns true if this cylinder fully contains another cylinder.
        /// </summary>
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

        // ==============================================================================
        // CONTAINS CONE
        // ==============================================================================
        #region ContainsCone

        /// <summary>
        /// Returns true if the cylinder fully contains the cone.
        /// </summary>
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
