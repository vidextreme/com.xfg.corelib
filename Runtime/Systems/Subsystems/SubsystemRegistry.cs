// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

using System.Collections.Generic;

namespace XFG.Subsystems
{
    /// <summary>
    /// Declares which subsystem assets exist at each runtime scope.
    /// Assets are engine-agnostic and instantiated through the engine API.
    /// </summary>
    public sealed class SubsystemRegistry
    {
        /// <summary>
        /// Engine-level subsystem assets (one instance per engine).
        /// </summary>
        public List<IEngineSubsystemAsset> EngineSubsystems { get; } =
            new List<IEngineSubsystemAsset>();

        /// <summary>
        /// Group-level subsystem assets (one instance per world group).
        /// </summary>
        public List<IGroupSubsystemAsset> GroupSubsystems { get; } =
            new List<IGroupSubsystemAsset>();

        /// <summary>
        /// World-level subsystem assets (one instance per world).
        /// </summary>
        public List<IWorldSubsystemAsset> WorldSubsystems { get; } =
            new List<IWorldSubsystemAsset>();
    }
}
