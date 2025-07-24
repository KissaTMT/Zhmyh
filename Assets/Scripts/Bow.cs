using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bow : MonoBehaviour
{
    public Transform Transform => _transform;
    public float Tension => _tension;
    [SerializeField] private Arrow _arrow;
    [SerializeField] private Transform _shootPoint;
    [SerializeField] private Transform _topPart;
    [SerializeField] private Transform _bottomPart;
    [SerializeField] private Transform _handlePart;

    private float _topPartPrimeAngle;
    private float _bottomPartPrimeAngle;

    private float _tension;
    private Coroutine _release;
    private Transform _transform;

    private void Awake()
    {
        _topPartPrimeAngle = _topPart.localRotation.eulerAngles.z;
        _bottomPartPrimeAngle = _bottomPart.localRotation.eulerAngles.z;
        _transform = GetComponent<Transform>();
    }
    public void Pull()
    {
        if (_release != null)
        {
            StopCoroutine(_release);
            _release = null;
        }
        if (_tension < 1) _tension += 2 * Time.deltaTime;
        _topPart.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(_topPartPrimeAngle, _topPartPrimeAngle + 32, _tension));
        _bottomPart.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(_bottomPartPrimeAngle, _bottomPartPrimeAngle - 32, _tension));
    }
    public void Release(Vector2 direction)
    {
        _release = StartCoroutine(ReleaseRoutine(10*_tension));
        if (_tension < 0.9f) return;
        var arrow = Instantiate(_arrow, _shootPoint.position, Quaternion.identity).GetComponent<Arrow>();
        arrow.Init(direction);
    }
    public void Cancel()
    {
        _release = StartCoroutine(ReleaseRoutine(3));
    }
    private IEnumerator ReleaseRoutine(float speed)
    {
        for(var i = 0f; i < 1f; i += speed * Time.deltaTime)
        {
            _topPart.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(_topPart.localRotation.eulerAngles.z, _topPartPrimeAngle, i));
            _bottomPart.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(_bottomPart.localRotation.eulerAngles.z, _bottomPartPrimeAngle, i));
            yield return null;
        }
        _tension = 0;
        _release = null;
    }
}
