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
    private Vector3 _handPrimeLocalPosition;
    private Vector3 _shootDirection;
    private Vector3 _lookDirection;
    private Coroutine _release;
    private bool _isPull;
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
        Transform(_aim,_shifter.Root,new Vector3(0,10,-1),Vector3.one, Quaternion.Euler(-_shifter.Root.localEulerAngles.x + 90, 0, 0));

        _body = _bow.Transform.parent;
    }
    public override void OnEnter()
    {
        Detach();
    }

    public override void OnExit()
    {
        Attach();
    }
    public override void OnTick()
    {
        Aiming();
        Shift();
    }
    public void SetLookDirection(Vector3 delta)
    {
        _lookDirection = delta;
    }
    public void SetShootDirection(Vector3 shootDirection)
    {
        _shootDirection = shootDirection;
    }
    public void SetPull(bool isPull)
    {
        _isPull = isPull;
        if (!_isPull) Release();
    }
    public void Aiming()
    {
        RotateToDirection();
        if (_isPull) Pull();
    }
    public void Release()
    {
        _release = _context.StartCoroutine(ReleaseRoutine(1 + _bow.Tension * 10));
        _bow.Release(_shootDirection);
    }
    public void Shift() => _shifter.Shift(_lookDirection);
    public override string ToString() => $"{base.ToString()} + {BaseState}";
    private void Detach()
    {
        _aim.gameObject.SetActive(true);

        _shifter.Detach(_bow.Transform, false);
        _shifter.Detach(_rightHand);
        _shifter.Detach(_leftHand);

        Transform(_rightHand, _aim, Vector3.zero, Vector3.one, Quaternion.identity);
        Transform(_leftHand, _aim, new Vector3(20, 0), Vector3.one, Quaternion.identity);
        Transform(_bow.Transform, _leftHand, new Vector3(0, 0, 0.0105f), Vector3.one * _bow.Transform.localScale.x, Quaternion.identity);
    }
    private void Attach()
    {
        _rightHand.parent = _shifter.Root;
        _leftHand.parent = _shifter.Root;
        _bow.Transform.parent = _body;

        _rightHand.localRotation = Quaternion.identity;
        _leftHand.localRotation = Quaternion.identity;
        _bow.Transform.localRotation = Quaternion.identity;

        _shifter.Attach(_bow.Transform, false);
        _shifter.Attach(_rightHand);
        _shifter.Attach(_leftHand);

        _aim.gameObject.SetActive(false);
    }
    private void Transform(Transform o,Transform parent, Vector3 position, Vector3 scale, Quaternion rotation)
    {
        o.parent = parent;
        o.localPosition = position;
        o.localScale = scale;
        o.localRotation = rotation;
    }
    private void Pull()
    {
        if (_release != null) return;
        
        _bow.Pull();
        _rightHand.localPosition = new Vector2(Mathf.Lerp(_handPrimeLocalPosition.x, _handPrimeLocalPosition.x - 27, _bow.Tension), _rightHand.localPosition.y);
    }
    
    private void RotateToDirection()
    {
        var rotation = _shifter.Root.localEulerAngles.y * Mathf.Deg2Rad;

        var z = Mathf.Atan2(_lookDirection.y, _lookDirection.x) * Mathf.Rad2Deg;
        _aim.localRotation = Quaternion.Euler(-_shifter.Root.localEulerAngles.x + 90, 0, z);

    }
   
    private IEnumerator ReleaseRoutine(float speed)
    {
        for (var i = 0f; i < 1f; i += speed * Time.deltaTime)
        {
            _rightHand.localPosition = Vector2.Lerp(_rightHand.localPosition, _handPrimeLocalPosition, i);
            yield return null;
        }
        _rightHand.localPosition = _handPrimeLocalPosition;
        _release = null;
    }
}
