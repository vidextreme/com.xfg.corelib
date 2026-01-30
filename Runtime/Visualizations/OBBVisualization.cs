// Copyright (c) 2025 John David Uy
// Licensed under the MIT License. See LICENSE for details.
// ------------------------------------------------------------------------------
// OBBVisualization
// ------------------------------------------------------------------------------
// Debug visualization helpers for oriented bounding boxes (OBB).
//
// Provides:
// - DrawWire (wireframe box)
// - DrawAxes (Right/Up/Forward basis vectors)
// - DrawCorners (corner markers)
//
// All methods are intended for debugging and editor visualization only.
// They are allocation-free and safe to call every frame.
// ------------------------------------------------------------------------------

using UnityEngine;

namespace XFG.Math.Shape
{
    public static class OBBVisualization
    {
        // ==========================================================================
        // DRAW WIRE BOX
        // ==========================================================================
        /// <summary>
        /// Draws a wireframe representation of the OBB using Debug.DrawLine.
        /// </summary>
        public static void DrawWire(OBB b, Color color, float duration = 0f)
        {
            Vector3[] c = new Vector3[8];
            b.GetCorners(c);

            // Top face
            Debug.DrawLine(c[0], c[1], color, duration);
            Debug.DrawLine(c[1], c[3], color, duration);
            Debug.DrawLine(c[3], c[2], color, duration);
            Debug.DrawLine(c[2], c[0], color, duration);

            // Bottom face
            Debug.DrawLine(c[4], c[5], color, duration);
            Debug.DrawLine(c[5], c[7], color, duration);
            Debug.DrawLine(c[7], c[6], color, duration);
            Debug.DrawLine(c[6], c[4], color, duration);

            // Vertical edges
            Debug.DrawLine(c[0], c[4], color, duration);
            Debug.DrawLine(c[1], c[5], color, duration);
            Debug.DrawLine(c[2], c[6], color, duration);
            Debug.DrawLine(c[3], c[7], color, duration);
        }

        // ==========================================================================
        // DRAW AXES
        // ==========================================================================
        /// <summary>
        /// Draws the OBB's local axes (Right, Up, Forward) for debugging.
        /// Useful for verifying orientation and basis correctness.
        /// </summary>
        public static void DrawAxes(OBB b, float axisLength = 1f, float duration = 0f)
        {
            Debug.DrawLine(b.Center, b.Center + b.Right * axisLength, Color.red, duration);
            Debug.DrawLine(b.Center, b.Center + b.Up * axisLength, Color.green, duration);
            Debug.DrawLine(b.Center, b.Center + b.Forward * axisLength, Color.blue, duration);
        }

        // ==========================================================================
        // DRAW CORNERS
        // ==========================================================================
        /// <summary>
        /// Draws small cross markers at each of the OBB's 8 corners.
        /// Helps visualize orientation and shape in 3D space.
        /// </summary>
        public static void DrawCorners(OBB b, float size = 0.05f, float duration = 0f)
        {
            Vector3[] c = new Vector3[8];
            b.GetCorners(c);

            for (int i = 0; i < 8; i++)
            {
                DrawCross(c[i], size, duration);
            }
        }

        // ==========================================================================
        // INTERNAL CROSS MARKER
        // ==========================================================================
        private static void DrawCross(Vector3 p, float size, float duration)
        {
            float s = size * 0.5f;

            Debug.DrawLine(p + new Vector3(-s, 0f, 0f), p + new Vector3(s, 0f, 0f), Color.yellow, duration);
            Debug.DrawLine(p + new Vector3(0f, -s, 0f), p + new Vector3(0f, s, 0f), Color.yellow, duration);
            Debug.DrawLine(p + new Vector3(0f, 0f, -s), p + new Vector3(0f, 0f, s), Color.yellow, duration);
        }
    }
}
