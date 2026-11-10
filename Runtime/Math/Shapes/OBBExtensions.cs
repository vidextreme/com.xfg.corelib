// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

using UnityEngine;

namespace XFG.Math.Shape
{
    /// <summary>
    /// Utility extensions for oriented bounding boxes (OBB).
    /// </summary>
    /// <remarks>
    /// Provides:
    /// <list type="bullet">
    /// <item>GetCorners</item>
    /// <item>GetAxis(i)</item>
    /// <item>Support point</item>
    /// </list>
    /// All repeated property getters are cached.
    /// </remarks>
    public static class OBBExtensions
    {
        // ---------------------------------------------------------------------
        // Corners
        // ---------------------------------------------------------------------
        /// <summary>
        /// Writes the 8 world-space corners of the box into <paramref name="outCorners"/>.
        /// </summary>
        /// <param name="b">The oriented bounding box.</param>
        /// <param name="outCorners">Destination array; must hold at least 8 elements.</param>
        public static void GetCorners(this OBB b, Vector3[] outCorners)
        {
            Vector3 c = b.Center;
            Vector3 e = b.Extents;
            Vector3 r = b.Right;
            Vector3 u = b.Up;
            Vector3 f = b.Forward;

            Vector3 rE = r * e.x;
            Vector3 uE = u * e.y;
            Vector3 fE = f * e.z;

            outCorners[0] = c + rE + uE + fE;
            outCorners[1] = c + rE + uE - fE;
            outCorners[2] = c + rE - uE + fE;
            outCorners[3] = c + rE - uE - fE;

            outCorners[4] = c - rE + uE + fE;
            outCorners[5] = c - rE + uE - fE;
            outCorners[6] = c - rE - uE + fE;
            outCorners[7] = c - rE - uE - fE;
        }

        // ---------------------------------------------------------------------
        // Axis Accessor
        // ---------------------------------------------------------------------
        /// <summary>
        /// Returns a local axis by index: 0 Right, 1 Up, 2 Forward.
        /// </summary>
        /// <param name="b">The oriented bounding box.</param>
        /// <param name="index">The axis index; out-of-range values return Right.</param>
        /// <returns>The requested axis.</returns>
        public static Vector3 GetAxis(this OBB b, int index)
        {
            switch (index)
            {
                case 0: return b.Right;
                case 1: return b.Up;
                case 2: return b.Forward;
            }
            return b.Right;
        }

        // ---------------------------------------------------------------------
        // Support Point
        // ---------------------------------------------------------------------
        /// <summary>
        /// Returns the support point: the corner of the box furthest along a direction.
        /// </summary>
        /// <param name="b">The oriented bounding box.</param>
        /// <param name="dir">The query direction.</param>
        /// <returns>The box corner furthest in <paramref name="dir"/>.</returns>
        public static Vector3 Support(this OBB b, Vector3 dir)
        {
            Vector3 c = b.Center;
            Vector3 e = b.Extents;
            Vector3 r = b.Right;
            Vector3 u = b.Up;
            Vector3 f = b.Forward;

            float x = Mathf.Sign(Vector3.Dot(dir, r));
            float y = Mathf.Sign(Vector3.Dot(dir, u));
            float z = Mathf.Sign(Vector3.Dot(dir, f));

            return c + r * (e.x * x) + u * (e.y * y) + f * (e.z * z);
        }
    }
}
