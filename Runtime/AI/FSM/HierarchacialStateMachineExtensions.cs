// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace XFG.AI.FSM.Hierarchical
{
    /// <summary>
    /// Hierarchical Finite State Machine (HFSM) extension layer for IStateMachine.
    /// </summary>
    /// <remarks>
    /// This adds hierarchical (parent -&gt; child) state behavior without modifying
    /// the original state machine. States may optionally implement
    /// IHierarchicalState to participate in the hierarchy.
    ///
    /// Implementation overview:
    /// <list type="bullet">
    /// <item><description>Each hierarchical state exposes a Parent pointer.</description></item>
    /// <item><description>The machine builds an active stack from root -&gt; leaf.</description></item>
    /// <item><description>Update bubbling: the stack is walked from root to leaf and the
    /// first state that handles updates receives it.</description></item>
    /// <item><description>Message bubbling: leaf states get first chance to handle messages.
    /// If they decline, parents may handle it.</description></item>
    /// </list>
    ///
    /// This design keeps the base FSM simple while enabling layered behavior
    /// such as movement -&gt; locomotion -&gt; running, combat -&gt; melee -&gt; combo, etc.
    /// </remarks>
    public static class HierarchacialStateMachineExtensions
    {
        // ---------------------------------------------------------------------
        // Hierarchical State Interface
        // ---------------------------------------------------------------------
        /// <summary>
        /// Optional interface for states that participate in a hierarchy.
        /// </summary>
        /// <typeparam name="TMachineType">The concrete machine component type.</typeparam>
        /// <typeparam name="TStateIDType">The state identifier type.</typeparam>
        /// <typeparam name="TMessageType">The message identifier type.</typeparam>
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
            /// <param name="msg">The message received.</param>
            /// <param name="args">Arguments sent with the message.</param>
            void OnReceiveMessageHierarchy(TMessageType msg, object[] args);
        }

        // ---------------------------------------------------------------------
        // Active Hierarchy Stack
        // ---------------------------------------------------------------------
        private static readonly Dictionary<object, List<object>> _activeStacks =
            new Dictionary<object, List<object>>();

        private static List<object> GetStack(object machine)
        {
            if (!_activeStacks.ContainsKey(machine))
                _activeStacks[machine] = new List<object>(8);

            return _activeStacks[machine];
        }

        // ---------------------------------------------------------------------
        // Stack Building
        // ---------------------------------------------------------------------
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

            // Reverse so the stack is root -> leaf
            stack.Reverse();
        }

        // ---------------------------------------------------------------------
        // Hook Into State Transitions
        // ---------------------------------------------------------------------
        /// <summary>
        /// Rebuilds the active hierarchy stack from the machine's current state.
        /// Call this after every state transition.
        /// </summary>
        /// <typeparam name="TMachineType">The concrete machine component type.</typeparam>
        /// <typeparam name="TStateIDType">The state identifier type.</typeparam>
        /// <typeparam name="TMessageType">The message identifier type.</typeparam>
        /// <param name="machine">The state machine whose hierarchy to rebuild.</param>
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

        // ---------------------------------------------------------------------
        // Hierarchical Update
        // ---------------------------------------------------------------------
        /// <summary>
        /// Ticks the hierarchy: the first state from root to leaf that handles updates receives it.
        /// Falls back to the current state's normal update if none do.
        /// </summary>
        /// <typeparam name="TMachineType">The concrete machine component type.</typeparam>
        /// <typeparam name="TStateIDType">The state identifier type.</typeparam>
        /// <typeparam name="TMessageType">The message identifier type.</typeparam>
        /// <param name="machine">The state machine to update.</param>
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

            // Fall back to the leaf's normal update
            machine.CurrentState?.OnStateUpdate();
        }

        // ---------------------------------------------------------------------
        // Hierarchical Message Routing
        // ---------------------------------------------------------------------
        /// <summary>
        /// Routes a message through the hierarchy: the first state from leaf to root that handles messages receives it.
        /// Falls back to the current state's normal message handler if none do.
        /// </summary>
        /// <typeparam name="TMachineType">The concrete machine component type.</typeparam>
        /// <typeparam name="TStateIDType">The state identifier type.</typeparam>
        /// <typeparam name="TMessageType">The message identifier type.</typeparam>
        /// <param name="machine">The state machine to send the message to.</param>
        /// <param name="msg">The message to send.</param>
        /// <param name="args">Arguments sent with the message.</param>
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

            // Fall back to the leaf's normal message handler
            machine.CurrentState?.OnReceiveMessage(msg, args);
        }
    }
}
