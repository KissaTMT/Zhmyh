using System.Collections;
using UnityEngine;

public class ZhmyhAimingState : DecorateState
{
    private MonoBehaviour _context;
    private Transform _transform;
    private Transform _rightHand;
    private Transform _leftHand;
    private Bow _bow;
    private Shifter _shifter;

    private Transform _aim;
    private Transform _body;
    private Vector2 _handPrimeLocalPosition;
    private Vector2 _direction;
    private Coroutine _release;
    private float _tension;
    public ZhmyhAimingState(MonoBehaviour context,Transform transform, Transform rightHand, Transform leftHand, Bow bow, Shifter shifter)
    {
        _context = context;
        _transform = transform;
        _rightHand = rightHand;
        _leftHand = leftHand;
        _bow = bow;
        _shifter = shifter;

        _handPrimeLocalPosition = _rightHand.localPosition;

        _aim = new GameObject("aim").transform;
        Transform(_aim,_shifter.Root,new Vector3(0,5,-1),Vector3.one, Quaternion.identity);

        _body = _bow.Transform.parent;
    }
    public override void Enter()
    {
        base.Enter();

        Run = ReloadRun;

        _aim.gameObject.SetActive(true);

        _shifter.Detach(_bow.Transform);
        _shifter.Detach(_rightHand);
        _shifter.Detach(_leftHand);

        Transform(_rightHand, _aim, Vector3.zero, Vector3.one, Quaternion.identity);
        Transform(_leftHand, _aim, new Vector3(15, 0), Vector3.one, Quaternion.identity);
        Transform(_bow.Transform, _leftHand, new Vector3(0, 0, 0.0105f), Vector3.one, Quaternion.identity);

    }
    public override void Exit()
    {
        base.Exit();

        _rightHand.parent = _shifter.Root;
        _leftHand.parent = _shifter.Root;
        _bow.Transform.parent = _body;

        _shifter.Attach(_bow.Transform);
        _shifter.Attach(_rightHand);
        _shifter.Attach(_leftHand);

        _aim.gameObject.SetActive(false);
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
    public void Shift() => _shifter.Shift(_direction - new Vector2(_transform.position.x, _transform.position.z));
    public override string ToString() => $"{base.ToString()} + {baseState}";
    private void Transform(Transform o,Transform parent, Vector3 position, Vector3 scale,Quaternion rotation)
    {
        o.parent = parent;
        o.localPosition = position;
        o.localScale = scale;
        o.localRotation = rotation;
    }
    private void RotateToDirection()
    {
        var delta = _direction - new Vector2(_transform.position.x, _transform.position.z);
        var angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
        _aim.localRotation = Quaternion.Euler(0f, 0f, angle);
    }
    private void Pull()
    {
        if (_release != null) return;
        if (_tension < 1) _tension += 2* Time.deltaTime;
        _rightHand.localPosition = new Vector2(Mathf.Lerp(_handPrimeLocalPosition.x, _handPrimeLocalPosition.x -27,_tension), _rightHand.localPosition.y);
        _bow.Pull();
    }
    private IEnumerator ReleaseRoutine(float speed)
    {
        for (var i = 0f; i < 1f; i += speed * Time.deltaTime)
        {
            _rightHand.localPosition = Vector2.Lerp(_rightHand.localPosition, _handPrimeLocalPosition, i);
            yield return null;
        }
        _tension = 0;
        _release = null;
    }
}
