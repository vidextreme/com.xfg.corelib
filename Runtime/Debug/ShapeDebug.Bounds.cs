// ------------------------------------------------------------------------------
// ShapeDebug.Bounds
// ------------------------------------------------------------------------------
// Editor-only visualization helpers for:
// - AABB (Unity Bounds)
// - OBB (center, extents, rotation)
// ------------------------------------------------------------------------------

using UnityEngine;

namespace XFG.Math.Shape
{
    public static partial class ShapeDebug
    {
        // ----------------------------------------------------------------------
        // AABB
        // ----------------------------------------------------------------------

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

        // ----------------------------------------------------------------------
        // OBB
        // ----------------------------------------------------------------------

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
