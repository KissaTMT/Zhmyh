using Components;
using System;
using UnityEngine;

public class Impulse : UnitComponent, IContributable<Vector3>, ITickable
{
    public float Progress => relativeTimeflow == 1 ? _progress : 1 - _progress;
    public Vector3 Contribute => _contribute;
    public Vector3 Force => _force;
    public float AtenuationTime => _atenuationTime;

    private float _atenuationTime;

    private Vector3 _contribute;
    private float _progress;

    private Vector3 _force;
    private Vector3 _start;
    private Vector3 _end;

    public void Apply(Vector3 impulse, float atenuationTime)
    {
        Apply(impulse, Vector3.zero, atenuationTime);
    }
    public void Apply(Vector3 start, Vector3 end, float atenuationTime)
    {
        _progress = relativeTimeflow == 1 ? 0f : 1f;
        _atenuationTime = atenuationTime;

        
        _start = start;
        _end = end;

        _force = start;
    }

    public void Tick(float deltaTime)
    {
        _progress = Mathf.Clamp01(_progress + relativeTimeflow * (deltaTime / _atenuationTime));
        _contribute = Vector3.Lerp(_start, _end, _progress);
    }
    protected override void OnDisable()
    {
        _contribute = Vector3.zero;
    }
    
}
