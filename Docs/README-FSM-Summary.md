# XFG StateMachine — Technical Summary  
**Core Architecture, Guarantees, and Extension Points**

The XFG StateMachine is a **generic, type‑safe, deterministic finite state machine** designed for Unity gameplay systems. It provides a minimal but extensible foundation for synchronous state transitions, message routing, and lifecycle management.

This document summarizes the architecture and technical guarantees of the implementation below.

## Technical Feature Comparison

| **Feature**                  | **Core FSM (Your Implementation)**                                          | **Async FSM**                                                                 | **Pushdown FSM**                                                              | **Hierarchical FSM**                                                           |
|-----------------------------|------------------------------------------------------------------------------|-------------------------------------------------------------------------------|--------------------------------------------------------------------------------|---------------------------------------------------------------------------------|
| **Transition Function**     | `ChangeState(nextStateType, params object[] args)`                          | `ChangeStateAsync(nextStateType, params object[] args)` (awaitable)          | `PushState(stateId)`, `PopState()`, `PeekState()`                              | Parent FSM calls `ChangeState` on child FSMs                                    |
| **Transition Semantics**    | Synchronous, deterministic                                                   | Awaitable enter/exit, async-safe                                              | Stack-based: push suspends previous, pop resumes                               | Parent exit → child exit → parent enter → child default enter                   |
| **Transition Ordering**     | 1. `OnStateExit` → 2. Update active refs → 3. `OnStateEnter`                | Same ordering, but each step may `await`                                      | Pop: `OnStateExit` → resume previous; Push: suspend current → enter new        | Hierarchical cascade of exit/enter across levels                                |
| **State Registry**          | `Dictionary<TStateIDType, IState>`                                          | Same as Core FSM                                                             | Same as Core FSM                                                               | Each FSM (parent/child) has its own registry                                   |
| **Active State Tracking**   | `CurrentStateType`, `CurrentState`, `HasState`                              | Same as Core FSM                                                             | Top of stack is active; lower states suspended                                 | Active state = deepest active child                                             |
| **Lifecycle Methods**       | `OnStateEnter`, `OnStateUpdate`, `OnStateExit`, `OnReceiveMessage`          | Adds: `OnStateEnterAsync`, `OnStateExitAsync`                                 | Adds: `OnStateSuspend`, `OnStateResume`                                        | Same as Core FSM, but applied recursively to child FSMs                         |
| **Message Routing**         | `SendMessageToMachine(msgType, args)` → active state only                   | Same as Core FSM                                                             | Routed to top-of-stack state only                                              | Routed parent → child or bubbled child → parent                                 |
| **Default State Behavior**  | No default; must explicitly call `ChangeState`                              | Same                                                                          | Stack must be seeded manually                                                  | Entering parent auto-enters default child                                       |
| **Cancellation Support**    | ❌                                                                            | ✅ via `CancellationToken`                                                   | ❌                                                                              | Optional if async child FSM is used                                             |
| **Suspend/Resume**          | ❌                                                                            | ❌                                                                            | `OnStateSuspend`, `OnStateResume`                                              | ❌ (child FSMs are exited/re-entered, not suspended)                            |
| **Nested Composition**      | ❌                                                                            | ❌                                                                            | ❌                                                                              | Parent FSM owns child FSM                                                       |
| **Stack Depth Awareness**   | ❌                                                                            | ❌                                                                            | `PeekState()` available                                                        | ❌                                                                              |
| **Unity Integration**       | MonoBehaviour-based; external `UpdateMachine()` tick                        | Same as Core FSM                                                             | Same as Core FSM                                                               | Same as Core FSM, with nested FSM containers                                    |
| **Editor Tooling Support**  | GraphView editor, visual transitions, runtime debugger                       | Same as Core FSM                                                             | Same as Core FSM                                                               | Same as Core FSM, with nested graph support (optional)                          |


---

## 1. Architectural Overview

The machine is defined as:

```csharp
public abstract class IStateMachine<TMachineType, TStateIDType, TMessageType> : MonoBehaviour
```

It enforces:

- **Strong typing** for machine type, state IDs, and message types  
- **Explicit state registration**  
- **Deterministic synchronous transitions**  
- **Predictable lifecycle ordering**  
- **Clean separation between machine and state logic**

The machine does **not** assume a default state. External systems must explicitly trigger the first transition.

---

## 2. Internal Storage Model

| Component | Description |
|----------|-------------|
| `Dictionary<TStateIDType, IState> _states` | Registry of all states keyed by strongly‑typed IDs |
| `CurrentStateType` | The ID of the active state |
| `CurrentState` | The active state instance |
| `HasState` | Indicates whether the machine has entered any state |

States register themselves via:

```csharp
RegisterState(IState state)
```

Each state receives a back‑reference to the owning machine.

---

## 3. Lifecycle Callbacks

Each state implements:

```csharp
void OnStateEnter(TStateIDType prevStateType, object[] args)
void OnStateUpdate()
void OnStateExit(TStateIDType nextStateType, object[] args)
void OnReceiveMessage(TMessageType msgtype, object[] args)
```

### Lifecycle Ordering Guarantee

Every transition follows the same deterministic sequence:

1. **Exit old state**  
2. **Update machine’s active state reference**  
3. **Enter new state**

This ordering is guaranteed by:

```csharp
prevState?.OnStateExit(nextStateType, args);
CurrentStateType = nextStateType;
CurrentState = nextState;
nextState.OnStateEnter(prevStateType, args);
```

---

## 4. Transition Pipeline

Transitions are triggered via:

```csharp
ChangeState(TStateIDType nextStateType, params object[] args)
```

The pipeline ensures:

- No re‑entry into the same state  
- No transition to unregistered states  
- Clean exit/enter ordering  
- Machine state updated **before** entering the new state  
- Full type safety for state IDs

If a state is not registered, the machine logs a warning and aborts the transition.

---

## 5. Update Loop

External systems (usually `MonoBehaviour.Update`) call:

```csharp
UpdateMachine()
```

This forwards update ticks to the active state only:

```csharp
CurrentState?.OnStateUpdate();
```

No other state receives updates.

---

## 6. Message Routing

Messages are delivered to the active state via:

```csharp
SendMessageToMachine(TMessageType msgtype, params object[] args)
```

If no state is active, the message is ignored with a warning.

This provides a clean, explicit message‑passing layer without global dispatch or hidden routing.

---

## 7. State Interface Contract

Each state must implement:

```csharp
public interface IState
{
    TMachineType Machine { get; set; }
    TStateIDType ID { get; }
}
```

This ensures:

- Strongly typed machine reference  
- Strongly typed state identifier  
- Optional override of lifecycle methods  

States are lightweight, self‑contained logic units.

---

## 8. Extensibility

The design intentionally keeps the core FSM minimal so it can be extended into:

### **Async FSM**
- Adds `OnStateEnterAsync` / `OnStateExitAsync`
- Awaitable transitions
- Cancellation tokens

### **Pushdown FSM**
- Stack‑based state layering
- Suspend/resume semantics

### **Hierarchical FSM**
- Parent/child FSM composition
- Message bubbling
- Nested lifecycle propagation

The core FSM is the foundation for all higher‑level FSM variants.

---

## 9. Design Principles

- **Explicit > Implicit**  
  No hidden transitions or default states.

- **Deterministic > Magical**  
  Transition ordering is guaranteed and observable.

- **Typed > String‑based**  
  All IDs and messages use strongly typed enums or structs.

- **Composable > Monolithic**  
  Extensions (async, pushdown, hierarchical) build on the same core.

- **Unity‑Friendly**  
  MonoBehaviour integration, clean update loop, predictable behavior.

---

## 10. Summary

The XFG StateMachine provides:

- A robust, deterministic foundation for gameplay logic  
- A clean API for state transitions and message routing  
- A strongly typed, extensible architecture  
- A predictable lifecycle model  
- A minimal surface area that scales into advanced FSM variants  

This makes it ideal for AI, UI flows, combat systems, interaction logic, and any gameplay system requiring explicit, maintainable state control.
