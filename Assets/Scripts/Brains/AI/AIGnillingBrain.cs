using UnityEngine;
using Zenject;

public class AIGnillingBrain : MonoBehaviour, IBrian
{
    public Transform Transform => _unit.Transform;
    private Gniling _unit;
    private Unit _target;

    [Inject]
    public void Construct(PlayerUnitBrian player)
    {
        _target = player.Unit;
    }
    public void Init()
    {
        _unit = GetComponent<Gniling>().Init() as Gniling;
    }
    private void Update()
    {
        var delta = new Vector3(_target.Transform.position.x - Transform.position.x,
            0,
            _target.Transform.position.z - Transform.position.z);
        _unit.SetMovementDirection(delta.sqrMagnitude > 60*60 ?
            delta.normalized : Vector3.Lerp(delta.normalized, Vector3.zero, 1-delta.sqrMagnitude/(60 * 60)));
        _unit.Tick();
    }
}
