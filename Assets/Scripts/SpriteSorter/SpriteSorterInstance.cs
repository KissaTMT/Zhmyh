using System;
using UnityEngine;
using Zenject;

public class SpriteSorterInstance : MonoBehaviour
{
    private ISpriteSorterable _sorter;
    private SpriteSorter _spriteSorter;

    [Inject]
    public void Construct(SpriteSorter spriteSorter)
    {
        _spriteSorter = spriteSorter;
        _sorter = GetComponent<ISpriteSorterable>();
    }
    private void OnEnable()
    {
        AddToSorting();
    }
    private void OnDisable()
    {
        RemoveToSorting();
    }
    private void AddToSorting()
    {
        _spriteSorter.Sorters.Add(_sorter);
    }
    private void RemoveToSorting()
    {
        _spriteSorter.Sorters.Remove(_sorter);
    }
}