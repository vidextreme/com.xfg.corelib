// Copyright (c) 2025 John David Uy
// Licensed under the MIT License. See LICENSE for details.
// ------------------------------------------------------------------------------
// TriangleVisualization
// ------------------------------------------------------------------------------
// Debug visualization helpers for triangles.
// Functions always exist, but bodies are editor-only.
// ------------------------------------------------------------------------------

using UnityEngine;

namespace XFG.Math.Shape
{
    public static class TriangleVisualization
    {
        public static void Draw(Vector3 a, Vector3 b, Vector3 c, Color color, float duration = 0f)
        {
#if UNITY_EDITOR
            Debug.DrawLine(a, b, color, duration);
            Debug.DrawLine(b, c, color, duration);
            Debug.DrawLine(c, a, color, duration);
#endif
        }
    }
}
