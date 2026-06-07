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
        public void Zero()
        {
            _velocity = Vector3.zero;
        }
        public void SetModifier(float modifier)
        {
            SetModifier(Vector3.one * modifier);
        }
        public void SetModifier(Vector3 modifier)
        {
            _velocity = Adamar(Adamar(_direction, _force), modifier);
        }
        public void SetDirection(Vector3 direction)
        {
            _direction = direction;
        }
        public Vector3 Contribute() => Apply();
        public Vector3 Apply()
        {
            _velocity += Adamar(_direction, _force) * Time.deltaTime * _enableModifier;
            return _velocity * Time.deltaTime;
        }
        private Vector3 Adamar(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
        }
    }
}
