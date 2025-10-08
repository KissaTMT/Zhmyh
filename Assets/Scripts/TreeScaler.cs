using UnityEngine;

public class TreeScaler : MonoBehaviour
{
    [SerializeField] private Vector2 _scaleRange;
    private Transform _root;

    private void Awake()
    {
        _root = transform.GetChild(0);
        var scaleModifier = Random.Range(_scaleRange.x, _scaleRange.y);
        var weightModifier = Random.Range(1, 1.5f);
        _root.localPosition = new Vector3(_root.localPosition.x, _root.localPosition.y * scaleModifier, _root.localPosition.z);
        _root.localScale = new Vector3(_root.localScale.x * scaleModifier * weightModifier, 
            _root.localScale.y * scaleModifier, 
            _root.localScale.z * scaleModifier * weightModifier);
    }
}
