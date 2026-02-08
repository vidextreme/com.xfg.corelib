// ------------------------------------------------------------------------------
// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.
// ------------------------------------------------------------------------------
//
// ISerializableStateMachine
//
// A serializable wrapper around IStateMachine that registers states using
// SerializeReference. This allows Unity to serialize polymorphic state classes
// while still using the engine-grade IStateMachine runtime.
//
// This class is fully compatible with:
// - Hierarchical FSM (HFSM)
// - Pushdown FSM extensions (PushState, PopState, ReplaceState, etc.)
// - Serializable state definitions
//
// Requirements for compatibility with Pushdown FSM:
// - Machine must expose CurrentStateType
// - Machine must expose HasState
// - Machine must support ChangeState(TStateIDType, object[])
// - Machine must register all states before the first ChangeState call
//
// This implementation satisfies all requirements.
// ------------------------------------------------------------------------------

using UnityEngine;
using System;

namespace XFG.AI.FSM.Serializable
{
    public abstract class ISerializableStateMachine<TMachineType, TState, TStateIDType, TMessageType>
        : IStateMachine<TMachineType, TStateIDType, TMessageType>
            where TMachineType : MonoBehaviour
            where TStateIDType : IComparable
            where TMessageType : IComparable
            where TState : ISerializableStateMachine<TMachineType, TState, TStateIDType, TMessageType>.ISerializableState
    {
        // ----------------------------------------------------------------------
        // SERIALIZED STATE LIST
        // ----------------------------------------------------------------------
        // States are stored using SerializeReference so Unity can serialize
        // derived classes. Each state is registered during Awake().
        //
        // This ensures:
        // - All states exist before any transitions occur
        // - Pushdown FSM operations always have valid state IDs
        // - HFSM parent/child relationships can be defined inside state classes
        // ----------------------------------------------------------------------

        [SerializeReference, Core.SerializableClass]
        public TState[] States;

        // ----------------------------------------------------------------------
        // UNITY LIFECYCLE
        // ----------------------------------------------------------------------
        // Registers all serialized states with the underlying IStateMachine
        // system. This guarantees that:
        // - ChangeState() can resolve state IDs
        // - PushState() and PopState() have valid targets
        // - HFSM transitions can locate parent/child states
        //
        // base.Awake() is called first to ensure the underlying FSM initializes
        // its internal structures before states are registered.
        // ----------------------------------------------------------------------

        protected override void Awake()
        {
            base.Awake();

            foreach (TState state in States)
            {
                RegisterState(state);
            }
        }

        // ----------------------------------------------------------------------
        // SERIALIZABLE STATE BASE CLASS
        // ----------------------------------------------------------------------
        // Each state must provide:
        // - A unique ID
        // - A Machine reference (assigned by RegisterState)
        // - Standard FSM callbacks
        //
        // This matches the IState interface required by IStateMachine and
        // therefore works seamlessly with:
        // - HFSM (hierarchical transitions)
        // - Pushdown FSM (stack transitions)
        // - Message routing
        //
        // No assumptions are made about how the base IStateMachine stores or
        // transitions states. This class only provides the serialized definition.
        // ----------------------------------------------------------------------

        public abstract class ISerializableState : IState
        {
            // Unique identifier for this state
            public abstract TStateIDType ID { get; }

            // Assigned automatically by RegisterState()
            public TMachineType Machine { get; set; }

            // Standard FSM callbacks
            public virtual void OnStateEnter(TStateIDType prevStateType, object[] args) { }
            public virtual void OnStateUpdate() { }
            public virtual void OnStateExit(TStateIDType nextStateType, object[] args) { }
            public virtual void OnReceiveMessage(TMessageType msgtype, object[] args) { }
        }
    }
}
