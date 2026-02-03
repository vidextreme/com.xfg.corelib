// Copyright (c) 2025 John David Uy
// Licensed under the MIT License. See LICENSE for details.

// ------------------------------------------------------------------------------
// ShapeDebug.Cone
// ------------------------------------------------------------------------------
//
using UnityEngine;

namespace XFG.Math.Shape
{
    public static partial class ShapeDebug
    {
        /// <summary>
        /// Draws a cone defined by apex, axis, height, and radius.
        /// </summary>
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
