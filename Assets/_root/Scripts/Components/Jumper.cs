using UnityEngine;

namespace Components
{
    public class Jumper : IContributable<Vector3>, IComponent
    {
        private AnimationCurve _animationCurve;
        private float _height;
        private float _duration;

        private float _elapsedTime;
        private float _previous;

        private float _directionModifier;
        public Jumper(AnimationCurve curve, float height, float duration)
        {
            _animationCurve = curve;
            _height = height;
            _duration = duration;
        }
        public void PerfomJump(float directionModifier = 1)
        {
            _directionModifier = Mathf.Sign(directionModifier);
            _elapsedTime = _directionModifier == 1 ? 0 : 1;
            _previous = _elapsedTime;
        }
        public Vector3 Jump()
        {
            var current = Mathf.Clamp01(_elapsedTime / _duration);

            var deltaY = CalculateCurveDelta(_previous, current) * _height;

            _elapsedTime += Time.deltaTime * _directionModifier;

            _previous = current;

            return new Vector3(0, deltaY, 0);
        }

        public Vector3 Contribute() => Jump();

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
    }
}