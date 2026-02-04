Random utilities and Burst-friendly PRNG usage

Overview

This folder contains deterministic, lightweight pseudo-random number generators and helper extensions used throughout the XFG core library:

- `PRNG_XorShift128Plus.cs` — XorShift128Plus implementation (struct, value-type) suitable for Burst.
- `PRNG_PCG32.cs` — PCG32 implementation (struct) suitable for Burst.
- `PRNG_Common.cs` — shared helpers (e.g. `SplitMix64`) and the managed `IRandom` API.
- `RandomUtilityExtensions.cs` — convenience helpers for managed code (ranges, sampling, etc.).
- `RandomUtilityExtensionsBurst.cs` — Burst-friendly helper methods and no-allocation utilities intended for use inside Burst-compiled jobs and structs.

Notes on Burst-friendly usage

The provided PRNGs are implemented as value-type structs with pure methods (`NextUInt`, `NextFloat01`, `NextFloat`, ...). To use them safely and efficiently with Unity Burst and Jobs:

- Do not share a single mutable RNG instance across threads without synchronization. Instead create one independent RNG per job/worker.
- Seed per-worker RNGs deterministically using a global seed plus the worker index. For stronger seeding, use `Core.SplitMix64(ref seed)` to derive different 64-bit seeds.
- Keep RNG stored as a struct field inside the job (or as an element of a `NativeArray`), so Burst can inline and optimize calls without managed allocations.

Example: simple Burst job using `Pcg32`

```csharp
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
using XFG.Algorithm;

[BurstCompile]
public struct FillRandomJob : IJobParallelFor
{
    public NativeArray<uint> outValues;
    public ulong globalSeed;

    public void Execute(int index)
    {
        // derive a per-index seed deterministically
        ulong s = globalSeed + (ulong)index;
        // expand/munge if desired via Core.SplitMix64(ref s)

        var rng = Pcg32.FromSeed(s);

        // use RNG in Burst: no allocations
        outValues[index] = rng.NextUInt();
    }
}
```

Managed convenience helpers

Use `RandomUtilityExtensions` from regular (non-Burst) code for functions that may allocate or use managed types (for example sampling arrays, selecting weighted choices, etc.).

When to use `RandomUtilityExtensionsBurst`

Use `RandomUtilityExtensionsBurst` inside Burst-compiled code paths where allocation-free, deterministic helpers are required. These helpers are implemented to avoid boxing, managed heap usage and to keep code Burst-compatible.

Summary / Best practices

- Prefer struct PRNGs (`Pcg32`, `XorShift128Plus`) inside Burst jobs.
- Seed per-worker RNGs deterministically (global seed + index) or using `SplitMix64` for robust derivation.
- Use `RandomUtilityExtensions` for managed convenience and `RandomUtilityExtensionsBurst` for Burst-safe helpers.

If you want, I can add a short example showing using `XorShift128Plus` in a parallel job and deriving seeds with `SplitMix64`.