using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

public class Player : MonoBehaviour
{
    public Transform Transform => _unit.Transform;

    private Zhmyh _unit;
    private Cursor _cursor;
    private IInput _input;

    [Inject]
    public void Construct(IInput input, Cursor cursor)
    {
        _input = input;
        _cursor = cursor;

        _input.OnDirection += SetDirection;
        _input.OnSpace += OnSpace;
        _input.OnAim += SetAim;
        _input.OnShoot += Shoot;
        _input.OnSetAim += SetAimReady;
    }
    public void Init()
    {
        _unit = GetComponent<Zhmyh>().Init();
    }
    private void SetAimReady() => _unit.SetAimReady();

    private void Shoot() => _unit.OnShoot();

    private void OnDisable()
    {
        _input.OnDirection -= SetDirection;
        _input.OnSpace -= OnSpace;
        _input.OnAim -= SetAim;
        _input.OnShoot -= Shoot;
        _input.OnSetAim -= SetAimReady;
    }
    private void SetDirection(Vector2 vector) => _unit.SetDirection(vector);
    private void SetAim(Vector2 delta) => _unit.SetAim(_cursor.RectTransform.position);
    private void OnSpace() => _unit.OnClimb();
    private void Update() => _unit.Run();
}
