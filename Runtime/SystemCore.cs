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