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

    [Inject]
    public void Construct(IInput input, Cursor cursor)
    {
        _input = input;
        _cursor = cursor;

        _input.Direction += SetDirection;
        _input.Space += Space;
        _input.SetPull += SetPull;
        _input.SetAim += SetAim;
    }
    public void Init()
    {
        _unit = (Zhmyh)GetComponent<Zhmyh>().Init();
        _cursor.Init(_input);
        _cameraMain = Camera.main;
        _cameraTransform = _cameraMain.GetComponent<Transform>();
        _cashedAimPosition = new Vector2(Screen.width / 2, Screen.height / 2);
    }
    private void OnDisable()
    {
        _input.Direction -= SetDirection;
        _input.Space -= Space;
        _input.SetPull -= SetPull;
        _input.SetAim -= SetAim;
    }

    private void SetAim(bool isAiming) => _unit.SetAim(isAiming);
    private void SetPull(bool isPull) => _unit.Pull(isPull);
    private void SetDirection(Vector2 delta) => _inputDirection = delta;
    private void Space() => _unit.Dash();
    private Vector3 CalculateMovementDirection()
    {
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

        var screenPoint = _cameraMain.WorldToScreenPoint(_unit.Transform.position);
        var delta = _cashedAimPosition - (Vector2)screenPoint;
        return delta;
    }
    private void Update()
    {
        _unit.SetLookDirection(CalculateLookDirection());
        _unit.SetMovementDirection(CalculateMovementDirection());
        _unit.Tick();
        _cursor.Tick();
    }
}
