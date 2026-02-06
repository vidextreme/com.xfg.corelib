// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace XFG.AI.FSM.Async
{
    /// <summary>
    /// Async/await extension layer for IStateMachine.
    ///
    /// This adds asynchronous state transitions with cancellation support
    /// without modifying the original state machine. States may optionally
    /// implement IAsyncState to participate in async transitions.
    ///
    /// IMPLEMENTATION OVERVIEW:
    /// -------------------------
    /// • Async transitions wrap the synchronous ChangeState call.
    /// • Each transition receives a cancellation token.
    /// • A transition token prevents race conditions (late completions).
    /// • Only states that implement IAsyncState receive async callbacks.
    ///
    /// This design keeps the base FSM simple while enabling async workflows
    /// such as UI fades, ability warm-ups, network handshakes, and scripted
    /// sequences.
    /// </summary>
    public static class AsyncStateMachineExtensions
    {
        // --------------------------------------------------------------------
        //  ASYNC STATE INTERFACE
        // --------------------------------------------------------------------

        /// <summary>
        /// Optional interface for states that support async enter/exit.
        /// </summary>
        public interface IAsyncState<TMachineType, TStateIDType, TMessageType> :
            IStateMachine<TMachineType, TStateIDType, TMessageType>.IState
            where TMachineType : MonoBehaviour
            where TStateIDType : IComparable
            where TMessageType : IComparable
        {
            Task OnStateEnterAsync(
                TStateIDType prevStateType,
                object[] args,
                CancellationToken token
            ) => Task.CompletedTask;

            Task OnStateExitAsync(
                TStateIDType nextStateType,
                object[] args,
                CancellationToken token
            ) => Task.CompletedTask;
        }


        // --------------------------------------------------------------------
        //  INTERNAL TRANSITION TRACKING
        // --------------------------------------------------------------------

        /// <summary>
        /// Tracks supersession tokens to prevent race conditions.
        /// </summary>
        private static readonly Dictionary<object, int> _transitionTokens =
            new Dictionary<object, int>();

        /// <summary>
        /// Tracks cancellation token sources for in-flight transitions.
        /// </summary>
        private static readonly Dictionary<object, CancellationTokenSource> _ctsMap =
            new Dictionary<object, CancellationTokenSource>();


        // --------------------------------------------------------------------
        //  ASYNC CHANGE STATE
        // --------------------------------------------------------------------

        /// <summary>
        /// Performs an asynchronous state transition with cancellation support.
        /// </summary>
        public static async Task<bool> ChangeStateAsync<TMachineType, TStateIDType, TMessageType>(
            this IStateMachine<TMachineType, TStateIDType, TMessageType> machine,
            TStateIDType nextStateType,
            CancellationToken externalToken = default,
            params object[] args)
            where TMachineType : MonoBehaviour
            where TStateIDType : IComparable
            where TMessageType : IComparable
        {
            // Initialize tracking
            if (!_transitionTokens.ContainsKey(machine))
                _transitionTokens[machine] = 0;

            if (!_ctsMap.ContainsKey(machine))
                _ctsMap[machine] = new CancellationTokenSource();

            // Cancel any previous transition
            _ctsMap[machine].Cancel();
            _ctsMap[machine] = new CancellationTokenSource();

            // Link external + internal cancellation
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                externalToken,
                _ctsMap[machine].Token
            );

            var token = linkedCts.Token;
            int myToken = ++_transitionTokens[machine];

            // Skip if already in this state
            if (machine.HasState &&
                machine.CurrentStateType.CompareTo(nextStateType) == 0)
                return false;

            // Retrieve next state
            if (!machine.TryGetState(nextStateType, out var nextStateBase))
            {
                Debug.LogWarning($"{nextStateType} is not registered in {machine.gameObject.name}");
                return false;
            }

            var prevState = machine.CurrentState as IAsyncState<TMachineType, TStateIDType, TMessageType>;
            var nextState = nextStateBase as IAsyncState<TMachineType, TStateIDType, TMessageType>;
            var prevStateType = machine.CurrentStateType;

            // Exit old state
            if (prevState != null)
                await prevState.OnStateExitAsync(nextStateType, args, token);

            // Check for supersession or cancellation
            if (token.IsCancellationRequested || myToken != _transitionTokens[machine])
                return false;

            // Perform synchronous transition
            machine.ChangeState(nextStateType, args);

            // Enter new state
            if (nextState != null)
                await nextState.OnStateEnterAsync(prevStateType, args, token);

            // Final supersession check
            return !token.IsCancellationRequested &&
                   myToken == _transitionTokens[machine];
        }


        // --------------------------------------------------------------------
        //  SAFE STATE RETRIEVAL
        // --------------------------------------------------------------------

        /// <summary>
        /// Retrieves a registered state without exposing internal storage.
        /// </summary>
        public static bool TryGetState<TMachineType, TStateIDType, TMessageType>(
            this IStateMachine<TMachineType, TStateIDType, TMessageType> machine,
            TStateIDType id,
            out IStateMachine<TMachineType, TStateIDType, TMessageType>.IState state)
            where TMachineType : MonoBehaviour
            where TStateIDType : IComparable
            where TMessageType : IComparable
        {
            // Access the private dictionary via reflection (non-breaking)
            var field = machine.GetType().GetField("_states",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);

            var dict = field?.GetValue(machine) as IDictionary;

            if (dict != null && dict.Contains(id))
            {
                state = (IStateMachine<TMachineType, TStateIDType, TMessageType>.IState)dict[id];
                return true;
            }

            state = null;
            return false;
        }
    }
}
