using System.Collections;
using UnityEngine;
using Zenject;

public class AIGnillingBrain : MonoBehaviour, IBrian
{
    public Transform Transform => _unit.Transform;
    private Gniling _unit;
    private Unit _target;

    private bool _enableCasting = false;
    [Inject]
    public void Construct(PlayerZhmyhBrian player)
    {
        _target = player.Unit;
    }
    public void Init()
    {
        _unit = GetComponent<Gniling>().Init() as Gniling;
        StartCoroutine(WaitRoutine());
    }
    private Vector3 CalculateDirection()
    {
        var delta =  new Vector3(_target.Transform.position.x - Transform.position.x,
            0,
            _target.Transform.position.z - Transform.position.z);
        return delta.sqrMagnitude > 60 * 60 ?
            delta.normalized : Vector3.Lerp(delta.normalized, Vector3.zero, 1 - delta.sqrMagnitude / (60 * 60));
    }
    private Vector3 CalculateCastDirection()
    {
        return new Vector3(_target.Transform.position.x - Transform.position.x,
            0,
            _target.Transform.position.z - Transform.position.z).normalized;
    }
    private IEnumerator WaitRoutine()
    {
        yield return new WaitForSeconds(2);
        _enableCasting = true;
    }
    private void Update()
    {
        _unit.SetMovementDirection(CalculateDirection());
        _unit.SetCastDirection(CalculateCastDirection());
        if(_enableCasting) _unit.Cast();
        _unit.Tick();
    }
}
