using UnityEngine;

namespace XFG.Math
{
    // ------------------------------------------------------------------------------
    // RayMath
    // ------------------------------------------------------------------------------
    // Pure geometric utilities for ray operations.
    // No shape-specific logic lives here.
    // ------------------------------------------------------------------------------

    public static class RayMath
    {
        // ============================================================
        // RAY vs TRIANGLE
        // ============================================================
        #region RayTriangle

        /// <summary>
        /// Returns true if a ray intersects a triangle. Outputs hit t.
        /// Uses the Moller-Trumbore algorithm.
        /// </summary>
        public static bool RayIntersectsTriangle(
            Vector3 R0, Vector3 Rd,
            Vector3 v0, Vector3 v1, Vector3 v2,
            out float t)
        {
            t = 0f;

            Vector3 e1 = v1 - v0;
            Vector3 e2 = v2 - v0;

            Vector3 p = Vector3.Cross(Rd, e2);
            float det = Vector3.Dot(e1, p);

            if (Mathf.Abs(det) < 1e-6f)
                return false;

            float invDet = 1f / det;

            Vector3 tvec = R0 - v0;
            float u = Vector3.Dot(tvec, p) / invDet;
            if (u < 0f || u > 1f)
                return false;

            Vector3 q = Vector3.Cross(tvec, e1);
            float v = Vector3.Dot(Rd, q) / invDet;
            if (v < 0f || u + v > 1f)
                return false;

            float tHit = Vector3.Dot(e2, q) / invDet;
            if (tHit < 0f)
                return false;

            t = tHit;
            return true;
        }

        #endregion
    }
}
