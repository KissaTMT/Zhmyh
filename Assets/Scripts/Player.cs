using System;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private RectTransform _aim;
    private Zhmyh _unit;
    private Camera _cameraMain;
    private IInput _input;

    private void Awake()
    {
        _input = new InputHandler();

        _unit = GetComponent<Zhmyh>().Init();
        _cameraMain = Camera.main;

        _input.OnDirection += SetDirection;
        _input.OnSpace += OnSpace;
        _input.OnAim += SetAim;
        _input.OnShoot += Shoot;
        _input.OnSetAim += SetAimReady;
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
    private void SetAim(Vector2 delta) => _unit.SetAim(_aim.transform.position);
    private void OnSpace() => _unit.OnClimb();
    private void Update() => _unit.Run();
}
