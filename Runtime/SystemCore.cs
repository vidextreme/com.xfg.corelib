// Copyright (c) 2026 John David Uy
// Licensed under the MIT License. See LICENSE for details.

using System;
using System.Collections.Generic;
using UnityEngine;
namespace XFG
{
    public static partial class Core
    {
        [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
        public sealed class SerializableClassAttribute : PropertyAttribute
        {
            // Optional base type override for the managed reference field.
            public Type BaseType;

            // Optional icon overrides.
            // 1) Unity built-in icon name (e.g. "d_ScriptableObject Icon").
            public string IconName;

            // 2) Use the icon of another type.
            public Type IconType;

            // 3) Direct texture override.
            public Texture CustomIcon;

            public SerializableClassAttribute() { }
        }
    }
    public class SimpleSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        [SerializeField]
        static T _instance = null;

        public static T Instance
        {
            get
            {
                return _instance;
            }
        }
        protected virtual bool DestroyIfAlreadyExists
        {
            get
            {
                return true;
            }
        }
        protected virtual void Awake()
        {
            InitSingleton();
        }
        protected virtual void OnDestroy()
        {
            UninitSingleton();
        }
        protected virtual void InitSingleton()
        {
            if (Instance != null && DestroyIfAlreadyExists)//there could be only one...
                Destroy(this.gameObject);
            else
                _instance = (T)((MonoBehaviour)this);//Dear compiler, Please shut up. Sincerely, developer
        }
        protected void UninitSingleton()
        {
            if (_instance == this)
                _instance = null;
        }
    }
}