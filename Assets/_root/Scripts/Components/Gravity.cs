using UnityEngine;

namespace Components
{
    public class Gravity : IContributable<Vector3>, IComponent
    {
        public Vector3 Direction => _direction;
        public Vector3 Force => _force;

        private Vector3 _force;
        private Vector3 _direction;

        private Vector3 _velocity;

        private float _enableModifier = 1;
        public Gravity() : this(new Vector3(0, -1, 0), new Vector3(0, Mathf.Abs(Physics.gravity.y), 0)) { }
        public Gravity(Vector3 direction, Vector3 force)
        {
            _direction = direction;
            _force = force;
        }
        public void Enable()
        {
            _enableModifier = 1;
        }
        public void Disable()
        {
            _enableModifier = 0;
        }
        public void Reset()
        {
            _velocity = new Vector3(_direction.x * _force.x, _direction.y * _force.y, _direction.z * _force.z) * 0.5f;
        }
        public void Reset(Vector3 force)
        {
            _velocity = force;
        }
        public void SetDirection(Vector3 direction)
        {
            _direction = direction;
        }
        public Vector3 Contribute() => Apply();
        public Vector3 Apply()
        {
            _velocity += new Vector3(_direction.x * _force.x, _direction.y * _force.y, _direction.z * _force.z) * Time.deltaTime * _enableModifier;
            return _velocity * Time.deltaTime;
        }
    }
}
