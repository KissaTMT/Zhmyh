using UnityEngine;
using Zenject;

public class Player : MonoBehaviour
{
    public Transform Transform => _unit.Transform;

    private Zhmyh _unit;
    private Camera _cameraMain;
    private IInput _input;

    [Inject]
    public void Construct(IInput input)
    {
        _input = input;

        _input.OnDirection += SetDirection;
        _input.OnSpace += OnSpace;
        _input.OnShoot += Shoot;
        _input.OnAim += Aim;
        _input.OnSetAim += SetAimReady;
    }

    private void Aim(Vector2 vector) => _unit.SetAim(_cameraMain.ScreenToWorldPoint(vector));
    public void Init()
    {
        _unit = GetComponent<Zhmyh>().Init();
        _cameraMain = Camera.main;
    }
    private void SetAimReady(bool isAiming) => _unit.SetAimReady(isAiming);

    private void Shoot() => _unit.Shoot();

    private void OnDisable()
    {
        _input.OnDirection -= SetDirection;
        _input.OnSpace -= OnSpace;
        _input.OnShoot -= Shoot;
        _input.OnAim -= Aim;
        _input.OnSetAim -= SetAimReady;
    }
    private void SetDirection(Vector2 vector) => _unit.SetDirection(vector);
    private void OnSpace() => _unit.OnClimb();
    private void Update() => _unit.Tick();
}
