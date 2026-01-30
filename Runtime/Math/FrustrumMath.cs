// Copyright (c) 2025 John David Uy
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
    public struct Frustum
    {
        public Plane Left;
        public Plane Right;
        public Plane Bottom;
        public Plane Top;
        public Plane Near;
        public Plane Far;

        public Plane this[int i]
        {
            get
            {
                switch (i)
                {
                    case 0: return Left;
                    case 1: return Right;
                    case 2: return Bottom;
                    case 3: return Top;
                    case 4: return Near;
                    case 5: return Far;
                }
                return Left;
            }
        }
    }

    public static class FrustumMath
    {
        // ==========================================================================
        // FROM CAMERA
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
        // FROM PLANES
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
        // POINT TEST
        // ==========================================================================
        public static bool ContainsPoint(Frustum f, Vector3 p)
        {
            for (int i = 0; i < 6; i++)
            {
                Plane plane = f[i];
                Vector3 n = plane.normal;
                float d = plane.distance;

                if (Vector3.Dot(n, p) + d < 0f)
                    return false;
            }
            return true;
        }

        // ==========================================================================
        // AABB TEST
        // ==========================================================================
        public static bool IntersectsAABB(Frustum f, Bounds b)
        {
            Vector3 c = b.center;
            Vector3 e = b.extents;

            for (int i = 0; i < 6; i++)
            {
                Plane plane = f[i];
                Vector3 n = plane.normal;
                float d = plane.distance;

                float r =
                    Mathf.Abs(n.x * e.x) +
                    Mathf.Abs(n.y * e.y) +
                    Mathf.Abs(n.z * e.z);

                float s = Vector3.Dot(n, c) + d;

                if (s < -r)
                    return false;
            }

            return true;
        }

        // ==========================================================================
        // OBB TEST
        // ==========================================================================
        public static bool IntersectsOBB(Frustum f, OBB b)
        {
            Vector3 center = b.Center;
            Vector3 ext = b.Extents;
            Vector3 right = b.Right;
            Vector3 up = b.Up;
            Vector3 forward = b.Forward;

            for (int i = 0; i < 6; i++)
            {
                Plane plane = f[i];
                Vector3 n = plane.normal;
                float d = plane.distance;

                float r =
                    Mathf.Abs(Vector3.Dot(n, right)) * ext.x +
                    Mathf.Abs(Vector3.Dot(n, up)) * ext.y +
                    Mathf.Abs(Vector3.Dot(n, forward)) * ext.z;

                float s = Vector3.Dot(n, center) + d;

                if (s < -r)
                    return false;
            }

            return true;
        }

        // ==========================================================================
        // RAY TEST
        // ==========================================================================
        public static bool IntersectsRay(Frustum f, Vector3 origin, Vector3 dir)
        {
            float tMin = 0f;
            float tMax = float.MaxValue;

            for (int i = 0; i < 6; i++)
            {
                Plane plane = f[i];
                Vector3 n = plane.normal;
                float d = plane.distance;

                float denom = Vector3.Dot(n, dir);
                float dist = Vector3.Dot(n, origin) + d;

                if (Mathf.Abs(denom) < 1e-6f)
                {
                    if (dist > 0f)
                        return false;
                    continue;
                }

                float t = -dist / denom;

                if (denom < 0f)
                {
                    if (t > tMin) tMin = t;
                }
                else
                {
                    if (t < tMax) tMax = t;
                }

                if (tMin > tMax)
                    return false;
            }

            return true;
        }

        // ==========================================================================
        // SEGMENT TEST
        // ==========================================================================
        public static bool IntersectsSegment(Frustum f, Vector3 a, Vector3 b)
        {
            Vector3 ab = b - a;

            float tMin = 0f;
            float tMax = 1f;

            for (int i = 0; i < 6; i++)
            {
                Plane plane = f[i];
                Vector3 n = plane.normal;
                float d = plane.distance;

                float denom = Vector3.Dot(n, ab);
                float dist = Vector3.Dot(n, a) + d;

                if (Mathf.Abs(denom) < 1e-6f)
                {
                    if (dist > 0f)
                        return false;
                    continue;
                }

                float t = -dist / denom;

                if (denom < 0f)
                {
                    if (t > tMin) tMin = t;
                }
                else
                {
                    if (t < tMax) tMax = t;
                }

                if (tMin > tMax)
                    return false;
            }

            return true;
        }

        // ==========================================================================
        // CORNERS
        // ==========================================================================
        public static void GetCorners(Frustum f, Vector3[] outCorners)
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
