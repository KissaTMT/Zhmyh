using UnityEngine;
using UnityEngine.UI;

public class Cursor : MonoBehaviour
{
    public const float SENSITIVITY = 128f;
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
    private bool _isAim;
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

        _input.Pulling += SetAim;
        _isInitialized = true;
    }

    private void InitializeCursorPosition()
    {
        _cashedPosition = new Vector2(Screen.width / 2f, Screen.height / 2f);

        UpdateCursorPosition(Vector2.zero);

        var color = _image.color;
        _image.color = new Color(color.r, color.g, color.b, 0);
        _isAim = false;
    }

    public void Tick()
    {
        if (!_isInitialized) return;
        if (!_isAim) return;

        _rectTransform.Rotate(0, 0, -_rotationSpeed * Time.deltaTime);

        _delta = _input.GetAiming();

        if(_delta == Vector2.zero) return;

        //UpdateCursorPosition(_delta);
    }

    private void SetAim(bool isAim)
    {
        if (!_isInitialized) return;
        if(_isAim == isAim) return;

        var color = _image.color;
        _image.color = new Color(color.r, color.g, color.b, isAim ? 1 : 0);
        _isAim = isAim;

        _cashedPosition = new Vector2(Screen.width / 2f, Screen.height / 2f);
        UpdateCursorPosition(Vector2.zero);
    }

    private void UpdateCursorPosition(Vector2 delta)
    {
        _cashedPosition.x += delta.x * Time.deltaTime * SENSITIVITY;
        _cashedPosition.y += delta.y * Time.deltaTime * SENSITIVITY;

        _cashedPosition.x = Mathf.Clamp(_cashedPosition.x, 0, Screen.width);
        _cashedPosition.y = Mathf.Clamp(_cashedPosition.y, 0, Screen.height);

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
            _input.Pulling -= SetAim;
    }

    private void OnRectTransformDimensionsChange()
    {
        if (_isInitialized)
        {
            UpdateCursorPosition(_delta);
        }
    }
}