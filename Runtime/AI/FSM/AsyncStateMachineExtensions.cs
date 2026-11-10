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
    /// </summary>
    /// <remarks>
    /// This adds asynchronous state transitions with cancellation support
    /// without modifying the original state machine. States may optionally
    /// implement IAsyncState to participate in async transitions.
    ///
    /// Implementation overview:
    /// <list type="bullet">
    /// <item><description>Async transitions wrap the synchronous ChangeState call.</description></item>
    /// <item><description>Each transition receives a cancellation token.</description></item>
    /// <item><description>A transition token prevents race conditions (late completions).</description></item>
    /// <item><description>Only states that implement IAsyncState receive async callbacks.</description></item>
    /// </list>
    ///
    /// This design keeps the base FSM simple while enabling async workflows
    /// such as UI fades, ability warm-ups, network handshakes, and scripted
    /// sequences.
    /// </remarks>
    public static class AsyncStateMachineExtensions
    {
        // ---------------------------------------------------------------------
        // Async State Interface
        // ---------------------------------------------------------------------
        /// <summary>
        /// Optional interface for states that support async enter/exit.
        /// </summary>
        /// <typeparam name="TMachineType">The concrete machine component type.</typeparam>
        /// <typeparam name="TStateIDType">The state identifier type.</typeparam>
        /// <typeparam name="TMessageType">The message identifier type.</typeparam>
        public interface IAsyncState<TMachineType, TStateIDType, TMessageType> :
            IStateMachine<TMachineType, TStateIDType, TMessageType>.IState
            where TMachineType : MonoBehaviour
            where TStateIDType : IComparable
            where TMessageType : IComparable
        {
            /// <summary>
            /// Called asynchronously when entering this state, after the synchronous transition.
            /// </summary>
            /// <param name="prevStateType">The ID of the state that was exited.</param>
            /// <param name="args">Arguments passed to the transition.</param>
            /// <param name="token">Cancelled when the transition is superseded or cancelled externally.</param>
            /// <returns>A task that completes when the enter logic finishes.</returns>
            Task OnStateEnterAsync(
                TStateIDType prevStateType,
                object[] args,
                CancellationToken token
            ) => Task.CompletedTask;

            /// <summary>
            /// Called asynchronously when exiting this state, before the synchronous transition.
            /// </summary>
            /// <param name="nextStateType">The ID of the state being entered.</param>
            /// <param name="args">Arguments passed to the transition.</param>
            /// <param name="token">Cancelled when the transition is superseded or cancelled externally.</param>
            /// <returns>A task that completes when the exit logic finishes.</returns>
            Task OnStateExitAsync(
                TStateIDType nextStateType,
                object[] args,
                CancellationToken token
            ) => Task.CompletedTask;
        }

        // ---------------------------------------------------------------------
        // Internal Transition Tracking
        // ---------------------------------------------------------------------
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

        // ---------------------------------------------------------------------
        // Async Change State
        // ---------------------------------------------------------------------
        /// <summary>
        /// Performs an asynchronous state transition with cancellation support.
        /// Starting a new transition cancels any transition already in flight on the same machine.
        /// </summary>
        /// <typeparam name="TMachineType">The concrete machine component type.</typeparam>
        /// <typeparam name="TStateIDType">The state identifier type.</typeparam>
        /// <typeparam name="TMessageType">The message identifier type.</typeparam>
        /// <param name="machine">The state machine to transition.</param>
        /// <param name="nextStateType">The ID of the state to enter.</param>
        /// <param name="externalToken">Optional token that cancels the transition from outside.</param>
        /// <param name="args">Arguments forwarded to the exit and enter callbacks.</param>
        /// <returns>True if the transition completed without being cancelled or superseded; otherwise false.</returns>
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

            // Link external and internal cancellation
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

        // ---------------------------------------------------------------------
        // Safe State Retrieval
        // ---------------------------------------------------------------------
        /// <summary>
        /// Retrieves a registered state without exposing internal storage.
        /// </summary>
        /// <typeparam name="TMachineType">The concrete machine component type.</typeparam>
        /// <typeparam name="TStateIDType">The state identifier type.</typeparam>
        /// <typeparam name="TMessageType">The message identifier type.</typeparam>
        /// <param name="machine">The state machine to query.</param>
        /// <param name="id">The ID of the state to find.</param>
        /// <param name="state">The registered state, or null if not found.</param>
        /// <returns>True if a state with <paramref name="id"/> is registered; otherwise false.</returns>
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
