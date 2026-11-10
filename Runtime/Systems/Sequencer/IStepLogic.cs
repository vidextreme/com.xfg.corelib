// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

using System;

namespace XFG.Sequencer
{
    /// <summary>
    /// Represents the current state of a step in the sequencer.
    /// </summary>
    public enum StepState
    {
        NotStarted,
        Running,
        Ending,
        AllowNext,
        Ended,
        Released
    }

    /// <summary>
    /// Controls how a step plays relative to other steps in the sequence.
    /// </summary>
    public enum StepPlayType
    {
        Sequential,
        Concurrent
    }

    /// <summary>
    /// Marker interface for the owner context passed to steps when a sequence starts.
    /// </summary>
    public interface IStepContext { }

    /// <summary>
    /// Defines the behavior and lifecycle of a single step in a sequence.
    /// </summary>
    /// <remarks>
    /// Steps represent atomic units of behavior that run inside a <see cref="Sequence"/>.
    /// Each step participates in a well-defined lifecycle:
    /// Start -&gt; Tick -&gt; Ending -&gt; Ended -&gt; Released.
    /// The <see cref="Sequencer"/> coordinates these steps across tracks with
    /// deterministic teardown.
    /// </remarks>
    public interface IStepLogic : IDisposable
    {
        /// <summary>
        /// Gets or sets the current state of the step.
        /// </summary>
        StepState State { get; set; }

        /// <summary>
        /// Gets how this step plays relative to other steps.
        /// </summary>
        StepPlayType PlayType { get; }

        /// <summary>
        /// Initializes the step before it is started.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Called when the step is first started.
        /// </summary>
        void OnStart();

        /// <summary>
        /// Called every update while the step is running.
        /// </summary>
        /// <param name="deltaTime">Time elapsed since the last update.</param>
        void OnTick(float deltaTime);

        /// <summary>
        /// Called every late update while the step is running.
        /// </summary>
        /// <param name="deltaTime">Time elapsed since the last late update.</param>
        void OnLateTick(float deltaTime);

        /// <summary>
        /// Called when the step is ending.
        /// </summary>
        void OnEnd();

        /// <summary>
        /// Requests that the step begin ending.
        /// </summary>
        void TryEnd();

        /// <summary>
        /// Called when the owning sequence is muted.
        /// </summary>
        void OnMute();

        /// <summary>
        /// Called when the owning sequence is unmuted.
        /// </summary>
        void OnUnmute();

        /// <summary>
        /// Receives a message payload while the step is active.
        /// </summary>
        /// <typeparam name="T">Type of the payload.</typeparam>
        /// <param name="payload">The message payload.</param>
        void OnReceiveMessage<T>(T payload);
    }
}
