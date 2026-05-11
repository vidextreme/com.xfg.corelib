// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

using System;
using UnityEngine;
using XFG.AI.FSM.Serializable;

namespace XFG.ActorController
{
    /// <summary>
    /// ActorSerializableStateMachine
    ///
    /// Serializable version of ActorStateMachine. Uses SerializeReference to
    /// allow Unity to store polymorphic state classes while still using the
    /// runtime FSM.
    ///
    /// Adds the same command-buffered behavior as the non-serializable version.
    /// </summary>
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
        // ----------------------------------------------------------------------
        // COMMAND BUFFER
        // ----------------------------------------------------------------------

        private readonly CommandBuffer<TCommandType, object> _commandBuffer =
            new CommandBuffer<TCommandType, object>();


        /// <summary>
        /// Enqueues a command issued by a Controller.
        /// </summary>
        public void ExecuteCommand<TParameterType>(TCommandType command, TParameterType parameter)
        {
            _commandBuffer.Enqueue(command, parameter);
        }


        /// <summary>
        /// Unity Update Loop
        ///
        /// Identical to the non-serializable version.
        /// </summary>
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
    ///
    /// States must implement ExecuteCommand to handle incoming commands.
    /// </summary>
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
        public abstract void ExecuteCommand<TParameterType>(TCommandType command, TParameterType parameter);
    }
}
