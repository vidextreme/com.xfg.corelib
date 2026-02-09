// ------------------------------------------------------------------------------
// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.
// ------------------------------------------------------------------------------
//
// Utility AI Controller
// ------------------------------------------------------------------------------
// Core decision engine for Utility AI agents. Evaluates tasks using suppression
// rules, weighted scoring with smoothing, optional priority overrides, and
// fallback behavior. Provides full task lifecycle callbacks and configurable
// rethink frequency for real-time gameplay systems.
// ------------------------------------------------------------------------------

using UnityEngine;
using System;
using System.Collections.Generic;

namespace XFG.AI.UAI
{
    public abstract class IUtilityAI<TBrainType, TMessageType> : MonoBehaviour
        where TBrainType : MonoBehaviour
        where TMessageType : struct, Enum
    {
        // ----------------------------------------------------------------------
        // Configuration
        // ----------------------------------------------------------------------
        public float DefaultRethinkTime = 1f;
        public RethinkFrequency DefaultRethinkType = RethinkFrequency.PerNTime;
        public float WeightSmoothingFactor = 0.15f;

        public ITask CurrentTask { protected set; get; }
        public ITask FallbackTask { get; set; }
        public bool MachineEnabled = true;

        bool _mustRethink;
        float _currentRethinkTime;

        readonly List<ITask> _tasks = new List<ITask>();
        readonly List<ITask> _candidateBuffer = new List<ITask>(16);

        // ----------------------------------------------------------------------
        // Unity Lifecycle
        // ----------------------------------------------------------------------
        protected virtual void Awake() { }

        // ----------------------------------------------------------------------
        // Rethink Frequency
        // ----------------------------------------------------------------------
        public RethinkFrequency RethinkFrequency
        {
            get
            {
                if (CurrentTask == null)
                    return DefaultRethinkType;
                return CurrentTask.Frequency;
            }
        }

        // ----------------------------------------------------------------------
        // Task Registration
        // ----------------------------------------------------------------------
        public void RegisterTask(ITask task)
        {
            task.Brain = this as TBrainType;
            _tasks.Add(task);
            OnTaskRegistered(task);
            task.OnTaskInitialized();
        }

        public void UnregisterTask(ITask task)
        {
            _tasks.Remove(task);
            task.OnTaskDestroyed();
            OnTaskUnregistered(task);
        }

        public virtual void OnTaskRegistered(ITask task) { }
        public virtual void OnTaskUnregistered(ITask task) { }

        // ----------------------------------------------------------------------
        // Update Loop
        // ----------------------------------------------------------------------
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

        // ----------------------------------------------------------------------
        // Task Completion
        // ----------------------------------------------------------------------
        public virtual void TaskSucceeded()
        {
            CurrentTask?.OnTaskDone();
            if (RethinkFrequency == RethinkFrequency.OnTaskEnd)
                _mustRethink = true;
        }

        public virtual void TaskFailed()
        {
            CurrentTask?.OnTaskInterrupted();
            if (RethinkFrequency == RethinkFrequency.OnTaskEnd)
                _mustRethink = true;
        }

        // ----------------------------------------------------------------------
        // Rethink Logic (No Linq, No Allocations)
        // ----------------------------------------------------------------------
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

            // 3. Analyze + smoothing
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

        public virtual void OnSelectTask(ITask task) { }

        // ----------------------------------------------------------------------
        // Messaging
        // ----------------------------------------------------------------------
        public void SendMessageToMachine(TMessageType msgtype, params object[] args)
        {
            CurrentTask?.OnReceiveMessage(msgtype, args);
        }

        public void ClearTasks()
        {
            _tasks.Clear();
        }

        // ----------------------------------------------------------------------
        // Reconsideration (No Linq)
        // ----------------------------------------------------------------------
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

        // ----------------------------------------------------------------------
        // Task Interface
        // ----------------------------------------------------------------------
        public interface ITask
        {
            TBrainType Brain { get; set; }

            float RawWeight { get; set; }
            float SmoothedWeight { get; set; }
            float Motivator { get; set; }
            float TotalWeight { get; }

            RethinkFrequency Frequency { get; }
            int Priority { get; }

            bool CanBeConsidered();

            void OnTaskInitialized() { }
            void OnTaskDestroyed() { }

            void Analyze();
            void OnTaskEnter(ITask previousTask) { }
            void OnTaskUpdate() { }
            void OnTaskExit() { }
            void OnTaskDone() { }
            void OnTaskInterrupted() { }
            void OnReceiveMessage(TMessageType msgtype, object[] args) { }
            Consideration OnReconsider();
        }
    }

    // --------------------------------------------------------------------------
    // Enums
    // --------------------------------------------------------------------------
    public enum RethinkFrequency
    {
        PerUpdate,
        PerNTime,
        OnTaskEnd
    }

    public enum Consideration
    {
        Maybe,
        Considered,
        StopConsidering
    }
}
