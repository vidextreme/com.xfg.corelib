// Copyright (c) 2025 John David Uy
// Licensed under the MIT License. See LICENSE for details.
// ------------------------------------------------------------------------------
// OBBCollisionExtensions
// ------------------------------------------------------------------------------
// Collision tests for oriented bounding boxes (OBB).
//
// Implements:
// - OBB vs OBB (SAT)
// - OBB vs AABB
// - OBB vs Sphere
//
// All repeated property getters are cached.
// All methods are deterministic, allocation-free, and Burst-friendly.
// ------------------------------------------------------------------------------

using UnityEngine;

namespace XFG.Math.Shape
{
    public static class OBBCollisionExtensions
    {
        // ==========================================================================
        // OBB vs OBB (SAT)
        // ==========================================================================
        public static bool Intersects(OBB a, OBB b)
        {
            // Cache A
            Vector3 aC = a.Center;
            Vector3 aE = a.Extents;
            Vector3 aR = a.Right;
            Vector3 aU = a.Up;
            Vector3 aF = a.Forward;

            // Cache B
            Vector3 bC = b.Center;
            Vector3 bE = b.Extents;
            Vector3 bR = b.Right;
            Vector3 bU = b.Up;
            Vector3 bF = b.Forward;

            // Vector between centers
            Vector3 t = bC - aC;

            // Build rotation matrix R = dot(a.axis, b.axis)
            float r00 = Vector3.Dot(aR, bR);
            float r01 = Vector3.Dot(aR, bU);
            float r02 = Vector3.Dot(aR, bF);

            float r10 = Vector3.Dot(aU, bR);
            float r11 = Vector3.Dot(aU, bU);
            float r12 = Vector3.Dot(aU, bF);

            float r20 = Vector3.Dot(aF, bR);
            float r21 = Vector3.Dot(aF, bU);
            float r22 = Vector3.Dot(aF, bF);

            // Absolute rotation matrix (with epsilon)
            float abs00 = Mathf.Abs(r00) + 1e-6f;
            float abs01 = Mathf.Abs(r01) + 1e-6f;
            float abs02 = Mathf.Abs(r02) + 1e-6f;

            float abs10 = Mathf.Abs(r10) + 1e-6f;
            float abs11 = Mathf.Abs(r11) + 1e-6f;
            float abs12 = Mathf.Abs(r12) + 1e-6f;

            float abs20 = Mathf.Abs(r20) + 1e-6f;
            float abs21 = Mathf.Abs(r21) + 1e-6f;
            float abs22 = Mathf.Abs(r22) + 1e-6f;

            // Transform t into A's basis
            float tA0 = Vector3.Dot(t, aR);
            float tA1 = Vector3.Dot(t, aU);
            float tA2 = Vector3.Dot(t, aF);

            // --- Test A's axes -----------------------------------------------------
            if (Mathf.Abs(tA0) > aE.x + bE.x * abs00 + bE.y * abs01 + bE.z * abs02)
                return false;

            if (Mathf.Abs(tA1) > aE.y + bE.x * abs10 + bE.y * abs11 + bE.z * abs12)
                return false;

            if (Mathf.Abs(tA2) > aE.z + bE.x * abs20 + bE.y * abs21 + bE.z * abs22)
                return false;

            // --- Test B's axes -----------------------------------------------------
            float tB0 = Mathf.Abs(Vector3.Dot(t, bR));
            float tB1 = Mathf.Abs(Vector3.Dot(t, bU));
            float tB2 = Mathf.Abs(Vector3.Dot(t, bF));

            if (tB0 > bE.x + aE.x * abs00 + aE.y * abs10 + aE.z * abs20)
                return false;

            if (tB1 > bE.y + aE.x * abs01 + aE.y * abs11 + aE.z * abs21)
                return false;

            if (tB2 > bE.z + aE.x * abs02 + aE.y * abs12 + aE.z * abs22)
                return false;

            // --- Test cross axes ---------------------------------------------------
            // A0 x B0
            if (Mathf.Abs(tA2 * r10 - tA1 * r20) >
                aE.y * abs20 + aE.z * abs10 + bE.y * abs02 + bE.z * abs01)
                return false;

            // A0 x B1
            if (Mathf.Abs(tA2 * r11 - tA1 * r21) >
                aE.y * abs21 + aE.z * abs11 + bE.x * abs02 + bE.z * abs00)
                return false;

            // A0 x B2
            if (Mathf.Abs(tA2 * r12 - tA1 * r22) >
                aE.y * abs22 + aE.z * abs12 + bE.x * abs01 + bE.y * abs00)
                return false;

            // A1 x B0
            if (Mathf.Abs(tA0 * r20 - tA2 * r00) >
                aE.x * abs20 + aE.z * abs00 + bE.y * abs12 + bE.z * abs11)
                return false;

            // A1 x B1
            if (Mathf.Abs(tA0 * r21 - tA2 * r01) >
                aE.x * abs21 + aE.z * abs01 + bE.x * abs12 + bE.z * abs10)
                return false;

            // A1 x B2
            if (Mathf.Abs(tA0 * r22 - tA2 * r02) >
                aE.x * abs22 + aE.z * abs02 + bE.x * abs11 + bE.y * abs10)
                return false;

            // A2 x B0
            if (Mathf.Abs(tA1 * r00 - tA0 * r10) >
                aE.x * abs10 + aE.y * abs00 + bE.y * abs22 + bE.z * abs21)
                return false;

            // A2 x B1
            if (Mathf.Abs(tA1 * r01 - tA0 * r11) >
                aE.x * abs11 + aE.y * abs01 + bE.x * abs22 + bE.z * abs20)
                return false;

            // A2 x B2
            if (Mathf.Abs(tA1 * r02 - tA0 * r12) >
                aE.x * abs12 + aE.y * abs02 + bE.x * abs21 + bE.y * abs20)
                return false;

            return true;
        }

        // ==========================================================================
        // OBB vs AABB
        // ==========================================================================
        public static bool Intersects(OBB a, Bounds b)
        {
            // Convert AABB to OBB with identity orientation
            OBB bb = new OBB(
                b.center,
                b.extents,
                Vector3.right,
                Vector3.up,
                Vector3.forward
            );

            return Intersects(a, bb);
        }

        // ==========================================================================
        // OBB vs Sphere
        // ==========================================================================
        public static bool Intersects(OBB b, Vector3 sphereCenter, float sphereRadius)
        {
            // Cache OBB
            Vector3 c = b.Center;
            Vector3 e = b.Extents;
            Vector3 r = b.Right;
            Vector3 u = b.Up;
            Vector3 f = b.Forward;

            // Vector from OBB center to sphere center
            Vector3 d = sphereCenter - c;

            // Project onto OBB axes
            float x = Vector3.Dot(d, r);
            float y = Vector3.Dot(d, u);
            float z = Vector3.Dot(d, f);

            // Clamp to extents
            float cx = Mathf.Clamp(x, -e.x, e.x);
            float cy = Mathf.Clamp(y, -e.y, e.y);
            float cz = Mathf.Clamp(z, -e.z, e.z);

            // Closest point on OBB
            Vector3 closest =
                c + r * cx +
                u * cy +
                f * cz;

            // Sphere intersects if distance <= radius
            return (closest - sphereCenter).sqrMagnitude <= sphereRadius * sphereRadius;
        }
    }
}
