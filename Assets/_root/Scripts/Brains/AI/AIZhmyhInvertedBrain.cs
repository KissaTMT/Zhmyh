using Components;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro.EditorUtilities;
using Units;
using UnityEngine;

namespace Brains
{
    public class AIZhmyhInvertedBrain : MonoBehaviour, IBrian
    {
        public Transform Transform => _unit.Transform;
        private UnitCore _unit;
        private List<IContributable<Vector3>> _contributables = new();

        public void Init()
        {
            var animationCurve = new AnimationCurve();
            animationCurve.AddKey(0, 0);
            animationCurve.AddKey(0.55f, 1);
            animationCurve.AddKey(1, 0);

            _unit = gameObject.AddComponent<UnitCore>().Init(new Timeflow(-1), "Zhmyh");

            _unit.Add(new CCMovementHandler(_unit.Transform.GetComponent<CharacterController>()));

            _unit.Add(new Mover(4));
            _unit.Add(new Dasher(4, 0.5f));
            _unit.Add(new Jumper(animationCurve, 2, 0.575f));
            _unit.Add(new Gravity());

            _contributables.AddRange(_unit.Components.Values.Where(v => v is IContributable<Vector3>).Cast<IContributable<Vector3>>());
            
            StartCoroutine(CastRoutine());
        }

        private IEnumerator CastRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(2);
                _unit.Get<Dasher>().PerfomDash(_unit.Transform.position, _unit.Transform.forward,_unit.Timeflow.Current);

                yield return new WaitForSeconds(2);
                _unit.Get<Gravity>().Disable();

                _unit.Get<Jumper>().PerfomJump(_unit.Timeflow.Current);

                yield return new WaitForSeconds(1);

                _unit.Get<Gravity>().Enable();

                yield return new WaitForSeconds(2);

                _unit.Get<Gravity>().Disable();

                _unit.Get<Jumper>().PerfomJump(-_unit.Timeflow.Current);

                yield return new WaitForSeconds(1);

                _unit.Get<Gravity>().Enable();
            }
        }

        private void Update()
        {
            _unit.Get<Mover>().SetDirection(Vector3.forward);

            var result = Vector3.zero;
            foreach (var c in _contributables)
            {
                result += c.Contribute();
            }

            _unit.Get<CCMovementHandler>().Handle(result);
        }

    }
}