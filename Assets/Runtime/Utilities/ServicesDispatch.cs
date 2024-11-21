using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gemity.Common
{
    public class CallbackDisposable : IDisposable
    {
        private Action action;
        public CallbackDisposable(Action action)
        {
            this.action = action;
        }

        public void Dispose()
        {
            action();
        }
    }

    public abstract class BaseService
    {
    }

    public class BaseService<T> : BaseService where T : BaseService<T>
    {
        protected event Action<T> action;
        public virtual IDisposable Add(Action<T> action)
        {
            this.action += action;
            return CreateDisposable(action);
        }

        public virtual IDisposable Override(Action<T> action)
        {
            this.action = action;
            return CreateDisposable(action);
        }

        public virtual void Execute()
        {
            action?.Invoke(this as T);
        }

        public virtual void Execute(T t)
        {
            action?.Invoke(t);
        }

        private IDisposable CreateDisposable(Action<T> action)
        {
            return new CallbackDisposable(() => this.action -= action);
        }
    }

    public class CallbackService : BaseService<CallbackService>
    {
    }

    public static class ServicesDispatch
    {
        private static Dictionary<Type, Dictionary<string, BaseService>> _services;

        static ServicesDispatch()
        {
            _services = new();
        }

        public static IDisposable Add<T>(string name, Action<T> action) where T : BaseService<T>, new()
        {
            T e = FindOrCreate<T>(name);
            return e.Add(action);
        }

        public static IDisposable Override<T>(string name, Action<T> action) where T : BaseService<T>, new()
        {
            T e = FindOrCreate<T>(name);
            return e.Override(action);
        }

        public static void Remove<T>(string name) where T : BaseService<T>
        {
            T e = Find<T>(name);
            if (e != null)
            {
                Type t = typeof(T);
                _services[t].Remove(name);
            }
        }

        public static void Execute<T>(string name) where T : BaseService<T>
        {
            Find<T>(name)?.Execute();
        }

        public static void ExecuteAndRemove<T>(string name) where T : BaseService<T>
        {
            Execute<T>(name);
            Remove<T>(name);
        }

        public static void Execute<T>(string name, T t) where T : BaseService<T>
        {
            Find<T>(name)?.Execute(t);
        }

        public static void ExecuteAndRemove<T>(string name, T t) where T : BaseService<T>
        {
            Execute(name, t);
            Remove<T>(name);
        }

        private static T FindOrCreate<T>(string name) where T : BaseService<T>, new()
        {
            T service = Find<T>(name);

            if (service == null)
            {
                Type t = typeof(T);
                if (!_services.ContainsKey(t))
                    _services.Add(t, new());

                service = new T();
                _services[t].Add(name, service);
            }

            return service;
        }

        private static T Find<T>(string name) where T : BaseService<T>
        {
            Type t = typeof(T);
            if (!_services.ContainsKey(t) || !_services[t].ContainsKey(name))
                return null;

            return _services[t][name] as T;
        }

        public static bool HasService<T>(string name) where T : BaseService<T>
        {
            return Find<T>(name) != null;
        }

        public static void RemoveAll()
        {
            _services.Clear();
        }
    }
}