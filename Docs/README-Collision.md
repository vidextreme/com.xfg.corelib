# 🧩 Collision Module — XFG Simple Game Core Library

The **Collision Module** provides deterministic, allocation‑free geometric queries for gameplay, physics proxies, character controllers, and debugging tools. It is designed for engine‑grade correctness, clarity, and extensibility, using an extension‑method architecture and full integration with `ShapeDebug` for SceneView visualization.

---

## ✨ Features

- Full suite of geometric primitives:
  - **Capsule, Sphere, Cylinder, Triangle, Segment, Ray, Plane, AABB, OBB, Frustum**
- Deterministic math (no randomness, no hidden state)
- Zero‑allocation implementations
- Static + swept intersection tests
- Closest‑point queries for all shapes
- Penetration depth + contact normal generation
- Extension‑method API for fluent usage
- Modular partial‑class architecture
- Full `ShapeDebug` visualization support

---

## 📐 Shapes

### Capsule
- Segment + radius representation  
- Closest‑point, intersection, and sweep tests  
- Works with triangle, segment, sphere, cylinder, and plane queries  

### Sphere
- Center + radius representation  
- Extremely fast distance and intersection queries  
- Supports:
  - Point → Sphere  
  - Segment → Sphere  
  - Ray → Sphere  
  - Sphere → Sphere  
  - Capsule → Sphere  
  - Cylinder → Sphere  

### Cylinder
- Axis + radius + height representation  
- Supports:
  - Point → Cylinder  
  - Segment → Cylinder  
  - Ray → Cylinder  
  - Sphere → Cylinder  
  - Capsule → Cylinder (broadphase)  
- Includes finite‑cylinder cap + side‑wall logic  

### Triangle
- Barycentric utilities  
- Edge/normal calculations  
- Closest‑point queries  

### Segment
- Distance and closest‑point utilities  
- Segment‑segment intersection  

### AABB / OBB
- Overlap tests  
- Point containment  
- Raycast support  

### Frustum
- Plane extraction  
- Point/box visibility tests  

---

## 🎯 Core Queries

### Closest‑Point Queries
- Point → Sphere  
- Point → Cylinder  
- Point → Segment  
- Point → Triangle  
- Point → Capsule  
- Segment → Segment  
- Sphere → Sphere  
- Cylinder → Sphere  
- Capsule → Cylinder (broadphase)  

### Intersection Tests
- Sphere ↔ Sphere  
- Sphere ↔ Segment  
- Sphere ↔ Ray  
- Cylinder ↔ Sphere  
- Cylinder ↔ Segment  
- Cylinder ↔ Ray  
- Capsule ↔ Sphere  
- Capsule ↔ Triangle  
- Capsule ↔ Segment  
- AABB ↔ AABB  
- OBB ↔ OBB  
- Ray ↔ AABB / OBB / Triangle / Sphere / Cylinder  

### Sweep Tests
- Sphere sweep vs. Sphere  
- Sphere sweep vs. Segment  
- Sphere sweep vs. Cylinder  
- Capsule sweep vs. Cylinder (broadphase)  
- Capsule sweep vs. Triangle  
- Capsule sweep vs. Segment  
- Segment sweep vs. Segment  

### Penetration Depth
- Sphere ↔ Sphere  
- Sphere ↔ Cylinder  
- Cylinder ↔ Capsule (broadphase)  
- Capsule ↔ Triangle contact manifold  
- Capsule ↔ Segment resolution vector  

---

## 🧪 Usage Examples

### Capsule → Sphere

```csharp
Capsule capsule = new Capsule(p0, p1, radius); 
Sphere sphere = new Sphere(center, sphereRadius);

//...

bool hit = capsule.Intersects(sphere);
if (hit)
{
    // Handle capsule–sphere intersection
}
```

### Capsule → Ray

```csharp
Capsule capsule = new Capsule(p0, p1, radius);
Vector3 rayOrigin = R0;
Vector3 rayDirection = Rd;

float t;
bool hitRay = capsule.IntersectsRay(rayOrigin, rayDirection, out t);
if (hitRay)
{
    // t is the ray distance to the first intersection point
}
```
---

## 🖥️ Debug Visualization

[ShapeDebug Readme](README-ShapeDebug.md)

```csharp
ShapeDebug.DrawCapsule(capsule);
ShapeDebug.DrawTriangle(triangle);
ShapeDebug.DrawCylinder(cylinder);
ShapeDebug.DrawSphere(sphere);

if (hit.intersecting)
{
    ShapeDebug.DrawPoint(hit.point, Color.red);
    ShapeDebug.DrawNormal(hit.point, hit.normal);
}


