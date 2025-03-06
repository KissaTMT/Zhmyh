using UnityEngine;

public class SpriteSorterInstance : MonoBehaviour
{
    private ISpriteSorterable _sorter;

    private void Awake() => _sorter = GetComponent<ISpriteSorterable>();
    private void Start()
    {
        SpriteSorter.instance.Sorters.Add(_sorter);
    }
    private void OnDisable()
    {
        SpriteSorter.instance.Sorters.Remove(_sorter);
    }
}