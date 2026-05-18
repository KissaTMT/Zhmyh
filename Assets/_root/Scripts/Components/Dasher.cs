using UnityEngine;

namespace Components
{
    public class Dasher : IContributable<Vector3>, IComponent
    {
        private float _distance;
        private float _duration;

        private Vector3 _startPosition;
        private Vector3 _targetPosition;

        private float _elapsedTime;
        private float _previous;

        public Dasher(float distance, float duration)
        {
            _distance = distance;
            _duration = duration;
        }
        public void PerfomDash(Vector3 start, Vector3 direction)
        {
            _startPosition = start;
            _targetPosition = _startPosition + direction * _distance;
            _elapsedTime = 0;
            _previous = 0;
        }
        public Vector3 Dash()
        {
            var current = Mathf.Clamp01(_elapsedTime / _duration);

            var delta = CalculateDelta(_previous, current);

            _elapsedTime += Time.deltaTime;

            _previous = current;

            return delta;
        }
        private Vector3 CalculateDelta(float t0, float t1)
        {
            return Vector3.Lerp(_startPosition, _targetPosition, t1) - Vector3.Lerp(_startPosition, _targetPosition, t0);
        }
        public Vector3 Contribute() => Dash();
    }
}