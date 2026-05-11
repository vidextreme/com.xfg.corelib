// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

using System;
using UnityEngine;
using XFG.AI;

namespace XFG.ActorController
{
    /// <summary>
    /// ActorStateMachine
    ///
    /// A command-buffered wrapper around IStateMachine that allows Actors to
    /// receive commands from Controllers in a deterministic, frame-consistent
    /// manner.
    ///
    /// Controllers issue commands immediately, but the Actor processes them
    /// only during Update(). This ensures:
    /// - Predictable state transitions
    /// - No mid-frame state changes
    /// - Identical behavior for AI, player, and network controllers
    ///
    /// Commands are routed to the active state only if it implements
    /// IActorState{TCommandType}.
    /// </summary>
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
        // ----------------------------------------------------------------------
        // COMMAND BUFFER
        // ----------------------------------------------------------------------
        // Stores commands issued by Controllers. Commands are processed in FIFO
        // order during Update(), ensuring deterministic behavior.
        // ----------------------------------------------------------------------

        private readonly CommandBuffer<TCommandType, object> _commandBuffer =
            new CommandBuffer<TCommandType, object>();


        /// <summary>
        /// Enqueues a command issued by a Controller. Commands are processed
        /// during Update(), not immediately.
        /// </summary>
        public void ExecuteCommand<TParameterType>(TCommandType command, TParameterType parameter)
        {
            _commandBuffer.Enqueue(command, parameter);
        }


        /// <summary>
        /// Unity Update Loop
        ///
        /// 1. Ticks the FSM (state update)
        /// 2. Processes buffered commands
        ///
        /// Made protected + virtual so derived Actors can extend behavior.
        /// </summary>
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
