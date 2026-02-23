using System;
using System.Collections;
using UnityEngine;

public class Bow : MonoBehaviour
{
    public Vector3 ShootPoint => _shootPoint.transform.position;
    public Transform Transform => _transform;
    public float Tension => _tension;
    [SerializeField] private Arrow _arrow;
    [SerializeField] private Transform _shootPoint;
    [SerializeField] private Transform[] _topParts;
    [SerializeField] private Transform[] _bottomParts;
    [SerializeField] private Transform _handlePart;

    private float[] _topPartPrimeAngles;
    private float[] _bottomPartPrimeAngles;

    private float _tension;
    private Coroutine _release;
    private Transform _transform;

    public void Pull()
    {
        if (_release != null) return;

        _tension = Mathf.Clamp01(_tension + 2 * Time.deltaTime);

        for (var i = 0; i < _topParts.Length; i++)
        {
            BowPartHandle(_topParts[i], Mathf.Lerp(_topPartPrimeAngles[i], _topPartPrimeAngles[i] + 32, _tension));
            BowPartHandle(_bottomParts[i], Mathf.Lerp(_bottomPartPrimeAngles[i], _bottomPartPrimeAngles[i] - 32, _tension));
        }
    }
    public void Release(Vector3 target)
    {
        Cancel(1 + 10 * _tension);

        if (_tension > 0.25f) Shoot(target);

        _tension = 0;
    }
    private void Awake()
    {
        _topPartPrimeAngles = new float[_topParts.Length];
        _bottomPartPrimeAngles = new float[_bottomParts.Length];

        for (var i = 0; i < _topParts.Length; i++)
        {
            _topPartPrimeAngles[i] = _topParts[i].transform.localEulerAngles.z;
            _bottomPartPrimeAngles[i] = _bottomParts[i].transform.localEulerAngles.z;
        }

        _transform = GetComponent<Transform>();
    }
    private void Shoot(Vector3 target)
    {
        var direction = CalculateDirectionToTarget(target);
        Instantiate(_arrow, _shootPoint.position, Quaternion.identity).GetComponent<Arrow>().Init(direction, _tension);
    }
    private Vector3 CalculateDirectionToTarget(Vector3 target)
    {
        var delta = target - _transform.position;

        var sqrDistance = delta.sqrMagnitude;
        var sqrDistanceXZ = new Vector2(delta.x,delta.z).sqrMagnitude;
        var sqrDistanceY = delta.y * delta.y;

        var maxSqrDistanceXZ = 32 * 32;
        var maxSqrDistanceY = 2 * 2;

        var direction = delta.normalized;

        //if (delta.y > 0 && sqrDistanceXZ < maxSqrDistanceXZ && sqrDistanceY < maxSqrDistanceY)
        //{
        //    direction.y *= Mathf.Lerp(1, 0.1f, 1 - sqrDistanceXZ / maxSqrDistanceXZ);
        //}

        return direction;
    }
    private void Cancel(float speed = 3)
    {
        _release = StartCoroutine(ReleaseRoutine(speed));
    }
    private void BowPartHandle(Transform part, float eulerZ)
    {
        part.transform.localRotation = 
            Quaternion.Euler(part.transform.localEulerAngles.x, part.transform.localEulerAngles.y, eulerZ);
    }
    private IEnumerator ReleaseRoutine(float speed)
    {
        for(var t = 0f; t < 1f; t += speed * Time.deltaTime)
        {
            for (var i = 0; i < _topParts.Length; i++)
            {
                BowPartHandle(_topParts[i], Mathf.Lerp(_topParts[i].localEulerAngles.z, _topPartPrimeAngles[i], t));
                BowPartHandle(_bottomParts[i], Mathf.Lerp(_bottomParts[i].localEulerAngles.z, _bottomPartPrimeAngles[i], t));
            }
            yield return null;
        }
        for (var i = 0; i < _topParts.Length; i++)
        {
            BowPartHandle(_topParts[i], _topPartPrimeAngles[i]);
            BowPartHandle(_bottomParts[i], _bottomPartPrimeAngles[i]);
        }
        _release = null;
    }
}
