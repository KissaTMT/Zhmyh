using UnityEngine;

namespace Components
{
    public class Dasher : IContributable<Vector3>, IComponent
    {
        public float Current => _current;
        private float _distance;
        private float _duration;

        private Vector3 _startPosition;
        private Vector3 _targetPosition;

        private float _elapsedTime;
        private float _previous;
        private float _current;

        private float _directionModifier;

        public Dasher(float distance, float duration)
        {
            _distance = distance;
            _duration = duration;
        }
        public Vector3 Contribute() => Dash();
        public void PerfomDash(Vector3 start, Vector3 direction, float directionModifier = 1)
        {
            _startPosition = start;
            _targetPosition = _startPosition + direction * _distance;

            _directionModifier = Mathf.Sign(directionModifier);
            _current = _directionModifier == 1 ? 0 : 1;
            _elapsedTime = _current;
            _previous = _elapsedTime;
        }
        public Vector3 Dash()
        {
            _current = Mathf.Clamp01(_elapsedTime / _duration);

            var delta = CalculateDelta(_previous, _current);

            _elapsedTime += Time.deltaTime * _directionModifier;

            _previous = _current;

            return delta;
        }
        private Vector3 CalculateDelta(float t0, float t1)
        {
            if(_directionModifier == -1)
            {
                var t = t0;
                t0 = t1;
                t1 = t;
            }
            return Vector3.Lerp(_startPosition, _targetPosition, t1) - Vector3.Lerp(_startPosition, _targetPosition, t0);
        }
    }
}