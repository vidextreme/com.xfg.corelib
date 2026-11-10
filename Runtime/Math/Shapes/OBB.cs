// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

using UnityEngine;

namespace XFG.Math.Shape
{
    /// <summary>
    /// Oriented Bounding Box (OBB) primitive.
    /// </summary>
    /// <remarks>
    /// Notes:
    /// <list type="bullet">
    /// <item>Axes must remain orthonormal for correct behavior.</item>
    /// <item>Extents must be non-negative.</item>
    /// <item>Suitable for Burst and deterministic math.</item>
    /// </list>
    /// </remarks>
    public struct OBB
    {
        /// <summary>
        /// World-space center of the box.
        /// </summary>
        public Vector3 Center;

        /// <summary>
        /// Half-size along each local axis.
        /// </summary>
        public Vector3 Extents;

        /// <summary>
        /// Normalized local X axis.
        /// </summary>
        public Vector3 Right;

        /// <summary>
        /// Normalized local Y axis.
        /// </summary>
        public Vector3 Up;

        /// <summary>
        /// Normalized local Z axis.
        /// </summary>
        public Vector3 Forward;

        /// <summary>
        /// Creates an OBB; the axis vectors are normalized.
        /// </summary>
        /// <param name="center">World-space center of the box.</param>
        /// <param name="extents">Half-size along each local axis.</param>
        /// <param name="right">Local X axis.</param>
        /// <param name="up">Local Y axis.</param>
        /// <param name="forward">Local Z axis.</param>
        public OBB(Vector3 center, Vector3 extents, Vector3 right, Vector3 up, Vector3 forward)
        {
            Center = center;
            Extents = extents;

            Right = right.normalized;
            Up = up.normalized;
            Forward = forward.normalized;
        }

        /// <summary>
        /// Creates an OBB from a transform and extents.
        /// Assumes the transform's rotation defines the box orientation.
        /// </summary>
        /// <param name="t">The transform providing position and orientation.</param>
        /// <param name="extents">Half-size along each local axis.</param>
        /// <returns>The resulting OBB.</returns>
        public static OBB FromTransform(Transform t, Vector3 extents)
        {
            return new OBB(
                t.position,
                extents,
                t.right,
                t.up,
                t.forward
            );
        }

        /// <summary>
        /// Returns the 8 world-space corners of the OBB.
        /// Useful for debugging, visualization, and SAT collision tests.
        /// </summary>
        /// <param name="outCorners">Destination array; must hold at least 8 elements.</param>
        public void GetCorners(Vector3[] outCorners)
        {
            Vector3 ex = Right * Extents.x;
            Vector3 ey = Up * Extents.y;
            Vector3 ez = Forward * Extents.z;

            outCorners[0] = Center + ex + ey + ez;
            outCorners[1] = Center + ex + ey - ez;
            outCorners[2] = Center + ex - ey + ez;
            outCorners[3] = Center + ex - ey - ez;

            outCorners[4] = Center - ex + ey + ez;
            outCorners[5] = Center - ex + ey - ez;
            outCorners[6] = Center - ex - ey + ez;
            outCorners[7] = Center - ex - ey - ez;
        }
    }
}
