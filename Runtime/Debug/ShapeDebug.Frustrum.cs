// ------------------------------------------------------------------------------
// ShapeDebug.Frustum
// ------------------------------------------------------------------------------

using UnityEngine;

namespace XFG.Math.Shape
{
    public static partial class ShapeDebug
    {
        /// <summary>
        /// Draws the normals of each frustum plane.
        /// </summary>
        public static void DrawFrustumPlanes(Plane[] planes, float scale)
        {
#if UNITY_EDITOR
            if (planes == null)
                return;

            for (int i = 0; i < planes.Length; i++)
            {
                Plane p = planes[i];

                Vector3 point = -p.normal * p.distance;
                Vector3 normal = p.normal;

                UnityEditor.Handles.color = Color.cyan;
                UnityEditor.Handles.DrawLine(point, point + normal * scale);
            }
#endif
        }
    }
}
