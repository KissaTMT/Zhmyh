using UnityEngine;
using System;
using System.Collections.Generic;

namespace Units
{
    public class UnitCore : MonoBehaviour
    {
        public string Tag { get; private set; }
        private Dictionary<Type, IComponent> _components;
        public void Init(string tag)
        {
            if(string.IsNullOrEmpty(tag))
            {
                throw new ArgumentException("Tag cannot be null or empty.");
            }

            Tag = tag;
            _components = new Dictionary<Type, IComponent>();
        }
        public void Tick()
        {
            
        }
        public T Get<T>() where T : IComponent
        {
            _components.TryGetValue(typeof(T), out var result);
            return (T)result;
        }
    }
}
