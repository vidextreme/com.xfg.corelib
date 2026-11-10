// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

namespace XFG
{
    /// <summary>
    /// Logical categories for log messages emitted by the XFG framework.
    /// </summary>
    public enum LogCategory
    {
        /// <summary>
        /// Core framework messages.
        /// </summary>
        Core,

        /// <summary>
        /// Engine integration messages.
        /// </summary>
        Engine,

        /// <summary>
        /// World group–level messages.
        /// </summary>
        Group,

        /// <summary>
        /// World-level messages.
        /// </summary>
        World,

        /// <summary>
        /// Subsystem lifecycle and behavior messages.
        /// </summary>
        Subsystem,

        /// <summary>
        /// Dependency resolution and injection messages.
        /// </summary>
        Dependency,

        /// <summary>
        /// Tick and FixedTick related messages.
        /// </summary>
        Tick,

        /// <summary>
        /// User-defined or application-level messages.
        /// </summary>
        User
    }

    /// <summary>
    /// Severity level for log messages.
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// Informational messages that describe normal operation.
        /// </summary>
        Info = 0,

        /// <summary>
        /// Messages that indicate a potential problem or unexpected state.
        /// </summary>
        Warning = 1,

        /// <summary>
        /// Messages that indicate an error or failure condition.
        /// </summary>
        Error = 2
    }
}
