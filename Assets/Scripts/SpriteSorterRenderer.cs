using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(SpriteSorterInstance))]
public class SpriteSorterRenderer : MonoBehaviour, ISpriteSorterable
{
    public Transform Transform => _transform;
    public int Order => _order;

    private List<SpriteRenderer> _renderers;
    private Transform _transform;
    private int _order;

    private void Awake()
    {
        _transform = GetComponent<Transform>();
        _renderers = GetComponentsInChildren<SpriteRenderer>().ToList();
    }
    public void Sort(int order)
    {
        _renderers = _renderers.OrderByDescending(i => i.sortingOrder).ToList();
        for (var i = 0; i < _renderers.Count; i++)
        {
            _renderers[i].sortingOrder = order - i;
        }
        _order = order;
    }
}
