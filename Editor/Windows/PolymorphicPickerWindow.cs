// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace XFG.Utilities
{
    /// <summary>
    /// Picker window for choosing an asset or scene object that satisfies a <see cref="PolymorphicReference{T}"/> contract type.
    /// </summary>
    /// <remarks>
    /// Offers Assets and Scene tabs, a name search, and a zoom slider that switches
    /// from a list to a grid of previews at higher zoom levels.
    /// </remarks>
    public class PolymorphicPickerWindow : EditorWindow
    {
        private Type _contractType;
        private SerializedProperty _targetProperty;

        private string _search = "";
        private int _tabIndex = 0;
        private Vector2 _scroll;

        private float _zoom = 1.0f;
        private const float MinZoom = 0.75f;
        private const float MaxZoom = 2.0f;
        private const float GridThreshold = 1.35f;

        private UnityEngine.Object _currentValue;

        private static readonly string[] Tabs = { "Assets", "Scene" };

        // ---------------------------------------------------------------------
        // Open
        // ---------------------------------------------------------------------
        /// <summary>
        /// Opens the picker for the given contract type and target property.
        /// </summary>
        /// <param name="contractType">The interface or abstract type that selected objects must satisfy.</param>
        /// <param name="targetProperty">The object-reference property (the reference's <c>_object</c> field) to assign to.</param>
        public static void Open(Type contractType, SerializedProperty targetProperty)
        {
            PolymorphicPickerWindow window = CreateInstance<PolymorphicPickerWindow>();
            window._contractType = contractType;
            window._targetProperty = targetProperty.Copy();
            window._currentValue = targetProperty.objectReferenceValue;

            // Auto-select tab
            if (window._currentValue != null)
            {
                if (IsSceneObject(window._currentValue))
                    window._tabIndex = 1;
                else
                    window._tabIndex = 0;
            }
            else
            {
                window._tabIndex = 0;
            }

            window.titleContent = new GUIContent("Select " + contractType.Name);
            window.ShowAuxWindow();
        }

        private static bool IsSceneObject(UnityEngine.Object obj)
        {
            if (obj is GameObject go)
                return go.scene.IsValid();

            if (obj is Component comp)
                return comp.gameObject.scene.IsValid();

            return false;
        }

        // ---------------------------------------------------------------------
        // GUI
        // ---------------------------------------------------------------------
        private void OnGUI()
        {
            if (_contractType == null || _targetProperty == null)
            {
                EditorGUILayout.LabelField("Invalid picker state.");
                return;
            }

            DrawSearchBar();
            DrawToolbar();
            DrawContent();
        }

        private void DrawSearchBar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);

            GUIStyle searchField = GUI.skin.FindStyle("ToolbarSearchTextField") ?? EditorStyles.toolbarSearchField;
            GUIStyle cancelButton = GUI.skin.FindStyle("ToolbarSearchCancelButton") ?? EditorStyles.toolbarButton;

            _search = GUILayout.TextField(_search, searchField);

            if (GUILayout.Button("", cancelButton))
            {
                _search = "";
                GUI.FocusControl(null);
            }

            GUILayout.EndHorizontal();
        }

        private void DrawToolbar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);

            _tabIndex = GUILayout.Toolbar(_tabIndex, Tabs, EditorStyles.toolbarButton, GUILayout.Width(140f));

            GUILayout.FlexibleSpace();

            GUILayout.Label("Zoom", EditorStyles.miniLabel);
            _zoom = GUILayout.HorizontalSlider(_zoom, MinZoom, MaxZoom, GUILayout.Width(80f));

            GUILayout.EndHorizontal();
        }

        private void DrawContent()
        {
            if (Event.current.type == EventType.MouseMove)
                Repaint();

            IEnumerable<UnityEngine.Object> items =
                _tabIndex == 0 ? GetAssetObjects() : GetSceneObjects();

            // Auto-scroll to current value (first frame)
            if (_currentValue != null && _scroll.y < 2)
            {
                int index = 0;
                foreach (var obj in items)
                {
                    if (obj == _currentValue)
                    {
                        _scroll.y = index * 24f * _zoom;
                        break;
                    }
                    index++;
                }
            }

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            if (_zoom > GridThreshold)
                DrawGrid(items);
            else
                DrawList(items);

            EditorGUILayout.EndScrollView();
        }

        // ---------------------------------------------------------------------
        // List Mode
        // ---------------------------------------------------------------------
        private void DrawList(IEnumerable<UnityEngine.Object> items)
        {
            float rowHeight = 20f * _zoom;

            foreach (UnityEngine.Object obj in items)
            {
                if (!MatchesSearch(obj))
                    continue;

                Rect rowRect = GUILayoutUtility.GetRect(0, rowHeight, GUILayout.ExpandWidth(true));

                DrawRow(rowRect, obj);

                if (Event.current.type == EventType.MouseDown &&
                    rowRect.Contains(Event.current.mousePosition))
                {
                    Assign(obj);
                }
            }
        }

        private void DrawRow(Rect rect, UnityEngine.Object obj)
        {
            bool isCurrent = (obj == _currentValue);

            if (Event.current.type == EventType.Repaint)
            {
                if (isCurrent)
                    EditorGUI.DrawRect(rect, new Color(0.2f, 0.6f, 0.2f, 0.35f)); // Highlight
                else if (rect.Contains(Event.current.mousePosition))
                    EditorGUI.DrawRect(rect, new Color(0.3f, 0.5f, 0.85f, 0.25f)); // Hover
            }

            float iconSize = 16f * _zoom;
            float padding = 4f * _zoom;

            Texture icon = GetIcon(obj, false);

            Rect iconRect = new Rect(
                rect.x + padding,
                rect.y + (rect.height - iconSize) * 0.5f,
                iconSize,
                iconSize
            );

            GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit, true);

            Rect labelRect = new Rect(
                iconRect.xMax + padding,
                rect.y,
                rect.width - iconRect.width - padding * 3f,
                rect.height
            );

            GUI.Label(labelRect, obj.name, EditorStyles.label);
        }

        // ---------------------------------------------------------------------
        // Grid Mode
        // ---------------------------------------------------------------------
        private void DrawGrid(IEnumerable<UnityEngine.Object> items)
        {
            float baseTile = 64f;
            float tileSize = baseTile * _zoom;
            float padding = 8f;

            float viewWidth = position.width - 20f;
            int columns = Mathf.Max(1, Mathf.FloorToInt(viewWidth / (tileSize + padding)));

            List<UnityEngine.Object> filtered = new List<UnityEngine.Object>();
            foreach (var obj in items)
                if (MatchesSearch(obj))
                    filtered.Add(obj);

            int rows = Mathf.CeilToInt(filtered.Count / (float)columns);
            float totalHeight = rows * (tileSize + padding);

            Rect gridRect = GUILayoutUtility.GetRect(viewWidth, totalHeight);
            GUI.BeginGroup(gridRect);

            for (int i = 0; i < filtered.Count; i++)
            {
                int col = i % columns;
                int row = i / columns;

                float x = col * (tileSize + padding);
                float y = row * (tileSize + padding);

                Rect tileRect = new Rect(x, y, tileSize, tileSize);

                DrawTile(tileRect, filtered[i]);

                if (Event.current.type == EventType.MouseDown &&
                    tileRect.Contains(Event.current.mousePosition))
                {
                    Assign(filtered[i]);
                }
            }

            GUI.EndGroup();
        }

        private void DrawTile(Rect rect, UnityEngine.Object obj)
        {
            bool isCurrent = (obj == _currentValue);

            if (Event.current.type == EventType.Repaint)
            {
                if (isCurrent)
                    EditorGUI.DrawRect(rect, new Color(0.2f, 0.6f, 0.2f, 0.35f));
                else if (rect.Contains(Event.current.mousePosition))
                    EditorGUI.DrawRect(rect, new Color(0.3f, 0.5f, 0.85f, 0.25f));
            }

            Texture icon = GetIcon(obj, true);

            float iconSize = Mathf.Min(rect.width, rect.height) * 0.6f;

            Rect iconRect = new Rect(
                rect.x + (rect.width - iconSize) * 0.5f,
                rect.y + (rect.height - iconSize) * 0.35f,
                iconSize,
                iconSize
            );

            GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit, true);

            Rect labelRect = new Rect(rect.x, rect.yMax - 16f, rect.width, 16f);
            GUI.Label(labelRect, obj.name, EditorStyles.miniLabel);
        }

        // ---------------------------------------------------------------------
        // Icons
        // ---------------------------------------------------------------------
        private Texture GetIcon(UnityEngine.Object obj, bool large)
        {
            Texture icon = null;

            if (large)
                icon = AssetPreview.GetAssetPreview(obj);

            if (icon == null)
                icon = AssetPreview.GetMiniThumbnail(obj);

            if (icon == null)
                icon = AssetPreview.GetMiniTypeThumbnail(obj.GetType());

            return icon ?? Texture2D.grayTexture;
        }

        // ---------------------------------------------------------------------
        // Filtering
        // ---------------------------------------------------------------------
        private bool MatchesSearch(UnityEngine.Object obj)
        {
            if (string.IsNullOrEmpty(_search))
                return true;

            return obj.name.IndexOf(_search, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private IEnumerable<UnityEngine.Object> GetAssetObjects()
        {
            string[] guids = AssetDatabase.FindAssets("t:Object");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);

                if (IsValid(obj))
                    yield return obj;
            }
        }

        private IEnumerable<UnityEngine.Object> GetSceneObjects()
        {
            foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (!go.scene.IsValid()) // Skip prefabs
                    continue;

                if (IsValid(go))
                    yield return go;
            }
        }

        private bool IsValid(UnityEngine.Object obj)
        {
            if (obj == null || _contractType == null)
                return false;

            // Reject components entirely
            if (obj is Component)
                return false;

            // GameObject: check components
            if (obj is GameObject go)
            {
                foreach (var c in go.GetComponents<Component>())
                {
                    if (c != null && _contractType.IsAssignableFrom(c.GetType()))
                        return true;
                }
                return false;
            }

            // ScriptableObject or asset directly implementing contract
            return _contractType.IsAssignableFrom(obj.GetType());
        }

        // ---------------------------------------------------------------------
        // Assignment
        // ---------------------------------------------------------------------
        private void Assign(UnityEngine.Object obj)
        {
            _targetProperty.objectReferenceValue = obj;
            _targetProperty.serializedObject.ApplyModifiedProperties();
            Close();
        }
    }
}
#endif
