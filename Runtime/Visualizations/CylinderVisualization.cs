// Copyright (c) 2025 John David Uy
// Licensed under the MIT License. See LICENSE for details.
// ------------------------------------------------------------------------------
// CylinderVisualization
// ------------------------------------------------------------------------------
// Debug visualization helpers for Cylinder.
// Functions always exist, but bodies are editor-only.
// ------------------------------------------------------------------------------

using UnityEngine;

namespace XFG.Math.Shape
{
    public static class CylinderVisualization
    {
        public static void DrawWire(Cylinder cy, Color color, float duration = 0f)
        {
#if UNITY_EDITOR
            Vector3 p0 = cy.P0;
            Vector3 p1 = cy.P1;
            float r = cy.Radius;

            Debug.DrawLine(p0, p1, color, duration);

            Vector3 up = (p1 - p0).normalized;
            Vector3 right = Vector3.Cross(up, Vector3.up);
            if (right.sqrMagnitude < 1e-6f)
                right = Vector3.Cross(up, Vector3.right);
            right.Normalize();
            Vector3 forward = Vector3.Cross(up, right);

            Vector3 rR = right * r;
            Vector3 rF = forward * r;

            Debug.DrawLine(p0 + rR, p1 + rR, color, duration);
            Debug.DrawLine(p0 - rR, p1 - rR, color, duration);
            Debug.DrawLine(p0 + rF, p1 + rF, color, duration);
            Debug.DrawLine(p0 - rF, p1 - rF, color, duration);
#endif
        }
    }
}
