// Copyright (c) 2025 John David Uy
// Licensed under the MIT License. See LICENSE for details.

using System;
using System.Collections.Generic;
namespace XFG
{
    public static partial class Core
    {
        public delegate void GameEvent(string eventName, object sender, object[] eventArgs);

        private static readonly Dictionary<string, GameEvent> _eventBag = new();
        public static void SubscribeEvent(string eventName, GameEvent gameEvent)
        {
            if(_eventBag.TryGetValue(eventName, out var existing))
            {
                existing += gameEvent;
            }

            _eventBag[eventName] = gameEvent;
        }
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
        public static void ClearEvent(string eventName)
        {
            if (_eventBag.ContainsKey(eventName))
            {
                _eventBag.Remove(eventName);
            }
        }

        public static void ClearAllEvents()
        {
            _eventBag.Clear();
        }

        public static bool HasEvent(string eventName)
        {
            return _eventBag.ContainsKey(eventName);
        }

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