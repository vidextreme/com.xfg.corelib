# 💥XFG Collision Library  
#### Part of the XFG Simple Game Core Library `com.xfg.corelib`
### High‑Performance Geometry for Games, Tools, and Simulation

The XFG Collision Library is a deterministic, allocation‑free geometry engine designed for real‑time applications such as gameplay, visibility, physics broadphase, and editor tooling. It provides a complete set of convex primitives, a fully symmetric collision matrix, and Burst‑friendly math with no GC allocations and no engine dependencies.

---

## Core Technology

### Deterministic Math Layer
- Pure C# structs  
- No heap allocations  
- No Unity‑specific assumptions  
- Stable across platforms and frame rates  

### SAT‑Driven Collision
- Separating Axis Theorem for OBB, Triangle, AABB, Frustum  
- Explicit edge–axis and face–axis projections  
- Stable under near‑parallel conditions  

### Segment‑Centric Architecture
- Capsule → segment + radius  
- Cylinder → segment + radius  
- Cone → axis segment + height/radius gradient  
- Frustum → 6 planes + segment clipping  

### Burst‑Ready
- No LINQ  
- No virtual dispatch  
- No exceptions  
- SIMD‑friendly math  

---

## Advantages

### Complete Symmetry
Every shape can test against every other shape, including **Frustum vs Frustum**.

### Explicit, Minimal API
No hidden conversions. No magic behavior. Predictable and debuggable.

### Engine‑Agnostic
Works in Unity, Godot, custom engines, or standalone C#.  
The math layer is portable and ASCII‑safe.

### Designed for Gameplay
Fast broadphase, stable narrowphase, and predictable behavior under extreme conditions.

---

## How to Use

### AABB vs Sphere
```csharp
bool hit = bounds.Intersects(sphere.Center, sphere.Radius);
```

### Capsule vs Frustum
```csharp
bool visible = frustum.Intersects(capsule);
```

### Triangle vs OBB
```csharp
bool hit = TrianglePrimitiveCollision.IntersectsTriangleObb(a, b, c, obb);
```

### Frustum vs Frustum
```csharp
bool overlap = frustumA.IntersectsFrustum(frustumB);
```

### Segment vs Anything
```csharp
bool hit = frustum.IntersectsSegment(p0, p1);
```

---

## Collision Matrix

| Shape     | AABB | Sphere | Capsule | Cylinder | Cone | Segment | Triangle | OBB | Frustum |
|-----------|------|--------|---------|----------|------|---------|----------|-----|----------|
| **AABB**      | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| **Sphere**    | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| **Capsule**   | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| **Cylinder**  | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| **Cone**      | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| **Segment**   | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| **Triangle**  | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| **OBB**       | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| **Frustum**   | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |

---

## Summary

The XFG Collision Library provides:
- A complete, symmetric collision matrix  
- Deterministic, allocation‑free math  
- Burst‑friendly performance  
- Clean, explicit APIs  
- Engine‑agnostic portability  
- Production‑ready geometry primitives  

It is designed for real‑time gameplay, tools, and simulation where correctness, clarity, and performance matter.

