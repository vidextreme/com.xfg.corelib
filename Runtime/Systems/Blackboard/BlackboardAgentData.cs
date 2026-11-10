// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

using System;
using System.Collections.Generic;

namespace XFG.Blackboard
{
    /// <summary>
    /// A single key/value pair used to initialize a <see cref="BlackboardAgent{TValue}"/>.
    /// This represents authoring-time configuration only. Runtime state is
    /// stored inside <see cref="BlackboardAgent{TValue}"/>, not in this asset.
    /// </summary>
    /// <remarks>
    /// Example: <c>Key = "Health"</c>, <c>Value = 100</c>.
    /// These entries are applied when <see cref="BlackboardAgentAsset{TValue}.CreateInstance"/>
    /// is called. This type contains no runtime logic; it is purely data used to
    /// initialize agents at spawn time.
    /// </remarks>
    [Serializable]
    public class BlackboardAgentEntry<TValue>
    {
        /// <summary>
        /// The blackboard key to initialize.
        /// Keys should be stable, descriptive strings such as
        /// <c>"Health"</c>, <c>"TargetPosition"</c>, or <c>"IsAlert"</c>.
        /// </summary>
        public string Key;

        /// <summary>
        /// The initial value for this key.
        /// This value is copied into the runtime <see cref="BlackboardAgent{TValue}"/>
        /// when the agent is created.
        /// </summary>
        public TValue Value;
    }

    /// <summary>
    /// A serializable container for a list of <see cref="BlackboardAgentEntry{TValue}"/>.
    /// This is the data block stored inside <see cref="BlackboardAgentAsset{TValue}"/>.
    /// </summary>
    /// <remarks>
    /// Example entries: <c>Health = 100</c>, <c>Aggression = 0.25</c>.
    /// This class exists to allow Unity to serialize a list of entries
    /// in a clean and inspector-friendly way.
    /// </remarks>
    [Serializable]
    public class BlackboardAgentData<TValue>
    {
        /// <summary>
        /// The list of initial key/value pairs for an agent blackboard.
        /// These values are applied to each new <see cref="BlackboardAgent{TValue}"/>
        /// created from a <see cref="BlackboardAgentAsset{TValue}"/>.
        /// </summary>
        public List<BlackboardAgentEntry<TValue>> Entries =
            new List<BlackboardAgentEntry<TValue>>();
    }
}
