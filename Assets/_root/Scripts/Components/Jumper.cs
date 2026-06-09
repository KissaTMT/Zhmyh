using UnityEngine;

namespace Components
{
    public class Jumper : UnitComponent, IContributable<Vector3>, ITickable
    {
        public Vector3 Contribute => _contribute;
        public float Current => _current;

        private AnimationCurve _animationCurve;
        private float _height;
        private float _duration;

        private float _elapsedTime;
        private float _current;
        private float _previous;

        private float _directionModifier;

        private Vector3 _contribute;
        public Jumper(AnimationCurve curve, float height, float duration)
        {
            _animationCurve = curve;
            _height = height;
            _duration = duration;
        }
        public void PerfomJump(float directionModifier = 1)
        {
            _directionModifier = Mathf.Sign(directionModifier);
            _current = _directionModifier == 1 ? 0 : 1;
            _elapsedTime = _current;
            _previous = _elapsedTime;
        }
        public void Jump(float dt)
        {
            _current = Mathf.Clamp01(_elapsedTime / _duration);

            var deltaY = CalculateCurveDelta(_previous, _current) * _height;

            _elapsedTime += dt * _directionModifier;

            _previous = _current;

            _contribute = new Vector3(0, deltaY, 0);
        }

        private float CalculateCurveDelta(float t0, float t1)
        {
            if (_directionModifier == -1)
            {
                var t = t0;
                t0 = t1;
                t1 = t;
            }

            return _animationCurve.Evaluate(t1) - _animationCurve.Evaluate(t0);
        }

        public void Tick(float dt)
        {
            Jump(dt);
        }
    }
}