// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

using System;
using UnityEngine;
using XFG.AI.FSM.Serializable;

namespace XFG.ActorController
{
    /// <summary>
    /// Serializable version of <see cref="ActorStateMachine{TMachineType, TStateIDType, TMessageType, TCommandType}"/>.
    /// Uses <c>SerializeReference</c> so Unity can store polymorphic state classes
    /// while still using the runtime FSM.
    /// </summary>
    /// <remarks>
    /// Adds the same command-buffered behavior as the non-serializable version.
    /// </remarks>
    public abstract class ActorSerializableStateMachine<TMachineType, TStateIDType, TCommandType, TMessageType>
        : ISerializableStateMachine<TMachineType,
                                    ActorSerializableState<TMachineType, TStateIDType, TCommandType, TMessageType>,
                                    TStateIDType,
                                    TMessageType>,
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
        /// Unity update loop. Ticks the FSM, then processes buffered commands.
        /// </summary>
        /// <remarks>
        /// Identical to the non-serializable version.
        /// </remarks>
#if UNITY_5_3_OR_NEWER
        protected virtual void Update()
        {
            UpdateMachine();

            while (_commandBuffer.TryDequeue(out var cmd, out var param))
            {
                if (CurrentState is IActorState<TCommandType> actorState)
                {
                    actorState.ExecuteCommand(cmd, param);
                }
            }
        }
#endif
    }


    /// <summary>
    /// Base class for all serializable Actor states.
    /// </summary>
    /// <remarks>
    /// States must implement <c>ExecuteCommand</c> to handle incoming commands.
    /// </remarks>
    [Serializable]
    public abstract class ActorSerializableState<TMachineType, TStateIDType, TCommandType, TMessageType>
        : ActorSerializableStateMachine<TMachineType, TStateIDType, TCommandType, TMessageType>.ISerializableState,
          IActorState<TCommandType>
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
        /// <summary>
        /// Handles a command routed to this state.
        /// </summary>
        /// <typeparam name="TParameterType">The type of the command parameter.</typeparam>
        /// <param name="command">The command identifier.</param>
        /// <param name="parameter">The command payload.</param>
        public abstract void ExecuteCommand<TParameterType>(TCommandType command, TParameterType parameter);
    }
}
