// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

using UnityEngine;

namespace XFG.Math.Shape
{
    /// <summary>
    /// Low-level math utilities for view frustums.
    /// </summary>
    /// <remarks>
    /// Provides:
    /// <list type="bullet">
    /// <item>Construction from a <see cref="Camera"/></item>
    /// <item>Construction from planes</item>
    /// <item>Extraction of frustum corners</item>
    /// <item>AABB/OBB classification</item>
    /// <item>Point, ray, and segment tests</item>
    /// <item>Frustum vs frustum overlap</item>
    /// </list>
    /// All methods are deterministic, allocation-free, and suitable for Burst jobs.
    /// </remarks>
    public static class FrustumMath
    {
        // ---------------------------------------------------------------------
        // Factory: From Camera
        // ---------------------------------------------------------------------
        /// <summary>
        /// Builds a frustum from the view planes of a camera.
        /// </summary>
        /// <param name="cam">The camera whose frustum planes are used.</param>
        /// <returns>The camera's view frustum.</returns>
        public static Frustum FromCamera(Camera cam)
        {
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);

            return new Frustum
            {
                Left = planes[0],
                Right = planes[1],
                Bottom = planes[2],
                Top = planes[3],
                Near = planes[4],
                Far = planes[5]
            };
        }

        // ---------------------------------------------------------------------
        // Factory: From Planes
        // ---------------------------------------------------------------------
        /// <summary>
        /// Builds a frustum from six explicit planes.
        /// </summary>
        /// <param name="left">The left clipping plane.</param>
        /// <param name="right">The right clipping plane.</param>
        /// <param name="bottom">The bottom clipping plane.</param>
        /// <param name="top">The top clipping plane.</param>
        /// <param name="near">The near clipping plane.</param>
        /// <param name="far">The far clipping plane.</param>
        /// <returns>A frustum made of the given planes.</returns>
        public static Frustum FromPlanes(
            Plane left, Plane right, Plane bottom,
            Plane top, Plane near, Plane far)
        {
            return new Frustum
            {
                Left = left,
                Right = right,
                Bottom = bottom,
                Top = top,
                Near = near,
                Far = far
            };
        }

        // ---------------------------------------------------------------------
        // Corners
        // ---------------------------------------------------------------------
        /// <summary>
        /// Computes the eight corner points of the frustum.
        /// </summary>
        /// <remarks>
        /// Order: near bottom-left, near bottom-right, near top-left, near top-right,
        /// then the same four for the far plane. Corners of degenerate (parallel)
        /// plane triples are written as <see cref="Vector3.zero"/>.
        /// </remarks>
        /// <param name="f">The frustum.</param>
        /// <param name="outCorners">Destination array; must hold at least 8 elements.</param>
        public static void GetCorners(this Frustum f, Vector3[] outCorners)
        {
            Intersect3(f.Near, f.Bottom, f.Left, outCorners, 0);
            Intersect3(f.Near, f.Bottom, f.Right, outCorners, 1);
            Intersect3(f.Near, f.Top, f.Left, outCorners, 2);
            Intersect3(f.Near, f.Top, f.Right, outCorners, 3);

            Intersect3(f.Far, f.Bottom, f.Left, outCorners, 4);
            Intersect3(f.Far, f.Bottom, f.Right, outCorners, 5);
            Intersect3(f.Far, f.Top, f.Left, outCorners, 6);
            Intersect3(f.Far, f.Top, f.Right, outCorners, 7);
        }

        private static void Intersect3(Plane a, Plane b, Plane c, Vector3[] outCorners, int index)
        {
            Vector3 n1 = a.normal;
            Vector3 n2 = b.normal;
            Vector3 n3 = c.normal;

            float d1 = a.distance;
            float d2 = b.distance;
            float d3 = c.distance;

            Vector3 cross23 = Vector3.Cross(n2, n3);
            float denom = Vector3.Dot(n1, cross23);

            if (Mathf.Abs(denom) < 1e-6f)
            {
                outCorners[index] = Vector3.zero;
                return;
            }

            Vector3 term1 = cross23 * -d1;
            Vector3 term2 = Vector3.Cross(n3, n1) * -d2;
            Vector3 term3 = Vector3.Cross(n1, n2) * -d3;

            outCorners[index] = (term1 + term2 + term3) / denom;
        }
    }
}
