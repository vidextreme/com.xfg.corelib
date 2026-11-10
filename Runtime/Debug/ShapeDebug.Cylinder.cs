// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

using UnityEngine;

namespace XFG.Math.Shape
{
    public static partial class ShapeDebug
    {
        /// <summary>
        /// Draws a cylinder defined by <paramref name="p0"/>, <paramref name="p1"/>, and <paramref name="radius"/>.
        /// </summary>
        /// <param name="p0">Center of the first cap.</param>
        /// <param name="p1">Center of the second cap.</param>
        /// <param name="radius">Cylinder radius.</param>
        /// <param name="color">Line color.</param>
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
