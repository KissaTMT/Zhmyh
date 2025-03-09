using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Zenject;

public class SpriteSorter : ITickable
{
    private const int SORT_BUFFER = 32;
    public List<ISpriteSorterable> Sorters;

    private LazyInject<Player> _player;

    [Inject]
    public void Construct(LazyInject<Player> player)
    {
        _player = player;
        Sorters = new List<ISpriteSorterable>();
    }

    public void Tick()
    {
        var sortedCollection = Sorters.OrderByDescending(i => i.Transform.parent.position.y).ToList();

        for (var i = 0; i < sortedCollection.Count; i++)
        {
            sortedCollection[i].Sort(i * SORT_BUFFER + SORT_BUFFER);
        }
    }
}
