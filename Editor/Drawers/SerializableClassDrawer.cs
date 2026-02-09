// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

// UIElements property drawer for managed reference fields marked with
// [SerializableClass]. Provides an expanding button with a type icon that
// opens the hierarchical searchable type popup, and draws the managed
// reference inline using PropertyField (no foldout, no labels).

using System;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static XFG.Core;

[CustomPropertyDrawer(typeof(SerializableClassAttribute))]
public class SerializableClassDrawer : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        var root = new VisualElement();

        if (property.propertyType != SerializedPropertyType.ManagedReference)
        {
            root.Add(new Label("Property is not a managed reference."));
            return root;
        }

        var attr = (SerializableClassAttribute)attribute;

        Type declaredType =
            attr.BaseType ??
            ManagedReferenceUtility.GetFieldDeclaredType(property);

        var assignable = ManagedReferenceUtility
            .GetAssignableConcreteTypes(declaredType)
            .ToList();

        var button = new Button();
        button.style.flexDirection = FlexDirection.Row;
        button.style.flexGrow = 1f;
        button.style.unityTextAlign = TextAnchor.MiddleLeft;

        UpdateButtonLabelAndIcon(button, property, attr);

        button.clicked += () =>
        {
            Rect rect = button.worldBound;
            SearchableTypeTreePopup.Show(rect, assignable, type =>
            {
                object instance = ManagedReferenceUtility.CreateInstance(type);

                property.serializedObject.Update();
                property.managedReferenceValue = instance;
                property.serializedObject.ApplyModifiedProperties();

                UpdateButtonLabelAndIcon(button, property, attr);
            });
        };

        root.Add(button);

        var field = new PropertyField(property);
        field.label = "";
        field.style.marginLeft = 0;
        field.style.paddingLeft = 0;
        field.Bind(property.serializedObject);

        root.Add(field);

        return root;
    }

    void UpdateButtonLabelAndIcon(Button button, SerializedProperty property, SerializableClassAttribute attr)
    {
        var type = ManagedReferenceUtility.GetCurrentValueType(property);

        string label = type != null
            ? ManagedReferenceUtility.GetNicifiedTypeName(type)
            : "<null>";

        button.contentContainer.Clear();

        Texture icon = type != null
            ? ManagedReferenceUtility.GetIconForType(type, attr)
            : null;

        if (icon != null)
        {
            var img = new Image();
            img.image = icon;
            img.scaleMode = ScaleMode.ScaleToFit;
            img.style.width = 16;
            img.style.height = 16;
            img.style.marginRight = 4;

            button.contentContainer.Add(img);
        }

        var text = new Label(label);
        text.style.flexGrow = 1f;
        text.style.unityTextAlign = TextAnchor.MiddleLeft;

        button.contentContainer.Add(text);
    }
}
