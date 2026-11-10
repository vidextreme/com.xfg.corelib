// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

using UnityEngine;
using System;
using System.Collections.Generic;

namespace XFG.AI.UAI
{
    /// <summary>
    /// Utility AI controller: the core decision engine for Utility AI agents.
    /// </summary>
    /// <remarks>
    /// Evaluates tasks using suppression rules, weighted scoring with smoothing,
    /// optional priority overrides, and fallback behavior. Provides full task
    /// lifecycle callbacks and configurable rethink frequency for real-time
    /// gameplay systems.
    /// </remarks>
    /// <typeparam name="TBrainType">The concrete brain component type.</typeparam>
    /// <typeparam name="TMessageType">The message enum type.</typeparam>
    public abstract class IUtilityAI<TBrainType, TMessageType> : MonoBehaviour
        where TBrainType : MonoBehaviour
        where TMessageType : struct, Enum
    {
        // ---------------------------------------------------------------------
        // Configuration
        // ---------------------------------------------------------------------
        /// <summary>
        /// Seconds between rethinks when using <c>RethinkFrequency.PerNTime</c>.
        /// </summary>
        public float DefaultRethinkTime = 1f;

        /// <summary>
        /// Rethink frequency used when there is no current task.
        /// </summary>
        public RethinkFrequency DefaultRethinkType = RethinkFrequency.PerNTime;

        /// <summary>
        /// Lerp factor used to smooth raw task weights between rethinks.
        /// </summary>
        public float WeightSmoothingFactor = 0.15f;

        /// <summary>
        /// The currently active task.
        /// </summary>
        public ITask CurrentTask { protected set; get; }

        /// <summary>
        /// Task used when no candidate is eligible or the best weight is not positive.
        /// </summary>
        public ITask FallbackTask { get; set; }

        /// <summary>
        /// When false, <see cref="UpdateMachine"/> does nothing.
        /// </summary>
        public bool MachineEnabled = true;

        bool _mustRethink;
        float _currentRethinkTime;

        readonly List<ITask> _tasks = new List<ITask>();
        readonly List<ITask> _candidateBuffer = new List<ITask>(16);

        // ---------------------------------------------------------------------
        // Unity Lifecycle
        // ---------------------------------------------------------------------
        /// <summary>
        /// Unity Awake callback. Empty by default; subclasses may override.
        /// </summary>
        protected virtual void Awake() { }

        // ---------------------------------------------------------------------
        // Rethink Frequency
        // ---------------------------------------------------------------------
        /// <summary>
        /// The effective rethink frequency: the current task's frequency, or
        /// <see cref="DefaultRethinkType"/> if there is no current task.
        /// </summary>
        public RethinkFrequency RethinkFrequency
        {
            get
            {
                if (CurrentTask == null)
                    return DefaultRethinkType;
                return CurrentTask.Frequency;
            }
        }

        // ---------------------------------------------------------------------
        // Task Registration
        // ---------------------------------------------------------------------
        /// <summary>
        /// Registers a task, assigns its Brain, and initializes it.
        /// </summary>
        /// <param name="task">The task to register.</param>
        public void RegisterTask(ITask task)
        {
            task.Brain = this as TBrainType;
            _tasks.Add(task);
            OnTaskRegistered(task);
            task.OnTaskInitialized();
        }

        /// <summary>
        /// Unregisters a task and notifies it that it was destroyed.
        /// </summary>
        /// <param name="task">The task to unregister.</param>
        public void UnregisterTask(ITask task)
        {
            _tasks.Remove(task);
            task.OnTaskDestroyed();
            OnTaskUnregistered(task);
        }

        /// <summary>
        /// Called after a task is registered.
        /// </summary>
        /// <param name="task">The registered task.</param>
        public virtual void OnTaskRegistered(ITask task) { }

        /// <summary>
        /// Called after a task is unregistered.
        /// </summary>
        /// <param name="task">The unregistered task.</param>
        public virtual void OnTaskUnregistered(ITask task) { }

        // ---------------------------------------------------------------------
        // Update Loop
        // ---------------------------------------------------------------------
        /// <summary>
        /// Rethinks if required by the current rethink frequency, then ticks the current task.
        /// </summary>
        public virtual void UpdateMachine()
        {
            if (!MachineEnabled)
                return;

            RethinkFrequency type = RethinkFrequency;

            if (type == RethinkFrequency.PerNTime)
            {
                _currentRethinkTime -= Time.deltaTime;
                if (_currentRethinkTime < 0)
                    Rethink();
            }
            else if (_mustRethink || type == RethinkFrequency.PerUpdate)
            {
                Rethink();
            }

            CurrentTask?.OnTaskUpdate();
        }

        // ---------------------------------------------------------------------
        // Task Completion
        // ---------------------------------------------------------------------
        /// <summary>
        /// Marks the current task as done and schedules a rethink if using <c>RethinkFrequency.OnTaskEnd</c>.
        /// </summary>
        public virtual void TaskSucceeded()
        {
            CurrentTask?.OnTaskDone();
            if (RethinkFrequency == RethinkFrequency.OnTaskEnd)
                _mustRethink = true;
        }

        /// <summary>
        /// Marks the current task as interrupted and schedules a rethink if using <c>RethinkFrequency.OnTaskEnd</c>.
        /// </summary>
        public virtual void TaskFailed()
        {
            CurrentTask?.OnTaskInterrupted();
            if (RethinkFrequency == RethinkFrequency.OnTaskEnd)
                _mustRethink = true;
        }

        // ---------------------------------------------------------------------
        // Rethink Logic (No Linq, No Allocations)
        // ---------------------------------------------------------------------
        /// <summary>
        /// Re-evaluates all tasks and switches to the best one.
        /// </summary>
        /// <remarks>
        /// Steps: suppression filter, priority override, analyze and smooth weights,
        /// sort by total weight, fall back if the best weight is not positive, then switch.
        /// </remarks>
        public virtual void Rethink()
        {
            _mustRethink = false;
            _currentRethinkTime = DefaultRethinkTime;

            if (_tasks.Count == 0)
                return;

            // 1. Suppression filter (reuse buffer)
            _candidateBuffer.Clear();

            for (int i = 0; i < _tasks.Count; i++)
            {
                ITask t = _tasks[i];
                if (t.CanBeConsidered())
                    _candidateBuffer.Add(t);
            }

            if (_candidateBuffer.Count == 0)
            {
                CurrentTask = FallbackTask;
                return;
            }

            // 2. Priority override
            ITask priorityTask = null;
            int highestPriority = 0;

            for (int i = 0; i < _candidateBuffer.Count; i++)
            {
                ITask t = _candidateBuffer[i];
                if (t.Priority > highestPriority)
                {
                    highestPriority = t.Priority;
                    priorityTask = t;
                }
            }

            if (priorityTask != null)
            {
                SwitchTask(priorityTask);
                return;
            }

            // 3. Analyze and smooth
            for (int i = 0; i < _candidateBuffer.Count; i++)
            {
                ITask task = _candidateBuffer[i];

                try
                {
                    task.Analyze();
                }
                catch (Exception ex)
                {
                    Debug.LogError("[UtilityAI] Analyze() failed on " + task.GetType().Name + ": " + ex);
                    task.RawWeight = 0;
                }

                task.SmoothedWeight = Mathf.Lerp(
                    task.SmoothedWeight,
                    task.RawWeight,
                    WeightSmoothingFactor
                );
            }

            // 4. Sort by total weight (in-place)
            _candidateBuffer.Sort((a, b) => b.TotalWeight.CompareTo(a.TotalWeight));

            ITask best = _candidateBuffer[0];

            // 5. Fallback if invalid
            if (best.TotalWeight <= 0)
            {
                CurrentTask = FallbackTask;
                return;
            }

            // 6. Switch task
            SwitchTask(best);
        }

        void SwitchTask(ITask next)
        {
            ITask previous = CurrentTask;

            previous?.OnTaskExit();
            CurrentTask = next;
            CurrentTask?.OnTaskEnter(previous);

            OnSelectTask(CurrentTask);
        }

        /// <summary>
        /// Called after a new task is selected through a task switch.
        /// </summary>
        /// <param name="task">The newly selected task.</param>
        public virtual void OnSelectTask(ITask task) { }

        // ---------------------------------------------------------------------
        // Messaging
        // ---------------------------------------------------------------------
        /// <summary>
        /// Sends a message to the current task, if any.
        /// </summary>
        /// <param name="msgtype">The message to send.</param>
        /// <param name="args">Arguments sent with the message.</param>
        public void SendMessageToMachine(TMessageType msgtype, params object[] args)
        {
            CurrentTask?.OnReceiveMessage(msgtype, args);
        }

        /// <summary>
        /// Removes all registered tasks without calling their destroy callbacks.
        /// </summary>
        public void ClearTasks()
        {
            _tasks.Clear();
        }

        // ---------------------------------------------------------------------
        // Reconsideration (No Linq)
        // ---------------------------------------------------------------------
        /// <summary>
        /// Asks every non-current task whether it wants to interrupt, stopping at the first
        /// task that returns Considered or StopConsidering.
        /// </summary>
        /// <param name="onConsider">Invoked with the final result and considering task when the result is not Maybe.</param>
        /// <returns>The last reconsideration result.</returns>
        public virtual Consideration Reconsider(Action<Consideration, ITask> onConsider)
        {
            Consideration result = Consideration.Maybe;
            ITask consideringTask = null;

            for (int i = 0; i < _tasks.Count; i++)
            {
                ITask task = _tasks[i];

                if (task == CurrentTask)
                    continue;

                result = task.OnReconsider();

                if (result == Consideration.Maybe)
                    continue;

                if (result == Consideration.StopConsidering)
                    break;

                if (result == Consideration.Considered)
                {
                    consideringTask = task;
                    break;
                }
            }

            if (result != Consideration.Maybe)
                onConsider.Invoke(result, consideringTask);

            return result;
        }

        // ---------------------------------------------------------------------
        // Task Interface
        // ---------------------------------------------------------------------
        /// <summary>
        /// A task that the Utility AI can score, select, and run.
        /// </summary>
        public interface ITask
        {
            /// <summary>
            /// The owning brain. Assigned on registration.
            /// </summary>
            TBrainType Brain { get; set; }

            /// <summary>
            /// Raw desirability score before smoothing.
            /// </summary>
            float RawWeight { get; set; }

            /// <summary>
            /// Smoothed weight used to avoid oscillation.
            /// </summary>
            float SmoothedWeight { get; set; }

            /// <summary>
            /// Multiplier applied to the smoothed weight.
            /// </summary>
            float Motivator { get; set; }

            /// <summary>
            /// Final weight used for selection.
            /// </summary>
            float TotalWeight { get; }

            /// <summary>
            /// How often this task should be reconsidered while active.
            /// </summary>
            RethinkFrequency Frequency { get; }

            /// <summary>
            /// Hard priority; the highest positive priority overrides weighted scoring.
            /// </summary>
            int Priority { get; }

            /// <summary>
            /// Whether this task is eligible to be considered.
            /// </summary>
            /// <returns>True if the task can be considered; otherwise false.</returns>
            bool CanBeConsidered();

            /// <summary>
            /// Called once after the task is registered.
            /// </summary>
            void OnTaskInitialized() { }

            /// <summary>
            /// Called when the task is unregistered.
            /// </summary>
            void OnTaskDestroyed() { }

            /// <summary>
            /// Computes RawWeight based on the current world/brain state.
            /// </summary>
            void Analyze();

            /// <summary>
            /// Called when the task becomes the current task.
            /// </summary>
            /// <param name="previousTask">The previously active task, or null.</param>
            void OnTaskEnter(ITask previousTask) { }

            /// <summary>
            /// Called every update while this task is active.
            /// </summary>
            void OnTaskUpdate() { }

            /// <summary>
            /// Called when the task stops being the current task.
            /// </summary>
            void OnTaskExit() { }

            /// <summary>
            /// Called when the task completes successfully.
            /// </summary>
            void OnTaskDone() { }

            /// <summary>
            /// Called when the task is interrupted or fails.
            /// </summary>
            void OnTaskInterrupted() { }

            /// <summary>
            /// Called when the brain sends a message to the current task.
            /// </summary>
            /// <param name="msgtype">The message received.</param>
            /// <param name="args">Arguments sent with the message.</param>
            void OnReceiveMessage(TMessageType msgtype, object[] args) { }

            /// <summary>
            /// Called during reconsideration to influence switching behavior.
            /// </summary>
            /// <returns>The task's reconsideration verdict.</returns>
            Consideration OnReconsider();
        }
    }

    // -------------------------------------------------------------------------
    // Enums
    // -------------------------------------------------------------------------
    /// <summary>
    /// How often a Utility AI rethinks its task selection.
    /// </summary>
    public enum RethinkFrequency
    {
        /// <summary>Rethink every update.</summary>
        PerUpdate,

        /// <summary>Rethink every <c>DefaultRethinkTime</c> seconds.</summary>
        PerNTime,

        /// <summary>Rethink when the current task succeeds or fails.</summary>
        OnTaskEnd
    }

    /// <summary>
    /// Result of a task's reconsideration check.
    /// </summary>
    public enum Consideration
    {
        /// <summary>No opinion; continue checking other tasks.</summary>
        Maybe,

        /// <summary>This task wants to take over; stop checking.</summary>
        Considered,

        /// <summary>Stop checking remaining tasks.</summary>
        StopConsidering
    }
}
