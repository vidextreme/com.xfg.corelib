# Pseudorandom Number Generator (PRNG) — XFG Simple Game Core Library
### Deterministic, Burst‑friendly random utilities for Unity

This package provides a deterministic, allocation‑free pseudorandom number system designed for gameplay, simulation, DOTS, and Burst‑compiled jobs. It includes:

- High‑performance PRNG implementations (`XorShift128Plus`, `PCG32`)
- Seed expansion utilities (`SplitMix64`)
- Unity‑style random helpers (range sampling, vector sampling, quaternion sampling)
- Managed and Burst‑friendly extension sets
- A deterministic drop‑in replacement for `UnityEngine.Random`

The system is built around the `IRandom` interface, enabling multiple PRNG algorithms to be used interchangeably.

[Why Not to Use UnityEngine.Random Readme](README-WhyNotUseUnityRandom.md)

## 🔢 Implemented primitives

- `SplitMix64` — high-quality 64-bit scramble function used to expand seeds. Typically used to initialize PRNG state; it can also be used as a simple generator for short sequences.
- `XorShift128Plus` — 128-bit xorshift+ generator implemented as `XorShift128Plus` (implements `IRandom`). Fast, deterministic, and suitable for gameplay and simulation where cryptographic strength is not required.
- `PCG32` — 64-bit PCG generator implemented as `Pcg32` (implements `IRandom`). Good statistical quality and speed; suitable for simulations and games.

## 🎯 Design goals

- Deterministic outputs for a given seed (reproducible simulations and tests)
- Low allocation and high throughput for runtime use
- Simple API surface via `IRandom` for interoperability between different generator implementations
- Non-cryptographic; not suitable for security-sensitive uses

## 🧠 Algorithm details

### SplitMix64
- **Purpose:** seed expansion / scrambling
- **Behavior:** given a 64-bit state `x`, `SplitMix64(ref x)` mutates `x` and returns a well-scrambled 64-bit value. It mixes bits thoroughly and is fast.
- **Use:** initialize the internal state of larger PRNGs (e.g., XorShift128Plus) to avoid correlated starting states.
- **Performance:** very fast, few integer operations
- **Quality:** good statistical properties for seeding; not suitable as a long-term generator on its own
- **Use case:** seeding other PRNGs, short-term random values
- **Cryptography:** not suitable for cryptographic use
- **Notes:** avoid using small integer seeds directly for multiple PRNGs; use `SplitMix64` to diversify state.

### XorShift128Plus
- **Type:** xorshift+ family (128-bit state)
- **State:** two 64-bit unsigned integers (`_s0`, `_s1`)
- **Period:** 2^128 - 1 (practical period for well-behaved seeds)
- **Output:** 64-bit (primary) and the `IRandom` methods expose 32-bit, floats in [0,1), and integer ranges
- **Seeding:** constructed from a 64-bit seed that is expanded via `SplitMix64` to populate the two 64-bit state values
- **Performance:** very fast, minimal branches and integer operations; suitable for tight loops and game logic
- **Quality:** good statistical properties for gameplay and simulations; not cryptographically secure
- **Use case:** general-purpose RNG for games, simulations, procedural generation
- **Cryptography:** not suitable for cryptographic use
- **Notes:** avoid seeding multiple instances with similar small integers directly; use `SplitMix64` to diversify state.

### PCG32
- **Type:** Permuted Congruential Generator (PCG) family
- **State:** single 64-bit unsigned integer
- **Period:** 2^64
- **Output:** 32-bit unsigned integer, with methods for floats and ranges
- **Seeding:** constructed from a 64-bit seed
- **Performance:** fast, with a few more operations than XorShift128Plus
- **Quality:** better statistical quality than simpler generators; suitable for simulations requiring higher quality randomness
- **Use case:** scientific simulations, procedural generation where statistical quality is important
- **Cryptography:** not suitable for cryptographic use

## 🔢 Usage examples

- Use the shared/global RNG (if present in your build)
```csharp
using XFG.Algorithm;

// Example: draw a few values from the global RNG
float p = Core.Random.NextFloat01();
int idx = Core.Random.NextInt(10);
float scaled = Core.Random.NextFloat(-5f, 5f);
uint raw = Core.Random.NextUInt();
```

- Create an independent deterministic RNG for tests or deterministic simulations
```csharp
using XFG.Algorithm;

var rng = new XorShift128Plus(123456789UL); // or new Pcg32(9001);
for (int i = 0; i < 5; i++)
{
    uint u = rng.NextUInt();
    float f = rng.NextFloat01();
    int r = rng.NextInt(0, 100);
    UnityEngine.Debug.Log($"u={u}, f={f}, r={r}");
}
```
### SplitMix64 Usage Example

```csharp
using XFG.Algorithm;

public static class SplitMix64Example
{
    public static void Run()
    {
        // Start with any 64‑bit seed (user‑provided, time‑based, hashed, etc.)
        ulong seed = 123456789UL;

        // SplitMix64 mutates the seed and returns a well‑scrambled 64‑bit value.
        // Each call advances the internal state.
        ulong a = Core.SplitMix64(ref seed);
        ulong b = Core.SplitMix64(ref seed);
        ulong c = Core.SplitMix64(ref seed);

        // These values are now decorrelated and suitable for initializing PRNGs.
        UnityEngine.Debug.Log($"A={a}, B={b}, C={c}");
    }
}
```

# 🎲 Practical notes

- Determinism: given the same seed each `XorShift128Plus` instance produces the same sequence. Use this for reproducible simulations and deterministic gameplay.
- Threading: generators are not thread-safe. For multithreaded deterministic streams, instantiate a separate PRNG per thread or use synchronization.
- Seeding: avoid seeding different PRNGs with similar small integers directly. Use `SplitMix64` to expand a small seed into diverse internal state values.

## 📊 Comparison


| Algorithm         | State size | Period       | Speed     | Quality (sim)   | Typical use                                    |
|------------------:|:----------:|:------------:|:---------:|:----------------:|:-----------------------------------------------|
| XorShift128Plus   | 128 bits   | ~2^128       | Very fast | Good            | Simulation, deterministic gameplay RNG         |
| SplitMix64        | 64 bits    | 2^64 (as gen)| Very fast | Moderate        | Seed expansion, short-use generator            |
| PCG32            | 64 bits    | 2^64         | Fast      | Very good       | Scientific simulation, higher-quality RNG      |

## Diagram notes
- **Speed** is qualitative: both are implemented with a few integer operations and are suitable for performance-critical code.
- **Quality** reflects suitability for statistical simulation and uniformity; neither is cryptographically secure.
- Use `SplitMix64` or `Pcg32` primarily for seeding larger generators; use `XorShift128Plus` for generating long reproducible streams.

## Extensions and suggestions

- If you need better statistical guarantees for Monte-Carlo or scientific use, consider PCG or xoshiro/xoroshiro variants with larger state and known equidistribution properties.
- For deterministic parallel streams, use a jump-ahead method or instantiate per-thread PRNGs with distinct seeds derived from a master seed via `SplitMix64`.

---

# 🎲 Unity Random Drop‑In Replacement

This library includes a deterministic, Burst‑friendly replacement for `UnityEngine.Random`.

It provides Unity‑style APIs such as:

- Value sampling  
- Range sampling (float and int)  
- Inside/on unit circle sampling  
- Inside/on unit sphere sampling  
- Uniform quaternion rotation  
- Rotation around an axis  
- Random Euler rotations  

All deterministic, seedable, and allocation‑free.

---

# 📦 Namespaces

There are two extension sets depending on your environment.


## 1. Managed / MonoBehaviour / Editor Code

Use UnityEngine types (`Vector2`, `Vector3`, `Quaternion`).

- Namespace: **`XFG.Algorithm`**  
- Extensions: **`RandomUtilityExtensions`**  
- Intended for gameplay logic, MonoBehaviours, editor tools, and runtime scripts  

```csharp
using XFG.Algorithm;
```
You get:

- **`RandomUtilityExtensions`**
- UnityEngine‑based helpers
- Ideal for gameplay, editor tools, and runtime scripts


## 2. Burst / DOTS / Jobs / HPC Code

Use Unity.Mathematics types (`float2`, `float3`, `quaternion`).

- Namespace: **`XFG.AlgorithmBurst`**  
- Extensions: **`RandomUtilityExtensionBurst`**  
- Intended for Burst‑compiled jobs, DOTS systems, HPC math, and NativeArray‑based simulations  

```csharp
using XFG.AlgorithmBurst;
```
You get:

- **`RandomUtilityExtensionBurst`**
- Burst‑safe helpers
- Ideal for Jobs, DOTS, and HPC simulations

---


## Random utilities and Burst-friendly PRNG usage

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
## 🧠 Best Practices

- Use one RNG instance per system or per job  
- Seed deterministically using `SplitMix64` or a similar scheme  
- Choose the correct namespace for your environment (managed vs. Burst)  
- Avoid UnityEngine types inside Burst‑compiled code  

---

## Saving and Restoring Random State  
#### Short How-To Guide

Deterministic systems often need to save, restore, or serialize the state of a PRNG. All XFG PRNGs (XorShift128Plus, PCG32) expose their internal state so you can persist it safely.

### Why Save RNG State?
- Resume deterministic simulations
- Rewind or replay gameplay sequences
- Sync RNG state across networked clients
- Serialize PRNG state into save files

### Saving State
Each PRNG exposes its internal fields so you can store them:
- **XorShift128Plus**: two ulong values
- **PCG32**: one ulong state + one ulong increment

### Restoring State
To restore the RNG, construct a new instance using the saved values. The generator will continue producing the exact same sequence from that point.

### Example State Layouts
#### XorShift128Plus
- `state0`
- `state1`

#### PCG32
- `state`
- `increment`

### Best Practices
- Save state after the last random call of the frame
- Never share a single RNG instance across threads
- Use SplitMix64 to generate initial seeds
- Keep RNG state in your save file or ECS component for deterministic replay

### Code Example
```csharp
using XFG.Algorithm;

public struct SavedRngState
{
    public ulong state0;
    public ulong state1;
}

public static class RngStateExample
{
    public static SavedRngState Save(XorShift128Plus rng)
    {
        return new SavedRngState
        {
            state0 = rng.State0,
            state1 = rng.State1
        };
    }

    public static XorShift128Plus Restore(SavedRngState saved)
    {
        return new XorShift128Plus(saved.state0, saved.state1);
    }
}
```
This pattern works identically for **PCG32**, using its `State` and `Increment` fields.

---

## 🎯 Summary

This PRNG system provides:

- Deterministic, reproducible random sequences  
- A Unity‑style API surface  
- Separate Burst‑safe and managed‑safe extension sets  
- Zero allocations and high performance  
- Multiple PRNG algorithms (XorShift128Plus, PCG32)
- A clean separation between gameplay code and HPC math
