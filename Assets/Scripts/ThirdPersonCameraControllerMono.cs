using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Windows;
using Zenject;

public class ThirdPersonCameraControllerMono : MonoBehaviour
{
    private CinemachineCamera _cinemachineCamera;
    private CinemachineThirdPersonFollow _personFollow;
    private CinemachineThirdPersonAim _personAim;

    private Zhmyh _unit;
    private IInput _input;

    [Inject]
    public void Construct(IInput input, PlayerZhmyhBrian player)
    {
        _unit = player.Unit as Zhmyh;
        _input = input;
    }

    private void Awake()
    {
        _cinemachineCamera = GetComponent<CinemachineCamera>();
        _personFollow = GetComponent<CinemachineThirdPersonFollow>();
        _personAim = GetComponent<CinemachineThirdPersonAim>();

    }
}
