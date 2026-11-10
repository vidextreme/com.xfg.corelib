// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using System.Reflection;

namespace XFG.Utilities
{
    /// <summary>
    /// Property drawer for <see cref="PolymorphicReference{T}"/> fields.
    /// </summary>
    /// <remarks>
    /// Draws an object field that only accepts objects satisfying the contract type,
    /// plus a "..." button that opens <see cref="PolymorphicPickerWindow"/>.
    /// </remarks>
    [CustomPropertyDrawer(typeof(PolymorphicReference<>), true)]
    public class PolymorphicReferenceDrawer : PropertyDrawer
    {
        private static Type GetContractType(FieldInfo field)
        {
            if (field == null)
                return null;

            Type t = field.FieldType;

            // PolymorphicReference<T>
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(PolymorphicReference<>))
                return t.GetGenericArguments()[0];

            // PolymorphicReference<T>[]
            if (t.IsArray)
            {
                Type elem = t.GetElementType();
                if (elem != null && elem.IsGenericType && elem.GetGenericTypeDefinition() == typeof(PolymorphicReference<>))
                    return elem.GetGenericArguments()[0];
            }

            // List<PolymorphicReference<T>>
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(System.Collections.Generic.List<>))
            {
                Type elem = t.GetGenericArguments()[0];
                if (elem != null && elem.IsGenericType && elem.GetGenericTypeDefinition() == typeof(PolymorphicReference<>))
                    return elem.GetGenericArguments()[0];
            }

            // Subclass of PolymorphicReference<T>
            Type bt = t.BaseType;
            if (bt != null && bt.IsGenericType && bt.GetGenericTypeDefinition() == typeof(PolymorphicReference<>))
                return bt.GetGenericArguments()[0];

            return null;
        }

        /// <summary>
        /// Draws the reference field and picker button.
        /// </summary>
        /// <param name="position">The rectangle to draw in.</param>
        /// <param name="property">The serialized <c>PolymorphicReference</c> property.</param>
        /// <param name="label">The field label.</param>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Type contractType = GetContractType(fieldInfo);
            if (contractType == null)
            {
                EditorGUI.LabelField(position, label.text, "Invalid PolymorphicReference type");
                return;
            }

            SerializedProperty objProp = property.FindPropertyRelative("_object");
            if (objProp == null)
            {
                EditorGUI.LabelField(position, label.text, "Missing _object field");
                return;
            }

            string assignedType = GetAssignedTypeName(contractType, objProp.objectReferenceValue);
            string contractKind = contractType.IsInterface ? "Interface" : "Abstract";
            string displayName = $"{label.text} ({contractKind} {contractType.Name}) -> {assignedType}";

            EditorGUI.BeginProperty(position, label, property);

            Rect fieldRect = position;
            fieldRect.width -= 22f;

            Rect buttonRect = position;
            buttonRect.x = fieldRect.xMax + 2f;
            buttonRect.width = 20f;

            // Only write when the user actually changes the field, so drawing
            // never overwrites existing data (including in multi-object editing)
            EditorGUI.showMixedValue = objProp.hasMultipleDifferentValues;
            EditorGUI.BeginChangeCheck();

            UnityEngine.Object newObj = EditorGUI.ObjectField(
                fieldRect,
                displayName,
                objProp.objectReferenceValue,
                typeof(UnityEngine.Object),
                true
            );

            bool changed = EditorGUI.EndChangeCheck();
            EditorGUI.showMixedValue = false;

            if (changed)
            {
                if (newObj == null || IsValid(contractType, newObj))
                {
                    objProp.objectReferenceValue = newObj;
                }
                else
                {
                    // Reject the invalid object and keep the previous value
                    Debug.LogWarning(
                        $"Assigned object does not implement or derive from required contract {contractType.Name}."
                    );
                }
            }

            if (GUI.Button(buttonRect, "..."))
            {
                PolymorphicPickerWindow.Open(contractType, objProp);
            }

            EditorGUI.EndProperty();
        }

        private bool IsValid(Type contractType, UnityEngine.Object obj)
        {
            if (obj == null)
                return false;

            // The contract itself is a Component type
            if (typeof(Component).IsAssignableFrom(contractType))
            {
                if (obj is Component comp)
                    return contractType.IsAssignableFrom(comp.GetType());

                if (obj is GameObject go)
                    return go.GetComponent(contractType) != null;

                return false;
            }

            // Interface / abstract class implemented by Components or ScriptableObjects
            if (obj is GameObject goObj)
            {
                // Check whether any component on the GameObject implements the contract
                var comps = goObj.GetComponents<Component>();
                foreach (var c in comps)
                {
                    if (c != null && contractType.IsAssignableFrom(c.GetType()))
                        return true;
                }
                return false;
            }

            if (obj is Component c2)
                return contractType.IsAssignableFrom(c2.GetType());

            // ScriptableObject or other UnityEngine.Object directly implementing the contract
            return contractType.IsAssignableFrom(obj.GetType());
        }

        private string GetAssignedTypeName(Type contractType, UnityEngine.Object obj)
        {
            if (obj == null)
                return "None";

            if (obj is GameObject go)
            {
                Component comp = go.GetComponent(contractType);
                return comp != null ? comp.GetType().Name : go.GetType().Name;
            }

            return obj.GetType().Name;
        }
    }
}
#endif
