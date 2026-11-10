// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

using UnityEngine;

namespace XFG.Math
{
    /// <summary>
    /// Pure geometric utilities for infinite line operations.
    /// </summary>
    /// <remarks>
    /// No shape-specific logic lives here.
    /// </remarks>
    public static class LineMath
    {
        // ---------------------------------------------------------------------
        // Line vs Line
        // ---------------------------------------------------------------------
        #region LineLine

        /// <summary>
        /// Returns squared distance between two infinite lines.
        /// </summary>
        /// <param name="p1">A point on the first line.</param>
        /// <param name="d1">The direction of the first line.</param>
        /// <param name="p2">A point on the second line.</param>
        /// <param name="d2">The direction of the second line.</param>
        /// <returns>The squared distance between the closest points of the two lines.</returns>
        public static float LineLineDistanceSq(
            Vector3 p1, Vector3 d1,
            Vector3 p2, Vector3 d2)
        {
            Vector3 r = p1 - p2;

            float a = Vector3.Dot(d1, d1);
            float e = Vector3.Dot(d2, d2);
            float b = Vector3.Dot(d1, d2);
            float c = Vector3.Dot(d1, r);
            float f = Vector3.Dot(d2, r);

            float denom = a * e - b * b;

            float s = 0f;
            float t = 0f;

            if (denom != 0f)
                s = (b * f - c * e) / denom;

            t = (b * s + f) / e;

            Vector3 c1 = p1 + d1 * s;
            Vector3 c2 = p2 + d2 * t;

            return (c1 - c2).sqrMagnitude;
        }

        #endregion


        // ---------------------------------------------------------------------
        // Line vs Segment
        // ---------------------------------------------------------------------
        #region LineSegment

        /// <summary>
        /// Returns squared distance between an infinite line and a finite segment.
        /// </summary>
        /// <remarks>
        /// The line is approximated by a long segment starting at <paramref name="L0"/>.
        /// </remarks>
        /// <param name="L0">A point on the line.</param>
        /// <param name="Ld">The direction of the line.</param>
        /// <param name="S0">The start point of the segment.</param>
        /// <param name="S1">The end point of the segment.</param>
        /// <returns>The squared distance between the line and the segment.</returns>
        public static float LineSegmentDistanceSq(
            Vector3 L0, Vector3 Ld,
            Vector3 S0, Vector3 S1)
        {
            Vector3 L1 = L0 + Ld * 1e6f;

            return SegmentMath.SegmentSegmentDistanceSq(
                L0, L1,
                S0, S1
            );
        }

        #endregion
    }
}
