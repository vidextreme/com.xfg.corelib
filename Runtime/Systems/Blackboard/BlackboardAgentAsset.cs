// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

#if UNITY_5_3_OR_NEWER
using UnityEngine;
#elif GODOT
using Godot;
#endif

using System;
using System.Collections.Generic;

namespace XFG.Blackboard
{
    /// <summary>
    /// Interface for agent-level blackboard assets.
    /// Provides a factory method for creating runtime <see cref="BlackboardAgent{TValue}"/> instances.
    /// </summary>
    public interface IBlackboardAgentAsset<TValue>
    {
        /// <summary>
        /// Human-readable display name for this asset.
        /// </summary>
        string DisplayName { get; set; }

        /// <summary>
        /// Creates a fully initialized runtime agent blackboard.
        /// </summary>
        /// <returns>The new agent blackboard.</returns>
        BlackboardAgent<TValue> CreateInstance();
    }

    /// <summary>
    /// Base class for agent-level blackboard assets.
    /// Creates runtime <see cref="BlackboardAgent{TValue}"/> instances, applies initial values,
    /// and registers knowledge sources via <c>ConfigureSources()</c>.
    /// </summary>
    /// <remarks>
    /// Backed by a ScriptableObject in Unity, a Resource in Godot, and a POCO in pure C#.
    /// Responsible for:
    /// <list type="bullet">
    /// <item>Defining initial blackboard values</item>
    /// <item>Creating runtime <see cref="BlackboardAgent{TValue}"/> instances</item>
    /// <item>Registering knowledge sources</item>
    /// </list>
    /// </remarks>
    public abstract class BlackboardAgentAsset<TValue> :
#if UNITY_5_3_OR_NEWER
        ScriptableObject,
#elif GODOT
        Resource,
#endif
        IBlackboardAgentAsset<TValue>
    {
#if UNITY_5_3_OR_NEWER
        /// <summary>
        /// Human-readable display name. In Unity this is the asset name and is read-only.
        /// </summary>
        public string DisplayName { get => name; set { } }
#else
        /// <summary>
        /// Human-readable display name for this asset.
        /// </summary>
#if GODOT
        [Export]
#endif
        public string DisplayName { get; set; } = "";
#endif

        /// <summary>
        /// Authoring-time initialization data for this agent.
        /// </summary>
        public BlackboardAgentData<TValue> Data = new BlackboardAgentData<TValue>();

        /// <summary>
        /// Creates a fully initialized runtime agent blackboard.
        /// Applies initial values and registers knowledge sources.
        /// </summary>
        /// <returns>The new agent blackboard.</returns>
        public BlackboardAgent<TValue> CreateInstance()
        {
            var agent = BlackboardAgent<TValue>.Create();

            foreach (var entry in Data.Entries)
                agent.Set(entry.Key, entry.Value);

            ConfigureSources(agent);

            return agent;
        }

        /// <summary>
        /// Subclasses override this to register knowledge sources.
        /// Called automatically during <c>CreateInstance()</c>.
        /// </summary>
        /// <param name="agent">The newly created agent to register sources on.</param>
        protected abstract void ConfigureSources(BlackboardAgent<TValue> agent);
    }
}
