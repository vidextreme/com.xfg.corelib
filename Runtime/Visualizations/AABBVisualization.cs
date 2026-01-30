// Copyright (c) 2025 John David Uy
// Licensed under the MIT License. See LICENSE for details.
// ------------------------------------------------------------------------------
// AABBVisualization
// ------------------------------------------------------------------------------
// Debug visualization helpers for UnityEngine.Bounds (AABB).
// Functions always exist, but bodies are editor-only.
// ------------------------------------------------------------------------------

using UnityEngine;

namespace XFG.Math.Shape
{
    public static class AABBVisualization
    {
        public static void DrawWire(Bounds b, Color color, float duration = 0f)
        {
#if UNITY_EDITOR
            Vector3 c = b.center;
            Vector3 e = b.extents;

            Vector3 v0 = c + new Vector3(e.x, e.y, e.z);
            Vector3 v1 = c + new Vector3(e.x, e.y, -e.z);
            Vector3 v2 = c + new Vector3(e.x, -e.y, e.z);
            Vector3 v3 = c + new Vector3(e.x, -e.y, -e.z);
            Vector3 v4 = c + new Vector3(-e.x, e.y, e.z);
            Vector3 v5 = c + new Vector3(-e.x, e.y, -e.z);
            Vector3 v6 = c + new Vector3(-e.x, -e.y, e.z);
            Vector3 v7 = c + new Vector3(-e.x, -e.y, -e.z);

            Debug.DrawLine(v0, v1, color, duration);
            Debug.DrawLine(v1, v5, color, duration);
            Debug.DrawLine(v5, v4, color, duration);
            Debug.DrawLine(v4, v0, color, duration);

            Debug.DrawLine(v2, v3, color, duration);
            Debug.DrawLine(v3, v7, color, duration);
            Debug.DrawLine(v7, v6, color, duration);
            Debug.DrawLine(v6, v2, color, duration);

            Debug.DrawLine(v0, v2, color, duration);
            Debug.DrawLine(v1, v3, color, duration);
            Debug.DrawLine(v4, v6, color, duration);
            Debug.DrawLine(v5, v7, color, duration);
#endif
        }
    }
}
