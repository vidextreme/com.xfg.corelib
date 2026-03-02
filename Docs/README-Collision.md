# 💥 Collision Module — XFG Simple Game Core Library

The Collision Module provides deterministic, allocation-free geometric queries for gameplay, physics proxies, character controllers, visibility checks, and debugging tools. It is built on a segment-centric and SAT-driven architecture, ensuring engine-grade correctness, clarity, and extensibility. All collision functions are implemented as extension methods and integrate cleanly with ShapeDebug for SceneView visualization.

---

## ✨ Features

- Complete set of convex geometric primitives:
  - Capsule, Sphere, Cylinder, Cone, Triangle, Segment, Ray, Plane, AABB, OBB, Frustum
- Deterministic math with no hidden state
- Zero-allocation implementations suitable for Burst
- Static and swept intersection tests
- Closest-point queries for all shapes
- Penetration depth and contact normal generation
- Extension-method API for fluent usage
- Modular partial-class architecture
- Full ShapeDebug visualization support

---

## 📐 Shapes

### Capsule
- Represented as a segment with radius
- Closest-point, intersection, and sweep tests
- Works with triangle, segment, sphere, cylinder, and plane queries

### Sphere
- Center and radius representation
- Fast distance and intersection queries
- Supports point, segment, ray, sphere, capsule, and cylinder tests

### Cylinder
- Axis, radius, and height representation
- Supports point, segment, ray, sphere, and capsule (broadphase) tests
- Includes finite-cylinder cap and side-wall logic

### Cone
- Apex, base center, radius, and height representation
- Supports point, segment, sphere, capsule (broadphase), and frustum tests

### Triangle
- Barycentric utilities
- Edge and normal calculations
- Closest-point queries

### Segment
- Distance and closest-point utilities
- Segment-segment intersection

### AABB and OBB
- Overlap tests
- Point containment
- Raycast support

### Frustum
- Plane extraction
- Point, box, sphere, capsule, cylinder, cone, triangle, and OBB visibility tests
- Frustum vs Frustum support

---

## 🎯 Core Queries

### Closest-Point Queries
- Point to Sphere
- Point to Cylinder
- Point to Segment
- Point to Triangle
- Point to Capsule
- Segment to Segment
- Sphere to Sphere
- Cylinder to Sphere
- Capsule to Cylinder (broadphase)

### Intersection Tests
- Sphere vs Sphere
- Sphere vs Segment
- Sphere vs Ray
- Cylinder vs Sphere
- Cylinder vs Segment
- Cylinder vs Ray
- Capsule vs Sphere
- Capsule vs Triangle
- Capsule vs Segment
- AABB vs AABB
- OBB vs OBB
- Ray vs AABB / OBB / Triangle / Sphere / Cylinder
- Frustum vs all primitives
- Frustum vs Frustum

### Sweep Tests
- Sphere sweep vs Sphere
- Sphere sweep vs Segment
- Sphere sweep vs Cylinder
- Capsule sweep vs Cylinder (broadphase)
- Capsule sweep vs Triangle
- Capsule sweep vs Segment
- Segment sweep vs Segment

### Penetration Depth
- Sphere vs Sphere
- Sphere vs Cylinder
- Cylinder vs Capsule (broadphase)
- Capsule vs Triangle contact manifold
- Capsule vs Segment resolution vector

---

## 🧮 Collision Matrix

| Shape     | AABB | Sphere | Capsule | Cylinder | Cone | Segment | Triangle | OBB | Frustum |
|-----------|------|--------|---------|----------|------|---------|----------|-----|----------|
| AABB      | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| Sphere    | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| Capsule   | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| Cylinder  | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| Cone      | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| Segment   | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| Triangle  | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| OBB       | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| Frustum   | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |

---

## 🧪 Usage Examples

### Capsule vs Sphere
```csharp
Capsule capsule = new Capsule(p0, p1, radius);
Sphere sphere = new Sphere(center, sphereRadius);

bool hit = capsule.Intersects(sphere);
if (hit)
{
    // Handle capsule-sphere intersection
}
```

### Capsule vs Ray
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
```