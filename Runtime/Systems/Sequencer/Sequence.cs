// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

using System;
using System.Collections.Generic;

namespace XFG.Sequencer
{
    /// <summary>
    /// Represents a running sequence of steps on a track.
    /// </summary>
    /// <remarks>
    /// A Sequence is an ordered collection of steps running on a track. It manages
    /// step lifecycle transitions and ensures deterministic teardown. Ending steps
    /// transition to Ended during tick, and are disposed during <see cref="CleanupSteps"/>.
    /// </remarks>
    public sealed class Sequence
    {
        #region Fields

        /// <summary>
        /// The identifier of the track this sequence is running on.
        /// </summary>
        public readonly int TrackID;

        /// <summary>
        /// The list of steps that belong to this sequence.
        /// </summary>
        public readonly List<IStepLogic> Steps = new();

        // The queued callbacks associated with this sequence
        //public readonly Queue<ActionStepContext> CallbackLineup = new();

        /// <summary>
        /// The owning sequencer instance.
        /// </summary>
        public Sequencer Sequencer;

        /// <summary>
        /// Callback invoked when the sequence finishes.
        /// </summary>
        public Action OnFinished;

        /// <summary>
        /// Gets a value indicating whether this sequence is muted.
        /// </summary>
        public bool Muted { get; private set; }

        private readonly List<int> _pendingCleanup = new();

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new sequence for the given track.
        /// </summary>
        /// <param name="sequencer">The owning sequencer instance.</param>
        /// <param name="trackID">The identifier of the track this sequence runs on.</param>
        public Sequence(Sequencer sequencer, int trackID)
        {
            Sequencer = sequencer;
            TrackID = trackID;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether all steps have been removed.
        /// </summary>
        public bool IsDone => Steps.Count == 0;

        #endregion

        #region Lifecycle

        /// <summary>
        /// Updates all steps in this sequence.
        /// </summary>
        /// <param name="deltaTime">Time elapsed since the last update.</param>
        public void Tick(float deltaTime)
        {
            static void Process(IStepLogic step, float dt) => step.OnTick(dt);
            OnTick(Process, deltaTime);
        }

        /// <summary>
        /// Performs a late update on all steps in this sequence.
        /// </summary>
        /// <param name="deltaTime">Time elapsed since the last late update.</param>
        public void LateTick(float deltaTime)
        {
            static void Process(IStepLogic step, float dt) => step.OnLateTick(dt);
            OnTick(Process, deltaTime);
        }

        /// <summary>
        /// Requests that all steps in this sequence begin ending.
        /// </summary>
        public void Stop()
        {
            for (int i = 0; i < Steps.Count; i++)
                Steps[i].TryEnd();
        }

        /// <summary>
        /// Disposes and removes all steps that have been marked for cleanup.
        /// </summary>
        public void CleanupSteps()
        {
            var steps = Steps;

            for (int i = _pendingCleanup.Count - 1; i >= 0; i--)
            {
                int index = _pendingCleanup[i];
                if (index >= steps.Count)
                    continue;

                var step = steps[index];

                step.Dispose();
                step.State = StepState.Released;

                steps.RemoveAt(index);
            }

            _pendingCleanup.Clear();
        }

        #endregion

        #region Messaging

        /// <summary>
        /// Sends a message payload to all active steps in this sequence.
        /// </summary>
        /// <typeparam name="T">Type of the payload.</typeparam>
        /// <param name="payload">The message payload.</param>
        public void SendMessage<T>(T payload)
        {
            for (int i = 0; i < Steps.Count; i++)
            {
                var step = Steps[i];
                var state = step.State;

                if (state == StepState.Running || state == StepState.AllowNext)
                    step.OnReceiveMessage(payload);
            }
        }

        #endregion

        #region Control

        /// <summary>
        /// Pops and executes queued actions for this sequence.
        /// </summary>
        /// <param name="allActions">If true, pops every queued action; otherwise, pops only one.</param>
        /// <returns>True if at least one action was popped; otherwise, false.</returns>
        public bool PopAction(bool allActions = false)
        {
            bool poppedAny = false;

            //while (CallbackLineup.TryDequeue(out var action))
            //{
            //    poppedAny = true;
            //    action.Callback();
            //    if (!allActions) break;
            //}

            return poppedAny;
        }

        /// <summary>
        /// Mutes this sequence and notifies all steps.
        /// </summary>
        public void Mute()
        {
            if (Muted) return;
            Muted = true;

            for (int i = 0; i < Steps.Count; i++)
                Steps[i].OnMute();
        }

        /// <summary>
        /// Unmutes this sequence and notifies all steps.
        /// </summary>
        public void Unmute()
        {
            if (!Muted) return;
            Muted = false;

            for (int i = 0; i < Steps.Count; i++)
                Steps[i].OnUnmute();
        }

        #endregion

        #region Internal

        private void OnTick(Action<IStepLogic, float> tick, float deltaTime)
        {
            var steps = Steps;
            int count = steps.Count;

            _pendingCleanup.Clear();

            for (int i = 0; i < count; i++)
            {
                // A step callback may have cleared the list (e.g. a forced stop)
                if (steps.Count < count)
                {
                    _pendingCleanup.Clear();
                    return;
                }

                var step = steps[i];
                var state = step.State;

                if (state == StepState.Running)
                {
                    tick(step, deltaTime);
                    if (step.PlayType != StepPlayType.Concurrent)
                        return;
                    continue;
                }

                if (state == StepState.AllowNext)
                {
                    tick(step, deltaTime);
                    continue;
                }

                switch (state)
                {
                    case StepState.NotStarted:
                        step.OnStart();
                        if (step.PlayType != StepPlayType.Concurrent)
                            return;
                        break;

                    case StepState.Ending:
                        tick(step, deltaTime);
                        step.OnEnd();
                        step.State = StepState.Ended;
                        _pendingCleanup.Add(i);
                        break;

                    case StepState.Ended:
                    case StepState.Released:
                        break;
                }
            }
        }

        #endregion
    }
}
