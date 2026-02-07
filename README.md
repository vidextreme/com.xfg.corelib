![xfg corelib](Docs/xfg-corelib.png#gh-dark-mode-only)
![xfg corelib](Docs/xfg-corelib-black.png#gh-dark-mode-only#gh-light-mode-only)

### Welcome to XFG Simple Game Core Library

A lightweight, engine‑agnostic C# foundation for reliable gameplay systems.  
Built for clarity, determinism, and extensibility, with optional Unity/Burst layers for performance and debugging.

---

## ✨ Features

### 🤖 PRNG Utilities
Deterministic pseudorandom generators for reproducible gameplay, procedural generation, and testing.

#### Current Implementations
- **XorShift128Plus** — fast, high‑quality PRNG with a
- **SplitMix64** — robust seeding algorithm for initializing PRNG states
- **PCG32** — statistically sound PRNG with excellent distribution properties

[Pseudorandom Number Generator (PRNG) Readme](Docs/README-Random.md)

## 🧠 StateMachine System

A lightweight, extensible state machine framework designed for gameplay, AI, UI flow, and asynchronous logic.

### Core Features
- **Synchronous StateMachine** — simple, predictable, engine‑agnostic  
- **AsyncStateMachine** — supports async/await for loading flows, network waits, cutscenes, etc.  
- **HFSM (Hierarchical StateMachine)** — nested parent/child states for layered behaviors  
- **Pushdown StateMachine (Stack‑based FSM)** — supports state stacking, pausing, and resuming  
- **Explicit Enter/Exit semantics** — clean lifecycle boundaries  
- **Strong typing** — explicit state classes, discoverable and testable  
- **Minimal boilerplate** — fast to onboard, easy to extend  

### Design Goals
- Deterministic behavior  
- Clear separation of concerns  
- Easy debugging and logging  
- Works in any .NET environment (Unity optional)

### Hierarchical FSM Capabilities
- Parent states own child states  
- Enter/Exit automatically bubble through the hierarchy  
- Shared logic at higher levels, specialized behavior in leaf states  
- Ideal for AI, combat systems, UI flows, and layered gameplay logic  

### Pushdown FSM Capabilities
- Stack‑based state transitions (`Push`, `Pop`, `Replace`)  
- Perfect for menus, modal UI, pause screens, nested gameplay modes  
- States can be paused and resumed without losing internal state  
- Clean separation between transient and persistent behaviors  


[State Machine Readme](Docs/README-StateMachine.md)


### 📐 Geometry & Math Utilities

A clean, engine‑agnostic geometry layer providing collision‑ready primitives and spatial reasoning tools.

#### Core Primitives
- **Line** — infinite line for projections and analytic geometry  
- **Ray** — semi‑infinite ray for sensing and intersection scaffolding  
- **Line Segment** — finite segment for hit detection and navigation logic  
- **Triangle** — fundamental surface primitive for barycentric math and mesh queries  
- **AABB** — axis‑aligned bounding box for broad‑phase culling and grid‑based partitioning  
- **OBB** — oriented bounding box for precise collision envelopes and SAT‑based checks  
- **Frustum** — camera‑style frustum for visibility, culling, and spatial queries  
- **Cone** — directional volume for AI sensing, field‑of‑view, and detection cones  
- **Cylinder** — analytic cylinder for volume checks and radial constraints  
- **Capsule** — segment‑based capsule for character collision, sweeps, and physics queries  
- **BoundingSphere** — fast broad‑phase culling, distance checks, and spatial queries  

#### Burst Variants
- **BoundingSphereBurst** — SIMD‑friendly, Unity.Mathematics‑based version for high‑throughput jobs  

[Collision Readme](Docs/README-Collision.md)

#### Debug Integration
- **ShapeDebug** — optional Unity‑only visualization layer (editor‑only, zero‑cost in builds)

[ShapeDebug Readme](Docs/README-ShapeDebug.md)

**Design Philosophy:**  
Pure C# core → optional Unity/Burst layers → optional debug layer.  
Modular, deterministic, and future‑proof for higher‑level collision systems.

---

## 📦 Installation

Install via Unity Package Manager using Git URL:

```https://github.com/vidextreme/com.xfg.corelib.git```


Or clone the repository directly into your project’s `Packages/` folder.

---

## 🗺️ Roadmap

Planned additions include:

- Additional PRNG algorithms (Xoshiro256**, Xoshiro128++, Mersenne Twister, PCG64)
- Expanded collision/intersection tests (Ray–Triangle, Capsule–Capsule, SAT)  
- More debug helpers  
- Additional Burst‑optimized variants 

---

## 📄 License

This project is released under the **MIT License**.


[Join the community!](https://discord.gg/3GCxggFA6q)

