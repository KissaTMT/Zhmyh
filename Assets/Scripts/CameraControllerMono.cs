using Unity.Cinemachine;
using UnityEngine;
using Zenject;

public class CameraControllerMono : MonoBehaviour
{
    private const float SENSIVITY = 10;
    public Transform Transform => _cinemachineCamera.transform;
    private CinemachineCamera _cinemachineCamera;
    private CinemachineFollow _cinemachineFollow;
    private PlayerUnitBrian _player;
    private Zhmyh _unit;
    private IInput _input;

    private Vector3 _mainOffset;
    private bool _locked;
    private Vector2 _delta;
    private Coroutine _rotateRoutine;

    [Inject]
    public void Construct(IInput input, PlayerUnitBrian player)
    {
        _player = player;
        _input = input;

        _input.Aiming += SetDelta;
        _input.SetAim += SetLock;
    }

    private void Awake()
    {
        _cinemachineCamera = GetComponent<CinemachineCamera>();
        _cinemachineFollow = GetComponent<CinemachineFollow>();
        _cinemachineCamera.Follow = _player.Transform;
        _unit = _player.Unit as Zhmyh;

        _mainOffset = _cinemachineFollow.FollowOffset;
    }
    private void OnDisable()
    {
        _input.Aiming -= SetDelta;
        _input.SetAim -= SetLock;
    }
    private void SetDelta(Vector2 delta) => _delta = delta;
    private void SetLock(bool locked)
    {
        _locked = locked;
        //if(_locked) _rotateRoutine = StartCoroutine(RotateToRoutine());
        //else StopCoroutine(_rotateRoutine);
    }
    private void Rotate(float delta)
    {
        _cinemachineCamera.transform.RotateAround(_player.Transform.position, Vector3.up, delta * SENSIVITY * Time.deltaTime);
        var angle = _cinemachineCamera.transform.eulerAngles.y * Mathf.Deg2Rad;
        var cos = Mathf.Cos(angle);
        var sin = Mathf.Sin(angle);
        _cinemachineFollow.FollowOffset = new Vector3(
            cos * _mainOffset.x + sin * _mainOffset.z,
            _mainOffset.y,
            -sin * _mainOffset.x + cos * _mainOffset.z);
        
    }
    //private IEnumerator RotateToRoutine()
    //{
    //    var angle = 0f;
    //    while (Mathf.Abs(_cinemachineCamera.transform.eulerAngles.y - angle) > 2f)
    //    {
    //        Rotate(20);
    //        yield return null;
    //    }
    //    _cinemachineCamera.transform.eulerAngles = new Vector3(_cinemachineCamera.transform.eulerAngles.x, 
    //        angle, 
    //        _cinemachineCamera.transform.eulerAngles.z);
    //}
    private void LateUpdate()
    {
        if (_locked || _delta == Vector2.zero) return;
        Rotate(_delta.x);
    }
}

