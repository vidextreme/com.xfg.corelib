using System;

namespace XFG.Algorithm
{
    public static partial class Core
    {
        public interface IWeighted
        {
            float Weight { get; }
        }

        #region RollWeightedChance

        /// <summary>
        /// Selects an item from the given span using weighted probability.
        /// Each item must expose a Weight property. Uses global Core.Random.
        /// </summary>
        /// <typeparam name="T">A type that implements IWeighted.</typeparam>
        /// <param name="items">A span of weighted items.</param>
        /// <returns>The selected item.</returns>
        public static T RollWeightedChance<T>(ReadOnlySpan<T> items) where T : IWeighted
        {
            return RollWeightedChance(items, Core.Random);
        }

        /// <summary>
        /// Selects an item from the given span using weighted probability.
        /// Each item must expose a Weight property. The provided RNG determines
        /// the random selection, allowing deterministic behavior when using
        /// deterministic RNG implementations such as XorShift128Plus or Pcg32.
        /// </summary>
        /// <typeparam name="T">A type that implements IWeighted.</typeparam>
        /// <param name="items">A span of weighted items.</param>
        /// <param name="rng">The random number generator to use.</param>
        /// <returns>The selected item.</returns>
        public static T RollWeightedChance<T>(ReadOnlySpan<T> items, IRandom rng)
            where T : IWeighted
        {
            if (items.Length == 0)
                throw new InvalidOperationException("Cannot roll from an empty span.");

            float total = 0f;

            // Sum weights
            for (int i = 0; i < items.Length; i++)
            {
                float w = items[i].Weight;
                if (w < 0f)
                    throw new InvalidOperationException("Weights cannot be negative.");

                total += w;
            }

            if (total <= 0f)
                throw new InvalidOperationException("Total weight must be greater than zero.");

            // Roll a value in [0, total)
            float r = rng.NextFloat(0f, total);

            // Walk the list and subtract weights until we find the winner
            for (int i = 0; i < items.Length; i++)
            {
                float w = items[i].Weight;
                if (r < w)
                    return items[i];

                r -= w;
            }

            // Fallback for floating point drift
            return items[^1];
        }

        #endregion
    }
}
