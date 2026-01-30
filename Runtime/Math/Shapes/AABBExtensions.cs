// Copyright (c) 2025 John David Uy
// Licensed under the MIT License. See LICENSE for details.
// ------------------------------------------------------------------------------
// AABBExtensions
// ------------------------------------------------------------------------------
// Utility extensions for UnityEngine.Bounds (AABB).
//
// These helpers provide:
// - Volume and surface area
// - Expand / Inflate operations
// - Encapsulation of primitive shapes (Sphere, Capsule, Cylinder, Cone)
// - Transforming an AABB by an arbitrary matrix
//
// All methods are deterministic, allocation-free, and suitable for Burst jobs.
// ------------------------------------------------------------------------------

using UnityEngine;

namespace XFG.Math.Shape
{
    public static class AABBExtensions
    {
        // ==========================================================================
        // VOLUME
        // ==========================================================================
        /// <summary>
        /// Returns the volume of the AABB.
        /// This is simply width * height * depth.
        /// </summary>
        public static float Volume(this Bounds b)
        {
            Vector3 s = b.size;
            return s.x * s.y * s.z;
        }

        // ==========================================================================
        // SURFACE AREA
        // ==========================================================================
        /// <summary>
        /// Returns the total surface area of the AABB.
        /// Formula: 2 * (xy + yz + zx)
        /// </summary>
        public static float SurfaceArea(this Bounds b)
        {
            Vector3 s = b.size;
            return 2f * (s.x * s.y + s.y * s.z + s.z * s.x);
        }

        // ==========================================================================
        // EXPAND / INFLATE
        // ==========================================================================
        /// <summary>
        /// Uniformly expands the AABB by the given amount in all directions.
        /// Equivalent to inflating the extents by +amount.
        /// </summary>
        public static Bounds Expand(this Bounds b, float amount)
        {
            Vector3 delta = new Vector3(amount, amount, amount);
            b.Expand(delta * 2f); // Bounds.Expand expects full size delta, not extents
            return b;
        }

        /// <summary>
        /// Expands the AABB by a non-uniform amount per axis.
        /// Each axis expands by ±amount.x/y/z.
        /// </summary>
        public static Bounds Expand(this Bounds b, Vector3 amount)
        {
            b.Expand(amount * 2f);
            return b;
        }

        // ==========================================================================
        // ENCAPSULATE SHAPES
        // ==========================================================================
        /// <summary>
        /// Expands the AABB so that it fully contains the sphere.
        /// Achieved by encapsulating the sphere's extreme points.
        /// </summary>
        public static Bounds Encapsulate(this Bounds b, Sphere s)
        {
            b.Encapsulate(s.Center + Vector3.one * s.Radius);
            b.Encapsulate(s.Center - Vector3.one * s.Radius);
            return b;
        }

        /// <summary>
        /// Expands the AABB so that it fully contains the capsule.
        /// Encapsulates both endpoints inflated by the capsule radius.
        /// </summary>
        public static Bounds Encapsulate(this Bounds b, Capsule c)
        {
            b.Encapsulate(c.P0 + Vector3.one * c.Radius);
            b.Encapsulate(c.P0 - Vector3.one * c.Radius);
            b.Encapsulate(c.P1 + Vector3.one * c.Radius);
            b.Encapsulate(c.P1 - Vector3.one * c.Radius);
            return b;
        }

        /// <summary>
        /// Expands the AABB so that it fully contains the cylinder.
        /// Encapsulates both endpoints inflated by the cylinder radius.
        /// </summary>
        public static Bounds Encapsulate(this Bounds b, Cylinder cy)
        {
            b.Encapsulate(cy.P0 + Vector3.one * cy.Radius);
            b.Encapsulate(cy.P0 - Vector3.one * cy.Radius);
            b.Encapsulate(cy.P1 + Vector3.one * cy.Radius);
            b.Encapsulate(cy.P1 - Vector3.one * cy.Radius);
            return b;
        }

        /// <summary>
        /// Expands the AABB so that it fully contains the cone.
        /// Encapsulates the apex and the base circle's extreme extents.
        /// </summary>
        public static Bounds Encapsulate(this Bounds b, Cone cone)
        {
            b.Encapsulate(cone.Apex);
            b.Encapsulate(cone.BaseCenter + Vector3.one * cone.Radius);
            b.Encapsulate(cone.BaseCenter - Vector3.one * cone.Radius);
            return b;
        }

        // ==========================================================================
        // TRANSFORM
        // ==========================================================================
        /// <summary>
        /// Transforms the AABB by an arbitrary matrix.
        /// 
        /// Because an AABB must remain axis-aligned, the transformed box is computed
        /// by transforming the center and then computing new extents based on the
        /// absolute contributions of each axis.
        ///
        /// This is the standard, stable method used in physics engines.
        /// </summary>
        public static Bounds Transform(this Bounds b, Matrix4x4 m)
        {
            // Transform the center normally.
            Vector3 c = m.MultiplyPoint3x4(b.center);

            // Extract extents and matrix axes.
            Vector3 ext = b.extents;
            Vector3 ax = m.GetColumn(0);
            Vector3 ay = m.GetColumn(1);
            Vector3 az = m.GetColumn(2);

            // New extents are the sum of absolute contributions of each axis.
            Vector3 newExtents = new Vector3(
                Mathf.Abs(ext.x * ax.x) + Mathf.Abs(ext.y * ay.x) + Mathf.Abs(ext.z * az.x),
                Mathf.Abs(ext.x * ax.y) + Mathf.Abs(ext.y * ay.y) + Mathf.Abs(ext.z * az.y),
                Mathf.Abs(ext.x * ax.z) + Mathf.Abs(ext.y * ay.z) + Mathf.Abs(ext.z * az.z)
            );

            return new Bounds(c, newExtents * 2f);
        }
    }
}
