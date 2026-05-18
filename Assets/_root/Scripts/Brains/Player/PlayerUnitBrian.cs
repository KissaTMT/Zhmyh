using Components;
using System.Collections.Generic;
using System.Linq;
using Units;
using UnityEngine;
using Zenject;

namespace Brains
{
    public class PlayerUnitBrian : MonoBehaviour
    {
        public Transform Transform => _unit.Transform;
        public UnitCore Unit => _unit;

        private UnitCore _unit;
        private Camera _cameraMain;
        private Cursor _cursor;
        private Transform _cameraTransform;

        private IInput _input;
        private bool _isPull;

        private List<IContributable<Vector3>> _contributables = new();

        [Inject]
        public void Construct(IInput input, Cursor cursor)
        {
            _input = input;
            _cursor = cursor;

            _input.Dash += Dash;
            _input.Jump += Jump;
            _input.Pulling += SetPull;
        }
        public void Init()
        {
            var animationCurve = new AnimationCurve();
            animationCurve.AddKey(0, 0);
            animationCurve.AddKey(0.55f, 1);
            animationCurve.AddKey(1, 0);

            _unit = gameObject.AddComponent<UnitCore>().Init(new Timeflow(), "Zhmyh");
            


            _unit.Add(new CCMovementHandler(_unit.Transform.GetComponent<CharacterController>()));

            _unit.Add(new Mover(5));
            _unit.Add(new Dasher(5, 0.5f));
            _unit.Add(new Jumper(animationCurve, 2, 0.575f));
            _unit.Add(new Gravity());

            _contributables.AddRange(_unit.Components.Values.Where(v => v is IContributable<Vector3>).Cast<IContributable<Vector3>>());

            _cursor.Init(_input);
            _cameraMain = Camera.main;
            _cameraTransform = _cameraMain.GetComponent<Transform>();
        }
        private void OnDisable()
        {
            _input.Dash -= Dash;
            _input.Jump -= Jump;
            _input.Pulling -= SetPull;
        }
        private void SetPull(bool isPull)
        {
            //_unit.Pull(isPull);
            //_isPull = isPull;
        }
        private void Dash()
        {
            _unit.Get<Dasher>().PerfomDash(_unit.Transform.position, CalculateMovementDirection());
        }
        private void Jump()
        {
            _unit.Get<Jumper>().PerfomJump();
        }
        private Vector3 CalculateMovementDirection()
        {
            var input = _input.GetMove();

            var angle = _cameraTransform.eulerAngles.y * Mathf.Deg2Rad;

            var cos = Mathf.Cos(angle);
            var sin = Mathf.Sin(angle);

            var x = input.x * cos + input.y * sin;
            var z = -input.x * sin + input.y * cos;

            return new Vector3(x, 0f, z);
        }
        private Vector2 CalculateLookDirection()
        {
            return _cursor.ScreenPosition - (Vector2)_cameraMain.WorldToScreenPoint(Transform.position);
        }
        private Vector3 CalculateShootDirection()
        {
            var ray = _cameraMain.ScreenPointToRay(_cursor.ScreenPosition);
            return Physics.Raycast(ray, out var hitInfo, 32) ? hitInfo.point : ray.origin + ray.direction * 32;
        }
        private void Update()
        {
            _unit.Get<Mover>().SetDirection(CalculateMovementDirection());

            var result = Vector3.zero;
            foreach (var c in _contributables)
            {
                result += c.Contribute();
            }

            _unit.Get<CCMovementHandler>().Handle(result);

            _cursor.Tick();
        }

    }
}