using System.Collections;
using UnityEngine;
using Zenject;

public class AIZhmyhBrian : MonoBehaviour, IBrian
{
    public Zhmyh Unit => _unit;
    private Zhmyh _unit;

    private PlayerZhmyhBrian _player;
    private Unit _target;

    [SerializeField] private LayerMask _layerMask;
    [SerializeField] private Arrow _arrow;
    private RaycastHit[] _hits = new RaycastHit[8];
    private Vector3 _shootDirection;
    private Coroutine _shoot;

    [Inject]
    public void Construct(PlayerZhmyhBrian player)
    {
        _player = player;
    }
    private void Start()
    {
        _unit = GetComponent<Zhmyh>().Init() as Zhmyh;
        _target = _player.Unit;
    }
    private void Update()
    {
        _unit.SetLookDirection(CalculateLookDirection());
        _unit.SetMovementDirection(CalculateMovementDirection());
        _unit.Tick();
        if (Time.frameCount % 120 == 0)
        {
            if (TryAim() && _shoot == null) _shoot = StartCoroutine(ShootRoutine());
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
        CalculateTrajectory(out var direction, out var tension);
        _unit.Pull(true);
        yield return new WaitUntil(() => _unit.Bow.Tension >= tension);
        _unit.SetShootDirection(direction);
        _unit.Pull(false);
        _shoot = null;
    }
    private bool ShootDirectionIsNotZero() => _shootDirection != Vector3.zero;
    private bool TryAim()
    {
        var offset = Vector3.up * 0;
        var origin = _unit.Transform.position + offset;
        var target = _target.Transform.position + offset;
        var delta = target - origin;
        var hitCount = Physics.RaycastNonAlloc(origin, delta.normalized, _hits, delta.sqrMagnitude, _layerMask);

        return hitCount == 0;
    }
    private void CalculateTrajectory(out Vector3 direction, out float tension)
    {
        var offset = Vector3.up * 10;
        var start = _unit.Transform.position + offset;
        var target = _target.Transform.position + offset;
        var delta = target - start;
        var deltaXZ = new Vector3(delta.x, 0, delta.z);
        var distanceXZ = deltaXZ.magnitude;
        var heightDiff = delta.y;

        var baseSpeed = _arrow.Speed;
        var g = _arrow.GravityScale;

        var baseTension = distanceXZ / 40f;

        baseTension *= heightDiff <= 0 ? (1 - heightDiff / 40f) : (1 + heightDiff / 20f);
        tension = Mathf.Clamp(baseTension, 0.25f, 1f);

        var v0 = baseSpeed * tension;

        var a = (g * distanceXZ * distanceXZ) / (2 * v0 * v0);
        var b = -distanceXZ;
        var c = a + heightDiff;

        var discriminant = b * b - 4 * a * c;

        if (discriminant < 0)
        {
            var angle45 = Mathf.Deg2Rad * 45;
            direction = (deltaXZ.normalized * Mathf.Cos(angle45) + Vector3.up * Mathf.Sin(angle45)).normalized;
            return;
        }

        var sqrtD = Mathf.Sqrt(discriminant);
        var tanTheta1 = (-b + sqrtD) / (2 * a);
        var tanTheta2 = (-b - sqrtD) / (2 * a);

        var tanTheta = (Mathf.Abs(tanTheta1) < Mathf.Abs(tanTheta2)) ? tanTheta1 : tanTheta2;
        var angle = Mathf.Atan(tanTheta);
        direction = (deltaXZ.normalized * Mathf.Cos(angle) + Vector3.up * Mathf.Sin(angle)).normalized;
    }
}
