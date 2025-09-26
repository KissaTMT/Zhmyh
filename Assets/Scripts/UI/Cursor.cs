using UnityEngine;
using UnityEngine.UI;

public class Cursor : MonoBehaviour
{
    public const float SENSITIVITY = 80f;
    public Vector2 ScreenPosition => _cashedPosition;

    [SerializeField] private float _rotationSpeed;

    private Canvas _canvas;
    private Camera _cameraMain;
    private RectTransform _canvasRectTransform;
    private RectTransform _rectTransform;
    private Image _image;

    private IInput _input;
    private Vector2 _cashedPosition;
    private Vector2 _delta;
    private bool _locked;
    private bool _isInitialized = false;

    public void Init(IInput input)
    {
        _input = input;

        _rectTransform = GetComponent<RectTransform>();
        _image = GetComponent<Image>();
        _canvas = GetComponentInParent<Canvas>();
        _canvasRectTransform = _canvas.GetComponent<RectTransform>();
        _cameraMain = Camera.main;

        InitializeCursorPosition();

        _input.InitAiming += SetAim;
        _isInitialized = true;
    }

    private void InitializeCursorPosition()
    {
        _cashedPosition = new Vector2(Screen.width / 2f, Screen.height / 2f);

        UpdateCursorPosition();

        var color = _image.color;
        _image.color = new Color(color.r, color.g, color.b, 0);
        _locked = true;
    }

    public void Tick()
    {
        if (!_isInitialized) return;
        if (_locked) return;

        _rectTransform.Rotate(0, 0, -_rotationSpeed * Time.deltaTime);

        _delta = _input.GetAiming();

        UpdateCursorPosition();
    }

    private void SetAim(bool isAim)
    {
        if (!_isInitialized) return;

        var color = _image.color;
        _image.color = new Color(color.r, color.g, color.b, isAim ? 1 : 0);
        _locked = !isAim;

        _cashedPosition = new Vector2(Screen.width / 2f, Screen.height - Screen.height / 4f);
        UpdateCursorPosition();
    }

    private void UpdateCursorPosition()
    {
        _cashedPosition.x = Mathf.Clamp(_cashedPosition.x, 0, Screen.width);
        _cashedPosition.y = Mathf.Clamp(_cashedPosition.y, 0, Screen.height);

        if (!_locked && _delta != Vector2.zero)
        {
            _cashedPosition.x += 0.2f * _delta.x * Time.deltaTime * SENSITIVITY;
            _cashedPosition.y += _delta.y * Time.deltaTime * SENSITIVITY;

            _cashedPosition.x = Mathf.Clamp(_cashedPosition.x, 0, Screen.width);
            _cashedPosition.y = Mathf.Clamp(_cashedPosition.y, 0, Screen.height);
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvasRectTransform,
            _cashedPosition,
            _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _cameraMain,
            out var anchoredPosition
        );

        _rectTransform.anchoredPosition = anchoredPosition;
    }

    private void Start()
    {
        if (!_isInitialized)
        {
            InitializeCursorPosition();
        }
    }

    private void OnDisable()
    {
        if (_input != null)
            _input.InitAiming -= SetAim;
    }

    private void OnRectTransformDimensionsChange()
    {
        if (_isInitialized)
        {
            UpdateCursorPosition();
        }
    }
}