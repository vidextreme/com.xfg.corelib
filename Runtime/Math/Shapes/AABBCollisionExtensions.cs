// Copyright (c) 2025 John David Uy
// Licensed under the MIT License. See LICENSE for details.
// ------------------------------------------------------------------------------
// AABBCollisionExtensions
// ------------------------------------------------------------------------------
// Collision tests for axis-aligned bounding boxes (AABB).
//
// Provides:
// - AABB vs AABB
// - AABB vs Sphere
// - AABB vs OBB (delegates to OBB SAT)
// ------------------------------------------------------------------------------

using UnityEngine;

namespace XFG.Math.Shape
{
    public static class AABBCollisionExtensions
    {
        // ==========================================================================
        // AABB vs AABB
        // ==========================================================================
        public static bool Intersects(Bounds a, Bounds b)
        {
            Vector3 aMin = a.min;
            Vector3 aMax = a.max;
            Vector3 bMin = b.min;
            Vector3 bMax = b.max;

            if (aMax.x < bMin.x || aMin.x > bMax.x) return false;
            if (aMax.y < bMin.y || aMin.y > bMax.y) return false;
            if (aMax.z < bMin.z || aMin.z > bMax.z) return false;

            return true;
        }

        // ==========================================================================
        // AABB vs SPHERE
        // ==========================================================================
        public static bool Intersects(Bounds a, Vector3 sphereCenter, float sphereRadius)
        {
            Vector3 c = a.center;
            Vector3 e = a.extents;

            Vector3 d = sphereCenter - c;

            float x = Mathf.Clamp(d.x, -e.x, e.x);
            float y = Mathf.Clamp(d.y, -e.y, e.y);
            float z = Mathf.Clamp(d.z, -e.z, e.z);

            Vector3 closest = c + new Vector3(x, y, z);

            return (closest - sphereCenter).sqrMagnitude <= sphereRadius * sphereRadius;
        }

        // ==========================================================================
        // AABB vs OBB
        // ==========================================================================
        public static bool Intersects(Bounds a, OBB b)
        {
            OBB aa = new OBB(
                a.center,
                a.extents,
                Vector3.right,
                Vector3.up,
                Vector3.forward
            );

            return OBBCollisionExtensions.Intersects(aa, b);
        }
    }
}
