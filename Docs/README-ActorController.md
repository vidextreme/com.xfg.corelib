# 🎮 Actor–Controller Architecture with Command Buffer

A lightweight, deterministic, and extensible Actor–Controller framework for Unity, Godot, MonoGame, and custom engines.  
Designed for gameplay systems that require clean separation between **decision‑making** (Controllers) and **execution** (Actors), with support for **AI**, **player input**, **network commands**, and **editor‑authored FSM states**.

This module is part of the **XFG Simple Game Core Library**, a collection of engine‑agnostic C# systems for building reliable, deterministic gameplay foundations.

The architecture is inspired by Unreal Engine’s Actor–Controller model, applying the same clean separation of responsibilities while adding a deterministic command buffer and a strongly‑typed FSM tailored for C#‑based engines.

---

## 🌐 Engine‑Agnostic Design

This framework is **engine‑agnostic at its core**.

The only engine‑specific dependency is the base type constraint on `TMachineType`, which is wrapped in conditional compilation:

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

This allows the same Actor–Controller architecture to run in:

- **Unity**
- **Godot**
- **MonoGame**
- **Custom engines**

The FSM, command buffer, and Actor/Controller abstractions are **pure C#** and require no engine APIs.

---

## 🚀 Core Concepts

### 🧩 Actor  
An Actor is a state‑driven gameplay entity. It receives commands from Controllers and executes them through its FSM.

Actors:
- Own the FSM  
- Process commands deterministically  
- Expose a single entry point:

```csharp
ExecuteCommand<T>(command, parameter)
```

### 🎛️ Controller  
A Controller decides what the Actor should do.

Examples:
- Player input controller  
- AI controller  
- Network controller  
- Scripted/cutscene controller  

Controllers send commands immediately, but Actors process them later.

### 📬 Command Buffer  
A FIFO queue that stores commands until the Actor’s update tick.

This ensures:
- No mid‑frame state changes  
- Deterministic behavior  
- Identical behavior for AI, player, and network controllers  
- Clean decoupling between decision and execution  

---

## 🧱 Architecture Overview

```
┌───────────────────────────┐
│        Controller         │
│  (AI / Player / Network)  │
└───────────────┬───────────┘
                │ ExecuteCommand(cmd, param)
                ▼
┌───────────────────────────┐
│           Actor           │
│  - Command Buffer         │
│  - FSM (StateMachine)     │
└───────────────┬───────────┘
                │ Routes command to current state
                ▼
┌───────────────────────────┐
│        Actor State        │
│   (IActorState<TCommand>) │
└───────────────────────────┘
```

---

## 🧠 Similarities to Unreal Engine’s Actor–Controller Model

Your architecture is conceptually aligned with Unreal’s:

### ✔ 1. Decision vs Execution  
Unreal: Controller decides → Pawn executes  
XFG: Controller decides → Actor executes  

### ✔ 2. Possession‑like Behavior  
Unreal Controllers possess Pawns  
XFG Controllers attach to Actors  

### ✔ 3. Commands Instead of Direct Manipulation  
Unreal Controllers issue movement/intent  
XFG Controllers issue typed commands  

### ✔ 4. Inform ≈ Unreal Events/Delegates  
Unreal: OnLanded, OnJumped, OnTakeDamage  
XFG: Inform(PlayerInform.Landed)  

### ✔ 5. Clean Decoupling  
Both enforce:
- Controller never mutates state directly  
- Actor never makes decisions  
- Communication is explicit and directional  

This makes the system familiar to Unreal developers while remaining deterministic, engine‑agnostic, and C#‑friendly.

---

# 🎨 Actor ↔ Controller Inform Design Overview

This diagram shows the **full communication loop** between Controller, Actor, and Actor States.

```
                   ┌───────────────────────────────┐
                   │          CONTROLLER           │
                   │  (Player, AI, Network, etc.)  │
                   └───────────────┬───────────────┘
                                   │
                                   │ ExecuteCommand(cmd, param)
                                   ▼
                   ┌───────────────────────────────┐
                   │             ACTOR             │
                   │  - Command Buffer             │
                   │  - FSM (StateMachine)         │
                   └───────────────┬───────────────┘
                                   │
                                   │ Routes command to current state
                                   ▼
                   ┌───────────────────────────────┐
                   │          ACTOR STATE          │
                   │  (IActorState<TCommand>)      │
                   └───────────────┬───────────────┘
                                   │
                                   │ Inform(info, args)
                                   ▼
                   ┌───────────────────────────────┐
                   │          CONTROLLER           │
                   │  Reacts to Actor events       │
                   └───────────────────────────────┘
```

### Flow Summary

1. **Controller decides** → sends command  
2. **Actor buffers** → processes deterministically  
3. **State executes** → performs logic  
4. **State informs Controller** → Controller reacts  
5. **Controller may issue new commands** → loop continues  

This creates a clean, deterministic, testable gameplay loop.

---

# 🧩 Implementing an Actor (Full Example)

### 1. Define State IDs, Messages, Commands, Inform Types

```csharp
public enum PlayerStateID { Idle, Moving, Jumping }
public enum PlayerMessage { None }
public enum PlayerCommand { Move, Jump }
public enum PlayerInform { Jumped, Landed, TookDamage }
```

---

### 2. Implement the Actor

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
        base.Update();             // Actor executes (FSM + command buffer)
    }
}
```

---

### 3. Implement States

```csharp
public class PlayerIdleState 
    : IActorState<PlayerCommand>, IState<PlayerActor, PlayerStateID, PlayerMessage>
{
    public void OnEnter(PlayerActor actor) { }
    public void OnExit(PlayerActor actor) { }
    public void OnUpdate(PlayerActor actor, float dt) { }

    public void ExecuteCommand<T>(PlayerCommand cmd, T param)
    {
        if (cmd == PlayerCommand.Move)
            actor.ChangeState(PlayerStateID.Moving);

        if (cmd == PlayerCommand.Jump)
            actor.ChangeState(PlayerStateID.Jumping);
    }
}
```

---

# 🕹️ Implementing a Player Input Controller (IController Example)

```csharp
public class PlayerInputController : IController<PlayerInform>
{
    private readonly PlayerActor _actor;

    public PlayerInputController(PlayerActor actor)
    {
        _actor = actor;
    }

    public void TickInput()
    {
        // Engine-agnostic pseudo-input
        if (InputSystem.JumpPressed)
            _actor.ExecuteCommand(PlayerCommand.Jump, null);

        if (InputSystem.MoveLeftHeld)
            _actor.ExecuteCommand(PlayerCommand.Move, Vector2.left);

        if (InputSystem.MoveRightHeld)
            _actor.ExecuteCommand(PlayerCommand.Move, Vector2.right);
    }

    public void Inform(PlayerInform info, params object[] args)
    {
        switch (info)
        {
            case PlayerInform.Jumped:
                Logger.Log("Player jumped");
                break;

            case PlayerInform.Landed:
                Logger.Log("Player landed");
                break;

            case PlayerInform.TookDamage:
                int amount = (int)args[0];
                Logger.Log($"Player took {amount} damage");
                break;
        }
    }
}
```

---

# 🧩 Using the Serialized Version (Unity Editor Workflow)

If you want designers to configure states visually:

- Use `ActorSerializableStateMachine<>`
- States become Unity‑serializable via `SerializeReference`
- You can assign, reorder, and edit states directly in the Inspector
- **No need to declare your own `States` array — it is inherited**

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

## 🛠 Extending the System

- Priority command buffers  
- Interrupt commands  
- Network timestamps  
- Possession manager  
- AI planners  
- Cutscene controllers  

---

## 📄 License

This project is licensed under the MIT License.  
See the `LICENSE` file for details.
