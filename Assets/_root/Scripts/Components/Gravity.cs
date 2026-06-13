using UnityEngine;

namespace Components
{
    public class Gravity : UnitComponent, IContributable<Vector3>, ITickable
    {
        public Vector3 Direction => _direction;
        public Vector3 Acceleration => _acceleration;
        public Vector3 Contribute => _contribute;
        public Vector3 Velocity => _velocity;

        private Vector3 _acceleration;
        private Vector3 _direction;

        private Vector3 _velocity;

        private Vector3 _contribute;
        public Gravity() : this(new Vector3(0, -1, 0), new Vector3(0, Mathf.Abs(Physics.gravity.y), 0)) { }
        public Gravity(Vector3 direction, Vector3 acceleration)
        {
            _direction = direction;
            _acceleration = acceleration;
        }
        public void Zero()
        {
            _velocity = Vector3.zero;
            _contribute = Vector3.zero;
        }
        public void SetVelocity(float modifier)
        {
            SetVelocity(Vector3.one * modifier);
        }
        public void SetVelocity(Vector3 modifier)
        {
            _velocity = Vector3.Max(Vector3.zero, modifier);
        }
        public void SetDirection(Vector3 direction)
        {
            _direction = direction;
        }
        public void SetAcceleration(Vector3 v)
        {
            _acceleration = new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
        }
        public void Apply(float deltaTime)
        {
            _velocity = Vector3.Max(Vector3.zero, _velocity + relativeTimeflow * _acceleration * deltaTime);
            _contribute = Adamar(_direction, _velocity) * deltaTime;
        }
        private Vector3 Adamar(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
        }

        public void Tick(float dt)
        {
            Apply(dt);
        }
        protected override void OnDisable() => Zero();
    }
}
