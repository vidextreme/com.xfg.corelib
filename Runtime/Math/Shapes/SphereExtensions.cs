// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.
// ------------------------------------------------------------------------------
// SphereExtensions
// ------------------------------------------------------------------------------
// Extension methods for Sphere providing:
// - Point and shape containment tests
// - Expansion to include points or other spheres
// - Sphere merging utilities
// - Intersection and overlap predicates
//
// All methods are deterministic, allocation-free, and suitable for Burst jobs.
// ------------------------------------------------------------------------------

using UnityEngine;

namespace XFG.Math.Shape
{
    public static class SphereExtensions
    {
        // ==============================================================================
        // CONTAINS
        // ==============================================================================
        #region Contains

        /// <summary>
        /// Returns true if the sphere contains the point.
        /// </summary>
        public static bool Contains(this Sphere s, Vector3 p)
        {
            return (p - s.Center).sqrMagnitude <= s.Radius * s.Radius;
        }

        /// <summary>
        /// Returns true if this sphere fully contains another sphere.
        /// </summary>
        public static bool Contains(this Sphere s, Sphere other)
        {
            if (other.Radius > s.Radius)
                return false;

            float maxDist = s.Radius - other.Radius;
            return (other.Center - s.Center).sqrMagnitude <= maxDist * maxDist;
        }

        /// <summary>
        /// Returns true if the sphere fully contains the capsule.
        /// </summary>
        public static bool Contains(this Sphere s, Capsule c)
        {
            float innerRadius = s.Radius - c.Radius;
            if (innerRadius < 0f)
                return false;

            float rSq = innerRadius * innerRadius;

            return (c.P0 - s.Center).sqrMagnitude <= rSq
                && (c.P1 - s.Center).sqrMagnitude <= rSq;
        }

        /// <summary>
        /// Returns true if the sphere fully contains the cylinder.
        /// </summary>
        public static bool Contains(this Sphere s, Cylinder cy)
        {
            float innerRadius = s.Radius - cy.Radius;
            if (innerRadius < 0f)
                return false;

            float rSq = innerRadius * innerRadius;

            return (cy.P0 - s.Center).sqrMagnitude <= rSq
                && (cy.P1 - s.Center).sqrMagnitude <= rSq;
        }

        /// <summary>
        /// Returns true if the sphere fully contains the cone.
        /// </summary>
        public static bool Contains(this Sphere s, Cone cone)
        {
            Vector3 baseCenter = cone.BaseCenter;

            float innerRadius = s.Radius - cone.Radius;
            if (innerRadius < 0f)
                return false;

            float rSq = innerRadius * innerRadius;

            bool apexInside = (cone.Apex - s.Center).sqrMagnitude <= s.Radius * s.Radius;
            bool baseInside = (baseCenter - s.Center).sqrMagnitude <= rSq;

            return apexInside && baseInside;
        }

        #endregion

        // ==============================================================================
        // EXPAND TO INCLUDE
        // ==============================================================================
        #region ExpandToInclude

        /// <summary>
        /// Expands the sphere so that it includes the given point.
        /// </summary>
        public static void ExpandToInclude(ref this Sphere s, Vector3 p)
        {
            Vector3 toPoint = p - s.Center;
            float distSq = toPoint.sqrMagnitude;

            if (distSq <= s.Radius * s.Radius)
                return;

            float dist = Mathf.Sqrt(distSq);
            float newRadius = (s.Radius + dist) * 0.5f;

            float shift = newRadius - s.Radius;
            Vector3 dir = toPoint / dist;

            s.Center += dir * shift;
            s.Radius = newRadius;
        }

        /// <summary>
        /// Expands the sphere so that it includes another sphere.
        /// </summary>
        public static void ExpandToInclude(ref this Sphere s, Sphere other)
        {
            Vector3 toOther = other.Center - s.Center;
            float distSq = toOther.sqrMagnitude;

            float rDiff = other.Radius - s.Radius;

            if (rDiff <= 0f && distSq <= (s.Radius - other.Radius) * (s.Radius - other.Radius))
                return;

            float dist = Mathf.Sqrt(distSq);

            if (dist + s.Radius <= other.Radius)
            {
                s.Center = other.Center;
                s.Radius = other.Radius;
                return;
            }

            float newRadius = (dist + s.Radius + other.Radius) * 0.5f;

            float shift = newRadius - s.Radius;
            Vector3 dir = dist > 0f ? toOther / dist : Vector3.zero;

            s.Center += dir * shift;
            s.Radius = newRadius;
        }

        /// <summary>
        /// Expands the sphere to include all points in the array.
        /// </summary>
        public static void ExpandToInclude(ref this Sphere s, Vector3[] points)
        {
            for (int i = 0; i < points.Length; i++)
                s.ExpandToInclude(points[i]);
        }

        #endregion

        // ==============================================================================
        // MERGE
        // ==============================================================================
        #region Merge

        /// <summary>
        /// Returns a new sphere that encloses both input spheres.
        /// </summary>
        public static Sphere Merge(Sphere a, Sphere b)
        {
            Sphere result = a;
            result.ExpandToInclude(b);
            return result;
        }

        #endregion

        // ==============================================================================
        // FROM POINTS
        // ==============================================================================
        #region FromPoints

        /// <summary>
        /// Builds a sphere that encloses all points in the array.
        /// </summary>
        public static Sphere FromPoints(Vector3[] points)
        {
            if (points == null || points.Length == 0)
                return new Sphere(Vector3.zero, 0f);

            Sphere s = new Sphere(points[0], 0f);

            for (int i = 0; i < points.Length; i++)
                s.ExpandToInclude(points[i]);

            return s;
        }

        #endregion

        // ==============================================================================
        // INTERSECTS OR CONTAINS
        // ==============================================================================
        #region IntersectsOrContains

        /// <summary>
        /// Returns true if the spheres intersect or one contains the other.
        /// </summary>
        public static bool IntersectsOrContains(this Sphere a, Sphere b)
        {
            if (a.Contains(b) || b.Contains(a))
                return true;

            float r = a.Radius + b.Radius;
            return (a.Center - b.Center).sqrMagnitude <= r * r;
        }

        /// <summary>
        /// Returns true if the sphere intersects or contains the point.
        /// </summary>
        public static bool IntersectsOrContains(this Sphere s, Vector3 p)
        {
            return s.Contains(p);
        }

        #endregion
    }
}
