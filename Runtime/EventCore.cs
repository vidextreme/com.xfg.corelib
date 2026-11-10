// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

using System;
using System.Collections.Generic;
namespace XFG
{
    public static partial class Core
    {
        /// <summary>
        /// Handler signature for string-keyed game events.
        /// </summary>
        /// <param name="eventName">The name of the event being raised.</param>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="eventArgs">The event arguments.</param>
        public delegate void GameEvent(string eventName, object sender, object[] eventArgs);

        private static readonly Dictionary<string, GameEvent> _eventBag = new();
        /// <summary>
        /// Subscribes a handler to the named event.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        /// <param name="gameEvent">The handler to subscribe.</param>
        public static void SubscribeEvent(string eventName, GameEvent gameEvent)
        {
            if (_eventBag.TryGetValue(eventName, out var existing))
            {
                existing += gameEvent;
                _eventBag[eventName] = existing;
            }
            else
            {
                _eventBag[eventName] = gameEvent;
            }

        }
        /// <summary>
        /// Unsubscribes a handler from the named event, removing the event when no handlers remain.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        /// <param name="gameEvent">The handler to unsubscribe.</param>
        public static void UnsubscribeEvent(string eventName, GameEvent gameEvent)
        {
            if (_eventBag.TryGetValue(eventName, out var existing))
            {
                existing -= gameEvent;
                if (existing == null)
                {
                    _eventBag.Remove(eventName);
                }
                else
                {
                    _eventBag[eventName] = existing;
                }
            }
        }
        /// <summary>
        /// Invokes every handler of the named event; exceptions from a handler are caught and logged in the editor.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        /// <param name="sender">The object raising the event.</param>
        /// <param name="eventArgs">The event arguments.</param>
        public static void BroadcastEvent(string eventName, object sender, params object[] eventArgs)
        {
            if (_eventBag.TryGetValue(eventName, out var existing))
            {
                foreach (var handler in existing.GetInvocationList())
                {
                    try { handler.DynamicInvoke(eventName, sender, eventArgs); }
                    catch (Exception ex)
                    {
#if UNITY_EDITOR
                        UnityEngine.Debug.LogError($"[Core] Exception in event '{eventName}' handler: {ex}");
#endif
                    }
                }
            }
        }
        /// <summary>
        /// Removes all handlers for the named event.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        public static void ClearEvent(string eventName)
        {
            if (_eventBag.ContainsKey(eventName))
            {
                _eventBag.Remove(eventName);
            }
        }

        /// <summary>
        /// Removes all events and handlers.
        /// </summary>
        public static void ClearAllEvents()
        {
            _eventBag.Clear();
        }

        /// <summary>
        /// Checks whether the named event has any handlers.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        /// <returns><c>true</c> if the event exists; otherwise <c>false</c>.</returns>
        public static bool HasEvent(string eventName)
        {
            return _eventBag.ContainsKey(eventName);
        }

        /// <summary>
        /// Gets the number of handlers subscribed to the named event.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        /// <returns>The handler count, or 0 if the event does not exist.</returns>
        public static int GetEventHandlerCount(string eventName)
        {
            if (_eventBag.TryGetValue(eventName, out var existing))
            {
                return existing.GetInvocationList().Length;
            }
            return 0;
        }
    }
}