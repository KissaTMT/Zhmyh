using Components;
using UnityEngine;

public class Impulse : UnitComponent, IContributable<Vector3>, ITickable
{
    public float Progress => relativeTimeflow == 1 ? _progress : 1 - _progress;
    public Vector3 Contribute => _contribute;

    private float _atenuationSpeed;

    private Vector3 _contribute;
    private float _progress;

    private Vector3 _start;
    private Vector3 _end;

    public void Apply(Vector3 impulse, float atenuationSpeed)
    {
        _progress = relativeTimeflow == 1 ? 0f : 1f;
        _atenuationSpeed = atenuationSpeed;

        _start = impulse;
        _end = Vector3.zero;
    }

    public void Tick(float deltaTime)
    {
        _progress = Mathf.Clamp01(_progress + relativeTimeflow * (deltaTime / _atenuationSpeed));
        _contribute = Vector3.Lerp(_start, _end, _progress);
    }
    protected override void OnDisable()
    {
        _contribute = Vector3.zero;
    }
}
