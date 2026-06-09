using UnityEngine;
using System;
using System.Collections.Generic;
using Components;

namespace Units
{
    public class UnitCore : MonoBehaviour
    {
        public IReadOnlyDictionary<Type, UnitComponent> Components => _components;
        public Transform Transform => _transform;
        public Timeflow Timeflow { get; private set; }
        public string Tag { get; private set; }

        private Transform _transform;

        private Dictionary<Type, UnitComponent> _components;
        private List<ITickable> _tickables;
        public UnitCore Init(Timeflow timeflow, string tag)
        {
            if(string.IsNullOrEmpty(tag))
            {
                throw new ArgumentException("Tag cannot be null or empty.");
            }

            Timeflow = timeflow;

            Tag = tag;

            _components = new Dictionary<Type, UnitComponent>();

            _transform = GetComponent<Transform>();

            return this;
        }
        public void Tick()
        {
            for(var i=0;i<_tickables.Count;i++)
            {
                var t = _tickables[i];
                if (t.Enabled) t.Tick(Time.deltaTime);
            }
        }
        public T Get<T>() where T : UnitComponent
        {
            _components.TryGetValue(typeof(T), out var result);
            return (T)result;
        }
        public void Add<T>(T component) where T : UnitComponent
        {
            _components.Add(typeof(T), component);
            if (component is ITickable tickable) _tickables.Add(tickable);
        }
        public bool Remove<T>()
        {
            if( _components.TryGetValue(typeof(T),out var result)) {
                if(result is ITickable tickable) _tickables.Remove(tickable);
                return _components.Remove(typeof(T));
            }
            return false;
        }
    }
}
