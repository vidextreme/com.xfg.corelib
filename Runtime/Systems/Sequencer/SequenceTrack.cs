// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

namespace XFG.Sequencer
{
    /// <summary>
    /// Defines how a track behaves within the sequencer.
    /// </summary>
    /// <remarks>
    /// Universal tracks always run, while Stack tracks behave like a priority
    /// stack where only the topmost track is active.
    /// </remarks>
    public enum SequenceTrackMode
    {
        Universal,
        Stack
    }

    /// <summary>
    /// Metadata describing a registered sequence track.
    /// </summary>
    /// <remarks>
    /// Tracks determine how sequences are grouped and executed.
    /// See <see cref="SequenceTrackMode"/> for the available behaviors.
    /// </remarks>
    public readonly struct SequenceTrack
    {
        /// <summary>
        /// The unique identifier of the track.
        /// </summary>
        public readonly int ID;

        /// <summary>
        /// The display name of the track.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// The behavior mode of the track.
        /// </summary>
        public readonly SequenceTrackMode Mode;

        internal SequenceTrack(int id, string name, SequenceTrackMode mode)
        {
            ID = id;
            Name = name;
            Mode = mode;
        }

        /// <summary>
        /// Gets a value indicating whether this track is valid.
        /// </summary>
        public bool IsValid => ID >= 0;

        /// <summary>
        /// Returns a string representation of the track.
        /// </summary>
        /// <returns>The track name, mode, and identifier.</returns>
        public override string ToString() => $"{Name} ({Mode}, ID={ID})";
    }
}
