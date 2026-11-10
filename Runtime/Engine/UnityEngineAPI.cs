// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

#if UNITY_5_3_OR_NEWER
using System;
using UnityEngine;
using XFG.Subsystems;

namespace XFG.Engine
{
    /// <summary>
    /// Unity-specific engine API using ScriptableObject.Instantiate when possible.
    /// </summary>
    public sealed class UnityEngineAPI : IEngineAPI
    {
        /// <summary>
        /// Writes a framework log message to the Unity console.
        /// </summary>
        /// <param name="category">The subsystem area the message belongs to.</param>
        /// <param name="level">The severity of the message.</param>
        /// <param name="message">The message text.</param>
        public void Log(LogCategory category, LogLevel level, string message)
        {
            switch (level)
            {
                case LogLevel.Info: Debug.Log($"[{category}] {message}"); break;
                case LogLevel.Warning: Debug.LogWarning($"[{category}] {message}"); break;
                case LogLevel.Error: Debug.LogError($"[{category}] {message}"); break;
            }
        }

        /// <summary>
        /// Creates a runtime subsystem instance from the given asset.
        /// </summary>
        /// <param name="asset">The subsystem asset to instantiate.</param>
        /// <returns>The new subsystem instance.</returns>
        public object InstantiateAsset(ISubsystemAsset asset)
        {
            if (asset is ScriptableObject so)
                return ScriptableObject.Instantiate(so);

            return Activator.CreateInstance(asset.GetType());
        }
    }
}
#endif
