# PRNG — XFG Simple Game Core Library

This document summarizes the pseudorandom number generator utilities included in `Runtime/Algorithm/` and shows a few usage examples.

Files
- `PRNG_Common.cs` — common helpers and the `IRandom` interface. Also exposes a global `Core.Random` instance.
- `PRNG_XorShift128Plus.cs` — implementation of `XorShift128Plus : IRandom` (fast, deterministic, non-cryptographic PRNG).
- `WeightedChance.cs` — helpers for selecting items by weighted probability using an `IRandom`.

Overview

IRandom
- A small, consistent RNG interface used across the library:
  - `uint NextUInt()`
  - `float NextFloat01()` — returns a value in `[0,1)`
  - `float NextFloat(float min, float max)` — returns a value in `[min,max)`
  - `int NextInt(int maxExclusive)` — returns in `[0, maxExclusive)`
  - `int NextInt(int minInclusive, int maxExclusive)` — bounded integer

Core.Random
- `Core` exposes a global `IRandom Random = new XorShift128Plus(9001UL)` in `PRNG_Common.cs`.
- Use `Core.Random` when you want a single shared deterministic RNG across systems.

XorShift128Plus
- Fast, deterministic 128-bit xorshift+ generator.
- Construct with a 64-bit seed (non-zero recommended). The seed is expanded via `SplitMix64` to initialize internal state.
- API includes `NextULong()`, plus the `IRandom` methods.
- Not suitable for cryptographic use.

Weighted chance
- `Core.RollWeightedChance<T>(ReadOnlySpan<T> items, IRandom rng)` selects an item from a span using each item's `Weight` property.
- There's also an overload that uses `Core.Random`.

Examples (C#)

- Using the global RNG
```csharp
using XFG.Algorithm;

// draw a few values from the global RNG
float p = Core.Random.NextFloat01();
int idx = Core.Random.NextInt(10);
float scaled = Core.Random.NextFloat(-5f, 5f);
uint raw = Core.Random.NextUInt();
```

- Creating your own deterministic RNG
```csharp
using XFG.Algorithm;

// Create an independent PRNG with a fixed seed for reproducible tests
var rng = new XorShift128Plus(123456789UL);

for (int i = 0; i < 5; i++)
{
    uint u = rng.NextUInt();
    float f = rng.NextFloat01();
    int r = rng.NextInt(0, 100);
    UnityEngine.Debug.Log($"u={u}, f={f}, r={r}");
}
```

- Using weighted chance
```csharp
using System;
using XFG.Algorithm;

struct Choice : Core.IWeighted
{
    public string Name;
    public float Weight { get; }

    public Choice(string name, float weight)
    {
        Name = name;
        Weight = weight;
    }
}

// select using the global RNG
Choice[] items = new[]
{
    new Choice("common", 70f),
    new Choice("rare", 25f),
    new Choice("epic", 5f),
};

Choice pick = Core.RollWeightedChance(items);

// select using a local RNG for deterministic simulation
var localRng = new XorShift128Plus(42UL);
Choice pick2 = Core.RollWeightedChance(items, localRng);
```

Notes
- The implementation uses `SplitMix64` to expand seeds and `XorShift128Plus` for speed and reproducibility.
- All RNGs are deterministic given the same initial seed.
- The RNGs are not thread-safe; if you need parallel deterministic streams, instantiate separate PRNGs per thread.

If you want, I can add a small unit-test file demonstrating deterministic sequences or add utility extensions (e.g. `Shuffle<T>(this Span<T>, IRandom)`).
