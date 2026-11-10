// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

using UnityEngine;

namespace XFG.Math.Shape
{
    // Core geometric primitives used throughout the geometry engine:
    // Sphere, Capsule, Cylinder, Cone and Frustum. All structs are deterministic,
    // allocation-free, and suitable for Burst jobs.

    // -------------------------------------------------------------------------
    // Sphere
    // -------------------------------------------------------------------------
    /// <summary>
    /// A sphere defined by a center and a radius.
    /// </summary>
    /// <remarks>
    /// Deterministic, allocation-free, and suitable for Burst jobs.
    /// </remarks>
    public struct Sphere
    {
        /// <summary>
        /// The world-space center of the sphere.
        /// </summary>
        public Vector3 Center;

        /// <summary>
        /// The radius of the sphere.
        /// </summary>
        public float Radius;

        /// <summary>
        /// Creates a sphere.
        /// </summary>
        /// <param name="center">The world-space center.</param>
        /// <param name="radius">The radius.</param>
        public Sphere(Vector3 center, float radius)
        {
            Center = center;
            Radius = radius;
        }

        /// <summary>
        /// Returns whether a point lies inside or on the sphere.
        /// </summary>
        /// <param name="p">The point to test.</param>
        /// <returns><c>true</c> if the point is inside or on the surface.</returns>
        public bool Contains(Vector3 p)
        {
            return (p - Center).sqrMagnitude <= Radius * Radius;
        }
    }

    // -------------------------------------------------------------------------
    // Capsule
    // -------------------------------------------------------------------------
    /// <summary>
    /// A capsule: a segment from <see cref="P0"/> to <see cref="P1"/> swept by a sphere of <see cref="Radius"/>.
    /// </summary>
    /// <remarks>
    /// Deterministic, allocation-free, and suitable for Burst jobs.
    /// </remarks>
    public struct Capsule
    {
        /// <summary>
        /// The first endpoint of the capsule's core segment.
        /// </summary>
        public Vector3 P0;

        /// <summary>
        /// The second endpoint of the capsule's core segment.
        /// </summary>
        public Vector3 P1;

        /// <summary>
        /// The capsule radius.
        /// </summary>
        public float Radius;

        /// <summary>
        /// Creates a capsule.
        /// </summary>
        /// <param name="p0">The first endpoint of the core segment.</param>
        /// <param name="p1">The second endpoint of the core segment.</param>
        /// <param name="radius">The capsule radius.</param>
        public Capsule(Vector3 p0, Vector3 p1, float radius)
        {
            P0 = p0;
            P1 = p1;
            Radius = radius;
        }

        /// <summary>
        /// The unnormalized axis from <see cref="P0"/> to <see cref="P1"/>.
        /// </summary>
        public Vector3 Axis => P1 - P0;

        /// <summary>
        /// The length of the core segment (excluding the hemispherical caps).
        /// </summary>
        public float Height => (P1 - P0).magnitude;

        /// <summary>
        /// Returns whether a point lies inside or on the capsule.
        /// </summary>
        /// <param name="p">The point to test.</param>
        /// <returns><c>true</c> if the point is inside or on the surface.</returns>
        public bool Contains(Vector3 p)
        {
            Vector3 closest = SegmentMath.ClosestPointOnSegment(P0, P1, p);
            return (p - closest).sqrMagnitude <= Radius * Radius;
        }
    }

    // -------------------------------------------------------------------------
    // Cylinder
    // -------------------------------------------------------------------------
    /// <summary>
    /// A flat-capped cylinder from <see cref="P0"/> to <see cref="P1"/> with a constant <see cref="Radius"/>.
    /// </summary>
    /// <remarks>
    /// Deterministic, allocation-free, and suitable for Burst jobs.
    /// </remarks>
    public struct Cylinder
    {
        /// <summary>
        /// The center of the first cap.
        /// </summary>
        public Vector3 P0;

        /// <summary>
        /// The center of the second cap.
        /// </summary>
        public Vector3 P1;

        /// <summary>
        /// The cylinder radius.
        /// </summary>
        public float Radius;

        /// <summary>
        /// Creates a cylinder.
        /// </summary>
        /// <param name="p0">The center of the first cap.</param>
        /// <param name="p1">The center of the second cap.</param>
        /// <param name="radius">The cylinder radius.</param>
        public Cylinder(Vector3 p0, Vector3 p1, float radius)
        {
            P0 = p0;
            P1 = p1;
            Radius = radius;
        }

        /// <summary>
        /// The unnormalized axis from <see cref="P0"/> to <see cref="P1"/>.
        /// </summary>
        public Vector3 Axis => P1 - P0;

        /// <summary>
        /// The distance between the two cap centers.
        /// </summary>
        public float Height => (P1 - P0).magnitude;

        /// <summary>
        /// Returns whether a point lies inside or on the cylinder.
        /// </summary>
        /// <param name="p">The point to test.</param>
        /// <returns><c>true</c> if the point is inside or on the surface.</returns>
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

    // -------------------------------------------------------------------------
    // Cone
    // -------------------------------------------------------------------------
    /// <summary>
    /// A solid cone defined by an apex, a base center and a base radius.
    /// </summary>
    /// <remarks>
    /// Deterministic, allocation-free, and suitable for Burst jobs.
    /// </remarks>
    public struct Cone
    {
        /// <summary>
        /// The tip of the cone.
        /// </summary>
        public Vector3 Apex;

        /// <summary>
        /// The center of the cone's base disc.
        /// </summary>
        public Vector3 BaseCenter;

        /// <summary>
        /// The radius of the base disc.
        /// </summary>
        public float Radius;

        /// <summary>
        /// The distance from <see cref="Apex"/> to <see cref="BaseCenter"/>.
        /// </summary>
        public float Height;

        /// <summary>
        /// Creates a cone and computes its height from the apex and base center.
        /// </summary>
        /// <param name="apex">The tip of the cone.</param>
        /// <param name="baseCenter">The center of the base disc.</param>
        /// <param name="radius">The base radius.</param>
        public Cone(Vector3 apex, Vector3 baseCenter, float radius)
        {
            Apex = apex;
            BaseCenter = baseCenter;
            Radius = radius;
            Height = (baseCenter - apex).magnitude;
        }

        /// <summary>
        /// The unnormalized axis from <see cref="Apex"/> to <see cref="BaseCenter"/>.
        /// </summary>
        public Vector3 Axis => BaseCenter - Apex;

        /// <summary>
        /// Returns whether a point lies inside or on the cone.
        /// </summary>
        /// <param name="p">The point to test.</param>
        /// <returns><c>true</c> if the point is inside or on the surface.</returns>
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

    // -------------------------------------------------------------------------
    // Frustum
    // -------------------------------------------------------------------------
    /// <summary>
    /// A view frustum made of six planes.
    /// </summary>
    /// <remarks>
    /// Deterministic, allocation-free, and suitable for Burst jobs.
    /// </remarks>
    public struct Frustum
    {
        /// <summary>
        /// The left clipping plane (index 0).
        /// </summary>
        public Plane Left;

        /// <summary>
        /// The right clipping plane (index 1).
        /// </summary>
        public Plane Right;

        /// <summary>
        /// The bottom clipping plane (index 2).
        /// </summary>
        public Plane Bottom;

        /// <summary>
        /// The top clipping plane (index 3).
        /// </summary>
        public Plane Top;

        /// <summary>
        /// The near clipping plane (index 4).
        /// </summary>
        public Plane Near;

        /// <summary>
        /// The far clipping plane (index 5).
        /// </summary>
        public Plane Far;

        /// <summary>
        /// Gets a plane by index: 0 Left, 1 Right, 2 Bottom, 3 Top, 4 Near, 5 Far.
        /// </summary>
        /// <param name="i">The plane index; out-of-range values return <see cref="Left"/>.</param>
        /// <returns>The plane at the given index.</returns>
        public Plane this[int i]
        {
            get
            {
                switch (i)
                {
                    case 0: return Left;
                    case 1: return Right;
                    case 2: return Bottom;
                    case 3: return Top;
                    case 4: return Near;
                    case 5: return Far;
                }
                return Left;
            }
        }
    }
}
