// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

using System;
using System.Collections.Generic;

namespace XFG.Blackboard
{
    /// <summary>
    /// Minimal runtime blackboard API exposed to knowledge sources.
    /// </summary>
    public interface IBlackboard<TValue>
    {
        /// <summary>
        /// Returns true if the blackboard contains the given key.
        /// </summary>
        /// <param name="key">The key to look up.</param>
        /// <returns>True if the key exists; otherwise, false.</returns>
        bool Has(string key);

        /// <summary>
        /// Returns the value for the given key, or default if missing.
        /// </summary>
        /// <param name="key">The key to look up.</param>
        /// <returns>The stored value, or default if the key is missing.</returns>
        TValue Get(string key);

        /// <summary>
        /// Attempts to retrieve a value without throwing.
        /// </summary>
        /// <param name="key">The key to look up.</param>
        /// <param name="value">The stored value, or default if the key is missing.</param>
        /// <returns>True if the key exists; otherwise, false.</returns>
        bool TryGet(string key, out TValue value);

        /// <summary>
        /// Sets or replaces a key/value pair.
        /// </summary>
        /// <param name="key">The key to set.</param>
        /// <param name="value">The value to store.</param>
        void Set(string key, TValue value);

        /// <summary>
        /// Removes a key/value pair if present.
        /// </summary>
        /// <param name="key">The key to remove.</param>
        /// <returns>True if the key was removed; otherwise, false.</returns>
        bool Remove(string key);

        /// <summary>
        /// Returns a read-only snapshot of all key/value pairs.
        /// </summary>
        /// <returns>A copy of the stored data, unaffected by later changes.</returns>
        IReadOnlyDictionary<string, TValue> Snapshot();
    }

    /// <summary>
    /// A knowledge source (KS) is a small logic module that reads/writes
    /// blackboard state and executes only when its preconditions are met.
    /// </summary>
    public interface IKnowledgeSource<TValue>
    {
        /// <summary>
        /// Execution priority. Higher values run first.
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// Checks whether this knowledge source's preconditions are met.
        /// </summary>
        /// <param name="bb">The blackboard to inspect.</param>
        /// <returns>True if the knowledge source should execute; otherwise, false.</returns>
        bool CanExecute(IBlackboard<TValue> bb);

        /// <summary>
        /// Runs this knowledge source against the blackboard.
        /// </summary>
        /// <param name="bb">The blackboard to read from and write to.</param>
        void Execute(IBlackboard<TValue> bb);
    }

    /// <summary>
    /// Optional base class for KS implementations.
    /// Provides a consistent structure for priority and execution.
    /// </summary>
    public abstract class KnowledgeSource<TValue> : IKnowledgeSource<TValue>
    {
        /// <summary>
        /// Execution priority. Higher values run first.
        /// </summary>
        public abstract int Priority { get; }

        /// <summary>
        /// Checks whether this knowledge source's preconditions are met.
        /// </summary>
        /// <param name="bb">The blackboard to inspect.</param>
        /// <returns>True if the knowledge source should execute; otherwise, false.</returns>
        public abstract bool CanExecute(IBlackboard<TValue> bb);

        /// <summary>
        /// Runs this knowledge source against the blackboard.
        /// </summary>
        /// <param name="bb">The blackboard to read from and write to.</param>
        public abstract void Execute(IBlackboard<TValue> bb);
    }

    /// <summary>
    /// Core blackboard implementation shared by world-level and agent-level blackboards.
    /// Stores key/value data, executes knowledge sources, and supports save/load.
    /// </summary>
    /// <remarks>
    /// Provides:
    /// <list type="bullet">
    /// <item>Key/value storage</item>
    /// <item>Priority-based knowledge source execution</item>
    /// <item>Save/load support</item>
    /// </list>
    /// Engine-agnostic. No Unity or Godot dependencies.
    /// </remarks>
    public class BlackboardCore<TValue> : IBlackboard<TValue>
    {
        /// <summary>
        /// Backing key/value storage.
        /// </summary>
        protected readonly Dictionary<string, TValue> _data =
            new Dictionary<string, TValue>();

        /// <summary>
        /// Registered knowledge sources, sorted by descending priority.
        /// </summary>
        protected readonly List<IKnowledgeSource<TValue>> _knowledgeSources =
            new List<IKnowledgeSource<TValue>>();

        // Immutable execution order used by Run; rebuilt when sources change
        private IKnowledgeSource<TValue>[] _runOrder;

        /// <summary>
        /// Returns true if the blackboard contains the given key.
        /// </summary>
        /// <param name="key">The key to look up.</param>
        /// <returns>True if the key exists; otherwise, false.</returns>
        public bool Has(string key) => _data.ContainsKey(key);

        /// <summary>
        /// Returns the value for the given key, or default if missing.
        /// </summary>
        /// <param name="key">The key to look up.</param>
        /// <returns>The stored value, or default if the key is missing.</returns>
        public TValue Get(string key) =>
            _data.TryGetValue(key, out var value) ? value : default;

        /// <summary>
        /// Attempts to retrieve a value without throwing.
        /// </summary>
        /// <param name="key">The key to look up.</param>
        /// <param name="value">The stored value, or default if the key is missing.</param>
        /// <returns>True if the key exists; otherwise, false.</returns>
        public bool TryGet(string key, out TValue value) =>
            _data.TryGetValue(key, out value);

        /// <summary>
        /// Sets or replaces a key/value pair.
        /// </summary>
        /// <param name="key">The key to set.</param>
        /// <param name="value">The value to store.</param>
        public void Set(string key, TValue value) =>
            _data[key] = value;

        /// <summary>
        /// Removes a key/value pair if present.
        /// </summary>
        /// <param name="key">The key to remove.</param>
        /// <returns>True if the key was removed; otherwise, false.</returns>
        public bool Remove(string key) =>
            _data.Remove(key);

        /// <summary>
        /// Returns a read-only snapshot of all key/value pairs.
        /// </summary>
        /// <returns>A copy of the stored data, unaffected by later changes.</returns>
        public IReadOnlyDictionary<string, TValue> Snapshot() => new Dictionary<string, TValue>(_data);

        /// <summary>
        /// Registers a knowledge source and keeps the list sorted by descending priority.
        /// </summary>
        /// <param name="ks">The knowledge source to register.</param>
        public void AddKnowledgeSource(IKnowledgeSource<TValue> ks)
        {
            if (ks == null) throw new ArgumentNullException(nameof(ks));
            _knowledgeSources.Add(ks);
            _knowledgeSources.Sort((a, b) => b.Priority.CompareTo(a.Priority));
            _runOrder = null;
        }

        /// <summary>
        /// Executes all knowledge sources in descending priority order.
        /// Each KS runs at most once per call.
        /// </summary>
        public void Run()
        {
            // Iterate a fixed copy so sources added during Run don't shift indices;
            // they take effect on the next call
            var order = _runOrder;
            if (order == null || order.Length != _knowledgeSources.Count)
                _runOrder = order = _knowledgeSources.ToArray();

            for (int i = 0; i < order.Length; i++)
            {
                var ks = order[i];
                if (ks.CanExecute(this))
                    ks.Execute(this);
            }
        }

        /// <summary>
        /// Serializable container for saving blackboard state.
        /// </summary>
        [Serializable]
        public class BlackboardSaveData
        {
            /// <summary>
            /// Saved keys, index-aligned with <see cref="Values"/>.
            /// </summary>
            public List<string> Keys = new List<string>();

            /// <summary>
            /// Saved values, index-aligned with <see cref="Keys"/>.
            /// </summary>
            public List<TValue> Values = new List<TValue>();
        }

        /// <summary>
        /// Captures the current blackboard state into a serializable DTO.
        /// </summary>
        /// <returns>A snapshot of the current key/value pairs.</returns>
        public BlackboardSaveData ToSaveData()
        {
            var save = new BlackboardSaveData();

            foreach (var kvp in _data)
            {
                save.Keys.Add(kvp.Key);
                save.Values.Add(kvp.Value);
            }

            return save;
        }

        /// <summary>
        /// Restores blackboard state from a previously captured snapshot.
        /// </summary>
        /// <param name="save">The snapshot to restore. Existing data is cleared first.</param>
        /// <exception cref="ArgumentNullException"><paramref name="save"/> or its lists are null.</exception>
        /// <exception cref="ArgumentException">The key and value counts differ.</exception>
        public void LoadFromSaveData(BlackboardSaveData save)
        {
            // Validate before clearing so a bad save leaves the current state intact
            if (save == null) throw new ArgumentNullException(nameof(save));
            if (save.Keys == null || save.Values == null)
                throw new ArgumentNullException(nameof(save), "Save data Keys and Values must not be null.");
            if (save.Keys.Count != save.Values.Count)
                throw new ArgumentException(
                    $"Save data has {save.Keys.Count} keys but {save.Values.Count} values.", nameof(save));

            _data.Clear();

            for (int i = 0; i < save.Keys.Count; i++)
                _data[save.Keys[i]] = save.Values[i];
        }
    }
}
