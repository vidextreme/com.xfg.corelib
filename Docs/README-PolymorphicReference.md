# 🔌 PolymorphicReference

Part of the **XFG Simple Game Core Library**, `PolymorphicReference<T>` provides a clean, engine‑agnostic way to serialize references to **interfaces or abstract classes** in Unity and Godot.  
Unity cannot serialize interface fields, and Godot cannot expose them directly in the inspector — this wrapper solves both problems with a unified, minimal API.

---

## ❓ Why This Exists

Unity does not serialize interface fields:

    public IWeapon weapon; // Not serializable

Godot cannot expose interface or abstract references in the inspector either.

Developers often resort to:

- Concrete types  
- ScriptableObject indirection  
- Manual lookups  
- Custom inspectors  

`PolymorphicReference<T>` solves this by storing an engine object reference and resolving the contract type at runtime.

---

## ⚙️ How It Works

`PolymorphicReference<T>` stores an engine object reference:

- **Unity:** UnityEngine.Object  
- **Godot:** GodotObject  

At runtime, it resolves the reference to the contract type `T`:

- If `T` is an **interface**, the object must implement it  
- If `T` is an **abstract class**, the object must derive from it  
- Concrete types are **not allowed** as `T`  

### Resolution Rules

**Unity:**

1. If the stored object is a `GameObject`, it searches for a component implementing `T`  
2. If it’s a `Component` or `ScriptableObject`, it checks assignability  
3. The result is cached on first access to `Value`  

Setting `RawObject` clears the cache automatically. If the underlying `_object` field changes any other way (for example, edited in the Inspector while the game is running), call `InvalidateCache()` so the next `Value` read resolves again.

**Godot:**

1. If the stored object is already `T`, it is returned  
2. (Optional future extension) Script‑based interface checks  

---

## 🎮 Unity Integration

Unity includes a custom property drawer that provides:

- A standard ObjectField  
- A **...** button that opens the custom picker window  
- Validation (only objects implementing/deriving from `T` are accepted)  
- Automatic type display in the label, e.g. `Weapon (Interface IWeapon) -> LaserGun`  
- Multi-object editing (shows a mixed value when the selected objects differ)  

### Supported Assignment Methods

- Drag & drop  
- Object selector  
- Custom picker window  

Invalid assignments are rejected with a console warning, and the previous value is kept. Clearing the field (assigning `None`) is always allowed.

---

## ![Unity](https://img.shields.io/badge/-Unity-%23444444?logo=Unity) Unity Picker Window

The picker window is designed to feel native to Unity.

### Features

- **Tabs:**  
  - Assets  
  - Scene  
- **Search bar:**  
  - Toolbar‑style  
  - Cancel button  
- **Zoom slider:**  
  - Scales the rows  
  - Switches from a list to a grid of previews at higher zoom levels  
- **List / grid view:**  
  - RL‑style rows  
  - Icons via AssetPreview  
  - Hover highlight  
  - Click‑to‑select  
- **Current value:**  
  - The currently assigned object is highlighted and scrolled into view  
  - Opens on the Scene tab when the current value is a scene object  

### Filtering

Only objects implementing or deriving from the contract type `T` are shown.

---

## 🧩 Godot Integration

Godot support is **runtime‑only**:

- Uses `[Export]` to expose the reference  
- Stores a `GodotObject`  
- Resolves via `as T`  
- No custom inspector or picker window  

This keeps the runtime engine‑agnostic while avoiding unnecessary editor complexity.

---

## 📘 Usage Example

### Interface

    public interface IWeapon
    {
        void Fire();
    }

### Component

    public class LaserGun : MonoBehaviour, IWeapon
    {
        public void Fire() { }
    }

### Script using PolymorphicReference

    public class Player : MonoBehaviour
    {
        public PolymorphicReference<IWeapon> Weapon;

        private void Start()
        {
            Weapon.Value?.Fire();
        }
    }

In Unity:

- Drag a GameObject with an `IWeapon` component  
- Or click **...** to use the custom picker  

In Godot:

- Assign any Node or Resource implementing the contract  

---

## ⚠️ Limitations

- Only supports **interfaces** and **abstract classes** as `T`  
- Unity’s AssetPreview may return null for some asset types  
- Scene tab lists GameObjects from all loaded scenes (including inactive ones), but not prefab assets  
- `Value` is cached; call `InvalidateCache()` if the reference changes outside `RawObject`  
- Godot inspector does not include a picker UI  
