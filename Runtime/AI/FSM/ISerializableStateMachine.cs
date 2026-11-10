// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

using UnityEngine;
using System;

namespace XFG.AI.FSM.Serializable
{
    /// <summary>
    /// A serializable wrapper around IStateMachine that registers states using
    /// SerializeReference. This allows Unity to serialize polymorphic state classes
    /// while still using the engine-grade IStateMachine runtime.
    /// </summary>
    /// <remarks>
    /// This class is fully compatible with:
    /// <list type="bullet">
    /// <item><description>Hierarchical FSM (HFSM)</description></item>
    /// <item><description>Pushdown FSM extensions (PushState, PopState, ReplaceState, etc.)</description></item>
    /// <item><description>Serializable state definitions</description></item>
    /// </list>
    ///
    /// Requirements for compatibility with Pushdown FSM:
    /// <list type="bullet">
    /// <item><description>Machine must expose CurrentStateType</description></item>
    /// <item><description>Machine must expose HasState</description></item>
    /// <item><description>Machine must support ChangeState(TStateIDType, object[])</description></item>
    /// <item><description>Machine must register all states before the first ChangeState call</description></item>
    /// </list>
    ///
    /// This implementation satisfies all requirements.
    /// </remarks>
    /// <typeparam name="TMachineType">The concrete machine component type.</typeparam>
    /// <typeparam name="TState">The serializable state base type.</typeparam>
    /// <typeparam name="TStateIDType">The state identifier type.</typeparam>
    /// <typeparam name="TMessageType">The message identifier type.</typeparam>
    public abstract class ISerializableStateMachine<TMachineType, TState, TStateIDType, TMessageType>
        : IStateMachine<TMachineType, TStateIDType, TMessageType>
            where TMachineType : MonoBehaviour
            where TStateIDType : IComparable
            where TMessageType : IComparable
            where TState : ISerializableStateMachine<TMachineType, TState, TStateIDType, TMessageType>.ISerializableState
    {
        // ---------------------------------------------------------------------
        // Serialized State List
        // ---------------------------------------------------------------------
        /// <summary>
        /// The serialized states. Each state is registered during Awake().
        /// </summary>
        /// <remarks>
        /// States are stored using SerializeReference so Unity can serialize
        /// derived classes. This ensures:
        /// <list type="bullet">
        /// <item><description>All states exist before any transitions occur</description></item>
        /// <item><description>Pushdown FSM operations always have valid state IDs</description></item>
        /// <item><description>HFSM parent/child relationships can be defined inside state classes</description></item>
        /// </list>
        /// </remarks>
        [SerializeReference, Core.SerializableClass]
        public TState[] States;

        // ---------------------------------------------------------------------
        // Unity Lifecycle
        // ---------------------------------------------------------------------
        /// <summary>
        /// Registers all serialized states with the underlying IStateMachine system.
        /// </summary>
        /// <remarks>
        /// This guarantees that:
        /// <list type="bullet">
        /// <item><description>ChangeState() can resolve state IDs</description></item>
        /// <item><description>PushState() and PopState() have valid targets</description></item>
        /// <item><description>HFSM transitions can locate parent/child states</description></item>
        /// </list>
        /// base.Awake() is called first to ensure the underlying FSM initializes
        /// its internal structures before states are registered.
        /// </remarks>
        protected override void Awake()
        {
            base.Awake();

            foreach (TState state in States)
            {
                RegisterState(state);
            }
        }

        // ---------------------------------------------------------------------
        // Serializable State Base Class
        // ---------------------------------------------------------------------
        /// <summary>
        /// Base class for serializable states. Provides the serialized definition only.
        /// </summary>
        /// <remarks>
        /// Each state must provide:
        /// <list type="bullet">
        /// <item><description>A unique ID</description></item>
        /// <item><description>A Machine reference (assigned by RegisterState)</description></item>
        /// <item><description>Standard FSM callbacks</description></item>
        /// </list>
        /// This matches the IState interface required by IStateMachine and
        /// therefore works seamlessly with HFSM (hierarchical transitions),
        /// Pushdown FSM (stack transitions), and message routing.
        ///
        /// No assumptions are made about how the base IStateMachine stores or
        /// transitions states.
        /// </remarks>
        public abstract class ISerializableState : IState
        {
            /// <summary>
            /// Unique identifier for this state.
            /// </summary>
            public abstract TStateIDType ID { get; }

            /// <summary>
            /// The owning machine. Assigned automatically by RegisterState().
            /// </summary>
            public TMachineType Machine { get; set; }

            // Standard FSM callbacks

            /// <summary>
            /// Called when entering this state.
            /// </summary>
            /// <param name="prevStateType">The ID of the state that was exited.</param>
            /// <param name="args">Arguments passed to the transition.</param>
            public virtual void OnStateEnter(TStateIDType prevStateType, object[] args) { }

            /// <summary>
            /// Called every update tick while this state is active.
            /// </summary>
            public virtual void OnStateUpdate() { }

            /// <summary>
            /// Called when exiting this state.
            /// </summary>
            /// <param name="nextStateType">The ID of the state being entered.</param>
            /// <param name="args">Arguments passed to the transition.</param>
            public virtual void OnStateExit(TStateIDType nextStateType, object[] args) { }

            /// <summary>
            /// Called when the machine sends a message to this state.
            /// </summary>
            /// <param name="msgtype">The message received.</param>
            /// <param name="args">Arguments sent with the message.</param>
            public virtual void OnReceiveMessage(TMessageType msgtype, object[] args) { }
        }
    }
}
