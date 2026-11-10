// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

namespace XFG.Subsystems
{
    /// <summary>
    /// Base interface for all runtime subsystem instances.
    /// </summary>
    public interface ISubsystemInstance
    {
        /// <summary>
        /// Called before <see cref="Initialize"/>, after dependencies are injected.
        /// </summary>
        void OnBeforeInitialize();

        /// <summary>
        /// Initializes the subsystem.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Called after <see cref="Initialize"/>.
        /// </summary>
        void OnAfterInitialize();

        /// <summary>
        /// Called before <see cref="Deinitialize"/>.
        /// </summary>
        void OnBeforeDeinitialize();

        /// <summary>
        /// Deinitializes the subsystem and releases its resources.
        /// </summary>
        void Deinitialize();

        /// <summary>
        /// Called after <see cref="Deinitialize"/>.
        /// </summary>
        void OnAfterDeinitialize();
    }

    /// <summary>
    /// Optional capability: receives per-frame updates.
    /// </summary>
    public interface ITickable
    {
        /// <summary>
        /// Advances the subsystem by one frame.
        /// </summary>
        /// <param name="dt">Time elapsed since the last tick, in seconds.</param>
        void Tick(float dt);
    }

    /// <summary>
    /// Optional capability: receives fixed-timestep updates.
    /// </summary>
    public interface IFixedTickable
    {
        /// <summary>
        /// Advances the subsystem by one fixed timestep.
        /// </summary>
        /// <param name="fdt">The fixed timestep, in seconds.</param>
        void FixedTick(float fdt);
    }

    /// <summary>
    /// Optional capability: subsystem requires dependency injection.
    /// </summary>
    public interface IRequireDependencies
    {
        /// <summary>
        /// Receives resolved subsystem instances before initialization.
        /// </summary>
        /// <param name="context">Provides access to engine, group, and world subsystems.</param>
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
        /// <param name="asset">The asset this instance was created from.</param>
        void InjectAsset(ISubsystemAsset asset);
    }
}
