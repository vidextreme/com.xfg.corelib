// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

using UnityEngine;

namespace XFG.Math.Shape
{
    /// <summary>
    /// Low-level math utilities for planes.
    /// </summary>
    /// <remarks>
    /// Provides:
    /// <list type="bullet">
    /// <item>Construction from point/normal or 3 points</item>
    /// <item>Normalization</item>
    /// <item>Signed distance</item>
    /// <item>Projection</item>
    /// <item>Ray/segment intersection</item>
    /// <item>Plane flipping</item>
    /// <item>Plane-plane intersection</item>
    /// <item>AABB/OBB classification</item>
    /// </list>
    /// All methods are deterministic, allocation-free, and suitable for Burst jobs.
    /// </remarks>
    public static class PlaneMath
    {
        // ---------------------------------------------------------------------
        // Construction
        // ---------------------------------------------------------------------
        /// <summary>
        /// Builds a plane that passes through a point with the given normal.
        /// </summary>
        /// <param name="point">A point on the plane.</param>
        /// <param name="normal">The plane normal; it is normalized internally.</param>
        /// <returns>The resulting plane.</returns>
        public static Plane FromPointNormal(Vector3 point, Vector3 normal)
        {
            Vector3 n = normal.normalized;
            float d = -Vector3.Dot(n, point);
            return new Plane(n, d);
        }

        /// <summary>
        /// Builds a plane through three points, with the normal following the winding a, b, c.
        /// </summary>
        /// <param name="a">The first point.</param>
        /// <param name="b">The second point.</param>
        /// <param name="c">The third point.</param>
        /// <returns>The resulting plane.</returns>
        public static Plane FromPoints(Vector3 a, Vector3 b, Vector3 c)
        {
            Vector3 n = Vector3.Cross(b - a, c - a).normalized;
            float d = -Vector3.Dot(n, a);
            return new Plane(n, d);
        }

        // ---------------------------------------------------------------------
        // Normalization
        // ---------------------------------------------------------------------
        /// <summary>
        /// Rescales the plane so its normal has unit length.
        /// </summary>
        /// <param name="p">The plane to normalize.</param>
        /// <returns>The normalized plane, or <paramref name="p"/> unchanged if its normal is near zero.</returns>
        public static Plane Normalize(Plane p)
        {
            Vector3 n = p.normal;
            float mag = n.magnitude;

            if (mag < 1e-6f)
                return p;

            float inv = 1f / mag;
            return new Plane(n * inv, p.distance * inv);
        }

        // ---------------------------------------------------------------------
        // Signed Distance
        // ---------------------------------------------------------------------
        /// <summary>
        /// Returns the signed distance from the plane to a point.
        /// </summary>
        /// <param name="p">The plane.</param>
        /// <param name="point">The query point.</param>
        /// <returns>Positive on the normal side, negative on the other side.</returns>
        public static float DistanceToPoint(Plane p, Vector3 point)
        {
            Vector3 n = p.normal;
            float d = p.distance;
            return Vector3.Dot(n, point) + d;
        }

        // ---------------------------------------------------------------------
        // Projection
        // ---------------------------------------------------------------------
        /// <summary>
        /// Projects a point onto the plane.
        /// </summary>
        /// <param name="p">The plane.</param>
        /// <param name="point">The point to project.</param>
        /// <returns>The projected point on the plane.</returns>
        public static Vector3 ProjectPoint(Plane p, Vector3 point)
        {
            Vector3 n = p.normal;
            float d = p.distance;
            float dist = Vector3.Dot(n, point) + d;
            return point - n * dist;
        }

        // ---------------------------------------------------------------------
        // Ray Intersection
        // ---------------------------------------------------------------------
        /// <summary>
        /// Intersects a ray with the plane.
        /// </summary>
        /// <param name="p">The plane.</param>
        /// <param name="origin">The ray origin.</param>
        /// <param name="dir">The ray direction.</param>
        /// <param name="t">The ray parameter of the hit point.</param>
        /// <returns><c>true</c> if the ray hits the plane at or in front of its origin.</returns>
        public static bool Raycast(Plane p, Vector3 origin, Vector3 dir, out float t)
        {
            t = 0f;

            Vector3 n = p.normal;
            float d = p.distance;

            float denom = Vector3.Dot(n, dir);
            if (Mathf.Abs(denom) < 1e-6f)
                return false;

            float num = -(Vector3.Dot(n, origin) + d);
            t = num / denom;

            return t >= 0f;
        }

        // ---------------------------------------------------------------------
        // Segment Intersection
        // ---------------------------------------------------------------------
        /// <summary>
        /// Intersects the segment ab with the plane.
        /// </summary>
        /// <param name="p">The plane.</param>
        /// <param name="a">The segment start point.</param>
        /// <param name="b">The segment end point.</param>
        /// <param name="hit">The intersection point, if any.</param>
        /// <returns><c>true</c> if the segment crosses the plane.</returns>
        public static bool SegmentIntersect(Plane p, Vector3 a, Vector3 b, out Vector3 hit)
        {
            hit = Vector3.zero;

            Vector3 n = p.normal;
            float d = p.distance;

            Vector3 ab = b - a;
            float denom = Vector3.Dot(n, ab);

            if (Mathf.Abs(denom) < 1e-6f)
                return false;

            float t = -(Vector3.Dot(n, a) + d) / denom;
            if (t < 0f || t > 1f)
                return false;

            hit = a + ab * t;
            return true;
        }

        // ---------------------------------------------------------------------
        // Flip
        // ---------------------------------------------------------------------
        /// <summary>
        /// Returns the same plane facing the opposite direction.
        /// </summary>
        /// <param name="p">The plane to flip.</param>
        /// <returns>The flipped plane.</returns>
        public static Plane Flip(Plane p)
        {
            Vector3 n = p.normal;
            float d = p.distance;
            return new Plane(-n, -d);
        }

        // ---------------------------------------------------------------------
        // Plane-Plane Intersection
        // ---------------------------------------------------------------------
        /// <summary>
        /// Computes the line where two planes intersect.
        /// </summary>
        /// <param name="a">The first plane.</param>
        /// <param name="b">The second plane.</param>
        /// <param name="point">A point on the intersection line.</param>
        /// <param name="dir">The (unnormalized) direction of the intersection line.</param>
        /// <returns><c>true</c> if the planes intersect; <c>false</c> if they are parallel.</returns>
        public static bool Intersect(Plane a, Plane b, out Vector3 point, out Vector3 dir)
        {
            point = Vector3.zero;

            Vector3 n1 = a.normal;
            Vector3 n2 = b.normal;

            dir = Vector3.Cross(n1, n2);
            float denom = dir.sqrMagnitude;

            if (denom < 1e-6f)
                return false;

            float d1 = -a.distance;
            float d2 = -b.distance;

            Vector3 c1 = Vector3.Cross(dir, n2) * d1;
            Vector3 c2 = Vector3.Cross(n1, dir) * d2;

            point = (c1 + c2) / denom;
            return true;
        }

        // ---------------------------------------------------------------------
        // AABB Classification
        // ---------------------------------------------------------------------
        /// <summary>
        /// Classifies an axis-aligned box against the plane.
        /// </summary>
        /// <param name="p">The plane.</param>
        /// <param name="b">The axis-aligned bounding box.</param>
        /// <returns>+1 if fully in front, -1 if fully behind, 0 if it straddles the plane.</returns>
        public static int ClassifyAABB(Plane p, Bounds b)
        {
            Vector3 n = p.normal;
            float d = p.distance;

            Vector3 c = b.center;
            Vector3 e = b.extents;

            float r =
                Mathf.Abs(n.x * e.x) +
                Mathf.Abs(n.y * e.y) +
                Mathf.Abs(n.z * e.z);

            float s = Vector3.Dot(n, c) + d;

            if (s > r) return +1;
            if (s < -r) return -1;
            return 0;
        }

        // ---------------------------------------------------------------------
        // OBB Classification
        // ---------------------------------------------------------------------
        /// <summary>
        /// Classifies an oriented box against the plane.
        /// </summary>
        /// <param name="p">The plane.</param>
        /// <param name="b">The oriented bounding box.</param>
        /// <returns>+1 if fully in front, -1 if fully behind, 0 if it straddles the plane.</returns>
        public static int ClassifyOBB(Plane p, OBB b)
        {
            Vector3 n = p.normal;
            float d = p.distance;

            float r =
                Mathf.Abs(Vector3.Dot(n, b.Right)) * b.Extents.x +
                Mathf.Abs(Vector3.Dot(n, b.Up)) * b.Extents.y +
                Mathf.Abs(Vector3.Dot(n, b.Forward)) * b.Extents.z;

            float s = Vector3.Dot(n, b.Center) + d;

            if (s > r) return +1;
            if (s < -r) return -1;
            return 0;
        }
    }
}
