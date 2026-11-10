// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

using UnityEngine;

namespace XFG.Math.Shape
{
    /// <summary>
    /// Containment extension methods for <see cref="Capsule"/>.
    /// </summary>
    /// <remarks>
    /// Provides:
    /// <list type="bullet">
    /// <item>Point and shape containment tests</item>
    /// <item>Capsule vs Sphere/Cylinder/Cone containment logic</item>
    /// <item>Deterministic, allocation-free geometric helpers</item>
    /// </list>
    /// All methods are suitable for Burst jobs and runtime gameplay systems.
    /// </remarks>
    public static class CapsuleExtensions
    {
        // ---------------------------------------------------------------------
        // Contains Point
        // ---------------------------------------------------------------------
        #region ContainsPoint

        /// <summary>
        /// Returns true if the capsule contains the point.
        /// </summary>
        /// <param name="c">The capsule.</param>
        /// <param name="p">The point to test.</param>
        /// <returns><c>true</c> if the point is inside or on the capsule.</returns>
        public static bool Contains(this Capsule c, Vector3 p)
        {
            Vector3 closest = SegmentMath.ClosestPointOnSegment(c.P0, c.P1, p);
            return (p - closest).sqrMagnitude <= c.Radius * c.Radius;
        }

        #endregion

        // ---------------------------------------------------------------------
        // Contains Sphere
        // ---------------------------------------------------------------------
        #region ContainsSphere

        /// <summary>
        /// Returns true if the capsule fully contains the sphere.
        /// </summary>
        /// <param name="c">The containing capsule.</param>
        /// <param name="s">The sphere to test.</param>
        /// <returns><c>true</c> if the sphere is fully inside.</returns>
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

        // ---------------------------------------------------------------------
        // Contains Capsule
        // ---------------------------------------------------------------------
        #region ContainsCapsule

        /// <summary>
        /// Returns true if this capsule fully contains another capsule.
        /// </summary>
        /// <param name="c">The containing capsule.</param>
        /// <param name="other">The capsule to test.</param>
        /// <returns><c>true</c> if <paramref name="other"/> is fully inside.</returns>
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

        // ---------------------------------------------------------------------
        // Contains Cylinder
        // ---------------------------------------------------------------------
        #region ContainsCylinder

        /// <summary>
        /// Returns true if the capsule fully contains the cylinder.
        /// </summary>
        /// <param name="c">The containing capsule.</param>
        /// <param name="cy">The cylinder to test.</param>
        /// <returns><c>true</c> if the cylinder is fully inside (cap centers tested only).</returns>
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

        // ---------------------------------------------------------------------
        // Contains Cone
        // ---------------------------------------------------------------------
        #region ContainsCone

        /// <summary>
        /// Returns true if the capsule fully contains the cone.
        /// </summary>
        /// <param name="c">The containing capsule.</param>
        /// <param name="cone">The cone to test.</param>
        /// <returns><c>true</c> if the cone is fully inside (apex and base center tested only).</returns>
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
