// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

namespace XFG.Blackboard
{
    /// <summary>
    /// Represents a per-entity blackboard instance.
    /// This is NOT a Unity component; it is a pure runtime blackboard
    /// that inherits all reasoning and storage behavior from <see cref="BlackboardCore{TValue}"/>.
    /// </summary>
    /// <remarks>
    /// Represents the runtime blackboard for a single agent, NPC, or entity.
    /// See <see cref="BlackboardAgentSystem{TValue}"/> for an optional driver
    /// that updates multiple agents per frame.
    /// </remarks>
    public class BlackboardAgent<TValue> : BlackboardCore<TValue>
    {
        /// <summary>
        /// Creates a new agent-level blackboard with its own runtime state.
        /// </summary>
        /// <returns>A new, empty agent blackboard.</returns>
        public static BlackboardAgent<TValue> Create()
        {
            return new BlackboardAgent<TValue>();
        }

        // Future agent-specific helpers can be added here
    }

    /// <summary>
    /// Optional entity-level system that drives blackboard reasoning.
    /// This is engine/ECS dependent and intentionally abstract.
    /// </summary>
    /// <remarks>
    /// Acts as an engine/ECS-level driver that updates multiple agents per frame.
    /// </remarks>
    public abstract class BlackboardAgentSystem<TValue>
    {
        /// <summary>
        /// Override this to iterate entities and call <c>Run()</c> on each agent.
        /// </summary>
        /// <param name="dt">Delta time, for engines that require time-based updates.</param>
        public abstract void Tick(float dt);
    }
}
