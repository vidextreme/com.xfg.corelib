# 🚫 Why Not to Use `UnityEngine.Random`

Unity’s built‑in RNG is convenient, but it isn’t designed for deterministic gameplay, networking, DOTS, or large‑scale procedural generation.  
The table below summarizes the core limitations and how the **XFG PRNG Suite** addresses them.

## UnityEngine.Random vs XFG PRNG Suite

| Feature / Requirement                 | UnityEngine.Random                               | XFG PRNG Suite (XorShift128Plus, PCG32, SplitMix64) |
|--------------------------------------|--------------------------------------------------|------------------------------------------------------|
| Deterministic across platforms        | ❌                                                | ✅                                                    |
| Deterministic across Unity versions   | ❌                                                | ✅                                                    |
| Burst-compatible                      | ❌                                                | ✅                                                    |
| DOTS / Jobs safe                      | ❌                                                | ✅                                                    |
| Global state                          | ❌ Global, opaque state                           | ✅ `Core.Random` (and per-instance RNGs)                             |
| State size                            | ❌ Small but non-deterministic, global            | ✅ Tiny (16–32 bytes)                                 |
| Performance                           | ❌ Slow for real-time systems                     | ✅ Extremely fast                                     |
| Parallel streams                      | ❌                                                | ✅                                                    |
| Replay-safe                           | ❌                                                | ✅                                                    |
| Serialization                         | ❌ Heavy, slow                                    | ✅ Lightweight, trivial                               |
| Designed for deterministic simulation | ❌                                                | ✅                                                    |
| Designed for procedural generation    | ⚠️ Usable but slow                                   | ✅ Optimized                                          |
| Designed for networking / lockstep    | ❌                                                | ✅                                                    |
| Algorithm transparency                | ❌ Xorshift128 (hidden implementation)            | ✅ Fully documented                                   |

**Implementation:** https://github.com/vidextreme/com.xfg.corelib


---

# 🔍 Unity’s Xorshift128 vs Xorshift128Plus

Unity internally uses **Xorshift128**, an older Marsaglia algorithm with known statistical weaknesses and global state issues.  
The XFG suite uses **Xorshift128Plus**, a modern, higher‑quality generator designed by Vigna.

## Xorshift128 (Unity) vs Xorshift128Plus (XFG)

| Property            | Unity Xorshift128 (Marsaglia 2003)         | Xorshift128Plus (Vigna 2014, XFG)                 |
|---------------------|---------------------------------------------|---------------------------------------------------|
| Algorithm family    | Xorshift                                    | Xorshift+                                         |
| Output width        | 32‑bit                                      | 64‑bit                                            |
| State size          | 128‑bit                                     | 128‑bit                                           |
| Statistical quality | Weak                                        | Strong                                            |
| BigCrush            | Fails several tests                         | Passes (except known low‑bit linearity)           |
| Parallel streams    | Unsafe (global state)                       | Safe with proper seeding                          |
| Determinism         | Not guaranteed across platforms/versions    | Guaranteed                                        |
| Thread safety       | No                                          | Yes (per‑instance RNGs)                           |
| Burst/DOTS safe     | No                                          | Yes                                               |
| Use cases           | Casual randomness                           | Simulation, procedural gen, networking, lockstep  |
