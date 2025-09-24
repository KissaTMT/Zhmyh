using System;
using System.Collections.Generic;

[Serializable]
public class ReactiveProperty<T>
{
    public Action<T> OnChanged { get; set; }

    private T _value;
    private IEqualityComparer<T> _comparer;

    public T Value
    {
        get { return _value; }
        set
        {
            if (!_comparer.Equals(value, _value))
            {
                _value = value;
                OnChanged?.Invoke(_value);
            }
        }
    }

    public ReactiveProperty() : this (default(T), EqualityComparer<T>.Default) { }
    public ReactiveProperty(T value) : this (value, EqualityComparer<T>.Default) { }
    public ReactiveProperty(T value, IEqualityComparer<T> comparer)
    {
        _value = value;
        _comparer = comparer;
    }
}
