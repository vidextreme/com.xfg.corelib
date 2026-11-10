// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

using System;

namespace XFG.ActorController
{
    /// <summary>
    /// A lightweight interface for all Controller types. Controllers receive
    /// notifications ("Inform") from Actors so they can react to Actor-driven events.
    /// </summary>
    /// <remarks>
    /// Typical events include:
    /// <list type="bullet">
    /// <item>State changes</item>
    /// <item>Messages</item>
    /// <item>World interactions</item>
    /// <item>AI perception updates</item>
    /// </list>
    /// Controllers do NOT drive the Actor directly. They only send commands TO the
    /// Actor, and receive information FROM the Actor. This keeps the Actor–Controller
    /// relationship clean, decoupled, and fully engine-agnostic.
    /// </remarks>
    public interface IController<TInformType>
        where TInformType : IComparable
    {
        /// <summary>
        /// Receives information from the Actor.
        /// Controllers use this to react to Actor events or world events.
        /// </summary>
        /// <param name="info">The kind of information being reported.</param>
        /// <param name="objs">Optional extra data associated with the information.</param>
        void Inform(TInformType info, params object[] objs);
    }
}
