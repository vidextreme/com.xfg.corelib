// Copyright (c) 2025 John David Uy
// Licensed under the MIT License. See LICENSE for details.
// ------------------------------------------------------------------------------
// CapsuleExtensions
// ------------------------------------------------------------------------------
// Extension methods for Capsule providing:
// - Point and shape containment tests
// - Capsule vs Sphere/Cylinder/Cone containment logic
// - Deterministic, allocation-free geometric helpers
//
// All methods are suitable for Burst jobs and runtime gameplay systems.
// ------------------------------------------------------------------------------

using UnityEngine;

namespace XFG.Math.Shape
{
    public static class CapsuleExtensions
    {
        // ==============================================================================
        // CONTAINS POINT
        // ==============================================================================
        #region ContainsPoint

        /// <summary>
        /// Returns true if the capsule contains the point.
        /// </summary>
        public static bool Contains(this Capsule c, Vector3 p)
        {
            Vector3 closest = SegmentMath.ClosestPointOnSegment(c.P0, c.P1, p);
            return (p - closest).sqrMagnitude <= c.Radius * c.Radius;
        }

        #endregion

        // ==============================================================================
        // CONTAINS SPHERE
        // ==============================================================================
        #region ContainsSphere

        /// <summary>
        /// Returns true if the capsule fully contains the sphere.
        /// </summary>
        public static bool Contains(this Capsule c, Sphere s)
        {
            float innerRadius = c.Radius - s.Radius;
            if (innerRadius < 0f)
                return false;

            float rSq = innerRadius * innerRadius;

            Vector3 closest = SegmentMath.ClosestPointOnSegment(c.P0, c.P1, s.Center);
            return (s.Center - closest).sqrMagnitude <= rSq;
        }

        #endregion

        // ==============================================================================
        // CONTAINS CAPSULE
        // ==============================================================================
        #region ContainsCapsule

        /// <summary>
        /// Returns true if this capsule fully contains another capsule.
        /// </summary>
        public static bool Contains(this Capsule c, Capsule other)
        {
            float innerRadius = c.Radius - other.Radius;
            if (innerRadius < 0f)
                return false;

            float rSq = innerRadius * innerRadius;

            Vector3 p0 = SegmentMath.ClosestPointOnSegment(c.P0, c.P1, other.P0);
            Vector3 p1 = SegmentMath.ClosestPointOnSegment(c.P0, c.P1, other.P1);

            return (other.P0 - p0).sqrMagnitude <= rSq
                && (other.P1 - p1).sqrMagnitude <= rSq;
        }

        #endregion

        // ==============================================================================
        // CONTAINS CYLINDER
        // ==============================================================================
        #region ContainsCylinder

        /// <summary>
        /// Returns true if the capsule fully contains the cylinder.
        /// </summary>
        public static bool Contains(this Capsule c, Cylinder cy)
        {
            float innerRadius = c.Radius - cy.Radius;
            if (innerRadius < 0f)
                return false;

            float rSq = innerRadius * innerRadius;

            Vector3 p0 = SegmentMath.ClosestPointOnSegment(c.P0, c.P1, cy.P0);
            Vector3 p1 = SegmentMath.ClosestPointOnSegment(c.P0, c.P1, cy.P1);

            return (cy.P0 - p0).sqrMagnitude <= rSq
                && (cy.P1 - p1).sqrMagnitude <= rSq;
        }

        #endregion

        // ==============================================================================
        // CONTAINS CONE
        // ==============================================================================
        #region ContainsCone

        /// <summary>
        /// Returns true if the capsule fully contains the cone.
        /// </summary>
        public static bool Contains(this Capsule c, Cone cone)
        {
            float innerRadius = c.Radius - cone.Radius;
            if (innerRadius < 0f)
                return false;

            float rSq = innerRadius * innerRadius;

            Vector3 baseCenter = cone.BaseCenter;

            Vector3 apexClosest = SegmentMath.ClosestPointOnSegment(c.P0, c.P1, cone.Apex);
            Vector3 baseClosest = SegmentMath.ClosestPointOnSegment(c.P0, c.P1, baseCenter);

            bool apexInside = (cone.Apex - apexClosest).sqrMagnitude <= c.Radius * c.Radius;
            bool baseInside = (baseCenter - baseClosest).sqrMagnitude <= rSq;

            return apexInside && baseInside;
        }

        #endregion
    }
}
