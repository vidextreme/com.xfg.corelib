// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

using System.Collections.Generic;
using UnityEngine;

namespace XFG.AI.FSM.Pushdown
{
    /// <summary>
    /// Stack-based state transitions for IStateMachine.
    /// </summary>
    /// <remarks>
    /// This module adds pushdown (stack-driven) behavior on top of the base FSM
    /// without modifying the core IStateMachine implementation. It enables temporary
    /// overlays, interruptible states, nested UI flows, and clean return-to-previous
    /// behavior.
    ///
    /// The pushdown layer is implemented entirely as extension methods. Each machine
    /// instance maintains its own stack of paused states. Only the top state is
    /// active; all others remain suspended until popped.
    /// </remarks>
    public static class PushdownStateMachineExtensions
    {
        // ---------------------------------------------------------------------
        // Internal Stack Storage
        // ---------------------------------------------------------------------
        // Each machine instance gets its own stack. The dictionary key is the
        // machine reference, and the value is a stack of TStateID entries.
        //
        // This design ensures:
        // - No modification to the base FSM class
        // - Multiple FSM instances can coexist safely
        // - Stack lifetime matches the machine lifetime
        private static readonly Dictionary<object, Stack<object>> _stateStacks =
            new Dictionary<object, Stack<object>>();

        private static Stack<object> GetStack(object machine)
        {
            if (!_stateStacks.TryGetValue(machine, out var stack))
            {
                stack = new Stack<object>();
                _stateStacks[machine] = stack;
            }
            return stack;
        }

        // ---------------------------------------------------------------------
        // Push State
        // ---------------------------------------------------------------------
        /// <summary>
        /// Saves the current state on the stack, then transitions into the new state.
        /// This is the core pushdown operation used for overlays such as Pause, Inventory, Dialogue, etc.
        /// </summary>
        /// <typeparam name="TMachine">The concrete machine component type.</typeparam>
        /// <typeparam name="TStateID">The state identifier type.</typeparam>
        /// <typeparam name="TMessage">The message identifier type.</typeparam>
        /// <param name="machine">The state machine to transition.</param>
        /// <param name="nextState">The ID of the state to enter.</param>
        /// <param name="args">Arguments forwarded to the transition callbacks.</param>
        /// <returns>True if the transition happened; otherwise false.</returns>
        public static bool PushState<TMachine, TStateID, TMessage>(
            this IStateMachine<TMachine, TStateID, TMessage> machine,
            TStateID nextState,
            params object[] args)
            where TMachine : MonoBehaviour
            where TStateID : System.IComparable
            where TMessage : System.IComparable
        {
            var stack = GetStack(machine);

            if (machine.HasState)
                stack.Push(machine.CurrentStateType);

            return machine.ChangeState(nextState, args);
        }

        // ---------------------------------------------------------------------
        // Pop State
        // ---------------------------------------------------------------------
        /// <summary>
        /// Restores the previous state from the stack. If the stack is empty, nothing happens.
        /// This is the inverse of PushState and is used to resume gameplay or return to the previous overlay.
        /// </summary>
        /// <typeparam name="TMachine">The concrete machine component type.</typeparam>
        /// <typeparam name="TStateID">The state identifier type.</typeparam>
        /// <typeparam name="TMessage">The message identifier type.</typeparam>
        /// <param name="machine">The state machine to transition.</param>
        /// <param name="args">Arguments forwarded to the transition callbacks.</param>
        /// <returns>True if a state was popped and the transition happened; otherwise false.</returns>
        public static bool PopState<TMachine, TStateID, TMessage>(
            this IStateMachine<TMachine, TStateID, TMessage> machine,
            params object[] args)
            where TMachine : MonoBehaviour
            where TStateID : System.IComparable
            where TMessage : System.IComparable
        {
            var stack = GetStack(machine);

            if (stack.Count == 0)
                return false;

            var previous = (TStateID)stack.Pop();
            return machine.ChangeState(previous, args);
        }

        // ---------------------------------------------------------------------
        // Clear Stack
        // ---------------------------------------------------------------------
        /// <summary>
        /// Removes all paused states. Useful when resetting gameplay, loading a
        /// new scene, or ensuring no stale states remain.
        /// </summary>
        /// <typeparam name="TMachine">The concrete machine component type.</typeparam>
        /// <typeparam name="TStateID">The state identifier type.</typeparam>
        /// <typeparam name="TMessage">The message identifier type.</typeparam>
        /// <param name="machine">The state machine whose stack to clear.</param>
        public static void ClearStateStack<TMachine, TStateID, TMessage>(
            this IStateMachine<TMachine, TStateID, TMessage> machine)
            where TMachine : MonoBehaviour
            where TStateID : System.IComparable
            where TMessage : System.IComparable
        {
            GetStack(machine).Clear();
        }

        // ---------------------------------------------------------------------
        // Replace State
        // ---------------------------------------------------------------------
        /// <summary>
        /// Replaces the active state without modifying the stack. This is useful
        /// for switching overlays (Pause -&gt; Settings) while preserving the return
        /// path (Settings -&gt; Pause -&gt; Gameplay).
        /// </summary>
        /// <typeparam name="TMachine">The concrete machine component type.</typeparam>
        /// <typeparam name="TStateID">The state identifier type.</typeparam>
        /// <typeparam name="TMessage">The message identifier type.</typeparam>
        /// <param name="machine">The state machine to transition.</param>
        /// <param name="newState">The ID of the state to enter.</param>
        /// <param name="args">Arguments forwarded to the transition callbacks.</param>
        /// <returns>True if the transition happened; otherwise false.</returns>
        public static bool ReplaceState<TMachine, TStateID, TMessage>(
            this IStateMachine<TMachine, TStateID, TMessage> machine,
            TStateID newState,
            params object[] args)
            where TMachine : MonoBehaviour
            where TStateID : System.IComparable
            where TMessage : System.IComparable
        {
            if (!machine.HasState)
                return machine.ChangeState(newState, args);

            return machine.ChangeState(newState, args);
        }

        // ---------------------------------------------------------------------
        // Peek State
        // ---------------------------------------------------------------------
        /// <summary>
        /// Returns the state that would be restored by PopState(), without
        /// removing it from the stack. Useful for UI previews or debugging.
        /// </summary>
        /// <typeparam name="TMachine">The concrete machine component type.</typeparam>
        /// <typeparam name="TStateID">The state identifier type.</typeparam>
        /// <typeparam name="TMessage">The message identifier type.</typeparam>
        /// <param name="machine">The state machine to query.</param>
        /// <returns>The top paused state ID, or the default value if the stack is empty.</returns>
        public static TStateID PeekState<TMachine, TStateID, TMessage>(
            this IStateMachine<TMachine, TStateID, TMessage> machine)
            where TMachine : MonoBehaviour
            where TStateID : System.IComparable
            where TMessage : System.IComparable
        {
            var stack = GetStack(machine);

            if (stack.Count == 0)
                return default;

            return (TStateID)stack.Peek();
        }

        // ---------------------------------------------------------------------
        // Stack Depth
        // ---------------------------------------------------------------------
        /// <summary>
        /// Returns the number of paused states currently stored. This is useful
        /// for debugging, UI indicators, or validating nested flows.
        /// </summary>
        /// <typeparam name="TMachine">The concrete machine component type.</typeparam>
        /// <typeparam name="TStateID">The state identifier type.</typeparam>
        /// <typeparam name="TMessage">The message identifier type.</typeparam>
        /// <param name="machine">The state machine to query.</param>
        /// <returns>The number of paused states on the stack.</returns>
        public static int StackDepth<TMachine, TStateID, TMessage>(
            this IStateMachine<TMachine, TStateID, TMessage> machine)
            where TMachine : MonoBehaviour
            where TStateID : System.IComparable
            where TMessage : System.IComparable
        {
            return GetStack(machine).Count;
        }

        // ---------------------------------------------------------------------
        // Replace Top And Push
        // ---------------------------------------------------------------------
        /// <summary>
        /// Pushes the current state onto the stack, then replaces the active
        /// state with a new one, preserving the full return path.
        /// </summary>
        /// <remarks>
        /// Useful for nested overlays such as:
        /// Pause -&gt; Settings -&gt; Controls -&gt; Back -&gt; Settings -&gt; Pop -&gt; Gameplay.
        /// </remarks>
        /// <typeparam name="TMachine">The concrete machine component type.</typeparam>
        /// <typeparam name="TStateID">The state identifier type.</typeparam>
        /// <typeparam name="TMessage">The message identifier type.</typeparam>
        /// <param name="machine">The state machine to transition.</param>
        /// <param name="newState">The ID of the state to enter.</param>
        /// <param name="args">Arguments forwarded to the transition callbacks.</param>
        /// <returns>True if the transition happened; otherwise false.</returns>
        public static bool ReplaceTopAndPush<TMachine, TStateID, TMessage>(
            this IStateMachine<TMachine, TStateID, TMessage> machine,
            TStateID newState,
            params object[] args)
            where TMachine : MonoBehaviour
            where TStateID : System.IComparable
            where TMessage : System.IComparable
        {
            var stack = GetStack(machine);

            if (machine.HasState)
                stack.Push(machine.CurrentStateType);

            return machine.ChangeState(newState, args);
        }
    }
}
