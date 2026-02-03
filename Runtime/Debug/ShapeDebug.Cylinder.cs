// Copyright (c) 2025 John David Uy
// Licensed under the MIT License. See LICENSE for details.

// ------------------------------------------------------------------------------
// ShapeDebug.Cylinder
// ------------------------------------------------------------------------------
//
using UnityEngine;

namespace XFG.Math.Shape
{
    public static partial class ShapeDebug
    {
        /// <summary>
        /// Draws a cylinder defined by p0, p1, and radius.
        /// </summary>
        public static void DrawCylinder(Vector3 p0, Vector3 p1, float radius, Color color)
        {
#if UNITY_EDITOR
            UnityEditor.Handles.color = color;

            UnityEditor.Handles.DrawLine(p0, p1);

            Vector3 axis = (p1 - p0).normalized;
            UnityEditor.Handles.DrawWireDisc(p0, axis, radius);
            UnityEditor.Handles.DrawWireDisc(p1, axis, radius);
#endif
        }
    }
}
