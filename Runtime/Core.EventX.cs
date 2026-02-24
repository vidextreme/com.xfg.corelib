// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

/// <summary>
/// Core.EventX — A zero-allocation, thread-safe, strongly-typed event system
/// for com.xfg.corelib.
///
/// This module provides a lightweight, engine-agnostic event dispatcher that
/// avoids reflection, avoids params arrays, avoids DynamicInvoke, and avoids
/// per-broadcast allocations. It supports any KeyType (string, enum, struct,
/// EventId, etc.) and any delegate signature (Action, Action<T1>, Action<T1,T2>, ...).
///
/// Key goals:
/// - Zero allocations during broadcast
/// - Thread-safe via ConcurrentDictionary
/// - Strongly typed payloads (no object[], no boxing)
/// - No reflection or DynamicInvoke
/// - Minimal API surface, easy onboarding
/// - Suitable for gameplay systems, tools, and Burst-friendly bridges
///
/// This is the foundational event system used throughout com.xfg.corelib.
/// </summary>

using System;
using System.Collections.Concurrent;

namespace XFG
{
    public static partial class Core
    {
        // =====================================================================
        // Internal Handler List
        // =====================================================================

        /// <summary>
        /// Internal zero-allocation handler list for a specific delegate type.
        /// Stores a multicast delegate and invokes it without allocations.
        /// </summary>
        sealed class HandlerList<TDelegate> where TDelegate : Delegate
        {
            private TDelegate? _handlers;

            /// <summary>
            /// Adds a handler to the list.
            /// </summary>
            public void Add(TDelegate handler)
            {
                _handlers = (TDelegate?)Delegate.Combine(_handlers, handler);
            }

            /// <summary>
            /// Removes a handler from the list.
            /// </summary>
            public void Remove(TDelegate handler)
            {
                _handlers = (TDelegate?)Delegate.Remove(_handlers, handler);
            }

            /// <summary>
            /// Invokes all handlers using a provided invoker function.
            /// This avoids reflection and allocations.
            /// </summary>
            public void Invoke(Action<TDelegate> invoker)
            {
                var h = _handlers;
                if (h != null)
                {
                    foreach (TDelegate d in h.GetInvocationList())
                        invoker((TDelegate)d);
                }
            }

            /// <summary>
            /// True if no handlers remain.
            /// </summary>
            public bool IsEmpty => _handlers == null;

            /// <summary>
            /// Number of handlers currently registered.
            /// </summary>
            public int Count => _handlers?.GetInvocationList().Length ?? 0;
        }

        // =====================================================================
        // Event System Core
        // =====================================================================

        /// <summary>
        /// Generic event system keyed by KeyType and delegate type.
        /// Thread-safe and zero-allocation during broadcast.
        /// </summary>
        sealed class EventSystem<KeyType, TDelegate> where TDelegate : Delegate
        {
            private readonly ConcurrentDictionary<KeyType, HandlerList<TDelegate>> _events =
                new ConcurrentDictionary<KeyType, HandlerList<TDelegate>>();

            /// <summary>
            /// Subscribes a handler to the specified event key.
            /// </summary>
            public void Subscribe(KeyType key, TDelegate handler)
            {
                var list = _events.GetOrAdd(key, _ => new HandlerList<TDelegate>());
                list.Add(handler);
            }

            /// <summary>
            /// Unsubscribes a handler from the specified event key.
            /// Removes the event entry if no handlers remain.
            /// </summary>
            public void Unsubscribe(KeyType key, TDelegate handler)
            {
                if (_events.TryGetValue(key, out var list))
                {
                    list.Remove(handler);
                    if (list.IsEmpty)
                        _events.TryRemove(key, out _);
                }
            }

            /// <summary>
            /// Broadcasts an event using a typed invoker.
            /// </summary>
            public void Broadcast(KeyType key, Action<TDelegate> invoker)
            {
                if (_events.TryGetValue(key, out var list))
                    list.Invoke(invoker);
            }

            /// <summary>
            /// Removes all handlers for a specific event key.
            /// </summary>
            public void ClearEvent(KeyType key)
            {
                _events.TryRemove(key, out _);
            }

            /// <summary>
            /// Removes all events and handlers.
            /// </summary>
            public void ClearAll()
            {
                _events.Clear();
            }

            /// <summary>
            /// Returns true if an event with the specified key exists.
            /// </summary>
            public bool HasEvent(KeyType key)
            {
                return _events.ContainsKey(key);
            }

            /// <summary>
            /// Returns the number of handlers registered to the event.
            /// </summary>
            public int Count(KeyType key)
            {
                return _events.TryGetValue(key, out var list) ? list.Count : 0;
            }
        }

        // =====================================================================
        // EventX Variants (0–4 parameters)
        // =====================================================================

        /// <summary>
        /// Zero-allocation event system for events with no parameters.
        /// </summary>
        public static class EventX<KeyType>
        {
            private static readonly EventSystem<KeyType, Action> _sys = new();

            public static void Subscribe(KeyType key, Action handler) =>
                _sys.Subscribe(key, handler);

            public static void Unsubscribe(KeyType key, Action handler) =>
                _sys.Unsubscribe(key, handler);

            public static void Broadcast(KeyType key) =>
                _sys.Broadcast(key, h => h());

            public static void ClearEvent(KeyType key) => _sys.ClearEvent(key);
            public static void ClearAll() => _sys.ClearAll();
            public static bool HasEvent(KeyType key) => _sys.HasEvent(key);
            public static int Count(KeyType key) => _sys.Count(key);
        }

        /// <summary>
        /// Zero-allocation event system for events with one parameter.
        /// </summary>
        public static class EventX<KeyType, T1>
        {
            private static readonly EventSystem<KeyType, Action<T1>> _sys = new();

            public static void Subscribe(KeyType key, Action<T1> handler) =>
                _sys.Subscribe(key, handler);

            public static void Unsubscribe(KeyType key, Action<T1> handler) =>
                _sys.Unsubscribe(key, handler);

            public static void Broadcast(KeyType key, T1 v1) =>
                _sys.Broadcast(key, h => h(v1));

            public static void ClearEvent(KeyType key) => _sys.ClearEvent(key);
            public static void ClearAll() => _sys.ClearAll();
            public static bool HasEvent(KeyType key) => _sys.HasEvent(key);
            public static int Count(KeyType key) => _sys.Count(key);
        }

        /// <summary>
        /// Zero-allocation event system for events with two parameters.
        /// </summary>
        public static class EventX<KeyType, T1, T2>
        {
            private static readonly EventSystem<KeyType, Action<T1, T2>> _sys = new();

            public static void Subscribe(KeyType key, Action<T1, T2> handler) =>
                _sys.Subscribe(key, handler);

            public static void Unsubscribe(KeyType key, Action<T1, T2> handler) =>
                _sys.Unsubscribe(key, handler);

            public static void Broadcast(KeyType key, T1 v1, T2 v2) =>
                _sys.Broadcast(key, h => h(v1, v2));

            public static void ClearEvent(KeyType key) => _sys.ClearEvent(key);
            public static void ClearAll() => _sys.ClearAll();
            public static bool HasEvent(KeyType key) => _sys.HasEvent(key);
            public static int Count(KeyType key) => _sys.Count(key);
        }

        /// <summary>
        /// Zero-allocation event system for events with three parameters.
        /// </summary>
        public static class EventX<KeyType, T1, T2, T3>
        {
            private static readonly EventSystem<KeyType, Action<T1, T2, T3>> _sys = new();

            public static void Subscribe(KeyType key, Action<T1, T2, T3> handler) =>
                _sys.Subscribe(key, handler);

            public static void Unsubscribe(KeyType key, Action<T1, T2, T3> handler) =>
                _sys.Unsubscribe(key, handler);

            public static void Broadcast(KeyType key, T1 v1, T2 v2, T3 v3) =>
                _sys.Broadcast(key, h => h(v1, v2, v3));

            public static void ClearEvent(KeyType key) => _sys.ClearEvent(key);
            public static void ClearAll() => _sys.ClearAll();
            public static bool HasEvent(KeyType key) => _sys.HasEvent(key);
            public static int Count(KeyType key) => _sys.Count(key);
        }

        /// <summary>
        /// Zero-allocation event system for events with four parameters.
        /// </summary>
        public static class EventX<KeyType, T1, T2, T3, T4>
        {
            private static readonly EventSystem<KeyType, Action<T1, T2, T3, T4>> _sys = new();

            public static void Subscribe(KeyType key, Action<T1, T2, T3, T4> handler) =>
                _sys.Subscribe(key, handler);

            public static void Unsubscribe(KeyType key, Action<T1, T2, T3, T4> handler) =>
                _sys.Unsubscribe(key, handler);

            public static void Broadcast(KeyType key, T1 v1, T2 v2, T3 v3, T4 v4) =>
                _sys.Broadcast(key, h => h(v1, v2, v3, v4));

            public static void ClearEvent(KeyType key) => _sys.ClearEvent(key);
            public static void ClearAll() => _sys.ClearAll();
            public static bool HasEvent(KeyType key) => _sys.HasEvent(key);
            public static int Count(KeyType key) => _sys.Count(key);
        }
    }
}
