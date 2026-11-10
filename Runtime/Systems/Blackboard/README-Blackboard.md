# 🧠 XFG Blackboard System  
A lightweight, engine‑agnostic reasoning framework for AI agents, gameplay logic, and world‑state coordination.

The XFG Blackboard System provides:
- A **typed key/value store** for runtime state  
- **Priority‑based Knowledge Sources (KS)** for modular reasoning  
- **Agent‑level blackboards** for per‑entity logic  
- **Optional system drivers** for engine/ECS integration  
- **Save/load support** for persistence and debugging  

The design is intentionally minimal, explicit, and easy to onboard.  
No reflection, no magic, no engine dependencies.

---

# 🧩 1. What Is a Blackboard System?

A **blackboard system** is a shared reasoning surface used in AI, robotics, and gameplay logic.  
It acts like a **central whiteboard** where multiple small logic modules (Knowledge Sources) can:

- Read the current state  
- Write new state  
- React to changes  
- Influence behavior  
- Coordinate indirectly  

Each module contributes independently, without needing to know about the others.

### Why this matters  
Blackboards solve the classic problems of game AI:

- Avoiding giant monolithic `Update()` functions  
- Avoiding tangled dependencies between systems  
- Allowing complex behavior to emerge from simple parts  
- Making logic modular, testable, and deterministic  

This architecture is widely used in:

- Robotics  
- Multi‑agent simulations  
- AAA game AI  
- Planning systems  
- Distributed reasoning  

The XFG implementation is a **clean, typed, engine‑agnostic version** of this pattern.

---

# 🎯 2. Common Purposes of a Blackboard

### ✔ Modular AI reasoning  
Each KS handles one small piece of logic.

### ✔ Per‑entity state management  
Agents store their own runtime state.

### ✔ World or team‑level coordination  
Multiple agents can read from shared blackboards.

### ✔ Rule‑based gameplay logic  
Buffs, debuffs, triggers, cooldowns, etc.

### ✔ Deterministic behavior  
KSs run in priority order for predictable results.

### ✔ Debugging and replay  
Blackboards can serialize themselves for snapshots.

---

# 🎮 3. Examples of Real Uses

### NPC AI  
- Health checks  
- Aggression decay  
- Target selection  
- Perception updates  
- Movement steering  
- Combat decision‑making  

### World Simulation  
- Global threat levels  
- Weather or time‑of‑day logic  
- Faction or team‑level state  

### Gameplay Systems  
- Ability cooldowns  
- Buff/debuff systems  
- Trigger conditions  
- Puzzle logic  

### Multi‑Agent Systems  
- Shared goals  
- Distributed planning  
- Sensor fusion  

The system is intentionally general so it can support all of these.

---

# 📦 4. Core Concepts

## Blackboard  
A typed dictionary:

```csharp
Dictionary<string, TValue>
```

Each blackboard stores:
- A set of **keys** (strings)
- A single **value type** (`TValue`)
- A list of **Knowledge Sources** that read/write state

---

# 🧠 5. Knowledge Sources (KS)

A **Knowledge Source** is a small, self‑contained logic module.

Each KS provides:
- `Priority` — higher runs first  
- `CanExecute(bb)` — checks preconditions  
- `Execute(bb)` — applies logic to the blackboard  

Example:

```csharp
public class DamageOverTimeKS : KnowledgeSource<SuperAgentData>
{
    public override int Priority => 50;

    public override bool CanExecute(IBlackboard<SuperAgentData> bb)
        => bb.Has("Agent");

    public override void Execute(IBlackboard<SuperAgentData> bb)
    {
        var data = bb.Get("Agent");
        data.Health -= 1;
        bb.Set("Agent", data);
    }
}
```

### Why KSs work well
- Pure reasoning units  
- Stateless  
- Easy to test  
- Easy to add/remove  
- Deterministic execution order  

---

# 🏗️ 6. BlackboardCore<TValue>

Implements the full runtime behavior:

- Key/value storage  
- Knowledge Source registration  
- Priority‑sorted execution  
- Save/load support  

Key API:

```csharp
bool Has(string key);
TValue Get(string key);                    // default if missing
bool TryGet(string key, out TValue value);
void Set(string key, TValue value);
bool Remove(string key);
IReadOnlyDictionary<string, TValue> Snapshot();

void AddKnowledgeSource(IKnowledgeSource<TValue> ks);
void Run(); // executes all KSs in priority order
```

### Run semantics
- KSs run in **descending priority** order; each KS runs **at most once** per `Run()` call  
- Each KS is checked with `CanExecute` right before it runs, so it sees writes made by higher‑priority KSs earlier in the same pass  
- A KS added **during** `Run()` (for example, by another KS) does not run in that pass. It runs from the next `Run()` call  

### Snapshot
`Snapshot()` returns a **copy** of the current data. Later `Set`/`Remove` calls do not change a snapshot you already took, so it is safe to keep for debugging or diffing.

### Save/Load

```csharp
var save = bb.ToSaveData();
bb.LoadFromSaveData(save);
```

`LoadFromSaveData` replaces all existing data. It validates the save first and throws, leaving the current state untouched, when:
- `save`, `save.Keys`, or `save.Values` is null (`ArgumentNullException`)  
- `Keys` and `Values` have different counts (`ArgumentException`)  

Knowledge sources are not part of the save data; register them again after loading into a new blackboard.

---

# 👤 7. BlackboardAgent<TValue>

A per‑entity blackboard:

```csharp
var agent = BlackboardAgent<SuperAgentData>.Create();
agent.Set("Agent", initialData);
agent.AddKnowledgeSource(new DamageOverTimeKS());
agent.Run();
```

Agents may also reference **world**, **team**, or **squad** blackboards for multi‑layer reasoning.

### Authoring agents as assets

`BlackboardAgentAsset<TValue>` lets designers author an agent's starting data as an asset (a `ScriptableObject` in Unity, a `Resource` in Godot):

```csharp
[CreateAssetMenu(menuName = "AI/Super Agent")]
public class SuperAgentAsset : BlackboardAgentAsset<SuperAgentData>
{
    protected override void ConfigureSources(BlackboardAgent<SuperAgentData> agent)
    {
        agent.AddKnowledgeSource(new DamageOverTimeKS());
    }
}

// At runtime
BlackboardAgent<SuperAgentData> agent = superAgentAsset.CreateInstance();
```

Mark `TValue` as `[Serializable]` so the engine can save the entries in the asset.

`CreateInstance()` creates a new agent, copies every key/value pair from `Data.Entries`, then calls `ConfigureSources` to register KSs. Each call returns an independent agent.

---

# ⚙️ 8. BlackboardAgentSystem<TValue>

Optional engine/ECS‑level driver:

```csharp
public abstract class BlackboardAgentSystem<TValue>
{
    public abstract void Tick(float dt);
}
```

Typical responsibilities:
- Iterate agents  
- Update per‑frame state (e.g., deltaTime inside `TValue`)  
- Call `agent.Run()`  

---

# 🧪 9. Recommended Usage Pattern

### 1. Define your value type

```csharp
public struct SuperAgentData
{
    public int Health;
    public float Aggression;
    public float DeltaTime;
}
```

### 2. Create an agent

```csharp
var agent = BlackboardAgent<SuperAgentData>.Create();
agent.Set("Agent", new SuperAgentData { Health = 100 });
```

### 3. Register KSs

```csharp
agent.AddKnowledgeSource(new DamageOverTimeKS());
agent.AddKnowledgeSource(new AggressionDecayKS());
```

### 4. Run per frame

```csharp
agent.Run();
```

---

# 🧱 10. Design Principles

### ✔ Explicitness  
No reflection or hidden behavior.

### ✔ Determinism  
KSs run in priority order.

### ✔ Engine‑agnostic  
Works in Unity, Godot, ECS, custom engines.

### ✔ Testability  
KSs are pure logic modules.

### ✔ Modularity  
Add/remove KSs without touching others.

---

# 🚫 11. When *Not* to Use It

Avoid using the blackboard for:

- High‑frequency numeric simulation  
- Physics  
- Rendering  
- Anything requiring micro‑updates  

Blackboards are for **reasoning**, not **simulation**.

---

# 🔧 12. Extending the System

You can extend the blackboard with:

- Custom KS base classes  
- Domain‑specific key definitions  
- Agent factories  
- World/team blackboards  
- Debug visualizers  
- Deterministic replay tools  

The system is intentionally small so you can grow it in the direction your project needs.