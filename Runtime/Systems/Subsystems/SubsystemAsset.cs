// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

#if UNITY_5_3_OR_NEWER
using UnityEngine;
#elif GODOT
using Godot;
#endif

namespace XFG.Subsystems
{
    /// <summary>
    /// Generic base class for subsystem assets.
    /// </summary>
    /// <remarks>
    /// Backed by a ScriptableObject in Unity, a Resource in Godot, and a POCO in pure C#.
    /// </remarks>
    /// <typeparam name="TInstance">The runtime subsystem instance type this asset creates.</typeparam>
    public abstract class SubsystemAsset<TInstance> : SubsystemAssetBase
        where TInstance : class, ISubsystemInstance, new()
    {
#if UNITY_5_3_OR_NEWER
        /// <summary>
        /// Human-readable display name. In Unity this is the asset name and is read-only.
        /// </summary>
        public override string DisplayName { get => name; set { } }
#else
        /// <summary>
        /// Human-readable display name for this asset.
        /// </summary>
#if GODOT
        [Export]
#endif
        public override string DisplayName { get; set; } = "";
#endif

        /// <summary>
        /// Whether this subsystem should be instantiated. Defaults to true.
        /// </summary>
        public override bool Enabled { get; set; } = true;

        /// <summary>
        /// Whether this subsystem can be paused. Defaults to true.
        /// </summary>
        public override bool IsPausable { get; set; } = true;

        /// <summary>
        /// Creates a new <typeparamref name="TInstance"/> instance.
        /// </summary>
        /// <returns>The new subsystem instance.</returns>
        public override ISubsystemInstance CreateInstance()
        {
            return new TInstance();
        }
    }


    /// <summary>
    /// Non-generic base class so Unity can serialize subsystem assets.
    /// </summary>
    public abstract class SubsystemAssetBase :
#if UNITY_5_3_OR_NEWER
                    ScriptableObject,
#elif GODOT
        Godot.Resource,
#endif
                    ISubsystemAsset
    {
        /// <summary>
        /// Human-readable display name for this asset.
        /// </summary>
        public abstract string DisplayName { get; set; }

        /// <summary>
        /// Whether this subsystem should be instantiated.
        /// </summary>
        public abstract bool Enabled { get; set; }

        /// <summary>
        /// Whether this subsystem can be paused.
        /// </summary>
        public abstract bool IsPausable { get; set; }

        /// <summary>
        /// Creates a new runtime instance of this subsystem.
        /// </summary>
        /// <returns>The new subsystem instance.</returns>
        public abstract ISubsystemInstance CreateInstance();
    }
}
