// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

using System;

namespace XFG.Subsystems
{
    /// <summary>
    /// Specifies the startup order for a subsystem.
    /// </summary>
    /// <remarks>
    /// The final order is computed as:
    ///     (int)category + offset
    /// OR as the explicit order value when using the integer constructor.
    ///
    /// This is the sole ordering mechanism used by the framework.
    /// Duplicate final orders are not allowed.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public sealed class StartupOrderAttribute : Attribute
    {
        /// <summary>
        /// The computed startup order (category base + offset, or explicit value).
        /// </summary>
        public int Order { get; }

        /// <summary>
        /// Sets the startup order from a category base value plus an offset.
        /// </summary>
        /// <param name="category">The category whose value is used as the base order.</param>
        /// <param name="offset">The offset added to the category base.</param>
        public StartupOrderAttribute(SubsystemCategory category, int offset = 0)
        {
            Order = (int)category + offset;
        }

        /// <summary>
        /// Sets an explicit startup order.
        /// </summary>
        /// <param name="order">The explicit startup order value.</param>
        public StartupOrderAttribute(int order)
        {
            Order = order;
        }
    }

    /// <summary>
    /// Specifies a stable identifier for a subsystem.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public sealed class SubsystemIdAttribute : Attribute
    {
        /// <summary>
        /// The stable subsystem identifier.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Creates the attribute with the given identifier.
        /// </summary>
        /// <param name="id">The stable subsystem identifier.</param>
        public SubsystemIdAttribute(string id)
        {
            Id = id;
        }
    }

    /// <summary>
    /// Provides a human-readable description for a subsystem.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public sealed class SubsystemDescriptionAttribute : Attribute
    {
        /// <summary>
        /// The human-readable description.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Creates the attribute with the given description.
        /// </summary>
        /// <param name="description">The human-readable description.</param>
        public SubsystemDescriptionAttribute(string description)
        {
            Description = description;
        }
    }

    /// <summary>
    /// Specifies tags for a subsystem.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public sealed class SubsystemTagsAttribute : Attribute
    {
        /// <summary>
        /// The tags assigned to the subsystem. Never null.
        /// </summary>
        public string[] Tags { get; }

        /// <summary>
        /// Creates the attribute with the given tags.
        /// </summary>
        /// <param name="tags">The tags to assign. Null is treated as an empty array.</param>
        public SubsystemTagsAttribute(params string[] tags)
        {
            Tags = tags ?? Array.Empty<string>();
        }
    }

    /// <summary>
    /// Apply this attribute to hide a field or property from the
    /// SubsystemRuntimeWindow. By default, the member is hidden only
    /// from the UI. Set <see cref="IncludeInSnapshot"/> to false to also hide it
    /// from the clipboard snapshot.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class SubsystemRuntimeIgnoreAttribute : Attribute
    {
        /// <summary>
        /// If false, the member will also be excluded from the
        /// clipboard snapshot. Default is true.
        /// </summary>
        public bool IncludeInSnapshot { get; }

        /// <summary>
        /// Creates the attribute.
        /// </summary>
        /// <param name="includeInSnapshot">If false, the member is also excluded from the clipboard snapshot.</param>
        public SubsystemRuntimeIgnoreAttribute(bool includeInSnapshot = true)
        {
            IncludeInSnapshot = includeInSnapshot;
        }
    }
}
