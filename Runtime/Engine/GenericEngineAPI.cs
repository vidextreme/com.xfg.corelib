// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

using System;
using XFG.Subsystems;

namespace XFG.Engine
{
    /// <summary>
    /// Generic fallback engine API using Activator for instantiation.
    /// </summary>
    public sealed class GenericEngineAPI : IEngineAPI
    {
        /// <summary>
        /// Writes a framework log message to the console.
        /// </summary>
        /// <param name="category">The subsystem area the message belongs to.</param>
        /// <param name="level">The severity of the message.</param>
        /// <param name="message">The message text.</param>
        public void Log(LogCategory category, LogLevel level, string message)
        {
            Console.WriteLine($"{level} [{category}] {message}");
        }

        /// <summary>
        /// Creates a runtime subsystem instance from the given asset.
        /// </summary>
        /// <param name="asset">The subsystem asset to instantiate.</param>
        /// <returns>The new subsystem instance.</returns>
        public object InstantiateAsset(ISubsystemAsset asset)
        {
            return Activator.CreateInstance(asset.GetType());
        }
    }
}
