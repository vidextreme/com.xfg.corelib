![xfg corelib](Docs/xfg-corelib.png#gh-dark-mode-only)
![xfg corelib](Docs/xfg-corelib-black.png#gh-dark-mode-only#gh-light-mode-only)

### Welcome to XFG Simple Game Core Library

A lightweight, engine‑agnostic C# foundation for reliable gameplay systems.  
Built for clarity, determinism, and extensibility, with optional Unity/Burst layers for performance and debugging.

---

## ✨ Features

### 🤖 PRNG Utilities
Deterministic pseudorandom generators for reproducible gameplay, procedural generation, and testing.

[Pseudorandom Number Generator (PRNG) Readme](Docs/README-Random.md)

#### Current Implementations
- **XorShift128Plus** — fast, high‑quality PRNG with a
- **SplitMix64** — robust seeding algorithm for initializing PRNG states
- **PCG32** — statistically sound PRNG with excellent distribution properties


### 🧠 StateMachine System

A flexible, engine‑agnostic framework for gameplay, AI, UI flow, and asynchronous logic, with optional Unity Inspector serialization for debugging and authoring.

[State Machine Readme](Docs/README-StateMachine.md)

#### Core Features
- **StateMachine** — simple, predictable synchronous FSM  
- **AsyncStateMachine** — async/await support for loading, networking, cutscenes  
- **HFSM (Hierarchical FSM)** — parent/child states for layered behaviors  
- **Pushdown FSM** — stack‑based states with PushState<T>(), PopState(), ReplaceState<T>()  
- **Unity Inspector Serialization** — FSMs and states can be serialized and visualized in the Unity Editor  
- **Explicit Enter/Exit** — clean lifecycle boundaries  
- **Strong typing** — explicit, testable state classes  

#### Unity Serialization Support
- FSMs implement **ISerializableStateMachine** for Unity‑friendly serialization  
- States implement **ISerializableState** to expose internal data in the Inspector  
- Supports serialization of:
  - Active state  
  - HFSM hierarchy  
  - Pushdown stack contents  
  - State‑specific fields  
- Enables:
  - Inspector debugging  
  - Authoring workflows  
  - Live state visualization  
  - Editor tooling and extensions  

This system is designed for Unity workflows but remains engine‑agnostic at its core.

#### HFSM Capabilities
- Nested states with shared parent logic  
- Automatic Enter/Exit bubbling  
- Ideal for AI, combat, UI, and multi‑layered systems  

#### Pushdown FSM Capabilities
- **PushState<TState>()** — push a new state on the stack  
- **PopState()** — pop and resume the previous state  
- **ReplaceState<TState>()** — atomic replace without resuming underlying state  
- Perfect for menus, modal UI, pause screens, nested gameplay modes  

## 🧠 Utility AI System

A modular, designer‑friendly Utility AI framework built for scalable decision‑making in gameplay and AI systems.

[Utility AI Readme](Docs/README-UtilityAI.md)

### Core Features
- **Action‑based architecture** — each action defines its own scoring logic  
- **Weighted scoring** — combine multiple considerations into a final utility value  
- **Considerations** — reusable scoring components (curves, clamps, multipliers, timers)  
- **Temporal control** — cooldowns, score decay, and gating logic  
- **Deterministic evaluation** — stable, predictable decision outcomes  
- **Engine‑agnostic core** — pure C# logic with optional Unity Inspector integration  

### Unity Inspector Support
- Fully serializable actions, considerations, and AI agents  
- Designer‑friendly inspector layout for tuning and debugging  
- Optional live score visualization  
- Supports nested consideration graphs  

### Design Goals
- Scalable for large AI systems  
- Easy to author and debug  
- Predictable and deterministic  
- Extensible for custom scoring logic  


### 📐 Geometry & Math Utilities

A clean, engine‑agnostic geometry layer providing collision‑ready primitives and spatial reasoning tools.

[Collision Readme](Docs/README-Collision.md)

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
- **Sphere** — fast broad‑phase culling, distance checks, and spatial queries  

**Design Philosophy:**  
Pure C# core → optional Unity/Burst layers → optional debug layer.  
Modular, deterministic, and future‑proof for higher‑level collision systems.

#### Burst Variants
- **BoundingSphereBurst** — SIMD‑friendly, Unity.Mathematics‑based version for high‑throughput jobs  

#### Debug Integration
- **ShapeDebug** — optional Unity‑only visualization layer (editor‑only, zero‑cost in builds)

[ShapeDebug Readme](Docs/README-ShapeDebug.md)


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

