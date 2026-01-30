// Copyright (c) 2025 John David Uy
// Licensed under the MIT License. See LICENSE for details.
// ------------------------------------------------------------------------------
// SegmentVisualization
// ------------------------------------------------------------------------------
// Debug visualization helpers for line segments.
// Functions always exist, but bodies are editor-only.
// ------------------------------------------------------------------------------

using UnityEngine;

namespace XFG.Math.Shape
{
    public static class SegmentVisualization
    {
        public static void Draw(Vector3 p0, Vector3 p1, Color color, float duration = 0f)
        {
#if UNITY_EDITOR
            Debug.DrawLine(p0, p1, color, duration);
#endif
        }
    }
}
