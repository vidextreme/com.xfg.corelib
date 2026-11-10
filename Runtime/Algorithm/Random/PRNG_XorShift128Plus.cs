// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

using System;
using System.Runtime.CompilerServices;

namespace XFG.Algorithm
{
    /// <summary>
    /// Deterministic 128-bit XorShift+ pseudorandom number generator.
    /// Fast, simple, and suitable for gameplay and reproducible simulations.
    /// Not cryptographically secure.
    /// </summary>
    public struct XorShift128Plus : IRandom
    {
        private ulong _s0;
        private ulong _s1;

        /// <summary>
        /// Creates a new XorShift128Plus generator using a 64-bit seed.
        /// The seed is expanded into two internal state values using SplitMix64.
        /// </summary>
        /// <param name="seed">Initial seed value; zero is replaced with a fixed default.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public XorShift128Plus(ulong seed)
        {
            const ulong DEFAULT_SEED = 0x9E3779B97F4A7C15UL;

            if (seed == 0)
                seed = DEFAULT_SEED;

            _s0 = Core.SplitMix64(ref seed);
            _s1 = Core.SplitMix64(ref seed);

            if (_s0 == 0 && _s1 == 0)
                _s1 = DEFAULT_SEED;
        }

        /// <summary>
        /// Creates a new XorShift128Plus generator from a 32-bit seed.
        /// </summary>
        /// <param name="seed">Initial seed value.</param>
        /// <returns>A new generator.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static XorShift128Plus FromUInt(uint seed)
        {
            return new XorShift128Plus(seed);
        }

        /// <summary>
        /// Returns the next 64-bit unsigned integer in the sequence.
        /// This is the core XorShift128+ step.
        /// </summary>
        /// <returns>The next 64-bit unsigned integer.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong NextULong()
        {
            ulong s1 = _s0;
            ulong s0 = _s1;

            _s0 = s0;

            s1 ^= s1 << 23;
            _s1 = s1 ^ s0 ^ (s1 >> 17) ^ (s0 >> 26);

            return _s1 + s0;
        }

        /// <summary>
        /// Returns a 32-bit unsigned integer.
        /// </summary>
        /// <returns>The upper 32 bits of the next 64-bit value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint NextUInt()
        {
            return (uint)(NextULong() >> 32);
        }

        /// <summary>
        /// Returns a float in the range [0, 1).
        /// </summary>
        /// <returns>A float in the range [0, 1).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float NextFloat01()
        {
            return (NextULong() >> 40) * (1f / (1UL << 24));
        }

        /// <summary>
        /// Returns a float in the range [min, max).
        /// </summary>
        /// <param name="min">Inclusive lower bound.</param>
        /// <param name="max">Exclusive upper bound.</param>
        /// <returns>A float in the range [min, max).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float NextFloat(float min, float max)
        {
            return min + NextFloat01() * (max - min);
        }

        /// <summary>
        /// Returns an integer in the range [0, maxExclusive).
        /// </summary>
        /// <param name="maxExclusive">Exclusive upper bound; must be greater than zero.</param>
        /// <returns>An integer in the range [0, maxExclusive).</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxExclusive"/> is zero or negative.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int NextInt(int maxExclusive)
        {
            if (maxExclusive <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxExclusive));

            return (int)(NextUInt() % (uint)maxExclusive);
        }

        /// <summary>
        /// Returns an integer in the range [minInclusive, maxExclusive).
        /// </summary>
        /// <param name="minInclusive">Inclusive lower bound.</param>
        /// <param name="maxExclusive">Exclusive upper bound; must be greater than <paramref name="minInclusive"/>.</param>
        /// <returns>An integer in the range [minInclusive, maxExclusive).</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="minInclusive"/> is not less than <paramref name="maxExclusive"/>.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int NextInt(int minInclusive, int maxExclusive)
        {
            if (minInclusive >= maxExclusive)
                throw new ArgumentOutOfRangeException();

            return minInclusive + NextInt(maxExclusive - minInclusive);
        }

        /// <summary>
        /// Captures the internal RNG state so it can be saved and restored later.
        /// Useful for save games, deterministic replays, and network sync.
        /// </summary>
        /// <returns>A snapshot of the current generator state.</returns>
        public XorShift128PlusState SaveState()
        {
            return new XorShift128PlusState(_s0, _s1);
        }

        /// <summary>
        /// Restores a previously saved RNG state.
        /// This recreates the exact sequence continuation.
        /// </summary>
        /// <param name="state">State previously captured with <see cref="SaveState"/>.</param>
        /// <returns>A generator that continues the saved sequence.</returns>
        public static XorShift128Plus FromState(XorShift128PlusState state)
        {
            var rng = new XorShift128Plus(1);
            rng._s0 = state.S0;
            rng._s1 = state.S1;
            return rng;
        }
    }

    /// <summary>
    /// Serializable state container for XorShift128Plus.
    /// Stores the two 64-bit state values.
    /// </summary>
    public readonly struct XorShift128PlusState
    {
        /// <summary>
        /// First 64-bit state word.
        /// </summary>
        public readonly ulong S0;

        /// <summary>
        /// Second 64-bit state word.
        /// </summary>
        public readonly ulong S1;

        /// <summary>
        /// Creates a new state snapshot.
        /// </summary>
        /// <param name="s0">First 64-bit state word.</param>
        /// <param name="s1">Second 64-bit state word.</param>
        public XorShift128PlusState(ulong s0, ulong s1)
        {
            S0 = s0;
            S1 = s1;
        }
    }
}
