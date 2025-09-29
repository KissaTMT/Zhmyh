using System;
using System.Collections;
using UnityEngine;
using Zenject;

public class AIUnitBrian : MonoBehaviour, IBrian
{
    public Unit Unit => _unit;
    private Zhmyh _unit;

    private PlayerUnitBrian _player;
    private Unit _target;

    [SerializeField] private LayerMask _layerMask;
    private RaycastHit[] _hits = new RaycastHit[8];
    private Vector3 _shootDirection;
    private Coroutine _shoot;
    private WaitForSeconds _sleep;

    [Inject]
    public void Construct(PlayerUnitBrian player)
    {
        _player = player;
    }
    private void Start()
    {
        _unit = (Zhmyh)GetComponent<Zhmyh>().Init();
        _target = _player.Unit;
        _sleep = new WaitForSeconds(1);
    }
    private void Update()
    {
        _unit.SetLookDirection(CalculateLookDirection());
        _unit.SetMovementDirection(CalculateMovementDirection());
        _unit.Tick();

        if (Time.frameCount % 32 == 0)
        {
            if (TryAim(out _shootDirection) && _shoot == null) _shoot = StartCoroutine(ShootRoutine());
        }
    }

    private Vector3 CalculateMovementDirection()
    {
        var delta = _target.Transform.position - _unit.Transform.position;
        return delta.magnitude > 25 ? new Vector3(delta.x, 0, delta.z).normalized : Vector3.zero;
    }

    private Vector2 CalculateLookDirection()
    {
        var origin = new Vector2(_unit.Transform.position.x, _unit.Transform.position.z);
        var destination = new Vector2(_target.Transform.position.x, _target.Transform.position.z);
        var delta = destination - origin;
        var rotation = _unit.Root.localEulerAngles.y * Mathf.Deg2Rad;

        var cos = Mathf.Cos(rotation);
        var sin = Mathf.Sin(rotation);

        var x = delta.x * cos - delta.y * sin;
        var y = delta.x * sin + delta.y * cos;

        return new Vector2(x,y).normalized;
    }
    private IEnumerator ShootRoutine()
    {
        _unit.SetAim(true);
        _unit.Pull(true);
        yield return _sleep;
        yield return new WaitUntil(ShootDirectionIsNotZero);
        _unit.SetShootDirection(_shootDirection);
        _unit.Pull(false);
        //_unit.SetAim(false);
        _shoot = null;
    }
    private bool ShootDirectionIsNotZero() => _shootDirection != Vector3.zero;
    private bool TryAim(out Vector3 direction)
    {
        direction = Vector3.zero;
        var offset = Vector3.up * 10;
        var origin = _unit.Transform.position + offset;
        var target = _target.Transform.position + offset;
        var delta = target - origin;
        var hitCount = Physics.RaycastNonAlloc(origin, (target - origin).normalized, _hits, delta.sqrMagnitude, _layerMask);

        if (hitCount == 0) return false;
        
        for (var i = 0; i < hitCount; i++)
        {
            var hit = _hits[i];
            if ((hit.point - origin).sqrMagnitude < 32 * 32) continue;
            if (hit.transform.GetComponentInParent<Zhmyh>() != null)
            {
                direction = hit.point;
                return true;
            }
        }
        return false;
    }
}
