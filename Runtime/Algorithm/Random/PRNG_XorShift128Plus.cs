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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static XorShift128Plus FromUInt(uint seed)
        {
            return new XorShift128Plus(seed);
        }

        /// <summary>
        /// Returns the next 64-bit unsigned integer in the sequence.
        /// This is the core XorShift128+ step.
        /// </summary>
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint NextUInt()
        {
            return (uint)(NextULong() >> 32);
        }

        /// <summary>
        /// Returns a float in the range [0, 1).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float NextFloat01()
        {
            return (NextULong() >> 40) * (1f / (1UL << 24));
        }

        /// <summary>
        /// Returns a float in the range [min, max).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float NextFloat(float min, float max)
        {
            return min + NextFloat01() * (max - min);
        }

        /// <summary>
        /// Returns an integer in the range [0, maxExclusive).
        /// </summary>
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
        public XorShift128PlusState SaveState()
        {
            return new XorShift128PlusState(_s0, _s1);
        }

        /// <summary>
        /// Restores a previously saved RNG state.
        /// This recreates the exact sequence continuation.
        /// </summary>
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
        public readonly ulong S0;
        public readonly ulong S1;

        public XorShift128PlusState(ulong s0, ulong s1)
        {
            S0 = s0;
            S1 = s1;
        }
    }
}
