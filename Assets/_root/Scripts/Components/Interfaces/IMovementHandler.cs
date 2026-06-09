using UnityEngine;

namespace Components
{
    public interface IMovementHandler : ITickable
    {
        public void Add(IContributable<Vector3> contributable);
        public bool Remove(IContributable<Vector3> contributable);
    }
}
