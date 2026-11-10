// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

using UnityEngine;

namespace XFG.Math.Shape
{
    public static partial class ShapeDebug
    {
        /// <summary>
        /// Draws a capsule defined by <paramref name="p0"/>, <paramref name="p1"/>, and <paramref name="radius"/>.
        /// </summary>
        /// <param name="p0">First endpoint of the capsule axis.</param>
        /// <param name="p1">Second endpoint of the capsule axis.</param>
        /// <param name="radius">Capsule radius.</param>
        /// <param name="color">Line color.</param>
        public static void DrawCapsule(Vector3 p0, Vector3 p1, float radius, Color color)
        {
#if UNITY_EDITOR
            UnityEditor.Handles.color = color;

            UnityEditor.Handles.DrawLine(p0, p1);

            UnityEditor.Handles.SphereHandleCap(0, p0, Quaternion.identity, radius * 2f, EventType.Repaint);
            UnityEditor.Handles.SphereHandleCap(0, p1, Quaternion.identity, radius * 2f, EventType.Repaint);
#endif
        }
    }
}
