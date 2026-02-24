# ⚡ What Is Core.EventX?
### A foundational module of the **XFG Simple Game Core Library**

Core.EventX is:
- A **zero‑allocation**, **thread‑safe**, **strongly‑typed** event dispatcher  
- Designed for **real‑time gameplay**, **tools**, and **modular architectures**  
- Engine‑agnostic and portable across any C# runtime  
- A core building block of the **XFG Simple Game Core Library**, powering clean, scalable communication between systems  

It provides a modern alternative to UnityEvent for production‑grade engineering.

---

# Why Core.EventX Exists
### Built for the architectural goals of the XFG Simple Game Core Library

UnityEvent is convenient, but not built for:
- High‑frequency gameplay  
- Deterministic performance  
- Modular engine‑agnostic systems  
- Burst/DOTS pipelines  
- Thread‑safe architectures  

Core.EventX solves these problems by offering:
- Zero allocations  
- Strong typing  
- Thread safety  
- No reflection  
- No serialization fragility  

It aligns with the XFG library’s mission: **simple, explicit, maintainable systems**.

---

# Core.EventX in One Sentence
### “A fast, deterministic, strongly‑typed event system powering the XFG Simple Game Core Library.”

---

# Core.EventX: How It Works
### Simple, predictable, and strongly typed.

- Events are keyed by a type you choose (enum, struct, string…)  
- Handlers are stored in a thread‑safe dictionary  
- Broadcasts invoke delegates directly — no reflection  
- Zero allocations during dispatch  
- Supports 0–4 parameters  

Example:

```csharp
Core.EventX<GameEvent, int>.Broadcast(GameEvent.DamageTaken, 50);
```

---

# Core.EventX: Ideal Use Cases
### When you want performance, clarity, and control.

Use Core.EventX for:
- Combat events  
- Physics events  
- AI events  
- Animation events  
- Networking  
- Tools and editor extensions  
- Modular gameplay systems  
- Burst/DOTS bridging  

These are the exact domains the XFG Simple Game Core Library is designed to support.

---

# UnityEvent: What It Is
### A convenient, inspector‑driven event system for designers.

UnityEvent is:
- Serialized  
- Inspector‑friendly  
- Easy for non‑programmers  
- Great for UI and simple interactions  

But it is **not** designed for:
- High‑frequency events  
- Scalable architectures  
- Deterministic performance  
- Burst/DOTS pipelines  

---

# UnityEvent: Technical Limitations
### Why UnityEvent breaks down in real gameplay systems.

UnityEvent suffers from:
- Allocations on add/remove/invoke  
- Reflection‑based invocation  
- Boxing of value types  
- Fragile serialized references  
- No thread safety  
- No Burst compatibility  
- Slow invocation path  

---

# Simple Comparison

| Category | UnityEvent | Core.EventX (XFG Library) |
|---------|------------|----------------------------|
| Performance | ❌ Slow | ✔ Fast |
| Allocations | ❌ Many | ✔ Zero |
| Reflection | ❌ Yes | ✔ None |
| Type Safety | ⚠ Medium | ✔ Strong |
| Thread Safety | ❌ No | ✔ Yes |
| Burst/DOTS | ❌ Not supported | ✔ Supported via wrapper |
| Scalability | ❌ Poor | ✔ Excellent |
| Inspector Wiring | ✔ Yes | ❌ No |
| Best For | UI, designers | Gameplay, systems |

---

# When to Use Core.EventX
### Choose Core.EventX when you need:

- High‑frequency dispatch  
- Zero allocations  
- Strong typing  
- Thread safety  
- Deterministic behavior  
- Modular architecture  
- Burst/DOTS compatibility  

This is why it’s a core pillar of the **XFG Simple Game Core Library**.

---

# When NOT to Use Core.EventX
### Choose UnityEvent when you need:

- Inspector‑driven wiring  
- Designer‑authored logic  
- Visual Scripting integration  
- Per‑instance MonoBehaviour events  
- Serialized callbacks  

---

# Final Takeaway
### UnityEvent is for **designers**.  
### Core.EventX is for **engineers**.  
### And within the XFG Simple Game Core Library, Core.EventX is the **standard**.

