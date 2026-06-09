using Components;
using UnityEngine;

public class Impulse : UnitComponent, IContributable<Vector3>
{
    public Vector3 Contribute => _contribute;

    private Vector3 _impulse;
    private float _atenuationSpeed;

    private Vector3 _contribute;
    private float _progress;
    public void Init(Vector3 impulse, float atenuationSpeed)
    {
        _impulse = impulse;
        _contribute = impulse;
        _atenuationSpeed = atenuationSpeed;
        _progress = 0;
    }
    public void Tick(float deltaTime)
    {
        _progress = Mathf.Clamp01(_progress + deltaTime / _atenuationSpeed);
        _contribute = Vector3.Lerp(_impulse, Vector3.zero, _progress);
    }
}
