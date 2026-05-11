// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

using System;

namespace XFG.ActorController
{
    /// <summary>
    /// IController
    ///
    /// A lightweight interface for all Controller types. Controllers receive
    /// notifications ("Inform") from Actors, allowing Controllers to react to
    /// Actor-driven events such as:
    /// - State changes
    /// - Messages
    /// - World interactions
    /// - AI perception updates
    ///
    /// Controllers do NOT drive the Actor directly. They only send commands TO the
    /// Actor, and receive information FROM the Actor.
    ///
    /// This keeps the Actor–Controller relationship clean, decoupled, and fully
    /// engine-agnostic.
    /// </summary>
    public interface IController<TInformType>
        where TInformType : IComparable
    {
        /// <summary>
        /// Receives information from the Actor.
        /// Controllers use this to react to Actor events or world events.
        /// </summary>
        void Inform(TInformType info, params object[] objs);
    }
}
