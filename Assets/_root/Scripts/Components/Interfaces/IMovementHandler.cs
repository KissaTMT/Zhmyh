using UnityEngine;

namespace Components
{
    public interface IMovementHandler : IComponent
    {
        public void Handle(Vector3 vector);
    }
}
