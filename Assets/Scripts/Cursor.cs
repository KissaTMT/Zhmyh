using UnityEngine;
using UnityEngine.UI;

public class Cursor : MonoBehaviour
{
    public const float SENSIVITY = 100f;
    public Vector2 ScreenPosition => _cashedPosition;

    [SerializeField] private float _rotationSpeed;
    
    private Canvas _canvas;
    private Camera _cameraMian;
    private RectTransform _canvasRectTransform;
    private RectTransform _rectTransform;
    private Image _image; 

    private IInput _input;
    private Vector2 _cashedPosition;
    private Vector2 _delta;
    private bool _locked;
    public void Init(IInput input)
    {
        _input = input;

        _rectTransform = GetComponent<RectTransform>();
        _image = GetComponent<Image>();
        _canvas = GetComponentInParent<Canvas>();
        _canvasRectTransform = _canvas.GetComponent<RectTransform>();
        _cameraMian = Camera.main;

        _cashedPosition = new Vector2(Screen.width / 2, Screen.height / 2);
        SetAim(false);

        _input.Aiming += SetDelta;
        _input.SetAim += SetAim;
    }
    public void Tick()
    {
        if (_locked) return;

        _rectTransform.Rotate(0, 0, -_rotationSpeed * Time.deltaTime);

        if (_delta == Vector2.zero) return;

        _rectTransform.anchoredPosition = CalculateAuchorPosition();
    }

    private void SetAim(bool isAim)
    {
        var color = _image.color;
        _image.color = new Color(color.r, color.g, color.b, isAim ? 1 : 0);
        _locked = !isAim;
    }
    private void OnDisable()
    {
        _input.Aiming -= SetDelta;
        _input.SetAim -= SetAim;
    }
    private void SetDelta(Vector2 delta)
    {
        _delta = delta;
    }
    private Vector2 CalculateAuchorPosition()
    {
        _cashedPosition += _delta * Time.deltaTime * SENSIVITY;

        _cashedPosition.x = Mathf.Clamp(_cashedPosition.x, 0, Screen.width);
        _cashedPosition.y = Mathf.Clamp(_cashedPosition.y, 0, Screen.height);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRectTransform, _cashedPosition,
            _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _cameraMian, out var achoredPosition);
        return achoredPosition;
    }
}
