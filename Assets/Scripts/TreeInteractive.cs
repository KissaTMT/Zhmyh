using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TreeInteractive : MonoBehaviour
{
    public PolygonCollider2D Trunk => _trunk;
    public Transform Transform => _transform;

    [SerializeField] private PolygonCollider2D _trunk;
    [SerializeField] private Collider2D _crown;
    [SerializeField] private Collider2D[] _branches;

    private Transform _transform;

    private bool _isInteractcive = false;

    public void ChangeInteractive()
    {
        _isInteractcive = !_isInteractcive;
        for (var i = 0; i < _branches.Length; i++)
        {
            _branches[i].isTrigger = !_isInteractcive;
            _branches[i].usedByEffector = _isInteractcive;
        }
    }
    private void Awake()
    {
        _transform = GetComponent<Transform>();
    }
}
