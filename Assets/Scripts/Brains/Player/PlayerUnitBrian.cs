using R3;
using UnityEngine;
using Zenject;

public class PlayerUnitBrian : MonoBehaviour, IBrian
{
    public Transform Transform => _unit.Transform;
    public Unit Unit => _unit;

    private Zhmyh _unit;
    private Camera _cameraMain;
    private Cursor _cursor;
    private Transform _cameraTransform;

    private IInput _input;
    private bool _isPull; 

    [Inject]
    public void Construct(IInput input, Cursor cursor)
    {
        _input = input;
        _cursor = cursor;

        _input.Space += Space;
        _input.Pulling += SetPull;
    }
    public void Init()
    {
        _unit = GetComponent<Unit>().Init() as Zhmyh;
        _cursor.Init(_input);
        _cameraMain = Camera.main;
        _cameraTransform = _cameraMain.GetComponent<Transform>();
    }
    private void OnDisable()
    {
        _input.Space -= Space;
        _input.Pulling -= SetPull;
    }
    private void SetPull(bool isPull)
    {
        _unit.Pull(isPull);
        _isPull = isPull;
    }
    private void Space()
    {
        _unit.Dash();
        _unit.Climb();
    }
    private Vector3 CalculateMovementDirection()
    {
        var input = _input.GetDirection();

        var angle = _cameraTransform.eulerAngles.y * Mathf.Deg2Rad;

        var cos = Mathf.Cos(angle);
        var sin = Mathf.Sin(angle);

        var x = input.x * cos + input.y * sin;
        var z = -input.x * sin + input.y * cos;

        return new Vector3(x, 0f, z);
    }
    private Vector2 CalculateLookDirection()
    {
        return _cursor.ScreenPosition - (Vector2)_cameraMain.WorldToScreenPoint(Transform.position);
    }
    private Vector3 CalculateShootDirection()
    {
        var ray = _cameraMain.ScreenPointToRay(_cursor.ScreenPosition);
        return Physics.Raycast(ray, out var hitInfo, 1024) ? hitInfo.point : ray.origin + ray.direction * 1024;
    }
    private void Update()
    {
        _unit.SetLookDirection(CalculateLookDirection());
        _unit.SetMovementDirection(CalculateMovementDirection());
        if(_isPull) _unit.SetShootDirection(CalculateShootDirection());
        _unit.Tick();
        _cursor.Tick();
    }
}
