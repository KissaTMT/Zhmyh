using Spirit;
using System.Linq;
using UnityEngine;

[ExecuteAlways]
public class BillboardingMono : MonoBehaviour
{
    [SerializeField] private bool _isTesting;
    [SerializeField] private Transform _refTransform;
    [SerializeField] private Transform _root;


    private void Awake()
    {
        Cashe();
    }
    private void Cashe()
    {
        if(_refTransform == null) _refTransform = Camera.main.GetComponent<Transform>();
        if (_root == null) _root = GetComponent<Transform>();
    }
    private void Update()
    {
        Cashe();
        _root.localEulerAngles = new Vector3(_root.localEulerAngles.x, (_isTesting ? 180-_refTransform.eulerAngles.y : _refTransform.eulerAngles.y), _root.localEulerAngles.z);
    }
}