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
    /// Unity: ScriptableObject
    /// Godot: Resource
    /// Pure C#: POCO
    /// </summary>
    public abstract class SubsystemAsset<TInstance> : SubsystemAssetBase
        where TInstance : class, ISubsystemInstance, new()
    {
#if UNITY_5_3_OR_NEWER
        public override string DisplayName { get => name; set { } }
#else
#if GODOT
        [Export]
#endif
        public override string DisplayName { get; set; } = "";
#endif

        public override bool Enabled { get; set; } = true;

        public override bool IsPausable { get; set; } = true;

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
        public abstract string DisplayName { get; set; }
        public abstract bool Enabled { get; set; }
        public abstract bool IsPausable { get; set; }
        public abstract ISubsystemInstance CreateInstance();
    }
}
