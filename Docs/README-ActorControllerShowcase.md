# 🎮 **XFG Actor–Controller Framework**
### A deterministic, engine‑agnostic gameplay framework  
Built for Unity, Godot, MonoGame, and custom engines.

- Clear separation of **decision** (Controller) and **execution** (Actor)  
- Deterministic **command buffer**  
- Strongly‑typed **FSM**  
- Engine‑agnostic core with conditional compilation  
- Supports **player input**, **AI**, **networking**, **cutscenes**, **tools**, **Utility AI**, **Behavior Trees**, **FSM AI**, **Replay**, and more  
- Inspired by Unreal Engine’s Actor–Controller model, adapted for C#

---

# 🧩 **What This Architecture Represents**
This architecture is built around a strict separation of responsibilities that produces predictable, deterministic, and maintainable gameplay behavior.

### Core principles:
- **Controllers decide** what should happen  
- **Actors execute** those decisions through a state machine  
- **States encapsulate behavior** and transitions  
- **Inform() provides feedback** from Actor → Controller  
- **Command buffer ensures determinism** and prevents mid‑frame state changes  

### Why it matters:
- Predictable behavior across platforms  
- Clean layering for large‑scale gameplay systems  
- Easy to test, debug, and extend  
- Works identically for AI, player input, network commands, and tools  

---

# 🎛️ **Controllers — The Decision Layer**
Controllers are the **brains** of the architecture.  
They decide what the Actor should do — the Actor only executes.

### Controllers can be:
- Player Input  
- Behavior Tree (B3)  
- Utility AI  
- FSM logic  
- Network replication  
- Scripted / Cutscene logic  
- Replay / Ghost input  
- Tooling / Editor drivers  

### Why this works:
- Controllers only call `ExecuteCommand()`  
- Actors only execute commands  
- States encapsulate behavior  
- Inform() provides feedback  
- Command buffer ensures determinism  

---

# 🏛️ **Inspiration from Unreal Engine**
This design draws directly from Unreal Engine’s proven Actor–Controller pattern:

### Unreal’s philosophy:
- Controller = **intent + decision layer**  
- Pawn/Character = **execution layer**  
- Controllers issue **commands**, not direct state changes  
- Pawns emit **events** back to Controllers  
- Controllers can be swapped (AI, player, network)  

### XFG adaptation:
- Strongly‑typed commands  
- Deterministic command buffer  
- Lightweight, engine‑agnostic FSM  
- Serializable polymorphic states (Unity)  
- Clean Inform() callback channel  

You preserve Unreal’s strengths while making the model portable, explicit, deterministic, and C#‑friendly.

---

# ⚖️ **Unreal Engine vs XFG Architecture**

| Concept / Behavior | Unreal Engine | XFG Actor–Controller Architecture |
|--------------------|---------------|----------------------------------|
| **Decision Layer** | Controller (PlayerController, AIController) | Controller (Input, AI, B3, Utility AI, FSM, Network, Replay) |
| **Execution Layer** | Pawn / Character | Actor (FSM‑driven entity) |
| **How Decisions Are Sent** | Input events, movement functions, ability triggers | Typed commands via `ExecuteCommand(cmd, param)` |
| **How Execution Happens** | Pawn processes input, CharacterMovement, components | Actor processes commands through FSM + command buffer |
| **Feedback to Controller** | Delegates, events | `Inform(info, args)` callback |
| **Possession Model** | Controller possesses Pawn | Controller attaches to Actor |
| **State Management** | State Trees, Behavior Trees | Strongly‑typed FSM with polymorphic states |
| **Determinism** | Not guaranteed | Guaranteed via command buffer + FSM |
| **Engine Dependency** | Unreal‑specific | Engine‑agnostic |
| **Serialization** | Blueprint, UObjects | `SerializeReference` polymorphic states (Unity) |

---

# 🌐 **Engine‑Agnostic Design**
The architecture is intentionally portable and avoids engine‑specific APIs.

### Conditional compilation:
```csharp
#if UNITY_5_3_OR_NEWER
    where TMachineType : MonoBehaviour
#elif GODOT
    where TMachineType : Godot.Node
#elif MONOGAME
    where TMachineType : Microsoft.Xna.Framework.GameComponent
#else
    where TMachineType : class
#endif
```

### Benefits:
- Same core logic runs in Unity, Godot, MonoGame, or custom engines  
- FSM, command buffer, and Actor/Controller abstractions are **pure C#**  
- Only the base type constraint changes per engine  

This makes the system ideal for cross‑engine prototyping or long‑term engine migration.

---

# 🧠 **Core Concepts**

### **Actor**
- Owns the FSM  
- Executes commands deterministically  
- Processes buffered input  
- Emits Inform events back to Controller  
- Defines per‑state behavior through polymorphic state classes  
- Never performs decision‑making  

### **Controller**
- Decides what the Actor should do  
- Sends typed commands  
- Reacts to Actor events via `Inform()`  
- Never mutates Actor state directly  
- Can be swapped (AI, player, network, scripted, Utility AI, B3, FSM, Replay)  

### **Command Buffer**
- FIFO  
- Prevents mid‑frame state changes  
- Ensures deterministic behavior  
- Makes AI, player, and network input behave identically  

---

# 🏗️ **Architecture Overview**

```
┌───────────────────────────┐
│        Controller         │
│  (AI / Player / Network)  │
└───────────────┬───────────┘
                │ ExecuteCommand()
                ▼
┌───────────────────────────┐
│           Actor           │
│  - Command Buffer         │
│  - FSM (StateMachine)     │
└───────────────┬───────────┘
                │ Routes command
                ▼
┌────────────────────────────┐
│        Actor State         │
│   (IActorState<TCommand>)  │
└────────────────────────────┘
```

---

# 🔁 **Actor ↔ Controller Inform Loop**

```
Controller ──► ExecuteCommand(cmd)
     ▲                          │
     │                          ▼
Inform(info) ◄── State ◄── Actor (FSM + Buffer)
```

### Flow:
1. Controller decides → sends command  
2. Actor buffers → processes deterministically  
3. State executes logic  
4. State informs Controller  
5. Controller reacts → may issue new commands  

This creates a clean, deterministic gameplay loop.

---

# 🧱 **Serialized State Machines (Unity)**

For designer‑friendly workflows, the architecture supports a fully serialized FSM.

### Features:
- Uses `SerializeReference` for polymorphic states  
- States can be reordered, edited, and configured in the Inspector  
- No need to declare your own `States[]` — inherited automatically  
- Perfect for designers, technical artists, and rapid iteration  

```csharp
public class PlayerActorSerialized 
    : ActorSerializableStateMachine<
        PlayerActorSerialized,
        PlayerStateID,
        PlayerCommand,
        PlayerMessage>
{
    // States[] is inherited.
}
```

---

# 🎮 **Player Input Controller Example**

```csharp
public class PlayerInputController : IController<PlayerInform>
{
    private readonly PlayerActor _actor;

    public void TickInput()
    {
        if (InputSystem.JumpPressed)
            _actor.ExecuteCommand(PlayerCommand.Jump, null);

        if (InputSystem.MoveLeftHeld)
            _actor.ExecuteCommand(PlayerCommand.Move, Vector2.left);

        if (InputSystem.MoveRightHeld)
            _actor.ExecuteCommand(PlayerCommand.Move, Vector2.right);
    }

    public void Inform(PlayerInform info, params object[] args)
    {
        if (info == PlayerInform.Jumped)
            Logger.Log("Player jumped");
    }
}
```

---

# 🧱 **Actor Example**

```csharp
public class PlayerActor 
    : ActorStateMachine<PlayerActor, PlayerStateID, PlayerMessage, PlayerCommand>
{
    private PlayerInputController _controller;

    private void Awake()
    {
        _controller = new PlayerInputController(this);

        RegisterState(PlayerStateID.Idle, new PlayerIdleState());
        RegisterState(PlayerStateID.Moving, new PlayerMovingState());
        RegisterState(PlayerStateID.Jumping, new PlayerJumpState());

        ChangeState(PlayerStateID.Idle);
    }

    protected override void Update()
    {
        _controller.TickInput();   // Controller decides
        base.Update();             // Actor executes
    }
}
```

---

# ⭐ **Benefits**

- Deterministic gameplay  
- Clean, testable architecture  
- Engine‑agnostic core  
- Supports designer‑authored serialized FSMs  
- Scales to AI, networking, cutscenes, tools  
- Familiar to Unreal developers, but simpler and more explicit  
- Ideal for long‑term maintainability and cross‑engine portability  

---
