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

        bool Enabled { get; }
        bool IsPausable { get; }

        ISubsystemInstance CreateInstance();
    }

    public interface IEngineSubsystemAsset : ISubsystemAsset { }
    public interface IGroupSubsystemAsset : ISubsystemAsset { }
    public interface IWorldSubsystemAsset : ISubsystemAsset { }
}
