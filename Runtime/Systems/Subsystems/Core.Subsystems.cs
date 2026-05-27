// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

using System;
using System.Collections.Generic;
using XFG.Subsystems;
using XFG.Worlds;

namespace XFG
{
    public static partial class Core
    {
        private static SubsystemRegistry _registry;

        /// <summary>
        /// Engine-level subsystem instances (one instance per engine).
        /// Backed by a private mutable dictionary.
        /// </summary>
        private static readonly Dictionary<Type, ISubsystemInstance> _engineSubsystems =
            new Dictionary<Type, ISubsystemInstance>();

        /// <summary>
        /// Read-only view of engine-level subsystem instances.
        /// </summary>
        public static IReadOnlyDictionary<Type, ISubsystemInstance> EngineSubsystems =>
            _engineSubsystems;

        private static readonly List<WorldGroup> _worldGroups = new List<WorldGroup>();
        private static float _fixedAccumulator;

        /// <summary>
        /// Fixed timestep used for FixedTick, in seconds.
        /// </summary>
        public static float FixedDeltaTime { get; set; } = 1f / 60f;

        /// <summary>
        /// Declares which subsystem assets exist at each runtime scope.
        /// Must be assigned before InitializeEngine is called.
        /// </summary>
        public static SubsystemRegistry Registry
        {
            get => _registry;
            set => _registry = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// All active world groups.
        /// </summary>
        public static IReadOnlyList<WorldGroup> WorldGroups => _worldGroups;

        /// <summary>
        /// Initializes all engine-level subsystems using the current registry.
        /// </summary>
        public static void InitializeEngine()
        {
            if (_registry == null)
                throw new InvalidOperationException("Subsystem registry not set.");

            _engineSubsystems.Clear();

            var sorted = SubsystemManager.Sort(_registry.EngineSubsystems);
            var instances = SubsystemManager.Instantiate(sorted);

            foreach (var kvp in instances)
                _engineSubsystems[kvp.Key] = kvp.Value;

            SubsystemManager.InjectDependencies(_engineSubsystems, Empty, Empty);
            SubsystemManager.InitializeAll(_engineSubsystems, Empty, Empty);

            Info("Engine subsystems initialized.", LogCategory.Engine);
        }

        /// <summary>
        /// Creates a new world group and initializes all group-level subsystems.
        /// </summary>
        public static WorldGroup CreateWorldGroup()
        {
            var sorted = SubsystemManager.Sort(_registry.GroupSubsystems);
            var instances = SubsystemManager.Instantiate(sorted);

            SubsystemManager.InjectDependencies(_engineSubsystems, instances, Empty);
            SubsystemManager.InitializeAll(Empty, instances, Empty);

            var group = new WorldGroup(instances);
            _worldGroups.Add(group);

            Info($"WorldGroup created. Total: {_worldGroups.Count}", LogCategory.Group);
            return group;
        }

        /// <summary>
        /// Advances all tickable subsystems and world groups by the given delta time.
        /// Also steps fixed-timestep updates as needed.
        /// </summary>
        public static void Tick(float dt)
        {
            foreach (var instance in _engineSubsystems.Values)
                if (instance is ITickable t) t.Tick(dt);

            foreach (var group in _worldGroups)
                group.Tick(dt);

            _fixedAccumulator += dt;
            while (_fixedAccumulator >= FixedDeltaTime)
            {
                FixedTick(FixedDeltaTime);
                _fixedAccumulator -= FixedDeltaTime;
            }
        }

        /// <summary>
        /// Advances all fixed-tick subsystems and world groups by the given fixed delta time.
        /// </summary>
        private static void FixedTick(float fdt)
        {
            foreach (var instance in _engineSubsystems.Values)
                if (instance is IFixedTickable ft) ft.FixedTick(fdt);

            foreach (var group in _worldGroups)
                group.FixedTick(fdt);
        }

        /// <summary>
        /// Deinitializes all worlds, world groups, and engine-level subsystems.
        /// </summary>
        public static void DeinitializeAll()
        {
            foreach (var group in _worldGroups)
                group.Deinitialize();

            _worldGroups.Clear();

            SubsystemManager.DeinitializeAll(_engineSubsystems, Empty, Empty);
            _engineSubsystems.Clear();

            Info("All subsystems deinitialized.", LogCategory.Core);
        }

        private static readonly Dictionary<Type, ISubsystemInstance> Empty =
            new Dictionary<Type, ISubsystemInstance>();
    }
}
