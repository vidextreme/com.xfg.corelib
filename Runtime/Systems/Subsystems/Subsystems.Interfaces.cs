// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

namespace XFG.Subsystems
{
    /// <summary>
    /// Base interface for all runtime subsystem instances.
    /// </summary>
    public interface ISubsystemInstance
    {
        void OnBeforeInitialize();
        void Initialize();
        void OnAfterInitialize();

        void OnBeforeDeinitialize();
        void Deinitialize();
        void OnAfterDeinitialize();
    }

    /// <summary>
    /// Optional capability: receives per-frame updates.
    /// </summary>
    public interface ITickable
    {
        void Tick(float dt);
    }

    /// <summary>
    /// Optional capability: receives fixed-timestep updates.
    /// </summary>
    public interface IFixedTickable
    {
        void FixedTick(float fdt);
    }

    /// <summary>
    /// Optional capability: subsystem requires dependency injection.
    /// </summary>
    public interface IRequireDependencies
    {
        void InjectDependencies(DependencyContext context);
    }

    /// <summary>
    /// Optional capability: subsystem instance wants its asset.
    /// The asset is injected during initialization and exposed
    /// as a read-only property for debugging and runtime tools.
    /// </summary>
    public interface IRequireAsset
    {
        /// <summary>
        /// The asset assigned to this subsystem instance.
        /// </summary>
        ISubsystemAsset Asset { get; }

        /// <summary>
        /// Injects the asset into the subsystem instance.
        /// Called automatically by the subsystem manager.
        /// </summary>
        void InjectAsset(ISubsystemAsset asset);
    }
}
