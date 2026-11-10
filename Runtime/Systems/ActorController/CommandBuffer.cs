// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

using System;
using System.Collections.Generic;

namespace XFG.ActorController
{
    /// <summary>
    /// A lightweight, strongly-typed FIFO buffer for Actor commands. Controllers
    /// enqueue commands immediately, and the Actor processes them deterministically
    /// during its update cycle.
    /// </summary>
    /// <remarks>
    /// This ensures:
    /// <list type="bullet">
    /// <item>Stable, predictable state transitions</item>
    /// <item>No mid-frame command execution</item>
    /// <item>Identical behavior for AI, player, and network controllers</item>
    /// <item>Clean decoupling between decision-making and execution</item>
    /// </list>
    /// The buffer supports optional capacity limits, peeking, clearing,
    /// and safe dequeue operations.
    /// </remarks>
    public class CommandBuffer<TCommand, TParam>
        where TCommand : IComparable
    {
        private readonly Queue<(TCommand command, TParam parameter)> _queue;

        /// <summary>
        /// Optional maximum capacity. If null, the buffer is unbounded.
        /// </summary>
        public int? Capacity { get; }

        /// <summary>
        /// Number of commands currently stored.
        /// </summary>
        public int Count => _queue.Count;

        /// <summary>
        /// Creates a new command buffer with an optional capacity limit.
        /// </summary>
        /// <param name="capacity">The maximum number of commands, or null for an unbounded buffer.</param>
        public CommandBuffer(int? capacity = null)
        {
            Capacity = capacity;
            _queue = new Queue<(TCommand, TParam)>();
        }

        /// <summary>
        /// Enqueues a command. If capacity is reached, the oldest command
        /// is dropped to maintain buffer size.
        /// </summary>
        /// <param name="command">The command identifier.</param>
        /// <param name="parameter">The command payload.</param>
        public void Enqueue(TCommand command, TParam parameter)
        {
            if (Capacity.HasValue && _queue.Count >= Capacity.Value)
            {
                _queue.Dequeue(); // Drop oldest
            }

            _queue.Enqueue((command, parameter));
        }

        /// <summary>
        /// Attempts to dequeue the next command.
        /// </summary>
        /// <param name="command">The dequeued command, or default if the buffer is empty.</param>
        /// <param name="parameter">The dequeued payload, or default if the buffer is empty.</param>
        /// <returns>True if a command was dequeued; otherwise, false.</returns>
        public bool TryDequeue(out TCommand command, out TParam parameter)
        {
            if (_queue.Count > 0)
            {
                var item = _queue.Dequeue();
                command = item.command;
                parameter = item.parameter;
                return true;
            }

            command = default;
            parameter = default;
            return false;
        }

        /// <summary>
        /// Returns the next command without removing it.
        /// </summary>
        /// <param name="command">The next command, or default if the buffer is empty.</param>
        /// <param name="parameter">The next payload, or default if the buffer is empty.</param>
        /// <returns>True if a command is available; otherwise, false.</returns>
        public bool TryPeek(out TCommand command, out TParam parameter)
        {
            if (_queue.Count > 0)
            {
                var item = _queue.Peek();
                command = item.command;
                parameter = item.parameter;
                return true;
            }

            command = default;
            parameter = default;
            return false;
        }

        /// <summary>
        /// Removes all commands from the buffer.
        /// </summary>
        public void Clear()
        {
            _queue.Clear();
        }
    }
}
