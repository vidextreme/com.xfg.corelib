// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace XFG.AI
{
    /// <summary>
    /// A generic, type-safe finite state machine for Unity components.
    /// </summary>
    /// <remarks>
    /// This class provides a lightweight, extensible framework for building
    /// gameplay state logic. States register themselves with the machine and
    /// expose lifecycle callbacks for entering, updating, exiting, and receiving
    /// messages. The machine guarantees predictable transitions, explicit state
    /// ownership, and clean separation of responsibilities.
    ///
    /// Implementation overview:
    /// <list type="bullet">
    /// <item><description>States are stored in a dictionary keyed by a strongly-typed ID.</description></item>
    /// <item><description>The machine tracks the active state and forwards update ticks and
    /// messages to it.</description></item>
    /// <item><description>Transitions are synchronous and ordered: (1) exit old state,
    /// (2) update the machine's active state reference, (3) enter new state.</description></item>
    /// <item><description>The machine does not assume any default state; transitions must be
    /// explicitly triggered by external systems.</description></item>
    /// </list>
    ///
    /// This design keeps the API minimal, predictable, and easy to extend
    /// (e.g., async transitions, hierarchical states, or message routing layers).
    /// </remarks>
    /// <typeparam name="TMachineType">The concrete machine component type.</typeparam>
    /// <typeparam name="TStateIDType">The state identifier type.</typeparam>
    /// <typeparam name="TMessageType">The message identifier type.</typeparam>
    public abstract class IStateMachine<TMachineType, TStateIDType, TMessageType> : MonoBehaviour
        where TMachineType : MonoBehaviour
        where TStateIDType : IComparable
        where TMessageType : IComparable
    {
        // ---------------------------------------------------------------------
        // Internal State Storage
        // ---------------------------------------------------------------------
        /// <summary>
        /// Bag of registered states, keyed by their ID.
        /// </summary>
        private readonly Dictionary<TStateIDType, IState> _states =
            new Dictionary<TStateIDType, IState>();

        /// <summary>
        /// True once the machine has successfully transitioned into a state.
        /// </summary>
        public bool HasState { get; private set; }

        /// <summary>
        /// The ID of the currently active state.
        /// </summary>
        public TStateIDType CurrentStateType { get; private set; }

        /// <summary>
        /// The currently active state instance.
        /// </summary>
        public IState CurrentState { get; private set; }

        // ---------------------------------------------------------------------
        // Unity Lifecycle
        // ---------------------------------------------------------------------
        /// <summary>
        /// Unity Awake callback. Empty by default; subclasses may override.
        /// </summary>
        protected virtual void Awake()
        {
            // Intentionally empty; subclasses may override
        }

        // ---------------------------------------------------------------------
        // State Registration
        // ---------------------------------------------------------------------
        /// <summary>
        /// Registers a state instance with the machine and assigns its machine back-reference.
        /// </summary>
        /// <param name="state">The state to register.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="state"/> is null.</exception>
        /// <exception cref="Exception">Thrown when a state with the same ID is already registered.</exception>
        public void RegisterState(IState state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));

            if (_states.ContainsKey(state.ID))
                throw new Exception($"State {state.ID} already registered.");

            state.Machine = this as TMachineType;
            _states.Add(state.ID, state);
        }

        // ---------------------------------------------------------------------
        // State Transition
        // ---------------------------------------------------------------------
        /// <summary>
        /// Performs a synchronous state transition.
        /// </summary>
        /// <param name="nextStateType">The ID of the state to enter.</param>
        /// <param name="args">Arguments forwarded to the exit and enter callbacks.</param>
        /// <returns>True if the transition happened; false if already in that state or it is not registered.</returns>
        public bool ChangeState(TStateIDType nextStateType, params object[] args)
        {
            // Skip if already in this state
            if (HasState && CurrentStateType.CompareTo(nextStateType) == 0)
                return false;

            // Retrieve next state
            if (!_states.TryGetValue(nextStateType, out IState nextState))
            {
                Debug.LogWarning($"{nextStateType} is not registered in {gameObject.name}");
                return false;
            }

            var prevState = CurrentState;
            var prevStateType = CurrentStateType;

            // Exit old state
            prevState?.OnStateExit(nextStateType, args);

            // Update machine state BEFORE entering new state
            CurrentStateType = nextStateType;
            CurrentState = nextState;
            HasState = true;

            // Enter new state
            nextState.OnStateEnter(prevStateType, args);

            return true;
        }

        // ---------------------------------------------------------------------
        // Update Loop
        // ---------------------------------------------------------------------
        /// <summary>
        /// Called by external systems (e.g., Update) to tick the active state.
        /// </summary>
        public virtual void UpdateMachine()
        {
            CurrentState?.OnStateUpdate();
        }

        // ---------------------------------------------------------------------
        // Message Routing
        // ---------------------------------------------------------------------
        /// <summary>
        /// Sends a message to the active state. Logs a warning if there is no active state.
        /// </summary>
        /// <param name="msgtype">The message to send.</param>
        /// <param name="args">Arguments forwarded to the state's message handler.</param>
        public void SendMessageToMachine(TMessageType msgtype, params object[] args)
        {
            if (!HasState)
            {
                Debug.LogWarning($"Message {msgtype} ignored — no active state.");
                return;
            }

            CurrentState?.OnReceiveMessage(msgtype, args);
        }

        // ---------------------------------------------------------------------
        // State Interface
        // ---------------------------------------------------------------------
        /// <summary>
        /// Base interface for all states used by this machine.
        /// </summary>
        public interface IState
        {
            /// <summary>
            /// Back-reference to the owning machine.
            /// </summary>
            TMachineType Machine { get; set; }

            /// <summary>
            /// Unique identifier for this state.
            /// </summary>
            TStateIDType ID { get; }

            /// <summary>
            /// Called when entering this state.
            /// </summary>
            /// <param name="prevStateType">The ID of the state that was exited.</param>
            /// <param name="args">Arguments passed to the transition.</param>
            void OnStateEnter(TStateIDType prevStateType, object[] args) { }

            /// <summary>
            /// Called every update tick while this state is active.
            /// </summary>
            void OnStateUpdate() { }

            /// <summary>
            /// Called when exiting this state.
            /// </summary>
            /// <param name="nextStateType">The ID of the state being entered.</param>
            /// <param name="args">Arguments passed to the transition.</param>
            void OnStateExit(TStateIDType nextStateType, object[] args) { }

            /// <summary>
            /// Called when the machine sends a message to this state.
            /// </summary>
            /// <param name="msgtype">The message received.</param>
            /// <param name="args">Arguments sent with the message.</param>
            void OnReceiveMessage(TMessageType msgtype, object[] args) { }
        }
    }
}
