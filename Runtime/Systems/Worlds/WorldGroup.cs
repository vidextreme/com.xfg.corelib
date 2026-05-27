// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

using System;
using System.Collections.Generic;
using XFG.Subsystems;

namespace XFG.Worlds
{
    /// <summary>
    /// Represents a group of worlds that share a set of group-level subsystems.
    /// </summary>
    public sealed class WorldGroup
    {
        private readonly Dictionary<Type, ISubsystemInstance> _groupSubsystems;
        private readonly List<World> _worlds = new List<World>();
        private bool _paused;

        internal WorldGroup(Dictionary<Type, ISubsystemInstance> groupSubsystems)
        {
            _groupSubsystems = groupSubsystems;
        }

        /// <summary>
        /// Whether this world group is paused.
        /// </summary>
        public bool Paused
        {
            get => _paused;
            set => _paused = value;
        }

        /// <summary>
        /// All worlds belonging to this group.
        /// </summary>
        public IReadOnlyList<World> Worlds => _worlds;

        /// <summary>
        /// Group-level subsystem instances (one instance per world group).
        /// </summary>
        public IReadOnlyDictionary<Type, ISubsystemInstance> Subsystems => _groupSubsystems;

        /// <summary>
        /// Gets a group-level subsystem instance by its type.
        /// </summary>
        public T GetSubsystem<T>() where T : class =>
            _groupSubsystems.TryGetValue(typeof(T), out var v) ? (T)v : null;

        /// <summary>
        /// Creates a new world within this group and initializes all world-level subsystems.
        /// </summary>
        public World CreateWorld(SubsystemRegistry registry)
        {
            var sorted = SubsystemManager.Sort(registry.WorldSubsystems);
            var instances = SubsystemManager.Instantiate(sorted);

            SubsystemManager.InjectDependencies(Core.EngineSubsystems, _groupSubsystems, instances);
            SubsystemManager.InitializeAll(Empty, Empty, instances);

            var world = new World(instances);
            _worlds.Add(world);

            Core.Info($"World created. Total: {_worlds.Count}", LogCategory.World);
            return world;
        }

        internal void Tick(float dt)
        {
            if (_paused) return;

            foreach (var instance in _groupSubsystems.Values)
                if (instance is ITickable t) t.Tick(dt);

            foreach (var world in _worlds)
                world.Tick(dt);
        }

        internal void FixedTick(float fdt)
        {
            if (_paused) return;

            foreach (var instance in _groupSubsystems.Values)
                if (instance is IFixedTickable ft) ft.FixedTick(fdt);

            foreach (var world in _worlds)
                world.FixedTick(fdt);
        }

        internal void Deinitialize()
        {
            foreach (var world in _worlds)
                world.Deinitialize();

            _worlds.Clear();

            SubsystemManager.DeinitializeAll(Empty, _groupSubsystems, Empty);
            _groupSubsystems.Clear();
        }

        private static readonly Dictionary<Type, ISubsystemInstance> Empty =
            new Dictionary<Type, ISubsystemInstance>();
    }
}
