// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

using System;
using System.Collections.Generic;

namespace XFG.Subsystems
{
    /// <summary>
    /// Provides access to resolved subsystem instances during dependency injection.
    /// </summary>
    public sealed class DependencyContext
    {
        private readonly IReadOnlyDictionary<Type, ISubsystemInstance> _engine;
        private readonly IReadOnlyDictionary<Type, ISubsystemInstance> _group;
        private readonly IReadOnlyDictionary<Type, ISubsystemInstance> _world;

        /// <summary>
        /// Creates a context over the resolved subsystem instances at each scope.
        /// </summary>
        /// <param name="engine">Engine-level subsystem instances.</param>
        /// <param name="group">Group-level subsystem instances.</param>
        /// <param name="world">World-level subsystem instances.</param>
        public DependencyContext(
            IReadOnlyDictionary<Type, ISubsystemInstance> engine,
            IReadOnlyDictionary<Type, ISubsystemInstance> group,
            IReadOnlyDictionary<Type, ISubsystemInstance> world)
        {
            _engine = engine;
            _group = group;
            _world = world;
        }

        /// <summary>
        /// Gets an engine-level subsystem instance by type, or null if not found.
        /// </summary>
        /// <typeparam name="T">The subsystem instance type.</typeparam>
        /// <returns>The instance, or null if not found.</returns>
        public T GetEngine<T>() where T : class, ISubsystemInstance
        {
            return _engine.TryGetValue(typeof(T), out var value) ? (T)value : null;
        }

        /// <summary>
        /// Gets a group-level subsystem instance by type, or null if not found.
        /// </summary>
        /// <typeparam name="T">The subsystem instance type.</typeparam>
        /// <returns>The instance, or null if not found.</returns>
        public T GetGroup<T>() where T : class, ISubsystemInstance
        {
            return _group.TryGetValue(typeof(T), out var value) ? (T)value : null;
        }

        /// <summary>
        /// Gets a world-level subsystem instance by type, or null if not found.
        /// </summary>
        /// <typeparam name="T">The subsystem instance type.</typeparam>
        /// <returns>The instance, or null if not found.</returns>
        public T GetWorld<T>() where T : class, ISubsystemInstance
        {
            return _world.TryGetValue(typeof(T), out var value) ? (T)value : null;
        }
    }
}
