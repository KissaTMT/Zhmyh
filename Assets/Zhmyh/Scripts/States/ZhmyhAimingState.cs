using System.Collections;
using UnityEngine;

public class ZhmyhAimingState : DecorateState
{
    private MonoBehaviour _context;
    private Transform _transform;
    private Transform _aim;
    private Bow _bow;
    private Shifter _shifter;

    private Transform _hand;
    private Vector2 _handPrimeLocalPosition;
    private Vector2 _direction;

    private Coroutine _release;
    private float _tension;
    public ZhmyhAimingState(MonoBehaviour context,Transform transform, Transform aim, Bow bow, Shifter shifter)
    {
        _context = context;
        _transform = transform;
        _aim = aim;
        _bow = bow;
        _shifter = shifter;

        _hand = aim.GetChild(0);
        _handPrimeLocalPosition = _hand.localPosition;
    }
    public override void Enter()
    {
        base.Enter();
        Run = ReloadRun;
    }
    public override void Exit()
    {
        base.Exit();
    }
    public override void ReloadRun()
    {
        Aiming();
        Shift();
    }
    public void SetDirection(Vector2 direction)
    {
        _direction = direction;
    }
    public void Aiming()
    {
        RotateToDirection();
        Pull();
    }
    public void Shoot()
    {
        _release = _context.StartCoroutine(ReleaseRoutine(4));
        _bow.Release(_direction);
    }
    public void Shift() => _shifter.Shift(_direction - (Vector2)_transform.position);
    public override string ToString() => $"{base.ToString()} + {baseState}";

    private void RotateToDirection()
    {
        var delta = _direction - (Vector2)_transform.position;
        var angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
        _aim.rotation = Quaternion.Euler(0f, 0f, angle);
    }
    private void Pull()
    {
        if (_release != null) return;
        if (_tension < 1) _tension += 2* Time.deltaTime;
        _hand.localPosition = new Vector2(Mathf.Lerp(_handPrimeLocalPosition.x, _handPrimeLocalPosition.x -4,_tension),_hand.localPosition.y);
        _bow.Pull();
    }
    private IEnumerator ReleaseRoutine(float speed)
    {
        for (var i = 0f; i < 1f; i += speed * Time.deltaTime)
        {
            _hand.localPosition = Vector2.Lerp(_hand.localPosition, _handPrimeLocalPosition, i);
            yield return null;
        }
        _tension = 0;
        _release = null;
    }
}
