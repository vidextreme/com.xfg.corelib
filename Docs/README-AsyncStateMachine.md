# 🧠 AsyncStateMachine  
```csharp
using XFG.AI.FSM.Async;
``` 
### 🎮 Asynchronous Finite State Machine for Unity

The `AsyncStateMachine` module extends `IStateMachine` with support for asynchronous transitions, awaited operations, and non-blocking state flows. It is designed for gameplay systems that require sequencing, delays, loading operations, or external async tasks without freezing the main thread.

This document describes the architecture, design goals, and usage patterns for the async FSM layer.

For the base FSM, see:  
[StateMachine ReadMe](README-StateMachine.md)

For Hierarchical FSM support, see:  
[HFSM - ReadMe](README-HierarchicalStateMachine.md)

For Pushdown FSM support, see:  
[Pushdown State Machine - ReadMe](README-PushdownStateMachine.md)

---

## Features

### Asynchronous State Transitions

Async transitions allow states to:

- Await animations  
- Await timers  
- Await external tasks  
- Await loading operations  
- Chain async sequences  

All without blocking Unity’s main thread.

---

### Awaitable Lifecycle Methods

Async states may optionally implement:
- `OnStateEnterAsync`
- `OnStateExitAsync`


These run **in addition to** the synchronous callbacks from `IState`.

The machine guarantees:

1. Exit old state (sync + async)  
2. Update machine references  
3. Enter new state (sync + async)  

---

### Non-Blocking Execution

Async transitions never stall the game loop.  
The FSM continues updating normally while async tasks run in the background.

---

### Backward Compatible

The async layer is implemented as an extension module under:
```csharp
using XFG.AI.FSM.Async;
```

No changes to the base `IStateMachine` are required.  
Synchronous states continue to work normally.



---

## Architecture

`AsyncStateMachineExtensions`
- `ChangeStateAsync`
- `TryChangeStateAsync`
- `AwaitStateExit`
- `AwaitStateEnter`

`IAsyncState` (optional)
- `OnStateEnterAsync`
- `OnStateExitAsync`

## Example: Full AsyncStateMachine Usage

Below is a complete example showing how to:

- Register async-capable states  
- Trigger async transitions  
- Await state entry/exit  
- Combine synchronous and asynchronous logic cleanly  

### Defining a State

```csharp
using XFG.AI.FSM;
using XFG.AI.FSM.Async;
using Cysharp.Threading.Tasks;
// -------------------------------
// Example Async State
// -------------------------------
public class AttackState :
    IStateMachine<PlayerMachine, PlayerStateID, PlayerMessage>.IState,
    IAsyncStateMachine<PlayerMachine, PlayerStateID, PlayerMessage>.IAsyncState
{
    public PlayerMachine Machine { get; set; }
    public PlayerStateID ID => PlayerStateID.Attack;

    public void OnStateEnter(PlayerStateID prev, object[] args)
    {
        Machine.PlayAnimation("Attack");
    }

    public async UniTask OnStateEnterAsync(PlayerStateID prev, object[] args)
    {
        // Wait for animation to finish
        await Machine.Animation.WaitForAnimation("Attack");

        // Automatically transition when done
        Machine.ChangeState(PlayerStateID.Recover);
    }
}

// -------------------------------
// Example Async Follow-up State
// -------------------------------
public class RecoverState :
    IStateMachine<PlayerMachine, PlayerStateID, PlayerMessage>.IState,
    IAsyncStateMachine<PlayerMachine, PlayerStateID, PlayerMessage>.IAsyncState
{
    public PlayerMachine Machine { get; set; }
    public PlayerStateID ID => PlayerStateID.Recover;

    public void OnStateEnter(PlayerStateID prev, object[] args)
    {
        Machine.PlayAnimation("Recover");
    }

    public async UniTask OnStateEnterAsync(PlayerStateID prev, object[] args)
    {
        // Wait for recovery animation
        await Machine.Animation.WaitForAnimation("Recover");

        // Return to Idle
        Machine.ChangeState(PlayerStateID.Idle);
    }
}
```
### Defining a State Machine

```csharp
using XFG.AI.FSM;
using XFG.AI.FSM.Async;
using Cysharp.Threading.Tasks;

public class PlayerMachine :
    IStateMachine<PlayerMachine, PlayerStateID, PlayerMessage>
{
    void Start()
    {
        // Register synchronous + async-capable states
        RegisterState(new IdleState());
        RegisterState(new AttackState());
        RegisterState(new RecoverState());

        // Start in Idle
        ChangeState(PlayerStateID.Idle);
    }

    void Update()
    {
        UpdateMachine();
    }

    // Example of triggering an async transition from gameplay code
    public async UniTask PerformAttackSequence()
    {
        // Switch to Attack and wait for it to finish
        await this.ChangeStateAsync(PlayerStateID.Attack);

        // After Attack finishes, transition to Recover
        await this.ChangeStateAsync(PlayerStateID.Recover);

        // Finally return to Idle
        ChangeState(PlayerStateID.Idle);
    }
}
```


## Example: Using Async Transitions

```csharp
await this.ChangeStateAsync(PlayerStateID.Attack);
```

```csharp
if (await this.TryChangeStateAsync(PlayerStateID.Attack))
{
    // Transition succeeded
}
```

## When to Use Async FSM

Use this module when your gameplay requires:

- Animation‑driven transitions  
- Timed sequences  
- Cutscenes  
- Ability cooldowns  
- Loading screens  
- Network waits  
- Any operation that must not block the main thread  

Async FSM keeps your logic clean, readable, and deterministic.
