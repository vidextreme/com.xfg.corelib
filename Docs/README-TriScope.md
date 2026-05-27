# ⚙️ XFG TriScope Runtime Architecture  

```csharp
using XFG.Subsystems;
``` 

*A deterministic, engine‑agnostic runtime framework for scalable simulation and game systems.*  
**This module is part of the XFG Simple Game Core Library.**

---

## 🌍 1. What Is the XFG TriScope Runtime Architecture?

The **XFG TriScope Runtime Architecture** is a modular, deterministic runtime framework built around **three explicit scopes**:

- **Engine Scope** — global systems  
- **Group Scope** — session‑level systems  
- **World Scope** — per‑world simulation  

These three scopes form the “TriScope” model, a clean and predictable structure for organizing runtime logic in games, simulations, and real‑time applications.

TriScope replaces ad‑hoc managers, hidden dependencies, and engine‑tangled lifecycles with a **clear, explicit, testable architecture**.

---

## ✨ 2. Design Inspiration

TriScope draws conceptual inspiration from **Unreal Engine’s World‑Subsystem architecture**, particularly:

- The idea of **subsystems as modular runtime units**  
- The separation of **global**, **game/session**, and **world‑level** responsibilities  
- The use of **deterministic startup ordering**  
- The ability to attach systems to a **world instance** rather than global singletons  

However, TriScope extends these ideas into a **fully engine‑agnostic**, **multi‑world**, **dependency‑injected**, and **asset‑driven** architecture that is simpler, more explicit, and more scalable.

---

## 🎯 3. Purpose of the System

TriScope exists to solve long‑standing architectural problems:

### ❌ The Problems
- Manager classes scattered across the codebase  
- Hidden global singletons  
- Unpredictable initialization order  
- Tight coupling to Unity/Godot lifecycle  
- Difficulty supporting multiple worlds  
- Hard‑to‑debug runtime state  
- No unified way to inspect or diff system values  

### ✅ The TriScope Solution
- **Subsystems** provide modular, isolated runtime logic  
- **StartupOrder** ensures deterministic initialization  
- **DependencyContext** provides explicit DI  
- **WorldGroup** enables multi‑world simulation  
- **World** isolates simulation state  
- **Engine‑agnostic Tick** ensures consistent timing  
- **Runtime Viewer** exposes live state, diffs, and metadata  

TriScope is built for **clarity, determinism, and maintainability** — especially in large projects or teams.

---

## 🧱 4. TriScope Layer Model

| Scope | Purpose | Example Responsibilities |
|-------|---------|--------------------------|
| **Engine Scope** | Global systems, one per process | Logging, platform, networking, time |
| **Group Scope** | Session‑level systems, one per WorldGroup | Match rules, orchestration, player/session logic |
| **World Scope** | Simulation container, one per World | Physics, AI, navigation, world state |

Each scope has its own subsystem list, lifecycle, and dependency boundaries.

---

## 🧩 5. Subsystem Model

Subsystems are created from assets and instantiated at runtime.  
Each subsystem implements:

```
ISubsystemInstance
```

Lifecycle:

```
OnBeforeInitialize()
Initialize()
OnAfterInitialize()

Tick(dt)          (optional)
FixedTick(fdt)    (optional)

OnBeforeDeinitialize()
Deinitialize()
OnAfterDeinitialize()
```

### 5.1 Optional Capabilities

- **ITickable** — receives per‑frame updates  
- **IFixedTickable** — receives fixed‑timestep updates  
- **IRequireDependencies** — receives injected subsystem references  
- **IRequireAsset** — receives its asset instance  

---

## 📦 6. Subsystem Registry

Subsystems are declared via a `SubsystemRegistry`:

```
EngineSubsystems
GroupSubsystems
WorldSubsystems
```

The registry must be assigned to:

```csharp
Core.Registry = registry;
```

before initialization.

---

## 🚀 7. Initialization Flow

### 7.1 Engine Initialization

```csharp
Core.InitializeEngine();
```

### 7.2 Creating a WorldGroup

```csharp
var group = Core.CreateWorldGroup();
```

### 7.3 Creating a World

```csharp
var world = group.CreateWorld(worldRegistry);
```

---

## ⏱️ 8. Tick Model

### 8.1 Single Entry Point

```csharp
Core.Tick(Time.deltaTime);
```

### 8.2 Fixed Timestep Loop

```csharp
_fixedAccumulator += dt;
while (_fixedAccumulator >= FixedDeltaTime)
{
    FixedTick(FixedDeltaTime);
    _fixedAccumulator -= FixedDeltaTime;
}
```

### 8.3 Why FixedUpdate Is Not Used

Unity’s FixedUpdate is not deterministic and is tied to physics.  
TriScope’s fixed timestep loop is deterministic and engine‑agnostic.

---

## 🌐 9. World Scope

A **World** is a simulation container.

Responsibilities:

- Holds world‑level subsystems  
- Runs Tick and FixedTick  
- Supports pausing  
- Provides subsystem lookup  
- Deinitializes cleanly  

---

## 🏗️ 10. Group Scope

A **WorldGroup** is a session‑level container.

Responsibilities:

- Holds group‑level subsystems  
- Owns multiple Worlds  
- Orchestrates multi‑world simulation  
- Runs Tick and FixedTick  
- Supports pausing  
- Injects dependencies into worlds  

---

## 🔌 11. Dependency Injection

```csharp
public void InjectDependencies(DependencyContext ctx)
{
    _weather = ctx.GetWorld<WeatherSubsystem>();
}
```

Dependency flow is always:

```
Engine Scope → Group Scope → World Scope
```

---

## 🧹 12. Deinitialization

```csharp
Core.DeinitializeAll();
```

---

## 🛠️ 13. Tooling Support

TriScope integrates with the SubsystemRuntimeWindow, providing:

- Live subsystem inspection  
- Value diff highlighting  
- Tag filtering  
- Metadata display  
- Asset inspection  
- Snapshot export  
- Pinned subsystems  

---

## 🆚 14. Comparison: TriScope vs Unreal Engine

TriScope draws inspiration from Unreal’s subsystem model but extends it significantly.

### 🔷 Similarities
- Modular subsystem units  
- World‑attached systems  
- Deterministic startup ordering  
- Clear separation of global vs world logic  

### 🔶 Key Differences

| Concept | Unreal Engine | XFG TriScope |
|--------|----------------|--------------|
| **World Subsystems** | Yes | Yes (cleaner, engine‑agnostic) |
| **GameInstance‑level systems** | Yes | Group Scope (explicit, deterministic) |
| **Multi‑world simulation** | Not supported | Native, first‑class |
| **Dependency Injection** | None | Explicit DI via DependencyContext |
| **Tick Model** | Engine‑driven, non‑deterministic | Engine‑agnostic, deterministic accumulator |
| **Subsystem Assets** | No | Yes (asset‑driven instantiation) |
| **Tooling** | Limited | Full runtime viewer, diffing, metadata |
| **Engine Coupling** | High | None |

---

## 🧪 15. Sample Subsystem Implementation (World Scope)

A complete example of a **World‑scope subsystem** in the TriScope architecture.

### **Subsystem Asset**

```csharp
using XFG.Subsystems;

[StartupOrder(SubsystemCategory.World, 20)]
[SubsystemId("world.time_of_day")]
[SubsystemDescription("Tracks and updates the world's time-of-day cycle.")]
[SubsystemTags("time", "environment", "simulation")]
public sealed class TimeOfDaySubsystemAsset 
    : SubsystemAsset<TimeOfDaySubsystem>, IWorldSubsystemAsset
{
    public float DayLengthSeconds { get; set; } = 300f;
}
```

---

### **Subsystem Instance**

```csharp
using XFG.Subsystems;

public sealed class TimeOfDaySubsystem : 
    ISubsystemInstance,
    ITickable,
    IRequireAsset,
    IRequireDependencies
{
    private TimeOfDaySubsystemAsset _asset;
    private WeatherSubsystem _weather;

    public float NormalizedTime { get; private set; }
    public ISubsystemAsset Asset => _asset;

    public void InjectAsset(ISubsystemAsset asset)
    {
        _asset = (TimeOfDaySubsystemAsset)asset;
    }

    public void InjectDependencies(DependencyContext ctx)
    {
        _weather = ctx.GetWorld<WeatherSubsystem>();
    }

    public void OnBeforeInitialize() { }

    public void Initialize()
    {
        NormalizedTime = 0f;
    }

    public void OnAfterInitialize() { }

    public void Tick(float dt)
    {
        if (_weather != null && _weather.IsStorming)
            return;

        if (_asset.DayLengthSeconds <= 0f)
            return;

        NormalizedTime += dt / _asset.DayLengthSeconds;

        if (NormalizedTime >= 1f)
            NormalizedTime -= 1f;
    }

    public void OnBeforeDeinitialize() { }
    public void Deinitialize() { }
    public void OnAfterDeinitialize() { }
}
```

---

### **Dependency Example: WeatherSubsystem**

```csharp
using XFG.Subsystems;

[StartupOrder(SubsystemCategory.World, 10)]
[SubsystemId("world.weather")]
[SubsystemDescription("Simulates weather patterns.")]
public sealed class WeatherSubsystemAsset 
    : SubsystemAsset<WeatherSubsystem>, IWorldSubsystemAsset
{
}

public sealed class WeatherSubsystem : ISubsystemInstance
{
    public bool IsStorming { get; private set; }

    public void OnBeforeInitialize() { }
    public void Initialize() { IsStorming = false; }
    public void OnAfterInitialize() { }

    public void OnBeforeDeinitialize() { }
    public void Deinitialize() { }
    public void OnAfterDeinitialize() { }
}
```

---

## 🧭 16. TriScope Architecture Diagram

```
┌──────────────────────────────────────────────────────────────────────────────┐
│                     XFG TRISCOPE RUNTIME ARCHITECTURE                        │
└──────────────────────────────────────────────────────────────────────────────┘

                                   ┌──────────────────────────────┐
                                   │        ENGINE SCOPE          │
                                   │  Engine Subsystems           │
                                   │  (Global, 1 per process)     │
                                   └───────────────┬──────────────┘
                                                   │
                                                   ▼
                                   ┌──────────────────────────────┐
                                   │        GROUP SCOPE           │
                                   │      Group Subsystems        │
                                   │  (Session‑level logic)       │
                                   │  Owns multiple Worlds        │
                                   └───────────────┬──────────────┘
                                                   │
                     ┌─────────────────────────────┼─────────────────────────────┐
                     ▼                             ▼                             ▼
        ┌──────────────────────────┐  ┌──────────────────────────┐  ┌──────────────────────────┐
        │        WORLD SCOPE       │  │        WORLD SCOPE       │  │        WORLD SCOPE       │
        │     World Subsystems     │  │     World Subsystems     │  │     World Subsystems     │
        │      (Simulation)        │  │      (Simulation)        │  │      (Simulation)        │
        │ Tick / FixedTick         │  │ Tick / FixedTick         │  │ Tick / FixedTick         │
        └──────────────────────────┘  └──────────────────────────┘  └──────────────────────────┘
```

---

## 🧠 17. Summary

The **XFG TriScope Runtime Architecture** provides:

- Engine → Group → World layering  
- Deterministic Tick + FixedTick  
- Asset‑driven subsystem creation  
- Explicit dependency injection  
- Multi‑world simulation  
- Strong tooling support  

TriScope is engine‑agnostic, maintainable, and built for modern game and simulation workloads.
