// Copyright (c) 2025 John David Uy
// Licensed under the MIT License. See LICENSE for details.
// ------------------------------------------------------------------------------
// Shapes
// ------------------------------------------------------------------------------
// Core geometric primitives used throughout the geometry engine.
// Includes:
// - Sphere
// - Capsule
// - Cylinder
// - Cone
//
// All structs are deterministic, allocation-free, and suitable for Burst jobs.
// ------------------------------------------------------------------------------

using UnityEngine;

namespace XFG.Math.Shape
{
    // ==========================================================================
    // SPHERE
    // ==========================================================================
    public struct Sphere
    {
        public Vector3 Center;
        public float Radius;

        public Sphere(Vector3 center, float radius)
        {
            Center = center;
            Radius = radius;
        }

        public bool Contains(Vector3 p)
        {
            return (p - Center).sqrMagnitude <= Radius * Radius;
        }
    }

    // ==========================================================================
    // CAPSULE
    // ==========================================================================
    public struct Capsule
    {
        public Vector3 P0;
        public Vector3 P1;
        public float Radius;

        public Capsule(Vector3 p0, Vector3 p1, float radius)
        {
            P0 = p0;
            P1 = p1;
            Radius = radius;
        }

        public Vector3 Axis => P1 - P0;
        public float Height => (P1 - P0).magnitude;

        public bool Contains(Vector3 p)
        {
            Vector3 closest = SegmentMath.ClosestPointOnSegment(P0, P1, p);
            return (p - closest).sqrMagnitude <= Radius * Radius;
        }
    }

    // ==========================================================================
    // CYLINDER
    // ==========================================================================
    public struct Cylinder
    {
        public Vector3 P0;
        public Vector3 P1;
        public float Radius;

        public Cylinder(Vector3 p0, Vector3 p1, float radius)
        {
            P0 = p0;
            P1 = p1;
            Radius = radius;
        }

        public Vector3 Axis => P1 - P0;
        public float Height => (P1 - P0).magnitude;

        public bool Contains(Vector3 p)
        {
            Vector3 axis = P1 - P0;
            float axisLenSq = Vector3.Dot(axis, axis);

            float t = Vector3.Dot(p - P0, axis) / axisLenSq;
            t = Mathf.Clamp01(t);

            Vector3 axisPoint = P0 + axis * t;
            return (p - axisPoint).sqrMagnitude <= Radius * Radius;
        }
    }

    // ==========================================================================
    // CONE
    // ==========================================================================
    public struct Cone
    {
        public Vector3 Apex;
        public Vector3 BaseCenter;
        public float Radius;
        public float Height;

        public Cone(Vector3 apex, Vector3 baseCenter, float radius)
        {
            Apex = apex;
            BaseCenter = baseCenter;
            Radius = radius;
            Height = (baseCenter - apex).magnitude;
        }

        public Vector3 Axis => BaseCenter - Apex;

        public bool Contains(Vector3 p)
        {
            Vector3 dir = Axis.normalized;
            float t = Vector3.Dot(p - Apex, dir);
            if (t < 0f || t > Height)
                return false;

            Vector3 axisPoint = Apex + dir * t;
            float localRadius = (t / Height) * Radius;

            return (p - axisPoint).sqrMagnitude <= localRadius * localRadius;
        }
    }
}
