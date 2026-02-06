// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace XFG.AI
{
    /// <summary>
    /// A generic, type-safe finite state machine for Unity components.
    ///
    /// This class provides a lightweight, extensible framework for building
    /// gameplay state logic. States register themselves with the machine and
    /// expose lifecycle callbacks for entering, updating, exiting, and receiving
    /// messages. The machine guarantees predictable transitions, explicit state
    /// ownership, and clean separation of responsibilities.
    ///
    /// IMPLEMENTATION OVERVIEW:
    /// -------------------------
    /// • States are stored in a dictionary keyed by a strongly-typed ID.
    /// • The machine tracks the active state and forwards update ticks and
    ///   messages to it.
    /// • Transitions are synchronous and ordered:
    ///       1. Exit old state
    ///       2. Update machine's active state reference
    ///       3. Enter new state
    /// • The machine does not assume any default state; transitions must be
    ///   explicitly triggered by external systems.
    ///
    /// This design keeps the API minimal, predictable, and easy to extend
    /// (e.g., async transitions, hierarchical states, or message routing layers).
    /// </summary>
    public abstract class IStateMachine<TMachineType, TStateIDType, TMessageType> : MonoBehaviour
        where TMachineType : MonoBehaviour
        where TStateIDType : IComparable
        where TMessageType : IComparable
    {
        // --------------------------------------------------------------------
        //  INTERNAL STATE STORAGE
        // --------------------------------------------------------------------

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


        // --------------------------------------------------------------------
        //  UNITY LIFECYCLE
        // --------------------------------------------------------------------

        protected virtual void Awake()
        {
            // Intentionally empty — subclasses may override.
        }


        // --------------------------------------------------------------------
        //  STATE REGISTRATION
        // --------------------------------------------------------------------

        /// <summary>
        /// Registers a state instance with the machine.
        /// </summary>
        public void RegisterState(IState state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));

            if (_states.ContainsKey(state.ID))
                throw new Exception($"State {state.ID} already registered.");

            state.Machine = this as TMachineType;
            _states.Add(state.ID, state);
        }


        // --------------------------------------------------------------------
        //  STATE TRANSITION
        // --------------------------------------------------------------------

        /// <summary>
        /// Performs a synchronous state transition.
        /// </summary>
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


        // --------------------------------------------------------------------
        //  UPDATE LOOP
        // --------------------------------------------------------------------

        /// <summary>
        /// Called by external systems (e.g., Update) to tick the active state.
        /// </summary>
        public virtual void UpdateMachine()
        {
            CurrentState?.OnStateUpdate();
        }


        // --------------------------------------------------------------------
        //  MESSAGE ROUTING
        // --------------------------------------------------------------------

        /// <summary>
        /// Sends a message to the active state.
        /// </summary>
        public void SendMessageToMachine(TMessageType msgtype, params object[] args)
        {
            if (!HasState)
            {
                Debug.LogWarning($"Message {msgtype} ignored — no active state.");
                return;
            }

            CurrentState?.OnReceiveMessage(msgtype, args);
        }


        // --------------------------------------------------------------------
        //  STATE INTERFACE
        // --------------------------------------------------------------------

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
            void OnStateEnter(TStateIDType prevStateType, object[] args) { }

            /// <summary>
            /// Called every update tick while this state is active.
            /// </summary>
            void OnStateUpdate() { }

            /// <summary>
            /// Called when exiting this state.
            /// </summary>
            void OnStateExit(TStateIDType nextStateType, object[] args) { }

            /// <summary>
            /// Called when the machine sends a message to this state.
            /// </summary>
            void OnReceiveMessage(TMessageType msgtype, object[] args) { }
        }
    }
}
