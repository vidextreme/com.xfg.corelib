# ShapeDebug — XFG Simple Game Core Library

ShapeDebug provides lightweight, editor-only visualization helpers for geometric shapes and shape math used across the XFG Simple Game Core Library.

- Project: `XFG Simple Game Core Library`
- Namespace: `XFG.Math.Shape`
- Class: `static partial class ShapeDebug` (implemented across multiple `ShapeDebug.*.cs` files)
- Editor-only: all draw helpers are guarded with `#if UNITY_EDITOR` and are stripped from player builds

Location
- `Runtime/Debug/` (files include `ShapeDebug.Bounds.cs`, `ShapeDebug.Cone.cs`, `ShapeDebug.Capsule.cs`, `ShapeDebug.Segment.cs`, `ShapeDebug.Triangle.cs`, `ShapeDebug.Frustrum.cs`, etc.)

Summary of helpers
- Bounds
  - `DrawAabb(Bounds bounds, Color color)`
  - `DrawObb(Vector3 center, Vector3 extents, Quaternion rotation, Color color)`
- Cone
  - `DrawCone(Vector3 apex, Vector3 axis, float height, float radius, Color color)`
- Capsule
  - `DrawCapsule(Vector3 p0, Vector3 p1, float radius, Color color)`
- Segment
  - `DrawSegment(Vector3 p0, Vector3 p1, Color color)`
  - `DrawSegmentClosestPoint(Vector3 p0, Vector3 p1, Vector3 q)`
- Triangle
  - `DrawTriangle(Vector3 a, Vector3 b, Vector3 c, Color color)`
  - `DrawTriangleNormal(Vector3 a, Vector3 b, Vector3 c, float scale, Color color)`
  - `DrawTriangleBarycentricAxes(Vector3 a, Vector3 b, Vector3 c, float scale)`
  - `DrawTriangleClosestPoint(Vector3 p, Vector3 a, Vector3 b, Vector3 c)`
- Frustum
  - `DrawFrustumPlanes(Plane[] planes, float scale)`

Related visualizers
- Runtime visualizers in `Runtime/Visualizations` show how to attach ShapeDebug drawings to GameObjects (AABBVisualization, OBBVisualization, CapsuleVisualization, CylinderVisualization, ConeVisualization, SegmentVisualization, FrustrumVisualization, etc.).

Usage notes
1. Add the namespace:

    ```csharp
    using XFG.Math.Shape;
    ```

2. Call helpers from `OnDrawGizmos`, `OnDrawGizmosSelected`, or an editor `OnSceneGUI`.

    Because the methods are editor-guarded you do not need to add `#if UNITY_EDITOR` at the call site.

Examples (C# syntax highlighted)

MonoBehaviour example (draw many debug shapes when selected):

```csharp
// Assets/Scripts/ShapeDebugExample.cs
using UnityEngine;
using XFG.Math.Shape;

public class ShapeDebugExample : MonoBehaviour
{
    void OnDrawGizmosSelected()
    {
        Vector3 pos = transform.position;

        // AABB
        ShapeDebug.DrawAabb(new Bounds(pos + Vector3.right * 1.0f, Vector3.one), Color.green);

        // OBB
        ShapeDebug.DrawObb(pos + Vector3.right * 2.5f, Vector3.one * 0.5f, transform.rotation, Color.cyan);

        // Cone
        ShapeDebug.DrawCone(pos + Vector3.up * 0.5f, transform.forward, 2f, 0.6f, Color.yellow);

        // Capsule
        ShapeDebug.DrawCapsule(pos + Vector3.up * 0.5f, pos - Vector3.up * 0.5f, 0.25f, Color.magenta);

        // Segment and closest point
        Vector3 a = pos + Vector3.left;
        Vector3 b = pos + Vector3.right;
        ShapeDebug.DrawSegment(a, b, Color.white);
        ShapeDebug.DrawSegmentClosestPoint(a, b, pos + transform.forward * 1f);

        // Triangle helpers
        Vector3 t0 = pos + Vector3.forward;
        Vector3 t1 = pos + Vector3.forward + Vector3.right;
        Vector3 t2 = pos + Vector3.forward + Vector3.up;
        ShapeDebug.DrawTriangle(t0, t1, t2, Color.blue);
        ShapeDebug.DrawTriangleNormal(t0, t1, t2, 0.5f, Color.red);
        ShapeDebug.DrawTriangleBarycentricAxes(t0, t1, t2, 0.3f);
        ShapeDebug.DrawTriangleClosestPoint(pos, t0, t1, t2);
    }
}
```

Editor Scene drawer example (place in `Assets/Editor`):

```csharp
// Assets/Editor/ShapeDebugSceneDrawer.cs
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using XFG.Math.Shape;

[CustomEditor(typeof(Transform))]
public class ShapeDebugSceneDrawer : Editor
{
    void OnSceneGUI()
    {
        var t = (Transform)target;
        ShapeDebug.DrawCone(t.position, t.forward, 1.5f, 0.35f, Color.yellow);
    }
}
#endif
```

Troubleshooting
- Nothing visible? Ensure you are using the correct hook and that the object is selected when using `OnDrawGizmosSelected`.
- Editor scripts must be in an `Editor` folder to compile into the editor assembly for `OnSceneGUI`.

Extending ShapeDebug
- Create `ShapeDebug.MyShape.cs` and add methods guarded with `#if UNITY_EDITOR`.

Want a ready-to-drop sample scene or demo prefab showcasing every helper? I can generate that and add example editor tooling.
