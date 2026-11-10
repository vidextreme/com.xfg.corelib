// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

using System;

#if UNITY_ENGINE || UNITY_5_3_OR_NEWER
using UnityEngine;
#endif

#if GODOT
using Godot;
#endif

namespace XFG.Utilities
{
    /// <summary>
    /// A serialized reference to a polymorphic contract type.
    /// T must be an interface or abstract class.
    /// The underlying engine object is stored and resolved to T at runtime.
    /// </summary>
    /// <typeparam name="T">
    /// The contract type. Must be an interface or abstract class.
    /// </typeparam>
    [Serializable]
    public sealed class PolymorphicReference<T> where T : class
    {
#if UNITY_ENGINE || UNITY_5_3_OR_NEWER
        [UnityEngine.SerializeField]
        private UnityEngine.Object _object;
#endif

#if GODOT
        [Export]
        private GodotObject _object;
#endif

        [NonSerialized]
        private T _cachedValue;

        [NonSerialized]
        private bool _isCached;

        static PolymorphicReference()
        {
            Type t = typeof(T);
            if (!t.IsInterface && !t.IsAbstract)
            {
#if UNITY_ENGINE || UNITY_5_3_OR_NEWER
                Debug.LogError(
                    $"PolymorphicReference<{t.Name}> requires T to be an interface or abstract class."
                );
#elif GODOT
                GD.PushError(
                    $"PolymorphicReference<{t.Name}> requires T to be an interface or abstract class."
                );
#endif
            }
        }

        /// <summary>
        /// The resolved value of type T, or null if the reference is invalid.
        /// </summary>
        public T Value
        {
            get
            {
                if (_isCached)
                    return _cachedValue;

                _cachedValue = Resolve();
                _isCached = true;
                return _cachedValue;
            }
        }

        /// <summary>
        /// The raw engine object backing this reference.
        /// </summary>
        public object RawObject
        {
            get => _object;
            set
            {
                if (ReferenceEquals(_object, value))
                    return;

#if UNITY_ENGINE || UNITY_5_3_OR_NEWER
                _object = value as UnityEngine.Object;
#elif GODOT
                _object = value as GodotObject;
#endif
                InvalidateCache();
            }
        }

        /// <summary>
        /// Clears the cached value so it will be resolved again on next access.
        /// </summary>
        public void InvalidateCache()
        {
            _cachedValue = null;
            _isCached = false;
        }

        /// <summary>
        /// Forces the cached value to be resolved again; used by editor tooling after assignments.
        /// </summary>
        public void EditorRefresh()
        {
            InvalidateCache();
        }

        private T Resolve()
        {
            if (_object == null)
                return null;

#if UNITY_ENGINE || UNITY_5_3_OR_NEWER
            return ResolveUnity();
#elif GODOT
            return ResolveGodot();
#else
            return null;
#endif
        }

#if UNITY_ENGINE || UNITY_5_3_OR_NEWER
        private T ResolveUnity()
        {
            // If the object is a GameObject, search for a component implementing T
            GameObject go = _object as GameObject;
            if (go != null)
            {
                Component comp = go.GetComponent(typeof(T));
                if (comp != null)
                    return comp as T;

                return null;
            }

            // If the object is a Component or ScriptableObject, check assignability
            if (typeof(T).IsAssignableFrom(_object.GetType()))
                return _object as T;

            return null;
        }
#endif

#if GODOT
        private T ResolveGodot()
        {
            // GodotObject is the base for Nodes, Resources, etc.
            // If the object is already T, just cast
            if (_object is T t)
                return t;

            // If you later add script-based interface checks, they can go here
            return null;
        }
#endif
    }
}
