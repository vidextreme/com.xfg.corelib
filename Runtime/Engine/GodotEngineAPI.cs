// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

#if GODOT
using System;
using Godot;
using XFG.Subsystems;

namespace XFG.Engine
{
    /// <summary>
    /// Godot-specific engine API using Resource.Duplicate when possible.
    /// </summary>
    public sealed class GodotEngineAPI : IEngineAPI
    {
        /// <summary>
        /// Writes a framework log message to the Godot log.
        /// </summary>
        /// <param name="category">The subsystem area the message belongs to.</param>
        /// <param name="level">The severity of the message.</param>
        /// <param name="message">The message text.</param>
        public void Log(LogCategory category, LogLevel level, string message)
        {
            switch (level)
            {
                case LogLevel.Info: GD.Print($"[{category}] {message}"); break;
                case LogLevel.Warning: GD.PushWarning($"[{category}] {message}"); break;
                case LogLevel.Error: GD.PushError($"[{category}] {message}"); break;
            }
        }

        /// <summary>
        /// Creates a runtime subsystem instance from the given asset.
        /// </summary>
        /// <param name="asset">The subsystem asset to instantiate.</param>
        /// <returns>The new subsystem instance.</returns>
        public object InstantiateAsset(ISubsystemAsset asset)
        {
            if (asset is Resource res)
                return res.Duplicate();

            return Activator.CreateInstance(asset.GetType());
        }
    }
}
#endif
