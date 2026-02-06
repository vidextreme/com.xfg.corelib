// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace XFG.AI.FSM.Hierarchical
{
    /// <summary>
    /// Hierarchical Finite State Machine (HFSM) extension layer for IStateMachine.
    ///
    /// This adds hierarchical (parent -> child) state behavior without modifying
    /// the original state machine. States may optionally implement
    /// IHierarchicalState to participate in the hierarchy.
    ///
    /// IMPLEMENTATION OVERVIEW:
    /// -------------------------
    /// - Each hierarchical state exposes a Parent pointer.
    /// - The machine builds an active stack from root -> leaf.
    /// - Update bubbling:
    ///       Child states get first chance to handle update.
    ///       If they decline, parents may handle it.
    /// - Message bubbling:
    ///       Leaf states get first chance to handle messages.
    ///       If they decline, parents may handle it.
    ///
    /// This design keeps the base FSM simple while enabling layered behavior
    /// such as movement -> locomotion -> running, combat -> melee -> combo, etc.
    /// </summary>
    public static class HFSMExtensions
    {
        // --------------------------------------------------------------------
        //  HIERARCHICAL STATE INTERFACE
        // --------------------------------------------------------------------

        /// <summary>
        /// Optional interface for states that participate in a hierarchy.
        /// </summary>
        public interface IHierarchicalState<TMachineType, TStateIDType, TMessageType> :
            IStateMachine<TMachineType, TStateIDType, TMessageType>.IState
            where TMachineType : MonoBehaviour
            where TStateIDType : IComparable
            where TMessageType : IComparable
        {
            /// <summary>
            /// Parent state in the hierarchy. Null for root states.
            /// </summary>
            IHierarchicalState<TMachineType, TStateIDType, TMessageType> Parent { get; }

            /// <summary>
            /// If true, this state handles Update before bubbling upward.
            /// </summary>
            bool HandlesUpdate { get; }

            /// <summary>
            /// If true, this state handles messages before bubbling upward.
            /// </summary>
            bool HandlesMessage { get; }

            /// <summary>
            /// Hierarchical update callback.
            /// </summary>
            void OnStateUpdateHierarchy();

            /// <summary>
            /// Hierarchical message callback.
            /// </summary>
            void OnReceiveMessageHierarchy(TMessageType msg, object[] args);
        }


        // --------------------------------------------------------------------
        //  ACTIVE HIERARCHY STACK
        // --------------------------------------------------------------------

        private static readonly Dictionary<object, List<object>> _activeStacks =
            new Dictionary<object, List<object>>();

        private static List<object> GetStack(object machine)
        {
            if (!_activeStacks.ContainsKey(machine))
                _activeStacks[machine] = new List<object>(8);

            return _activeStacks[machine];
        }


        // --------------------------------------------------------------------
        //  STACK BUILDING
        // --------------------------------------------------------------------

        private static void BuildActiveStack<TMachineType, TStateIDType, TMessageType>(
            IStateMachine<TMachineType, TStateIDType, TMessageType> machine,
            IHierarchicalState<TMachineType, TStateIDType, TMessageType> leaf)
            where TMachineType : MonoBehaviour
            where TStateIDType : IComparable
            where TMessageType : IComparable
        {
            var stack = GetStack(machine);
            stack.Clear();

            var current = leaf;
            while (current != null)
            {
                stack.Add(current);
                current = current.Parent;
            }

            // Reverse so stack is root -> leaf
            stack.Reverse();
        }


        // --------------------------------------------------------------------
        //  HOOK INTO STATE TRANSITIONS
        // --------------------------------------------------------------------

        public static void RebuildHierarchy<TMachineType, TStateIDType, TMessageType>(
            this IStateMachine<TMachineType, TStateIDType, TMessageType> machine)
            where TMachineType : MonoBehaviour
            where TStateIDType : IComparable
            where TMessageType : IComparable
        {
            if (!machine.HasState)
            {
                GetStack(machine).Clear();
                return;
            }

            if (machine.CurrentState is
                IHierarchicalState<TMachineType, TStateIDType, TMessageType> hState)
            {
                BuildActiveStack(machine, hState);
            }
            else
            {
                GetStack(machine).Clear();
            }
        }


        // --------------------------------------------------------------------
        //  HIERARCHICAL UPDATE
        // --------------------------------------------------------------------

        public static void UpdateMachineHFSM<TMachineType, TStateIDType, TMessageType>(
            this IStateMachine<TMachineType, TStateIDType, TMessageType> machine)
            where TMachineType : MonoBehaviour
            where TStateIDType : IComparable
            where TMessageType : IComparable
        {
            if (!machine.HasState)
                return;

            var stack = GetStack(machine);

            // Root -> leaf bubbling
            for (int i = 0; i < stack.Count; i++)
            {
                var state = stack[i] as
                    IHierarchicalState<TMachineType, TStateIDType, TMessageType>;

                if (state != null && state.HandlesUpdate)
                {
                    state.OnStateUpdateHierarchy();
                    return;
                }
            }

            // Fallback to leaf's normal update
            machine.CurrentState?.OnStateUpdate();
        }


        // --------------------------------------------------------------------
        //  HIERARCHICAL MESSAGE ROUTING
        // --------------------------------------------------------------------

        public static void SendMessageHFSM<TMachineType, TStateIDType, TMessageType>(
            this IStateMachine<TMachineType, TStateIDType, TMessageType> machine,
            TMessageType msg,
            params object[] args)
            where TMachineType : MonoBehaviour
            where TStateIDType : IComparable
            where TMessageType : IComparable
        {
            if (!machine.HasState)
                return;

            var stack = GetStack(machine);

            // Leaf -> root bubbling
            for (int i = stack.Count - 1; i >= 0; i--)
            {
                var state = stack[i] as
                    IHierarchicalState<TMachineType, TStateIDType, TMessageType>;

                if (state != null && state.HandlesMessage)
                {
                    state.OnReceiveMessageHierarchy(msg, args);
                    return;
                }
            }

            // Fallback to leaf's normal message handler
            machine.CurrentState?.OnReceiveMessage(msg, args);
        }
    }
}
