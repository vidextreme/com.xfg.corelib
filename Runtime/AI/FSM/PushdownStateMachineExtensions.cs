// ------------------------------------------------------------------------------
// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.
// ------------------------------------------------------------------------------
//
// PushdownStateMachineExtensions
// Stack-based state transitions for IStateMachine.
//
// This module adds pushdown (stack-driven) behavior on top of the base FSM
// without modifying the core IStateMachine implementation. It enables temporary
// overlays, interruptible states, nested UI flows, and clean return-to-previous
// behavior.
//
// The pushdown layer is implemented entirely as extension methods. Each machine
// instance maintains its own stack of paused states. Only the top state is
// active; all others remain suspended until popped.
//
// ------------------------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;

namespace XFG.AI.FSM.Pushdown
{
    public static class PushdownStateMachineExtensions
    {
        // ----------------------------------------------------------------------
        // INTERNAL STACK STORAGE
        // ----------------------------------------------------------------------
        // Each machine instance gets its own stack. The dictionary key is the
        // machine reference, and the value is a stack of TStateID entries.
        //
        // This design ensures:
        // - No modification to the base FSM class
        // - Multiple FSM instances can coexist safely
        // - Stack lifetime matches the machine lifetime
        // ----------------------------------------------------------------------

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

        // ----------------------------------------------------------------------
        // PUSH STATE
        // ----------------------------------------------------------------------
        // Saves the current state on the stack, then transitions into the new
        // state. This is the core pushdown operation used for overlays such as
        // Pause, Inventory, Dialogue, etc.
        // ----------------------------------------------------------------------

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

        // ----------------------------------------------------------------------
        // POP STATE
        // ----------------------------------------------------------------------
        // Restores the previous state from the stack. If the stack is empty,
        // nothing happens. This is the inverse of PushState and is used to
        // resume gameplay or return to the previous overlay.
        // ----------------------------------------------------------------------

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

        // ----------------------------------------------------------------------
        // CLEAR STACK
        // ----------------------------------------------------------------------
        // Removes all paused states. Useful when resetting gameplay, loading a
        // new scene, or ensuring no stale states remain.
        // ----------------------------------------------------------------------

        public static void ClearStateStack<TMachine, TStateID, TMessage>(
            this IStateMachine<TMachine, TStateID, TMessage> machine)
            where TMachine : MonoBehaviour
            where TStateID : System.IComparable
            where TMessage : System.IComparable
        {
            GetStack(machine).Clear();
        }

        // ----------------------------------------------------------------------
        // REPLACE STATE
        // ----------------------------------------------------------------------
        // Replaces the active state without modifying the stack. This is useful
        // for switching overlays (Pause -> Settings) while preserving the return
        // path (Settings -> Pause -> Gameplay).
        // ----------------------------------------------------------------------

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

        // ----------------------------------------------------------------------
        // PEEK STATE
        // ----------------------------------------------------------------------
        // Returns the state that would be restored by PopState(), without
        // removing it from the stack. Useful for UI previews or debugging.
        // ----------------------------------------------------------------------

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

        // ----------------------------------------------------------------------
        // STACK DEPTH
        // ----------------------------------------------------------------------
        // Returns the number of paused states currently stored. This is useful
        // for debugging, UI indicators, or validating nested flows.
        // ----------------------------------------------------------------------

        public static int StackDepth<TMachine, TStateID, TMessage>(
            this IStateMachine<TMachine, TStateID, TMessage> machine)
            where TMachine : MonoBehaviour
            where TStateID : System.IComparable
            where TMessage : System.IComparable
        {
            return GetStack(machine).Count;
        }

        // ----------------------------------------------------------------------
        // REPLACE TOP AND PUSH
        // ----------------------------------------------------------------------
        // Pushes the current state onto the stack, then replaces the active
        // state with a new one. This is useful for nested overlays such as:
        //
        // Pause -> Settings -> Controls -> Back -> Settings -> Pop -> Gameplay
        //
        // It preserves the full return path while allowing the top state to be
        // replaced cleanly.
        // ----------------------------------------------------------------------

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
