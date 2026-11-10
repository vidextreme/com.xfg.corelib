// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

using System;
using XFG.Engine;

namespace XFG
{
    /// <summary>
    /// Entry point and global coordinator for the XFG framework.
    /// </summary>
    public static partial class Core
    {
        private static IEngineAPI _engineAPI;

        /// <summary>
        /// Raised whenever the framework emits a log message.
        /// </summary>
        /// <remarks>
        /// The installed engine API already receives every message through
        /// <see cref="IEngineAPI.Log"/>; subscribe here only for additional listeners.
        /// </remarks>
        public static event Action<LogCategory, LogLevel, string> OnLog;

        /// <summary>
        /// Sets the engine abstraction used by the framework.
        /// </summary>
        /// <param name="engineAPI">The engine API implementation.</param>
        public static void SetEngineAPI(IEngineAPI engineAPI)
        {
            _engineAPI = engineAPI ?? throw new ArgumentNullException(nameof(engineAPI));
        }

        /// <summary>
        /// Gets the currently configured engine API.
        /// </summary>
        internal static IEngineAPI EngineAPI
        {
            get
            {
                if (_engineAPI == null)
                {
                    throw new InvalidOperationException("Engine API has not been set. Call Core.SetEngineAPI first.");
                }

                return _engineAPI;
            }
        }

        /// <summary>
        /// Emits a log message with the specified category and level.
        /// </summary>
        /// <param name="category">The log category.</param>
        /// <param name="level">The severity level.</param>
        /// <param name="message">The message text.</param>
        internal static void Log(LogCategory category, LogLevel level, string message)
        {
            // Only the installed engine API writes to the engine log, so replacing
            // it never duplicates output or leaks the previous instance
            _engineAPI?.Log(category, level, message);
            OnLog?.Invoke(category, level, message);
        }

        /// <summary>
        /// Emits a log message with the specified category and level using an arbitrary payload.
        /// </summary>
        /// <param name="category">The log category.</param>
        /// <param name="level">The severity level.</param>
        /// <param name="payload">The payload, converted with <c>ToString()</c>; <c>null</c> logs "null".</param>
        internal static void Log(LogCategory category, LogLevel level, object payload)
        {
            Log(category, level, payload?.ToString() ?? "null");
        }

        /// <summary>
        /// Emits an informational log message.
        /// </summary>
        /// <param name="message">The message text.</param>
        /// <param name="category">The log category. Defaults to <see cref="LogCategory.Core"/>.</param>
        public static void Info(string message, LogCategory category = LogCategory.Core)
        {
            Log(category, LogLevel.Info, message);
        }

        /// <summary>
        /// Emits a warning log message.
        /// </summary>
        /// <param name="message">The message text.</param>
        /// <param name="category">The log category. Defaults to <see cref="LogCategory.Core"/>.</param>
        public static void Warn(string message, LogCategory category = LogCategory.Core)
        {
            Log(category, LogLevel.Warning, message);
        }

        /// <summary>
        /// Emits an error log message.
        /// </summary>
        /// <param name="message">The message text.</param>
        /// <param name="category">The log category. Defaults to <see cref="LogCategory.Core"/>.</param>
        public static void Error(string message, LogCategory category = LogCategory.Core)
        {
            Log(category, LogLevel.Error, message);
        }

        /// <summary>
        /// Emits an informational log message using an arbitrary payload.
        /// </summary>
        /// <param name="payload">The payload, converted with <c>ToString()</c>.</param>
        /// <param name="category">The log category. Defaults to <see cref="LogCategory.Core"/>.</param>
        public static void Info(object payload, LogCategory category = LogCategory.Core)
        {
            Log(category, LogLevel.Info, payload);
        }

        /// <summary>
        /// Emits a warning log message using an arbitrary payload.
        /// </summary>
        /// <param name="payload">The payload, converted with <c>ToString()</c>.</param>
        /// <param name="category">The log category. Defaults to <see cref="LogCategory.Core"/>.</param>
        public static void Warn(object payload, LogCategory category = LogCategory.Core)
        {
            Log(category, LogLevel.Warning, payload);
        }

        /// <summary>
        /// Emits an error log message using an arbitrary payload.
        /// </summary>
        /// <param name="payload">The payload, converted with <c>ToString()</c>.</param>
        /// <param name="category">The log category. Defaults to <see cref="LogCategory.Core"/>.</param>
        public static void Error(object payload, LogCategory category = LogCategory.Core)
        {
            Log(category, LogLevel.Error, payload);
        }
    }
}
