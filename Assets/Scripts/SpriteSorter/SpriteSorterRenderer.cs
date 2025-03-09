using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(SpriteSorterInstance))]
public class SpriteSorterRenderer : MonoBehaviour, ISpriteSorterable
{
    public Transform Transform => transform;
    public int Order => _order;

    protected List<SpriteRenderer> renderers;
    protected new Transform transform;

    private int _order;


    private void Awake()
    {
        transform = GetComponent<Transform>();
        renderers = GetComponentsInChildren<SpriteRenderer>().ToList();
        ReSort();
    }
    public virtual void Sort(int order)
    {
        for (var i = 0; i < renderers.Count; i++)
        {
            renderers[i].sortingOrder = order - i;
        }
        _order = order;
    }
    public void ReSort() => renderers = renderers.OrderByDescending(i => i.sortingOrder).ToList();
    
}
