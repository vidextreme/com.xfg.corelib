// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

using UnityEngine;

namespace XFG.Math.Shape
{
    /// <summary>
    /// Collision tests for axis-aligned bounding boxes (AABB).
    /// </summary>
    /// <remarks>
    /// Provides:
    /// <list type="bullet">
    /// <item>AABB vs AABB</item>
    /// <item>AABB vs Sphere</item>
    /// <item>AABB vs Capsule</item>
    /// <item>AABB vs Cylinder</item>
    /// <item>AABB vs Cone</item>
    /// <item>AABB vs Segment</item>
    /// <item>AABB vs Triangle</item>
    /// <item>AABB vs OBB (delegates to OBB SAT)</item>
    /// </list>
    /// </remarks>
    public static class AABBCollisionExtensions
    {
        // ---------------------------------------------------------------------
        // AABB vs AABB
        // ---------------------------------------------------------------------
        /// <summary>
        /// Tests whether two AABBs overlap.
        /// </summary>
        /// <param name="a">The first box.</param>
        /// <param name="b">The second box.</param>
        /// <returns><c>true</c> if the shapes overlap.</returns>
        public static bool Intersects(Bounds a, Bounds b)
        {
            Vector3 aMin = a.min;
            Vector3 aMax = a.max;
            Vector3 bMin = b.min;
            Vector3 bMax = b.max;

            if (aMax.x < bMin.x || aMin.x > bMax.x) return false;
            if (aMax.y < bMin.y || aMin.y > bMax.y) return false;
            if (aMax.z < bMin.z || aMin.z > bMax.z) return false;

            return true;
        }

        // ---------------------------------------------------------------------
        // AABB vs Sphere
        // ---------------------------------------------------------------------
        /// <summary>
        /// Tests whether an AABB overlaps a sphere.
        /// </summary>
        /// <param name="a">The axis-aligned bounding box.</param>
        /// <param name="sphereCenter">The sphere center.</param>
        /// <param name="sphereRadius">The sphere radius.</param>
        /// <returns><c>true</c> if the shapes overlap.</returns>
        public static bool Intersects(Bounds a, Vector3 sphereCenter, float sphereRadius)
        {
            Vector3 c = a.center;
            Vector3 e = a.extents;

            Vector3 d = sphereCenter - c;

            float x = Mathf.Clamp(d.x, -e.x, e.x);
            float y = Mathf.Clamp(d.y, -e.y, e.y);
            float z = Mathf.Clamp(d.z, -e.z, e.z);

            Vector3 closest = c + new Vector3(x, y, z);

            return (closest - sphereCenter).sqrMagnitude <= sphereRadius * sphereRadius;
        }

        // ---------------------------------------------------------------------
        // AABB vs Capsule
        // ---------------------------------------------------------------------
        /// <summary>
        /// Tests whether an AABB overlaps a capsule (approximate).
        /// </summary>
        /// <param name="a">The axis-aligned bounding box.</param>
        /// <param name="capsule">The capsule.</param>
        /// <returns><c>true</c> if the shapes overlap.</returns>
        public static bool Intersects(Bounds a, Capsule capsule)
        {
            Vector3 p0 = capsule.P0;
            Vector3 p1 = capsule.P1;
            float r = capsule.Radius;

            Vector3 closest = SegmentMath.ClosestPointOnSegmentToAABB(p0, p1, a.min, a.max);
            float rSq = r * r;

            return (closest - p0).sqrMagnitude <= rSq ||
                   (closest - p1).sqrMagnitude <= rSq;
        }

        // ---------------------------------------------------------------------
        // AABB vs Cylinder
        // ---------------------------------------------------------------------
        /// <summary>
        /// Tests whether an AABB overlaps a cylinder (approximate).
        /// </summary>
        /// <param name="a">The axis-aligned bounding box.</param>
        /// <param name="cy">The cylinder.</param>
        /// <returns><c>true</c> if the shapes overlap.</returns>
        public static bool Intersects(Bounds a, Cylinder cy)
        {
            Vector3 p0 = cy.P0;
            Vector3 p1 = cy.P1;
            float r = cy.Radius;

            Vector3 closest = SegmentMath.ClosestPointOnSegmentToAABB(p0, p1, a.min, a.max);
            float rSq = r * r;

            return (closest - p0).sqrMagnitude <= rSq ||
                   (closest - p1).sqrMagnitude <= rSq;
        }

        // ---------------------------------------------------------------------
        // AABB vs Cone
        // ---------------------------------------------------------------------
        /// <summary>
        /// Tests whether an AABB overlaps a cone, using the box point closest to the apex.
        /// </summary>
        /// <param name="a">The axis-aligned bounding box.</param>
        /// <param name="cone">The cone.</param>
        /// <returns><c>true</c> if the shapes overlap.</returns>
        public static bool Intersects(Bounds a, Cone cone)
        {
            Vector3 apex = cone.Apex;
            Vector3 axis = cone.Axis;      // Must be normalized
            float height = cone.Height;
            float radius = cone.Radius;

            // 1. Closest point on AABB to the cone apex
            Vector3 closest = SegmentMath.ClosestPointOnAABB(apex, a.min, a.max);

            // 2. Vector from apex to closest point
            Vector3 v = closest - apex;

            // 3. Height along cone axis
            float h = Vector3.Dot(v, axis);

            // Outside vertical range
            if (h < 0f || h > height)
                return false;

            // 4. Radius at this height (linear interpolation)
            float t = h / height;
            float rAtH = radius * t;

            // 5. Radial distance from axis
            Vector3 radial = v - axis * h;
            return radial.sqrMagnitude <= rAtH * rAtH;
        }

        // ---------------------------------------------------------------------
        // AABB vs Segment
        // ---------------------------------------------------------------------
        /// <summary>
        /// Tests whether a segment intersects an AABB using the slab method.
        /// </summary>
        /// <param name="a">The axis-aligned bounding box.</param>
        /// <param name="s0">The segment start point.</param>
        /// <param name="s1">The segment end point.</param>
        /// <returns><c>true</c> if the segment touches the box.</returns>
        public static bool Intersects(Bounds a, Vector3 s0, Vector3 s1)
        {
            Vector3 dir = s1 - s0;

            float invX = Mathf.Abs(dir.x) < 1e-12f ? float.PositiveInfinity : 1f / dir.x;
            float invY = Mathf.Abs(dir.y) < 1e-12f ? float.PositiveInfinity : 1f / dir.y;
            float invZ = Mathf.Abs(dir.z) < 1e-12f ? float.PositiveInfinity : 1f / dir.z;

            Vector3 min = a.min;
            Vector3 max = a.max;

            float t1x = (min.x - s0.x) * invX;
            float t2x = (max.x - s0.x) * invX;
            float t1y = (min.y - s0.y) * invY;
            float t2y = (max.y - s0.y) * invY;
            float t1z = (min.z - s0.z) * invZ;
            float t2z = (max.z - s0.z) * invZ;

            float tmin = Mathf.Max(
                Mathf.Min(t1x, t2x),
                Mathf.Min(t1y, t2y),
                Mathf.Min(t1z, t2z));

            float tmax = Mathf.Min(
                Mathf.Max(t1x, t2x),
                Mathf.Max(t1y, t2y),
                Mathf.Max(t1z, t2z));

            return tmax >= 0f && tmin <= 1f && tmin <= tmax;
        }

        // ---------------------------------------------------------------------
        // AABB vs Triangle
        // ---------------------------------------------------------------------
        /// <summary>
        /// Tests whether a triangle overlaps an AABB using the separating axis theorem.
        /// </summary>
        /// <param name="a">The axis-aligned bounding box.</param>
        /// <param name="A">Triangle vertex A.</param>
        /// <param name="B">Triangle vertex B.</param>
        /// <param name="C">Triangle vertex C.</param>
        /// <returns><c>true</c> if the shapes overlap.</returns>
        public static bool Intersects(Bounds a, Vector3 A, Vector3 B, Vector3 C)
        {
            Vector3 triMin = Vector3.Min(A, Vector3.Min(B, C));
            Vector3 triMax = Vector3.Max(A, Vector3.Max(B, C));

            if (triMax.x < a.min.x || triMin.x > a.max.x) return false;
            if (triMax.y < a.min.y || triMin.y > a.max.y) return false;
            if (triMax.z < a.min.z || triMin.z > a.max.z) return false;

            Vector3 center = a.center;
            Vector3 ext = a.extents;

            Vector3 aL = A - center;
            Vector3 bL = B - center;
            Vector3 cL = C - center;

            float min, max;

            TriangleMath.ProjectTriangleOnAxis(aL, bL, cL, Vector3.right, out min, out max);
            if (max < -ext.x || min > ext.x) return false;

            TriangleMath.ProjectTriangleOnAxis(aL, bL, cL, Vector3.up, out min, out max);
            if (max < -ext.y || min > ext.y) return false;

            TriangleMath.ProjectTriangleOnAxis(aL, bL, cL, Vector3.forward, out min, out max);
            if (max < -ext.z || min > ext.z) return false;

            Vector3 n = Vector3.Cross(bL - aL, cL - aL);
            if (n.sqrMagnitude > 1e-12f)
            {
                n.Normalize();
                TriangleMath.ProjectTriangleOnAxis(aL, bL, cL, n, out min, out max);

                float r = Mathf.Abs(n.x) * ext.x +
                          Mathf.Abs(n.y) * ext.y +
                          Mathf.Abs(n.z) * ext.z;

                if (max < -r || min > r) return false;
            }

            Vector3 e0 = bL - aL;
            Vector3 e1 = cL - bL;
            Vector3 e2 = aL - cL;

            if (!TestEdgeAxesAabb(e0, aL, bL, cL, ext)) return false;
            if (!TestEdgeAxesAabb(e1, aL, bL, cL, ext)) return false;
            if (!TestEdgeAxesAabb(e2, aL, bL, cL, ext)) return false;

            return true;
        }

        private static bool TestEdgeAxesAabb(
            Vector3 edge,
            Vector3 a, Vector3 b, Vector3 c,
            Vector3 ext)
        {
            if (!TestSingleEdgeAxis(edge, Vector3.right, a, b, c, ext)) return false;
            if (!TestSingleEdgeAxis(edge, Vector3.up, a, b, c, ext)) return false;
            if (!TestSingleEdgeAxis(edge, Vector3.forward, a, b, c, ext)) return false;

            return true;
        }

        private static bool TestSingleEdgeAxis(
            Vector3 edge,
            Vector3 axisBase,
            Vector3 a, Vector3 b, Vector3 c,
            Vector3 ext)
        {
            Vector3 axis = Vector3.Cross(edge, axisBase);
            if (axis.sqrMagnitude < 1e-12f)
                return true;

            axis.Normalize();

            TriangleMath.ProjectTriangleOnAxis(a, b, c, axis, out float min, out float max);

            float r = Mathf.Abs(axis.x) * ext.x +
                      Mathf.Abs(axis.y) * ext.y +
                      Mathf.Abs(axis.z) * ext.z;

            return !(max < -r || min > r);
        }

        // ---------------------------------------------------------------------
        // AABB vs OBB
        // ---------------------------------------------------------------------
        /// <summary>
        /// Tests whether an AABB overlaps an OBB by treating the AABB as an axis-aligned OBB.
        /// </summary>
        /// <param name="a">The axis-aligned bounding box.</param>
        /// <param name="b">The oriented bounding box.</param>
        /// <returns><c>true</c> if the shapes overlap.</returns>
        public static bool Intersects(Bounds a, OBB b)
        {
            OBB aa = new OBB(
                a.center,
                a.extents,
                Vector3.right,
                Vector3.up,
                Vector3.forward
            );

            return OBBCollisionExtensions.Intersects(aa, b);
        }
    }
}
