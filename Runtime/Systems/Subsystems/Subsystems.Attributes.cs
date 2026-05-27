// ======================================
// XFG.Subsystems — Subsystem Attributes
// ======================================

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

        public StartupOrderAttribute(SubsystemCategory category, int offset = 0)
        {
            Order = (int)category + offset;
        }

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
        public string Id { get; }

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
        public string Description { get; }

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
        public string[] Tags { get; }

        public SubsystemTagsAttribute(params string[] tags)
        {
            Tags = tags ?? Array.Empty<string>();
        }
    }

    /// <summary>
    /// Apply this attribute to hide a field or property from the
    /// SubsystemRuntimeWindow. By default, the member is hidden only
    /// from the UI. Set IncludeInSnapshot to false to also hide it
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

        public SubsystemRuntimeIgnoreAttribute(bool includeInSnapshot = true)
        {
            IncludeInSnapshot = includeInSnapshot;
        }
    }
}



