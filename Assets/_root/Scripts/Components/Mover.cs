using UnityEngine;

namespace Components
{
    public class Mover : UnitComponent, IContributable<Vector3>, ITickable
    {
        public Vector3 Contribute => _contribute;

        private float _speed;
        private Vector3 _direction;

        private Vector3 _contribute;

        public Mover(float speed)
        {
            _speed = speed;
        }
        public void SetDirection(Vector3 direction)
        {
            _direction = direction;
        }
        public void Move(float dt)
        {
            _contribute = _direction * _speed * dt;
        }

        public void Tick(float dt)
        {
            Move(dt);
        }
    }
}