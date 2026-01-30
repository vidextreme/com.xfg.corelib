// Copyright (c) 2025 John David Uy
// Licensed under the MIT License. See LICENSE for details.
// ------------------------------------------------------------------------------
// OBB
// ------------------------------------------------------------------------------
// Oriented Bounding Box (OBB) primitive.
//
// Fields:
// - Center   : world-space center of the box
// - Extents  : half-size along each local axis
// - Right    : normalized local X axis
// - Up       : normalized local Y axis
// - Forward  : normalized local Z axis
//
// Notes:
// - Axes must remain orthonormal for correct behavior.
// - Extents must be non-negative.
// - Suitable for Burst and deterministic math.
// ------------------------------------------------------------------------------

using UnityEngine;

namespace XFG.Math.Shape
{
    public struct OBB
    {
        public Vector3 Center;
        public Vector3 Extents;

        public Vector3 Right;
        public Vector3 Up;
        public Vector3 Forward;

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
