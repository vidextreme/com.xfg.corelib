// Copyright (c) 2025 John David Uy
// Licensed under the MIT License. See LICENSE for details.
// ------------------------------------------------------------------------------
// OBBUtility
// ------------------------------------------------------------------------------
// Construction and validation helpers for oriented bounding boxes (OBB).
//
// Provides:
// - FromBounds
// - FromCenterSizeRotation
// - FromTransform (with optional non-uniform scale support)
// - NormalizeAxes
// - Validate
//
// All methods are deterministic, allocation-free, and suitable for Burst jobs.
// ------------------------------------------------------------------------------

using UnityEngine;

namespace XFG.Math.Shape
{
    public static class OBBUtility
    {
        // ==========================================================================
        // FROM BOUNDS (AABB -> OBB)
        // ==========================================================================
        /// <summary>
        /// Creates an OBB from a Unity Bounds (AABB).
        /// Orientation is identity; extents and center match the AABB.
        /// </summary>
        public static OBB FromBounds(Bounds b)
        {
            return new OBB(
                b.center,
                b.extents,
                Vector3.right,
                Vector3.up,
                Vector3.forward
            );
        }

        // ==========================================================================
        // FROM CENTER + SIZE + ROTATION
        // ==========================================================================
        /// <summary>
        /// Creates an OBB from center, size, and rotation.
        /// Size is full size; extents are half-size.
        /// </summary>
        public static OBB FromCenterSizeRotation(Vector3 center, Vector3 size, Quaternion rotation)
        {
            Vector3 ext = size * 0.5f;

            return new OBB(
                center,
                ext,
                rotation * Vector3.right,
                rotation * Vector3.up,
                rotation * Vector3.forward
            );
        }

        // ==========================================================================
        // FROM TRANSFORM
        // ==========================================================================
        /// <summary>
        /// Creates an OBB from a Transform and extents.
        /// Supports non-uniform scale.
        /// </summary>
        public static OBB FromTransform(Transform t, Vector3 extents)
        {
            Vector3 scaledExtents = new Vector3(
                extents.x * Mathf.Abs(t.localScale.x),
                extents.y * Mathf.Abs(t.localScale.y),
                extents.z * Mathf.Abs(t.localScale.z)
            );

            return new OBB(
                t.position,
                scaledExtents,
                t.right,
                t.up,
                t.forward
            );
        }

        // ==========================================================================
        // NORMALIZE AXES
        // ==========================================================================
        /// <summary>
        /// Ensures the OBB axes are orthonormal.
        /// Useful after manual axis manipulation.
        /// </summary>
        public static void NormalizeAxes(ref OBB b)
        {
            b.Right = b.Right.normalized;
            b.Up = b.Up.normalized;
            b.Forward = b.Forward.normalized;
        }

        // ==========================================================================
        // VALIDATION
        // ==========================================================================
        /// <summary>
        /// Returns true if the OBB is valid:
        /// - Extents are non-negative
        /// - Axes are non-zero
        /// </summary>
        public static bool IsValid(OBB b)
        {
            if (b.Extents.x < 0f || b.Extents.y < 0f || b.Extents.z < 0f)
                return false;

            if (b.Right.sqrMagnitude < 1e-6f) return false;
            if (b.Up.sqrMagnitude < 1e-6f) return false;
            if (b.Forward.sqrMagnitude < 1e-6f) return false;

            return true;
        }
    }
}
