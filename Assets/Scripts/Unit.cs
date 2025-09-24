using System.Collections.Generic;
using UnityEngine;

public abstract class Unit : MonoBehaviour
{
    public Dictionary<string, ReactiveProperty<float>> Properties => properties;
    public Transform Transform => transform;
    protected Dictionary<string, ReactiveProperty<float>> properties = new();
    protected new Transform transform;
    public virtual Unit Init()
    {
        transform = GetComponent<Transform>();
        return this;
    }
    public abstract void Tick();
}