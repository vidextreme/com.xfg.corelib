// ------------------------------------------------------------------------------
// TriangleMath
// ------------------------------------------------------------------------------
// Core math utilities for triangles.
//
// Provides:
// - Common helpers (normals, projections, interval tests)
// - Barycentric coordinates and interpolation
// - Area, normal, centroid, incenter, circumcenter
// - Inradius, circumradius, aspect ratio, quality metrics
// - Degeneracy checks
// - Closest point queries (point, segment, ray, line)
//
// All methods are deterministic, allocation-free, and suitable for Burst jobs.
// ------------------------------------------------------------------------------

using UnityEngine;

namespace XFG.Math
{
    public static class TriangleMath
    {
        // ==============================================================================
        // COMMON
        // ==============================================================================
        #region Common

        /// <summary>
        /// Returns the unnormalized triangle normal.
        /// </summary>
        /// <param name="a">Triangle vertex A.</param>
        /// <param name="b">Triangle vertex B.</param>
        /// <param name="c">Triangle vertex C.</param>
        /// <returns>Unnormalized normal vector.</returns>
        public static Vector3 NormalUnnormalized(Vector3 a, Vector3 b, Vector3 c)
        {
            return Vector3.Cross(b - a, c - a);
        }

        /// <summary>
        /// Returns the normalized triangle normal, or Vector3.zero if degenerate.
        /// </summary>
        /// <param name="a">Triangle vertex A.</param>
        /// <param name="b">Triangle vertex B.</param>
        /// <param name="c">Triangle vertex C.</param>
        /// <returns>Normalized normal vector.</returns>
        public static Vector3 Normal(Vector3 a, Vector3 b, Vector3 c)
        {
            Vector3 n = Vector3.Cross(b - a, c - a);
            float mag = n.magnitude;
            return mag < 1e-12f ? Vector3.zero : n / mag;
        }

        /// <summary>
        /// Projects triangle vertices onto an axis.
        /// </summary>
        /// <param name="a">Triangle vertex A.</param>
        /// <param name="b">Triangle vertex B.</param>
        /// <param name="c">Triangle vertex C.</param>
        /// <param name="axis">Axis to project onto.</param>
        /// <param name="min">Minimum projection value.</param>
        /// <param name="max">Maximum projection value.</param>
        public static void ProjectTriangleOnAxis(
            Vector3 a, Vector3 b, Vector3 c,
            Vector3 axis,
            out float min, out float max)
        {
            float da = Vector3.Dot(a, axis);
            float db = Vector3.Dot(b, axis);
            float dc = Vector3.Dot(c, axis);

            min = da;
            max = da;

            if (db < min) min = db;
            if (db > max) max = db;

            if (dc < min) min = dc;
            if (dc > max) max = dc;
        }

        /// <summary>
        /// Returns true if 1D intervals overlap.
        /// </summary>
        /// <param name="minA">Minimum of interval A.</param>
        /// <param name="maxA">Maximum of interval A.</param>
        /// <param name="minB">Minimum of interval B.</param>
        /// <param name="maxB">Maximum of interval B.</param>
        /// <returns>True if intervals overlap.</returns>
        public static bool Overlaps1D(float minA, float maxA, float minB, float maxB)
        {
            return !(maxA < minB || maxB < minA);
        }

        #endregion

        // ==============================================================================
        // BARYCENTRIC
        // ==============================================================================
        #region Barycentric

        /// <summary>
        /// Computes barycentric coordinates of point p relative to triangle (a, b, c).
        /// </summary>
        /// <param name="p">Point to evaluate.</param>
        /// <param name="a">Triangle vertex A.</param>
        /// <param name="b">Triangle vertex B.</param>
        /// <param name="c">Triangle vertex C.</param>
        /// <returns>Barycentric coordinates (u, v, w).</returns>
        public static Vector3 Barycentric(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
        {
            Vector3 v0 = b - a;
            Vector3 v1 = c - a;
            Vector3 v2 = p - a;

            float d00 = Vector3.Dot(v0, v0);
            float d01 = Vector3.Dot(v0, v1);
            float d11 = Vector3.Dot(v1, v1);
            float d20 = Vector3.Dot(v2, v0);
            float d21 = Vector3.Dot(v2, v1);

            float denom = d00 * d11 - d01 * d01;
            if (Mathf.Abs(denom) < 1e-12f)
                return new Vector3(1f, 0f, 0f);

            float inv = 1f / denom;
            float v = (d11 * d20 - d01 * d21) * inv;
            float w = (d00 * d21 - d01 * d20) * inv;
            float u = 1f - v - w;

            return new Vector3(u, v, w);
        }

        /// <summary>
        /// Returns true if barycentric coordinates lie inside the triangle.
        /// </summary>
        /// <param name="bary">Barycentric coordinates.</param>
        /// <returns>True if inside or on edges.</returns>
        public static bool IsInsideTriangle(Vector3 bary)
        {
            return bary.x >= 0f && bary.y >= 0f && bary.z >= 0f &&
                   bary.x <= 1f && bary.y <= 1f && bary.z <= 1f;
        }

        /// <summary>
        /// Interpolates a point using barycentric coordinates.
        /// </summary>
        /// <param name="a">Triangle vertex A.</param>
        /// <param name="b">Triangle vertex B.</param>
        /// <param name="c">Triangle vertex C.</param>
        /// <param name="bary">Barycentric coordinates.</param>
        /// <returns>Interpolated point.</returns>
        public static Vector3 Interpolate(Vector3 a, Vector3 b, Vector3 c, Vector3 bary)
        {
            return a * bary.x + b * bary.y + c * bary.z;
        }

        #endregion

        // ==============================================================================
        // GEOMETRY
        // ==============================================================================
        #region Geometry

        /// <summary>
        /// Returns the unsigned area of the triangle.
        /// </summary>
        /// <param name="a">Triangle vertex A.</param>
        /// <param name="b">Triangle vertex B.</param>
        /// <param name="c">Triangle vertex C.</param>
        /// <returns>Triangle area.</returns>
        public static float Area(Vector3 a, Vector3 b, Vector3 c)
        {
            return 0.5f * Vector3.Cross(b - a, c - a).magnitude;
        }

        /// <summary>
        /// Returns the signed area of the triangle projected onto a plane with normal n.
        /// </summary>
        /// <param name="a">Triangle vertex A.</param>
        /// <param name="b">Triangle vertex B.</param>
        /// <param name="c">Triangle vertex C.</param>
        /// <param name="n">Reference normal.</param>
        /// <returns>Signed area.</returns>
        public static float SignedArea(Vector3 a, Vector3 b, Vector3 c, Vector3 n)
        {
            Vector3 cross = Vector3.Cross(b - a, c - a);
            float sign = Mathf.Sign(Vector3.Dot(cross, n));
            return 0.5f * cross.magnitude * sign;
        }

        /// <summary>
        /// Returns the centroid of the triangle.
        /// </summary>
        /// <param name="a">Triangle vertex A.</param>
        /// <param name="b">Triangle vertex B.</param>
        /// <param name="c">Triangle vertex C.</param>
        /// <returns>Centroid position.</returns>
        public static Vector3 Centroid(Vector3 a, Vector3 b, Vector3 c)
        {
            return (a + b + c) / 3f;
        }

        #endregion

        // ==============================================================================
        // CENTERS & RADII
        // ==============================================================================
        #region CentersAndRadii

        /// <summary>
        /// Returns the incenter of the triangle.
        /// </summary>
        /// <param name="a">Triangle vertex A.</param>
        /// <param name="b">Triangle vertex B.</param>
        /// <param name="c">Triangle vertex C.</param>
        /// <returns>Incenter position.</returns>
        public static Vector3 Incenter(Vector3 a, Vector3 b, Vector3 c)
        {
            float la = (b - c).magnitude;
            float lb = (a - c).magnitude;
            float lc = (a - b).magnitude;

            float sum = la + lb + lc;
            if (sum < 1e-12f)
                return a;

            return (a * la + b * lb + c * lc) / sum;
        }

        /// <summary>
        /// Returns the circumcenter of the triangle.
        /// </summary>
        /// <param name="a">Triangle vertex A.</param>
        /// <param name="b">Triangle vertex B.</param>
        /// <param name="c">Triangle vertex C.</param>
        /// <returns>Circumcenter position.</returns>
        public static Vector3 Circumcenter(Vector3 a, Vector3 b, Vector3 c)
        {
            Vector3 ab = b - a;
            Vector3 ac = c - a;
            Vector3 cross = Vector3.Cross(ab, ac);

            float denom = 2f * cross.sqrMagnitude;
            if (denom < 1e-12f)
                return a;

            float ab2 = ab.sqrMagnitude;
            float ac2 = ac.sqrMagnitude;

            Vector3 term1 = Vector3.Cross(cross, ab) * ac2;
            Vector3 term2 = Vector3.Cross(ac, cross) * ab2;

            return a + (term1 + term2) / denom;
        }

        /// <summary>
        /// Returns the inradius of the triangle.
        /// </summary>
        /// <param name="a">Triangle vertex A.</param>
        /// <param name="b">Triangle vertex B.</param>
        /// <param name="c">Triangle vertex C.</param>
        /// <returns>Inradius value.</returns>
        public static float Inradius(Vector3 a, Vector3 b, Vector3 c)
        {
            float area = Area(a, b, c);
            float la = (b - c).magnitude;
            float lb = (a - c).magnitude;
            float lc = (a - b).magnitude;

            float perimeter = la + lb + lc;
            return perimeter < 1e-12f ? 0f : (2f * area / perimeter);
        }

        /// <summary>
        /// Returns the circumradius of the triangle.
        /// </summary>
        /// <param name="a">Triangle vertex A.</param>
        /// <param name="b">Triangle vertex B.</param>
        /// <param name="c">Triangle vertex C.</param>
        /// <returns>Circumradius value.</returns>
        public static float Circumradius(Vector3 a, Vector3 b, Vector3 c)
        {
            float la = (b - c).magnitude;
            float lb = (a - c).magnitude;
            float lc = (a - b).magnitude;

            float area = Area(a, b, c);
            return area < 1e-12f ? 0f : (la * lb * lc) / (4f * area);
        }

        #endregion

        // ==============================================================================
        // QUALITY
        // ==============================================================================
        #region Quality

        /// <summary>
        /// Computes edge lengths of the triangle.
        /// </summary>
        /// <param name="a">Triangle vertex A.</param>
        /// <param name="b">Triangle vertex B.</param>
        /// <param name="c">Triangle vertex C.</param>
        /// <param name="ab">Length of AB.</param>
        /// <param name="bc">Length of BC.</param>
        /// <param name="ca">Length of CA.</param>
        public static void EdgeLengths(
            Vector3 a, Vector3 b, Vector3 c,
            out float ab, out float bc, out float ca)
        {
            ab = (b - a).magnitude;
            bc = (c - b).magnitude;
            ca = (a - c).magnitude;
        }

        /// <summary>
        /// Returns the perimeter of the triangle.
        /// </summary>
        /// <param name="a">Triangle vertex A.</param>
        /// <param name="b">Triangle vertex B.</param>
        /// <param name="c">Triangle vertex C.</param>
        /// <returns>Perimeter value.</returns>
        public static float Perimeter(Vector3 a, Vector3 b, Vector3 c)
        {
            float ab = (b - a).magnitude;
            float bc = (c - b).magnitude;
            float ca = (a - c).magnitude;
            return ab + bc + ca;
        }

        /// <summary>
        /// Returns the aspect ratio of the triangle (lower is better).
        /// </summary>
        /// <param name="a">Triangle vertex A.</param>
        /// <param name="b">Triangle vertex B.</param>
        /// <param name="c">Triangle vertex C.</param>
        /// <returns>Aspect ratio value.</returns>
        public static float AspectRatio(Vector3 a, Vector3 b, Vector3 c)
        {
            float area = Area(a, b, c);
            if (area < 1e-12f)
                return float.PositiveInfinity;

            float ab = (b - a).magnitude;
            float bc = (c - b).magnitude;
            float ca = (a - c).magnitude;

            float longest = Mathf.Max(ab, Mathf.Max(bc, ca));

            float hA = 2f * area / ab;
            float hB = 2f * area / bc;
            float hC = 2f * area / ca;

            float minAlt = Mathf.Min(hA, Mathf.Min(hB, hC));
            return minAlt < 1e-12f ? float.PositiveInfinity : longest / minAlt;
        }

        /// <summary>
        /// Returns a simple quality metric in [0, 1], where 1 is equilateral.
        /// </summary>
        /// <param name="a">Triangle vertex A.</param>
        /// <param name="b">Triangle vertex B.</param>
        /// <param name="c">Triangle vertex C.</param>
        /// <returns>Quality metric.</returns>
        public static float Quality(Vector3 a, Vector3 b, Vector3 c)
        {
            float ar = AspectRatio(a, b, c);
            return (!float.IsFinite(ar) || ar <= 0f) ? 0f : 1f / ar;
        }

        #endregion

        // ==============================================================================
        // DEGENERACY
        // ==============================================================================
        #region Degeneracy

        /// <summary>
        /// Returns true if the triangle is degenerate (area near zero).
        /// </summary>
        /// <param name="a">Triangle vertex A.</param>
        /// <param name="b">Triangle vertex B.</param>
        /// <param name="c">Triangle vertex C.</param>
        /// <param name="epsilon">Area threshold.</param>
        /// <returns>True if degenerate.</returns>
        public static bool IsDegenerate(Vector3 a, Vector3 b, Vector3 c, float epsilon = 1e-6f)
        {
            return Area(a, b, c) < epsilon;
        }

        #endregion
        // ==============================================================================
        // CLOSEST POINTS
        // ==============================================================================
        #region ClosestPoints

        /// <summary>
        /// Returns the closest point on the triangle to point p.
        /// Uses Ericson's branch-based algorithm.
        /// </summary>
        /// <param name="p">Point to evaluate.</param>
        /// <param name="a">Triangle vertex A.</param>
        /// <param name="b">Triangle vertex B.</param>
        /// <param name="c">Triangle vertex C.</param>
        /// <returns>Closest point on the triangle.</returns>
        public static Vector3 ClosestPointOnTriangle(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
        {
            Vector3 ab = b - a;
            Vector3 ac = c - a;
            Vector3 ap = p - a;

            float d1 = Vector3.Dot(ab, ap);
            float d2 = Vector3.Dot(ac, ap);

            if (d1 <= 0f && d2 <= 0f)
                return a;

            Vector3 bp = p - b;
            float d3 = Vector3.Dot(ab, bp);
            float d4 = Vector3.Dot(ac, bp);

            if (d3 >= 0f && d4 <= d3)
                return b;

            float vc = d1 * d4 - d3 * d2;
            if (vc <= 0f && d1 >= 0f && d3 <= 0f)
            {
                float v = d1 / (d1 - d3);
                return a + ab * v;
            }

            Vector3 cp = p - c;
            float d5 = Vector3.Dot(ab, cp);
            float d6 = Vector3.Dot(ac, cp);

            if (d6 >= 0f && d5 <= d6)
                return c;

            float vb = d5 * d2 - d1 * d6;
            if (vb <= 0f && d2 >= 0f && d6 <= 0f)
            {
                float w = d2 / (d2 - d6);
                return a + ac * w;
            }

            float va = d3 * d6 - d5 * d4;
            if (va <= 0f && (d4 - d3) >= 0f && (d5 - d6) >= 0f)
            {
                float w = (d4 - d3) / ((d4 - d3) + (d5 - d6));
                return b + (c - b) * w;
            }

            float denom = 1f / (va + vb + vc);
            float v2 = vb * denom;
            float w2 = vc * denom;
            float u2 = 1f - v2 - w2;

            return a * u2 + b * v2 + c * w2;
        }

        /// <summary>
        /// Returns the closest point on the triangle to a line segment.
        /// </summary>
        /// <param name="a">Triangle vertex A.</param>
        /// <param name="b">Triangle vertex B.</param>
        /// <param name="c">Triangle vertex C.</param>
        /// <param name="p0">Segment start point.</param>
        /// <param name="p1">Segment end point.</param>
        /// <returns>Closest point on the triangle.</returns>
        public static Vector3 ClosestPointOnTriangleToSegment(
            Vector3 a, Vector3 b, Vector3 c,
            Vector3 p0, Vector3 p1)
        {
            Vector3 q0 = ClosestPointOnTriangle(p0, a, b, c);
            Vector3 q1 = ClosestPointOnTriangle(p1, a, b, c);

            float d0 = (q0 - p0).sqrMagnitude;
            float d1 = (q1 - p1).sqrMagnitude;

            return d0 <= d1 ? q0 : q1;
        }

        /// <summary>
        /// Returns the closest point on the triangle to a ray.
        /// </summary>
        /// <param name="a">Triangle vertex A.</param>
        /// <param name="b">Triangle vertex B.</param>
        /// <param name="c">Triangle vertex C.</param>
        /// <param name="origin">Ray origin.</param>
        /// <param name="direction">Ray direction.</param>
        /// <returns>Closest point on the triangle.</returns>
        public static Vector3 ClosestPointOnTriangleToRay(
            Vector3 a, Vector3 b, Vector3 c,
            Vector3 origin, Vector3 direction)
        {
            Vector3 triClosest = ClosestPointOnTriangle(origin, a, b, c);
            return SegmentMath.ClosestPointOnRayToPoint(origin, direction, triClosest);
        }

        /// <summary>
        /// Returns the closest point on the triangle to a line.
        /// </summary>
        /// <param name="a">Triangle vertex A.</param>
        /// <param name="b">Triangle vertex B.</param>
        /// <param name="c">Triangle vertex C.</param>
        /// <param name="L0">A point on the line.</param>
        /// <param name="Ld">Line direction.</param>
        /// <returns>Closest point on the triangle.</returns>
        public static Vector3 ClosestPointOnTriangleToLine(
            Vector3 a, Vector3 b, Vector3 c,
            Vector3 L0, Vector3 Ld)
        {
            Vector3 triClosest = ClosestPointOnTriangle(L0, a, b, c);
            return SegmentMath.ClosestPointOnLineToPoint(L0, L0 + Ld, triClosest);
        }

        #endregion
    }
}


