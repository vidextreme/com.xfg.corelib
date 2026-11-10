// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

using UnityEngine;

namespace XFG.Math.Shape
{
    public static partial class ShapeDebug
    {
        /// <summary>
        /// Draws a segment between <paramref name="p0"/> and <paramref name="p1"/>.
        /// </summary>
        /// <param name="p0">Segment start point.</param>
        /// <param name="p1">Segment end point.</param>
        /// <param name="color">Line color.</param>
        public static void DrawSegment(Vector3 p0, Vector3 p1, Color color)
        {
#if UNITY_EDITOR
            UnityEditor.Handles.color = color;
            UnityEditor.Handles.DrawLine(p0, p1);
#endif
        }

        /// <summary>
        /// Draws the closest point from <paramref name="q"/> to the segment.
        /// </summary>
        /// <param name="p0">Segment start point.</param>
        /// <param name="p1">Segment end point.</param>
        /// <param name="q">Query point.</param>
        public static void DrawSegmentClosestPoint(Vector3 p0, Vector3 p1, Vector3 q)
        {
#if UNITY_EDITOR
            Vector3 cp = SegmentMath.ClosestPointOnSegmentToPoint(p0, p1, q);

            UnityEditor.Handles.color = Color.yellow;
            UnityEditor.Handles.DrawLine(q, cp);

            UnityEditor.Handles.color = Color.magenta;
            UnityEditor.Handles.SphereHandleCap(0, cp, Quaternion.identity, 0.05f, EventType.Repaint);
#endif
        }
    }
}
