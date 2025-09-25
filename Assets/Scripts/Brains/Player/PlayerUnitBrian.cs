using System;
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
    private Vector2 _inputDirection;
    private Vector2 _cashedAimPosition;
    private Vector2 _cashedScreenPoint;


    [Inject]
    public void Construct(IInput input, Cursor cursor)
    {
        _input = input;
        _cursor = cursor;

        _input.Space += Space;
        _input.Pulling += SetPull;
        _input.InitAiming += SetAim;
    }
    public void Init()
    {
        _unit = (Zhmyh)GetComponent<Zhmyh>().Init();
        _cursor.Init(_input);
        _cameraMain = Camera.main;
        _cameraTransform = _cameraMain.GetComponent<Transform>();
    }
    private void OnDisable()
    {
        _input.Space -= Space;
        _input.Pulling -= SetPull;
        _input.InitAiming -= SetAim;
    }

    private void SetAim(bool isAiming) => _unit.SetAim(isAiming);
    private void SetPull(bool isPull) => _unit.Pull(isPull);
    private void Space() => _unit.Dash();
    private Vector3 CalculateMovementDirection()
    {
        _inputDirection = _input.GetDirection();

        var angle = _cameraTransform.eulerAngles.y * Mathf.Deg2Rad;

        var cos = Mathf.Cos(angle);
        var sin = Mathf.Sin(angle);

        var x = _inputDirection.x * cos + _inputDirection.y * sin;
        var z = -_inputDirection.x * sin + _inputDirection.y * cos;

        return new Vector3(x, 0f, z);
    }
    private Vector2 CalculateLookDirection()
    {
        _cashedAimPosition = _cursor.ScreenPosition;

        _cashedScreenPoint = _cameraMain.WorldToScreenPoint(_unit.Transform.position);
        return _cashedAimPosition - _cashedScreenPoint;
    }
    private void Update()
    {
        _unit.SetLookDirection(CalculateLookDirection());
        _unit.SetMovementDirection(CalculateMovementDirection());
        _unit.Tick();
        _cursor.Tick();
    }
}
