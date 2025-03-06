using UnityEngine;

public interface ISpriteSorterable
{
    Transform Transform { get; }
    int Order { get; }
    void Sort(int order);
}
