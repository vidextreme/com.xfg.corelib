# 🧠 UtilityAI: When FSM Isn’t Enough
### Engine‑Agnostic Decision System for Scalable, Reactive Behavior

Finite State Machines work for simple logic — but they collapse under complexity.  
UtilityAI solves this by scoring behaviors continuously and selecting the most desirable one every frame.

---

# ⭐ What Is UtilityAI?
### A scoring‑based decision model

- Each behavior computes a **utility score**
- Highest score wins
- All behaviors compete continuously
- Produces **fluid, reactive, emergent** behavior

> “All behaviors compete continuously; the most desirable one wins.”

---

# ⚠️ Why FSMs Break Down
### The pain points

- Rigid transitions  
- Exponential graph growth  
- Hard to scale  
- Hard to interrupt  
- Hard to tune  
- Designer‑unfriendly  

---

# 🚀 Why UtilityAI Works Better
### Natural, scalable decision‑making

- Continuous scoring  
- Easy to extend  
- Easy to tune  
- Interruptible  
- Emergent behavior  
- Designer‑friendly  

---

# 🧩 Architecture Overview
### 1. Engine‑Agnostic Core ("IUtilityAI")

- Task registration  
- Scoring + smoothing  
- Priority overrides  
- Suppression rules  
- Reconsideration  
- Lifecycle callbacks  
- Zero‑allocation rethink loop  

Runs in:
- Unity  
- Godot  
- Custom engines  
- Dedicated servers  
- Any C# runtime  

---

# 🧩 Architecture Overview
### 2. Unity Integration Layer ("ISerializableUtilityAI")

Adds:
- "SerializeReference" polymorphic task lists  
- Automatic brain assignment  
- Inspector‑authored tasks  
- Designer‑friendly workflow  

Unity layer is optional — core remains portable.

---

# ⚙️ How UtilityAI Works
### The decision loop

1. **Analyze** → compute RawWeight  
2. **Suppress** → filter invalid tasks  
3. **Priority** → hard overrides  
4. **Smooth** → stabilize behavior  
5. **Motivate** → personality/urgency  
6. **Sort** → highest TotalWeight wins  
7. **Lifecycle** → Enter → Update → Exit  
8. **Reconsider** → tasks may request switch  

---

# 🧠 Unity Example: Brain

```csharp
public class EnemyBrain 
    : ISerializableUtilityAI<EnemyBrain, EnemyTask, EnemyMessage> 
{
    void Update() => UpdateMachine();
}
```

---

# 🧱 Unity Example: Base Task

```csharp
public abstract class EnemyTask 
    : ISerializableUtilityAI<EnemyBrain, EnemyTask, EnemyMessage>.ISerializableTask
{
}
```

---

# 🎯 Unity Example: Implementing a Task

```csharp
[System.Serializable]
public class ChaseTask : EnemyTask
{
    public override void Analyze()
    {
        RawWeight = /* compute score */;
    }

    public override RethinkFrequency Frequency => RethinkFrequency.PerUpdate;
}
```

---

# ⚖️ UtilityAI vs FSM

| Feature              | Utility AI                          | FSM                               |
|----------------------|--------------------------------------|-----------------------------------|
| Decision Model       | Continuous scoring                  | Hard‑coded transitions            |
| Flexibility          | High                                 | Low                               |
| Scalability          | Linear growth                        | Exponential transitions           |
| Designer Control     | Tunable weights                      | Graph editing                     |
| Responsiveness       | Instant                              | Transition‑bound                  |
| Emergent Behavior    | Natural                              | Rare                              |
| Interruptibility     | Built‑in                             | Manual                            |

---

# 🌟 Advantages of This Implementation

- Engine‑agnostic core  
- Optional Unity integration  
- Strong typing across Brain/Task/Message  
- Zero allocations in rethink loop  
- Inspector‑friendly polymorphic tasks  
- Priority overrides + smoothing  
- Full lifecycle hooks  
- Reconsideration system  
- Fallback behavior  
- Error‑safe "Analyze()"  
- Deterministic, production‑ready architecture  

---

# 🔄 Full Rethink Pipeline

```text
1. Suppression → filter tasks
2. Priority → override if needed
3. Analyze → compute RawWeight
4. Smooth → Lerp SmoothedWeight
5. Sort → by TotalWeight
6. Fallback → if best ≤ 0
7. Switch → Enter/Exit lifecycle
```
