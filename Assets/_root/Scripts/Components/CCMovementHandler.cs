using UnityEngine;

namespace Components
{
    public class CCMovementHandler : IMovementHandler
    {
        private readonly CharacterController _characterController;

        public CCMovementHandler(CharacterController characterController)
        {
            _characterController = characterController;
        }
        public void Handle(Vector3 vector)
        {
            _characterController.Move(vector);
        }
    }
}
