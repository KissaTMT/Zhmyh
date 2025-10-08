using System.Drawing;
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
        _unit = GetComponent<Zhmyh>().Init() as Zhmyh;
        _cursor.Init(_input);
        _cameraMain = Camera.main;
        _cameraTransform = _cameraMain.GetComponent<Transform>();
    }
    private void OnDisable()
    {
        _input.Space -= Space;
        _input.Pulling -= SetPull;
    }
    private void SetPull(bool isPull) => _unit.Pull(isPull);
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
        return _cursor.ScreenPosition - (Vector2)_cameraMain.WorldToScreenPoint(_unit.Transform.position);
    }
    private Vector3 CalculateShootDirection()
    {
        return Quaternion.AngleAxis(-7, _cameraTransform.right) * _cameraTransform.forward;
    }
    private void Update()
    {
        _unit.SetLookDirection(CalculateLookDirection());
        _unit.SetMovementDirection(CalculateMovementDirection());
        _unit.SetShootDirection(CalculateShootDirection());
        _unit.Tick();
        _cursor.Tick();
    }
}
