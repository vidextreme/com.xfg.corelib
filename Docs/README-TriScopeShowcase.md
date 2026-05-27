# 🚀 XFG TriScope Runtime Architecture
### *A module of the XFG Simple Game Core Library*  
*A deterministic, engine‑agnostic runtime framework for scalable simulation and game systems — **inspired by Unreal Engine’s World‑Subsystem architecture**, then expanded into a fully portable, multi‑world, dependency‑injected runtime model.*

---

## 🎯 1. What TriScope Is  
The **XFG TriScope Runtime Architecture** is a modern, deterministic runtime framework built around **three explicit scopes**:

- **Engine Scope** — global systems  
- **Group Scope** — session‑level systems  
- **World Scope** — per‑world simulation  

TriScope provides a **clean, predictable, testable** structure for real‑time games and simulations.

---

## 🧩 2. Why TriScope Exists  
### Traditional architectures struggle with:
- Scattered manager classes  
- Hidden global singletons  
- Unpredictable initialization order  
- Tight coupling to Unity/Godot lifecycle  
- No multi‑world support  
- Hard‑to‑debug runtime state  

### TriScope solves this with:
- Deterministic subsystem lifecycle  
- Explicit dependency injection  
- Asset‑driven subsystem creation  
- Engine‑agnostic Tick + FixedTick  
- Native multi‑world simulation  
- Runtime inspection + diff tooling  

---

## ✨ 3. Design Inspiration  
TriScope draws heavily from **Unreal Engine’s World‑Subsystem architecture**, specifically:

- Modular runtime units  
- Clear separation of global vs world logic  
- Deterministic startup ordering  

But TriScope extends these ideas into something **Unreal does not provide**:

- Engine‑agnostic  
- Multi‑world by design  
- Explicit DI  
- Asset‑driven instantiation  
- Strong runtime tooling  

It is the **portable, modernized evolution** of Unreal’s subsystem architecture.

---

## 🧱 4. The Three Scopes  
### **Engine Scope**  
Global, process‑wide systems  
Examples: logging, platform, networking, metrics

### **Group Scope**  
Session‑level systems shared across multiple worlds  
Examples: match rules, orchestration, player/session logic

### **World Scope**  
Per‑world simulation systems  
Examples: AI, physics, navigation, time‑of‑day, weather

---

## 🔧 5. Subsystem Model  
Each subsystem implements:

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

Optional interfaces:

- **ITickable**  
- **IFixedTickable**  
- **IRequireDependencies**  
- **IRequireAsset**

---

## 🔄 6. Deterministic Initialization Pipeline  
1. Sort subsystem assets by `StartupOrder`  
2. Instantiate subsystem instances  
3. Inject assets  
4. Inject dependencies  
5. Run initialization lifecycle  

This pipeline runs independently for:

- Engine  
- Group  
- World  

---

## ⏱️ 7. Tick Model  
### Single entry point  
```csharp
Core.Tick(dt);
```

### Deterministic fixed timestep  
```csharp
_fixedAccumulator += dt;
while (_fixedAccumulator >= FixedDeltaTime)
{
    FixedTick(FixedDeltaTime);
    _fixedAccumulator -= FixedDeltaTime;
}
```

Benefits:
- Engine‑agnostic  
- Deterministic stepping  
- Multiple fixed steps per frame when needed  

No reliance on Unity’s non‑deterministic FixedUpdate.

---

## 🔌 8. Dependency Injection  
Explicit, predictable, and scope‑aware:

```csharp
public void InjectDependencies(DependencyContext ctx)
{
    _weather = ctx.GetWorld<WeatherSubsystem>();
}
```

Dependency flow is always:

**Engine → Group → World**

---

## 🧪 9. Sample Subsystem (World Scope)

### Subsystem Asset  
```csharp
[StartupOrder(SubsystemCategory.World, 20)]
[SubsystemId("world.time_of_day")]
public sealed class TimeOfDaySubsystemAsset 
    : SubsystemAsset<TimeOfDaySubsystem>, IWorldSubsystemAsset
{
    public float DayLengthSeconds { get; set; } = 300f;
}
```

### Subsystem Instance  
```csharp
public sealed class TimeOfDaySubsystem : 
    ISubsystemInstance, ITickable, IRequireAsset, IRequireDependencies
{
    private TimeOfDaySubsystemAsset _asset;
    private WeatherSubsystem _weather;

    public float NormalizedTime { get; private set; }

    public void InjectAsset(ISubsystemAsset asset)
        => _asset = (TimeOfDaySubsystemAsset)asset;

    public void InjectDependencies(DependencyContext ctx)
        => _weather = ctx.GetWorld<WeatherSubsystem>();

    public void Initialize() => NormalizedTime = 0f;

    public void Tick(float dt)
    {
        if (_weather?.IsStorming == true) return;

        NormalizedTime += dt / _asset.DayLengthSeconds;
        if (NormalizedTime >= 1f) NormalizedTime -= 1f;
    }
}
```

---

## 🆚 10. TriScope vs Unreal Engine

| Feature | Unreal Engine | XFG TriScope |
|--------|----------------|--------------|
| World Subsystems | ✔ | ✔ (engine‑agnostic) |
| GameInstance‑level systems | ✔ | Group Scope (explicit) |
| Multi‑world simulation | ✖ | ✔ Native |
| Dependency Injection | ✖ | ✔ Explicit |
| Tick Model | Engine‑bound | Engine‑agnostic |
| Subsystem Assets | ✖ | ✔ |
| Runtime Tooling | Limited | Full inspection + diff |

TriScope is the **deterministic, multi‑world, engine‑agnostic evolution** of Unreal’s subsystem model.

---

## 🧭 11. Architecture Diagram  

```
┌──────────────────────────────────────────────────────────────────────────────┐
│                     XFG TRISCOPE RUNTIME ARCHITECTURE                        │
└──────────────────────────────────────────────────────────────────────────────┘

                                   ┌──────────────────────────────┐
                                   │        ENGINE SCOPE          │
                                   └───────────────┬──────────────┘
                                                   │
                                                   ▼
                                   ┌──────────────────────────────┐
                                   │        GROUP SCOPE           │
                                   └───────────────┬──────────────┘
                                                   │
                     ┌─────────────────────────────┼─────────────────────────────┐
                     ▼                             ▼                             ▼
        ┌──────────────────────────┐  ┌──────────────────────────┐  ┌──────────────────────────┐
        │        WORLD SCOPE       │  │        WORLD SCOPE       │  │        WORLD SCOPE       │
        └──────────────────────────┘  └──────────────────────────┘  └──────────────────────────┘
```

---

## 🧠 12. Why TriScope Matters  
TriScope gives teams:

- Predictable architecture  
- Deterministic simulation  
- Multi‑world support  
- Explicit DI  
- Engine‑agnostic runtime  
- Strong debugging tools  
- Clean separation of concerns  
- Scalable subsystem‑based design  

It’s built for **modern game development**, **simulation platforms**, and **high‑reliability runtime systems**.
