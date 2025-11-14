using R3;
using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class Unit : MonoBehaviour
{
    public IReadOnlyDictionary<string, ReactiveProperty<float>> Properties => properties;
    public Transform Transform => transform;
    public Health Health => health;

    protected Dictionary<string, ReactiveProperty<float>> properties;
    protected new Transform transform;

    protected Health health = new();
    public Unit Init()
    {
        transform = GetComponent<Transform>();
        properties = new();
        SetupProperties();
        OnInit();
        return this;
    }
    public virtual void Tick() { }
    protected abstract void SetupProperties();
    protected virtual void OnInit() { }
}