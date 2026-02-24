# ⚡ Global Event System (Core.EventX)

```csharp
using XFG;
``` 
A zero‑allocation, thread‑safe, strongly‑typed event system for **com.xfg.corelib**.

`Core.EventX` provides a lightweight, engine‑agnostic event dispatcher designed for
high‑performance gameplay systems. It avoids reflection, avoids `params object[]`,
avoids `DynamicInvoke`, and avoids per‑broadcast allocations — making it suitable
for real‑time games, tools, and Burst‑friendly pipelines.

The system supports:
- Any **KeyType** (string, enum, struct, EventId, etc.)
- Strongly‑typed delegates (`Action`, `Action<T1>`, `Action<T1,T2>`, …)
- Zero allocations during broadcast
- Thread‑safe operations via `ConcurrentDictionary`
- Up to **4 parameters** per event

This is the foundational event system used throughout **com.xfg.corelib**.

---

## ✨ Key Features

- 🚀 **Zero‑allocation broadcast**  
  No `params`, no boxing, no reflection, no `DynamicInvoke`.

- 🔒 **Thread‑safe**  
  Uses `ConcurrentDictionary` internally.

- 🎯 **Strongly typed**  
  Payloads are compile‑time typed — no casting or object arrays.

- 🧩 **Flexible KeyType**  
  Works with strings, enums, structs, or custom `EventId` types.

- 🧵 **Supports up to 4 parameters**  
  - `EventX<KeyType>`  
  - `EventX<KeyType, T1>`  
  - `EventX<KeyType, T1, T2>`  
  - `EventX<KeyType, T1, T2, T3>`  
  - `EventX<KeyType, T1, T2, T3, T4>`

- 📦 **Minimal API surface**  
  Easy to onboard, safe to scale across large codebases.
---

## Usages (0 to 4 Parameters)

### 🚀 Basic Usage (0 Parameters)

```csharp
Core.EventX<string>.Subscribe("OnStart", OnStart);

void OnStart()
{
    Console.WriteLine("Game started!");
}

Core.EventX<string>.Broadcast("OnStart");
```

### 🧩 Usage With 1 Parameter
```csharp
Core.EventX<string, int>.Subscribe("DamageTaken", OnDamageTaken);

void OnDamageTaken(int amount)
{
    Console.WriteLine($"Player took {amount} damage.");
}

Core.EventX<string, int>.Broadcast("DamageTaken", 25);
```
### 🧩 Usage With 2 Parameter
```csharp
Core.EventX<string, int, float>.Subscribe("DamageTaken", OnDamageTaken);

void OnDamageTaken(int amount, float knockback)
{
    Console.WriteLine($"Damage: {amount}, Knockback: {knockback}");
}

Core.EventX<string, int, float>.Broadcast("DamageTaken", 25, 3.5f);
```

### 🧩 Usage With 3 Parameter
```csharp
Core.EventX<string, int, float, bool>.Subscribe("Hit", OnHit);

void OnHit(int damage, float force, bool crit)
{
    Console.WriteLine($"Hit for {damage}, force {force}, crit={crit}");
}

Core.EventX<string, int, float, bool>.Broadcast("Hit", 10, 2.0f, true);
```

### 🧩 Usage With 4 Parameter
```csharp
Core.EventX<string, int, float, bool, string>.Subscribe("Hit", OnHit);

void OnHit(int dmg, float force, bool crit, string source)
{
    Console.WriteLine($"{source} hit for {dmg}, crit={crit}");
}

Core.EventX<string, int, float, bool, string>.Broadcast("Hit", 10, 2.0f, true, "Enemy");
```
---

## 🔍 Introspection API
```csharp
bool exists = Core.EventX<string>.HasEvent("OnStart");
int count = Core.EventX<string>.Count("OnStart");

Core.EventX<string>.ClearEvent("OnStart");
Core.EventX<string>.ClearAll();
```

## 🧩 Usage With Enums (Recommended)

Using an enum as the event key provides:
- Compile‑time safety  
- Auto‑completion  
- No string allocations  
- No typo risk  
- Clean grouping of event domains  

```csharp
public enum GameEvent
{
    PlayerDied,
    DamageTaken,
    LevelLoaded
}
```

### Subscribe

```csharp
Core.EventX<GameEvent, int>.Subscribe(GameEvent.DamageTaken, OnDamageTaken);
```

### Handler

```csharp
void OnDamageTaken(int amount)
{
    Console.WriteLine($"Player took {amount} damage.");
}
```

### Broadcast

```csharp
Core.EventX<GameEvent, int>.Broadcast(GameEvent.DamageTaken, 50);
```

### Example With Two Parameters

```csharp
public enum CombatEvent
{
    HitLanded,
    CriticalHit
}

Core.EventX<CombatEvent, int, float>.Subscribe(CombatEvent.HitLanded, OnHit);

void OnHit(int damage, float force)
{
    Console.WriteLine($"Hit for {damage} with force {force}");
}

Core.EventX<CombatEvent, int, float>.Broadcast(CombatEvent.HitLanded, 20, 1.5f);
```


## 🧱 Recommended: Strongly‑Typed Event IDs

Instead of strings, you can use a struct:

```csharp
public readonly struct EventId
{
    public readonly int Value;
    public EventId(int value) => Value = value;
}
```

Usage:

```csharp
static readonly EventId DamageEvent = new EventId(1);

Core.EventX<EventId, int>.Subscribe(DamageEvent, OnDamage);
Core.EventX<EventId, int>.Broadcast(DamageEvent, 50);
```
This pattern is common in AAA engines and scales extremely well.

---

## 🧠 When to Use Core.EventX

Use this system when you need:

- High‑frequency events  
- Zero‑allocation dispatch  
- Strong typing  
- Thread‑safe event registration  
- Engine‑agnostic behavior  
- Clean separation between managed and Burst systems  

It is ideal for gameplay, tools, UI, and hybrid DOTS pipelines.

---

## 📦 Summary

`Core.EventX` is a fast, deterministic, strongly‑typed event system designed for
modern game architectures. It avoids the pitfalls of UnityEvent, C# events, and
reflection‑based systems while remaining simple and expressive.

It is the recommended event dispatcher for **com.xfg.corelib**.
