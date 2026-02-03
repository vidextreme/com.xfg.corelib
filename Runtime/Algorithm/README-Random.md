# Pseudorandom Number Generator (PRNG) — XFG Simple Game Core Library

This document summarizes the pseudorandom number utilities in `Runtime/Algorithm/` and shows usage examples and a compact comparison.

Implemented primitives

- `SplitMix64` — high-quality 64-bit scramble function used to expand seeds. Typically used to initialize PRNG state; it can also be used as a simple generator for short sequences.
- `XorShift128Plus` — 128-bit xorshift+ generator implemented as `XorShift128Plus` (implements `IRandom`). Fast, deterministic, and suitable for gameplay and simulation where cryptographic strength is not required.

Design goals

- Deterministic outputs for a given seed (reproducible simulations and tests)
- Low allocation and high throughput for runtime use
- Simple API surface via `IRandom` for interoperability between different generator implementations
- Non-cryptographic; not suitable for security-sensitive uses

Algorithm details

SplitMix64
- Purpose: seed expansion / scrambling
- Behavior: given a 64-bit state `x`, `SplitMix64(ref x)` mutates `x` and returns a well-scrambled 64-bit value. It mixes bits thoroughly and is fast.
- Use: initialize the internal state of larger PRNGs (e.g., XorShift128Plus) to avoid correlated starting states.

XorShift128Plus
- Type: xorshift+ family (128-bit state)
- State: two 64-bit unsigned integers (`_s0`, `_s1`)
- Period: 2^128 - 1 (practical period for well-behaved seeds)
- Output: 64-bit (primary) and the `IRandom` methods expose 32-bit, floats in [0,1), and integer ranges
- Seeding: constructed from a 64-bit seed that is expanded via `SplitMix64` to populate the two 64-bit state values
- Performance: very fast, minimal branches and integer operations; suitable for tight loops and game logic
- Quality: good statistical properties for gameplay and simulations; not cryptographically secure

Usage examples

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

var rng = new XorShift128Plus(123456789UL);
for (int i = 0; i < 5; i++)
{
    uint u = rng.NextUInt();
    float f = rng.NextFloat01();
    int r = rng.NextInt(0, 100);
    UnityEngine.Debug.Log($"u={u}, f={f}, r={r}");
}
```

Practical notes

- Determinism: given the same seed each `XorShift128Plus` instance produces the same sequence. Use this for reproducible simulations and deterministic gameplay.
- Threading: generators are not thread-safe. For multithreaded deterministic streams, instantiate a separate PRNG per thread or use synchronization.
- Seeding: avoid seeding different PRNGs with similar small integers directly. Use `SplitMix64` to expand a small seed into diverse internal state values.

Comparison

The table below compares `XorShift128Plus` and `SplitMix64` (as a seeding/scrambling primitive and a lightweight generator). Interpret `Quality` in terms of statistical properties for simulation (not cryptographic strength).

| Algorithm         | State size | Period       | Speed     | Quality (sim)   | Typical use                                    |
|------------------:|:----------:|:------------:|:---------:|:----------------:|:-----------------------------------------------|
| XorShift128Plus   | 128 bits   | ~2^128       | Very fast | Good            | Simulation, deterministic gameplay RNG         |
| SplitMix64        | 64 bits    | 2^64 (as gen)| Very fast | Moderate        | Seed expansion, short-use generator            |

Diagram notes
- "Speed" is qualitative: both are implemented with a few integer operations and are suitable for performance-critical code.
- "Quality" reflects suitability for statistical simulation and uniformity; neither is cryptographically secure.
- Use `SplitMix64` primarily for seeding larger generators; use `XorShift128Plus` for generating long reproducible streams.

Extensions and suggestions

- If you need better statistical guarantees for Monte-Carlo or scientific use, consider PCG or xoshiro/xoroshiro variants with larger state and known equidistribution properties.
- For deterministic parallel streams, use a jump-ahead method or instantiate per-thread PRNGs with distinct seeds derived from a master seed via `SplitMix64`.

