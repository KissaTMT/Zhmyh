using UnityEngine;
using System;
using System.Collections.Generic;
using Components;

namespace Units
{
    public class UnitCore : MonoBehaviour
    {
        public IReadOnlyDictionary<Type, IComponent> Components => _components;
        public Transform Transform => _transform;
        public Timeflow Timeflow { get; private set; }
        public string Tag { get; private set; }

        private Transform _transform;

        private Dictionary<Type, IComponent> _components;
        public UnitCore Init(Timeflow timeflow, string tag)
        {
            if(string.IsNullOrEmpty(tag))
            {
                throw new ArgumentException("Tag cannot be null or empty.");
            }

            Timeflow = timeflow;

            Tag = tag;

            _components = new Dictionary<Type, IComponent>();

            _transform = GetComponent<Transform>();

            return this;
        }
        public void Tick()
        {
            
        }
        public T Get<T>() where T : IComponent
        {
            _components.TryGetValue(typeof(T), out var result);
            return (T)result;
        }
        public void Add<T>(T component) where T : IComponent
        {
            _components.Add(typeof(T), component);
        }
        public bool Remove<T>()
        {
            return _components.Remove(typeof(T));
        }
    }
}
