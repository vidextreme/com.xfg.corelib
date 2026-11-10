// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

using System;

namespace XFG.ActorController
{
    /// <summary>
    /// Represents an Actor capable of receiving typed commands.
    /// Controllers call <c>ExecuteCommand()</c> to request actions.
    /// </summary>
    /// <remarks>
    /// This interface is intentionally minimal:
    /// <list type="bullet">
    /// <item>Actors do not decide what to do.</item>
    /// <item>Actors only execute commands deterministically.</item>
    /// <item>All decision-making lives in Controllers.</item>
    /// </list>
    /// </remarks>
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
    /// </summary>
    /// <remarks>
    /// This interface allows the FSM engine to remain generic while
    /// enabling each state to define its own command-handling logic.
    /// States should:
    /// <list type="bullet">
    /// <item>Handle only the commands relevant to them.</item>
    /// <item>Trigger transitions when appropriate.</item>
    /// <item>Use <c>Inform()</c> to notify Controllers of events.</item>
    /// </list>
    /// </remarks>
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
