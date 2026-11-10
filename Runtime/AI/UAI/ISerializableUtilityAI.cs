// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

using System;
using UnityEngine;

namespace XFG.AI.UAI.Serializable
{
    /// <summary>
    /// Serializable wrapper for the Utility AI controller. Bridges Unity's
    /// serialization system with the runtime Utility AI architecture.
    /// </summary>
    /// <remarks>
    /// Enables designer-authored tasks using SerializeReference while maintaining
    /// full compatibility with the AI's scoring, smoothing, reconsideration, and
    /// lifecycle flow.
    ///
    /// Key features:
    /// <list type="bullet">
    /// <item><description>Polymorphic task list via SerializeReference</description></item>
    /// <item><description>Automatic task registration and Brain assignment</description></item>
    /// <item><description>Strong typing for Brain, Task, and Message enums</description></item>
    /// <item><description>Designer-friendly serializable task base class</description></item>
    /// <item><description>Clean lifecycle hooks and consistent weight model</description></item>
    /// </list>
    ///
    /// This layer ensures that tasks defined in the inspector behave
    /// identically to code-registered tasks, making the Utility AI system
    /// fully extensible and Unity-friendly.
    /// </remarks>
    /// <typeparam name="TBrainType">The concrete brain component type.</typeparam>
    /// <typeparam name="TTaskType">The serializable task base type.</typeparam>
    /// <typeparam name="TMessageType">The message enum type.</typeparam>
    public abstract class ISerializableUtilityAI<TBrainType, TTaskType, TMessageType>
        : IUtilityAI<TBrainType, TMessageType>
        where TBrainType : MonoBehaviour
        where TTaskType : class, IUtilityAI<TBrainType, TMessageType>.ITask
        where TMessageType : struct, Enum
    {
        /// <summary>
        /// Polymorphic list of tasks defined in the Unity inspector.
        /// SerializeReference allows derived task types to be stored safely.
        /// </summary>
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

                // Assign strongly-typed Brain reference if using our serializable base
                if (task is ISerializableTask serializableTask)
                    serializableTask.Brain = brain;

                RegisterTask(task);
            }
        }

        /// <summary>
        /// Typed Reconsider wrapper that exposes the concrete task type.
        /// Allows callers to inspect reconsideration results without casting.
        /// </summary>
        /// <param name="onConsider">Optional callback invoked with the result and the considered task.</param>
        /// <returns>The reconsideration result.</returns>
        public Consideration Reconsider(Action<Consideration, TTaskType> onConsider = null)
        {
            return base.Reconsider((consideration, task) =>
            {
                if (task is TTaskType typedTask)
                    onConsider?.Invoke(consideration, typedTask);
            });
        }

        // ---------------------------------------------------------------------
        // Serializable Task Base Class
        // ---------------------------------------------------------------------
        /// <summary>
        /// Designer-friendly base class for tasks authored in the inspector.
        /// </summary>
        public abstract class ISerializableTask : ITask
        {
            /// <summary>
            /// Strongly-typed reference to the owning brain/MonoBehaviour.
            /// Assigned automatically during Awake().
            /// </summary>
            public TBrainType Brain { get; set; }

            /// <summary>
            /// Raw desirability score before smoothing.
            /// </summary>
            public virtual float RawWeight { get; set; }

            /// <summary>
            /// Smoothed weight used to avoid oscillation.
            /// </summary>
            public virtual float SmoothedWeight { get; set; }

            /// <summary>
            /// Multiplier applied to the smoothed weight.
            /// </summary>
            public virtual float Motivator { get; set; } = 1f;

            /// <summary>
            /// Final weight used for selection.
            /// </summary>
            public float TotalWeight => SmoothedWeight * Motivator;

            /// <summary>
            /// Optional hard priority override.
            /// </summary>
            public virtual int Priority => 0;

            /// <summary>
            /// How often this task should be reconsidered.
            /// </summary>
            public abstract RethinkFrequency Frequency { get; }

            /// <summary>
            /// Whether this task is eligible to be considered.
            /// </summary>
            /// <returns>True if the task can be considered; otherwise false.</returns>
            public virtual bool CanBeConsidered() => true;

            /// <summary>
            /// Computes RawWeight based on the current world/brain state.
            /// </summary>
            public abstract void Analyze();

            /// <summary>
            /// Called when the task becomes the current task.
            /// </summary>
            /// <param name="previousTask">The previously active task, or null if it was not an ISerializableTask.</param>
            public virtual void OnTaskEnter(ISerializableTask previousTask) { }

            // Interface bridge: safely cast the previous task when possible
            void ITask.OnTaskEnter(ITask previousTask)
            {
                OnTaskEnter(previousTask as ISerializableTask);
            }

            /// <summary>
            /// Called every update while this task is active.
            /// </summary>
            public virtual void OnTaskUpdate() { }

            /// <summary>
            /// Called when the task stops being the current task.
            /// </summary>
            public virtual void OnTaskExit() { }

            /// <summary>
            /// Called when the task completes successfully.
            /// </summary>
            public virtual void OnTaskDone() { }

            /// <summary>
            /// Called when the task is interrupted or fails.
            /// </summary>
            public virtual void OnTaskInterrupted() { }

            /// <summary>
            /// Called when the brain sends a message to the current task.
            /// </summary>
            /// <param name="messageType">The message received.</param>
            /// <param name="args">Arguments sent with the message.</param>
            public virtual void OnReceiveMessage(TMessageType messageType, object[] args) { }

            /// <summary>
            /// Called during reconsideration to influence switching behavior.
            /// </summary>
            /// <returns>The task's reconsideration verdict; Maybe by default.</returns>
            public virtual Consideration OnReconsider() => Consideration.Maybe;
        }
    }
}
