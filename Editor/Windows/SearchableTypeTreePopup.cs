// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static XFG.Core;

/// <summary>
/// UIElements popup window providing a searchable hierarchical tree of types.
/// </summary>
/// <remarks>
/// A custom row template replaces Unity's default label with an icon + label row.
/// Icons come from <see cref="SerializableClassAttribute"/> metadata when present,
/// falling back to Unity's default type icon. Rows collapse cleanly when no icon
/// is available (no leftover spacing).
/// </remarks>
public class SearchableTypeTreePopup : EditorWindow
{
    Action<Type> _onSelected;
    List<Type> _allTypes;
    List<Type> _filteredTypes;

    TextField _searchField;
    TreeView _treeView;

    // -------------------------------------------------------------------------
    // Show Popup
    // -------------------------------------------------------------------------
    /// <summary>
    /// Shows the popup as a dropdown anchored to the activator rectangle.
    /// </summary>
    /// <param name="activatorRect">The rectangle the dropdown is anchored to.</param>
    /// <param name="types">The selectable types; <c>null</c> shows an empty tree.</param>
    /// <param name="onSelected">Called with the chosen type before the popup closes.</param>
    public static void Show(Rect activatorRect, IEnumerable<Type> types, Action<Type> onSelected)
    {
        var wnd = CreateInstance<SearchableTypeTreePopup>();

        wnd._allTypes = types != null ? types.ToList() : new List<Type>();
        wnd._filteredTypes = wnd._allTypes;
        wnd._onSelected = onSelected;

        // Build the tree before UIElements binds rows
        wnd.RebuildTree(wnd._filteredTypes);

        wnd.ShowAsDropDown(activatorRect, new Vector2(350f, 500f));
    }

    // -------------------------------------------------------------------------
    // Initialize UI
    // -------------------------------------------------------------------------
    void OnEnable()
    {
        var root = rootVisualElement;
        root.style.flexDirection = FlexDirection.Column;

        // Search bar
        _searchField = new TextField("Search");
        _searchField.RegisterValueChangedCallback(evt => ApplyFilter(evt.newValue));
        root.Add(_searchField);

        // TreeView
        _treeView = new TreeView();
        _treeView.selectionType = SelectionType.Single;

        // ---------------------------------------------------------------------
        // Custom Row Template: Icon + Label
        // ---------------------------------------------------------------------
        _treeView.makeItem = () =>
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;

            var icon = new Image();
            icon.name = "icon";
            icon.style.width = 16;
            icon.style.height = 16;
            icon.style.marginRight = 4;

            var label = new Label();
            label.name = "label";
            label.style.flexGrow = 1f;
            label.style.unityTextAlign = TextAnchor.MiddleLeft;

            row.Add(icon);
            row.Add(label);

            return row;
        };

        // ---------------------------------------------------------------------
        // Bind Row: Icon + Label (Collapse Spacing When No Icon)
        // ---------------------------------------------------------------------
        _treeView.bindItem = (element, index) =>
        {
            var item = _treeView.GetItemDataForIndex<TreeItem>(index);
            if (item == null)
                return;

            var icon = element.Q<Image>("icon");
            var label = element.Q<Label>("label");

            if (label != null)
                label.text = item.DisplayName != null ? item.DisplayName : "";

            if (icon != null)
            {
                Texture tex = null;

                if (item.Type != null)
                {
                    var attr = item.Type.GetCustomAttribute<SerializableClassAttribute>();
                    tex = ManagedReferenceUtility.GetIconForType(item.Type, attr);
                }

                if (tex != null)
                {
                    // Show icon with spacing
                    icon.image = tex;
                    icon.style.display = DisplayStyle.Flex;
                    icon.style.width = 16;
                    icon.style.height = 16;
                    icon.style.marginRight = 4;
                }
                else
                {
                    // No icon: collapse spacing
                    icon.image = null;
                    icon.style.display = DisplayStyle.None;
                    icon.style.width = 0;
                    icon.style.height = 0;
                    icon.style.marginRight = 0;
                }
            }
        };

        // ---------------------------------------------------------------------
        // Selection
        // ---------------------------------------------------------------------
        _treeView.onItemsChosen += items =>
        {
            foreach (var obj in items)
            {
                var item = obj as TreeItem;
                if (item != null && item.Type != null)
                {
                    _onSelected?.Invoke(item.Type);
                    Close();
                    break;
                }
            }
        };

        root.Add(_treeView);

        // Rebuild again as a safety net
        RebuildTree(_filteredTypes);
    }

    // -------------------------------------------------------------------------
    // Search Filter
    // -------------------------------------------------------------------------
    void ApplyFilter(string filter)
    {
        if (string.IsNullOrWhiteSpace(filter))
        {
            _filteredTypes = _allTypes;
        }
        else
        {
            string f = filter.ToLowerInvariant();
            _filteredTypes = _allTypes
                .Where(t =>
                    t != null &&
                    t.FullName != null &&
                    t.FullName.ToLowerInvariant().Contains(f))
                .ToList();
        }

        RebuildTree(_filteredTypes);
    }

    // -------------------------------------------------------------------------
    // Build Tree Hierarchy
    // -------------------------------------------------------------------------
    void RebuildTree(List<Type> types)
    {
        if (types == null)
            types = new List<Type>();

        var rootItem = new TreeItem("root", null);

        foreach (var type in types)
        {
            if (type == null)
                continue;

            string[] crumbs = ManagedReferenceUtility.GetBreadcrumbNames(type);
            if (crumbs == null || crumbs.Length == 0)
                continue;

            TreeItem parent = rootItem;

            // Namespace / nested type hierarchy
            for (int i = 0; i < crumbs.Length - 1; i++)
            {
                string name = crumbs[i];
                if (string.IsNullOrEmpty(name))
                    continue;

                if (parent.Children == null)
                    parent.Children = new List<TreeItem>();

                var existing = parent.Children.FirstOrDefault(c => c.DisplayName == name);
                if (existing == null)
                {
                    existing = new TreeItem(name, null);
                    parent.Children.Add(existing);
                }

                parent = existing;
            }

            // Final concrete type
            string finalName = ManagedReferenceUtility.GetNicifiedTypeName(type);

            if (parent.Children == null)
                parent.Children = new List<TreeItem>();

            parent.Children.Add(new TreeItem(finalName, type));
        }

        var treeData = rootItem.ToTreeViewItems();
        if (treeData == null)
            treeData = new List<TreeViewItemData<TreeItem>>();

        _treeView.SetRootItems(treeData);
        _treeView.Rebuild();
    }

    // -------------------------------------------------------------------------
    // Tree Node
    // -------------------------------------------------------------------------
    class TreeItem
    {
        public string DisplayName;
        public Type Type;
        public List<TreeItem> Children = new List<TreeItem>();

        public TreeItem(string displayName, Type type)
        {
            DisplayName = displayName;
            Type = type;
        }

        public List<TreeViewItemData<TreeItem>> ToTreeViewItems()
        {
            var result = new List<TreeViewItemData<TreeItem>>();
            int id = 0;

            foreach (var child in Children)
                AddRecursive(child, ref id, result);

            return result;
        }

        static void AddRecursive(TreeItem item, ref int id, List<TreeViewItemData<TreeItem>> list)
        {
            int currentId = id++;

            var children = new List<TreeViewItemData<TreeItem>>();
            foreach (var child in item.Children)
                AddRecursive(child, ref id, children);

            list.Add(new TreeViewItemData<TreeItem>(currentId, item, children));
        }
    }
}
