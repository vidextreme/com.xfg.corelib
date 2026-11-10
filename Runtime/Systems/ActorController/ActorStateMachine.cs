// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

using System;
using UnityEngine;
using XFG.AI;

namespace XFG.ActorController
{
    /// <summary>
    /// A command-buffered wrapper around <c>IStateMachine</c> that allows Actors to
    /// receive commands from Controllers in a deterministic, frame-consistent manner.
    /// </summary>
    /// <remarks>
    /// Controllers issue commands immediately, but the Actor processes them
    /// only during <c>Update()</c>. This ensures:
    /// <list type="bullet">
    /// <item>Predictable state transitions</item>
    /// <item>No mid-frame state changes</item>
    /// <item>Identical behavior for AI, player, and network controllers</item>
    /// </list>
    /// Commands are routed to the active state only if it implements
    /// <see cref="IActorState{TCommandType}"/>.
    /// </remarks>
    public abstract class ActorStateMachine<TMachineType, TStateIDType, TMessageType, TCommandType>
        : IStateMachine<TMachineType, TStateIDType, TMessageType>,
          IActor<TCommandType>
#if UNITY_5_3_OR_NEWER
        where TMachineType : MonoBehaviour
#elif GODOT
        where TMachineType : Godot.Node
#elif MONOGAME
        where TMachineType : Microsoft.Xna.Framework.GameComponent
#else
        where TMachineType : class
#endif
        where TStateIDType : IComparable
        where TMessageType : IComparable
        where TCommandType : IComparable
    {
        // ---------------------------------------------------------------------
        // Command Buffer
        // ---------------------------------------------------------------------
        // Stores commands issued by Controllers. Commands are processed in FIFO
        // order during Update(), ensuring deterministic behavior.
        private readonly CommandBuffer<TCommandType, object> _commandBuffer =
            new CommandBuffer<TCommandType, object>();


        /// <summary>
        /// Enqueues a command issued by a Controller. Commands are processed
        /// during <c>Update()</c>, not immediately.
        /// </summary>
        /// <typeparam name="TParameterType">The type of the command parameter.</typeparam>
        /// <param name="command">The command identifier.</param>
        /// <param name="parameter">The command payload.</param>
        public void ExecuteCommand<TParameterType>(TCommandType command, TParameterType parameter)
        {
            _commandBuffer.Enqueue(command, parameter);
        }


        /// <summary>
        /// Unity update loop. Ticks the FSM (state update), then processes buffered commands.
        /// </summary>
        /// <remarks>
        /// Protected and virtual so derived Actors can extend the behavior.
        /// </remarks>
#if UNITY_5_3_OR_NEWER
        protected virtual void Update()
        {
            // Tick the FSM
            UpdateMachine();

            // Process buffered commands
            while (_commandBuffer.TryDequeue(out var cmd, out var param))
            {
                // Only Actor states implement command handling
                if (CurrentState is IActorState<TCommandType> actorState)
                {
                    actorState.ExecuteCommand(cmd, param);
                }
            }
        }
#endif
    }
}
