// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

using System;
using System.Collections.Generic;
using System.Reflection;
using XFG.Subsystems;

namespace XFG
{
    /// <summary>
    /// Lightweight static helper for sorting, instantiating, injecting,
    /// and initializing subsystem instances.
    /// </summary>
    internal static class SubsystemManager
    {
        /// <summary>
        /// Sorts subsystem assets using <see cref="StartupOrderAttribute"/>.
        /// Duplicate final orders are not allowed.
        /// </summary>
        /// <typeparam name="T">The subsystem asset type.</typeparam>
        /// <param name="assets">The assets to sort.</param>
        /// <returns>A new list of assets in ascending startup order.</returns>
        public static List<T> Sort<T>(IEnumerable<T> assets) where T : ISubsystemAsset
        {
            var list = new List<T>(assets);
            list.Sort(CompareByStartupOrder);
            return list;
        }

        private static int CompareByStartupOrder<T>(T a, T b) where T : ISubsystemAsset
        {
            var ta = a.GetType();
            var tb = b.GetType();

            var oa = ta.GetCustomAttribute<StartupOrderAttribute>();
            var ob = tb.GetCustomAttribute<StartupOrderAttribute>();

            if (oa == null)
                throw new InvalidOperationException(
                    $"Subsystem asset '{a.DisplayName}' ({ta.FullName}) is missing StartupOrderAttribute.");

            if (ob == null)
                throw new InvalidOperationException(
                    $"Subsystem asset '{b.DisplayName}' ({tb.FullName}) is missing StartupOrderAttribute.");

            int orderA = oa.Order;
            int orderB = ob.Order;

            int cmp = orderA.CompareTo(orderB);
            if (cmp != 0)
                return cmp;

            throw new InvalidOperationException(
                $"Duplicate startup order {orderA} for '{a.DisplayName}' and '{b.DisplayName}'.");
        }

        /// <summary>
        /// Instantiates subsystem instances from assets.
        /// Calls <c>InjectAsset()</c> if the instance implements <see cref="IRequireAsset"/>.
        /// Disabled assets are skipped.
        /// </summary>
        /// <param name="assets">The assets to instantiate.</param>
        /// <returns>The created instances, keyed by instance type.</returns>
        public static Dictionary<Type, ISubsystemInstance> Instantiate(IEnumerable<ISubsystemAsset> assets)
        {
            var map = new Dictionary<Type, ISubsystemInstance>();

            foreach (var asset in assets)
            {
                if (!asset.Enabled)
                    continue;

                // Create the instance
                var instance = asset.CreateInstance();
                var type = instance.GetType();

                // Inject the asset if required
                if (instance is IRequireAsset ra)
                    ra.InjectAsset(asset);

                // Store by instance type
                map[type] = instance;
            }

            return map;
        }

        /// <summary>
        /// Injects dependencies into all subsystem instances that require them.
        /// </summary>
        /// <param name="engine">Engine-level subsystem instances.</param>
        /// <param name="group">Group-level subsystem instances.</param>
        /// <param name="world">World-level subsystem instances.</param>
        public static void InjectDependencies(
            IReadOnlyDictionary<Type, ISubsystemInstance> engine,
            IReadOnlyDictionary<Type, ISubsystemInstance> group,
            IReadOnlyDictionary<Type, ISubsystemInstance> world)
        {
            var ctx = new DependencyContext(engine, group, world);

            void InjectAll(IReadOnlyDictionary<Type, ISubsystemInstance> map)
            {
                foreach (var instance in map.Values)
                {
                    if (instance is IRequireDependencies req)
                        req.InjectDependencies(ctx);
                }
            }

            InjectAll(engine);
            InjectAll(group);
            InjectAll(world);
        }

        /// <summary>
        /// Runs the initialization lifecycle for all subsystem instances,
        /// in engine, group, then world order.
        /// </summary>
        /// <param name="engine">Engine-level subsystem instances.</param>
        /// <param name="group">Group-level subsystem instances.</param>
        /// <param name="world">World-level subsystem instances.</param>
        public static void InitializeAll(
            IReadOnlyDictionary<Type, ISubsystemInstance> engine,
            IReadOnlyDictionary<Type, ISubsystemInstance> group,
            IReadOnlyDictionary<Type, ISubsystemInstance> world)
        {
            void Run(IReadOnlyDictionary<Type, ISubsystemInstance> map)
            {
                foreach (var instance in map.Values)
                {
                    instance.OnBeforeInitialize();
                    instance.Initialize();
                    instance.OnAfterInitialize();
                }
            }

            Run(engine);
            Run(group);
            Run(world);
        }

        /// <summary>
        /// Runs the deinitialization lifecycle for all subsystem instances,
        /// in engine, group, then world order.
        /// </summary>
        /// <param name="engine">Engine-level subsystem instances.</param>
        /// <param name="group">Group-level subsystem instances.</param>
        /// <param name="world">World-level subsystem instances.</param>
        public static void DeinitializeAll(
            IReadOnlyDictionary<Type, ISubsystemInstance> engine,
            IReadOnlyDictionary<Type, ISubsystemInstance> group,
            IReadOnlyDictionary<Type, ISubsystemInstance> world)
        {
            void Run(IReadOnlyDictionary<Type, ISubsystemInstance> map)
            {
                foreach (var instance in map.Values)
                {
                    instance.OnBeforeDeinitialize();
                    instance.Deinitialize();
                    instance.OnAfterDeinitialize();
                }
            }

            Run(engine);
            Run(group);
            Run(world);
        }
    }
}
