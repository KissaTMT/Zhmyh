using UnityEngine;

public class Unit : MonoBehaviour
{
    public Transform Transform => transform;
    public Health Health => health;

    protected new Transform transform;

    protected Health health = new();
    public Unit Init()
    {
        transform = GetComponent<Transform>();
        OnInit();
        return this;
    }
    public virtual void Tick() { }
    protected virtual void OnInit() { }
}