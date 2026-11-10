// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

using UnityEngine;

namespace XFG.Math.Shape
{
    public static partial class ShapeDebug
    {
        /// <summary>
        /// Draws a cone defined by apex, axis, height, and radius.
        /// </summary>
        /// <param name="apex">Tip of the cone.</param>
        /// <param name="axis">Direction from the apex to the base center (expected to be normalized).</param>
        /// <param name="height">Distance from the apex to the base.</param>
        /// <param name="radius">Radius of the base.</param>
        /// <param name="color">Line color.</param>
        public static void DrawCone(Vector3 apex, Vector3 axis, float height, float radius, Color color)
        {
#if UNITY_EDITOR
            UnityEditor.Handles.color = color;

            Vector3 baseCenter = apex + axis * height;

            UnityEditor.Handles.DrawLine(apex, baseCenter);
            UnityEditor.Handles.DrawWireDisc(baseCenter, axis, radius);
#endif
        }
    }
}
