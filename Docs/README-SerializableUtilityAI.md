# 🧠Serializable Utility AI  

```csharp
using XFG.AI.UAI.Serializable;
``` 
A Unity‑friendly, designer‑oriented wrapper around the core Utility AI controller.  
It bridges Unity’s serialization system with the runtime Utility AI architecture, enabling fully polymorphic, inspector‑authored tasks using `SerializeReference`.

This layer ensures that tasks defined in the Unity Inspector behave identically to tasks registered in code, while maintaining strong typing, predictable lifecycle flow, and full compatibility with the Utility AI scoring and reconsideration pipeline.

For the base UAI, see:  
[Utility AI Readme](README-UtilityAI.md)

---

## Key Features

### ✔ Polymorphic Task Serialization  
Uses `SerializeReference` + custom attribute to allow any derived task type to be authored directly in the inspector.

### ✔ Automatic Task Registration  
All tasks in the serialized list are automatically registered during `Awake()`, ensuring consistent runtime behavior.

### ✔ Strong Typing  
Generic constraints enforce:
- `TBrainType` is a `MonoBehaviour`
- `TTaskType` implements the AI’s `ITask` interface
- `TMessageType` is a strongly‑typed enum

### ✔ Designer‑Friendly Task Base Class  
`ISerializableTask` provides:
- Clean override points  
- Strongly‑typed `Brain` reference  
- Weight model (`RawWeight`, `SmoothedWeight`, `Motivator`, `TotalWeight`)  
- Lifecycle hooks (`OnTaskEnter`, `OnTaskUpdate`, etc.)  
- Reconsideration control  

### ✔ Full Compatibility With Utility AI Flow  
Serializable tasks integrate seamlessly with:
- Scoring  
- Smoothing  
- Priority overrides  
- Reconsideration  
- Messaging  
- Task switching  

---

## How It Works

### 1. Tasks Are Authored in the Inspector  
Because `SerializeReference` supports polymorphism, designers can add any derived task type to the `Tasks[]` list.

### 2. Tasks Are Registered Automatically  
During `Awake()`, each task is:
- Assigned a strongly‑typed `Brain` reference  
- Registered with the AI controller  

### 3. Tasks Participate in the Full AI Pipeline  
Serializable tasks behave exactly like code‑registered tasks:
- They are analyzed  
- Their weights are smoothed  
- They participate in reconsideration  
- They can override priority  
- They can receive messages  
- They can be switched, interrupted, or completed  

---

## Example Usage

### Creating a Serializable Task

```csharp
[Serializable]
public class PatrolTask : ISerializableUtilityAI<MyBrain, PatrolTask, MyMessage>.ISerializableTask
{
    public override RethinkFrequency Frequency => RethinkFrequency.Often;

    public override void Analyze()
    {
        RawWeight = Brain.CanPatrol ? 1f : 0f;
    }

    public override void OnTaskEnter(ISerializableTask previous)
    {
        Brain.StartPatrol();
    }

    public override void OnTaskUpdate()
    {
        Brain.UpdatePatrol();
    }
}
```

### Adding Tasks in the Inspector

1. Add your AI component (derived from `ISerializableUtilityAI`) to a GameObject.  
2. Expand the **Tasks** list.  
3. Add any number of serializable task types.  
4. Configure their fields directly in the inspector.

---

## When to Use This Layer

Use `ISerializableUtilityAI` when:

- Designers need to author tasks without writing code  
- You want polymorphic task lists in the inspector  
- You want clean separation between runtime logic and authored content  
- You want full Utility AI behavior with zero boilerplate  

If tasks are purely code‑driven, you can use the non‑serializable `IUtilityAI` directly.

---

## Benefits for Teams

### Designers  
- Can author tasks visually  
- Can tune weights and motivators in real time  
- Can reorder or disable tasks without code changes  

### Programmers  
- Maintain strong typing and predictable behavior  
- Avoid reflection‑based hacks  
- Keep AI logic clean and modular  

### Technical Leads  
- Gain a scalable, maintainable AI architecture  
- Ensure consistent behavior across authored and code tasks  
- Reduce onboarding friction for new team members  
