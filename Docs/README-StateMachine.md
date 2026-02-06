# 🧠 IStateMachine  

```csharp
using XFG.AI.FSM;
```
### 🎮 Finite State Machine for Unity

The `IStateMachine` module provides the foundational finite state machine used throughout the XFG AI framework. It is intentionally minimal, predictable, and type-safe. All advanced behavior (async transitions, hierarchical FSM, pushdown stacks) is implemented as optional extension layers.

This document describes the architecture, design goals, and usage patterns for the FSM.

For async support, see:  
[Async StateMachine - ReadMe](README-AsyncStateMachine.md)

---

## Features

### Strongly Typed State Identifiers

The machine uses three generic type parameters:

- `TMachineType`   - The MonoBehaviour that owns the machine
- `TStateIDType`   - The identifier for states (usually an enum)
- `TMessageType`   - The identifier for messages (usually an enum)


This ensures compile-time safety and eliminates string-based errors.

---

### Explicit Lifecycle

States implement the nested `IState` interface and may override:
- `OnStateEnter`
- `OnStateUpdate`
- `OnStateExit`
- `OnReceiveMessage`



All callbacks are optional. The machine guarantees ordered transitions:

1. Exit old state  
2. Update machine references  
3. Enter new state  

---

### Predictable Behavior

The FSM does not:

- Auto-transition  
- Implicitly update states  
- Implicitly handle messages  
- Modify state order  

All behavior is explicit and controlled by the user.

---

### Extensible by Design

The base FSM is intentionally minimal.  
Additional capabilities are provided through optional extension layers:

- **AsyncStateMachine** → asynchronous transitions  
- **HierarchicalStateMachine** → parent/child state trees  
- **PushdownStateMachine** → stack-based state control  

Each extension has its own README.

---

## Architecture


`IStateMachine<TMachine, TStateID, TMessage>`
- Registers states
- Stores active state
- Performs synchronous transitions
- Forwards update ticks
- Routes messages to active state

`IState` (nested interface)
- Machine reference
- State ID
- Enter / Update / Exit / Message callbacks


The machine does not assume a default state.  
Transitions must be explicitly triggered.

---

## Example: Defining a State

```csharp
public class IdleState :
    IStateMachine<PlayerMachine, PlayerStateID, PlayerMessage>.IState
{
    public PlayerMachine Machine { get; set; }
    public PlayerStateID ID => PlayerStateID.Idle;

    public void OnStateEnter(PlayerStateID prev, object[] args)
    {
        Machine.PlayAnimation("Idle");
    }

    public void OnStateUpdate()
    {
        if (Machine.Input.Move)
            Machine.ChangeState(PlayerStateID.Move);
    }
}
```
## Example: Using the Machine

```csharp
public class PlayerMachine :
    IStateMachine<PlayerMachine, PlayerStateID, PlayerMessage>
{
    void Start()
    {
        RegisterState(new IdleState());
        RegisterState(new MoveState());
        RegisterState(new AttackState());

        ChangeState(PlayerStateID.Idle);
    }

    void Update()
    {
        UpdateMachine();
    }
}

```

## Message Routing
### Messages are forwarded to the active state:

```csharp
machine.SendMessageToMachine(PlayerMessage.Hit, damageAmount);
```

States may override `OnReceiveMessage` to handle them.

## Design Goals
### Clarity
The FSM is intentionally small.
No hidden transitions, no implicit behavior.

### Extensibility
Async and hierarchical behavior are optional layers, not built-in.

### Maintainability
The API surface is minimal and stable.

### Engine-Grade Reliability
The machine enforces ordered transitions and predictable state ownership.

## When to Use the FSM

Use this module when you need:
- Clean gameplay state logic
- Predictable transitions
- Strong typing
- Minimal overhead
- A stable foundation for more advanced systems

If you need async transitions or hierarchical behavior, simply add the corresponding extension layers.