# 🎬 Sequencer System

```csharp
using XFG.Sequencer;
```

### A Deterministic, Track-Based Step Runner for Cutscenes, Scripted Events, and Gameplay Flow

## What Is the Sequencer?

The **Sequencer** plays ordered lists of **steps** (wait, move, play animation, show dialogue, etc.) over time.
Steps are grouped into **sequences**, and sequences run on **tracks** that decide what is allowed to play at the same time.

It is designed for:

- Cutscenes and scripted moments  
- Tutorial and dialogue flow  
- Ability or combo timelines  
- UI transitions and modal flows  
- Any "do this, then that, while this other thing runs" logic  

The core is plain C# with no engine dependencies. Steps are authored as serializable data, so a list of steps can be edited in the Unity Inspector and turned into runtime logic when the sequence starts.

---

## 📦 Core Concepts

| Type | Role |
|------|------|
| `Sequencer` | Owns the tracks and sequences; you call `Update`/`LateUpdate` on it every frame |
| `SequenceTrack` | A named lane that sequences play on (`Universal` or `Stack` mode) |
| `Sequence` | One running list of steps on a track |
| `Step` | Serializable wrapper holding one `IStepData` (`[SerializeReference]`) |
| `IStepData` | Authoring-time data for a step; builds the runtime logic via `CreateLogic` |
| `IStepLogic` | Runtime logic for one step: lifecycle callbacks and state |
| `IStepContext` | Marker interface for the owner passed to `StartSequence` (e.g. the actor or cutscene) |

### Data vs Logic

Each step has two halves:

- **`IStepData`** is what designers edit. It is serialized and shared; it should not hold runtime state.
- **`IStepLogic`** is created fresh for every sequence run by `IStepData.CreateLogic(sequence, owner)`. It holds the runtime state (timers, targets, handles).

This split lets the same authored step list play many times, even at once, without runs interfering with each other.

---

## 🔁 Step Lifecycle

Every `IStepLogic` has a `State` that the sequence reads each tick:

| State | Who sets it | What the sequence does |
|-------|-------------|------------------------|
| `NotStarted` | Initial value | Calls `OnStart()` |
| `Running` | The step, in `OnStart()` | Calls `OnTick`/`OnLateTick` every frame |
| `AllowNext` | The step | Keeps ticking it, and also lets the following steps start |
| `Ending` | The step, usually in `TryEnd()` | Ticks it one last time, calls `OnEnd()`, then sets `Ended` |
| `Ended` | The sequence | Disposes the step and removes it |
| `Released` | The sequence | Final state after `Dispose()` |

Full order for a typical step:

```
CreateLogic → Initialize → OnStart → OnTick… → (State = Ending) → OnTick → OnEnd → Dispose
```

Rules for step authors:

- **Set `State = StepState.Running` in `OnStart()`.** If the state stays `NotStarted`, `OnStart()` is called again next frame.
- **Finish by setting `State = StepState.Ending`** (typically from `TryEnd()`). Don't set `Ended` or `Released` yourself; the sequence does that, and a step left in `Ended` by its own code is never cleaned up.
- `Initialize()` is called by the sequencer right after `CreateLogic`, before the sequence is registered. Don't call it yourself.

### Sequential vs Concurrent

`IStepLogic.PlayType` controls whether a step blocks the steps after it:

- **`Sequential`**: while this step is `NotStarted` or `Running`, later steps wait.  
- **`Concurrent`**: later steps start and tick in the same frame.  

A sequential step can also set its state to **`AllowNext`** to keep running while letting the next step begin, for example a camera move that keeps going while the dialogue after it starts.

A sequence is **done** when all of its steps have been removed.

---

## 🛤️ Tracks

Register tracks once, up front:

```csharp
var sequencer = new Sequencer();
SequenceTrack ambient = sequencer.RegisterTrack("Ambient", SequenceTrackMode.Universal);
SequenceTrack gameplay = sequencer.RegisterTrack("Gameplay", SequenceTrackMode.Stack);
SequenceTrack cutscene = sequencer.RegisterTrack("Cutscene", SequenceTrackMode.Stack);
```

| Mode | Behavior |
|------|----------|
| `Universal` | Always active. Use for ambient or background sequences that never yield. |
| `Stack` | Behaves like a priority stack. Among stack tracks that have at least one sequence, the **last registered** one is active. Sequences on lower stack tracks are **muted**. |

In the example above, starting a sequence on `cutscene` mutes everything on `gameplay`. When the cutscene finishes, `gameplay` is unmuted.

`IsTrackActive(trackID)` reports whether a track is currently active. A stack track with no sequences is never active.

### Muting

Muting is a **notification**, not a pause. A muted sequence still ticks. Each step receives `OnMute()` / `OnUnmute()` and decides what to do (pause a timer, hide UI, stop audio, etc.).

---

## 🚀 Quick Start

### 1. Define a step (data + logic)

```csharp
using System;
using XFG.Sequencer;

[Serializable]
public class WaitStepData : IStepData
{
    public float Duration = 1f;

    public StepPlayType PlayType => StepPlayType.Sequential;

    public IStepLogic CreateLogic(Sequence sequence, IStepContext owner)
        => new WaitStepLogic(Duration, PlayType);
}

public class WaitStepLogic : IStepLogic
{
    private readonly float _duration;
    private float _elapsed;
    private bool _paused;

    public WaitStepLogic(float duration, StepPlayType playType)
    {
        _duration = duration;
        PlayType = playType;
    }

    public StepState State { get; set; } = StepState.NotStarted;
    public StepPlayType PlayType { get; }

    public void Initialize() => _elapsed = 0f;

    public void OnStart() => State = StepState.Running;

    public void OnTick(float deltaTime)
    {
        if (State != StepState.Running || _paused)
            return;

        _elapsed += deltaTime;
        if (_elapsed >= _duration)
            TryEnd();
    }

    public void OnLateTick(float deltaTime) { }

    public void OnEnd() { }

    public void TryEnd()
    {
        if (State == StepState.Running || State == StepState.AllowNext)
            State = StepState.Ending;
    }

    public void OnMute() => _paused = true;
    public void OnUnmute() => _paused = false;

    public void OnReceiveMessage<T>(T payload) { }

    public void Dispose() { }
}
```

### 2. Drive the sequencer (Unity example)

```csharp
using System.Collections.Generic;
using UnityEngine;
using XFG.Sequencer;

public class CutscenePlayer : MonoBehaviour, IStepContext
{
    // Each entry's Data is serialized by reference, so any IStepData type can go here
    public List<Step> Steps = new List<Step>();

    private Sequencer _sequencer;
    private SequenceTrack _track;

    private void Awake()
    {
        _sequencer = new Sequencer();
        _track = _sequencer.RegisterTrack("Cutscene", SequenceTrackMode.Stack);
    }

    public void Play()
    {
        Sequence sequence = _sequencer.StartSequence(_track.ID, Steps, this);
        sequence.OnFinished = () => Debug.Log("Cutscene finished");
    }

    private void Update() => _sequencer.Update(Time.deltaTime);
    private void LateUpdate() => _sequencer.LateUpdate(Time.deltaTime);
}
```

The `owner` argument (`this` above) is passed to every `CreateLogic` call, so step logic can reach the actor, cutscene, or service that started it.

---

## ▶️ Starting Sequences

```csharp
Sequence StartSequence(int trackID, IEnumerable<Step> steps, IStepContext owner);
Sequence StartSequenceFiltered(int trackID, Func<Step, bool> filter, IEnumerable<Step> steps, IStepContext owner);
```

When a sequence starts, for each step in order:

1. **Empty slots are skipped**: a `null` step or a step whose `Data` is `null` (for example an unassigned entry in an Inspector list).  
2. **The filter runs** (`StartSequenceFiltered` only). Steps for which it returns `false` are skipped.  
3. **`Data.CreateLogic(sequence, owner)` builds the logic.** It must return a new, non-null instance; returning `null` throws `InvalidOperationException` naming the data type.  
4. **`Initialize()` is called** on the new logic.  

The sequence is added to its track **only after every step is built**. If `CreateLogic` throws, nothing is registered and no half-built sequence is left on the track.

`OnStart()` is not called here; it runs on the next `Update` or `LateUpdate`.

---

## ⏹️ Stopping

| Call | Effect |
|------|--------|
| `sequence.Stop()` | Calls `TryEnd()` on every step of that sequence. Steps end gracefully over the next ticks. |
| `sequencer.StopAllSequences()` | Calls `TryEnd()` on every step of every sequence. |
| `sequencer.StopAllSequences(forceStop: true)` | Immediately calls `OnEnd()` and `Dispose()` on every step and clears all tracks. |

A force stop:
- Does **not** invoke `OnFinished`.  
- Calls `OnEnd()` on every step, including steps that never started.  

Use it for teardown (scene unload, returning to the main menu), not for normal flow.

---

## ✅ Finishing

When a sequence becomes done during a tick, the sequencer:

1. Removes it from its track  
2. Updates track muting  
3. Invokes `sequence.OnFinished`  

Because removal happens first, an `OnFinished` handler can safely start a new sequence, stop others, or check `IsTrackActive`.

---

## 📨 Messaging

```csharp
sequencer.Broadcast(new DialogueAdvanced());
```

`Broadcast<T>` calls `OnReceiveMessage<T>` on every step that is `Running` or `AllowNext`, across all sequences and tracks. Use it for input or game events that steps wait on ("press A to continue", "enemy defeated").

---

## 🔄 Update Loop

- `Update(deltaTime)` calls `OnTick` on steps; `LateUpdate(deltaTime)` calls `OnLateTick`.  
- **Both** passes advance step states (start `NotStarted` steps, finish `Ending` steps). If you only need one pass, it is fine to call only `Update`.  
- Step callbacks may start or stop sequences, including the one being ticked, or force-stop everything. The sequencer detects the change and continues safely.  

---

## ⚠️ Notes & Limitations

- **Muting does not pause.** Muted sequences still tick; handle `OnMute`/`OnUnmute` in your steps.  
- **Force stop** skips `OnFinished` and calls `OnEnd` on unstarted steps (see Stopping above).  
- **`FlushActions()` / `Sequence.PopAction()`** are placeholders for a planned callback queue and currently do nothing.  
- **Inspector type picker:** `Step.Data` uses `[SerializeReference]` without `[Core.SerializableClass]`, so the Inspector shows no built-in way to choose the data type for a new entry. Assign `Data` from code or with your own drawer.  
- **Name lookup:** inside a namespace that starts with `XFG.`, the name `Sequencer` resolves to the `XFG.Sequencer` namespace. Write `XFG.Sequencer.Sequencer` there, or add an alias: `using SequencerRunner = XFG.Sequencer.Sequencer;`.  
- **Not thread-safe.** Call the sequencer from one thread (normally the main thread).  

---

## 🧪 Design Goals

- Deterministic, frame-driven execution  
- Clean separation of authored data and runtime logic  
- Safe re-entrancy from step callbacks  
- Engine-agnostic core; Unity-friendly authoring  
