using System;
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
    private Vector3 _shootTarget;
    private Vector2 _lookDirection;
    private Coroutine _release;
    private float _tension;
    private bool _isPull;
    public ZhmyhAimingState(MonoBehaviour context,Transform transform, Transform rightHand, Transform leftHand, Bow bow, Shifter shifter)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));
        if (transform == null) throw new ArgumentNullException(nameof(transform));
        if (rightHand == null) throw new ArgumentNullException(nameof(rightHand));
        if (leftHand == null) throw new ArgumentNullException(nameof(leftHand));
        if (bow == null) throw new ArgumentNullException(nameof(bow));
        if (shifter == null) throw new ArgumentNullException(nameof(shifter));

        _context = context;
        _transform = transform;
        _rightHand = rightHand;
        _leftHand = leftHand;
        _bow = bow;
        _shifter = shifter;

        _handPrimeLocalPosition = _rightHand.localPosition;

        _aim = new GameObject("aim").transform;
        Transform(_aim,_shifter.Root,new Vector3(0,10,-1),Vector3.one, Quaternion.Euler(-_shifter.Root.localEulerAngles.x + 90, 0, 0));

        _body = _bow.Transform.parent;
    }
    public override void OnEnter()
    {
        _aim.gameObject.SetActive(true);

        _shifter.Detach(_bow.Transform);
        _shifter.Detach(_rightHand);
        _shifter.Detach(_leftHand);

        Transform(_rightHand, _aim, Vector3.zero, Vector3.one, Quaternion.identity);
        Transform(_leftHand, _aim, new Vector3(20, 0), Vector3.one, Quaternion.identity);
        Transform(_bow.Transform, _leftHand, new Vector3(0, 0, 0.0105f), new Vector3(0.75f,1.25f,1.25f), Quaternion.identity);

    }
    public override void OnExit()
    {
        _rightHand.parent = _shifter.Root;
        _leftHand.parent = _shifter.Root;
        _bow.Transform.parent = _body;

        _rightHand.localRotation = Quaternion.identity;
        _leftHand.localRotation = Quaternion.identity;
        _bow.Transform.localRotation = Quaternion.identity;

        _shifter.Attach(_bow.Transform);
        _shifter.Attach(_rightHand);
        _shifter.Attach(_leftHand);

        _aim.gameObject.SetActive(false);
    }
    public override void OnTick()
    {
        Aiming();
        Shift();
    }
    public void SetLookDirection(Vector2 delta)
    {
        _lookDirection = delta;
    }
    public void SetPull(bool isPull)
    {
        _isPull = isPull;
    }
    public void Aiming()
    {
        RotateToDirection();
        if (_isPull) Pull();
    }
    public void Release()
    {
        _release = _context.StartCoroutine(ReleaseRoutine(4));
        _bow.Release(_shootTarget);
    }
    public void Shift() => _shifter.Shift(_lookDirection);
    public override string ToString() => $"{base.ToString()} + {BaseState}";
    private void Transform(Transform o,Transform parent, Vector3 position, Vector3 scale,Quaternion rotation)
    {
        o.parent = parent;
        o.localPosition = position;
        o.localScale = scale;
        o.localRotation = rotation;
    }
    private void Pull()
    {
        if (_release != null) return;
        if (_tension < 1) _tension += 2 * Time.deltaTime;
        _rightHand.localPosition = new Vector2(Mathf.Lerp(_handPrimeLocalPosition.x, _handPrimeLocalPosition.x - 27, _tension), _rightHand.localPosition.y);
        _shootTarget = CalculateShootTarget();
        _bow.Pull();
    }
    private Vector3 CalculateShootTarget()
    {
        var ray = Camera.main.ScreenPointToRay(_lookDirection + (Vector2)Camera.main.WorldToScreenPoint(_transform.position));
        var point = Vector3.zero;
        if (Physics.Raycast(ray, out var hit)) point = hit.point;
        else point = ray.GetPoint(10);
        Debug.DrawLine(_bow.ShootPoint, point);
        return point;
    }
    
    private void RotateToDirection()
    {
        var rotation = _shifter.Root.localEulerAngles.y * Mathf.Deg2Rad;

        var angle = Mathf.Atan2(_lookDirection.y, _lookDirection.x) * Mathf.Rad2Deg;
        _aim.localRotation = Quaternion.Euler(-_shifter.Root.localEulerAngles.x + 90, 0, angle);
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
