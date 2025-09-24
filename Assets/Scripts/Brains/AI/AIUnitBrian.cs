using System;
using UnityEngine;
using Zenject;

public class AIUnitBrian : MonoBehaviour, IBrian
{
    public Unit Unit => _unit;
    private Zhmyh _unit;

    private PlayerUnitBrian _player;
    private Unit _target;

    [Inject]
    public void Construct(PlayerUnitBrian player)
    {
        _player = player;
    }
    private void Start()
    {
        _unit = (Zhmyh)GetComponent<Zhmyh>().Init();
        _target = _player.Unit;
    }
    private void Update()
    {
        _unit.SetLookDirection(CalculateLookDirection());
        _unit.SetMovementDirection(CalculateMovementDirection());
        _unit.Tick();
    }

    private Vector3 CalculateMovementDirection()
    {
        var delta = _target.Transform.position - _unit.Transform.position;
        return delta.magnitude > 25 ? new Vector3(delta.x, 0, delta.z).normalized : Vector3.zero;
    }

    private Vector2 CalculateLookDirection()
    {
        return (_target.Transform.position - _unit.Transform.position).normalized;
    }
}
