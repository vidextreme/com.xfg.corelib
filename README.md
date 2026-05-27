![xfg corelib](Docs/xfg-corelib.png#gh-dark-mode-only)
![xfg corelib](Docs/xfg-corelib-black.png#gh-dark-mode-only#gh-light-mode-only)

### Welcome to XFG Simple Game Core Library

A lightweight, engine-agnostic C# foundation for reliable gameplay systems.  
Built for clarity, determinism, and extensibility, with optional Unity/Burst layers for performance and debugging.

#### 📘 Quick Jump

- [🤖 PRNG Utilities](#-prng-utilities)
- [⚡ Global EventX](#-global-eventx)
- [🧠 StateMachine System](#-statemachine-system)
- [🧠 Utility AI System](#-utility-ai-system)
- [🎮 Actor–Controller Architecture](#-actorcontroller-architecture)
- [🏗️ TriScope Runtime System](#-triscope-runtime-system)
- [📐 Geometry & Math Utilities](#-geometry--math-utilities)
- [📦 Installation](#-installation)
- [🗺️ Roadmap](#-roadmap)
- [📄 License](#-license)

---

## ✨ Features

### 🤖 PRNG Utilities
Deterministic pseudorandom generators for reproducible gameplay, procedural generation, and testing.

[Pseudorandom Number Generator (PRNG) Readme](Docs/README-Random.md)

[Why Not to Use UnityEngine.Random Readme](Docs/README-WhyNotUseUnityRandom.md)

#### Current Implementations
- XorShift128Plus — fast, high-quality PRNG
- SplitMix64 — robust seeding algorithm for initializing PRNG states
- PCG32 — statistically sound PRNG with excellent distribution properties

---

### ⚡ Global EventX

A zero-allocation, thread-safe, strongly-typed event system for com.xfg.corelib. Designed for high-performance, engine-agnostic gameplay code without reflection, params object[], DynamicInvoke, or per-broadcast allocations.

#### Core Features
- Supports any KeyType (string, enum, struct, EventId, etc.)
- Strongly-typed delegates (Action, Action<T1>, Action<T1,T2>, Action<T1,T2,T3>, Action<T1,T2,T3,T4>)
- Zero allocations during broadcast
- Thread-safe via ConcurrentDictionary
- Up to 4 parameters per event
- Engine-agnostic C# core with optional Unity integration

[Global EventX Readme](Docs/README-GlobalEventX.md)

---

### 🧠 StateMachine System

A flexible, engine-agnostic framework for gameplay, AI, UI flow, and asynchronous logic, with optional Unity Inspector serialization for debugging and authoring.

[State Machine Readme](Docs/README-StateMachine.md)

#### Core Features
- StateMachine — simple, predictable synchronous FSM
- AsyncStateMachine — async/await support for loading, networking, cutscenes
- HFSM (Hierarchical FSM) — parent/child states for layered behaviors
- Pushdown FSM — stack-based states with PushState<T>(), PopState(), ReplaceState<T>()
- Unity Inspector Serialization — FSMs and states can be serialized and visualized in the Unity Editor
- Explicit Enter/Exit — clean lifecycle boundaries
- Strong typing — explicit, testable state classes

#### Unity Serialization Support
- FSMs implement ISerializableStateMachine for Unity-friendly serialization
- States implement ISerializableState to expose internal data in the Inspector
- Supports serialization of:
  - Active state
  - HFSM hierarchy
  - Pushdown stack contents
  - State-specific fields
- Enables:
  - Inspector debugging
  - Authoring workflows
  - Live state visualization
  - Editor tooling and extensions

This system is designed for Unity workflows but remains engine-agnostic at its core.

#### HFSM Capabilities
- Nested states with shared parent logic
- Automatic Enter/Exit bubbling
- Ideal for AI, combat, UI, and multi-layered systems

#### Pushdown FSM Capabilities
- PushState<TState>() — push a new state on the stack
- PopState() — pop and resume the previous state
- ReplaceState<TState>() — atomic replace without resuming underlying state
- Perfect for menus, modal UI, pause screens, nested gameplay modes

---

## 🧠 Utility AI System

A modular, designer-friendly Utility AI framework built for scalable decision-making in gameplay and AI systems.

[Utility AI Readme](Docs/README-UtilityAI.md)

### Core Features
- Action-based architecture — each action defines its own scoring logic
- Weighted scoring — combine multiple considerations into a final utility value
- Considerations — reusable scoring components (curves, clamps, multipliers, timers)
- Temporal control — cooldowns, score decay, and gating logic
- Deterministic evaluation — stable, predictable decision outcomes
- Engine-agnostic core — pure C# logic with optional Unity Inspector integration

### Unity Inspector Support
- Fully serializable actions, considerations, and AI agents
- Designer-friendly inspector layout for tuning and debugging
- Optional live score visualization
- Supports nested consideration graphs

### Design Goals
- Scalable for large AI systems
- Easy to author and debug
- Predictable and deterministic
- Extensible for custom scoring logic

---

### 🎮 Actor–Controller Architecture

A deterministic, engine-agnostic gameplay pattern built around a clear separation of responsibilities: Controllers decide what should happen, and Actors execute those decisions through a strongly-typed state machine and command buffer.

Inspired by Unreal Engine’s Actor–Controller model, adapted for explicit, portable C# workflows.

#### Core Concepts
- **Actor**
  - Owns the FSM and executes commands deterministically
  - Uses a command buffer to prevent mid-frame state changes
  - Sends Inform() messages back to the Controller
  - Defines behavior through polymorphic state classes

- **Controller**
  - Decides what the Actor should do
  - Issues typed commands (no direct state mutation)
  - Reacts to Actor feedback via Inform()
  - Can be swapped at runtime

  **Supported Controller Types**
  - Player Input Controller  
  - Behavior Tree (B3) Controller  
  - Utility AI Controller  
  - FSM-driven AI Controller  
  - Network/Remote Controller  
  - Scripted/Cutscene Controller  
  - Replay/Deterministic Playback Controller  

- **Command Buffer**
  - FIFO buffer ensuring deterministic behavior
  - Prevents unpredictable mid-frame transitions
  - Works consistently across AI, player, and network input

#### Benefits
- Deterministic and testable gameplay
- Clean separation of decision vs execution
- Scales to AI, networking, cutscenes, and tools
- Familiar to Unreal developers but simpler and engine-agnostic

[Actor–Controller Readme](Docs/README-ActorController.md)

---

### 🏗️ TriScope Runtime System

A deterministic, scope-driven runtime architecture built around three explicit execution layers:  
**Engine Scope**, **Group Scope**, and **World Scope**.  
TriScope provides a clean, modular foundation for simulation, tools, and multi-world workflows.

Designed for clarity, testability, and engine-agnostic portability.

#### Core Concepts

- **Engine Scope**
  - Global, process-level systems (logging, time, global services)
  - Created once per application
  - No per-session or per-world state

- **Group Scope**
  - Session-level systems (match, run, dungeon, simulation session)
  - Owns one or more Worlds
  - Ideal for multiplayer sessions, multi-run roguelikes, or editor preview sessions

- **World Scope**
  - Per-world simulation state
  - Contains systems, components, and runtime data
  - Fully isolated for deterministic multi-world execution

#### Runtime Features
- Deterministic startup order via StartupOrder
- Explicit dependency injection via DependencyContext
- Modular subsystem registration per scope
- Engine-agnostic Tick loop with stable timing
- Supports multi-world simulation (parallel or sequential)
- Built-in Runtime Viewer for live inspection and debugging
- Eliminates hidden singletons, scattered managers, and engine-tangled lifecycles

#### Benefits
- Predictable, testable runtime behavior
- Clear separation of global, session, and world responsibilities
- Scales from small prototypes to large multi-world simulations
- Works with Unity, Godot, MonoGame, or pure C#

[TriScope Runtime Readme](Docs/README-TriScope.md)

---

### 📐 Geometry & Math Utilities

A clean, engine-agnostic geometry layer providing collision-ready primitives and spatial reasoning tools.

[Collision Readme](Docs/README-Collision.md)

#### Core Primitives
- Line
- Ray
- Line Segment
- Triangle
- AABB
- OBB
- Frustum
- Cone
- Cylinder
- Capsule
- Sphere

**Design Philosophy:**  
Pure C# core → optional Unity/Burst layers → optional debug layer.  
Modular, deterministic, and future-proof for higher-level collision systems.

#### Burst Variants
- BoundingSphereBurst — SIMD-friendly, Unity.Mathematics-based version for high-throughput jobs

#### Debug Integration
- ShapeDebug — optional Unity-only visualization layer (editor-only, zero-cost in builds)

[ShapeDebug Readme](Docs/README-ShapeDebug.md)

---

## 📦 Installation

Install via Unity Package Manager using Git URL:

```https://github.com/vidextreme/com.xfg.corelib.git```

Or clone the repository directly into your project’s `Packages/` folder.

---

## 🗺️ Roadmap

Planned additions include:

* PRNG
  * Additional PRNG algorithms (Xoshiro256**, Xoshiro128++, Mersenne Twister, PCG64)
* Collision
  * Expanded collision/intersection tests (Ray–Triangle, Capsule–Capsule, SAT)
  * Additional Burst-optimized variants
* Systems
  * Templated Global Event System
  * Action System
  * UI Manager
  * Game Mode System
* Math
  * Curve
  * Statistics
  * A*
* Extensions
  * Unity types extensions (Component, Transform, etc.)

---

## 📄 License

This project is released under the MIT License.

[Join the community!](https://discord.gg/3GCxggFA6q)

### Main Author:
John David Uy (https://www.linkedin.com/in/johndaviduy/ >> Connect with me!)
