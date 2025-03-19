using UnityEngine;
using Zenject;

public class Cursor : MonoBehaviour
{
    [SerializeField] private float _rotationSpeed;
    
    private Canvas _canvas;
    private RectTransform _canvasRectTransform;
    private RectTransform _rectTransform;

    private IInput _input;

    [Inject]
    public void Construct(IInput input)
    {
        _input = input;
        _input.OnAim += Aim;
    }
    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _canvas = GetComponentInParent<Canvas>();
        _canvasRectTransform = _canvas.GetComponent<RectTransform>();
    }
    private void OnDisable()
    {
        _input.OnAim -= Aim;
    }

    private void Aim(Vector2 position)
    {
        AuchorPosition(position);
    }
    private void AuchorPosition(Vector2 position)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRectTransform,position, _canvas.renderMode == RenderMode.ScreenSpaceOverlay?null:Camera.main,out var achoredPosition);
        _rectTransform.anchoredPosition = achoredPosition;
    }
    private void Update()
    {
        _rectTransform.Rotate(0,0, -_rotationSpeed * Time.deltaTime);
    }
}
