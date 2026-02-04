// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

using System.Runtime.CompilerServices;

namespace XFG.Algorithm
{
    public static partial class Core
    {
        public static readonly IRandom Random = new XorShift128Plus(9001UL);
        #region SplitMix64

        /// <summary>
        /// Generates a well-scrambled 64-bit value from the given seed.
        /// This function mutates the seed so each call produces a new,
        /// independent value. Commonly used to initialize PRNG state.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong SplitMix64(ref ulong x)
        {
            const ulong C1 = 0x9E3779B97F4A7C15UL;
            const ulong C2 = 0xBF58476D1CE4E5B9UL;
            const ulong C3 = 0x94D049BB133111EBUL;

            ulong z = (x += C1);
            z = (z ^ (z >> 30)) * C2;
            z = (z ^ (z >> 27)) * C3;
            return z ^ (z >> 31);
        }

        #endregion
    }

    #region IRandom
    /// <summary>
    /// Common interface for deterministic pseudorandom number generators.
    /// Provides a unified API for integer and float generation so different
    /// RNG algorithms (such as PCG32 and XorShift128Plus) can be used
    /// interchangeably in gameplay systems.
    /// </summary>
    public interface IRandom
    {
        /// <summary>
        /// Returns a 32-bit unsigned integer.
        /// This is the fundamental output for most RNG algorithms.
        /// </summary>
        uint NextUInt();

        /// <summary>
        /// Returns a float in the range [0, 1).
        /// Useful for normalized random values and probability checks.
        /// </summary>
        float NextFloat01();

        /// <summary>
        /// Returns a float in the range [min, max).
        /// Useful for continuous random ranges such as positions or speeds.
        /// </summary>
        float NextFloat(float min, float max);

        /// <summary>
        /// Returns an integer in the range [0, maxExclusive).
        /// Useful for indexing into arrays or selecting random items.
        /// </summary>
        int NextInt(int maxExclusive);

        /// <summary>
        /// Returns an integer in the range [minInclusive, maxExclusive).
        /// Useful for bounded integer ranges such as levels or counts.
        /// </summary>
        int NextInt(int minInclusive, int maxExclusive);
    }

    #endregion
}
