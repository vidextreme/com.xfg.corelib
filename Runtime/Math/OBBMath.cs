// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

using UnityEngine;

namespace XFG.Math.Shape
{
    /// <summary>
    /// Math utilities for oriented bounding boxes.
    /// </summary>
    /// <remarks>
    /// Provides:
    /// <list type="bullet">
    /// <item>Closest point</item>
    /// <item>Distance to point</item>
    /// <item>Projection onto axes</item>
    /// </list>
    /// All repeated property getters are cached.
    /// </remarks>
    public static class OBBMath
    {
        // ---------------------------------------------------------------------
        // Closest Point
        // ---------------------------------------------------------------------
        /// <summary>
        /// Returns the point on or inside the box closest to the given point.
        /// </summary>
        /// <param name="b">The oriented bounding box.</param>
        /// <param name="point">The query point in world space.</param>
        /// <returns>The closest point on or inside the box.</returns>
        public static Vector3 ClosestPoint(OBB b, Vector3 point)
        {
            Vector3 c = b.Center;
            Vector3 e = b.Extents;
            Vector3 r = b.Right;
            Vector3 u = b.Up;
            Vector3 f = b.Forward;

            Vector3 d = point - c;

            float x = Mathf.Clamp(Vector3.Dot(d, r), -e.x, e.x);
            float y = Mathf.Clamp(Vector3.Dot(d, u), -e.y, e.y);
            float z = Mathf.Clamp(Vector3.Dot(d, f), -e.z, e.z);

            return c + r * x + u * y + f * z;
        }

        // ---------------------------------------------------------------------
        // Distance to Point
        // ---------------------------------------------------------------------
        /// <summary>
        /// Returns the distance from the box to a point (zero if the point is inside).
        /// </summary>
        /// <param name="b">The oriented bounding box.</param>
        /// <param name="point">The query point in world space.</param>
        /// <returns>The distance from the box surface to the point, or zero if inside.</returns>
        public static float DistanceToPoint(OBB b, Vector3 point)
        {
            Vector3 cp = ClosestPoint(b, point);
            return (cp - point).magnitude;
        }

        // ---------------------------------------------------------------------
        // Project Extents onto Axis
        // ---------------------------------------------------------------------
        /// <summary>
        /// Returns the projected half-length (radius) of the box onto an axis.
        /// </summary>
        /// <param name="b">The oriented bounding box.</param>
        /// <param name="axis">The axis to project onto; normalize it for a true length.</param>
        /// <returns>The half-length of the box's projection onto the axis.</returns>
        public static float ProjectExtent(OBB b, Vector3 axis)
        {
            Vector3 e = b.Extents;
            Vector3 r = b.Right;
            Vector3 u = b.Up;
            Vector3 f = b.Forward;

            return
                Mathf.Abs(Vector3.Dot(axis, r)) * e.x +
                Mathf.Abs(Vector3.Dot(axis, u)) * e.y +
                Mathf.Abs(Vector3.Dot(axis, f)) * e.z;
        }
    }
}
