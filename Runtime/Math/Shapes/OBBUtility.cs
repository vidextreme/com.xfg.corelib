// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

using UnityEngine;

namespace XFG.Math.Shape
{
    /// <summary>
    /// Construction and validation helpers for oriented bounding boxes (OBB).
    /// </summary>
    /// <remarks>
    /// Provides:
    /// <list type="bullet">
    /// <item>FromBounds</item>
    /// <item>FromCenterSizeRotation</item>
    /// <item>FromTransform (with optional non-uniform scale support)</item>
    /// <item>NormalizeAxes</item>
    /// <item>IsValid</item>
    /// </list>
    /// All methods are deterministic, allocation-free, and suitable for Burst jobs.
    /// </remarks>
    public static class OBBUtility
    {
        // ---------------------------------------------------------------------
        // From Bounds (AABB -> OBB)
        // ---------------------------------------------------------------------
        /// <summary>
        /// Creates an OBB from a Unity Bounds (AABB).
        /// Orientation is identity; extents and center match the AABB.
        /// </summary>
        /// <param name="b">The source bounds.</param>
        /// <returns>The equivalent axis-aligned OBB.</returns>
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

        // ---------------------------------------------------------------------
        // From Center + Size + Rotation
        // ---------------------------------------------------------------------
        /// <summary>
        /// Creates an OBB from center, size, and rotation.
        /// Size is full size; extents are half-size.
        /// </summary>
        /// <param name="center">World-space center.</param>
        /// <param name="size">Full size of the box along each local axis.</param>
        /// <param name="rotation">Orientation of the box.</param>
        /// <returns>The resulting OBB.</returns>
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

        // ---------------------------------------------------------------------
        // From Transform
        // ---------------------------------------------------------------------
        /// <summary>
        /// Creates an OBB from a Transform and extents.
        /// Supports non-uniform scale.
        /// </summary>
        /// <param name="t">The transform providing position, orientation and local scale.</param>
        /// <param name="extents">Unscaled half-size along each local axis.</param>
        /// <returns>The resulting OBB.</returns>
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

        // ---------------------------------------------------------------------
        // Normalize Axes
        // ---------------------------------------------------------------------
        /// <summary>
        /// Normalizes the OBB axes to unit length (does not re-orthogonalize them).
        /// Useful after manual axis manipulation.
        /// </summary>
        /// <param name="b">The box to modify in place.</param>
        public static void NormalizeAxes(ref OBB b)
        {
            b.Right = b.Right.normalized;
            b.Up = b.Up.normalized;
            b.Forward = b.Forward.normalized;
        }

        // ---------------------------------------------------------------------
        // Validation
        // ---------------------------------------------------------------------
        /// <summary>
        /// Returns true if the OBB is valid.
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item>Extents are non-negative</item>
        /// <item>Axes are non-zero</item>
        /// </list>
        /// </remarks>
        /// <param name="b">The box to validate.</param>
        /// <returns><c>true</c> if the box is valid.</returns>
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
