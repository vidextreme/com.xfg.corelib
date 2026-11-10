// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

using UnityEngine;

namespace XFG.Math.Shape
{
    /// <summary>
    /// Editor-only visualization helpers for shapes. This part draws bounding boxes.
    /// </summary>
    /// <remarks>
    /// Covers:
    /// <list type="bullet">
    /// <item><description>AABB (Unity <see cref="Bounds"/>)</description></item>
    /// <item><description>OBB (center, extents, rotation)</description></item>
    /// </list>
    /// </remarks>
    public static partial class ShapeDebug
    {
        // ---------------------------------------------------------------------
        // AABB
        // ---------------------------------------------------------------------
        /// <summary>
        /// Draws an axis-aligned bounding box.
        /// </summary>
        /// <param name="bounds">AABB to draw.</param>
        /// <param name="color">Line color.</param>
        public static void DrawAabb(Bounds bounds, Color color)
        {
#if UNITY_EDITOR
            UnityEditor.Handles.color = color;
            UnityEditor.Handles.DrawWireCube(bounds.center, bounds.size);
#endif
        }

        // ---------------------------------------------------------------------
        // OBB
        // ---------------------------------------------------------------------
        /// <summary>
        /// Draws an oriented bounding box.
        /// </summary>
        /// <param name="center">OBB center.</param>
        /// <param name="extents">OBB extents.</param>
        /// <param name="rotation">OBB rotation.</param>
        /// <param name="color">Line color.</param>
        public static void DrawObb(Vector3 center, Vector3 extents, Quaternion rotation, Color color)
        {
#if UNITY_EDITOR
            UnityEditor.Handles.color = color;

            Matrix4x4 m = Matrix4x4.TRS(center, rotation, Vector3.one);
            using (new UnityEditor.Handles.DrawingScope(m))
            {
                UnityEditor.Handles.DrawWireCube(Vector3.zero, extents * 2f);
            }
#endif
        }
    }
}
