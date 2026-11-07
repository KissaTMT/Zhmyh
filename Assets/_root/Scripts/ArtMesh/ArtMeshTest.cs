using ArtMesh;
using System;
using UnityEngine;

[ExecuteAlways]
public class ArtMeshTest : MonoBehaviour
{
    [SerializeField] private bool _isTesting;
    [SerializeField, Range(0,360)] private float _yaw;

    private ArtMeshRenderer[] _renderers; 
    private void Cashe()
    {
        _renderers = _renderers ?? GetComponentsInChildren<ArtMeshRenderer>();
    }
    private void OnValidate()
    {
        if (_isTesting == false) return;

        Cashe();

        for (var i = 0; i < _renderers.Length; i++)
        {

        }
    }
}
