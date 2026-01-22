// Copyright (c) 2025 Extreme Focus Games
// Licensed under the MIT License. See LICENSE for details.

using UnityEngine;

namespace XFG.Math.Shape
{
    // ------------------------------------------------------------------------------
    // ShapeBounds
    // ------------------------------------------------------------------------------
    // Computes axis-aligned bounding boxes (AABB) for all XFG.Math.Shape primitives.
    //
    // These functions:
    // - Are deterministic
    // - Are allocation-free
    // - Are Burst-friendly
    // - Do not depend on Unity physics
    //
    // AABB is returned as UnityEngine.Bounds for convenience.
    // ------------------------------------------------------------------------------

    public static class ShapeBounds
    {
        // ============================================================
        // SPHERE
        // ============================================================
        #region Sphere

        /// <summary>
        /// Computes the AABB of a sphere.
        /// </summary>
        public static Bounds GetAABB(this Sphere s)
        {
            Vector3 extents = new Vector3(s.Radius, s.Radius, s.Radius);
            return new Bounds(s.Center, extents * 2f);
        }

        #endregion


        // ============================================================
        // CAPSULE
        // ============================================================
        #region Capsule

        /// <summary>
        /// Computes the AABB of a capsule.
        /// </summary>
        public static Bounds GetAABB(this Capsule c)
        {
            Vector3 min = Vector3.Min(c.P0, c.P1);
            Vector3 max = Vector3.Max(c.P0, c.P1);

            Vector3 r = new Vector3(c.Radius, c.Radius, c.Radius);

            min -= r;
            max += r;

            Bounds b = new Bounds();
            b.SetMinMax(min, max);
            return b;
        }

        #endregion


        // ============================================================
        // CYLINDER
        // ============================================================
        #region Cylinder

        /// <summary>
        /// Computes the AABB of a finite cylinder.
        /// </summary>
        public static Bounds GetAABB(this Cylinder cy)
        {
            Vector3 min = Vector3.Min(cy.P0, cy.P1);
            Vector3 max = Vector3.Max(cy.P0, cy.P1);

            Vector3 r = new Vector3(cy.Radius, cy.Radius, cy.Radius);

            min -= r;
            max += r;

            Bounds b = new Bounds();
            b.SetMinMax(min, max);
            return b;
        }

        #endregion


        // ============================================================
        // TRIANGLE
        // ============================================================
        #region Triangle

        /// <summary>
        /// Computes the AABB of a triangle.
        /// </summary>
        public static Bounds GetAABB(Vector3 a, Vector3 b, Vector3 c)
        {
            Vector3 min = Vector3.Min(a, Vector3.Min(b, c));
            Vector3 max = Vector3.Max(a, Vector3.Max(b, c));

            Bounds bnd = new Bounds();
            bnd.SetMinMax(min, max);
            return bnd;
        }

        #endregion


        // ============================================================
        // LINE (infinite)
        // ============================================================
        #region Line

        /// <summary>
        /// Infinite lines do not have finite AABBs.
        /// This returns an empty Bounds centered at L0.
        /// </summary>
        public static Bounds GetAABB_Line(Vector3 L0)
        {
            return new Bounds(L0, Vector3.zero);
        }

        #endregion


        // ============================================================
        // RAY
        // ============================================================
        #region Ray

        /// <summary>
        /// Rays do not have finite AABBs.
        /// This returns an empty Bounds centered at R0.
        /// </summary>
        public static Bounds GetAABB_Ray(Vector3 R0)
        {
            return new Bounds(R0, Vector3.zero);
        }

        #endregion


        // ============================================================
        // SEGMENT
        // ============================================================
        #region Segment

        /// <summary>
        /// Computes the AABB of a finite segment.
        /// </summary>
        public static Bounds GetAABB_Segment(Vector3 p0, Vector3 p1)
        {
            Vector3 min = Vector3.Min(p0, p1);
            Vector3 max = Vector3.Max(p0, p1);

            Bounds b = new Bounds();
            b.SetMinMax(min, max);
            return b;
        }

        #endregion
    }
}
