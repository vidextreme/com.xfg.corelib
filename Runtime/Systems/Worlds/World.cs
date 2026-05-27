// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

using System;
using System.Collections.Generic;
using XFG.Subsystems;

namespace XFG.Worlds
{
    /// <summary>
    /// Represents a single world instance containing world-level subsystems.
    /// </summary>
    public sealed class World
    {
        private readonly Dictionary<Type, ISubsystemInstance> _worldSubsystems;
        private bool _paused;

        internal World(Dictionary<Type, ISubsystemInstance> worldSubsystems)
        {
            _worldSubsystems = worldSubsystems;
        }

        /// <summary>
        /// Whether this world is paused.
        /// </summary>
        public bool Paused
        {
            get => _paused;
            set => _paused = value;
        }

        /// <summary>
        /// World-level subsystem instances (one instance per world).
        /// </summary>
        public IReadOnlyDictionary<Type, ISubsystemInstance> Subsystems =>
            _worldSubsystems;

        /// <summary>
        /// Gets a world-level subsystem instance by its type.
        /// </summary>
        public T GetSubsystem<T>() where T : class =>
            _worldSubsystems.TryGetValue(typeof(T), out var v) ? (T)v : null;

        /// <summary>
        /// Advances all tickable world-level subsystems by the given delta time.
        /// </summary>
        internal void Tick(float dt)
        {
            if (_paused) return;

            foreach (var instance in _worldSubsystems.Values)
                if (instance is ITickable t) t.Tick(dt);
        }

        /// <summary>
        /// Advances all fixed-tick world-level subsystems by the given fixed delta time.
        /// </summary>
        internal void FixedTick(float fdt)
        {
            if (_paused) return;

            foreach (var instance in _worldSubsystems.Values)
                if (instance is IFixedTickable ft) ft.FixedTick(fdt);
        }

        /// <summary>
        /// Deinitializes all world-level subsystems.
        /// </summary>
        internal void Deinitialize()
        {
            SubsystemManager.DeinitializeAll(Empty, Empty, _worldSubsystems);
            _worldSubsystems.Clear();
        }

        private static readonly Dictionary<Type, ISubsystemInstance> Empty =
            new Dictionary<Type, ISubsystemInstance>();
    }
}
