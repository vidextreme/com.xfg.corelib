// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

using System;
using UnityEngine;

namespace XFG.Sequencer
{
    /// <summary>
    /// Authoring-time data for a step.
    /// </summary>
    public interface IStepData
    {
        /// <summary>
        /// Gets how this step plays relative to other steps.
        /// </summary>
        StepPlayType PlayType { get; }

        /// <summary>
        /// Creates the runtime logic that plays this step.
        /// </summary>
        /// <param name="sequence">The sequence the step will run in.</param>
        /// <param name="owner">The owner context passed to <see cref="Sequencer.StartSequence"/>.</param>
        /// <returns>A new, not-yet-initialized step logic instance; must not be null.</returns>
        IStepLogic CreateLogic(Sequence sequence, IStepContext owner);
    }

    /// <summary>
    /// Serializable step definition that wraps polymorphic <see cref="IStepData"/>.
    /// </summary>
    [Serializable]
    public sealed class Step
    {
        /// <summary>
        /// The step's data, serialized by reference so derived types are preserved.
        /// </summary>
        [SerializeReference]
        public IStepData Data;
    }
}
