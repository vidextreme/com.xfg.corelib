// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

using System;
using System.Collections.Generic;

namespace XFG.ActorController
{
    /// <summary>
    /// CommandBuffer
    ///
    /// A lightweight, strongly-typed FIFO buffer for Actor commands.
    /// Controllers enqueue commands immediately, and the Actor processes
    /// them deterministically during its update cycle.
    ///
    /// This ensures:
    /// - Stable, predictable state transitions
    /// - No mid-frame command execution
    /// - Identical behavior for AI, player, and network controllers
    /// - Clean decoupling between decision-making and execution
    ///
    /// The buffer supports optional capacity limits, peeking, clearing,
    /// and safe dequeue operations.
    /// </summary>
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

        public CommandBuffer(int? capacity = null)
        {
            Capacity = capacity;
            _queue = new Queue<(TCommand, TParam)>();
        }

        /// <summary>
        /// Enqueues a command. If capacity is reached, the oldest command
        /// is dropped to maintain buffer size.
        /// </summary>
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
        /// Returns true if successful.
        /// </summary>
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
