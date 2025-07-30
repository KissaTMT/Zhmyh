using Unity.Cinemachine;
using UnityEngine;
using Zenject;

public class Unit : MonoBehaviour
{
    private Transform _camera;
    private Transform _root;
    protected virtual void Awake()
    {
        _camera = Camera.main.transform;
        _root = transform.GetChild(0);
    }
    protected virtual void Update()
    {
        _root.localEulerAngles = _camera.eulerAngles;
    }
}