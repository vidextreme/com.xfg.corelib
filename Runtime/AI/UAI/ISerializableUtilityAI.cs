// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.
//
// Description:
//     Serializable wrapper for the Utility AI controller.
//     This class bridges Unity’s serialization system with the runtime
//     Utility AI architecture. It enables designer-authored tasks using
//     SerializeReference while maintaining full compatibility with the
//     AI’s scoring, smoothing, reconsideration, and lifecycle flow.
//
//     Key Features:
//     - Polymorphic task list via SerializeReference
//     - Automatic task registration and Brain assignment
//     - Strong typing for Brain, Task, and Message enums
//     - Designer-friendly serializable task base class
//     - Clean lifecycle hooks and consistent weight model
//
//     This layer ensures that tasks defined in the inspector behave
//     identically to code-registered tasks, making the Utility AI system
//     fully extensible and Unity-friendly.

using System;
using UnityEngine;

namespace XFG.AI.UAI.Serializable
{
    public abstract class ISerializableUtilityAI<TBrainType, TTaskType, TMessageType>
        : IUtilityAI<TBrainType, TMessageType>
        where TBrainType : MonoBehaviour
        where TTaskType : class, IUtilityAI<TBrainType, TMessageType>.ITask
        where TMessageType : struct, Enum
    {
        // Polymorphic list of tasks defined in the Unity inspector.
        // SerializeReference allows derived task types to be stored safely.
        [SerializeReference, Core.SerializableClass]
        public TTaskType[] Tasks;

        /// <summary>
        /// Registers all serialized tasks and assigns their Brain reference.
        /// Ensures inspector-authored tasks behave like code-registered tasks.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            if (Tasks == null)
                return;

            var brain = this as TBrainType;

            for (int i = 0; i < Tasks.Length; i++)
            {
                var task = Tasks[i];
                if (task == null)
                    continue;

                // Assign strongly-typed Brain reference if using our serializable base.
                if (task is ISerializableTask serializableTask)
                    serializableTask.Brain = brain;

                RegisterTask(task);
            }
        }

        /// <summary>
        /// Typed Reconsider wrapper that exposes the concrete task type.
        /// Allows callers to inspect reconsideration results without casting.
        /// </summary>
        public Consideration Reconsider(Action<Consideration, TTaskType> onConsider = null)
        {
            return base.Reconsider((consideration, task) =>
            {
                if (task is TTaskType typedTask)
                    onConsider?.Invoke(consideration, typedTask);
            });
        }

        // ====================================================================
        //  Serializable Task Base Class
        // ====================================================================
        public abstract class ISerializableTask : ITask
        {
            // Strongly-typed reference to the owning brain/MonoBehaviour.
            // Assigned automatically during Awake().
            public TBrainType Brain { get; set; }

            // Raw desirability score before smoothing.
            public virtual float RawWeight { get; set; }

            // Smoothed weight used to avoid oscillation.
            public virtual float SmoothedWeight { get; set; }

            // Multiplier applied to the smoothed weight.
            public virtual float Motivator { get; set; } = 1f;

            // Final weight used for selection.
            public float TotalWeight => SmoothedWeight * Motivator;

            // Optional hard priority override.
            public virtual int Priority => 0;

            // How often this task should be reconsidered.
            public abstract RethinkFrequency Frequency { get; }

            // Whether this task is eligible to be considered.
            public virtual bool CanBeConsidered() => true;

            // Compute RawWeight based on current world/brain state.
            public abstract void Analyze();

            // Called when the task becomes the current task.
            public virtual void OnTaskEnter(ISerializableTask previousTask) { }

            // Interface bridge: safely cast previous task when possible.
            void ITask.OnTaskEnter(ITask previousTask)
            {
                OnTaskEnter(previousTask as ISerializableTask);
            }

            // Called every update while this task is active.
            public virtual void OnTaskUpdate() { }

            // Called when the task stops being the current task.
            public virtual void OnTaskExit() { }

            // Called when the task completes successfully.
            public virtual void OnTaskDone() { }

            // Called when the task is interrupted or fails.
            public virtual void OnTaskInterrupted() { }

            // Called when the brain sends a message to the current task.
            public virtual void OnReceiveMessage(TMessageType messageType, object[] args) { }

            // Called during reconsideration to influence switching behavior.
            public virtual Consideration OnReconsider() => Consideration.Maybe;
        }
    }
}
