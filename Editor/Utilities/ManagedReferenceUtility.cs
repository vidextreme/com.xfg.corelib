// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using static XFG.Core;

/// <summary>
/// Editor helpers for <c>[SerializeReference]</c> managed reference fields.
/// </summary>
/// <remarks>
/// Covers type discovery, nicified names, breadcrumbs, declared type resolution,
/// instance creation, and icon resolution for <see cref="SerializableClassAttribute"/>.
/// </remarks>
public static class ManagedReferenceUtility
{
    // -------------------------------------------------------------------------
    // Declared Type Resolution
    // -------------------------------------------------------------------------
    /// <summary>
    /// Gets the declared field type for a managed reference property, using the element type for arrays.
    /// </summary>
    /// <param name="property">The managed reference property.</param>
    /// <returns>The declared type, or <c>null</c> if it cannot be resolved.</returns>
    public static Type GetFieldDeclaredType(SerializedProperty property)
    {
        if (property == null)
            return null;

        Type hostType = property.serializedObject.targetObject.GetType();
        FieldInfo field = GetFieldInfoFromPropertyPath(hostType, property.propertyPath);
        if (field == null)
            return null;

        Type t = field.FieldType;
        if (t.IsArray)
            return t.GetElementType();

        return t;
    }

    static FieldInfo GetFieldInfoFromPropertyPath(Type host, string path)
    {
        if (host == null || string.IsNullOrEmpty(path))
            return null;

        string[] parts = path.Split('.');
        Type currentType = host;
        FieldInfo field = null;

        for (int i = 0; i < parts.Length; i++)
        {
            string name = parts[i];

            if (name == "Array" && i + 1 < parts.Length && parts[i + 1].StartsWith("data["))
            {
                i++;
                continue;
            }

            field = currentType.GetField(name,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (field == null)
                return null;

            currentType = field.FieldType;
        }

        return field;
    }

    // -------------------------------------------------------------------------
    // Type Discovery
    // -------------------------------------------------------------------------
    /// <summary>
    /// Gets all loaded types assignable to a base type, ordered by full name.
    /// </summary>
    /// <param name="baseType">The base type or interface.</param>
    /// <returns>The assignable types, or an empty sequence if <paramref name="baseType"/> is <c>null</c>.</returns>
    public static IEnumerable<Type> GetAssignableConcreteTypes(Type baseType)
    {
        if (baseType == null)
            return Enumerable.Empty<Type>();

        return AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a =>
            {
                try { return a.GetTypes(); }
                catch { return Array.Empty<Type>(); }
            })
            .Where(t =>
                t != null &&
                baseType.IsAssignableFrom(t) )
            .OrderBy(t => t.FullName);
    }

    // -------------------------------------------------------------------------
    // Instance Creation
    // -------------------------------------------------------------------------
    /// <summary>
    /// Creates an instance of a type with its parameterless constructor (null-safe).
    /// </summary>
    /// <param name="type">The type to instantiate.</param>
    /// <returns>The new instance, or <c>null</c> if <paramref name="type"/> is <c>null</c> or construction fails.</returns>
    public static object CreateInstance(Type type)
    {
        if (type == null)
            return null;

        try
        {
            return Activator.CreateInstance(type);
        }
        catch
        {
            return null;
        }
    }

    // -------------------------------------------------------------------------
    // Current Value Type
    // -------------------------------------------------------------------------
    /// <summary>
    /// Gets the runtime type stored in a managed reference property.
    /// </summary>
    /// <param name="property">The managed reference property.</param>
    /// <returns>The stored value's type, or <c>null</c> if the value is null or the type cannot be found.</returns>
    public static Type GetCurrentValueType(SerializedProperty property)
    {
        if (property == null)
            return null;

        string typeName = property.managedReferenceFullTypename;
        if (string.IsNullOrEmpty(typeName))
            return null;

        int index = typeName.IndexOf(' ');
        if (index < 0)
            return null;

        string assemblyName = typeName.Substring(0, index);
        string className = typeName.Substring(index + 1);

        Assembly asm = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name == assemblyName);

        if (asm == null)
            return null;

        return asm.GetType(className);
    }

    // -------------------------------------------------------------------------
    // Display Names
    // -------------------------------------------------------------------------
    /// <summary>
    /// Gets a display-friendly type name with generic arity removed.
    /// </summary>
    /// <param name="type">The type to name.</param>
    /// <returns>The nicified name, or "&lt;null&gt;" if <paramref name="type"/> is <c>null</c>.</returns>
    public static string GetNicifiedTypeName(Type type)
    {
        if (type == null)
            return "<null>";

        string name = type.Name;

        if (type.IsGenericType)
        {
            name = name.Split('`')[0];
        }

        return ObjectNames.NicifyVariableName(name);
    }

    // -------------------------------------------------------------------------
    // Breadcrumbs
    // -------------------------------------------------------------------------
    /// <summary>
    /// Gets the breadcrumb path for a type: its namespace parts followed by the type and its declaring types.
    /// </summary>
    /// <param name="type">The type to describe.</param>
    /// <returns>The breadcrumb names, or an empty array if <paramref name="type"/> is <c>null</c>.</returns>
    public static string[] GetBreadcrumbNames(Type type)
    {
        if (type == null)
            return Array.Empty<string>();

        List<string> parts = new List<string>();

        if (!string.IsNullOrEmpty(type.Namespace))
        {
            parts.AddRange(type.Namespace.Split('.'));
        }

        Type current = type;
        while (current != null)
        {
            parts.Add(current.Name.Split('`')[0]);
            current = current.DeclaringType;
        }

        return parts.ToArray();
    }

    // -------------------------------------------------------------------------
    // Icon Resolution
    // -------------------------------------------------------------------------
    /// <summary>
    /// Resolves the icon for a type, honoring any overrides on the attribute.
    /// </summary>
    /// <param name="type">The type to get an icon for.</param>
    /// <param name="attr">The attribute with optional icon overrides; may be <c>null</c>.</param>
    /// <returns>The icon texture, or <c>null</c> if <paramref name="type"/> is <c>null</c> or none is found.</returns>
    /// <remarks>
    /// Checks <c>CustomIcon</c>, then <c>IconName</c>, then <c>IconType</c>, then the type's default icon.
    /// </remarks>
    public static Texture GetIconForType(Type type, SerializableClassAttribute attr)
    {
        if (type == null)
            return null;

        // 1. Direct custom texture override
        if (attr != null && attr.CustomIcon != null)
            return attr.CustomIcon;

        // 2. Built-in Unity icon name
        if (attr != null && !string.IsNullOrEmpty(attr.IconName))
        {
            GUIContent content = EditorGUIUtility.IconContent(attr.IconName);
            if (content != null && content.image != null)
                return content.image;
        }

        // 3. Icon from another type
        if (attr != null && attr.IconType != null)
        {
            GUIContent content = EditorGUIUtility.ObjectContent(null, attr.IconType);
            if (content != null && content.image != null)
                return content.image;
        }

        // 4. Default icon for the type
        GUIContent def = EditorGUIUtility.ObjectContent(null, type);
        return def != null ? def.image : null;
    }
}
