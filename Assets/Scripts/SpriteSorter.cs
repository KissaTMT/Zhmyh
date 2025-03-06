using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpriteSorter : MonoBehaviour
{
    public static SpriteSorter instance;
    public List<ISpriteSorterable> Sorters = new List<ISpriteSorterable>();
    private void Awake()
    {
        instance = this;
    }
    private void Update()
    {
        var sortedCollection = Sorters.OrderByDescending(i => i.Transform.parent.position.y).ToList();

        for (var i = 0; i < sortedCollection.Count; i++)
        {
            sortedCollection[i].Sort(i * 32 + 32);
        }
    }
}
