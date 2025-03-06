using UnityEngine;

public class ClimbController
{
    private Transform _transform;
    private Transform _checker;
    private float _climbForce;

    public ClimbController(Transform transform, Transform checker, float climbForce)
    {
        _transform = transform;
        _checker = checker;
        _climbForce = climbForce;
    }

    public Vector2 Climb(Vector2 input)
    {
        var direction = CalculateDirection(input);
        _transform.position += direction * _climbForce * Time.deltaTime;
        return direction;
    }
    public bool IsClimb() => Physics2D.OverlapCircle(_transform.position, 3, LayerMask.GetMask("Climb"));
    private RaycastHit2D OnClimb() => Physics2D.Raycast(_checker.position, _checker.transform.right * _checker.localScale.x, 5, LayerMask.GetMask("Climb"));
    private Vector3 CalculateDirection(Vector2 input)
    {
        var hit = OnClimb();
        var normal = hit.normal.normalized;
        var product = Vector2.Dot(input, normal);
        return input - product * normal;
    }
}
