// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

using XFG.Subsystems;

namespace XFG.Engine
{
    /// <summary>
    /// Engine abstraction layer responsible for instantiating subsystem assets.
    /// Each engine determines how assets are cloned or constructed.
    /// </summary>
    public interface IEngineAPI
    {
        /// <summary>
        /// Creates a runtime subsystem instance from the given asset.
        /// </summary>
        /// <param name="asset">The subsystem asset to instantiate.</param>
        /// <returns>The new subsystem instance.</returns>
        object InstantiateAsset(ISubsystemAsset asset);

        /// <summary>
        /// Writes a framework log message to the engine's log output.
        /// Called by <see cref="Core"/> for the currently installed engine API only.
        /// </summary>
        /// <param name="category">The subsystem area the message belongs to.</param>
        /// <param name="level">The severity of the message.</param>
        /// <param name="message">The message text.</param>
        void Log(LogCategory category, LogLevel level, string message);
    }
}
