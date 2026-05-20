using UnityEngine;

namespace Components
{
    public class JumpPredictor
    {
        private AnimationCurve _curve;
        private float _height;
        public JumpPredictor(AnimationCurve curve, float height)
        {
            _curve = curve;
            _height = height;
        }
        public void Predict(Vector3 start)
        {
            for(var i = 0f; i < 1; i += 0.1f)
            {
                var position = start + Vector3.up * _curve.Evaluate(i);
            }
        }
    }
}