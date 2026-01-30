// Copyright (c) 2025 John David Uy
// Licensed under the MIT License. See LICENSE for details.
// ------------------------------------------------------------------------------
// OBBMath
// ------------------------------------------------------------------------------
// Math utilities for oriented bounding boxes.
//
// Provides:
// - Closest point
// - Distance to point
// - Projection onto axes
//
// All repeated property getters are cached.
// ------------------------------------------------------------------------------

using UnityEngine;

namespace XFG.Math.Shape
{
    public static class OBBMath
    {
        // ==========================================================================
        // CLOSEST POINT
        // ==========================================================================
        public static Vector3 ClosestPoint(OBB b, Vector3 point)
        {
            Vector3 c = b.Center;
            Vector3 e = b.Extents;
            Vector3 r = b.Right;
            Vector3 u = b.Up;
            Vector3 f = b.Forward;

            Vector3 d = point - c;

            float x = Mathf.Clamp(Vector3.Dot(d, r), -e.x, e.x);
            float y = Mathf.Clamp(Vector3.Dot(d, u), -e.y, e.y);
            float z = Mathf.Clamp(Vector3.Dot(d, f), -e.z, e.z);

            return c + r * x + u * y + f * z;
        }

        // ==========================================================================
        // DISTANCE TO POINT
        // ==========================================================================
        public static float DistanceToPoint(OBB b, Vector3 point)
        {
            Vector3 cp = ClosestPoint(b, point);
            return (cp - point).magnitude;
        }

        // ==========================================================================
        // PROJECT EXTENTS ONTO AXIS
        // ==========================================================================
        public static float ProjectExtent(OBB b, Vector3 axis)
        {
            Vector3 e = b.Extents;
            Vector3 r = b.Right;
            Vector3 u = b.Up;
            Vector3 f = b.Forward;

            return
                Mathf.Abs(Vector3.Dot(axis, r)) * e.x +
                Mathf.Abs(Vector3.Dot(axis, u)) * e.y +
                Mathf.Abs(Vector3.Dot(axis, f)) * e.z;
        }
    }
}
