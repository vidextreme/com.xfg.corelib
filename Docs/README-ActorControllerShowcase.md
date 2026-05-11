# 🎮 **XFG Actor–Controller Architecture**
### A deterministic, engine‑agnostic gameplay framework  
Built for Unity, Godot, MonoGame, and custom engines.

- Clear separation of **decision** (Controller) and **execution** (Actor)  
- Deterministic **command buffer**  
- Strongly‑typed **FSM**  
- Supports **player input**, **AI**, **networking**, **cutscenes**, **tools**  
- Inspired by Unreal’s Actor–Controller model, adapted for C#

---

# 🧩 **What This Architecture Represents**
This architecture is built around a strict separation of responsibilities that produces predictable, deterministic, and maintainable gameplay behavior.

- **Controllers decide** what should happen  
- **Actors execute** those decisions through a state machine  
- **States encapsulate behavior** and transitions  
- **Inform() provides feedback** from Actor → Controller  

This creates a clean, extensible loop suitable for gameplay, AI, networking, and tools.

---

# 🏛️ **Inspiration from Unreal Engine**
This design draws directly from Unreal Engine’s proven Actor–Controller pattern:

- Unreal separates **Controller** (intent, input, AI) from **Pawn/Character** (movement, abilities).  
- Controllers issue **commands or input events**, not direct state changes.  
- Pawns emit **events** back to Controllers (OnLanded, OnJumped, etc.).  
- Controllers can be swapped (AI, player, network) without modifying the Pawn.  

Your architecture adapts these principles to C# engines with:

- Strongly‑typed commands  
- A deterministic command buffer  
- A lightweight, engine‑agnostic FSM  
- Serializable polymorphic states (Unity)  
- A clean Inform() callback channel  

It preserves Unreal’s strengths while making the model portable, explicit, and deterministic.

---

# ⚖️ **Unreal Engine vs XFG Architecture**

| Concept / Behavior | Unreal Engine | XFG Actor–Controller Architecture |
|--------------------|---------------|----------------------------------|
| **Decision Layer** | Controller (PlayerController, AIController) | Controller (PlayerInputController, AIController, NetworkController) |
| **Execution Layer** | Pawn / Character | Actor (FSM‑driven entity) |
| **How Decisions Are Sent** | Input events, movement functions, ability triggers | Typed commands via `ExecuteCommand(cmd, param)` |
| **How Execution Happens** | Pawn processes input, CharacterMovement, components | Actor processes commands through FSM + command buffer |
| **Feedback to Controller** | Delegates, events (OnLanded, OnJumped, etc.) | `Inform(info, args)` callback |
| **Possession Model** | Controller possesses Pawn | Controller attaches to Actor |
| **State Management** | State Trees, Behavior Trees, components | Strongly‑typed FSM with polymorphic states |
| **Determinism** | Not guaranteed | Guaranteed via command buffer + FSM |
| **Engine Dependency** | Unreal‑specific | Engine‑agnostic |
| **Serialization** | Blueprint, UObjects | `SerializeReference` polymorphic states (Unity) |

---

# 🧠 **Core Concepts**

### **Actor**
- Owns the FSM  
- Executes commands deterministically  
- Processes buffered input  
- Emits Inform events back to Controller  

### **Controller**
- Decides what the Actor should do  
- Sends typed commands  
- Reacts to Actor events via `Inform()`  
- Never mutates Actor state directly  

### **Command Buffer**
- FIFO  
- Prevents mid‑frame state changes  
- Ensures deterministic behavior across all controllers  

---

# 🏗️ **Architecture Overview**

```
┌──────────────────────────┐
│        Controller         │
│  (AI / Player / Network)  │
└───────────────┬──────────┘
                │ ExecuteCommand()
                ▼
┌──────────────────────────┐
│           Actor           │
│  - Command Buffer         │
│  - FSM (StateMachine)     │
└───────────────┬──────────┘
                │ Routes command
                ▼
┌──────────────────────────┐
│        Actor State        │
│   (IActorState<TCommand>) │
└──────────────────────────┘
```

---

# 🔁 **Actor ↔ Controller Inform Loop**

```
Controller ──► ExecuteCommand(cmd)
     ▲                          │
     │                          ▼
Inform(info) ◄── State ◄── Actor (FSM + Buffer)
```

### **Flow**
1. Controller decides → sends command  
2. Actor buffers → processes deterministically  
3. State executes logic  
4. State informs Controller  
5. Controller reacts → may issue new commands  

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

---
