// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

namespace XFG.Subsystems
{
    /// <summary>
    /// Base interface for all subsystem assets.
    /// </summary>
    public interface ISubsystemAsset
    {
        /// <summary>
        /// Human-readable display name for this asset.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Whether this subsystem should be instantiated. Disabled assets are skipped.
        /// </summary>
        bool Enabled { get; }

        /// <summary>
        /// Whether this subsystem can be paused.
        /// </summary>
        bool IsPausable { get; }

        /// <summary>
        /// Creates a new runtime instance of this subsystem.
        /// </summary>
        /// <returns>The new subsystem instance.</returns>
        ISubsystemInstance CreateInstance();
    }

    /// <summary>
    /// Marks a subsystem asset as engine-level (one instance per engine).
    /// </summary>
    public interface IEngineSubsystemAsset : ISubsystemAsset { }

    /// <summary>
    /// Marks a subsystem asset as group-level (one instance per world group).
    /// </summary>
    public interface IGroupSubsystemAsset : ISubsystemAsset { }

    /// <summary>
    /// Marks a subsystem asset as world-level (one instance per world).
    /// </summary>
    public interface IWorldSubsystemAsset : ISubsystemAsset { }
}
