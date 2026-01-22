// Copyright (c) 2025 Extreme Focus Games
// Licensed under the MIT License. See LICENSE for details.

using UnityEngine;

namespace XFG.Math.Shape
{
    // ------------------------------------------------------------------------------
    // SHAPES
    // ------------------------------------------------------------------------------

    #region Sphere

    /// <summary>
    /// Sphere defined by a center point and radius.
    /// </summary>
    public struct Sphere
    {
        public Vector3 Center;
        public float Radius;

        public Sphere(Vector3 center, float radius)
        {
            Center = center;
            Radius = radius;
        }
    }

    #endregion


    #region Capsule

    /// <summary>
    /// Capsule defined by two endpoints and a radius.
    /// </summary>
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
    }

    #endregion


    #region Cylinder

    /// <summary>
    /// Finite cylinder defined by two endpoints and a radius.
    /// </summary>
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
    }

    #endregion


    #region Cone

    /// <summary>
    /// Finite right circular cone defined by apex, axis, height, and base radius.
    /// Axis does not need to be normalized.
    /// </summary>
    public struct Cone
    {
        public Vector3 Apex;
        public Vector3 Axis;
        public float Height;
        public float Radius;

        public Cone(Vector3 apex, Vector3 axis, float height, float radius)
        {
            Apex = apex;
            Axis = axis;
            Height = height;
            Radius = radius;
        }

        /// <summary>
        /// Returns the center of the base circle.
        /// </summary>
        public Vector3 BaseCenter
        {
            get
            {
                Vector3 dir = Axis.normalized;
                return Apex + dir * Height;
            }
        }
    }

    #endregion
}
