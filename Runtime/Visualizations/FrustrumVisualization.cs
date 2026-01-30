// Copyright (c) 2025 John David Uy
// Licensed under the MIT License. See LICENSE for details.
// ------------------------------------------------------------------------------
// FrustumVisualization
// ------------------------------------------------------------------------------
// Debug visualization helpers for camera frustums.
// Functions always exist, but bodies are editor-only.
// ------------------------------------------------------------------------------

using UnityEngine;

namespace XFG.Math.Shape
{
    public static class FrustumVisualization
    {
        public static void Draw(Camera cam, Color color, float duration = 0f)
        {
#if UNITY_EDITOR
            if (cam == null)
                return;

            Transform t = cam.transform;

            Vector3[] corners = new Vector3[4];

            // Near plane corners
            cam.CalculateFrustumCorners(
                new Rect(0f, 0f, 1f, 1f),
                cam.nearClipPlane,
                Camera.MonoOrStereoscopicEye.Mono,
                corners
            );

            Vector3 n0 = t.TransformPoint(corners[0]);
            Vector3 n1 = t.TransformPoint(corners[1]);
            Vector3 n2 = t.TransformPoint(corners[2]);
            Vector3 n3 = t.TransformPoint(corners[3]);

            // Far plane corners
            cam.CalculateFrustumCorners(
                new Rect(0f, 0f, 1f, 1f),
                cam.farClipPlane,
                Camera.MonoOrStereoscopicEye.Mono,
                corners
            );

            Vector3 f0 = t.TransformPoint(corners[0]);
            Vector3 f1 = t.TransformPoint(corners[1]);
            Vector3 f2 = t.TransformPoint(corners[2]);
            Vector3 f3 = t.TransformPoint(corners[3]);

            // Near plane
            Debug.DrawLine(n0, n1, color, duration);
            Debug.DrawLine(n1, n2, color, duration);
            Debug.DrawLine(n2, n3, color, duration);
            Debug.DrawLine(n3, n0, color, duration);

            // Far plane
            Debug.DrawLine(f0, f1, color, duration);
            Debug.DrawLine(f1, f2, color, duration);
            Debug.DrawLine(f2, f3, color, duration);
            Debug.DrawLine(f3, f0, color, duration);

            // Connect near to far
            Debug.DrawLine(n0, f0, color, duration);
            Debug.DrawLine(n1, f1, color, duration);
            Debug.DrawLine(n2, f2, color, duration);
            Debug.DrawLine(n3, f3, color, duration);
#endif
        }
    }
}
