// Copyright (c) 2025 John David Uy
// Licensed under the MIT License. See LICENSE for details.
// ------------------------------------------------------------------------------
// ConeVisualization
// ------------------------------------------------------------------------------
// Debug visualization helpers for Cone.
// Functions always exist, but bodies are editor-only.
// ------------------------------------------------------------------------------

using UnityEngine;

namespace XFG.Math.Shape
{
    public static class ConeVisualization
    {
        public static void DrawWire(Cone c, Color color, float duration = 0f)
        {
#if UNITY_EDITOR
            Vector3 apex = c.Apex;
            Vector3 baseCenter = c.BaseCenter;
            float r = c.Radius;

            Debug.DrawLine(apex, baseCenter, color, duration);

            Vector3 up = c.Axis;
            Vector3 right = Vector3.Cross(up, Vector3.up);
            if (right.sqrMagnitude < 1e-6f)
                right = Vector3.Cross(up, Vector3.right);
            right.Normalize();
            Vector3 forward = Vector3.Cross(up, right);

            Vector3 rR = right * r;
            Vector3 rF = forward * r;

            Debug.DrawLine(baseCenter + rR, apex, color, duration);
            Debug.DrawLine(baseCenter - rR, apex, color, duration);
            Debug.DrawLine(baseCenter + rF, apex, color, duration);
            Debug.DrawLine(baseCenter - rF, apex, color, duration);
#endif
        }
    }
}
