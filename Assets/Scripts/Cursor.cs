using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Cursor : MonoBehaviour
{
    [SerializeField] private float _rotationSpeed;
    private InputHandler _input;
    private RectTransform _rectTransform;
    private Vector2 _delta;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _input = new InputHandler();
        _input.OnAim += Aim;
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
        _rectTransform.localPosition += (Vector3)_delta;
        _rectTransform.Rotate(0,0, -_rotationSpeed * Time.deltaTime);
    }
}
