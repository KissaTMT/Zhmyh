using Unity.Cinemachine;
using UnityEngine;
using Zenject;

public class CameraControllerMono : MonoBehaviour
{
    public Transform Transform => _cinemachineCamera.transform;
    private CinemachineCamera _cinemachineCamera;
    private CinemachineFollow _cinemachineFollow;
    private Player _player;
    private IInput _input;

    private Vector3 _mainOffset;


    [Inject]
    public void Construct(IInput input, Player player)
    {
        _player = player;
        _input = input;

        _input.OnAimDelta += Rotate;

    }
    private void Awake()
    {
        _cinemachineCamera = GetComponent<CinemachineCamera>();
        _cinemachineFollow = GetComponent<CinemachineFollow>();
        _cinemachineCamera.Follow = _player.Transform;

        _mainOffset = _cinemachineFollow.FollowOffset = new Vector3(0, 100, -170);
    }
    private void OnDisable()
    {
        _input.OnAimDelta -= Rotate;
    }
    private void Rotate(Vector2 delta)
    {
        _cinemachineCamera.transform.RotateAround(_player.Transform.position, Vector3.up, delta.x * 10 * Time.deltaTime);
        var angle = _cinemachineCamera.transform.eulerAngles.y * Mathf.Deg2Rad;
        _cinemachineFollow.FollowOffset = new Vector3(
            Mathf.Cos(angle) * _mainOffset.x + Mathf.Sin(angle) * _mainOffset.z,
            _mainOffset.y,
            -Mathf.Sin(angle) * _mainOffset.x + Mathf.Cos(angle) * _mainOffset.z);
    }
}

