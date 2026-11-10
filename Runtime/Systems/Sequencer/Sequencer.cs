// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

using System;
using System.Collections.Generic;

namespace XFG.Sequencer
{
    /// <summary>
    /// Manages tracks and sequences, updating and coordinating their execution.
    /// </summary>
    /// <remarks>
    /// The Sequencer updates sequences, coordinates track muting rules, and ensures
    /// deterministic cleanup. Universal tracks always run; Stack tracks behave like
    /// a priority stack.
    /// </remarks>
    public sealed class Sequencer
    {
        #region Fields

        private delegate void TickDelegate(Sequence sequence, float deltaTime);

        private readonly struct TrackRecord
        {
            public readonly SequenceTrack Public;
            public readonly int BucketIndex;

            public TrackRecord(SequenceTrack track, int bucketIndex)
            {
                Public = track;
                BucketIndex = bucketIndex;
            }
        }

        private readonly List<TrackRecord> _tracks = new();
        private readonly List<List<Sequence>> _universalTracks = new();
        private readonly List<List<Sequence>> _stackTracks = new();

        private readonly int _defaultTrackCapacity;

        private static readonly TickDelegate sProcessTick = (seq, dt) => seq.Tick(dt);
        private static readonly TickDelegate sProcessLateTick = (seq, dt) => seq.LateTick(dt);

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new sequencer instance.
        /// </summary>
        /// <param name="defaultTrackCapacity">The default capacity for track sequence lists.</param>
        public Sequencer(int defaultTrackCapacity = 4)
        {
            _defaultTrackCapacity = defaultTrackCapacity;
        }

        #endregion

        #region Track Management

        /// <summary>
        /// Registers a new track with the given name and mode.
        /// </summary>
        /// <param name="name">The display name of the track.</param>
        /// <param name="mode">The behavior mode of the track.</param>
        /// <returns>The created track metadata.</returns>
        public SequenceTrack RegisterTrack(string name, SequenceTrackMode mode)
        {
            int id = _tracks.Count;
            var track = new SequenceTrack(id, name, mode);

            int bucketIndex;
            if (mode == SequenceTrackMode.Universal)
            {
                bucketIndex = _universalTracks.Count;
                _universalTracks.Add(new List<Sequence>(_defaultTrackCapacity));
            }
            else
            {
                bucketIndex = _stackTracks.Count;
                _stackTracks.Add(new List<Sequence>(_defaultTrackCapacity));
            }

            _tracks.Add(new TrackRecord(track, bucketIndex));
            return track;
        }

        /// <summary>
        /// Gets the track metadata for the given track identifier.
        /// </summary>
        /// <param name="trackID">The track identifier.</param>
        /// <returns>The track metadata.</returns>
        public SequenceTrack GetTrack(int trackID) => _tracks[trackID].Public;

        private List<Sequence> GetTrackBucket(int trackID)
        {
            var record = _tracks[trackID];
            return record.Public.Mode == SequenceTrackMode.Universal
                ? _universalTracks[record.BucketIndex]
                : _stackTracks[record.BucketIndex];
        }

        /// <summary>
        /// Determines whether the specified track is currently active.
        /// </summary>
        /// <param name="trackID">The track identifier.</param>
        /// <returns>True if the track is active; otherwise, false.</returns>
        public bool IsTrackActive(int trackID)
        {
            var record = _tracks[trackID];
            var meta = record.Public;

            if (meta.Mode == SequenceTrackMode.Universal)
                return true;

            int activeStack = GetActiveStackIndex();
            if (activeStack < 0)
                return false;

            return record.BucketIndex == activeStack;
        }

        private int GetActiveStackIndex()
        {
            for (int i = _stackTracks.Count - 1; i >= 0; i--)
            {
                if (_stackTracks[i].Count > 0)
                    return i;
            }
            return -1;
        }

        private void UpdateTrackStates()
        {
            int activeStack = GetActiveStackIndex();

            for (int i = 0; i < _stackTracks.Count; i++)
            {
                var list = _stackTracks[i];
                bool shouldMute = activeStack >= 0 && i < activeStack;

                for (int s = 0; s < list.Count; s++)
                {
                    var seq = list[s];
                    if (shouldMute) seq.Mute();
                    else seq.Unmute();
                }
            }
        }

        #endregion

        #region Sequence Creation

        /// <summary>
        /// Starts a new sequence on the specified track.
        /// </summary>
        /// <param name="trackID">The track identifier.</param>
        /// <param name="steps">The steps to play in the sequence.</param>
        /// <param name="owner">The owner context for the sequence.</param>
        /// <returns>The created sequence.</returns>
        public Sequence StartSequence(
            int trackID,
            IEnumerable<Step> steps,
            IStepContext owner)
        {
            return StartSequenceFiltered(trackID, null, steps, owner);
        }

        /// <summary>
        /// Starts a new sequence on the specified track, filtering which steps are included.
        /// </summary>
        /// <param name="trackID">The track identifier.</param>
        /// <param name="filter">The filter used to determine which steps to include.</param>
        /// <param name="steps">The steps to consider for the sequence.</param>
        /// <param name="owner">The owner context for the sequence.</param>
        /// <returns>The created sequence.</returns>
        public Sequence StartSequenceFiltered(
            int trackID,
            Func<Step, bool> filter,
            IEnumerable<Step> steps,
            IStepContext owner)
        {
            var sequence = new Sequence(this, trackID);
            var bucket = GetTrackBucket(trackID);

            if (steps != null)
            {
                foreach (var step in steps)
                {
                    // Skip empty slots (e.g. unassigned entries in an inspector list)
                    if (step?.Data == null)
                        continue;

                    if (filter != null && !filter(step))
                        continue;

                    IStepLogic logic = step.Data.CreateLogic(sequence, owner);
                    if (logic == null)
                        throw new InvalidOperationException(
                            $"{step.Data.GetType().Name}.CreateLogic returned null.");

                    sequence.Steps.Add(logic);
                    logic.Initialize();
                }
            }

            // Register only after all steps are built, so a failure leaves no partial sequence
            bucket.Add(sequence);

            //var actionCtx = contextMap.Get<ActionStepContext>();
            //if (actionCtx != null)
            //    sequence.CallbackLineup.Enqueue(actionCtx);

            UpdateTrackStates();
            return sequence;
        }

        #endregion

        #region Lifecycle

        /// <summary>
        /// Updates all sequences managed by this sequencer.
        /// </summary>
        /// <param name="deltaTime">Time elapsed since the last update.</param>
        public void Update(float deltaTime) => OnTick(sProcessTick, deltaTime);

        /// <summary>
        /// Performs a late update on all sequences managed by this sequencer.
        /// </summary>
        /// <param name="deltaTime">Time elapsed since the last late update.</param>
        public void LateUpdate(float deltaTime) => OnTick(sProcessLateTick, deltaTime);

        /// <summary>
        /// Stops all sequences, optionally forcing immediate teardown.
        /// </summary>
        /// <param name="forceStop">
        /// If true, immediately ends and disposes all steps; otherwise, requests graceful ending.
        /// </param>
        public void StopAllSequences(bool forceStop = false)
        {
            if (forceStop)
            {
                void ForceStop(List<List<Sequence>> lists)
                {
                    for (int t = 0; t < lists.Count; t++)
                    {
                        var sequences = lists[t];
                        for (int i = 0; i < sequences.Count; i++)
                        {
                            var seq = sequences[i];
                            var steps = seq.Steps;
                            for (int s = 0; s < steps.Count; s++)
                            {
                                var step = steps[s];
                                step.OnEnd();
                                step.Dispose();
                            }
                            steps.Clear();
                        }
                        sequences.Clear();
                    }
                }

                ForceStop(_universalTracks);
                ForceStop(_stackTracks);
                return;
            }

            void SoftStop(List<List<Sequence>> lists)
            {
                for (int t = 0; t < lists.Count; t++)
                {
                    var sequences = lists[t];
                    for (int i = 0; i < sequences.Count; i++)
                    {
                        var seq = sequences[i];
                        for (int s = 0; s < seq.Steps.Count; s++)
                            seq.Steps[s].TryEnd();
                    }
                }
            }

            SoftStop(_universalTracks);
            SoftStop(_stackTracks);
        }

        private void OnTick(TickDelegate tick, float deltaTime)
        {
            bool anyChanged = false;

            void TickLists(List<List<Sequence>> lists)
            {
                for (int t = 0; t < lists.Count; t++)
                {
                    var sequences = lists[t];
                    if (sequences.Count == 0)
                        continue;

                    for (int i = 0; i < sequences.Count;)
                    {
                        var sequence = sequences[i];

                        tick(sequence, deltaTime);

                        // A step callback may have stopped or removed sequences
                        if (i >= sequences.Count || sequences[i] != sequence)
                        {
                            anyChanged = true;
                            continue;
                        }

                        sequence.CleanupSteps();

                        if (sequence.IsDone)
                        {
                            // Remove before invoking so handlers can safely modify the list
                            sequences.RemoveAt(i);
                            anyChanged = true;
                            sequence.OnFinished?.Invoke();
                        }
                        else
                        {
                            i++;
                        }
                    }
                }
            }

            TickLists(_universalTracks);
            TickLists(_stackTracks);

            if (anyChanged)
                UpdateTrackStates();
        }

        #endregion

        #region Messaging

        /// <summary>
        /// Broadcasts a payload to all active sequences and their steps.
        /// </summary>
        /// <typeparam name="T">Type of the payload.</typeparam>
        /// <param name="payload">The message payload.</param>
        public void Broadcast<T>(T payload)
        {
            void Send(List<List<Sequence>> lists)
            {
                for (int t = 0; t < lists.Count; t++)
                {
                    var sequences = lists[t];
                    for (int i = 0; i < sequences.Count; i++)
                        sequences[i].SendMessage(payload);
                }
            }

            Send(_universalTracks);
            Send(_stackTracks);
        }

        /// <summary>
        /// Flushes and executes all queued actions from all sequences.
        /// </summary>
        public void FlushActions()
        {
            void Flush(List<List<Sequence>> lists)
            {
                for (int t = 0; t < lists.Count; t++)
                {
                    var sequences = lists[t];
                    for (int i = 0; i < sequences.Count; i++)
                        sequences[i].PopAction(true);
                }
            }

            Flush(_universalTracks);
            Flush(_stackTracks);
        }

        #endregion
    }
}
