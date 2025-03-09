using Cinemachine;
using UnityEngine;
using Zenject;

public class CameraMover : MonoBehaviour
{
    private CinemachineVirtualCamera _virtualCamera;
    private Player _player;

    [Inject]
    public void Construct(Player player)
    {
        _player = player;
        _virtualCamera = GetComponent<CinemachineVirtualCamera>();
        _virtualCamera.Follow = _player.Transform;
    }
}
