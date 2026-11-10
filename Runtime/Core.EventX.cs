// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

using System;
using System.Collections.Concurrent;

namespace XFG
{
    public static partial class Core
    {
        // ---------------------------------------------------------------------
        // Internal Handler List
        // ---------------------------------------------------------------------
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
            /// <param name="handler">The handler to add.</param>
            public void Add(TDelegate handler)
            {
                _handlers = (TDelegate?)Delegate.Combine(_handlers, handler);
            }

            /// <summary>
            /// Removes a handler from the list.
            /// </summary>
            /// <param name="handler">The handler to remove.</param>
            public void Remove(TDelegate handler)
            {
                _handlers = (TDelegate?)Delegate.Remove(_handlers, handler);
            }

            /// <summary>
            /// Invokes all handlers using a provided invoker function.
            /// This avoids reflection and allocations.
            /// </summary>
            /// <param name="invoker">Callback that invokes a single handler with the event payload.</param>
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

        // ---------------------------------------------------------------------
        // Event System Core
        // ---------------------------------------------------------------------
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
            /// <param name="key">The event key.</param>
            /// <param name="handler">The handler to subscribe.</param>
            public void Subscribe(KeyType key, TDelegate handler)
            {
                var list = _events.GetOrAdd(key, _ => new HandlerList<TDelegate>());
                list.Add(handler);
            }

            /// <summary>
            /// Unsubscribes a handler from the specified event key.
            /// Removes the event entry if no handlers remain.
            /// </summary>
            /// <param name="key">The event key.</param>
            /// <param name="handler">The handler to unsubscribe.</param>
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
            /// <param name="key">The event key.</param>
            /// <param name="invoker">Callback that invokes a single handler with the event payload.</param>
            public void Broadcast(KeyType key, Action<TDelegate> invoker)
            {
                if (_events.TryGetValue(key, out var list))
                    list.Invoke(invoker);
            }

            /// <summary>
            /// Removes all handlers for a specific event key.
            /// </summary>
            /// <param name="key">The event key.</param>
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
            /// Checks whether an event with the specified key exists.
            /// </summary>
            /// <param name="key">The event key.</param>
            /// <returns><c>true</c> if the event exists; otherwise <c>false</c>.</returns>
            public bool HasEvent(KeyType key)
            {
                return _events.ContainsKey(key);
            }

            /// <summary>
            /// Gets the number of handlers registered to the event.
            /// </summary>
            /// <param name="key">The event key.</param>
            /// <returns>The handler count, or 0 if the event does not exist.</returns>
            public int Count(KeyType key)
            {
                return _events.TryGetValue(key, out var list) ? list.Count : 0;
            }
        }

        // ---------------------------------------------------------------------
        // EventX Variants (0–4 Parameters)
        // ---------------------------------------------------------------------
        /// <summary>
        /// Zero-allocation event system for events with no parameters.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <c>EventX</c> family is a zero-allocation, thread-safe, strongly-typed
        /// event system and the foundational event system used throughout com.xfg.corelib.
        /// It is a lightweight, engine-agnostic dispatcher that avoids reflection,
        /// <c>params</c> arrays, <c>DynamicInvoke</c>, and per-broadcast allocations.
        /// It supports any key type (string, enum, struct, EventId, etc.) and any
        /// delegate signature (<c>Action</c>, <c>Action{T1}</c>, <c>Action{T1, T2}</c>, ...).
        /// </para>
        /// <para>Key goals:</para>
        /// <list type="bullet">
        /// <item><description>Zero allocations during broadcast.</description></item>
        /// <item><description>Thread-safe via <c>ConcurrentDictionary</c>.</description></item>
        /// <item><description>Strongly typed payloads (no <c>object[]</c>, no boxing).</description></item>
        /// <item><description>No reflection or <c>DynamicInvoke</c>.</description></item>
        /// <item><description>Minimal API surface and easy onboarding.</description></item>
        /// <item><description>Suitable for gameplay systems, tools, and Burst-friendly bridges.</description></item>
        /// </list>
        /// </remarks>
        public static class EventX<KeyType>
        {
            private static readonly EventSystem<KeyType, Action> _sys = new();

            /// <summary>
            /// Subscribes a handler to the specified event key.
            /// </summary>
            /// <param name="key">The event key.</param>
            /// <param name="handler">The handler to subscribe.</param>
            public static void Subscribe(KeyType key, Action handler) =>
                _sys.Subscribe(key, handler);

            /// <summary>
            /// Unsubscribes a handler from the specified event key.
            /// </summary>
            /// <param name="key">The event key.</param>
            /// <param name="handler">The handler to unsubscribe.</param>
            public static void Unsubscribe(KeyType key, Action handler) =>
                _sys.Unsubscribe(key, handler);

            /// <summary>
            /// Invokes every handler subscribed to the specified event key.
            /// </summary>
            /// <param name="key">The event key.</param>
            public static void Broadcast(KeyType key) =>
                _sys.Broadcast(key, h => h());

            /// <summary>
            /// Removes all handlers for the specified event key.
            /// </summary>
            /// <param name="key">The event key.</param>
            public static void ClearEvent(KeyType key) => _sys.ClearEvent(key);

            /// <summary>
            /// Removes all events and handlers for this signature.
            /// </summary>
            public static void ClearAll() => _sys.ClearAll();

            /// <summary>
            /// Checks whether an event with the specified key exists.
            /// </summary>
            /// <param name="key">The event key.</param>
            /// <returns><c>true</c> if the event exists; otherwise <c>false</c>.</returns>
            public static bool HasEvent(KeyType key) => _sys.HasEvent(key);

            /// <summary>
            /// Gets the number of handlers registered to the event.
            /// </summary>
            /// <param name="key">The event key.</param>
            /// <returns>The handler count, or 0 if the event does not exist.</returns>
            public static int Count(KeyType key) => _sys.Count(key);
        }

        /// <summary>
        /// Zero-allocation event system for events with one parameter.
        /// </summary>
        public static class EventX<KeyType, T1>
        {
            private static readonly EventSystem<KeyType, Action<T1>> _sys = new();

            /// <summary>
            /// Subscribes a handler to the specified event key.
            /// </summary>
            /// <param name="key">The event key.</param>
            /// <param name="handler">The handler to subscribe.</param>
            public static void Subscribe(KeyType key, Action<T1> handler) =>
                _sys.Subscribe(key, handler);

            /// <summary>
            /// Unsubscribes a handler from the specified event key.
            /// </summary>
            /// <param name="key">The event key.</param>
            /// <param name="handler">The handler to unsubscribe.</param>
            public static void Unsubscribe(KeyType key, Action<T1> handler) =>
                _sys.Unsubscribe(key, handler);

            /// <summary>
            /// Invokes every handler subscribed to the specified event key.
            /// </summary>
            /// <param name="key">The event key.</param>
            /// <param name="v1">The first event argument.</param>
            public static void Broadcast(KeyType key, T1 v1) =>
                _sys.Broadcast(key, h => h(v1));

            /// <summary>
            /// Removes all handlers for the specified event key.
            /// </summary>
            /// <param name="key">The event key.</param>
            public static void ClearEvent(KeyType key) => _sys.ClearEvent(key);

            /// <summary>
            /// Removes all events and handlers for this signature.
            /// </summary>
            public static void ClearAll() => _sys.ClearAll();

            /// <summary>
            /// Checks whether an event with the specified key exists.
            /// </summary>
            /// <param name="key">The event key.</param>
            /// <returns><c>true</c> if the event exists; otherwise <c>false</c>.</returns>
            public static bool HasEvent(KeyType key) => _sys.HasEvent(key);

            /// <summary>
            /// Gets the number of handlers registered to the event.
            /// </summary>
            /// <param name="key">The event key.</param>
            /// <returns>The handler count, or 0 if the event does not exist.</returns>
            public static int Count(KeyType key) => _sys.Count(key);
        }

        /// <summary>
        /// Zero-allocation event system for events with two parameters.
        /// </summary>
        public static class EventX<KeyType, T1, T2>
        {
            private static readonly EventSystem<KeyType, Action<T1, T2>> _sys = new();

            /// <summary>
            /// Subscribes a handler to the specified event key.
            /// </summary>
            /// <param name="key">The event key.</param>
            /// <param name="handler">The handler to subscribe.</param>
            public static void Subscribe(KeyType key, Action<T1, T2> handler) =>
                _sys.Subscribe(key, handler);

            /// <summary>
            /// Unsubscribes a handler from the specified event key.
            /// </summary>
            /// <param name="key">The event key.</param>
            /// <param name="handler">The handler to unsubscribe.</param>
            public static void Unsubscribe(KeyType key, Action<T1, T2> handler) =>
                _sys.Unsubscribe(key, handler);

            /// <summary>
            /// Invokes every handler subscribed to the specified event key.
            /// </summary>
            /// <param name="key">The event key.</param>
            /// <param name="v1">The first event argument.</param>
            /// <param name="v2">The second event argument.</param>
            public static void Broadcast(KeyType key, T1 v1, T2 v2) =>
                _sys.Broadcast(key, h => h(v1, v2));

            /// <summary>
            /// Removes all handlers for the specified event key.
            /// </summary>
            /// <param name="key">The event key.</param>
            public static void ClearEvent(KeyType key) => _sys.ClearEvent(key);

            /// <summary>
            /// Removes all events and handlers for this signature.
            /// </summary>
            public static void ClearAll() => _sys.ClearAll();

            /// <summary>
            /// Checks whether an event with the specified key exists.
            /// </summary>
            /// <param name="key">The event key.</param>
            /// <returns><c>true</c> if the event exists; otherwise <c>false</c>.</returns>
            public static bool HasEvent(KeyType key) => _sys.HasEvent(key);

            /// <summary>
            /// Gets the number of handlers registered to the event.
            /// </summary>
            /// <param name="key">The event key.</param>
            /// <returns>The handler count, or 0 if the event does not exist.</returns>
            public static int Count(KeyType key) => _sys.Count(key);
        }

        /// <summary>
        /// Zero-allocation event system for events with three parameters.
        /// </summary>
        public static class EventX<KeyType, T1, T2, T3>
        {
            private static readonly EventSystem<KeyType, Action<T1, T2, T3>> _sys = new();

            /// <summary>
            /// Subscribes a handler to the specified event key.
            /// </summary>
            /// <param name="key">The event key.</param>
            /// <param name="handler">The handler to subscribe.</param>
            public static void Subscribe(KeyType key, Action<T1, T2, T3> handler) =>
                _sys.Subscribe(key, handler);

            /// <summary>
            /// Unsubscribes a handler from the specified event key.
            /// </summary>
            /// <param name="key">The event key.</param>
            /// <param name="handler">The handler to unsubscribe.</param>
            public static void Unsubscribe(KeyType key, Action<T1, T2, T3> handler) =>
                _sys.Unsubscribe(key, handler);

            /// <summary>
            /// Invokes every handler subscribed to the specified event key.
            /// </summary>
            /// <param name="key">The event key.</param>
            /// <param name="v1">The first event argument.</param>
            /// <param name="v2">The second event argument.</param>
            /// <param name="v3">The third event argument.</param>
            public static void Broadcast(KeyType key, T1 v1, T2 v2, T3 v3) =>
                _sys.Broadcast(key, h => h(v1, v2, v3));

            /// <summary>
            /// Removes all handlers for the specified event key.
            /// </summary>
            /// <param name="key">The event key.</param>
            public static void ClearEvent(KeyType key) => _sys.ClearEvent(key);

            /// <summary>
            /// Removes all events and handlers for this signature.
            /// </summary>
            public static void ClearAll() => _sys.ClearAll();

            /// <summary>
            /// Checks whether an event with the specified key exists.
            /// </summary>
            /// <param name="key">The event key.</param>
            /// <returns><c>true</c> if the event exists; otherwise <c>false</c>.</returns>
            public static bool HasEvent(KeyType key) => _sys.HasEvent(key);

            /// <summary>
            /// Gets the number of handlers registered to the event.
            /// </summary>
            /// <param name="key">The event key.</param>
            /// <returns>The handler count, or 0 if the event does not exist.</returns>
            public static int Count(KeyType key) => _sys.Count(key);
        }

        /// <summary>
        /// Zero-allocation event system for events with four parameters.
        /// </summary>
        public static class EventX<KeyType, T1, T2, T3, T4>
        {
            private static readonly EventSystem<KeyType, Action<T1, T2, T3, T4>> _sys = new();

            /// <summary>
            /// Subscribes a handler to the specified event key.
            /// </summary>
            /// <param name="key">The event key.</param>
            /// <param name="handler">The handler to subscribe.</param>
            public static void Subscribe(KeyType key, Action<T1, T2, T3, T4> handler) =>
                _sys.Subscribe(key, handler);

            /// <summary>
            /// Unsubscribes a handler from the specified event key.
            /// </summary>
            /// <param name="key">The event key.</param>
            /// <param name="handler">The handler to unsubscribe.</param>
            public static void Unsubscribe(KeyType key, Action<T1, T2, T3, T4> handler) =>
                _sys.Unsubscribe(key, handler);

            /// <summary>
            /// Invokes every handler subscribed to the specified event key.
            /// </summary>
            /// <param name="key">The event key.</param>
            /// <param name="v1">The first event argument.</param>
            /// <param name="v2">The second event argument.</param>
            /// <param name="v3">The third event argument.</param>
            /// <param name="v4">The fourth event argument.</param>
            public static void Broadcast(KeyType key, T1 v1, T2 v2, T3 v3, T4 v4) =>
                _sys.Broadcast(key, h => h(v1, v2, v3, v4));

            /// <summary>
            /// Removes all handlers for the specified event key.
            /// </summary>
            /// <param name="key">The event key.</param>
            public static void ClearEvent(KeyType key) => _sys.ClearEvent(key);

            /// <summary>
            /// Removes all events and handlers for this signature.
            /// </summary>
            public static void ClearAll() => _sys.ClearAll();

            /// <summary>
            /// Checks whether an event with the specified key exists.
            /// </summary>
            /// <param name="key">The event key.</param>
            /// <returns><c>true</c> if the event exists; otherwise <c>false</c>.</returns>
            public static bool HasEvent(KeyType key) => _sys.HasEvent(key);

            /// <summary>
            /// Gets the number of handlers registered to the event.
            /// </summary>
            /// <param name="key">The event key.</param>
            /// <returns>The handler count, or 0 if the event does not exist.</returns>
            public static int Count(KeyType key) => _sys.Count(key);
        }
    }
}
