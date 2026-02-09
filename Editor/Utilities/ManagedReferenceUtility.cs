// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

// Utility methods for managed reference handling, type discovery,
// nicified names, breadcrumbs, declared type resolution, instance
// creation, and icon resolution for SerializableClassAttribute.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using static XFG.Core;

public static class ManagedReferenceUtility
{
    // ---------------------------------------------------------------------
    // Get the declared field type for a managed reference property.
    // ---------------------------------------------------------------------
    public static Type GetFieldDeclaredType(SerializedProperty property)
    {
        if (property == null)
            return null;

        Type hostType = property.serializedObject.targetObject.GetType();
        FieldInfo field = GetFieldInfoFromPropertyPath(hostType, property.propertyPath);
        if (field == null)
            return null;

        return field.FieldType;
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

    // ---------------------------------------------------------------------
    // Get all concrete types assignable to a base type.
    // ---------------------------------------------------------------------
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
                baseType.IsAssignableFrom(t) &&
                !t.IsAbstract &&
                !t.IsInterface &&
                t.GetConstructor(Type.EmptyTypes) != null)
            .OrderBy(t => t.FullName);
    }

    // ---------------------------------------------------------------------
    // Create an instance of a type (null-safe).
    // ---------------------------------------------------------------------
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

    // ---------------------------------------------------------------------
    // Get the runtime type stored in a managed reference property.
    // ---------------------------------------------------------------------
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

    // ---------------------------------------------------------------------
    // Nicify type name for display.
    // ---------------------------------------------------------------------
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

    // ---------------------------------------------------------------------
    // Breadcrumbs for namespace and nested types.
    // ---------------------------------------------------------------------
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

    // ---------------------------------------------------------------------
    // Icon resolution for SerializableClassAttribute.
    // ---------------------------------------------------------------------
    public static Texture GetIconForType(Type type, SerializableClassAttribute attr)
    {
        if (type == null)
            return null;

        // 1. Direct custom texture override.
        if (attr != null && attr.CustomIcon != null)
            return attr.CustomIcon;

        // 2. Built-in Unity icon name.
        if (attr != null && !string.IsNullOrEmpty(attr.IconName))
        {
            GUIContent content = EditorGUIUtility.IconContent(attr.IconName);
            if (content != null && content.image != null)
                return content.image;
        }

        // 3. Icon from another type.
        if (attr != null && attr.IconType != null)
        {
            GUIContent content = EditorGUIUtility.ObjectContent(null, attr.IconType);
            if (content != null && content.image != null)
                return content.image;
        }

        // 4. Default icon for the type.
        GUIContent def = EditorGUIUtility.ObjectContent(null, type);
        return def != null ? def.image : null;
    }
}
