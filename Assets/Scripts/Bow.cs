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

    private void Awake()
    {
        _topPartPrimeAngles = new float[_topParts.Length];
        _bottomPartPrimeAngles = new float[_bottomParts.Length];

        for(var i = 0; i < _topParts.Length; i++)
        {
            _topPartPrimeAngles[i] = _topParts[i].transform.localEulerAngles.z;
            _bottomPartPrimeAngles[i] = _bottomParts[i].transform.localEulerAngles.z;
        }

        _transform = GetComponent<Transform>();
    }
    public void Pull()
    {
        if (_release != null) return;

        _tension = Mathf.Clamp01(_tension + 2 * Time.deltaTime);

        for (var i = 0; i < _topParts.Length; i++)
        {
            _topParts[i].localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(_topPartPrimeAngles[i], _topPartPrimeAngles[i] + 32, _tension));
            _bottomParts[i].localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(_bottomPartPrimeAngles[i], _bottomPartPrimeAngles[i] - 32, _tension));
        }
    }
    public void Release(Vector3 direction)
    {
        Cancel(1 + 10 * _tension);
        if (_tension > 0.25f) Instantiate(_arrow, _shootPoint.position, Quaternion.identity).GetComponent<Arrow>().Init(direction, _tension);
        _tension = 0;
    }
    public void Cancel(float speed = 3)
    {
        _release = StartCoroutine(ReleaseRoutine(speed));
    }
    private IEnumerator ReleaseRoutine(float speed)
    {
        for(var t = 0f; t < 1f; t += speed * Time.deltaTime)
        {
            for (var i = 0; i < _topParts.Length; i++)
            {
                _topParts[i].localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(_topParts[i].localEulerAngles.z, _topPartPrimeAngles[i], t));
                _bottomParts[i].localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(_bottomParts[i].localEulerAngles.z, _bottomPartPrimeAngles[i], t));
            }
            yield return null;
        }
        for (var i = 0; i < _topParts.Length; i++)
        {
            _topParts[i].localRotation = Quaternion.Euler(0, 0, _topPartPrimeAngles[i]);
            _bottomParts[i].localRotation = Quaternion.Euler(0, 0, _bottomPartPrimeAngles[i]);
        }
        _release = null;
    }
}
