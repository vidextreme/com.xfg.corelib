// ------------------------------------------------------------------------------
// ShapeDebug.Segment
// ------------------------------------------------------------------------------

using UnityEngine;

namespace XFG.Math.Shape
{
    public static partial class ShapeDebug
    {
        /// <summary>
        /// Draws a segment between p0 and p1.
        /// </summary>
        public static void DrawSegment(Vector3 p0, Vector3 p1, Color color)
        {
#if UNITY_EDITOR
            UnityEditor.Handles.color = color;
            UnityEditor.Handles.DrawLine(p0, p1);
#endif
        }

        /// <summary>
        /// Draws the closest point from q to the segment.
        /// </summary>
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
