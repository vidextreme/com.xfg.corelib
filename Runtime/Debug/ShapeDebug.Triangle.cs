// Copyright (c) 2025 John David Uy
// Licensed under the MIT License. See LICENSE for details.

// ------------------------------------------------------------------------------
// ShapeDebug.Triangle
// ------------------------------------------------------------------------------
// Editor-only visualization helpers for triangle math.
// ------------------------------------------------------------------------------
//
using UnityEngine;

namespace XFG.Math.Shape
{
    public static partial class ShapeDebug
    {
        /// <summary>
        /// Draws the triangle edges.
        /// </summary>
        public static void DrawTriangle(Vector3 a, Vector3 b, Vector3 c, Color color)
        {
#if UNITY_EDITOR
            UnityEditor.Handles.color = color;
            UnityEditor.Handles.DrawLine(a, b);
            UnityEditor.Handles.DrawLine(b, c);
            UnityEditor.Handles.DrawLine(c, a);
#endif
        }

        /// <summary>
        /// Draws the triangle normal at the centroid.
        /// </summary>
        public static void DrawTriangleNormal(Vector3 a, Vector3 b, Vector3 c, float scale, Color color)
        {
#if UNITY_EDITOR
            Vector3 centroid = (a + b + c) / 3f;
            Vector3 normal = TriangleMath.Normal(a, b, c);

            UnityEditor.Handles.color = color;
            UnityEditor.Handles.DrawLine(centroid, centroid + normal * scale);
#endif
        }

        /// <summary>
        /// Draws barycentric axes from the triangle centroid.
        /// </summary>
        public static void DrawTriangleBarycentricAxes(Vector3 a, Vector3 b, Vector3 c, float scale)
        {
#if UNITY_EDITOR
            Vector3 centroid = (a + b + c) / 3f;

            UnityEditor.Handles.color = Color.red;
            UnityEditor.Handles.DrawLine(centroid, centroid + (a - centroid).normalized * scale);

            UnityEditor.Handles.color = Color.green;
            UnityEditor.Handles.DrawLine(centroid, centroid + (b - centroid).normalized * scale);

            UnityEditor.Handles.color = Color.blue;
            UnityEditor.Handles.DrawLine(centroid, centroid + (c - centroid).normalized * scale);
#endif
        }

        /// <summary>
        /// Draws the closest point from p to the triangle.
        /// </summary>
        public static void DrawTriangleClosestPoint(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
        {
#if UNITY_EDITOR
            Vector3 q = TriangleMath.ClosestPointOnTriangle(p, a, b, c);

            UnityEditor.Handles.color = Color.yellow;
            UnityEditor.Handles.DrawLine(p, q);

            UnityEditor.Handles.color = Color.magenta;
            UnityEditor.Handles.SphereHandleCap(0, q, Quaternion.identity, 0.05f, EventType.Repaint);
#endif
        }
    }
}
