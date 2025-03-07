using UnityEngine;
using Zenject;

public class Cursor : MonoBehaviour
{
    public RectTransform RectTransform => _crosshairRectTransform;
    [SerializeField] private float _mouseSensitivity = 0.01f;
    [SerializeField] private float _gamepadSensitivity = 100f;
    [SerializeField] private float _rotationSpeed;
    private IInput _input;
    private RectTransform _crosshairRectTransform;
    private Vector2 _delta;

    [Inject]
    public void Construct(IInput input)
    {
        _input = input;
        _input.OnAim += Aim;
    }
    private void Awake()
    {
        _crosshairRectTransform = GetComponent<RectTransform>();
    }
    private void OnDisable()
    {
        _input.OnAim -= Aim;
    }

    private void Aim(Vector2 delta)
    {
        _delta = delta;
    }
    private void Update()
    {
        _crosshairRectTransform.localPosition += (Vector3)_delta * _mouseSensitivity;
        _crosshairRectTransform.Rotate(0,0, -_rotationSpeed * Time.deltaTime);
    }
}
