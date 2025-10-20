using UnityEngine;

public class SurfaceClimber
{
    private Transform _transform;
    private float _climbForce;
    private Vector3 _direction;
    private Vector3 _surfaceNormal;
    private Collider[] _colliders = new Collider[4];
    private RaycastHit[] _raycastHits = new RaycastHit[4];

    public SurfaceClimber(Transform transform, float climbForce)
    {
        _transform = transform;
        _climbForce = climbForce;
    }

    public void Climb(Vector2 input)
    {
        //if (!IsClimb()) return;

        _direction = CalculateDirection(input);
        _transform.position += _direction * _climbForce * Time.deltaTime;
    }

    public bool IsClimb() => Physics.OverlapSphereNonAlloc(_transform.position, 32f, _colliders, LayerMask.GetMask("Climb")) > 0;
    private int GetHits() => Physics.RaycastNonAlloc(_transform.position + Vector3.up * 8, 
        _transform.GetChild(0).forward, 
        _raycastHits, 32, LayerMask.GetMask("Climb"));
    private Vector3 CalculateSurfaceNormal()
    {
        var hitCount = GetHits();

        var averageNormal = Vector3.zero;

        if (hitCount == 0) return averageNormal;

        for (int i = 0; i < hitCount; i++)
        {
            averageNormal += _raycastHits[i].normal;
            Debug.Log(_raycastHits[i].normal);
        }
        return (averageNormal / hitCount).normalized;
    }
    private Vector3 CalculateDirection(Vector3 input)
    {
        _surfaceNormal = CalculateSurfaceNormal();

        //if (_surfaceNormal == Vector3.zero) return Vector3.zero;

        return input - Vector3.Dot(input, _surfaceNormal) * _surfaceNormal;
    }

    public void DrawGizmos()
    {
        Gizmos.color = Color.white;
        //Gizmos.DrawWireSphere(_transform.position, 32);
        Gizmos.DrawLine(_transform.position + Vector3.up * 8, _transform.position + Vector3.up * 8 + _transform.GetChild(0).forward * 32);
        if (_surfaceNormal != Vector3.zero)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(_transform.position + Vector3.up * 8, _transform.position + Vector3.up * 8 + _surfaceNormal * 32 + Vector3.up * 8);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(_transform.position + Vector3.up * 8, _transform.position + Vector3.up * 8 + _direction * 32 + Vector3.up * 8);
        }
    }
}