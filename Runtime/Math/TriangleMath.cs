// Copyright (c) 2025 John David Uy
// Licensed under the MIT License. See LICENSE for details.
// ------------------------------------------------------------------------------
// TriangleMath
// ------------------------------------------------------------------------------
// Low-level math utilities for triangle operations:
// - Closest point on triangle to point
// - Closest point on triangle to segment
// - Barycentric helpers
//
// All methods are deterministic, allocation-free, and suitable for Burst jobs.
// ------------------------------------------------------------------------------

using UnityEngine;

namespace XFG.Math
{
    public static class TriangleMath
    {
        // ==============================================================================
        // POINT TO TRIANGLE
        // ==============================================================================
        #region PointToTriangle

        /// <summary>
        /// Returns the closest point on triangle ABC to point P.
        /// </summary>
        public static Vector3 ClosestPointOnTriangle(Vector3 P, Vector3 A, Vector3 B, Vector3 C)
        {
            Vector3 AB = B - A;
            Vector3 AC = C - A;
            Vector3 AP = P - A;

            float d1 = Vector3.Dot(AB, AP);
            float d2 = Vector3.Dot(AC, AP);

            if (d1 <= 0f && d2 <= 0f)
                return A;

            Vector3 BP = P - B;
            float d3 = Vector3.Dot(AB, BP);
            float d4 = Vector3.Dot(AC, BP);

            if (d3 >= 0f && d4 <= d3)
                return B;

            float vc = d1 * d4 - d3 * d2;
            if (vc <= 0f && d1 >= 0f && d3 <= 0f)
            {
                float v = d1 / (d1 - d3);
                return A + AB * v;
            }

            Vector3 CP = P - C;
            float d5 = Vector3.Dot(AB, CP);
            float d6 = Vector3.Dot(AC, CP);

            if (d6 >= 0f && d5 <= d6)
                return C;

            float vb = d5 * d2 - d1 * d6;
            if (vb <= 0f && d2 >= 0f && d6 <= 0f)
            {
                float w = d2 / (d2 - d6);
                return A + AC * w;
            }

            float va = d3 * d6 - d5 * d4;
            if (va <= 0f && (d4 - d3) >= 0f && (d5 - d6) >= 0f)
            {
                float w = (d4 - d3) / ((d4 - d3) + (d5 - d6));
                return B + (C - B) * w;
            }

            float denom = 1f / (va + vb + vc);
            float v2 = vb * denom;
            float w2 = vc * denom;

            return A + AB * v2 + AC * w2;
        }

        #endregion

        // ==============================================================================
        // SEGMENT TO TRIANGLE
        // ==============================================================================
        #region SegmentToTriangle

        /// <summary>
        /// Returns the closest point on triangle ABC to segment PQ.
        /// </summary>
        public static Vector3 ClosestPointOnTriangleToSegment(Vector3 A, Vector3 B, Vector3 C, Vector3 P, Vector3 Q)
        {
            Vector3 pA = ClosestPointOnTriangle(P, A, B, C);
            Vector3 pB = ClosestPointOnTriangle(Q, A, B, C);

            float dA = (pA - P).sqrMagnitude;
            float dB = (pB - Q).sqrMagnitude;

            return dA < dB ? pA : pB;
        }

        #endregion
    }
}
