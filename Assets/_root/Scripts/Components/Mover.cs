using UnityEngine;

namespace Components
{
    public class Mover : IContributable<Vector3>, IComponent
    {
        private float _speed;
        private Vector3 _direction;
        public Mover(float speed)
        {
            _speed = speed;
        }
        public void SetDirection(Vector3 direction)
        {
            _direction = direction;
        }
        public Vector3 Contribute() => Move();

        public Vector3 Move()
        {
            return _direction * _speed * Time.deltaTime;
        }
    }
}