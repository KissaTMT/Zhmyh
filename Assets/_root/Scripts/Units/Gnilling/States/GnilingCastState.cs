using R3;
using System.Collections;
using UnityEngine;

public class GnilingCastState : DecorateState
{
    public bool IsCast;
    private Shifter _shifter;
    private Transform _handle;
    private Sword _sword;
    private Vector3 _currentDirection;
    private Vector3 _direction;
    public GnilingCastState(Shifter shifter, Sword sword, Transform handle)
    {
        _shifter = shifter;
        _sword = sword;
        _handle = handle;
    }
    public override void OnEnter()
    {
        _currentDirection = _direction;
        _sword.SetDirection(_currentDirection);
        _sword.StartCoroutine(CastRoutine());
    }
    public void SetDirection(Vector3 direction)
    {
        _direction = direction;
    }
    private IEnumerator CastRoutine()
    {
        IsCast = true;
        _shifter.Detach(_handle);
        var start = _handle.transform.localPosition;
        var end = _handle.transform.localPosition + _handle.transform.forward * 10;
        for(var i = 0f; i < 1; i += 2 * Time.deltaTime)
        {
            _handle.transform.localPosition = Vector3.Lerp(start,end,i);
            yield return null;
        }
        start = end;
        end = start - _handle.transform.forward * 20;
        for (var i = 0f; i < 1; i += 8 * Time.deltaTime)
        {
            _handle.transform.localPosition = Vector3.Lerp(start, end, i);
            yield return null;
        }
        _sword.Cast();
        _shifter.Attach(_handle);
        start = end;
        end = start + _handle.transform.forward * 10;
        for (var i = 0f; i < 1; i += 2 * Time.deltaTime)
        {
            _handle.transform.localPosition = Vector3.Lerp(start, end, i);
            yield return null;
        }
        yield return new WaitUntil(() => _sword.State == Sword.SwordState.Own);
        yield return new WaitForSeconds(0.25f);
        IsCast = false;
    }
}
