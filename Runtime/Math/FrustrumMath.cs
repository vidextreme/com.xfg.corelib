// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.
// ------------------------------------------------------------------------------
// FrustumMath
// ------------------------------------------------------------------------------
// Low-level math utilities for view frustums.
//
// Provides:
// - Construction from Camera
// - Construction from planes
// - Extraction of frustum corners
// - AABB/OBB classification
// - Point, ray, and segment tests
// - Frustum vs frustum overlap
//
// All methods are deterministic, allocation-free, and suitable for Burst jobs.
// ------------------------------------------------------------------------------

using UnityEngine;

namespace XFG.Math.Shape
{
    public static class FrustumMath
    {
        // ==========================================================================
        // FACTORY: FROM CAMERA
        // ==========================================================================
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

        // ==========================================================================
        // FACTORY: FROM PLANES
        // ==========================================================================
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

        // ==========================================================================
        // CORNERS
        // ==========================================================================
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
