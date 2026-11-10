// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

using System;
using System.Collections.Generic;
using UnityEngine;
namespace XFG
{
    public static partial class Core
    {
        /// <summary>
        /// Marks a <c>[SerializeReference]</c> field so the inspector draws a type picker for it.
        /// </summary>
        /// <remarks>
        /// The editor checks the icon overrides in this order: <see cref="CustomIcon"/>,
        /// <see cref="IconName"/>, <see cref="IconType"/>, then the type's default icon.
        /// </remarks>
        [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
        public sealed class SerializableClassAttribute : PropertyAttribute
        {
            /// <summary>
            /// Optional base type override for the managed reference field.
            /// </summary>
            public Type BaseType;

            /// <summary>
            /// Optional icon override: a Unity built-in icon name (e.g. "d_ScriptableObject Icon").
            /// </summary>
            public string IconName;

            /// <summary>
            /// Optional icon override: use the icon of another type.
            /// </summary>
            public Type IconType;

            /// <summary>
            /// Optional icon override: a direct texture override.
            /// </summary>
            public Texture CustomIcon;

            /// <summary>
            /// Creates the attribute with no overrides.
            /// </summary>
            public SerializableClassAttribute() { }
        }
    }
    /// <summary>
    /// Minimal <see cref="MonoBehaviour"/> singleton base that exposes the active instance of <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The concrete singleton component type.</typeparam>
    public class SimpleSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        [SerializeField]
        static T _instance = null;

        /// <summary>
        /// The active instance, or <c>null</c> if none is registered.
        /// </summary>
        public static T Instance
        {
            get
            {
                return _instance;
            }
        }
        /// <summary>
        /// Whether a duplicate instance destroys its GameObject on <see cref="Awake"/>. Defaults to <c>true</c>.
        /// </summary>
        protected virtual bool DestroyIfAlreadyExists
        {
            get
            {
                return true;
            }
        }
        /// <summary>
        /// Unity callback that registers this instance as the singleton.
        /// </summary>
        protected virtual void Awake()
        {
            InitSingleton();
        }
        /// <summary>
        /// Unity callback that unregisters this instance if it is the singleton.
        /// </summary>
        protected virtual void OnDestroy()
        {
            UninitSingleton();
        }
        /// <summary>
        /// Registers this instance, or destroys its GameObject if another instance already exists.
        /// </summary>
        protected virtual void InitSingleton()
        {
            if (Instance != null && DestroyIfAlreadyExists) // There can be only one
                Destroy(this.gameObject);
            else
                _instance = (T)((MonoBehaviour)this); // Cast through MonoBehaviour to satisfy the compiler
        }
        /// <summary>
        /// Clears the singleton reference if it points to this instance.
        /// </summary>
        protected void UninitSingleton()
        {
            if (_instance == this)
                _instance = null;
        }
    }
}