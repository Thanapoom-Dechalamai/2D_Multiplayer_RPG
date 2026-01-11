using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Managers
{
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        public static void Register<T>(object service)
        {
            if (!_services.ContainsKey(typeof(T)))
            {
                _services[typeof(T)] = service;
            }
            else
            {
                Debug.LogWarning($"Service {typeof(T)} already registered.");
            }
        }

        public static void Unregister<T>()
        {
            if (_services.ContainsKey(typeof(T)))
            {
                _services.Remove(typeof(T));
            }
        }

        public static T Get<T>()
        {
            if (_services.TryGetValue(typeof(T), out var service))
            {
                return (T)service;
            }

            Debug.LogError($"Service {typeof(T)} not found!");
            return default;
        }
    }
}