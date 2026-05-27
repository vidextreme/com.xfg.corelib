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
        /// Sorts subsystem assets using StartupOrderAttribute.
        /// Duplicate final orders are not allowed.
        /// </summary>
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
        /// Calls InjectAsset() if the instance implements IRequireAsset.
        /// </summary>
        public static Dictionary<Type, ISubsystemInstance> Instantiate(IEnumerable<ISubsystemAsset> assets)
        {
            var map = new Dictionary<Type, ISubsystemInstance>();

            foreach (var asset in assets)
            {
                if (!asset.Enabled)
                    continue;

                // 1. Create the instance
                var instance = asset.CreateInstance();
                var type = instance.GetType();

                // 2. Inject the asset if required
                if (instance is IRequireAsset ra)
                    ra.InjectAsset(asset);

                // 3. Store by instance type
                map[type] = instance;
            }

            return map;
        }

        /// <summary>
        /// Injects dependencies into all subsystem instances that require them.
        /// </summary>
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
        /// Runs initialization lifecycle for all subsystem instances.
        /// </summary>
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
        /// Runs deinitialization lifecycle for all subsystem instances.
        /// </summary>
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
