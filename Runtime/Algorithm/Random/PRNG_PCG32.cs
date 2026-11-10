// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

using System;
using System.Runtime.CompilerServices;

namespace XFG.Algorithm
{
    /// <summary>
    /// PCG32: 32-bit pseudorandom number generator with 64-bit state and stream.
    /// Deterministic, statistically strong, and suitable for gameplay and simulations.
    /// Not cryptographically secure.
    /// </summary>
    public struct Pcg32 : IRandom
    {
        private ulong _state;
        private ulong _increment;

        /// <summary>
        /// Creates a new PCG32 generator with the given seed and stream.
        /// The stream value is forced to be odd internally.
        /// </summary>
        /// <param name="seed">Initial seed value.</param>
        /// <param name="stream">Stream selector; different streams produce independent sequences.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Pcg32(ulong seed, ulong stream)
        {
            _state = 0UL;
            _increment = (stream << 1) | 1UL;

            Step();
            _state += seed;
            Step();
        }

        /// <summary>
        /// Creates a new PCG32 generator from a 64-bit seed and a default stream.
        /// </summary>
        /// <param name="seed">Initial seed value.</param>
        /// <returns>A new generator.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Pcg32 FromSeed(ulong seed)
        {
            const ulong DEFAULT_STREAM = 0x853C49E6748FEA9BUL;
            return new Pcg32(seed, DEFAULT_STREAM);
        }

        /// <summary>
        /// Creates a new PCG32 generator from a 32-bit seed.
        /// </summary>
        /// <param name="seed">Initial seed value.</param>
        /// <returns>A new generator using the default stream.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Pcg32 FromUInt(uint seed)
        {
            return FromSeed(seed);
        }

        /// <summary>
        /// Advances the internal state by one step.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Step()
        {
            const ulong MULTIPLIER = 6364136223846793005UL;
            _state = _state * MULTIPLIER + _increment;
        }

        /// <summary>
        /// Returns the next 32-bit unsigned integer in the sequence.
        /// </summary>
        /// <returns>The next 32-bit unsigned integer.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint NextUInt()
        {
            ulong oldState = _state;
            Step();

            uint xorshifted = (uint)(((oldState >> 18) ^ oldState) >> 27);
            uint rot = (uint)(oldState >> 59);

            return (xorshifted >> (int)rot) | (xorshifted << (int)((-rot) & 31));
        }

        /// <summary>
        /// Returns a float in the range [0, 1).
        /// </summary>
        /// <returns>A float in the range [0, 1).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float NextFloat01()
        {
            uint value = NextUInt() >> 8;
            return value * (1f / (1u << 24));
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
        /// Captures the internal PCG32 state so it can be saved and restored later.
        /// Useful for save games, deterministic replays, and network sync.
        /// </summary>
        /// <returns>A snapshot of the current generator state.</returns>
        public Pcg32State SaveState()
        {
            return new Pcg32State(_state, _increment);
        }

        /// <summary>
        /// Restores a previously saved PCG32 state.
        /// This recreates the exact sequence continuation.
        /// </summary>
        /// <param name="state">State previously captured with <see cref="SaveState"/>.</param>
        /// <returns>A generator that continues the saved sequence.</returns>
        public static Pcg32 FromState(Pcg32State state)
        {
            var rng = new Pcg32(0, 1);
            rng._state = state.State;
            rng._increment = state.Increment;
            return rng;
        }
    }

    /// <summary>
    /// Serializable state container for PCG32.
    /// Stores the 64-bit state and 64-bit stream increment.
    /// </summary>
    public readonly struct Pcg32State
    {
        /// <summary>
        /// The 64-bit internal state.
        /// </summary>
        public readonly ulong State;

        /// <summary>
        /// The 64-bit stream increment (always odd).
        /// </summary>
        public readonly ulong Increment;

        /// <summary>
        /// Creates a new state snapshot.
        /// </summary>
        /// <param name="state">The 64-bit internal state.</param>
        /// <param name="increment">The 64-bit stream increment.</param>
        public Pcg32State(ulong state, ulong increment)
        {
            State = state;
            Increment = increment;
        }
    }
}
