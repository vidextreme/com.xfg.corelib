using UnityEngine;

namespace XFG.Math
{
    // ------------------------------------------------------------------------------
    // SegmentMath
    // ------------------------------------------------------------------------------
    // Low-level geometric utilities for computing distances and closest points
    // between line segments in 3D space.
    //
    // These functions form the backbone of all capsule-capsule, capsule-cylinder,
    // and cylinder-cylinder collision tests. They are written to be:
    //
    // - Allocation-free
    // - Burst-friendly
    // - Deterministic
    // - Symmetric and numerically stable
    //
    // Segment-segment distance is the most general primitive used for all
    // "swept-radius" shapes (capsules, cylinders). The math handles:
    //
    // - Parallel segments
    // - Degenerate segments (zero-length -> points)
    // - Overlapping or skewed segments
    // - Proper clamping to finite segment extents
    //
    // This class intentionally contains no shape-specific logic.
    // It is purely geometric and safe to use anywhere in XFG.Math.
    // ------------------------------------------------------------------------------

    public static class SegmentMath
    {
        // Returns squared distance between two finite 3D line segments.
        // This is the core primitive used for capsule and cylinder intersection.
        public static float SegmentSegmentDistanceSq(
            Vector3 p1, Vector3 q1,
            Vector3 p2, Vector3 q2)
        {
            Vector3 d1 = q1 - p1;
            Vector3 d2 = q2 - p2;
            Vector3 r = p1 - p2;

            float a = Vector3.Dot(d1, d1);
            float e = Vector3.Dot(d2, d2);
            float f = Vector3.Dot(d2, r);

            float s, t;

            if (a <= Mathf.Epsilon && e <= Mathf.Epsilon)
                return (p1 - p2).sqrMagnitude;

            if (a <= Mathf.Epsilon)
            {
                s = 0f;
                t = Mathf.Clamp01(f / e);
            }
            else
            {
                float c = Vector3.Dot(d1, r);

                if (e <= Mathf.Epsilon)
                {
                    t = 0f;
                    s = Mathf.Clamp01(-c / a);
                }
                else
                {
                    float b = Vector3.Dot(d1, d2);
                    float denom = a * e - b * b;

                    s = denom != 0f ? Mathf.Clamp01((b * f - c * e) / denom) : 0f;

                    float tNom = b * s + f;

                    if (tNom < 0f)
                    {
                        t = 0f;
                        s = Mathf.Clamp01(-c / a);
                    }
                    else if (tNom > e)
                    {
                        t = 1f;
                        s = Mathf.Clamp01((b - c) / a);
                    }
                    else
                    {
                        t = tNom / e;
                    }
                }
            }

            Vector3 c1 = p1 + d1 * s;
            Vector3 c2 = p2 + d2 * t;

            return (c1 - c2).sqrMagnitude;
        }

        // Returns closest point on segment AB to point P.
        // Used for sphere-capsule and sphere-cylinder intersection.
        public static Vector3 ClosestPointOnSegment(Vector3 a, Vector3 b, Vector3 p)
        {
            Vector3 ab = b - a;
            float abLenSq = Vector3.Dot(ab, ab);

            if (abLenSq <= Mathf.Epsilon)
                return a;

            float t = Vector3.Dot(p - a, ab) / abLenSq;
            t = Mathf.Clamp01(t);
            return a + ab * t;
        }

        // Returns true if two segments intersect within a small epsilon.
        // Treats segments as infinitely thin.
        public static bool SegmentsIntersect(
            Vector3 p1, Vector3 q1,
            Vector3 p2, Vector3 q2,
            float epsilon = 1e-5f)
        {
            float distSq = SegmentSegmentDistanceSq(p1, q1, p2, q2);
            return distSq <= epsilon * epsilon;
        }

        // Returns true if two segments come within the given radius of each other.
        // Useful for thick-line or tube-like intersection tests.
        public static bool SegmentsIntersectWithinRadius(
            Vector3 p1, Vector3 q1,
            Vector3 p2, Vector3 q2,
            float radius)
        {
            float distSq = SegmentSegmentDistanceSq(p1, q1, p2, q2);
            return distSq <= radius * radius;
        }
    }
}
