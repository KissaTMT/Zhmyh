using UnityEngine;

public class BillboardingMono : MonoBehaviour
{
    private Transform _cameraTransform;
    private Transform _root;
    private void Awake()
    {
        _cameraTransform = Camera.main.GetComponent<Transform>();
        _root = GetComponent<Transform>();
    }
    private void Update()
    {
        _root.localEulerAngles = new Vector3(_root.localEulerAngles.x, _cameraTransform.eulerAngles.y, _root.localEulerAngles.z);
    }
}