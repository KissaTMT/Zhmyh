using UnityEngine;

public class SurfaceClimber
{
    private Transform _transform;
    private float _climbForce;
    private Vector3 _surfaceDirection;
    private Collider[] _colliders = new Collider[4];

    public SurfaceClimber(Transform transform,float climbForce)
    {
        _transform = transform;
        _climbForce = climbForce;
    }

    public void Climb(Vector3 input)
    {
        var direction = CalculateDirection(input);
        _transform.position += direction * _climbForce * Time.deltaTime;
    }
    public bool IsClimb() => Physics.OverlapSphereNonAlloc(_transform.position, 8, _colliders, LayerMask.GetMask("Climb")) > 0;
    private bool OnClimb(out RaycastHit hitInfo) => Physics.Raycast(_transform.position, _transform.position + _transform.forward, out hitInfo, 4, LayerMask.GetMask("Climb"));
    private Vector3 CalculateDirection(Vector3 input)
    {
        if (OnClimb(out var hit)) return -_surfaceDirection;
        var normal = hit.normal.normalized;
        var product = Vector3.Dot(input, normal);
        _surfaceDirection = input - product * normal;
        return _surfaceDirection;
    }
}
