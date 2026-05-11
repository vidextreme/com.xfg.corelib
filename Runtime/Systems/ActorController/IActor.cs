// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

using System;

namespace XFG.ActorController
{
    /// <summary>
    /// Represents an Actor capable of receiving typed commands.
    /// Controllers call ExecuteCommand() to request actions.
    /// 
    /// This interface is intentionally minimal:
    /// - Actors do not decide what to do.
    /// - Actors only execute commands deterministically.
    /// - All decision-making lives in Controllers.
    /// </summary>
    public interface IActor<TCommandType>
        where TCommandType : IComparable
    {
        /// <summary>
        /// Executes a typed command with an optional parameter.
        /// Commands are routed to the Actor's current state.
        /// </summary>
        /// <typeparam name="TParameterType">The type of the command parameter.</typeparam>
        /// <param name="command">The command identifier.</param>
        /// <param name="parameter">The command payload.</param>
        void ExecuteCommand<TParameterType>(TCommandType command, TParameterType parameter);
    }

    /// <summary>
    /// Implemented by all Actor states that can receive commands.
    /// 
    /// This interface allows the FSM engine to remain generic while
    /// enabling each state to define its own command-handling logic.
    /// 
    /// States should:
    /// - Handle only the commands relevant to them.
    /// - Trigger transitions when appropriate.
    /// - Use Inform() to notify Controllers of events.
    /// </summary>
    public interface IActorState<TCommandType>
    {
        /// <summary>
        /// Handles a command routed to this state.
        /// </summary>
        /// <typeparam name="TParameterType">The type of the command parameter.</typeparam>
        /// <param name="command">The command identifier.</param>
        /// <param name="parameter">The command payload.</param>
        void ExecuteCommand<TParameterType>(TCommandType command, TParameterType parameter);
    }
}
